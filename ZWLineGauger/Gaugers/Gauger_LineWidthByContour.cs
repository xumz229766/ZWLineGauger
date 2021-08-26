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
    class Gauger_LineWidthByContour : Gauger
    {
        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool rotate_crd(double[] in_crds, double[] out_crds, double rotate_angle);

        [DllImport("pxflow.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool find_line(byte[] in_bytes, int[] in_data, double[] in_crds, int[] out_ints, double[] out_crds, bool bIsCalib);

        [DllImport("pxflow.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool find_line_by_contour_method(byte[] in_bytes, int[] in_data, double[] in_crds, int[] out_ints, double[] out_crds);

        [DllImport("pxflow.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool find_bulge_item(byte[] in_bytes, int[] in_data, double[] in_crds, int[] out_ints, double[] out_crds);

        Point2d[] m_rotated_rect_corners = new Point2d[4];
        
        double m_rotated_rect_wh_ratio = DEFAULT_ROTATED_RECT_WH_RATIO;

        public Gauger_LineWidthByContour(MainUI parent, PictureBox pb, MEASURE_TYPE type)
        {
            this.parent = parent;
            m_nPictureBoxWidth = pb.Width;
            m_nPictureBoxHeight = pb.Height;

            m_measure_type = type;
        }

        public override bool gauge(Image img, byte[] pImageBuf = null, int nWidth = 0, int nHeight = 0, int nBytesPerLine = 0,
            int nAlgorithm = 0, bool bLocateLineInRealTime = false)
        {
            int nStride = 0;
            byte[] pBuf;
            double ratio_x = 1;
            double ratio_y = 1;
            int[] out_ints = new int[100];
            int[] in_data = new int[10];

            if (true == bLocateLineInRealTime)
            {
                pBuf = pImageBuf;
                nStride = nBytesPerLine;

                ratio_x = (double)nWidth / (double)m_nPictureBoxWidth;
                ratio_y = (double)nHeight / (double)m_nPictureBoxHeight;
                in_data[0] = nWidth;
                in_data[1] = nHeight;
            }
            else
            {
                lock (parent.m_main_cam_lock)
                {
                    pBuf = GeneralUtils.convert_image_to_bytes(img, img.RawFormat, ref nStride);
                }

                ratio_x = (double)img.Width / (double)m_nPictureBoxWidth;
                ratio_y = (double)img.Height / (double)m_nPictureBoxHeight;
                in_data[0] = img.Width;
                in_data[1] = img.Height;
            }
            in_data[2] = pBuf.Length;
            in_data[3] = nStride;

            double[] in_crds = new double[10];
            double[] out_crds = new double[10000];

            lock (m_real_time_lock)
            {
                for (int n = 0; n < 4; n++)
                {
                    double x = Math.Round(m_rotated_rect_corners[n].x * ratio_x);
                    double y = Math.Round(m_rotated_rect_corners[n].y * ratio_y);

                    in_crds[n * 2] = x;
                    in_crds[n * 2 + 1] = y;

                    m_ROI_rect[n].x = x;
                    m_ROI_rect[n].y = y;

                    //Debugger.Log(0, null, string.Format("222222 [{0:0.0},{1:0.0}]", in_crds[n * 2], in_crds[n * 2 + 1]));
                }
            }
            
            m_list_contours.Clear();
            m_key_points.Clear();

            for (int t = 0; t < 100; t++)
                out_ints[t] = 0;

            find_line_by_contour_method(pBuf, in_data, in_crds, out_ints, out_crds);

            lock (m_real_time_lock)
            {
                if (1 == out_ints[0])
                {
                    m_bHasValidGaugeResult = true;

                    int nCounter = 0;
                    for (int t = 0; t < 10; t++)
                    {
                        int nContourLen = out_ints[t + 10];

                        if (nContourLen > 0)
                        {
                            Debugger.Log(0, null, string.Format("222222 nContourLen {0}", nContourLen));

                            List<Point2d> contour = new List<Point2d>();
                            for (int n = 0; n < nContourLen; n++)
                            {
                                Point2d pt = new Point2d(out_crds[20 + nCounter * 2 + n * 2], out_crds[20 + nCounter * 2 + n * 2 + 1]);

                                contour.Add(pt);
                            }

                            m_list_contours.Add(contour);

                            nCounter += nContourLen;
                        }
                    }

                    m_key_points.Add(new Point2d(out_crds[10], out_crds[11]));
                    m_key_points.Add(new Point2d(out_crds[12], out_crds[13]));
                    m_key_points.Add(new Point2d(out_crds[14], out_crds[15]));
                    m_key_points.Add(new Point2d(out_crds[16], out_crds[17]));

                    m_gauged_line_width = out_crds[0];
                    m_gauged_line_width2 = out_crds[1];

                    return true;
                }
                else
                {
                    //if (true == m_bRotatedRectIsReady)
                    set_failure_time();

                    return false;
                }
            }
        }

        public bool gauge(Image img, Point2d[] rect, double extension, int nAlgorithm, bool bHandMade = false)
        {
            int nStride = 0;
            byte[] pBuf;

            lock (parent.m_main_cam_lock)
            {
                pBuf = GeneralUtils.convert_image_to_bytes(img, img.RawFormat, ref nStride);
            }

            int[] out_ints = new int[100];
            int[] in_data = new int[10];
            in_data[0] = img.Width;
            in_data[1] = img.Height;
            in_data[2] = pBuf.Length;
            in_data[3] = nStride;

            double[] in_crds = new double[10];
            double[] out_crds = new double[10000];

            //for (int n = 0; n < 4; n++)
            //{
            //    rect[n].x = Math.Round(rect[n].x);
            //    rect[n].y = Math.Round(rect[n].y);
            //}

            if (true == bHandMade)
            {
                for (int n = 0; n < 4; n++)
                {
                    in_crds[n * 2] = rect[n].x;
                    in_crds[n * 2 + 1] = rect[n].y;

                    Debugger.Log(0, null, string.Format("222222 mmm [{0:0.0},{1:0.0}]", in_crds[n * 2], in_crds[n * 2 + 1]));
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

            m_list_contours.Clear();
            m_key_points.Clear();

            for (int t = 0; t < 100; t++)
                out_ints[t] = 0;

            find_line_by_contour_method(pBuf, in_data, in_crds, out_ints, out_crds);

            if (1 == out_ints[0])
            {
                m_bHasValidGaugeResult = true;

                int nCounter = 0;
                for (int t = 0; t < 10; t++)
                {
                    int nContourLen = out_ints[t + 10];

                    if (nContourLen > 0)
                    {
                        Debugger.Log(0, null, string.Format("222222 nContourLen {0}", nContourLen));

                        List<Point2d> contour = new List<Point2d>();
                        for (int n = 0; n < nContourLen; n++)
                        {
                            Point2d pt = new Point2d(out_crds[20 + nCounter * 2 + n * 2], out_crds[20 + nCounter * 2 + n * 2 + 1]);

                            contour.Add(pt);
                        }

                        m_list_contours.Add(contour);

                        nCounter += nContourLen;
                    }
                }

                m_key_points.Add(new Point2d(out_crds[10], out_crds[11]));
                m_key_points.Add(new Point2d(out_crds[12], out_crds[13]));
                m_key_points.Add(new Point2d(out_crds[14], out_crds[15]));
                m_key_points.Add(new Point2d(out_crds[16], out_crds[17]));

                m_gauged_line_width = out_crds[0];
                m_gauged_line_width2 = out_crds[1];

                return true;
            }
            else
            {
                //if (true == m_bRotatedRectIsReady)
                set_failure_time();

                return false;
            }
        }

        public void update_rotated_rect(bool bHorizonModeForGaugerRect)
        {
            Point2d[] pts = new Point2d[4];
            double ratio = m_rotated_rect_wh_ratio;

            if (true == bHorizonModeForGaugerRect)
            {
                //double center_x = (m_up_pt.x + m_down_pt.x) / 2;
                //double width = (Math.Abs(m_up_pt.x - m_down_pt.x) + Math.Abs(m_up_pt.y - m_down_pt.y)) / ratio;

                //m_rotated_rect_corners[0].set(center_x - width, m_down_pt.y);
                //m_rotated_rect_corners[1].set(center_x + width, m_down_pt.y);
                //m_rotated_rect_corners[2].set(center_x + width, m_up_pt.y);
                //m_rotated_rect_corners[3].set(center_x - width, m_up_pt.y);

                int left = (int)(Math.Min(m_down_pt.x, m_up_pt.x));
                int right = (int)(Math.Max(m_down_pt.x, m_up_pt.x));
                int top = (int)(Math.Min(m_down_pt.y, m_up_pt.y));
                int bottom = (int)(Math.Max(m_down_pt.y, m_up_pt.y));

                int width = (int)(right - left);

                if (((int)(width)) % 4 != 0)
                    width += (4 - ((int)(width)) % 4);

                m_rotated_rect_corners[0].set(left, top);
                m_rotated_rect_corners[1].set(right, top);
                m_rotated_rect_corners[2].set(right, bottom);
                m_rotated_rect_corners[3].set(left, bottom);
            }
            else
            {
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
                //if (true == m_bRotatedRectIsReady)
                //{
                //    if (true == gauge(pb.Image))
                //    {
                //        m_bShowLineOnly = true;
                //        pb.Refresh();
                //    }
                //}

                return;
            }
            // 右键鼠标松开
            else if (MouseButtons.Right == me.Button)
            {
                //Debugger.Log(0, null, string.Format("222222 111"));

                if (true == m_bRotatedRectIsReady)
                {
                    //lock (m_real_time_lock)
                    {
                        m_bRotatedRectIsReady = false;
                    }

                    gauge(pb.Image, null, 0, 0, 0, parent.m_nAlgorithm);
                }

                m_down_pt.set(0, 0);
                m_up_pt.set(0, 0);

                m_bShowLineOnly = false;

                pb.Refresh();
            }
        }

        public override void on_mouse_move(MouseEventArgs me, PictureBox pb, bool bHorizonModeForGaugerRect = true)
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

                update_rotated_rect(bHorizonModeForGaugerRect);

                //string msg = string.Format("222222 m_mv_down_pt = [{0},{1}], m_mv_up_pt = [{2},{3}]", m_mv_down_pt.x, m_mv_down_pt.y, m_mv_up_pt.x, m_mv_up_pt.y);
                //Debugger.Log(0, null, msg);

                pb.Refresh();
            }
        }

        public override void on_mouse_wheel(MouseEventArgs me, PictureBox pb, bool bHorizonModeForGaugerRect = true)
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

                update_rotated_rect(bHorizonModeForGaugerRect);

                pb.Refresh();
            }
        }

        public override void on_mouse_leave(EventArgs me, PictureBox pb)
        {
            m_bRotatedRectIsReady = false;

            m_nClickCounter = 0;

            m_down_pt.set(0, 0);
            m_up_pt.set(0, 0);

            pb.Refresh();
        }

        public override void on_paint(PaintEventArgs pe, PictureBox pb, Image source_image = null, bool bDrawToSourceImage = false)
        {
            Graphics g = pe.Graphics;

            if (true == bDrawToSourceImage)
            {
                g = Graphics.FromImage(source_image);
                
                if (true == m_bIsCopiedFromDifferentMeasureType)
                {
                    Gauger gauger = create_gauger(parent, pb, m_measure_type_of_clone_object);
                    
                    this.copy_measure_result_data(ref gauger);
                    
                    gauger.on_paint(pe, pb, source_image, bDrawToSourceImage);

                    return;
                }
            }

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
                    //arrow_pen.CustomEndCap = arrow_cap;

                    PointF down_pt = new PointF((float)m_down_pt.x, (float)m_down_pt.y);
                    PointF up_pt = new PointF((float)m_up_pt.x, (float)m_up_pt.y);
                    PointF[] arrow_pts = new PointF[4];
                    arrow_pts[0] = down_pt;
                    arrow_pts[2] = up_pt;
                    if (true == m_bHorizonModeForGaugerRect)
                    {
                        arrow_pts[1].X = down_pt.X;
                        arrow_pts[3].X = up_pt.X;
                    }
                    else
                    {
                        arrow_pts[1].X = down_pt.X + (up_pt.X - down_pt.X) / 7;
                        arrow_pts[3].X = up_pt.X + (down_pt.X - up_pt.X) / 7;
                    }
                    arrow_pts[1].Y = down_pt.Y + (up_pt.Y - down_pt.Y) / 7;
                    arrow_pts[3].Y = up_pt.Y + (down_pt.Y - up_pt.Y) / 7;
                    g.DrawLine(arrow_pen, arrow_pts[0], arrow_pts[1]);
                    g.DrawLine(arrow_pen, arrow_pts[2], arrow_pts[3]);
                }

                // 显示抓边结果
                #region
                if (true == m_bHasValidGaugeResult)
                {
                    float ratio_x = (float)pb.Image.Width / (float)m_nPictureBoxWidth;
                    float ratio_y = (float)pb.Image.Height / (float)m_nPictureBoxHeight;

                    if (true == bDrawToSourceImage)
                    {
                        ratio_x = 1;
                        ratio_y = 1;
                    }

                    Color violet = Color.FromArgb(255, 0, 255);

                    for (int n = 0; n < m_list_contours.Count; n++)
                    {
                        for (int k = 0; k < m_list_contours[n].Count; k++)
                        {
                            //if (n < 2)
                            //    continue;
                            PointF pt = new PointF((float)m_list_contours[n][k].x / ratio_x, (float)m_list_contours[n][k].y / ratio_y);

                            g.FillEllipse(new SolidBrush(Color.FromArgb(0, 255, 0)), pt.X - 2, pt.Y - 1, 2, 2);
                            //Debugger.Log(0, null, string.Format("222222 nk [{0},{1}]: [{2:0.000},{3:0.000}]", n, k, pt.X, pt.Y));
                        }
                    }

                    if (4 == m_key_points.Count)
                    {
                        PointF pt1 = new PointF((float)m_key_points[0].x / ratio_x, (float)m_key_points[0].y / ratio_y);
                        PointF pt2 = new PointF((float)m_key_points[1].x / ratio_x, (float)m_key_points[1].y / ratio_y);
                        PointF pt3 = new PointF((float)m_key_points[2].x / ratio_x, (float)m_key_points[2].y / ratio_y);
                        PointF pt4 = new PointF((float)m_key_points[3].x / ratio_x, (float)m_key_points[3].y / ratio_y);

                        //g.DrawLine(new Pen(Color.FromArgb(255, 0, 0), 2), pt1, pt2);
                        g.DrawLine(new Pen(Color.FromArgb(255, 0, 0), 2), pt3, pt4);

                        for (int k = 2; k < m_key_points.Count; k++)
                        {
                            PointF pt = new PointF((float)m_key_points[k].x / ratio_x, (float)m_key_points[k].y / ratio_y);

                            g.DrawEllipse(new Pen(Color.FromArgb(255, 0, 0), 2), pt.X - 3, pt.Y - 3, 6, 6);
                        }

                        double value = m_gauged_line_width / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                        value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                        String str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                            m_strUnits[parent.m_nUnitType]);

                        PointF[] mid_pts = new PointF[6];
                        mid_pts[2].X = (pt3.X + pt4.X) / 2;
                        mid_pts[2].Y = (pt3.Y + pt4.Y) / 2;
                        //mid_pts[2].Y += Math.Abs(pt3.Y - pt4.Y) / 2;

                        //Font ft = new Font("宋体", 25, FontStyle.Bold);
                        //Brush brush = new SolidBrush(violet);
                       
                        //g.DrawString(str, ft, brush, mid_pts[2].X, mid_pts[2].Y);
                        set_Font(g, str, pb, mid_pts[2], bDrawToSourceImage);//设置字体颜色位置等

                        //value = m_gauged_line_width2 / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                        //value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                        //str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                        //    m_strUnits[parent.m_nUnitType]);

                        //mid_pts[2].X = (pt3.X + pt4.X) / 2;
                        //mid_pts[2].Y = (pt3.Y + pt4.Y) / 2;
                        //mid_pts[2].Y -= Math.Abs(pt3.Y - pt4.Y);
                        //g.DrawString(str, ft, brush, mid_pts[2].X, mid_pts[2].Y);
                    }
                }
                #endregion
            }
            catch (System.AccessViolationException)
            {
                MessageBox.Show("Gauger.on_paint() 函数出现内存访问异常。", "内存访问异常");
            }
        }

        public override void show_selection_frame(PaintEventArgs pe, PictureBox pb, Image source_image = null, bool bDrawToSourceImage = false)
        {

        }

        public override void copy_measure_result_data(ref Gauger dest)
        {
            if (this.m_measure_type == dest.m_measure_type)
            {
                for (int n = 0; n < m_gauge_result_pts.Length; n++)
                    ((Gauger_LineWidthByContour)dest).m_gauge_result_pts[n] = m_gauge_result_pts[n];

                ((Gauger_LineWidthByContour)dest).m_bHasValidGaugeResult = m_bHasValidGaugeResult;
                ((Gauger_LineWidthByContour)dest).m_gauged_line_width = m_gauged_line_width;
                ((Gauger_LineWidthByContour)dest).m_gauged_line_width2 = m_gauged_line_width2;

                ((Gauger_LineWidthByContour)dest).m_list_contours.Clear();
                ((Gauger_LineWidthByContour)dest).m_key_points.Clear();

                for (int n = 0; n < m_list_contours.Count; n++)
                {
                    List<Point2d> contour = new List<Point2d>();

                    for (int k = 0; k < m_list_contours[n].Count; k++)
                    {
                        contour.Add(m_list_contours[n][k]);
                    }

                    ((Gauger_LineWidthByContour)dest).m_list_contours.Add(contour);
                }

                for (int n = 0; n < m_key_points.Count; n++)
                    ((Gauger_LineWidthByContour)dest).m_key_points.Add(m_key_points[n]);
                dest.m_string_offset_real = m_string_offset_real;
            }
            else
            {
                dest.m_bIsCopiedFromDifferentMeasureType = true;
                dest.m_measure_type_of_clone_object = m_measure_type;

                for (int n = 0; n < m_click_pts.Length; n++)
                    dest.m_click_pts[n] = m_click_pts[n];

                for (int n = 0; n < m_gauge_result_pts.Length; n++)
                    dest.m_gauge_result_pts[n] = m_gauge_result_pts[n];

                dest.m_bHasValidGaugeResult = m_bHasValidGaugeResult;
                dest.m_gauged_line_width = m_gauged_line_width;
                dest.m_gauged_line_width2 = m_gauged_line_width2;

                dest.m_list_contours.Clear();
                dest.m_key_points.Clear();

                for (int n = 0; n < m_list_contours.Count; n++)
                {
                    List<Point2d> contour = new List<Point2d>();

                    for (int k = 0; k < m_list_contours[n].Count; k++)
                    {
                        contour.Add(m_list_contours[n][k]);
                    }

                    dest.m_list_contours.Add(contour);
                }

                for (int n = 0; n < m_key_points.Count; n++)
                    dest.m_key_points.Add(m_key_points[n]);

                for (int n = 0; n < 4; n++)
                    dest.m_ROI_rect[n] = m_ROI_rect[n];
                dest.m_string_offset_real = m_string_offset_real;
            }
        }
    }
}
