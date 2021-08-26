using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZWLineGauger.Gaugers
{
    class Gauger_HandPickLine : Gauger
    {
        Point2d m_move_pt = new Point2d();

        public Gauger_HandPickLine(MainUI parent, PictureBox pb)
        {
            this.parent = parent;
            m_nPictureBoxWidth = pb.Width;
            m_nPictureBoxHeight = pb.Height;

            m_measure_type = MEASURE_TYPE.HAND_PICK_LINE;
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

                if (2 == m_nClickCounter)
                {
                    m_nClickCounter = 0;

                    m_click_pts[0].set(me.X, me.Y);

                    for (int n = 1; n < m_click_pts.Length; n++)
                        m_click_pts[n].set(0, 0);
                }
                else
                    m_click_pts[m_nClickCounter].set(me.X, me.Y);

                m_nClickCounter++;
                if (1 == m_nClickCounter)
                    m_move_pt.set(me.X, me.Y);

                pb.Refresh();
            }
        }

        public override void on_mouse_up(MouseEventArgs me, PictureBox pb)
        {
            // 右键鼠标松开
            if (MouseButtons.Right == me.Button)
            {
                if (2 == m_nClickCounter)
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

        public override void on_mouse_move(MouseEventArgs me, PictureBox pb)
        {
            if (1 == m_nClickCounter)
            {
                m_move_pt.set(me.X, me.Y);

                if (GeneralUtils.get_distance(m_click_pts[0], m_move_pt) > 5)
                {
                    //Debugger.Log(0, null, string.Format("222222 on_mouse_move"));
                    pb.Refresh();
                }
            }
        }

        public override void on_mouse_wheel(MouseEventArgs me, PictureBox pb)
        {

        }

        public override void on_paint(PaintEventArgs pe, PictureBox pb)
        {
            Graphics g = pe.Graphics;

            if (m_nClickCounter > 0)
            {
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

                // 绘制手拉线
                if ((1 == m_nClickCounter) || (2 == m_nClickCounter))
                {
                    double ratio_x = (double)pb.Image.Width / (double)m_nPictureBoxWidth;
                    double ratio_y = (double)pb.Image.Height / (double)m_nPictureBoxHeight;

                    if (1 == m_nClickCounter)
                        g.DrawLine(yellow_pen_with_dash, (float)m_click_pts[0].x, (float)m_click_pts[0].y, (float)m_move_pt.x, (float)m_move_pt.y);
                    else
                    {
                        Point2d pt1 = new Point2d(m_click_pts[0].x * ratio_x, m_click_pts[0].y * ratio_y);
                        Point2d pt2 = new Point2d(m_click_pts[1].x * ratio_x, m_click_pts[1].y * ratio_y);

                        m_gauged_line_width = GeneralUtils.get_distance(pt1, pt2);

                        m_bHasValidGaugeResult = true;

                        Point2d center = new Point2d((pt1.x + pt2.x) / 2, (pt1.y + pt2.y) / 2);
                        m_object_center = center;

                        g.DrawLine(green_pen, (float)m_click_pts[0].x, (float)m_click_pts[0].y, (float)m_click_pts[1].x, (float)m_click_pts[1].y);

                        // 绘制数字值
                        if (true)
                        {
                            double value = m_gauged_line_width / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                            value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                            String str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                m_strUnits[parent.m_nUnitType]);

                            Font ft = new Font("宋体", 25, FontStyle.Bold);
                            Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));
                            PointF mid_pt = new PointF((float)(m_click_pts[0].x + m_click_pts[1].x) / 2, (float)(m_click_pts[0].y + m_click_pts[1].y) / 2);

                            g.DrawString(str, ft, brush, mid_pt.X, mid_pt.Y);
                        }
                    }
                }

                    for (int n = 0; n < m_nClickCounter; n++)
                    {
                        g.DrawEllipse(green_pen, (float)(m_click_pts[n].x - dot_radius), (float)(m_click_pts[n].y - dot_radius),
                            (float)dot_radius * 2, (float)dot_radius * 2);
                    }
                }
            }
        }
    }
