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

namespace ZWLineGauger
{
    public class ThumbnailView : PictureBox
    {
        public int        m_nOrientation = 0;
        public bool     m_bMouseEntered = false;
        public bool     m_bMousePressed = false;
        public bool     m_bMouseDragged = false;
        public bool     m_bDrawArrayRects = false;                          // 是否需要绘制阵列

        public bool       m_bRequireRedraw32Bitmap = false;
        public bool       m_bHasValidImage = false;
        public Bitmap   m_bitmap_1bit;
        public Bitmap   m_bitmap_32bits;

        byte[]   m_pOriginImageData;

        double    m_prev_zoom_ratio = 1;
        double    m_zoom_ratio = 1;
        double    m_zoom_ratio_min = 1;
        double    m_zoom_ratio_max = 3;
        Point2d   m_current_view_crd_on_graph;                    // 当前视野左上角在图形上的坐标
        Point       m_mouse_pt;

        public ThumbnailView(int orient)
        {
            m_nOrientation = orient;

            this.MouseEnter += new System.EventHandler(OnMouseEnter);
            this.MouseLeave += new System.EventHandler(OnMouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(OnMouseMove);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(OnMouseWheel);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(OnMouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(OnMouseUp);
            this.Click += new System.EventHandler(OnClick);
            this.DoubleClick += new System.EventHandler(OnDoubleClick);
        }
        
        public void init_params(double current_ratio, double prev_ratio, double min_ratio)
        {
            m_zoom_ratio = current_ratio;
            m_prev_zoom_ratio = prev_ratio;
            m_zoom_ratio_min = min_ratio;

            m_current_view_crd_on_graph.set(0, 0);
        }

        public void render_black_image()
        {
            Bitmap bmp = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(bmp);
            SolidBrush b = new SolidBrush(Color.Black);           // 这里修改颜色
            g.FillRectangle(b, 0, 0, this.Width, this.Height);
            this.Image = bmp;
        }

        public void draw_array_rects_overlay(Bitmap bmp, double zoom_ratio, int rect_left, int rect_top, List<List<rotated_array_rect>> list_list_rects)
        {
            // 绘制覆盖层
            if (true)
            {
                Graphics g = Graphics.FromImage(bmp);

                Pen p = new Pen(Color.FromArgb(255, 150, 0), (float)2);

                int off_x = Form_GraphOrientation.m_nOdbOffsetX;
                int off_y = Form_GraphOrientation.m_nOdbOffsetY;
                double pixels_per_mm = Form_GraphOrientation.m_pixels_per_mm;

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
                            g.DrawLine(p, pts[k], pts[(k + 1) % 4]);
                    }
                }
            }
        }

        public void refresh_image()
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
            }
            else
            {
                ratio = (double)(this.Height) / (double)(m_bitmap_1bit.Height);
                double temp = (double)(m_bitmap_1bit.Width) * ratio;
                nBmpWidth = (int)temp;
            }
            
            // Image对象 m_show_image 初次赋值
            if (true == m_bRequireRedraw32Bitmap)
            {
                m_bitmap_32bits = new Bitmap(nBmpWidth, nBmpHeight, PixelFormat.Format32bppRgb);

                init_params(ratio, ratio, ratio);

                // 渲染
                if (true)
                {
                    Rectangle rect = new Rectangle(0, 0, m_bitmap_1bit.Width, m_bitmap_1bit.Height);
                    BitmapData bmpData = m_bitmap_1bit.LockBits(rect, ImageLockMode.ReadOnly, m_bitmap_1bit.PixelFormat);
                    IntPtr ptr = bmpData.Scan0;
                    int nBytesPerLineForOriginImage = Math.Abs(bmpData.Stride);
                    int nBytesLen = nBytesPerLineForOriginImage * m_bitmap_1bit.Height;
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
                        
                        double temp = Math.Round(((double)a / ratio), 0) * (double)nBytesPerLineForOriginImage;
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
                                thumbnail_buf[offset + b * 4 + 2] = GraphView.CAM_COLOR_VALUE;
                        }
                    }
                    
