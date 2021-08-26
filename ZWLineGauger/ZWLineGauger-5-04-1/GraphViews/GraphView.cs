using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ZWLineGauger.Gaugers;
using System.IO;
using System.Threading;

namespace ZWLineGauger
{
    public class GraphView : PictureBox
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();

        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_find_gerber_array(uint[] in_values, uint[] out_values);

        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_gerber_array_info(uint[] in_values, uint[] out_values);

        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool get_theta(double start_x, double start_y, double end_x, double end_y, double[] out_doubles);

        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool rotate_crd(double[] in_crds, double[] out_crds, double rotate_angle);

        MainUI m_parent;
        
        public int   m_nOrientation = 0;
        public bool m_bMouseEntered = false;
        public bool m_bMousePressed = false;
        public bool m_bMouseDragged = false;
        public bool m_bDrawArrayRects = false;                          // 是否需要绘制阵列

        public bool   m_bDrawLineFrameByHand = false;
        double          m_wh_ratio_for_drawn_line = 1;
        public MEASURE_TYPE   m_nTypeOfDrawnLine = MEASURE_TYPE.NONE;
        public Point2d   m_start_pt_for_drawn_line = new Point2d(0, 0);
        public Point2d   m_end_pt_for_drawn_line = new Point2d(0, 0);
        public Point2d[]   m_drawn_line_pts = new Point2d[4];

        public bool m_bRequireRedraw32Bitmap = false;
        public bool m_bHasValidImage = false;
        public Bitmap m_bitmap_1bit;
        public Bitmap m_bitmap_32bits;

        public const int   CAM_COLOR_VALUE = 210;

        byte[]   m_pOriginImageData;
        int        m_nBytesPerLineForOriginImage = 0;

        double m_prev_zoom_ratio = 1;
        public double m_zoom_ratio_min = 1;
        public double m_zoom_ratio_max = 3;
        public double m_zoom_ratio = 1;
        public Point2d m_current_view_crd_on_graph;                    // 当前视野左上角在图形上的坐标
        public Point2d m_alignment_crd_on_graph;                         // 对齐点在图纸上的坐标
        public Point m_mouse_pt;

        public bool    m_bFinishFindingArray = true;                                               // 阵列识别是否已经完成
        public List<Point>   m_list_array_unit_anchor_pts = new List<Point>();
        static public List<List<rotated_array_rect>>   m_ODB_thumbnail_array_rects = new List<List<rotated_array_rect>>();
        
        int m_nLastRefreshTime = 0;

