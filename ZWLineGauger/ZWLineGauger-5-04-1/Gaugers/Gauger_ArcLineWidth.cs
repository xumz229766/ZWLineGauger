
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZWLineGauger.Gaugers
{
    class Gauger_ArcLineWidth : Gauger
    {
        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool rotate_crd(double[] in_crds, double[] out_crds, double rotate_angle);

        [DllImport("pxflow.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool find_line_space(byte[] in_bytes, int[] in_data, double[] in_crds, int[] out_ints, double[] out_crds);

        [DllImport("pxflow.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool find_arc_line_width(byte[] in_bytes, int[] in_data, double[] in_crds, int[] out_ints, double[] out_crds);

        Point2d m_down_pt = new Point2d(0, 0);
        Point2d m_up_pt = new Point2d(0, 0);
        Point2d[] m_rotated_rect_corners = new Point2d[4];
        Point2d[] m_gauge_result_pts = new Point2d[20];

        double m_rotated_rect_wh_ratio = DEFAULT_ROTATED_RECT_WH_RATIO;

        public Gauger_ArcLineWidth(MainUI parent, PictureBox pb, MEASURE_TYPE type)
        {
            this.parent = parent;
            m_nPictureBoxWidth = pb.Width;
            m_nPictureBoxHeight = pb.Height;

            m_measure_type = type;
        }

        public override bool gauge(Image img, byte[] pImageBuf = null, int nWidth = 0, int nHeight = 0, int nBytesPerLine = 0, int nAlgorithm = 0, bool bLocateLineInRealTime = false)
        {
            int nStride = 0;
            byte[] pBuf;

            lock (parent.m_main_cam_lock)
            {
                pBuf = GeneralUtils.convert_image_to_bytes(img, img.RawFormat, ref nStride);
            }

            double ratio_x = (double)img.Width / (double)m_nPictureBoxWidth;
            double ratio_y = (double)img.Height / (double)m_nPictureBoxHeight;

            int[] out_ints = new int[10];
            int[] in_data = new int[10];
            in_data[0] = img.Width;
            in_data[1] = img.Height;
            in_data[2] = pBuf.Length;
            in_data[3] = nStride;

            double[] in_crds = new double[10];
            double[] out_crds = new double[40];
            for (int n = 0; n < 4; n++)
            {
                in_crds[n * 2] = m_rotated_rect_corners[n].x * ratio_x;
                in_crds[n * 2 + 1] = m_rotated_rect_corners[n].y * ratio_y;

                m_ROI_rect[n].x = m_rotated_rect_corners[n].x * ratio_x;
                m_ROI_rect[n].y = m_rotated_rect_corners[n].y * ratio_y;

                //Debugger.Log(0, null, string.Format("222222 [{0:0.0},{1:0.0}]", in_crds[n * 2], in_crds[n * 2 + 1]));
            }

            out_ints[0] = 0;
            find_arc_line_width(pBuf, in_data, in_crds, out_ints, out_crds);
            if (1 == out_ints[0])
            {
                m_bHasValidGaugeResult = true;
                m_gauge_result_pts[0].x = out_crds[0];
                m_gauge_result_pts[0].y = out_crds[1];
                m_gauge_result_pts[1].x = out_crds[2];
                m_gauge_result_pts[1].y = out_crds[3];
                m_gauge_result_pts[2].x = out_crds[4];
                m_gauge_result_pts[2].y = out_crds[5];
                m_gauge_result_pts[3].x = out_crds[6];
                m_gauge_result_pts[3].y = out_crds[7];
                m_gauge_result_pts[4].x = out_crds[8];
                m_gauge_result_pts[4].y = out_crds[9];
                m_gauge_result_pts[5].x = out_crds[10];
                m_gauge_result_pts[5].y = out_crds[11];

                m_gauge_result_pts[10].x = out_crds[20];
                m_gauge_result_pts[10].y = out_crds[21];
                m_gauge_result_pts[11].x = out_crds[22];
                m_gauge_result_pts[11].y = out_crds[23];
                m_gauge_result_pts[12].x = out_crds[24];
                m_gauge_result_pts[12].y = out_crds[25];
                m_gauge_result_pts[13].x = out_crds[26];
                m_gauge_result_pts[13].y = out_crds[27];
                m_gauge_result_pts[14].x = out_crds[28];
                m_gauge_result_pts[14].y = out_crds[29];
                m_gauge_result_pts[15].x = out_crds[30];
                m_gauge_result_pts[15].y = out_crds[31];

                m_gauged_line_width = GeneralUtils.get_distance(m_gauge_result_pts[2], m_gauge_result_pts[12]);

                return true;
            }
            else
                return false;
        }

        public bool gauge(Image img, Point2d[] rect, double extension, bool bHandMade = false)
        {
            int nStride = 0;
            byte[] pBuf;

            lock (parent.m_main_cam_lock)
            {
                pBuf = GeneralUtils.convert_image_to_bytes(img, img.RawFormat, ref nStride);
            }

            int[] out_ints = new int[10];
            int[] in_data = new int[10];
            in_data[0] = img.Width;
            in_data[1] = img.Height;
            in_data[2] = pBuf.Length;
            in_data[3] = nStride;

            double[] in_crds = new double[10];
            double[] out_crds = new double[40];

            if (true == bHandMade)
            {
                for (int n = 0; n < 4; n++)
                {
                    in_crds[n * 2] = rect[n].x;
                    in_crds[n * 2 + 1] = rect[n].y;

                    //Debugger.Log(0, null, string.Format("222222 [{0:0.0},{1:0.0}]", in_crds[n * 2], in_crds[n * 2 + 1]));
                }
            }
            else
            {
                in_crds[0] = rect[0].x;
                in_crds[1] = rect[0].y;
                in_crds[2] = rect[3].x;
                in_crds[3] = rect[3].y;
                in_crds[4] = rect[2].x;
                in_crds[5] = rect[2].y;
                in_crds[6] = rect[1].x;
                in_crds[7] = rect[1].y;
            }

            out_ints[0] = 0;
            find_arc_line_width(pBuf, in_data, in_crds, out_ints, out_crds);
            if (1 == out_ints[0])
            {
                m_bHasValidGaugeResult = true;
                m_gauge_result_pts[0].x = out_crds[0];
                m_gauge_result_pts[0].y = out_crds[1];
                m_gauge_result_pts[1].x = out_crds[2];
                m_gauge_result_pts[1].y = out_crds[3];
                m_gauge_result_pts[2].x = out_crds[4];
                m_gauge_result_pts[2].y = out_crds[5];
                m_gauge_result_pts[3].x = out_crds[6];
                m_gauge_result_pts[3].y = out_crds[7];
                m_gauge_result_pts[4].x = out_crds[8];
                m_gauge_result_pts[4].y = out_crds[9];
                m_gauge_result_pts[5].x = out_crds[10];
                m_gauge_result_pts[5].y = out_crds[11];

                m_gauge_result_pts[10].x = out_crds[20];
                m_gauge_result_pts[10].y = out_crds[21];
                m_gauge_result_pts[11].x = out_crds[22];
                m_gauge_result_pts[11].y = out_crds[23];
                m_gauge_result_pts[12].x = out_crds[24];
                m_gauge_result_pts[12].y = out_crds[25];
                m_gauge_result_pts[13].x = out_crds[26];
                m_gauge_result_pts[13].y = out_crds[27];
                m_gauge_result_pts[14].x = out_crds[28];
                m_gauge_result_pts[14].y = out_crds[29];
                m_gauge_result_pts[15].x = out_crds[30];
                m_gauge_result_pts[15].y = out_crds[31];

                m_gauged_line_width = GeneralUtils.get_distance(m_gauge_result_pts[2], m_gauge_result_pts[12]);

                return true;
            }
            else
                return false;
        }

        public void update_rotated_rect()
        {
            Point2d[] pts = new Point2d[4];
            double ratio = m_rotated_rect_wh_ratio;
            double[] in_crds = new double[2];
            double[] out_crds = new double[2];
            in_crds[0] = (m_up_pt.x - m_down_pt.x) / ratio;
            in_crds[1] = (m_up_pt.y - m_down_pt.y) / ratio;

            rotate_crd(in_crds, out_crds, 90);
            m_rotated_rect_corners[0].set(m_down_pt.x + out_crds[0], m_down_pt.y + out_crds[1]);

            rotate_crd(in_crds, out_crds, -90);
            m_rotated_rect_corners[1].set(m_down_pt.x + out_crds[0], m_down_pt.y + out_crds[1]);

            in_crds[0] = (m_down_pt.x - m_up_pt.x) / ratio;
            in_crds[1] = (m_down_pt.y - m_up_pt.y) / ratio;

            rotate_crd(in_crds, out_crds, 90);
            m_rotated_rect_corners[2].set(m_up_pt.x + out_crds[0], m_up_pt.y + out_crds[1]);

            rotate_crd(in_crds, out_crds, -90);
            m_rotated_rect_corners[3].set(m_up_pt.x + out_crds[0], m_up_pt.y + out_crds[1]);
        }

        public override void on_mouse_down(MouseEventArgs me, PictureBox pb)
        {
            // 左键鼠标按下
            if (MouseButtons.Left == me.Button)
            {
                m_nClickCounter++;

                if (1 == (m_nClickCounter % 2))
                {
                    m_bIsMouseDown = true;
                    m_bRotatedRectIsReady = false;
                    m_bHasValidGaugeResult = false;

                    //m_rotated_rect_wh_ratio = DEFAULT_ROTATED_RECT_WH_RATIO;
                    m_down_pt.x = me.X;
                    m_down_pt.y = me.Y;
                }
                else
                {
                    double dist = Math.Sqrt((me.X - m_down_pt.x) * (me.X - m_down_pt.x) + (me.Y - m_down_pt.y) * (me.Y - m_down_pt.y));
                    if (dist > 5)
                    {
                        m_bRotatedRectIsReady = true;
                    }
                }
            }
            else if (MouseButtons.Right == me.Button)
            {
                if ((0 == m_down_pt.x) && (0 == m_down_pt.y))
                    return;

                if (1 == (m_nClickCounter % 2))
                {
                    m_nClickCounter++;
                    m_up_pt.set(me.X, me.Y);
                }

                double dist = Math.Sqrt((me.X - m_down_pt.x) * (me.X - m_down_pt.x) + (me.Y - m_down_pt.y) * (me.Y - m_down_pt.y));
                if (dist < 5)
                {
                    m_down_pt.set(0, 0);
                    m_up_pt.set(0, 0);
                    for (int n = 0; n < 4; n++)
                    {
                        m_rotated_rect_corners[n].set(0, 0);
                    }
                }
                else
                {
                    m_bIsMouseDown = false;
                    m_bRotatedRectIsReady = true;
                }

                pb.Refresh();
            }
        }

        public override void on_mouse_up(MouseEventArgs me, PictureBox pb)
        {
            // 左键鼠标松开
            if ((MouseButtons.Left == me.Button) && (m_down_pt.x > 0))
            {
                return;
            }
            // 右键鼠标松开
            else if (MouseButtons.Right == me.Button)
            {
                //if (true == m_bRotatedRectIsReady)
                {
                    gauge(pb.Image);
                }

                m_bRotatedRectIsReady = false;
                m_down_pt.set(0, 0);
                m_up_pt.set(0, 0);

                pb.Refresh();
            }
        }

        public override void on_mouse_move(MouseEventArgs me, PictureBox pb)
        {
            // 左键鼠标松开
            if ((1 == (m_nClickCounter % 2)) && (m_down_pt.x > 0))
            {
                m_up_pt.set(me.X, me.Y);

                double dist = Math.Sqrt((me.X - m_down_pt.x) * (me.X - m_down_pt.x) + (me.Y - m_down_pt.y) * (me.Y - m_down_pt.y));
                if (dist > 5)
                {
                    m_bRotatedRectIsReady = true;
                }

                update_rotated_rect();

                //string msg = string.Format("222222 m_mv_down_pt = [{0},{1}], m_mv_up_pt = [{2},{3}]", m_mv_down_pt.x, m_mv_down_pt.y, m_mv_up_pt.x, m_mv_up_pt.y);
                //Debugger.Log(0, null, msg);

                pb.Refresh();
            }
        }

        public override void on_mouse_wheel(MouseEventArgs me, PictureBox pb)
        {
            //string msg = string.Format("222222 111 down [{0},{1}], up [{2},{3}]", m_down_pt.x, m_down_pt.y, m_up_pt.x, m_up_pt.y);
            //Debugger.Log(0, null, msg);

            if (true == m_bRotatedRectIsReady)
            {
                double ratio = m_rotated_rect_wh_ratio;
                if (me.Delta < 0)
                    ratio *= 1.1;
                else
                    ratio /= 1.1;
                if (ratio > 15) ratio = 15;
                if (ratio < 0.2) ratio = 0.2;
                m_rotated_rect_wh_ratio = ratio;

                update_rotated_rect();
                pb.Refresh();
            }
        }

        public override void on_paint(PaintEventArgs pe, PictureBox pb)
        {
            Graphics g = pe.Graphics;

            try
            {
                // 绘制ROI框
                if ((m_down_pt.x > 0) && (m_down_pt.y > 0))
                {
                    Pen p = new Pen(Color.FromArgb(0, 255, 0), 2);
                    p.DashStyle = DashStyle.Custom;
                    p.DashPattern = new float[] { 3f, 3f };

                    PointF[] corner_pts = new PointF[4];
                    for (int n = 0; n < 4; n++)
                    {
                        corner_pts[n].X = (float)m_rotated_rect_corners[n].x;
                        corner_pts[n].Y = (float)m_rotated_rect_corners[n].y;
                    }
                    for (int n = 0; n < 4; n++)
                        g.DrawLine(p, corner_pts[n], corner_pts[(n + 1) % 4]);

                    AdjustableArrowCap arrow_cap = new AdjustableArrowCap(6, 6, true);
                    Pen arrow_pen = new Pen(Color.FromArgb(0, 255, 0), 2);
                    arrow_pen.CustomEndCap = arrow_cap;

                    PointF down_pt = new PointF((float)m_down_pt.x, (float)m_down_pt.y);
                    PointF up_pt = new PointF((float)m_up_pt.x, (float)m_up_pt.y);
                    PointF[] arrow_pts = new PointF[4];
                    arrow_pts[0] = down_pt;
                    arrow_pts[2] = up_pt;
                    arrow_pts[1].X = down_pt.X + (up_pt.X - down_pt.X) / 7;
                    arrow_pts[1].Y = down_pt.Y + (up_pt.Y - down_pt.Y) / 7;
                    arrow_pts[3].X = up_pt.X + (down_pt.X - up_pt.X) / 7;
                    arrow_pts[3].Y = up_pt.Y + (down_pt.Y - up_pt.Y) / 7;
                    g.DrawLine(arrow_pen, arrow_pts[0], arrow_pts[1]);
                    g.DrawLine(arrow_pen, arrow_pts[2], arrow_pts[3]);
                }

                // 显示抓边结果
                if (true == m_bHasValidGaugeResult)
                {
                    double ratio_x = (double)pb.Image.Width / (double)m_nPictureBoxWidth;
                    double ratio_y = (double)pb.Image.Height / (double)m_nPictureBoxHeight;

                    Pen red_pen = new Pen(Color.FromArgb(255, 0, 0), 1);
                    Pen green_pen = new Pen(Color.FromArgb(0, 255, 0), 2);
                    Point2d center = new Point2d(0, 0);
                    PointF[] pts = new PointF[20];
                    PointF[] mid_pts = new PointF[6];

                    for (int n = 0; n < 2; n++)
                    {
                        pts[n].X = (float)((m_gauge_result_pts[n].x - 1) / ratio_x);
                        pts[n].Y = (float)((m_gauge_result_pts[n].y - 1) / ratio_y);
                        pts[n + 2].X = (float)((m_gauge_result_pts[n + 2].x - 1) / ratio_x);
                        pts[n + 2].Y = (float)((m_gauge_result_pts[n + 2].y - 1) / ratio_y);
                        
                        pts[n + 10].X = (float)((m_gauge_result_pts[n + 10].x - 1) / ratio_x);
                        pts[n + 10].Y = (float)((m_gauge_result_pts[n + 10].y - 1) / ratio_y);
                        pts[n + 12].X = (float)((m_gauge_result_pts[n + 12].x - 1) / ratio_x);
                        pts[n + 12].Y = (float)((m_gauge_result_pts[n + 12].y - 1) / ratio_y);

                        center.x += m_gauge_result_pts[n].x;
                        center.y += m_gauge_result_pts[n].y;
                        center.x += m_gauge_result_pts[n + 10].x;
                        center.y += m_gauge_result_pts[n + 10].y;
                    }
                    pts[4].X = (float)((m_gauge_result_pts[4].x) / ratio_x);
                    pts[4].Y = (float)(m_gauge_result_pts[4].y);
                    pts[5].X = (float)(m_gauge_result_pts[5].x);
                    pts[14].X = (float)((m_gauge_result_pts[14].x) / ratio_x);
                    pts[14].Y = (float)(m_gauge_result_pts[14].y);
                    pts[15].X = (float)(m_gauge_result_pts[15].x);

                    center.x /= 4;
                    center.y /= 4;
                    m_object_center = center;
                    
                    g.DrawLine(green_pen, pts[2], pts[12]);
                    //g.DrawLine(green_pen, pts[0], pts[1]);
                    //g.DrawLine(green_pen, pts[10], pts[11]);

                    float top_arc_radius = pts[4].X;
                    float bot_arc_radius = pts[14].X;
                    float top_arc_start_angle = pts[4].Y;
                    float top_arc_sweep_angle = pts[5].X;
                    float bot_arc_start_angle = pts[14].Y;
                    float bot_arc_sweep_angle = pts[15].X;

                    //Debugger.Log(0, null, string.Format("222222 top_arc_sweep_angle = {0}", top_arc_sweep_angle));

                    g.DrawArc(green_pen, pts[3].X - top_arc_radius, pts[3].Y - top_arc_radius, top_arc_radius * 2, top_arc_radius * 2,
                        top_arc_start_angle, top_arc_sweep_angle);
                    g.DrawArc(green_pen, pts[13].X - bot_arc_radius, pts[13].Y - bot_arc_radius, bot_arc_radius * 2, bot_arc_radius * 2,
                        bot_arc_start_angle, bot_arc_sweep_angle);
                    //g.DrawEllipse(green_pen, pts[3].X - top_arc_radius, pts[3].Y - top_arc_radius, top_arc_radius * 2, top_arc_radius * 2);
                    //g.DrawEllipse(green_pen, pts[13].X - bot_arc_radius, pts[13].Y - bot_arc_radius, bot_arc_radius * 2, bot_arc_radius * 2);

                    double value = m_gauged_line_width / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                    value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                    String str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                        m_strUnits[parent.m_nUnitType]);

                    Font ft = new Font("宋体", 25, FontStyle.Bold);
                    Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));

                    mid_pts[0].X = (pts[2].X + pts[12].X) / 2;
                    mid_pts[0].Y = (pts[2].Y + pts[12].Y) / 2;
                    g.DrawString(str, ft, brush, mid_pts[0].X, mid_pts[0].Y);
                }
            }
            catch (System.AccessViolationException)
            {
                MessageBox.Show("Gauger.on_paint() 函数出现内存访问异常。", "内存访问异常");
            }
        }
    }
}
