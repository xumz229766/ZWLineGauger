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
    class Gauger_HandPickCircle : Gauger
    {
        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool fit_circle(double[] in_doubles, int[] out_ints, double[] out_doubles);

        Point2d m_move_pt = new Point2d();
        
        public Gauger_HandPickCircle(MainUI parent, PictureBox pb)
        {
            this.parent = parent;
            m_nPictureBoxWidth = pb.Width;
            m_nPictureBoxHeight = pb.Height;

            m_measure_type = MEASURE_TYPE.HAND_PICK_CIRCLE;
        }

        public override bool gauge(Image img, byte[] pImageBuf = null, int nWidth = 0, int nHeight = 0, int nBytesPerLine = 0, int nAlgorithm = 0, bool bLocateLineInRealTime = false)
        {
            return true;
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
                else
                {
                    if (false == m_bHasValidGaugeResult)
                    {
                        for (int n = 0; n < m_click_pts.Length; n++)
                            m_click_pts[n].set(0, 0);

                        m_nClickCounter = 0;
                    }
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
                if ((2 == m_nClickCounter) || (3 == m_nClickCounter))
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

                            double ratio_x = (double)pb.Image.Width / (double)m_nPictureBoxWidth;
                            double ratio_y = (double)pb.Image.Height / (double)m_nPictureBoxHeight;

                            Point2d center = new Point2d(out_doubles[0], out_doubles[1]);
                            double radius = out_doubles[2] * 2;

                            m_gauged_circle_center.x = out_doubles[0] * ratio_x;
                            m_gauged_circle_center.y = out_doubles[1] * ratio_y;
                            m_gauged_circle_radius = out_doubles[2] * 2 * (ratio_x + ratio_y) / 2;

                            m_object_center = m_gauged_circle_center;

                            //Debugger.Log(0, null, string.Format("222222 radius = {0:0.000}, {1:0.000}", radius, m_gauged_circle_radius));

                            float x = (float)(center.x - (radius / 2));
                            float y = (float)(center.y - (radius / 2));

                            //Debugger.Log(0, null, string.Format("222222 m_gauged_circle_center = [{0:0.000},{1:0.000}]", m_gauged_circle_center.x, m_gauged_circle_center.y));

                            if (3 == m_nClickCounter)
                            {
                                g.DrawEllipse(green_pen, x, y, (float)radius, (float)radius);

                                m_bHasValidGaugeResult = true;

                                // 绘制数字值
                                if (true)
                                {
                                    double value = m_gauged_circle_radius / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                                    value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                                    String str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                        m_strUnits[parent.m_nUnitType]);
                                    
                                    float start_x = (float)((m_gauged_circle_center.x) / ratio_x);
                                    float start_y = (float)((m_gauged_circle_center.y) / ratio_y);

                                    //Font ft = new Font("宋体", 25, FontStyle.Bold);
                                    //Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));
                                    //xumz
                                    //Font ft = new Font(globaldata.m_fontname, (float)globaldata.m_fontsize, FontStyle.Bold);
                                    //Color fontcolor = ColorTranslator.FromHtml("#" + globaldata.m_fontcolor);
                                    //Brush brush = new SolidBrush(fontcolor);
                                    //g.DrawString(str, ft, brush, start_x, start_y);
                                    PointF mid_pts = new PointF();
                                    mid_pts.X= start_x;
                                    mid_pts.Y= start_y;
                                    set_Font_handle(g, str, pb, mid_pts, bDrawToSourceImage);
                                }
                            }
                            else
                                g.DrawEllipse(yellow_pen_with_dash, x, y, (float)radius, (float)radius);
                        }

                    }
                }

                if (false == m_bHasValidGaugeResult)
                {
                    for (int n = 0; n < m_nClickCounter; n++)
                    {
                        g.DrawEllipse(green_pen, (float)(m_click_pts[n].x - dot_radius), (float)(m_click_pts[n].y - dot_radius),
                            (float)dot_radius * 2, (float)dot_radius * 2);
                    }
                }
            }

        }

        public override void show_selection_frame(PaintEventArgs pe, PictureBox pb, Image source_image = null, bool bDrawToSourceImage = false)
        {

        }

        public override void copy_measure_result_data(ref Gauger dest)
        {
            ((Gauger_HandPickCircle)dest).m_bHasValidGaugeResult = m_bHasValidGaugeResult;
            ((Gauger_HandPickCircle)dest).m_nClickCounter = m_nClickCounter;

            for (int n = 0; n < m_click_pts.Length; n++)
                ((Gauger_HandPickCircle)dest).m_click_pts[n] = m_click_pts[n];

            ((Gauger_HandPickCircle)dest).m_gauged_circle_center = m_gauged_circle_center;
            ((Gauger_HandPickCircle)dest).m_gauged_circle_radius = m_gauged_circle_radius;
            dest.m_string_offset_real = m_string_offset_real;
        }
    }
}
