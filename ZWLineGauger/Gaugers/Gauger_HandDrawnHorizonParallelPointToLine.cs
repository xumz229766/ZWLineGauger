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
    class Gauger_HandDrawnHorizonParallelPointToLine : Gauger
    {
        Point2d m_move_pt = new Point2d();
        
        public Gauger_HandDrawnHorizonParallelPointToLine(MainUI parent, PictureBox pb)
        {
            this.parent = parent;
            m_nPictureBoxWidth = pb.Width;
            m_nPictureBoxHeight = pb.Height;

            m_measure_type = MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE;
        }

        public override bool gauge(Image img, byte[] pImageBuf = null, int nWidth = 0, int nHeight = 0, int nBytesPerLine = 0, int nAlgorithm = 0, bool bLocateLineInRealTime = false)
        {
            //clear_string_position_data();
            return true;
        }

        public void update_ROI_rect()
        {
            Point2d second_pt = m_click_pts[1];
            if (1 == m_nClickCounter)
                second_pt = m_move_pt;

            int left = (int)(Math.Min(m_click_pts[0].x, second_pt.x));
            int right = (int)(Math.Max(m_click_pts[0].x, second_pt.x));
            int top = (int)(Math.Min(m_click_pts[0].y, second_pt.y));
            int bottom = (int)(Math.Max(m_click_pts[0].y, second_pt.y));

            int mid_x = (right + left) / 2;
            int width = 20 + (right - left);
            
            if (((int)(width)) % 4 != 0)
                width += (4 - ((int)(width)) % 4);

            m_ROI_rect[0].set(left, top);
            m_ROI_rect[1].set(right, top);
            m_ROI_rect[2].set(right, bottom);
            m_ROI_rect[3].set(left, bottom);

            //m_ROI_rect[0].set(mid_x - width, top);
            //m_ROI_rect[1].set(mid_x + width, top);
            //m_ROI_rect[2].set(mid_x + width, bottom);
            //m_ROI_rect[3].set(mid_x - width, bottom);

            //Debugger.Log(0, null, string.Format("222222 left [{0},{1}], [{2},{3}]", left, top, right, bottom));
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

        public override void on_mouse_move(MouseEventArgs me, PictureBox pb, bool bHorizonModeForGaugerRect = true)
        {
            if (1 == m_nClickCounter)
            {
                m_move_pt.set(me.X, me.Y);

                if (GeneralUtils.get_distance(m_click_pts[0], m_move_pt) > 5)
                {
                    //Debugger.Log(0, null, string.Format("222222 on_mouse_move"));

                    update_ROI_rect();

                    pb.Refresh();
                }
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

            if (true == bDrawToSourceImage)
            {
                g = Graphics.FromImage(source_image);
                
                m_nClickCounter = 2;
            }
            
            if (m_nClickCounter > 0)
            {
                double dot_radius = 2;
                Pen red_pen = new Pen(Color.FromArgb(255, 0, 0), 2);
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

                double ratio_x = (double)pb.Image.Width / (double)m_nPictureBoxWidth;
                double ratio_y = (double)pb.Image.Height / (double)m_nPictureBoxHeight;

                // 绘制手拉线
                if ((1 == m_nClickCounter) || (2 == m_nClickCounter))
                {
                    for (int n = 0; n < 4; n++)
                    {
                        PointF pt = new PointF((float)m_ROI_rect[n].x, (float)m_ROI_rect[n].y);
                        if (true == bDrawToSourceImage)
                            pt = new PointF((float)(m_ROI_rect[n].x * ratio_x), (float)(m_ROI_rect[n].y * ratio_y));

                        if(n!=1)
                        g.DrawEllipse(new Pen(Color.FromArgb(255, 0, 0), 2), pt.X - 2, pt.Y - 2, 4, 4);
                    }

                    for (int n = 1; n < 2; n++)
                    {
                        float xx = (float)m_ROI_rect[n * 2 + 1].x - (float)m_ROI_rect[n * 2].x;
                        if (true == bDrawToSourceImage)
                            g.DrawLine(red_pen, (float)(m_ROI_rect[n * 2].x * ratio_x), (float)(m_ROI_rect[n * 2].y * ratio_y), 
                                (float)((m_ROI_rect[n * 2 + 1].x + xx) * ratio_x), (float)(m_ROI_rect[n * 2 + 1].y * ratio_y));
                        else
                            g.DrawLine(red_pen, (float)m_ROI_rect[n * 2].x, (float)m_ROI_rect[n * 2].y, (float)m_ROI_rect[n * 2 + 1].x+xx, (float)m_ROI_rect[n * 2 + 1].y);
                    }

                    if (1 == m_nClickCounter)
                    {
                        g.DrawLine(yellow_pen_with_dash, (float)m_click_pts[0].x, (float)m_click_pts[0].y, (float)m_ROI_rect[3].x, (float)m_ROI_rect[3].y);
                    }
                    else
                    {
                        Point2d pt1 = new Point2d(m_ROI_rect[0].x * ratio_x, m_ROI_rect[0].y * ratio_y);
                        Point2d pt2 = new Point2d(m_ROI_rect[3].x * ratio_x, m_ROI_rect[3].y * ratio_y);

                        m_gauged_line_width = GeneralUtils.get_distance(pt1, pt2);
                        m_bHasValidGaugeResult = true;
                        
                        Point2d mid_pt1 = new Point2d(m_ROI_rect[0].x , m_ROI_rect[0].y);
                        Point2d mid_pt2 = new Point2d(m_ROI_rect[3].x, m_ROI_rect[3].y);

                        m_object_center = new Point2d((mid_pt1.x + mid_pt2.x) * ratio_x / 2, (mid_pt1.y + mid_pt2.y) * ratio_y / 2);

                        if (true == bDrawToSourceImage)
                            g.DrawLine(red_pen, (float)(mid_pt1.x * ratio_x), (float)(mid_pt1.y * ratio_y), (float)(mid_pt2.x * ratio_x), (float)(mid_pt2.y * ratio_y));
                        else
                            g.DrawLine(red_pen, (float)mid_pt1.x, (float)mid_pt1.y, (float)mid_pt2.x, (float)mid_pt2.y);

                        // 绘制数字值
                        if (true)
                        {
                            double value = m_gauged_line_width / parent.m_calib_data[parent.comboBox_Len.SelectedIndex];
                            value = GeneralUtils.convert_um_value_by_unit(value, parent.m_nUnitType);
                            String str = string.Format("{0}{1}", GeneralUtils.convert_number_to_string_with_digits(value, parent.m_nMeasureResultDigits[parent.m_nUnitType]),
                                m_strUnits[parent.m_nUnitType]);

                            //Font ft = new Font("宋体", 25, FontStyle.Bold);
                            //Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));
                            //xumz
                            //Font ft = new Font(globaldata.m_fontname, (float)globaldata.m_fontsize, FontStyle.Bold);
                            //Color fontcolor = ColorTranslator.FromHtml("#" + globaldata.m_fontcolor);
                            //Brush brush = new SolidBrush(fontcolor);
                            PointF mid_pts = new PointF((float)m_click_pts[0].x , (float)(m_click_pts[0].y + m_click_pts[1].y) / 2);


                            set_Font_handle(g, str, pb, mid_pts, bDrawToSourceImage);

                            //if (true == bDrawToSourceImage)
                            //{
                            //    //if (globaldata.isStringOffset)
                            //    {
                            //        globaldata.isStringOffset = false;
                            //        m_string_position.x = (mid_pts.X) * (float)ratio_x;
                            //        m_string_position.y = (mid_pts.Y) * (float)ratio_y;
                            //        m_mouse_string_down_pt = m_mouse_string_move_pt;

                            //        m_fStringWidth = (float)(globaldata.m_fontsize * 5.6);
                            //        m_fStringHeight = (float)globaldata.m_fontsize;
                            //        //ft = new Font(globaldata.m_fontname, (float)globaldata.m_fontsize * ((float)ratio_x + (float)ratio_y) / 2, FontStyle.Bold);

                            //        g.DrawString(str, ft, brush, m_string_position.x, m_string_position.y);


                            //    }
                            //}
                            //else
                            //{

                            //    if ((true == bDrawToSourceImage) && (2 == m_nClickCounter))
                            //        mid_pts = new PointF((float)(m_click_pts[0].x * ratio_x + m_click_pts[1].x * ratio_x) / 2,
                            //            (float)(m_click_pts[0].y * ratio_y + m_click_pts[1].y * ratio_y) / 2);

                            //    g.DrawString(str, ft, brush, mid_pts.X, mid_pts.Y);
                            //}
                        }
                        
                        }
                }

                //for (int n = 0; n < m_nClickCounter; n++)
                //{
                //    if ((true == bDrawToSourceImage) && (2 == m_nClickCounter))
                //        g.DrawEllipse(green_pen, (float)(m_click_pts[n].x * ratio_x - dot_radius), (float)(m_click_pts[n].y * ratio_y - dot_radius),
                //            (float)dot_radius * 2, (float)dot_radius * 2);
                //    else
                //        g.DrawEllipse(green_pen, (float)(m_click_pts[n].x - dot_radius), (float)(m_click_pts[n].y - dot_radius), (float)dot_radius * 2, (float)dot_radius * 2);
                //}
            }
        }

        public override void show_selection_frame(PaintEventArgs pe, PictureBox pb, Image source_image = null, bool bDrawToSourceImage = false)
        {

        }

        public override void copy_measure_result_data(ref Gauger dest)
        {
            Debugger.Log(0, null, string.Format("222222 this.m_measure_type {0}, dest.m_measure_type {1}", this.m_measure_type, dest.m_measure_type));
            if (this.m_measure_type == dest.m_measure_type)
            {
                ((Gauger_HandDrawnHorizonParallelPointToLine)dest).m_bHasValidGaugeResult = m_bHasValidGaugeResult;
                ((Gauger_HandDrawnHorizonParallelPointToLine)dest).m_nClickCounter = m_nClickCounter;

                for (int n = 0; n < m_click_pts.Length; n++)
                    ((Gauger_HandDrawnHorizonParallelPointToLine)dest).m_click_pts[n] = m_click_pts[n];

                ((Gauger_HandDrawnHorizonParallelPointToLine)dest).m_gauged_circle_center = m_gauged_circle_center;
                ((Gauger_HandDrawnHorizonParallelPointToLine)dest).m_gauged_circle_radius = m_gauged_circle_radius;

                for (int n = 0; n < 4; n++)
                    ((Gauger_HandDrawnHorizonParallelPointToLine)dest).m_ROI_rect[n] = m_ROI_rect[n];

                dest.m_bHasValidGaugeResult = true;
                dest.m_gauged_line_width = m_gauged_line_width;
                dest.m_gauged_line_width2 = m_gauged_line_width2;
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

                dest.m_bHasValidGaugeResult = true;
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