        public GraphView(MainUI parent, int orient)
        {
            this.m_parent = parent;
            m_nOrientation = orient;

            this.MouseEnter += new System.EventHandler(OnMouseEnter);
            this.MouseLeave += new System.EventHandler(OnMouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(OnMouseMove);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(OnMouseWheel);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(OnMouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(OnMouseUp);
            this.Click += new System.EventHandler(OnClick);
            this.DoubleClick += new System.EventHandler(OnDoubleClick);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMouseClick);
        }

        public void init_params(double current_ratio, double prev_ratio, double min_ratio)
        {
            m_zoom_ratio = current_ratio;
            m_prev_zoom_ratio = prev_ratio;
            m_zoom_ratio_min = min_ratio;

            m_current_view_crd_on_graph.set(0, 0);
        }

        public void set_view_ratio_and_crd(double target_ratio, double crd_x, double crd_y)
        {
            m_zoom_ratio = target_ratio;

            if (m_zoom_ratio < m_zoom_ratio_min)
                m_zoom_ratio = m_zoom_ratio_min;
            if (m_zoom_ratio > m_zoom_ratio_max)
                m_zoom_ratio = m_zoom_ratio_max;

            m_prev_zoom_ratio = m_zoom_ratio;

            double x = crd_x;
            double y = crd_y;
            if (x < 0)
                x = 0;
            else if (x >= m_bitmap_1bit.Width)
                x = m_bitmap_1bit.Width - 1;
            if (y < 0)
                y = 0;
            else if (y >= m_bitmap_1bit.Height)
                y = m_bitmap_1bit.Height - 1;

            m_current_view_crd_on_graph.set(x, y);
        }

        // 获取鼠标在图纸上的位置坐标
        public void get_mouse_crd_on_graph(ref int x, ref int y)
        {
            Point2d left_top, current_mouse_pt_on_image, current_mouse_pt_on_graph;
            left_top.x = (double)(Width - Image.Width) / 2.0;
            left_top.y = (double)(Height - Image.Height) / 2.0;
            current_mouse_pt_on_image.x = (double)(m_mouse_pt.X) - left_top.x;
            current_mouse_pt_on_image.y = (double)(m_mouse_pt.Y) - left_top.y;
            current_mouse_pt_on_graph.x = m_current_view_crd_on_graph.x + (current_mouse_pt_on_image.x / m_zoom_ratio);
            current_mouse_pt_on_graph.y = m_current_view_crd_on_graph.y + (current_mouse_pt_on_image.y / m_zoom_ratio);

            x = (int)Math.Round(current_mouse_pt_on_graph.x, 0);
            y = (int)Math.Round(current_mouse_pt_on_graph.y, 0);
        }

        public void render_black_image()
        {
            Bitmap bmp = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(bmp);
            SolidBrush b = new SolidBrush(Color.Black);           // 这里修改颜色
            g.FillRectangle(b, 0, 0, this.Width, this.Height);
            this.Image = bmp;
        }

        public void refresh_image(bool bRefreshOverlay = false)
        {
            if (null == m_bitmap_1bit)
                return;

            double ratio = 1;
            int nBmpWidth = this.Width;
            int nBmpHeight = this.Height;

            if (m_bitmap_1bit.Width >= m_bitmap_1bit.Height)
            {
                ratio = (double)(this.Width) / (double)(m_bitmap_1bit.Width);
                double temp = (double)(m_bitmap_1bit.Height) * ratio;
                nBmpHeight = (int)temp;
                if (nBmpHeight > this.Height)
                {
                    nBmpHeight = this.Height;
                    ratio = (double)(this.Height) / (double)(m_bitmap_1bit.Height);
                    temp = (double)(m_bitmap_1bit.Width) * ratio;
                    nBmpWidth = (int)temp;
                }
            }
            else
            {
                ratio = (double)(this.Height) / (double)(m_bitmap_1bit.Height);
                double temp = (double)(m_bitmap_1bit.Width) * ratio;
                nBmpWidth = (int)temp;
                if (nBmpWidth > this.Width)
                {
                    nBmpWidth = this.Width;
                    ratio = (double)(this.Width) / (double)(m_bitmap_1bit.Width);
                    temp = (double)(m_bitmap_1bit.Height) * ratio;
                    nBmpHeight = (int)temp;
                }
            }

            //string msg2 = string.Format("222222 nBmpWidth [{0},{1}], this.Width [{2},{3}], m_bitmap_1bit [{4},{5}]", 
            //    nBmpWidth, nBmpHeight, this.Width, this.Height, m_bitmap_1bit.Width, m_bitmap_1bit.Height);
            //Debugger.Log(0, null, msg2);
            
            // Image对象 m_show_image 初次赋值
            #region
            if (true == m_bRequireRedraw32Bitmap)
            {
                //string msg2 = string.Format("222222 m_origin_image [{0},{1}], 控件宽高 [{2},{3}], 转换图像宽高 [{4},{5}], ratio = {6}", 
                //   m_origin_image.Width, m_origin_image.Height, this.Width, this.Height, nBmpWidth, nBmpHeight, ratio);
                //Debugger.Log(0, null, msg2);
                
                m_list_array_unit_anchor_pts.Clear();

                m_bHasValidImage = false;

                m_bitmap_32bits = new Bitmap(nBmpWidth, nBmpHeight, PixelFormat.Format32bppRgb);

                //Debugger.Log(0, null, string.Format("222222 nBmpWidth [{0},{1}]", nBmpWidth, nBmpHeight));

                init_params(ratio, ratio, ratio);

                // 渲染
                if (true)
                {
                    Rectangle rect = new Rectangle(0, 0, m_bitmap_1bit.Width, m_bitmap_1bit.Height);
                    BitmapData bmpData = m_bitmap_1bit.LockBits(rect, ImageLockMode.ReadOnly, m_bitmap_1bit.PixelFormat);
                    IntPtr ptr = bmpData.Scan0;
                    m_nBytesPerLineForOriginImage = Math.Abs(bmpData.Stride);
                    int nBytesLen = m_nBytesPerLineForOriginImage * m_bitmap_1bit.Height;
                    m_pOriginImageData = new byte[nBytesLen];
                    Marshal.Copy(ptr, m_pOriginImageData, 0, nBytesLen);
                    m_bitmap_1bit.UnlockBits(bmpData);

                    int nBytesPerLine = m_bitmap_32bits.Width * 4;
                    byte[] thumbnail_buf = new byte[nBytesPerLine * m_bitmap_32bits.Height];
                    int h = m_bitmap_32bits.Height;
                    int w = m_bitmap_32bits.Width;
                    for (int a = 0; a < h; a++)
                    {
                        int offset = a * nBytesPerLine;

                        double temp = Math.Round(((double)a / ratio), 0) * (double)m_nBytesPerLineForOriginImage;
                        int offset2 = (int)temp;

                        for (int b = 0; b < w; b++)
                        {
                            temp = (double)b / ratio;
                            int b2 = (int)temp;

                            int flag = 0;
                            switch (b2 % 8)
                            {
                                case 0:
                                    flag = ((m_pOriginImageData[offset2 + b2 / 8] & 0x80) > 0) ? 1 : 0;
                                    break;
                                case 1:
                                    flag = ((m_pOriginImageData[offset2 + b2 / 8] & 0x40) > 0) ? 1 : 0;
                                    break;
                                case 2:
                                    flag = ((m_pOriginImageData[offset2 + b2 / 8] & 0x20) > 0) ? 1 : 0;
                                    break;
                                case 3:
                                    flag = ((m_pOriginImageData[offset2 + b2 / 8] & 0x10) > 0) ? 1 : 0;
                                    break;
                                case 4:
                                    flag = ((m_pOriginImageData[offset2 + b2 / 8] & 0x08) > 0) ? 1 : 0;
                                    break;
                                case 5:
                                    flag = ((m_pOriginImageData[offset2 + b2 / 8] & 0x04) > 0) ? 1 : 0;
                                    break;
                                case 6:
                                    flag = ((m_pOriginImageData[offset2 + b2 / 8] & 0x02) > 0) ? 1 : 0;
                                    break;
                                case 7:
                                    flag = ((m_pOriginImageData[offset2 + b2 / 8] & 0x01) > 0) ? 1 : 0;
                                    break;
                            }

                            if (1 == flag)
                                thumbnail_buf[offset + b * 4 + 2] = CAM_COLOR_VALUE;
                        }
                    }

                    rect = new Rectangle(new Point(0, 0), m_bitmap_32bits.Size);
                    BitmapData bmp_data = m_bitmap_32bits.LockBits(rect, ImageLockMode.WriteOnly, m_bitmap_32bits.PixelFormat);
                    System.Runtime.InteropServices.Marshal.Copy(thumbnail_buf, 0, bmp_data.Scan0, thumbnail_buf.Length);
                    m_bitmap_32bits.UnlockBits(bmp_data);
                    
                    if (true == m_bDrawArrayRects)
                        draw_array_rects_overlay(m_bitmap_32bits, ratio, 0, 0, m_ODB_thumbnail_array_rects);
                    
                    draw_overlay_for_measure_points(m_bitmap_32bits, ratio, 0, 0, m_parent.m_measure_items_on_graph);

                    this.Image = m_bitmap_32bits;
                }

                m_bHasValidImage = true;
                m_bRequireRedraw32Bitmap = false;

                return;
            }
            #endregion
            
            if (true)
            {
                ratio = m_zoom_ratio;
                
                double temp1 = (double)m_bitmap_32bits.Width / ratio;
                double temp2 = (double)m_bitmap_32bits.Height / ratio;
                int rect_width = (int)temp1;
                int rect_height = (int)temp2;

                int rect_left = (m_bitmap_1bit.Width - rect_width) / 2;
                int rect_top = (m_bitmap_1bit.Height - rect_height) / 2;

                Point2d mouse_crd_on_graph;
                mouse_crd_on_graph.x = m_current_view_crd_on_graph.x + (double)(m_mouse_pt.X - (this.Width - m_bitmap_32bits.Width) / 2) / m_prev_zoom_ratio;
                mouse_crd_on_graph.y = m_current_view_crd_on_graph.y + (double)(m_mouse_pt.Y - (this.Height - m_bitmap_32bits.Height) / 2) / m_prev_zoom_ratio;
                
                double new_start_x = mouse_crd_on_graph.x - ((double)(m_mouse_pt.X - (this.Width - m_bitmap_32bits.Width) / 2) / ratio);
                double new_start_y = mouse_crd_on_graph.y - ((double)(m_mouse_pt.Y - (this.Height - m_bitmap_32bits.Height) / 2) / ratio);
                if (false == bRefreshOverlay)
                {
                    m_current_view_crd_on_graph.x = new_start_x;
                    m_current_view_crd_on_graph.y = new_start_y;
                }
                else
                {
                    new_start_x = m_current_view_crd_on_graph.x;
                    new_start_y = m_current_view_crd_on_graph.y;
                }

                rect_left = (int)Math.Round(new_start_x, 0);
                rect_top = (int)Math.Round(new_start_y, 0);
                if (rect_top < 0)
                    rect_top = 0;
                if (rect_left < 0)
                    rect_left = 0;

                int temp_height = (int)Math.Round(((double)(m_bitmap_32bits.Height - 1) / ratio), 0);
                int temp_width = (int)((double)(m_bitmap_32bits.Width - 1) / ratio);
                if ((temp_height + rect_top) >= m_bitmap_1bit.Height)
                    rect_top = m_bitmap_1bit.Height - temp_height - 1;
                if ((temp_width + rect_left) >= m_bitmap_1bit.Width)
                    rect_left = m_bitmap_1bit.Width - temp_width - 1;

                m_current_view_crd_on_graph.x = rect_left;
                m_current_view_crd_on_graph.y = rect_top;
                
                //string msg = string.Format("222222 m_origin_image [{0},{1}], m_bitmap [{2},{3}], rect_width [{4},{5}], m_zoom_ratio = {6}",
                //    m_origin_image.Width, m_origin_image.Height, m_bitmap.Width, m_bitmap.Height, rect_width, rect_height, m_zoom_ratio);
                //Debugger.Log(0, null, msg);
                //msg = string.Format("222222 m_mouse_pt [{0},{1}], prev_ratio = {2}, mouse_crd_on_graph = [{3},{4}]", 
                //    m_mouse_pt.X, m_mouse_pt.Y, m_prev_zoom_ratio, mouse_crd_on_graph.x, mouse_crd_on_graph.y);
                //Debugger.Log(0, null, msg);
                //string msg = string.Format("222222 rect_left [{0},{1}], temp_width [{2},{3}]", rect_left, rect_top, temp_width, temp_height);
                //Debugger.Log(0, null, msg);

                Rectangle rect = new Rectangle(0, 0, m_bitmap_1bit.Width, m_bitmap_1bit.Height);
                BitmapData bmpData = m_bitmap_1bit.LockBits(rect, ImageLockMode.ReadOnly, m_bitmap_1bit.PixelFormat);
                int nBytesPerLineForOriginImage = Math.Abs(bmpData.Stride);
                m_bitmap_1bit.UnlockBits(bmpData);
                
                int nBytesPerLine = m_bitmap_32bits.Width * 4;
                byte[] thumbnail_buf = new byte[nBytesPerLine * m_bitmap_32bits.Height];
                int h = m_bitmap_32bits.Height;
                int w = m_bitmap_32bits.Width;
                for (int a = 0; a < h; a++)
                {
                    int offset = a * nBytesPerLine;

                    double temp = (Math.Round(((double)a / ratio), 0) + rect_top) * (double)nBytesPerLineForOriginImage;
                    int offset2 = (int)temp;

                    for (int b = 0; b < w; b++)
                    {
                        temp = ((double)b / ratio) + rect_left;
                        int b2 = (int)temp;

                        int flag = get_byte_buffer_value_flag(m_pOriginImageData, offset2, b2);

                        if (1 == flag)
                            thumbnail_buf[offset + b * 4 + 2] = CAM_COLOR_VALUE;
                    }
                }
                
                rect = new Rectangle(new Point(0, 0), m_bitmap_32bits.Size);
                BitmapData bmp_data = m_bitmap_32bits.LockBits(rect, ImageLockMode.WriteOnly, m_bitmap_32bits.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(thumbnail_buf, 0, bmp_data.Scan0, thumbnail_buf.Length);
                m_bitmap_32bits.UnlockBits(bmp_data);
                
                if (true == m_bDrawArrayRects)
                    draw_array_rects_overlay(m_bitmap_32bits, ratio, rect_left, rect_top, m_ODB_thumbnail_array_rects);
                
                draw_overlay_for_measure_points(m_bitmap_32bits, ratio, rect_left, rect_top, m_parent.m_measure_items_on_graph);

                draw_overlay_for_measure_points(m_bitmap_32bits, ratio, rect_left, rect_top, m_parent.m_measure_items_on_graph);

                this.Image = m_bitmap_32bits;
            }
        }

        public void draw_array_rects_overlay(Bitmap bmp, double zoom_ratio, int rect_left, int rect_top, List<List<rotated_array_rect>> list_list_rects)
        {
            // 绘制覆盖层
            if (true)
            {
                Graphics g = Graphics.FromImage(bmp);

                Pen gray_pen = new Pen(Color.FromArgb(130, 130, 130), (float)4);
                Pen green_pen = new Pen(Color.FromArgb(0, 255, 0), (float)2);
                Pen highlight_pen = new Pen(Color.FromArgb(150, 255, 0), (float)3);

                int off_x = MainUI.m_nGraphOffsetX;
                int off_y = MainUI.m_nGraphOffsetY;
                double pixels_per_mm = MainUI.m_pixels_per_mm;

                //Debugger.Log(0, null, string.Format("222222 off_x [{0},{1}], pixels_per_mm = {2}", off_x, off_y, pixels_per_mm));

                for (int p = 0; p < 2; p++)
                {
                    for (int n = 0; n < list_list_rects.Count; n++)
                    {
                        for (int m = 0; m < list_list_rects[n].Count; m++)
                        {
                            double left = list_list_rects[n][m].left * pixels_per_mm + off_x;
                            double top = list_list_rects[n][m].top * pixels_per_mm + off_y;
                            double width = list_list_rects[n][m].width * pixels_per_mm;
                            double height = list_list_rects[n][m].height * pixels_per_mm;
                            
                            PointF[] pts = new PointF[4];
                            pts[0].X = (float)((left - (double)rect_left) * zoom_ratio);
                            pts[0].Y = (float)((top - (double)rect_top) * zoom_ratio);
                            pts[1].X = pts[0].X + (float)(width * zoom_ratio);
                            pts[1].Y = pts[0].Y;
                            pts[2].X = pts[1].X;
                            pts[2].Y = pts[1].Y + (float)(height * zoom_ratio);
                            pts[3].X = pts[0].X;
                            pts[3].Y = pts[2].Y;
                            for (int k = 0; k < 4; k++)
                            {
                                if (list_list_rects[n][m].bSelected && (1 == p))
                                    g.DrawLine(highlight_pen, pts[k], pts[(k + 1) % 4]);
                                else if (0 == p)
                                    g.DrawLine(gray_pen, pts[k], pts[(k + 1) % 4]);
                            }
                        }
                    }
                }
                
                for (int n = 0; n < m_list_array_unit_anchor_pts.Count; n++)
                {
                    PointF center = new PointF(m_list_array_unit_anchor_pts[n].X, m_list_array_unit_anchor_pts[n].Y);
                    center.X = (float)((center.X - (double)rect_left) * zoom_ratio);
                    center.Y = (float)((center.Y - (double)rect_top) * zoom_ratio);
                    
                    float radius = 10;
                    if (zoom_ratio < 0.1)
                        radius /= 2;
                    
                    Rectangle rect = new Rectangle((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2));
                    g.DrawEllipse(green_pen, rect);

                    g.DrawLine(green_pen, new PointF(center.X - radius, center.Y), new PointF(center.X + radius, center.Y));
                    g.DrawLine(green_pen, new PointF(center.X, center.Y - radius), new PointF(center.X, center.Y + radius));
                }
            }
        }

        public void draw_overlay_for_measure_points(Bitmap bmp, double zoom_ratio, int rect_left, int rect_top, List<MeasurePointData> list_point_data)
        {
            // 绘制覆盖层
            if (true)
            {
                Graphics g = Graphics.FromImage(bmp);

                //Pen p = new Pen(Color.FromArgb(255, 150, 0), (float)2);
                //Pen thick_pen = new Pen(Color.FromArgb(255, 0, 0), (float)5);
                Pen p = new Pen(Color.FromArgb(150, 255, 0), (float)2);
                Pen error_pen = new Pen(Color.FromArgb(255, 255, 0), (float)2);
                Pen thick_pen = new Pen(Color.FromArgb(0, 255, 0), (float)5);

                // 画对齐点
                if (true == m_parent.m_bIsAlignmentPtSet)
                {
                    PointF[] pts = new PointF[6];

                    pts[0].X = (float)((m_alignment_crd_on_graph.x - (double)rect_left) * zoom_ratio);
                    pts[0].Y = (float)((m_alignment_crd_on_graph.y - (double)rect_top) * zoom_ratio);
                    pts[1].X = pts[0].X - 50;
                    pts[1].Y = pts[0].Y;
                    pts[2].X = pts[0].X + 50;
                    pts[2].Y = pts[0].Y;
                    pts[3].X = pts[0].X;
                    pts[3].Y = pts[0].Y - 50;
                    pts[4].X = pts[0].X;
                    pts[4].Y = pts[0].Y + 50;

                    g.DrawLine(thick_pen, pts[1], pts[2]);
                    g.DrawLine(thick_pen, pts[3], pts[4]);
                }

                for (int n = 0; n < list_point_data.Count; n++)
                {
                    PointF[] pts = new PointF[6];

                    //Debugger.Log(0, null, string.Format("222222 list_point_data[n].m_mes_type = {0}", list_point_data[n].m_mes_type));

                    if (list_point_data[n].m_bIsInvalidItem)
                    {
                        pts[0].X = (float)((list_point_data[n].m_center_x_on_graph - 50 - (double)rect_left) * zoom_ratio);
                        pts[0].Y = (float)((list_point_data[n].m_center_y_on_graph - 50 - (double)rect_top) * zoom_ratio);
                        pts[1].X = (float)((list_point_data[n].m_center_x_on_graph + 50 - (double)rect_left) * zoom_ratio);
                        pts[1].Y = (float)((list_point_data[n].m_center_y_on_graph - 50 - (double)rect_top) * zoom_ratio);
                        pts[2].X = (float)((list_point_data[n].m_center_x_on_graph + 50 - (double)rect_left) * zoom_ratio);
                        pts[2].Y = (float)((list_point_data[n].m_center_y_on_graph + 50 - (double)rect_top) * zoom_ratio);
                        pts[3].X = (float)((list_point_data[n].m_center_x_on_graph - 50 - (double)rect_left) * zoom_ratio);
                        pts[3].Y = (float)((list_point_data[n].m_center_y_on_graph + 50 - (double)rect_top) * zoom_ratio);

                        g.DrawLine(error_pen, pts[0], pts[2]);
                        g.DrawLine(error_pen, pts[1], pts[3]);
                        continue;
                    }

                    switch (list_point_data[n].m_mes_type)
                    {
                        case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                        case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                        case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                            for (int m = 0; m < 4; m++)
                            {
                                pts[m].X = (float)((list_point_data[n].m_fit_rect_on_graph[m].x - (double)rect_left) * zoom_ratio);
                                pts[m].Y = (float)((list_point_data[n].m_fit_rect_on_graph[m].y - (double)rect_top) * zoom_ratio);
                            }
                            for (int m = 0; m < 4; m++)
                            {
                                g.DrawLine(p, pts[m], pts[(m + 1) % 4]);
                            }
                            break;

                        case MEASURE_TYPE.LINE_WIDTH_14:
                        case MEASURE_TYPE.LINE_WIDTH_23:
                        case MEASURE_TYPE.LINE_WIDTH_13:
                        case MEASURE_TYPE.LINE_WIDTH_1234:
                        case MEASURE_TYPE.ARC_LINE_WIDTH:
                            for (int m = 0; m < 4; m++)
                            {
                                pts[m].X = (float)((list_point_data[n].m_fit_rect_on_graph[m].x - (double)rect_left) * zoom_ratio);
                                pts[m].Y = (float)((list_point_data[n].m_fit_rect_on_graph[m].y - (double)rect_top) * zoom_ratio);
                            }
                            for (int m = 0; m < 4; m++)
                            {
                                g.DrawLine(p, pts[m], pts[(m + 1) % 4]);
                            }
                            break;

                        case MEASURE_TYPE.LINE_SPACE:
                        case MEASURE_TYPE.ARC_LINE_SPACE:
                            for (int m = 0; m < 4; m++)
                            {
                                pts[m].X = (float)((list_point_data[n].m_fit_rect_on_graph[m].x - (double)rect_left) * zoom_ratio);
                                pts[m].Y = (float)((list_point_data[n].m_fit_rect_on_graph[m].y - (double)rect_top) * zoom_ratio);
                            }
                            for (int m = 0; m < 4; m++)
                            {
                                //g.DrawLine(p, pts[m], pts[(m + 1) % 4]);
                            }
                            g.DrawLine(p, pts[1], pts[2]);
                            g.DrawLine(p, pts[3], pts[0]);

                            pts[4].X = (pts[1].X + pts[2].X) / 2;
                            pts[4].Y = (pts[1].Y + pts[2].Y) / 2;
                            pts[5].X = (pts[3].X + pts[0].X) / 2;
                            pts[5].Y = (pts[3].Y + pts[0].Y) / 2;
                            g.DrawLine(p, pts[4], pts[5]);
                            break;
                    }
                }

                // 画图纸手拉框
                if ((true == m_bDrawLineFrameByHand) && ((Math.Abs(m_start_pt_for_drawn_line.x) + Math.Abs(m_start_pt_for_drawn_line.y)) > 0))
                {
                    Point2d pt1 = m_start_pt_for_drawn_line;
                    Point2d pt2 = m_end_pt_for_drawn_line;
                    
                    double angle = 0;
                    double side_len = Math.Sqrt((pt1.x - pt2.x) * (pt1.x - pt2.x) + (pt1.y - pt2.y) * (pt1.y - pt2.y));

                    Point2d center;
                    Point2d start, end;
                    Point2d[] pts = new Point2d[4];
                    Point2d[] rotated_pts = new Point2d[4];
                    Point2d[] new_pts = new Point2d[4];

                    center.x = (pt1.x + pt2.x) / 2.0;
                    center.y = (pt1.y + pt2.y) / 2.0;

                    start.x = pt1.x - center.x;
                    start.y = pt1.y - center.y;
                    end.x = pt2.x - center.x;
                    end.y = pt2.y - center.y;

                    pts[0].x = -side_len / 2.0;
                    pts[0].y = -side_len / 2.0;
                    pts[1].x = side_len / 2.0;
                    pts[1].y = -side_len / 2.0;
                    pts[2].x = side_len / 2.0;
                    pts[2].y = side_len / 2.0;
                    pts[3].x = -side_len / 2.0;
                    pts[3].y = side_len / 2.0;

                    for (int n = 0; n < 4; n++)
                    {
                        pts[n].y *= m_wh_ratio_for_drawn_line;
                    }

                    double[] out_doubles = new double[10];
                    angle = 0;
                    get_theta(start.x, start.y, end.x, end.y, out_doubles);
                    angle = out_doubles[0];

                    for (int n = 0; n < 4; n++)
                    {
                        double[] in_crds = new double[2];
                        double[] out_crds = new double[2];
                        in_crds[0] = pts[n].x;
                        in_crds[1] = pts[n].y;

                        rotate_crd(in_crds, out_crds, angle);
                        rotated_pts[n].x = out_crds[0];
                        rotated_pts[n].y = out_crds[1];
                    }

                    for (int n = 0; n < 4; n++)
                    {
                        new_pts[n].x = center.x + rotated_pts[n].x;
                        new_pts[n].y = center.y + rotated_pts[n].y;

                        m_drawn_line_pts[n] = new_pts[n];
                    }

                    switch (m_nTypeOfDrawnLine)
                    {
                        case MEASURE_TYPE.LINE_WIDTH_14:
                            if (true)
                            {
                                PointF point1 = new PointF();
                                PointF point2 = new PointF();
                                point1.X = (float)((pt1.x - (double)rect_left) * zoom_ratio);
                                point1.Y = (float)((pt1.y - (double)rect_top) * zoom_ratio);
                                point2.X = (float)((pt2.x - (double)rect_left) * zoom_ratio);
                                point2.Y = (float)((pt2.y - (double)rect_top) * zoom_ratio);
                                g.DrawLine(p, point1, point2);

                                for (int n = 0; n < 4; n++)
                                {
                                    point1.X = (float)((new_pts[n].x - (double)rect_left) * zoom_ratio);
                                    point1.Y = (float)((new_pts[n].y - (double)rect_top) * zoom_ratio);
                                    point2.X = (float)((new_pts[(n + 1) % 4].x - (double)rect_left) * zoom_ratio);
                                    point2.Y = (float)((new_pts[(n + 1) % 4].y - (double)rect_top) * zoom_ratio);
                                    g.DrawLine(p, point1, point2);
                                }
                            }
                            break;
                    }
                }
            }
        }

        public void translate_mouse_pt_to_graph_crd(int mouse_x, int mouse_y, ref int graph_x, ref int graph_y)
        {
            Point2d left_top, current_mouse_pt_on_image, current_mouse_pt_on_graph;
            left_top.x = (double)(Width - Image.Width) / 2.0;
            left_top.y = (double)(Height - Image.Height) / 2.0;
            current_mouse_pt_on_image.x = (double)(mouse_x) - left_top.x;
            current_mouse_pt_on_image.y = (double)(mouse_y) - left_top.y;
            current_mouse_pt_on_graph.x = m_current_view_crd_on_graph.x + (current_mouse_pt_on_image.x / m_zoom_ratio);
            current_mouse_pt_on_graph.y = m_current_view_crd_on_graph.y + (current_mouse_pt_on_image.y / m_zoom_ratio);

            graph_x = (int)Math.Round(current_mouse_pt_on_graph.x, 0);
            graph_y = (int)Math.Round(current_mouse_pt_on_graph.y, 0);
        }

        private int get_byte_buffer_value_flag(byte[] pBuf, int offset, int col)
        {
            int flag = 0;
            switch (col % 8)
            {
                case 0:
                    flag = ((m_pOriginImageData[offset + col / 8] & 0x80) > 0) ? 1 : 0;
                    break;
                case 1:
                    flag = ((m_pOriginImageData[offset + col / 8] & 0x40) > 0) ? 1 : 0;
                    break;
                case 2:
                    flag = ((m_pOriginImageData[offset + col / 8] & 0x20) > 0) ? 1 : 0;
                    break;
                case 3:
                    flag = ((m_pOriginImageData[offset + col / 8] & 0x10) > 0) ? 1 : 0;
                    break;
                case 4:
                    flag = ((m_pOriginImageData[offset + col / 8] & 0x08) > 0) ? 1 : 0;
                    break;
                case 5:
                    flag = ((m_pOriginImageData[offset + col / 8] & 0x04) > 0) ? 1 : 0;
                    break;
                case 6:
                    flag = ((m_pOriginImageData[offset + col / 8] & 0x02) > 0) ? 1 : 0;
                    break;
                case 7:
                    flag = ((m_pOriginImageData[offset + col / 8] & 0x01) > 0) ? 1 : 0;
                    break;
            }

            return flag;
        }

        public bool find_corner_pos(int nFiducialMarkCount, ref double x, ref double y)
        {
            int h = m_bitmap_1bit.Height;
            int w = m_bitmap_1bit.Width;
            int matched_x = 0;
            int matched_y = 0;
            int recheck_counter = 0;
            int step = MainUI.m_nGraphZoomRatio / 5000;
            if (step < 1)
                step = 1;
            int RECHECK_TIMES = 60 / step;
            double min_score = 0.2;

            Debugger.Log(0, null, string.Format("222222 find_corner_pos 111"));
            switch (nFiducialMarkCount)
            {
                case 1:
                    recheck_counter = 0;
                    for (int a = 0; a < h - step; a += step)
                    {
                        int count = 0;
                        int offset = a * m_nBytesPerLineForOriginImage;
                        for (int b = 0; b < w - step; b += step)
                        {
                            if (1 == get_byte_buffer_value_flag(m_pOriginImageData, offset, b))
                                count++;
                        }

                        double ratio = ((double)count / (double)w) * step;
                        if (ratio > min_score)
                        {
                            recheck_counter++;
                            if (recheck_counter > RECHECK_TIMES)
                            {
                                matched_y = a;
                                break;
                            }
                        }
                        else
                            recheck_counter = 0;
                    }

                    recheck_counter = 0;
                    for (int b = 0; b < w - step; b += step)
                    {
                        int count = 0;
                        for (int a = 0; a < h - step; a += step)
                        {
                            int offset = a * m_nBytesPerLineForOriginImage;
                            if (1 == get_byte_buffer_value_flag(m_pOriginImageData, offset, b))
                                count++;
                        }

                        double ratio = ((double)count / (double)h) * step;
                        if (ratio > min_score)
                        {
                            recheck_counter++;
                            if (recheck_counter > RECHECK_TIMES)
                            {
                                matched_x = b;
                                break;
                            }
                        }
                        else
                            recheck_counter = 0;
                    }

                    if (matched_x > 0 && matched_y > 0)
                    {
                        x = matched_x;
                        y = matched_y;
                    }
                    //Debugger.Log(0, null, string.Format("222222 matched_x = [{0:0.000},{1:0.000}]", matched_x, matched_y));
                    break;

                case 2:
                    recheck_counter = 0;
                    for (int a = 0; a < h - step; a += step)
                    {
                        int count = 0;
                        int offset = a * m_nBytesPerLineForOriginImage;
                        for (int b = 0; b < w - step; b += step)
                        {
                            if (1 == get_byte_buffer_value_flag(m_pOriginImageData, offset, b))
                                count++;
                        }

                        double ratio = ((double)count / (double)w) * step;
                        if (ratio > min_score)
                        {
                            recheck_counter++;
                            if (recheck_counter > RECHECK_TIMES)
                            {
                                matched_y = a;
                                break;
                            }
                        }
                        else
                            recheck_counter = 0;
                    }

                    recheck_counter = 0;
                    for (int b = w - 1; b > step; b -= step)
                    {
                        int count = 0;
                        for (int a = 0; a < h - step; a += step)
                        {
                            int offset = a * m_nBytesPerLineForOriginImage;
                            if (1 == get_byte_buffer_value_flag(m_pOriginImageData, offset, b))
                                count++;
                        }

                        double ratio = ((double)count / (double)h) * step;
                        if (ratio > min_score)
                        {
                            recheck_counter++;
                            if (recheck_counter > RECHECK_TIMES)
                            {
                                matched_x = b;
                                break;
                            }
                        }
                        else
                            recheck_counter = 0;
                    }

                    if (matched_x > 0 && matched_y > 0)
                    {
                        x = matched_x;
                        y = matched_y;
                    }
                    //Debugger.Log(0, null, string.Format("222222 matched_x = [{0:0.000},{1:0.000}]", matched_x, matched_y));
                    break;
            }
            
            Debugger.Log(0, null, string.Format("222222 width {0}, {1}", m_bitmap_1bit.Width, m_bitmap_1bit.Height));
            Debugger.Log(0, null, string.Format("222222 222 width {0}, {1}", m_bitmap_32bits.Width, m_bitmap_32bits.Height));

            return true;
        }

        public void clear_array_rects(ref List<List<rotated_array_rect>> array_rects)
        {
            for (int n = 0; n < array_rects.Count; n++)
                array_rects[n].Clear();
            array_rects.Clear();
        }

        public bool pre_check_before_array_recognition()
        {
            if (null == Image)
                return false;
            if ((Image.Width * Image.Height) <= 0)
                return false;
            if (0 == Form_GraphOrientation.m_nGraphType)
            {
                MessageBox.Show(null, "仅能在gerber图纸中进行此项操作。", "提示");
                return false;
            }
            if (false == m_bFinishFindingArray)
            {
                MessageBox.Show(null, "请等待上一次阵列识别完成，再进行阵列操作。", "提示");
                return false;
            }

            return true;
        }

        // 选定阵列unit左上角点的位置
        public void add_array_unit_lefttop_pos()
        {
            int graph_x = 0, graph_y = 0;
            get_mouse_crd_on_graph(ref graph_x, ref graph_y);

            if (m_list_array_unit_anchor_pts.Count > 0)
                m_list_array_unit_anchor_pts[0] = new Point(graph_x, graph_y);
            else
                m_list_array_unit_anchor_pts.Add(new Point(graph_x, graph_y));
        }

        // 选定阵列unit右下角点的位置
        public void add_array_unit_rightbottom_pos()
        {
            if (m_list_array_unit_anchor_pts.Count <= 0)
            {
                MessageBox.Show(null, "请先选定阵列unit左上角点。", "提示");
                return;
            }

            int graph_x = 0, graph_y = 0;
            get_mouse_crd_on_graph(ref graph_x, ref graph_y);

            if (m_list_array_unit_anchor_pts.Count > 1)
                m_list_array_unit_anchor_pts[1] = new Point(graph_x, graph_y);
            else
                m_list_array_unit_anchor_pts.Add(new Point(graph_x, graph_y));
        }

        public bool run_array_recognition()
        {
            List<Point> anchor_pts = m_list_array_unit_anchor_pts;
            if (anchor_pts.Count < 2)
                return false;

            clear_array_rects(ref m_ODB_thumbnail_array_rects);

            uint[] in_data = new uint[20];
            uint[] out_data = new uint[20];
            in_data[1] = (uint)anchor_pts[0].X;
            in_data[2] = (uint)anchor_pts[0].Y;
            in_data[3] = (uint)anchor_pts[1].X;
            in_data[4] = (uint)anchor_pts[1].Y;

            in_data[0] = 0;
            dllapi_find_gerber_array(in_data, out_data);
            if ((1 == in_data[0]) && (out_data[0] > 0))
            {
                Debugger.Log(0, null, string.Format("222222 array size = {0}", out_data[0]));

                int size = (int)out_data[0];
                out_data = new uint[size * 2];
                in_data[0] = 0;
                dllapi_get_gerber_array_info(in_data, out_data);

                int nRectWidth = (int)in_data[1];
                int nRectHeight = (int)in_data[2];
                int off_x = MainUI.m_nGraphOffsetX;
                int off_y = MainUI.m_nGraphOffsetY;
                double pixels_per_mm = MainUI.m_pixels_per_mm;

                List<rotated_array_rect> list_rects = new List<rotated_array_rect>();
                for (int n = 0; n < size; n++)
                {
                    rotated_array_rect rect = new rotated_array_rect();
                    rect.strUnitName = "gerber_default";
                    rect.angle = 0;
                    rect.odb_angle = 0;
                    rect.left = out_data[n * 2];
                    rect.top = out_data[n * 2 + 1];
                    rect.width = nRectWidth;
                    rect.height = nRectHeight;

                    rect.left = (rect.left - (double)off_x) / pixels_per_mm;
                    rect.top = (rect.top - (double)off_y) / pixels_per_mm;
                    rect.width /= pixels_per_mm;
                    rect.height /= pixels_per_mm;

                    list_rects.Add(rect);
                }
                list_rects[0].bSelected = true;
                m_ODB_thumbnail_array_rects.Add(list_rects);

                Debugger.Log(0, null, string.Format("222222 nRectWidth = [{0},{1}]", nRectWidth, nRectHeight));
                Debugger.Log(0, null, string.Format("222222 m_ODB_thumbnail_array_rects size = {0}", m_ODB_thumbnail_array_rects.Count));

                return true;
            }
            else
                return false;
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            if (MainUI.m_bIsFormActivated)
            {
                //string msg = string.Format("222222 ThumbnailView_{0}: OnMouseEnter", m_nOrientation);
                //Debugger.Log(0, null, msg);
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (MainUI.m_bIsFormActivated)
            {
                m_bMousePressed = false;
            }
        }

        private void OnClick(object sender, EventArgs e)
        {
            if (MainUI.m_bIsFormActivated && m_bHasValidImage)
            {
                MouseEventArgs mouse_event = (MouseEventArgs)e;
                if (MouseButtons.Left == mouse_event.Button)
                {
                    if (true == m_bDrawLineFrameByHand)
                        return;

                    if (true == m_parent.m_bIsCtrlKeyPressed)                               // 阵列unit选择
                    {
                        int graph_x = 0, graph_y = 0;
                        get_mouse_crd_on_graph(ref graph_x, ref graph_y);
                        if ((graph_x <= 0) || (graph_y <= 0)) return;
                        if ((graph_x >= m_bitmap_1bit.Width) || (graph_y >= m_bitmap_1bit.Height)) return;

                        int off_x = MainUI.m_nGraphOffsetX;
                        int off_y = MainUI.m_nGraphOffsetY;
                        double pixels_per_mm = MainUI.m_pixels_per_mm;
                        for (int n = 0; n < GraphView.m_ODB_thumbnail_array_rects.Count; n++)
                        {
                            List<rotated_array_rect> vec_rects = GraphView.m_ODB_thumbnail_array_rects[n];
                            for (int m = 0; m < vec_rects.Count; m++)
                            {
                                double left = vec_rects[m].left * pixels_per_mm + off_x;
                                double top = vec_rects[m].top * pixels_per_mm + off_y;
                                double right = left + vec_rects[m].width * pixels_per_mm;
                                double bottom = top + vec_rects[m].height * pixels_per_mm;

                                if ((graph_x > left) && (graph_x < right) && (graph_y > top) && (graph_y < bottom))
                                {
                                    vec_rects[m].bSelected = !vec_rects[m].bSelected;
                                    if (true == vec_rects[m].bSelected)
                                    {
                                        rotated_array_rect.m_nSelectCount++;
                                        vec_rects[m].m_nSelectOrder = rotated_array_rect.m_nSelectCount;
                                    }
                                    else
                                        rotated_array_rect.m_nSelectCount--;
                                    //Debugger.Log(0, null, string.Format("222222 name = {0}, angle = {1}, odb angle = {2}", 
                                    //    vec_rects[m].strUnitName, vec_rects[m].angle, vec_rects[m].odb_angle));
                                    break;
                                }
                            }
                        }

                        refresh_image(true);
                    }
                    else
                    {
                        if (false == m_bMouseDragged)
                        {
                            double temp = m_zoom_ratio * 4;
                            m_prev_zoom_ratio = m_zoom_ratio;
                            m_zoom_ratio = Math.Min(m_zoom_ratio_max, temp);
                            refresh_image();
                        }
                        else
                            m_bMouseDragged = false;
                    }
                }
                else if(MouseButtons.Right == mouse_event.Button)
                {
                }
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (MainUI.m_bIsFormActivated && m_bHasValidImage)
            {
                if (MouseButtons.Left == e.Button)
                {
                }
                else if (MouseButtons.Right == e.Button)
                {
                }
            }
        }

        public void OnDoubleClick(object sender, EventArgs e)
        {
            if (MainUI.m_bIsFormActivated && m_bHasValidImage)
            {
                MouseEventArgs mouse_event = (MouseEventArgs)e;
                if (MouseButtons.Left == mouse_event.Button)
                {
                    init_params(m_zoom_ratio_min, m_zoom_ratio_min, m_zoom_ratio_min);
                    refresh_image();
                }
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (MainUI.m_bIsFormActivated && m_bHasValidImage)
            {
                MouseEventArgs mouse_event = (MouseEventArgs)e;
                if (MouseButtons.Left == mouse_event.Button)
                {
                    if (true == m_bDrawLineFrameByHand)
                    {
                        int graph_x = 0;
                        int graph_y = 0;
                        translate_mouse_pt_to_graph_crd(e.X, e.Y, ref graph_x, ref graph_y);

                        m_start_pt_for_drawn_line = new Point2d(graph_x, graph_y);
                        m_end_pt_for_drawn_line = m_start_pt_for_drawn_line;
                        m_wh_ratio_for_drawn_line = 1;

                        this.refresh_image(true);
                    }

                    m_mouse_pt.X = e.X;
                    m_mouse_pt.Y = e.Y;
                    m_bMousePressed = true;
                }
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            MouseEventArgs mouse_event = (MouseEventArgs)e;
            if (MouseButtons.Left == mouse_event.Button)
            {
                m_bMousePressed = false;

                if (true == m_bDrawLineFrameByHand)
                {
                    if (DialogResult.Yes == MessageBox.Show(this, "请确认是否采用该手拉框?", "提示", MessageBoxButtons.YesNo))
                    {
                        m_parent.m_bIsWaitingForConfirm = false;
                        MainUI.m_event_wait_for_confirm_during_creation.Set();
                        MainUI.m_event_wait_for_manual_gauge.Set();
                        Thread.Sleep(50);

                        if (true == m_parent.m_bIsMeasuringDuringCreation)
                        {
                            m_bDrawLineFrameByHand = false;
                            return;
                        }
                        m_parent.m_bIsMeasuringDuringCreation = true;

                        int graph_x = 0, graph_y = 0;
                        MeasurePointData data = new MeasurePointData();

                        switch (m_nTypeOfDrawnLine)
                        {
                            case MEASURE_TYPE.LINE_WIDTH_14:
                                get_mouse_crd_on_graph(ref graph_x, ref graph_y);
                                if (false == m_parent.add_measure_point(m_nTypeOfDrawnLine, 0, 0, ref data, false, true))
                                {
                                    m_bDrawLineFrameByHand = false;
                                    m_parent.m_bIsMeasuringDuringCreation = false;
                                    return;
                                }
                                break;
                        }

                        m_parent.tabControl_Task.SelectedIndex = 1;
                        MainUI.m_event_wait_for_manual_gauge.Set();

                        MeasurePointData new_data = data.cloneClass();
                        new_data.m_bIsDrawnByHand = true;
                        m_parent.m_current_measure_graph_item = new_data;

                        if (m_parent.get_array_rects_count(GraphView.m_ODB_thumbnail_array_rects) > 1)
                            new Thread(m_parent.thread_locate_and_measure_item_in_array_mode).Start(new_data);
                        else
                        {
                            m_parent.m_measure_items_on_graph.Add(data);

                            MainUI.dl_message_sender send_message = m_parent.CBD_SendMessage;
                            send_message("刷新图纸测量项列表", false, true, null);
                            new Thread(m_parent.thread_locate_and_measure_item).Start(new_data);

                            for (int n = 0; n < 4; n++)
                                m_drawn_line_pts[n] = new Point2d(0, 0);
                        }
                        
                        m_bDrawLineFrameByHand = false;
                    }
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (MainUI.m_bIsFormActivated && m_bHasValidImage)
            {
                int graph_x = 0;
                int graph_y = 0;
                translate_mouse_pt_to_graph_crd(e.X, e.Y, ref graph_x, ref graph_y);

                if (true)
                {
                    double center_x = (graph_x - (double)(MainUI.m_nGraphOffsetX)) / MainUI.m_pixels_per_mm;
                    double center_y = ((double)(MainUI.m_nGraphHeight - MainUI.GRAPH_Y_OFFSET_SUM) - (graph_y
                        - (double)(MainUI.m_nGraphOffsetY))) / MainUI.m_pixels_per_mm;

                    string info = string.Format("图纸信息：{0}", Path.GetFileName(MainUI.m_strGraphFilePath));
                    //string crd = string.Format("        光标坐标: [{0}, {1}]", graph_x, graph_y);
                    string crd = string.Format("   [{0:0.000},{1:0.000}], [{2},{3}]", center_x, center_y, graph_x, MainUI.m_nGraphHeight - graph_y);
                    m_parent.toolStripStatusLabel_GraphInfo.Text = info + crd;
                }

                if (m_bMousePressed)
                {
                    if (true == m_bDrawLineFrameByHand)
                    {
                        m_end_pt_for_drawn_line = new Point2d(graph_x, graph_y);

                        this.refresh_image(true);
                        return;
                    }

                    m_bMouseDragged = true;

                    if (((m_zoom_ratio - m_zoom_ratio_min) > 0.00000001))
                    {
                        int offset_x = e.X - m_mouse_pt.X;
                        int offset_y = e.Y - m_mouse_pt.Y;

                        m_mouse_pt.X = e.X;
                        m_mouse_pt.Y = e.Y;

                        m_current_view_crd_on_graph.x -= (double)(offset_x) / m_zoom_ratio;
                        m_current_view_crd_on_graph.y -= (double)(offset_y) / m_zoom_ratio;

                        m_prev_zoom_ratio = m_zoom_ratio;

                        if ((GetTickCount() - m_nLastRefreshTime) > 18)
                        {
                            refresh_image();
                            m_nLastRefreshTime = GetTickCount();
                        }
                    }
                }
                else
                {
                    m_mouse_pt.X = e.X;
                    m_mouse_pt.Y = e.Y;
                }
            }
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (MainUI.m_bIsFormActivated && m_bHasValidImage)
            {
                if (true == m_bDrawLineFrameByHand)
                {
                    if (e.Delta > 0)
                    {
                        if (m_wh_ratio_for_drawn_line < 2)
                        {
                            m_wh_ratio_for_drawn_line += 0.1;
                            refresh_image(true);
                        }
                    }
                    else if (e.Delta < 0)
                    {
                        if (m_wh_ratio_for_drawn_line > 0.25)
                        {
                            m_wh_ratio_for_drawn_line -= 0.1;
                            refresh_image(true);
                        }
                    }
                }
                else
                {
                    if (e.Delta > 0)
                    {
                        double temp = m_zoom_ratio * 1.70;
                        m_prev_zoom_ratio = m_zoom_ratio;
                        m_zoom_ratio = Math.Min(m_zoom_ratio_max, temp);
                    }
                    else
                    {
                        double temp = m_zoom_ratio / 1.70;
                        m_prev_zoom_ratio = m_zoom_ratio;
                        m_zoom_ratio = Math.Max(m_zoom_ratio_min, temp);
                    }

                    refresh_image();
                }
            }
        }
    }
}