                    rect = new Rectangle(new Point(0, 0), m_bitmap_32bits.Size);
                    BitmapData bmp_data = m_bitmap_32bits.LockBits(rect, ImageLockMode.WriteOnly, m_bitmap_32bits.PixelFormat);
                    System.Runtime.InteropServices.Marshal.Copy(thumbnail_buf, 0, bmp_data.Scan0, thumbnail_buf.Length);
                    m_bitmap_32bits.UnlockBits(bmp_data);

                    if (true == m_bDrawArrayRects)
                        draw_array_rects_overlay(m_bitmap_32bits, ratio, 0, 0, GraphView.m_ODB_thumbnail_array_rects);

                    this.Image = m_bitmap_32bits;

                    string msg2 = string.Format("222222 控件宽高 [{0},{1}], 转换图像宽高 [{2},{3}], ratio = {4}",
                        this.Width, this.Height, nBmpWidth, nBmpHeight, ratio);
                    //Debugger.Log(0, null, msg2);
                }

                m_bHasValidImage = true;
                m_bRequireRedraw32Bitmap = false;
                
                return;
            }
            
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
                m_current_view_crd_on_graph.x = new_start_x;
                m_current_view_crd_on_graph.y = new_start_y;

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
                            thumbnail_buf[offset + b * 4 + 2] = GraphView.CAM_COLOR_VALUE;
                    }
                }
                
                rect = new Rectangle(new Point(0, 0), m_bitmap_32bits.Size);
                BitmapData bmp_data = m_bitmap_32bits.LockBits(rect, ImageLockMode.WriteOnly, m_bitmap_32bits.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(thumbnail_buf, 0, bmp_data.Scan0, thumbnail_buf.Length);
                m_bitmap_32bits.UnlockBits(bmp_data);

                if (true == m_bDrawArrayRects)
                    draw_array_rects_overlay(m_bitmap_32bits, ratio, rect_left, rect_top, GraphView.m_ODB_thumbnail_array_rects);
                
                this.Image = m_bitmap_32bits;
            }
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            if (Form_GraphOrientation.m_bIsFormActivated)
            {
                //string msg = string.Format("222222 ThumbnailView_{0}: OnMouseEnter", m_nOrientation);
                //Debugger.Log(0, null, msg);
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (Form_GraphOrientation.m_bIsFormActivated)
            {
                m_bMousePressed = false;
            }
        }

        private void OnClick(object sender, EventArgs e)
        {
            if (Form_GraphOrientation.m_bIsFormActivated && m_bHasValidImage)
            {
                MouseEventArgs mouse_event = (MouseEventArgs)e;
                if (MouseButtons.Left == mouse_event.Button)
                {
                    if (false == m_bMouseDragged)
                    {
                        double temp = m_zoom_ratio * 3;
                        m_prev_zoom_ratio = m_zoom_ratio;
                        m_zoom_ratio = Math.Min(m_zoom_ratio_max, temp);
                        refresh_image();
                    }
                    else
                        m_bMouseDragged = false;
                }
                else
                {

                }
            }
        }

        private void OnDoubleClick(object sender, EventArgs e)
        {
            if (Form_GraphOrientation.m_bIsFormActivated && m_bHasValidImage)
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
            if (Form_GraphOrientation.m_bIsFormActivated && m_bHasValidImage)
            {
                MouseEventArgs mouse_event = (MouseEventArgs)e;
                if (MouseButtons.Left == mouse_event.Button)
                {
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
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Form_GraphOrientation.m_bIsFormActivated && m_bHasValidImage)
            {
                if (m_bMousePressed)
                {
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
                        
                        refresh_image();
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
            if (Form_GraphOrientation.m_bIsFormActivated && m_bHasValidImage)
            {
                if (e.Delta > 0)
                {
                    double temp = m_zoom_ratio * 1.5;
                    m_prev_zoom_ratio = m_zoom_ratio;
                    m_zoom_ratio = Math.Min(m_zoom_ratio_max, temp);
                }
                else
                {
                    double temp = m_zoom_ratio / 1.5;
                    m_prev_zoom_ratio = m_zoom_ratio;
                    m_zoom_ratio = Math.Max(m_zoom_ratio_min, temp);
                }

                refresh_image();
            }
        }

    }
}
