using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Diagnostics;

namespace ZWLineGauger.Gaugers
{
    class Gauger_LineWidth : Gauger
    {
        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool rotate_crd(double[] in_crds, double[] out_crds, double rotate_angle);

        [DllImport("pxflow.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool find_line(byte[] in_bytes, int[] in_data, double[] in_crds, int[] out_ints, double[] out_crds, bool bIsCalib);

        [DllImport("pxflow.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool find_line_by_contour(byte[] in_bytes, int[] in_data, double[] in_crds, int[] out_ints, double[] out_crds, int nAlgorithm);

        Point2d[]   m_rotated_rect_corners = new Point2d[4];
        Point2d[]   m_gauge_result_pts = new Point2d[8];

        double m_rotated_rect_wh_ratio = DEFAULT_ROTATED_RECT_WH_RATIO;

        public Gauger_LineWidth(MainUI parent, PictureBox pb, MEASURE_TYPE type)
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
            int[] out_ints = new int[10];
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
            double[] out_crds = new double[20];

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

            for (int n = 0; n < 4; n++)
            {
                //Debugger.Log(0, null, string.Format("222222 [{0:0.0},{1:0.0}]", in_crds[n * 2], in_crds[n * 2 + 1]));
            }
            
            out_ints[0] = 0;
            if (true == GeneralUtils.check_if_form_is_open("Form_Calibration"))
                find_line(pBuf, in_data, in_crds, out_ints, out_crds, true);
            else
            {
                if (0 == parent.m_nLineMeasurementMethod)
                    find_line(pBuf, in_data, in_crds, out_ints, out_crds, false);
                else if (1 == parent.m_nLineMeasurementMethod)
                    find_line_by_contour(pBuf, in_data, in_crds, out_ints, out_crds, nAlgorithm);
            }

            lock (m_real_time_lock)
            {
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
                    m_gauge_result_pts[6].x = out_crds[12];
                    m_gauge_result_pts[6].y = out_crds[13];
                    m_gauge_result_pts[7].x = out_crds[14];
                    m_gauge_result_pts[7].y = out_crds[15];

                    if ((true == GeneralUtils.check_if_form_is_open("Form_Calibration")) || (0 == parent.m_nLineMeasurementMethod))
                    {
                        switch (m_measure_type)
                        {
                            case MEASURE_TYPE.LINE_WIDTH_14:
                                m_gauged_line_width = GeneralUtils.get_distance(m_gauge_result_pts[0], m_gauge_result_pts[6]);
                                break;
                            case MEASURE_TYPE.LINE_WIDTH_23:
                                m_gauged_line_width = GeneralUtils.get_distance(m_gauge_result_pts[2], m_gauge_result_pts[4]);
                                break;
                            case MEASURE_TYPE.LINE_WIDTH_13:
                                m_gauged_line_width = GeneralUtils.get_distance(m_gauge_result_pts[0], m_gauge_result_pts[4]);
                                break;
                            case MEASURE_TYPE.LINE_WIDTH_1234:
                                m_gauged_line_width = GeneralUtils.get_distance(m_gauge_result_pts[0], m_gauge_result_pts[6]);
                                m_gauged_line_width2 = GeneralUtils.get_distance(m_gauge_result_pts[2], m_gauge_result_pts[4]);
                                break;
                        }
                    }
                    else if (1 == parent.m_nLineMeasurementMethod)
                    {
                        Point2d mid1, mid2;
                        switch (m_measure_type)
                        {
                            case MEASURE_TYPE.LINE_WIDTH_14:
                                mid1.x = (m_gauge_result_pts[0].x + m_gauge_result_pts[1].x) / 2;
                                mid1.y = (m_gauge_result_pts[0].y + m_gauge_result_pts[1].y) / 2;
                                mid2.x = (m_gauge_result_pts[6].x + m_gauge_result_pts[7].x) / 2;
                                mid2.y = (m_gauge_result_pts[6].y + m_gauge_result_pts[7].y) / 2;
                                m_gauged_line_width = GeneralUtils.get_distance(mid1, mid2);
                                break;

                            case MEASURE_TYPE.LINE_WIDTH_23:
                                mid1.x = (m_gauge_result_pts[2].x + m_gauge_result_pts[3].x) / 2;
                                mid1.y = (m_gauge_result_pts[2].y + m_gauge_result_pts[3].y) / 2;
                                mid2.x = (m_gauge_result_pts[4].x + m_gauge_result_pts[5].x) / 2;
                                mid2.y = (m_gauge_result_pts[4].y + m_gauge_result_pts[5].y) / 2;
                                m_gauged_line_width = GeneralUtils.get_distance(mid1, mid2);
                                break;

                            case MEASURE_TYPE.LINE_WIDTH_13:
                                mid1.x = (m_gauge_result_pts[0].x + m_gauge_result_pts[1].x) / 2;
                                mid1.y = (m_gauge_result_pts[0].y + m_gauge_result_pts[1].y) / 2;
                                mid2.x = (m_gauge_result_pts[4].x + m_gauge_result_pts[5].x) / 2;
                                mid2.y = (m_gauge_result_pts[4].y + m_gauge_result_pts[5].y) / 2;
                                m_gauged_line_width = GeneralUtils.get_distance(mid1, mid2);
                                break;

                            case MEASURE_TYPE.LINE_WIDTH_1234:
                                mid1.x = (m_gauge_result_pts[0].x + m_gauge_result_pts[1].x) / 2;
                                mid1.y = (m_gauge_result_pts[0].y + m_gauge_result_pts[1].y) / 2;
                                mid2.x = (m_gauge_result_pts[6].x + m_gauge_result_pts[7].x) / 2;
                                mid2.y = (m_gauge_result_pts[6].y + m_gauge_result_pts[7].y) / 2;
                                m_gauged_line_width = GeneralUtils.get_distance(mid1, mid2);

                                mid1.x = (m_gauge_result_pts[2].x + m_gauge_result_pts[3].x) / 2;
                                mid1.y = (m_gauge_result_pts[2].y + m_gauge_result_pts[3].y) / 2;
                                mid2.x = (m_gauge_result_pts[4].x + m_gauge_result_pts[5].x) / 2;
                                mid2.y = (m_gauge_result_pts[4].y + m_gauge_result_pts[5].y) / 2;
                                m_gauged_line_width2 = GeneralUtils.get_distance(mid1, mid2);
                                break;
                        }
                    }

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
            
            int[] out_ints = new int[10];
            int[] in_data = new int[10];
            in_data[0] = img.Width;
            in_data[1] = img.Height;
            in_data[2] = pBuf.Length;
            in_data[3] = nStride;

            double[] in_crds = new double[10];
            double[] out_crds = new double[20];

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

            out_ints[0] = 0;
            if (true == GeneralUtils.check_if_form_is_open("Form_Calibration"))
                find_line(pBuf, in_data, in_crds, out_ints, out_crds, true);
            else
            {
                if (0 == parent.m_nLineMeasurementMethod)
                    find_line(pBuf, in_data, in_crds, out_ints, out_crds, false);
                else if (1 == parent.m_nLineMeasurementMethod)
                    find_line_by_contour(pBuf, in_data, in_crds, out_ints, out_crds, nAlgorithm);
            }

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
                m_gauge_result_pts[6].x = out_crds[12];
                m_gauge_result_pts[6].y = out_crds[13];
                m_gauge_result_pts[7].x = out_crds[14];
                m_gauge_result_pts[7].y = out_crds[15];

                if ((true == GeneralUtils.check_if_form_is_open("Form_Calibration")) || (0 == parent.m_nLineMeasurementMethod))
                {
                    switch (m_measure_type)
                    {
                        case MEASURE_TYPE.LINE_WIDTH_14:
                            m_gauged_line_width = GeneralUtils.get_distance(m_gauge_result_pts[0], m_gauge_result_pts[6]);
                            break;
                        case MEASURE_TYPE.LINE_WIDTH_23:
                            m_gauged_line_width = GeneralUtils.get_distance(m_gauge_result_pts[2], m_gauge_result_pts[4]);
                            break;
                        case MEASURE_TYPE.LINE_WIDTH_13:
                            m_gauged_line_width = GeneralUtils.get_distance(m_gauge_result_pts[0], m_gauge_result_pts[4]);
                            break;
                        case MEASURE_TYPE.LINE_WIDTH_1234:
                            m_gauged_line_width = GeneralUtils.get_distance(m_gauge_result_pts[0], m_gauge_result_pts[6]);
                            m_gauged_line_width2 = GeneralUtils.get_distance(m_gauge_result_pts[2], m_gauge_result_pts[4]);
                            break;
                    }
                }
                else if (1 == parent.m_nLineMeasurementMethod)
                {
                    Point2d mid1, mid2;
                    switch (m_measure_type)
                    {
                        case MEASURE_TYPE.LINE_WIDTH_14:
                            mid1.x = (m_gauge_result_pts[0].x + m_gauge_result_pts[1].x) / 2;
                            mid1.y = (m_gauge_result_pts[0].y + m_gauge_result_pts[1].y) / 2;
                            mid2.x = (m_gauge_result_pts[6].x + m_gauge_result_pts[7].x) / 2;
                            mid2.y = (m_gauge_result_pts[6].y + m_gauge_result_pts[7].y) / 2;
                            m_gauged_line_width = GeneralUtils.get_distance(mid1, mid2);
                            break;

                        case MEASURE_TYPE.LINE_WIDTH_23:
                            mid1.x = (m_gauge_result_pts[2].x + m_gauge_result_pts[3].x) / 2;
                            mid1.y = (m_gauge_result_pts[2].y + m_gauge_result_pts[3].y) / 2;
                            mid2.x = (m_gauge_result_pts[4].x + m_gauge_result_pts[5].x) / 2;
                            mid2.y = (m_gauge_result_pts[4].y + m_gauge_result_pts[5].y) / 2;
                            m_gauged_line_width = GeneralUtils.get_distance(mid1, mid2);
                            break;

                        case MEASURE_TYPE.LINE_WIDTH_13:
                            mid1.x = (m_gauge_result_pts[0].x + m_gauge_result_pts[1].x) / 2;
                            mid1.y = (m_gauge_result_pts[0].y + m_gauge_result_pts[1].y) / 2;
                            mid2.x = (m_gauge_result_pts[4].x + m_gauge_result_pts[5].x) / 2;
                            mid2.y = (m_gauge_result_pts[4].y + m_gauge_result_pts[5].y) / 2;
                            m_gauged_line_width = GeneralUtils.get_distance(mid1, mid2);
                            break;

                        case MEASURE_TYPE.LINE_WIDTH_1234:
                            mid1.x = (m_gauge_result_pts[0].x + m_gauge_result_pts[1].x) / 2;
                            mid1.y = (m_gauge_result_pts[0].y + m_gauge_result_pts[1].y) / 2;
                            mid2.x = (m_gauge_result_pts[6].x + m_gauge_result_pts[7].x) / 2;
                            mid2.y = (m_gauge_result_pts[6].y + m_gauge_result_pts[7].y) / 2;
                            m_gauged_line_width = GeneralUtils.get_distance(mid1, mid2);

                            mid1.x = (m_gauge_result_pts[2].x + m_gauge_result_pts[3].x) / 2;
                            mid1.y = (m_gauge_result_pts[2].y + m_gauge_result_pts[3].y) / 2;
                            mid2.x = (m_gauge_result_pts[4].x + m_gauge_result_pts[5].x) / 2;
                            mid2.y = (m_gauge_result_pts[4].y + m_gauge_result_pts[5].y) / 2;
                            m_gauged_line_width2 = GeneralUtils.get_distance(mid1, mid2);
                            break;
                    }
                }

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

            //if (true == bHorizonModeForGaugerRect)
            //{
            //    int left = (int)(Math.Min(m_down_pt.x, m_up_pt.x));
            //    int right = (int)(Math.Max(m_down_pt.x, m_up_pt.x));
            //    int top = (int)(Math.Min(m_down_pt.y, m_up_pt.y));
            //    int bottom = (int)(Math.Max(m_down_pt.y, m_up_pt.y));

            //    int width = (int)(right - left);

            //    if (((int)(width)) % 4 != 0)
            //        width += (4 - ((int)(width)) % 4);

            //    m_rotated_rect_corners[0].set(left, top);
            //    m_rotated_rect_corners[1].set(right, top);
            //    m_rotated_rect_corners[2].set(right, bottom);
            //    m_rotated_rect_corners[3].set(left, bottom);
            //}
            //else
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
                
                pb.Refresh();
            }
        }

        public override void on_mouse_wheel(MouseEventArgs me, PictureBox pb, bool bHorizonModeForGaugerRect = true)
        {
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

                m_nClickCounter = 2;
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
                    double ratio_x = (double)pb.Image.Width / (double)m_nPictureBoxWidth;
                    double ratio_y = (double)pb.Image.Height / (double)m_nPictureBoxHeight;

                    if (true == bDrawToSourceImage)
                    {
                        ratio_x = 1;
                        ratio_y = 1;
                    }

                    Pen red_pen = new Pen(Color.FromArgb(255, 0, 0), 1);
                    Pen green_pen = new Pen(Color.FromArgb(0, 255, 0), 1);
                    Pen violet_pen = new Pen(Color.FromArgb(255, 0, 255), 1);
                    Color violet = Color.FromArgb(255, 0, 255);
                    Point2d center = new Point2d(0, 0);
                    PointF[] pts = new PointF[8];
                    PointF[] mid_pts = new PointF[6];

                    switch (m_measure_type)
                    {
                        case MEASURE_TYPE.LINE_WIDTH_14:
                            for (int n = 0; n < 2; n++)
                            {
                                pts[n].X = (float)((m_gauge_result_pts[n].x - 1) / ratio_x);
                                pts[n].Y = (float)((m_gauge_result_pts[n].y - 1) / ratio_y);

                                center.x += m_gauge_result_pts[n].x;
                                center.y += m_gauge_result_pts[n].y;
                            }
                            for (int n = 6; n < 8; n++)
                            {
                                pts[n - 4].X = (float)((m_gauge_result_pts[n].x - 1) / ratio_x);
                                pts[n - 4].Y = (float)((m_gauge_result_pts[n].y - 1) / ratio_y);

                                center.x += m_gauge_result_pts[n].x;
                                center.y += m_gauge_result_pts[n].y;
                            }
                            break;

                        case MEASURE_TYPE.LINE_WIDTH_23:
                            for (int n = 2; n < 4; n++)
                            {
                                pts[n - 2].X = (float)((m_gauge_result_pts[n].x - 1) / ratio_x);
                                pts[n - 2].Y = (float)((m_gauge_result_pts[n].y - 1) / ratio_y);

                                center.x += m_gauge_result_pts[n].x;
                                center.y += m_gauge_result_pts[n].y;
                            }
                            for (int n = 4; n < 6; n++)
                            {
                                pts[n - 2].X = (float)((m_gauge_result_pts[n].x - 1) / ratio_x);
                                pts[n - 2].Y = (float)((m_gauge_result_pts[n].y - 1) / ratio_y);

                                center.x += m_gauge_result_pts[n].x;
                                center.y += m_gauge_result_pts[n].y;
                            }
                            break;

                        case MEASURE_TYPE.LINE_WIDTH_13:
                            for (int n = 0; n < 2; n++)
                            {
                                pts[n].X = (float)((m_gauge_result_pts[n].x - 1) / ratio_x);
                                pts[n].Y = (float)((m_gauge_result_pts[n].y - 1) / ratio_y);

                                center.x += m_gauge_result_pts[n].x;
                                center.y += m_gauge_result_pts[n].y;
                            }
                            for (int n = 4; n < 6; n++)
                            {
                                pts[n - 2].X = (float)((m_gauge_result_pts[n].x - 1) / ratio_x);
                                pts[n - 2].Y = (float)((m_gauge_result_pts[n].y - 1) / ratio_y);

                                center.x += m_gauge_result_pts[n].x;
                                center.y += m_gauge_result_pts[n].y;
                            }
                            break;

                        case MEASURE_TYPE.LINE_WIDTH_1234:
                            for (int n = 0; n < 8; n++)
                            {
                                pts[n].X = (float)((m_gauge_result_pts[n].x - 1) / ratio_x);
                                pts[n].Y = (float)((m_gauge_result_pts[n].y - 1) / ratio_y);

                                center.x += m_gauge_result_pts[n].x;
                                center.y += m_gauge_result_pts[n].y;
                            }
                            break;
                    }

                    if (MEASURE_TYPE.LINE_WIDTH_1234 == m_measure_type)
                    {
                        center.x /= 8;
                        center.y /= 8;
                    }
                    else
                    {
                        center.x /= 4;
                        center.y /= 4;
                    }
                    
                    m_object_center = center;

                    //Debugger.Log(0, null, string.Format("222222 m_object_center [{0:0.000},{1:0.000}]", m_object_center.x, m_object_center.y));

                    // 检查是否处于标定状态
                    Form form = null;
                    if ((false == parent.m_bIsCreatingTask) && (false == parent.m_bIsWaitingForConfirm)
                        && (true == GeneralUtils.check_if_form_is_open("Form_Calibration", ref form)))
                    {
                        g.DrawLine(green_pen, pts[0], pts[1]);
                        g.DrawLine(green_pen, pts[2], pts[3]);

                        mid_pts[0].X = (pts[0].X + pts[1].X) / 2;
                        mid_pts[0].Y = (pts[0].Y + pts[1].Y) / 2;
                        mid_pts[1].X = (pts[2].X + pts[3].X) / 2;
                        mid_pts[1].Y = (pts[2].Y + pts[3].Y) / 2;
                        mid_pts[2].X = (mid_pts[0].X + mid_pts[1].X) / 2;
                        mid_pts[2].Y = (mid_pts[0].Y + mid_pts[1].Y) / 2;

                        AdjustableArrowCap arrow_cap = new AdjustableArrowCap(6, 6, true);
                        Pen arrow_pen = new Pen(Color.FromArgb(0, 255, 0), 1);
                        arrow_pen.CustomEndCap = arrow_cap;

                        if (false == m_bShowLineOnly)
                        {
                            g.DrawLine(arrow_pen, mid_pts[2], mid_pts[0]);
                            g.DrawLine(arrow_pen, mid_pts[2], mid_pts[1]);

                            String str = string.Format("{0:0.000}px", m_gauged_line_width);

                            //Font ft = new Font("宋体", 25, FontStyle.Bold);

                            ////Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));

                            //Brush brush = new SolidBrush(violet);
                        
                            //g.DrawString(str, ft, brush, mid_pts[2].X, mid_pts[2].Y);
                            set_Font(g, str, pb, mid_pts[2], bDrawToSourceImage);//设置字体颜色位置等
                        }
                    }
                    else
                    {
                        if ((false == GeneralUtils.check_if_form_is_open("Form_Calibration")) && (1 == parent.m_nLineMeasurementMethod))
                        {
                            if (MEASURE_TYPE.LINE_WIDTH_1234 == m_measure_type)
                            {
                                #region
                                for (int n = 0; n < 4; n++)
                                    g.DrawLine(green_pen, pts[n * 2], pts[n * 2 + 1]);

                                mid_pts[0].X = pts[0].X + (pts[1].X - pts[0].X) / 3;
                                mid_pts[0].Y = pts[0].Y + (pts[1].Y - pts[0].Y) / 3;
                                mid_pts[1].X = pts[6].X + (pts[7].X - pts[6].X) / 3;
                                mid_pts[1].Y = pts[6].Y + (pts[7].Y - pts[6].Y) / 3;
                                mid_pts[2].X = mid_pts[0].X + (mid_pts[1].X - mid_pts[0].X) * 2 / 3;
                                mid_pts[2].Y = mid_pts[0].Y + (mid_pts[1].Y - mid_pts[0].Y) * 2 / 3;

                                mid_pts[3].X = pts[2].X + (pts[3].X - pts[2].X) * 2 / 3;
                                mid_pts[3].Y = pts[2].Y + (pts[3].Y - pts[2].Y) * 2 / 3;
                                mid_pts[4].X = pts[4].X + (pts[5].X - pts[4].X) * 2 / 3;
                                mid_pts[4].Y = pts[4].Y + (pts[5].Y - pts[4].Y) * 2 / 3;
                                mid_pts[5].X = mid_pts[3].X + (mid_pts[4].X - mid_pts[3].X) / 3;
                                mid_pts[5].Y = mid_pts[3].Y + (mid_pts[4].Y - mid_pts[3].Y) / 3;

                                AdjustableArrowCap arrow_cap = new AdjustableArrowCap(6, 6, true);
                                Pen arrow_pen = new Pen(Color.FromArgb(0, 255, 0), 1);
                                arrow_pen.CustomEndCap = arrow_cap;

                                if (false == m_bShowLineOnly)
                                {
                                    g.DrawLine(arrow_pen, mid_pts[2], mid_pts[0]);
                                    g.DrawLine(arrow_pen, mid_pts[2], mid_pts[1]);

                                    g.DrawLine(arrow_pen, mid_pts[5], mid_pts[3]);
                                    g.DrawLine(arrow_pen, mid_pts[5], mid_pts[4]);

                                    double value = m_gauged_line_width / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                                    String str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                        m_strUnits[parent.m_nUnitType]);

                                    Font ft = new Font("宋体", 20, FontStyle.Bold);
                                    //Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));
                                    Brush brush = new SolidBrush(violet);
                                    g.DrawString(str, ft, brush, mid_pts[2].X, mid_pts[2].Y);

                                    value = m_gauged_line_width2 / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                                    str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                        m_strUnits[parent.m_nUnitType]);

                                    //brush = new SolidBrush(Color.FromArgb(255, 0, 0));
                                    brush = new SolidBrush(violet);
                                    g.DrawString(str, ft, brush, mid_pts[5].X, mid_pts[5].Y);

                                    double value1 = 0;
                                    double value2 = 0;
                                    double value3 = 0;
                                    double value4 = 0;
                                    double min_dist1 = 0;
                                    double max_dist1 = 0;
                                    double min_dist2 = 0;
                                    double max_dist2 = 0;

                                    switch (m_measure_type)
                                    {
                                        case MEASURE_TYPE.LINE_WIDTH_1234:
                                            value1 = GeneralUtils.get_distance(m_gauge_result_pts[0], m_gauge_result_pts[6]);
                                            value2 = GeneralUtils.get_distance(m_gauge_result_pts[1], m_gauge_result_pts[7]);
                                            value3 = GeneralUtils.get_distance(m_gauge_result_pts[2], m_gauge_result_pts[4]);
                                            value4 = GeneralUtils.get_distance(m_gauge_result_pts[3], m_gauge_result_pts[5]);
                                            break;
                                    }
                                    if (value1 > value2)
                                    {
                                        min_dist1 = value2;
                                        max_dist1 = value1;
                                    }
                                    else
                                    {
                                        min_dist1 = value1;
                                        max_dist1 = value2;
                                    }
                                    if (value3 > value4)
                                    {
                                        min_dist2 = value4;
                                        max_dist2 = value3;
                                    }
                                    else
                                    {
                                        min_dist2 = value3;
                                        max_dist2 = value4;
                                    }

                                    min_dist1 /= parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    max_dist1 /= parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    min_dist1 = GeneralUtils.convert_um_value_by_unit(min_dist1, parent.m_nUnitType);
                                    max_dist1 = GeneralUtils.convert_um_value_by_unit(max_dist1, parent.m_nUnitType);
                                    min_dist2 /= parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    max_dist2 /= parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    min_dist2 = GeneralUtils.convert_um_value_by_unit(min_dist2, parent.m_nUnitType);
                                    max_dist2 = GeneralUtils.convert_um_value_by_unit(max_dist2, parent.m_nUnitType);

                                    if (false == m_bIsClonedObject)
                                    {
                                        ft = new Font("宋体", 13, FontStyle.Bold);
                                        str = string.Format("下线宽最小值: {0}{1}", GeneralUtils.convert_number_to_string_with_digits(min_dist1, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                            m_strUnits[parent.m_nUnitType]);
                                        g.DrawString(str, ft, brush, m_nPictureBoxWidth - 220, 3);

                                        str = string.Format("下线宽最大值: {0}{1}", GeneralUtils.convert_number_to_string_with_digits(max_dist1, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                            m_strUnits[parent.m_nUnitType]);
                                        g.DrawString(str, ft, brush, m_nPictureBoxWidth - 220, 32);

                                        str = string.Format("上线宽最小值: {0}{1}", GeneralUtils.convert_number_to_string_with_digits(min_dist2, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                            m_strUnits[parent.m_nUnitType]);
                                        g.DrawString(str, ft, brush, m_nPictureBoxWidth - 220, 62);

                                        str = string.Format("上线宽最大值: {0}{1}", GeneralUtils.convert_number_to_string_with_digits(max_dist2, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                            m_strUnits[parent.m_nUnitType]);
                                        g.DrawString(str, ft, brush, m_nPictureBoxWidth - 220, 92);
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                g.DrawLine(green_pen, pts[0], pts[1]);
                                g.DrawLine(green_pen, pts[2], pts[3]);

                                mid_pts[0].X = (pts[0].X + pts[1].X) / 2;
                                mid_pts[0].Y = (pts[0].Y + pts[1].Y) / 2;
                                mid_pts[1].X = (pts[2].X + pts[3].X) / 2;
                                mid_pts[1].Y = (pts[2].Y + pts[3].Y) / 2;
                                mid_pts[2].X = (mid_pts[0].X + mid_pts[1].X) / 2;
                                mid_pts[2].Y = (mid_pts[0].Y + mid_pts[1].Y) / 2;

                                AdjustableArrowCap arrow_cap = new AdjustableArrowCap(6, 6, true);
                                Pen arrow_pen = new Pen(Color.FromArgb(0, 255, 0), 1);
                                arrow_pen.CustomEndCap = arrow_cap;

                                if (false == m_bShowLineOnly)
                                {
                                    g.DrawLine(arrow_pen, mid_pts[2], mid_pts[0]);
                                    g.DrawLine(arrow_pen, mid_pts[2], mid_pts[1]);

                                    double value = m_gauged_line_width / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                                    String str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                        m_strUnits[parent.m_nUnitType]);

                                    //Font ft = new Font("宋体", 25, FontStyle.Bold);

                                    //Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));
                                    //Brush brush = new SolidBrush(violet);
                                    Font ft = new Font(globaldata.m_fontname, (float)globaldata.m_fontsize, FontStyle.Bold);
                                    Color fontcolor = ColorTranslator.FromHtml("#" + globaldata.m_fontcolor);
                                    Brush brush = new SolidBrush(fontcolor);
                                    g.DrawString(str, ft, brush, mid_pts[2].X, mid_pts[2].Y);

                                    double value1 = 0;
                                    double value2 = 0;
                                    double min_dist = 0;
                                    double max_dist = 0;

                                    switch (m_measure_type)
                                    {
                                        case MEASURE_TYPE.LINE_WIDTH_14:
                                            value1 = GeneralUtils.get_distance(m_gauge_result_pts[0], m_gauge_result_pts[6]);
                                            value2 = GeneralUtils.get_distance(m_gauge_result_pts[1], m_gauge_result_pts[7]);
                                            break;
                                        case MEASURE_TYPE.LINE_WIDTH_23:
                                            value1 = GeneralUtils.get_distance(m_gauge_result_pts[2], m_gauge_result_pts[4]);
                                            value2 = GeneralUtils.get_distance(m_gauge_result_pts[3], m_gauge_result_pts[5]);
                                            break;
                                        case MEASURE_TYPE.LINE_WIDTH_13:
                                            value1 = GeneralUtils.get_distance(m_gauge_result_pts[0], m_gauge_result_pts[4]);
                                            value2 = GeneralUtils.get_distance(m_gauge_result_pts[1], m_gauge_result_pts[5]);
                                            break;
                                    }
                                    if (value1 > value2)
                                    {
                                        min_dist = value2;
                                        max_dist = value1;
                                    }
                                    else
                                    {
                                        min_dist = value1;
                                        max_dist = value2;
                                    }
                                    min_dist /= parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    max_dist /= parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    min_dist = GeneralUtils.convert_um_value_by_unit(min_dist, parent.m_nUnitType);
                                    max_dist = GeneralUtils.convert_um_value_by_unit(max_dist, parent.m_nUnitType);

                                    //ft = new Font("宋体", 13, FontStyle.Bold);
                                    //str = string.Format("最小值: {0}{1}", GeneralUtils.convert_number_to_string_with_digits(min_dist, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                    //    m_strUnits[parent.m_nUnitType]);
                                    //g.DrawString(str, ft, brush, m_nPictureBoxWidth - 180, 3);

                                    //str = string.Format("最大值: {0}{1}", GeneralUtils.convert_number_to_string_with_digits(max_dist, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                    //    m_strUnits[parent.m_nUnitType]);
                                    //g.DrawString(str, ft, brush, m_nPictureBoxWidth - 180, 42);
                                }
                            }
                        }
                        else
                        {
                            if (MEASURE_TYPE.LINE_WIDTH_1234 == m_measure_type)
                            {
                                #region
                                for (int n = 0; n < 4; n++)
                                    g.DrawLine(green_pen, pts[n * 2], pts[n * 2 + 1]);

                                mid_pts[0].X = pts[0].X + (pts[1].X - pts[0].X) / 3;
                                mid_pts[0].Y = pts[0].Y + (pts[1].Y - pts[0].Y) / 3;
                                mid_pts[1].X = pts[6].X + (pts[7].X - pts[6].X) / 3;
                                mid_pts[1].Y = pts[6].Y + (pts[7].Y - pts[6].Y) / 3;
                                mid_pts[2].X = mid_pts[0].X + (mid_pts[1].X - mid_pts[0].X) * 2 / 3;
                                mid_pts[2].Y = mid_pts[0].Y + (mid_pts[1].Y - mid_pts[0].Y) * 2 / 3;

                                mid_pts[3].X = pts[2].X + (pts[3].X - pts[2].X) * 2 / 3;
                                mid_pts[3].Y = pts[2].Y + (pts[3].Y - pts[2].Y) * 2 / 3;
                                mid_pts[4].X = pts[4].X + (pts[5].X - pts[4].X) * 2 / 3;
                                mid_pts[4].Y = pts[4].Y + (pts[5].Y - pts[4].Y) * 2 / 3;
                                mid_pts[5].X = mid_pts[3].X + (mid_pts[4].X - mid_pts[3].X) / 3;
                                mid_pts[5].Y = mid_pts[3].Y + (mid_pts[4].Y - mid_pts[3].Y) / 3;

                                AdjustableArrowCap arrow_cap = new AdjustableArrowCap(6, 6, true);
                                Pen arrow_pen = new Pen(Color.FromArgb(0, 255, 0), 1);
                                arrow_pen.CustomEndCap = arrow_cap;

                                if (false == m_bShowLineOnly)
                                {
                                    g.DrawLine(arrow_pen, mid_pts[2], mid_pts[0]);
                                    g.DrawLine(arrow_pen, mid_pts[2], mid_pts[1]);

                                    g.DrawLine(arrow_pen, mid_pts[5], mid_pts[3]);
                                    g.DrawLine(arrow_pen, mid_pts[5], mid_pts[4]);

                                    double value = m_gauged_line_width / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                                    String str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                        m_strUnits[parent.m_nUnitType]);

                                    Font ft = new Font("宋体", 20, FontStyle.Bold);
                                    //Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));
                                    Brush brush = new SolidBrush(violet);
                                    g.DrawString(str, ft, brush, mid_pts[2].X, mid_pts[2].Y);

                                    value = m_gauged_line_width2 / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                                    str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                        m_strUnits[parent.m_nUnitType]);

                                    //brush = new SolidBrush(Color.FromArgb(255, 0, 0));
                                    brush = new SolidBrush(violet);
                                    g.DrawString(str, ft, brush, mid_pts[5].X, mid_pts[5].Y);

                                    double value1 = 0;
                                    double value2 = 0;
                                    double min_dist = 0;
                                    double max_dist = 0;

                                    switch (m_measure_type)
                                    {
                                        case MEASURE_TYPE.LINE_WIDTH_1234:
                                            value1 = GeneralUtils.get_distance(m_gauge_result_pts[0], m_gauge_result_pts[6]);
                                            value2 = GeneralUtils.get_distance(m_gauge_result_pts[1], m_gauge_result_pts[7]);
                                            break;
                                    }
                                    if (value1 > value2)
                                    {
                                        min_dist = value2;
                                        max_dist = value1;
                                    }
                                    else
                                    {
                                        min_dist = value1;
                                        max_dist = value2;
                                    }
                                    min_dist /= parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    max_dist /= parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    min_dist = GeneralUtils.convert_um_value_by_unit(min_dist, parent.m_nUnitType);
                                    max_dist = GeneralUtils.convert_um_value_by_unit(max_dist, parent.m_nUnitType);

                                    //if (false == m_bIsClonedObject)
                                    //{
                                    //    ft = new Font("宋体", 13, FontStyle.Bold);
                                    //    str = string.Format("最小值: {0}{1}", GeneralUtils.convert_number_to_string_with_digits(min_dist, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                    //        m_strUnits[parent.m_nUnitType]);
                                    //    g.DrawString(str, ft, brush, m_nPictureBoxWidth - 180, 3);

                                    //    str = string.Format("最大值: {0}{1}", GeneralUtils.convert_number_to_string_with_digits(max_dist, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                    //        m_strUnits[parent.m_nUnitType]);
                                    //    g.DrawString(str, ft, brush, m_nPictureBoxWidth - 180, 42);
                                    //}
                                }
                                #endregion
                            }
                            else
                            {
                                g.DrawLine(green_pen, pts[0], pts[1]);
                                g.DrawLine(green_pen, pts[2], pts[3]);

                                mid_pts[0].X = (pts[0].X + pts[1].X) / 2;
                                mid_pts[0].Y = (pts[0].Y + pts[1].Y) / 2;
                                mid_pts[1].X = (pts[2].X + pts[3].X) / 2;
                                mid_pts[1].Y = (pts[2].Y + pts[3].Y) / 2;
                                mid_pts[2].X = (mid_pts[0].X + mid_pts[1].X) / 2;
                                mid_pts[2].Y = (mid_pts[0].Y + mid_pts[1].Y) / 2;

                                AdjustableArrowCap arrow_cap = new AdjustableArrowCap(6, 6, true);
                                Pen arrow_pen = new Pen(Color.FromArgb(0, 255, 0), 1);
                                arrow_pen.CustomEndCap = arrow_cap;
                                
                                if (false == m_bShowLineOnly)
                                {
                                    g.DrawLine(arrow_pen, mid_pts[2], mid_pts[0]);
                                    g.DrawLine(arrow_pen, mid_pts[2], mid_pts[1]);

                                    double value = m_gauged_line_width / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                                    String str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                        m_strUnits[parent.m_nUnitType]);

                                    //Font ft = new Font("宋体", 25, FontStyle.Bold);
                                    Font ft = new Font(globaldata.m_fontname, (float)globaldata.m_fontsize, FontStyle.Bold);
                                    //Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));
                                    //Brush brush = new SolidBrush(violet);
                                    Color fontcolor = ColorTranslator.FromHtml("#" + globaldata.m_fontcolor);
                                    Brush brush = new SolidBrush(fontcolor);
                                    //g.DrawString(str, ft, brush, mid_pts[2].X, mid_pts[2].Y);
                                    set_Font(g, str, pb, mid_pts[2], bDrawToSourceImage);//设置字体颜色位置等
                                }
                            }
                        }
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
            Graphics g = pe.Graphics;

            if (true == bDrawToSourceImage)
                g = Graphics.FromImage(source_image);

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
            arrow_pts[1].X = down_pt.X + (up_pt.X - down_pt.X) / 7;
            arrow_pts[1].Y = down_pt.Y + (up_pt.Y - down_pt.Y) / 7;
            arrow_pts[3].X = up_pt.X + (down_pt.X - up_pt.X) / 7;
            arrow_pts[3].Y = up_pt.Y + (down_pt.Y - up_pt.Y) / 7;
            g.DrawLine(arrow_pen, arrow_pts[0], arrow_pts[1]);
            g.DrawLine(arrow_pen, arrow_pts[2], arrow_pts[3]);
        }

        public override void copy_measure_result_data(ref Gauger dest)
        {
            if (this.m_measure_type == dest.m_measure_type)
            {
                for (int n = 0; n < m_gauge_result_pts.Length; n++)
                    ((Gauger_LineWidth)dest).m_gauge_result_pts[n] = m_gauge_result_pts[n];

                ((Gauger_LineWidth)dest).m_bHasValidGaugeResult = m_bHasValidGaugeResult;
                ((Gauger_LineWidth)dest).m_gauged_line_width = m_gauged_line_width;
                ((Gauger_LineWidth)dest).m_gauged_line_width2 = m_gauged_line_width2;

                ((Gauger_LineWidth)dest).m_list_contours.Clear();
                ((Gauger_LineWidth)dest).m_key_points.Clear();

                for (int n = 0; n < m_list_contours.Count; n++)
                {
                    List<Point2d> contour = new List<Point2d>();

                    for (int k = 0; k < m_list_contours[n].Count; k++)
                    {
                        contour.Add(m_list_contours[n][k]);
                    }

                    ((Gauger_LineWidth)dest).m_list_contours.Add(contour);
                }

                for (int n = 0; n < m_key_points.Count; n++)
                    ((Gauger_LineWidth)dest).m_key_points.Add(m_key_points[n]);
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

        public void Setstringfont()
        {


        }
    }
}
