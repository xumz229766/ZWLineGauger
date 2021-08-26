using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace ZWLineGauger.Gaugers
{
    class Gauger_CircleOuterToInner : Gauger
    {
        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool fit_circle(double[] in_doubles, int[] out_ints, double[] out_doubles);

        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool find_outer_circle(byte[] in_bytes, int[] in_data, int[] out_ints, double[] out_doubles);

        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool find_outer_circle2(byte[] in_bytes, int[] in_data, int[] out_ints, double[] out_doubles);

        Point2d m_move_pt = new Point2d();
        Point2d   m_region_center = new Point2d();
        
        double   m_region_radius = 0;
        double   m_region_extension = 30;

        public Gauger_CircleOuterToInner(MainUI parent, PictureBox pb)
        {
            this.parent = parent;
            m_nPictureBoxWidth = pb.Width;
            m_nPictureBoxHeight = pb.Height;

            m_measure_type = MEASURE_TYPE.CIRCLE_OUTER_TO_INNER;
        }
        
        // 测量
        public override bool gauge(Image img, byte[] pImageBuf = null, int nWidth = 0, int nHeight = 0, int nBytesPerLine = 0, int nAlgorithm = 0, bool bLocateLineInRealTime = false)
        {
            //Debugger.Log(0, null, string.Format("222222 111"));

            int nStride = 0;
            byte[] pBuf;

            lock (parent.m_main_cam_lock)
            {
                pBuf = GeneralUtils.convert_image_to_bytes(img, img.RawFormat, ref nStride);
            }

            int[] in_data = new int[10];
            int[] out_ints = new int[10];
            double[] out_doubles = new double[10];

            double ratio_x = (double)img.Width / (double)m_nPictureBoxWidth;
            double ratio_y = (double)img.Height / (double)m_nPictureBoxHeight;
            
            // ROI 范围
            int nRoiLeft = (int)((m_region_center.x - m_region_radius - m_region_extension) * ratio_x);
            int nRoiTop = (int)((m_region_center.y - m_region_radius - m_region_extension) * ratio_y);
            int nRoiWidth = (int)((m_region_radius + m_region_extension) * 2 * ratio_x);
            int nRoiHeight = (int)((m_region_radius + m_region_extension) * 2 * ratio_y);
            
            in_data[0] = img.Width;
            in_data[1] = img.Height;
            in_data[2] = pBuf.Length;
            in_data[3] = nStride;
            in_data[4] = nRoiLeft;
            in_data[5] = nRoiTop;
            in_data[6] = nRoiWidth;
            in_data[7] = nRoiHeight;

            out_ints[0] = 0;
            find_outer_circle2(pBuf, in_data, out_ints, out_doubles);
            if (1 == out_ints[0])
            {
                m_bHasValidGaugeResult = true;
                
                m_gauged_circle_center.x = out_doubles[0];
                m_gauged_circle_center.y = out_doubles[1];
                m_gauged_circle_radius = out_doubles[2] * 2;

                m_object_center = m_gauged_circle_center;
                return true;
            }
            else
            {
                //if (true == m_bRotatedRectIsReady)
                    set_failure_time();

                return false;
            }
        }

        // 测量
        public bool gauge(Image img, object cam_lock, Point2d center, double region_radius, double extension)
        {
            int nStride = 0;
            byte[] pBuf;

            //Debugger.Log(0, null, string.Format("222222 aaa"));
            //return true;

            lock (cam_lock)
            {
                pBuf = GeneralUtils.convert_image_to_bytes(img, img.RawFormat, ref nStride);
            }

            int[] in_data = new int[10];
            int[] out_ints = new int[10];
            double[] out_doubles = new double[10];

            // ROI 范围
            int nRoiLeft = (int)(center.x - (region_radius / 2) - extension);
            int nRoiTop = (int)(center.y - (region_radius / 2) - extension);
            int nRoiWidth = (int)(region_radius + extension * 2);
            int nRoiHeight = (int)(region_radius + extension * 2);

            //Debugger.Log(0, null, string.Format("222222 nRoiLeft = [{0},{1}], nRoiWidth = {2},{3}, region_radius = {4:0}", nRoiLeft, nRoiTop, nRoiWidth, nRoiHeight, region_radius));

            in_data[0] = img.Width;
            in_data[1] = img.Height;
            in_data[2] = pBuf.Length;
            in_data[3] = nStride;
            in_data[4] = nRoiLeft;
            in_data[5] = nRoiTop;
            in_data[6] = nRoiWidth;
            in_data[7] = nRoiHeight;

            //Debugger.Log(0, null, string.Format("222222 bbb"));

            out_ints[0] = 0;
            find_outer_circle2(pBuf, in_data, out_ints, out_doubles);

            //Debugger.Log(0, null, string.Format("222222 ccc"));

            if (1 == out_ints[0])
            {
                m_bHasValidGaugeResult = true;

                m_gauged_circle_center.x = out_doubles[0];
                m_gauged_circle_center.y = out_doubles[1];
                m_gauged_circle_radius = out_doubles[2] * 2;

                return true;
            }
            else
            {
                //if (true == m_bRotatedRectIsReady)
                    set_failure_time();

                return false;
            }
        }

        public override void on_mouse_down(MouseEventArgs me, PictureBox pb)
        {
            // 左键鼠标按下
            if (MouseButtons.Left == me.Button)
            {
                m_bHasValidGaugeResult = false;

                if (3 == m_nClickCounter)
                {
                    m_nClickCounter = 0;

                    m_click_pts[0].set(me.X, me.Y);
                    
                    for (int n = 1; n < m_click_pts.Length; n++)
                        m_click_pts[n].set(0, 0);
                }
                else
                    m_click_pts[m_nClickCounter].set(me.X, me.Y);

                m_nClickCounter++;
                
                pb.Refresh();
            }
        }

        public override void on_mouse_up(MouseEventArgs me, PictureBox pb)
        {
            // 右键鼠标松开
            if (MouseButtons.Right == me.Button)
            {
                if (3 == m_nClickCounter)
                {
                    gauge(pb.Image);
                }
                
                if (false == m_bHasValidGaugeResult)
                {
                    for (int n = 0; n < m_click_pts.Length; n++)
                        m_click_pts[n].set(0, 0);

                    m_nClickCounter = 0;
                }

                pb.Refresh();
            }
        }

        public override void on_mouse_move(MouseEventArgs me, PictureBox pb, bool bHorizonModeForGaugerRect = true)
        {
            if (2 == m_nClickCounter)
            {
                m_move_pt.set(me.X, me.Y);

                if (GeneralUtils.get_distance(m_click_pts[1], m_move_pt) > 5)
                    pb.Refresh();
            }
        }

        public override void on_mouse_wheel(MouseEventArgs me, PictureBox pb, bool bHorizonModeForGaugerRect = true)
        {
            if (3 == m_nClickCounter)
            {
                if (me.Delta > 0)
                    m_region_extension += 5;
                else
                    m_region_extension -= 5;

                if (m_region_extension < 3)
                    m_region_extension = 3;
                
                pb.Refresh();
            }
        }

        public override void on_mouse_leave(EventArgs me, PictureBox pb)
        {

        }

        public override void on_paint(PaintEventArgs pe, PictureBox pb, Image source_image = null, bool bDrawToSourceImage = false)
        {
            Graphics g = pe.Graphics;

            if (m_nClickCounter > 0)
            {
                bool bIsValidCircle = false;
                double dot_radius = 2;
                Pen red_pen = new Pen(Color.FromArgb(255, 0, 0), 3);
                Pen green_pen = new Pen(Color.FromArgb(0, 255, 0), 2);
                Pen yellow_pen = new Pen(Color.FromArgb(255, 255, 0), 2);
                Pen red_pen_with_dash = new Pen(Color.FromArgb(255, 0, 0), 2);
                Pen green_pen_with_dash = new Pen(Color.FromArgb(0, 255, 0), 2);
                Pen yellow_pen_with_dash = new Pen(Color.FromArgb(255, 255, 0), 2);

                red_pen_with_dash.DashStyle = DashStyle.Custom;
                red_pen_with_dash.DashPattern = new float[] { 3f, 5f };
                green_pen_with_dash.DashStyle = DashStyle.Custom;
                green_pen_with_dash.DashPattern = new float[] { 3f, 5f };
                yellow_pen_with_dash.DashStyle = DashStyle.Custom;
                yellow_pen_with_dash.DashPattern = new float[] { 3f, 5f };

                // 绘制圆
                if (((2 == m_nClickCounter) || (3 == m_nClickCounter)) && (false == m_bHasValidGaugeResult))
                {
                    if (GeneralUtils.get_distance(m_click_pts[1], m_move_pt) > 5)
                    {
                        //Debugger.Log(0, null, string.Format("222222 [{0},{1}], [{2},{3}], [{4},{5}]", m_click_pts[0].x, m_click_pts[0].y,
                            //m_click_pts[1].x, m_click_pts[1].y, m_move_pt.x, m_move_pt.y));

                        int[] out_ints = new int[10];
                        double[] in_doubles = new double[10];
                        double[] out_doubles = new double[10];

                        in_doubles[0] = m_click_pts[0].x;
                        in_doubles[1] = m_click_pts[0].y;
                        in_doubles[2] = m_click_pts[1].x;
                        in_doubles[3] = m_click_pts[1].y;
                        if (2 == m_nClickCounter)
                        {
                            in_doubles[4] = m_move_pt.x;
                            in_doubles[5] = m_move_pt.y;
                        }
                        else
                        {
                            in_doubles[4] = m_click_pts[2].x;
                            in_doubles[5] = m_click_pts[2].y;
                        }

                        out_ints[0] = 0;
                        fit_circle(in_doubles, out_ints, out_doubles);
                        if (1 == out_ints[0])
                        {
                            bIsValidCircle = true;

                            m_region_center.x = out_doubles[0];
                            m_region_center.y = out_doubles[1];
                            m_region_radius = out_doubles[2];

                            float x = (float)(m_region_center.x - m_region_radius);
                            float y = (float)(m_region_center.y - m_region_radius);

                            if (3 == m_nClickCounter)
                                g.DrawEllipse(yellow_pen, x, y, (float)m_region_radius * 2, (float)m_region_radius * 2);
                            else
                                g.DrawEllipse(yellow_pen_with_dash, x, y, (float)m_region_radius * 2, (float)m_region_radius * 2);
                        }
                        
                    }
                }

                // 绘制 region
                if ((3 == m_nClickCounter) && (true == bIsValidCircle) && (false == m_bHasValidGaugeResult))
                {
                    double radius = m_region_radius + m_region_extension;
                    float x = (float)(m_region_center.x - radius);
                    float y = (float)(m_region_center.y - radius);
                    g.DrawEllipse(green_pen_with_dash, x, y, (float)radius * 2, (float)radius * 2);

                    radius = m_region_radius - m_region_extension;
                    x = (float)(m_region_center.x - radius);
                    y = (float)(m_region_center.y - radius);
                    g.DrawEllipse(green_pen_with_dash, x, y, (float)radius * 2, (float)radius * 2);
                }

                if (false == m_bHasValidGaugeResult)
                {
                    for (int n = 0; n < m_nClickCounter; n++)
                    {
                        g.DrawEllipse(red_pen, (float)(m_click_pts[n].x - dot_radius), (float)(m_click_pts[n].y - dot_radius),
                            (float)dot_radius * 2, (float)dot_radius * 2);
                    }
                }
            }

            // 绘制结果
            if (true == m_bHasValidGaugeResult)
            {
                Pen green_pen = new Pen(Color.FromArgb(0, 255, 0), 2);

                double ratio_x = (double)pb.Image.Width / (double)m_nPictureBoxWidth;
                double ratio_y = (double)pb.Image.Height / (double)m_nPictureBoxHeight;

                float x = (float)((m_gauged_circle_center.x - (m_gauged_circle_radius / 2)) / ratio_x);
                float y = (float)((m_gauged_circle_center.y - (m_gauged_circle_radius / 2)) / ratio_y);

                g.DrawEllipse(green_pen, x, y,
                    (float)((m_gauged_circle_radius * 1) / ratio_x), (float)((m_gauged_circle_radius * 1) / ratio_y));

                // 绘制数字值
                if (true)
                {
                    double value = m_gauged_circle_radius / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                    value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                    String str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                        m_strUnits[parent.m_nUnitType]);
                    
                    float start_x = (float)((m_gauged_circle_center.x) / ratio_x);
                    float start_y = (float)((m_gauged_circle_center.y) / ratio_y);
                    PointF start = new PointF();
                    start.X = start_x;
                    start.Y = start_y;
                    //Font ft = new Font("宋体", 25, FontStyle.Bold);
                    //Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));
                 
                    //g.DrawString(str, ft, brush, start_x, start_y);
                    set_Font(g, str, pb, start, bDrawToSourceImage);//设置字体颜色位置等
                }
            }
        }

        public override void show_selection_frame(PaintEventArgs pe, PictureBox pb, Image source_image = null, bool bDrawToSourceImage = false)
        {

        }

        public override void copy_measure_result_data(ref Gauger dest)
        {
            ((Gauger_CircleOuterToInner)dest).m_bHasValidGaugeResult = m_bHasValidGaugeResult;
            ((Gauger_CircleOuterToInner)dest).m_gauged_circle_center = m_gauged_circle_center;
            ((Gauger_CircleOuterToInner)dest).m_gauged_circle_radius = m_gauged_circle_radius;
            ((Gauger_CircleOuterToInner)dest).m_region_center = m_region_center;
            ((Gauger_CircleOuterToInner)dest).m_region_radius = m_region_radius;
            dest.m_string_offset_real = m_string_offset_real;
        }
    }
}
