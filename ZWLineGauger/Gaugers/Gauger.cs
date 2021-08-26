using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ZWLineGauger.Gaugers
{
    public enum MEASURE_TYPE
    {
        NONE = 0,
        LINE = 1,
        LINE_WIDTH_14 = 2,
        LINE_WIDTH_23 = 3,
        LINE_WIDTH_13 = 4,
        LINE_WIDTH_1234 = 5,
        LINE_WIDTH_AVR = 6,
        ARC_LINE_WIDTH = 7,
        LINE_SPACE = 8,
        LINE_SPACE_13 = 9,
        ARC_LINE_SPACE = 10,
        HAND_PICK_LINE = 11,
        CIRCLE_OUTER_TO_INNER = 12,
        CIRCLE_INNER_TO_OUTER = 13,
        FIDUCIAL_MARK_CIRCLE = 14,
        HAND_PICK_CIRCLE = 15,
        COMBO_LINE_TO_LINE = 16,
        SHORT_SPACE_BETWEEN_TWO_EMPTY_CIRCLES = 17,
        L_SHAPE = 18,
        BULGE = 19,
        LINE_WIDTH_BY_CONTOUR = 20,
        LINE_TO_EDGE = 21,
        ETCH_DOWN = 22,
        HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE = 23,             // 水平平行线
        HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE = 24,           // 竖直平行线
        HAND_DRAWN_PARALLEL_LINE_TO_LINE = 25,                                 // 任意角度平行线
        HAND_DRAWN_HORIZON_POINT_TO_LINE = 26,
        HAND_DRAWN_VERTICAL_POINT_TO_LINE = 27,
        HAND_DRAWN_POINT_TO_LINE = 28
    }

    public abstract class Gauger
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();

        public MainUI parent;

        public string[]   m_strUnits = new string[] { "mm", "um", "mil" };

        public MEASURE_TYPE   m_measure_type = MEASURE_TYPE.NONE;
        public const double   DEFAULT_ROTATED_RECT_WH_RATIO = 2.0;

        public object     m_real_time_lock = new object();

        public Point2d   m_down_pt = new Point2d(0, 0);
        public Point2d   m_up_pt = new Point2d(0, 0);

        public int   m_nClickCounter = 0;
        public int   m_nLastFailureTime = 0;                                          // 最后一次测量失败的时间戳
        public int   m_nPictureBoxWidth = 1;
        public int   m_nPictureBoxHeight = 1;
        public bool   m_bIsMouseDown = false;
        public bool   m_bRotatedRectIsReady = false;
        public bool   m_bHasValidGaugeResult = false;
        public bool   m_bIsClonedObject = false;                                   // 如果是拷贝的对象，则不显示最大值和最小值
        public bool   m_bShowLineOnly = false;                                    // 仅显示找到的边，不显示测量数值，用于实时找边

        public Point2d[] m_ROI_rect = new Point2d[4];                        // 用户拉出来的ROI框
        public Point2d   m_object_center = new Point2d();

        public Point2d   m_gauged_circle_center = new Point2d();

        public double    m_gauged_circle_radius = 0;

        public double    m_gauged_line_width = 0;
        public double    m_gauged_line_width2 = 0;                // 上下线宽时，2是上线宽

        public double    m_gauged_line_space = 0;

        public bool        m_bIsCopiedFromDifferentMeasureType = false;                                                 // 是否属于其它测量项的拷贝
        public MEASURE_TYPE   m_measure_type_of_clone_object = MEASURE_TYPE.NONE;      // 如果是其它测量项的拷贝，记录拷贝对象的类型
        //public MEASURE_TYPE   m_origin_measure_type = MEASURE_TYPE.NONE;                    // 如果是其它测量项的拷贝，记录原始测量项的类型

        public bool        m_bHorizonModeForGaugerRect = true;

        public Point2d[]   m_gauge_result_pts = new Point2d[8];

        public Point2d[]   m_click_pts = new Point2d[6];

        public Point2f      m_string_offset = new Point2f(0, 0);
        public Point2f      m_string_position = new Point2f(0, 0);
        public Point2f    m_string_offset_real = new Point2f(0, 0);
        public Point2f      m_mouse_string_down_pt = new Point2f(0, 0);
        public Point2f      m_mouse_string_move_pt = new Point2f(0, 0);
        public float          m_fStringWidth = 0;
        public float          m_fStringHeight = 0;
        public bool          m_bIsMouseClickDownOnString = false;

        public List<List<Point2d>>   m_list_contours = new List<List<Point2d>>();          // 轮廓线点集合
        public List<Point2d>              m_key_points = new List<Point2d>();                        // 关键点坐标集合，如拐角点，最近点等等
        
        public abstract bool gauge(Image img, byte[] pImageBuf = null, int nWidth = 0, int nHeight = 0, int nBytesPerLine = 0, int nAlgorithm = 0, bool bLocateLineInRealTime = false);
        
        protected Gauger create_gauger(MainUI parent, PictureBox pb, MEASURE_TYPE type)
        {
            Gauger gauger = new Gauger_LineWidth(parent, pb, type);

            switch (type)
            {
                case MEASURE_TYPE.LINE_WIDTH_14:
                    gauger = new Gauger_LineWidth(parent, pb, type);
                    break;
                case MEASURE_TYPE.LINE_WIDTH_23:
                    gauger = new Gauger_LineWidth(parent, pb, type);
                    break;
                case MEASURE_TYPE.L_SHAPE:
                    gauger = new Gauger_LShapeItem(parent, pb, type);
                    break;
                case MEASURE_TYPE.ETCH_DOWN:
                    gauger = new Gauger_EtchDown(parent, pb, type);
                    break;
                case MEASURE_TYPE.BULGE:
                    gauger = new Gauger_Bulge(parent, pb, type);
                    break;
                case MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR:
                    gauger = new Gauger_LineWidthByContour(parent, pb, type);
                    break;
                case MEASURE_TYPE.LINE_TO_EDGE:
                    gauger = new Gauger_LineToEdge(parent, pb, type);
                    break;
                case MEASURE_TYPE.LINE_WIDTH_13:
                    gauger = new Gauger_LineWidth(parent, pb, type);
                    break;
                case MEASURE_TYPE.LINE_WIDTH_1234:
                    gauger = new Gauger_LineWidth(parent, pb, type);
                    break;
                case MEASURE_TYPE.LINE_SPACE:
                    gauger = new Gauger_LineSpace(parent, pb, type);
                    break;
                case MEASURE_TYPE.ARC_LINE_SPACE:
                    gauger = new Gauger_ArcLineSpace(parent, pb, type);
                    break;
                case MEASURE_TYPE.HAND_PICK_LINE:
                    gauger = new Gauger_HandPickLine1(parent, pb);
                    break;
                case MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE:
                    Debugger.Log(0, null, string.Format("222222 ......gauger = new Gauger_HandDrawnHorizonParallelLineToLine"));
                    gauger = new Gauger_HandDrawnHorizonParallelLineToLine(parent, pb);
                    break;
                case MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE:
                    gauger = new Gauger_HandDrawnVerticalParallelLineToLine(parent, pb);
                    break;
                case MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE:
                    Debugger.Log(0, null, string.Format("222222 ......gauger = new Gauger_HandDrawnHorizonParallelLineToLine"));
                    gauger = new Gauger_HandDrawnHorizonParallelPointToLine(parent, pb);
                    break;
                case MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE:
                    gauger = new Gauger_HandDrawnVerticalParallelPointToLine(parent, pb);
                    break;
                case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                    gauger = new Gauger_CircleOuterToInner(parent, pb);
                    break;
                case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                    gauger = new Gauger_CircleInnerToOuter(parent, pb);
                    break;
                case MEASURE_TYPE.HAND_PICK_CIRCLE:
                    gauger = new Gauger_HandPickCircle(parent, pb);
                    break;
            }

            return gauger;
        }

        public abstract void on_mouse_down(MouseEventArgs me, PictureBox pb);

        public abstract void on_mouse_up(MouseEventArgs me, PictureBox pb);

        public abstract void on_mouse_move(MouseEventArgs me, PictureBox pb, bool bHorizonModeForGaugerRect = true);

        public abstract void on_mouse_wheel(MouseEventArgs me, PictureBox pb, bool bHorizonModeForGaugerRect = true);

        public abstract void on_mouse_leave(EventArgs me, PictureBox pb);

        public abstract void on_paint(PaintEventArgs pe, PictureBox pb, Image source_image = null, bool bDrawToSourceImage = false);

        public abstract void show_selection_frame(PaintEventArgs pe, PictureBox pb, Image source_image = null, bool bDrawToSourceImage = false);

        public abstract void copy_measure_result_data(ref Gauger dest);

        public void clear_gauger_state()
        {
            m_bHasValidGaugeResult = false;
            m_bShowLineOnly = false;

            m_nClickCounter = 0;
        }

        public void set_failure_time()
        {
            m_nLastFailureTime = GetTickCount();
        }

        public void set_horizon_mode(bool bHorizonModeForGaugerRect)
        {
            m_bHorizonModeForGaugerRect = bHorizonModeForGaugerRect;
        }
        
        public void set_mouse_down_on_string_state(bool bDown)
        {
            m_bIsMouseClickDownOnString = bDown;
        }

        public void set_mouse_string_down_pt(MouseEventArgs e)
        {
            m_mouse_string_down_pt = new Point2f(e.X, e.Y);
        }

        public void set_mouse_string_move_pt(MouseEventArgs e)
        {
            m_mouse_string_move_pt = new Point2f(e.X, e.Y);
        }

        public void clear_string_position_data()
        {
            m_mouse_string_down_pt = new Point2f(0, 0);
            m_mouse_string_move_pt = new Point2f(0, 0);
            m_string_position = new Point2f(0, 0);
            //m_string_offset_real = new Point2f(0, 0);
            
        }

        public bool is_mouse_in_string_rect(MouseEventArgs e)
        {
            if ((e.X > m_string_position.x) && (e.X < (m_string_position.x + m_fStringWidth)))
            {
                if ((e.Y > m_string_position.y) && (e.Y < (m_string_position.y + m_fStringHeight)))
                    return true;
            }

            return false;
        }

        public void set_Font(Graphics g,string str, PictureBox pb, PointF mid_pts,bool bDrawToSourceImage=true)
        {
            //Font ft = new Font("宋体", 25, FontStyle.Bold);
            ////Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));
            //Brush brush = new SolidBrush(violet);
            //xumz
            Font ft = new Font(globaldata.m_fontname, (float)globaldata.m_fontsize,(FontStyle) globaldata.m_fontstyle);
            Color fontcolor = ColorTranslator.FromHtml("#" + globaldata.m_fontcolor);
            Brush brush = new SolidBrush(fontcolor);

            m_string_offset.x = m_mouse_string_move_pt.x - m_mouse_string_down_pt.x;
            m_string_offset.y = m_mouse_string_move_pt.y - m_mouse_string_down_pt.y;


            #region
            if (!globaldata.isRun)
            {
                if (m_mouse_string_move_pt.x > 0)
                {
                    m_string_position.x += m_string_offset.x;
                    m_string_position.y += m_string_offset.y;
                }
                else
                {
                    m_string_position.x = mid_pts.X + m_string_offset.x;
                    m_string_position.y = mid_pts.Y + m_string_offset.y;
                }
                m_string_offset_real.x = m_string_position.x - mid_pts.X;
                m_string_offset_real.y = m_string_position.y - mid_pts.Y;
                m_mouse_string_down_pt = m_mouse_string_move_pt;

                m_fStringWidth = (float)(globaldata.m_fontsize * 5.6);
                m_fStringHeight = (float)globaldata.m_fontsize;

                //Debugger.Log(0, null, string.Format("222222 m_string_position = [{0:0.},{1:0.}], strlen = {2}, m_fStringWidth = [{3:0.},{4:0.}]", 
                //    m_string_position.x, m_string_position.y, str.Length, m_fStringWidth, m_fStringHeight));

                g.DrawString(str, ft, brush, m_string_position.x, m_string_position.y);
            }
            else
            {


                if (true == bDrawToSourceImage)
                {
                    //if (globaldata.isStringOffset)
                    {
                        float ratio_x = (float)pb.Image.Width / (float)m_nPictureBoxWidth;
                        float ratio_y = (float)pb.Image.Height / (float)m_nPictureBoxHeight;
                        globaldata.isStringOffset = false;
                        m_string_position.x = mid_pts.X + m_string_offset_real.x * ratio_x;
                        m_string_position.y = mid_pts.Y + m_string_offset_real.y * ratio_y;
                        m_mouse_string_down_pt = m_mouse_string_move_pt;

                        m_fStringWidth = (float)(globaldata.m_fontsize * 5.6);
                        m_fStringHeight = (float)globaldata.m_fontsize;
                        ft = new Font(globaldata.m_fontname, (float)globaldata.m_fontsize * (float)((ratio_x + ratio_y) / 2.0), FontStyle.Bold);

                        g.DrawString(str, ft, brush, m_string_position.x, m_string_position.y);


                    }
                }
                else
                {
                    //if (globaldata.isStringOffset)
                    {
                        globaldata.isStringOffset = false;
                        m_string_position.x = mid_pts.X + m_string_offset_real.x;
                        m_string_position.y = mid_pts.Y + m_string_offset_real.y;
                        m_mouse_string_down_pt = m_mouse_string_move_pt;

                        m_fStringWidth = (float)(globaldata.m_fontsize * 5.6);
                        m_fStringHeight = (float)globaldata.m_fontsize;

                        //Debugger.Log(0, null, string.Format("222222 m_string_position = [{0:0.},{1:0.}], strlen = {2}, m_fStringWidth = [{3:0.},{4:0.}]", 
                        //    m_string_position.x, m_string_position.y, str.Length, m_fStringWidth, m_fStringHeight));

                        g.DrawString(str, ft, brush, m_string_position.x, m_string_position.y);
                    }
                }
            }
                #endregion
            }

        public void set_Font_handle(Graphics g, string str, PictureBox pb, PointF mid_pts, bool bDrawToSourceImage = true)
        {
            //Font ft = new Font("宋体", 25, FontStyle.Bold);
            ////Brush brush = new SolidBrush(Color.FromArgb(0, 255, 0));
            //Brush brush = new SolidBrush(violet);
            //xumz
            Font ft = new Font(globaldata.m_fontname, (float)globaldata.m_fontsize, (FontStyle)globaldata.m_fontstyle);
            Color fontcolor = ColorTranslator.FromHtml("#" + globaldata.m_fontcolor);
            Brush brush = new SolidBrush(fontcolor);

            m_string_offset.x = m_mouse_string_move_pt.x - m_mouse_string_down_pt.x;
            m_string_offset.y = m_mouse_string_move_pt.y - m_mouse_string_down_pt.y;


            #region

            if (true == bDrawToSourceImage)
            {
                //if (globaldata.isStringOffset)
                {
                    float ratio_x = (float)pb.Image.Width / (float)m_nPictureBoxWidth;
                    float ratio_y = (float)pb.Image.Height / (float)m_nPictureBoxHeight;
                    globaldata.isStringOffset = false;
                    m_string_position.x = (mid_pts.X + m_string_offset_real.x) * ratio_x;
                    m_string_position.y = (mid_pts.Y + m_string_offset_real.y) * ratio_y;
                    m_mouse_string_down_pt = m_mouse_string_move_pt;

                    m_fStringWidth = (float)(globaldata.m_fontsize * 5.6);
                    m_fStringHeight = (float)globaldata.m_fontsize;
                    ft = new Font(globaldata.m_fontname, (float)globaldata.m_fontsize * (float)((ratio_x + ratio_y) / 2.0), FontStyle.Bold);

                    g.DrawString(str, ft, brush, m_string_position.x, m_string_position.y);

                    globaldata.isDrawFinish = true;//xumz 防止刷新字体位置偏移
                }
            }
            else if (globaldata.isRun)
            {
                if (globaldata.isDrawFinish || globaldata.isHandleFinish)
                {

                    return;
                }
                {
                    float ratio_x = (float)pb.Image.Width / (float)m_nPictureBoxWidth;
                    float ratio_y = (float)pb.Image.Height / (float)m_nPictureBoxHeight;
                    globaldata.isStringOffset = false;
                    m_string_position.x = mid_pts.X + m_string_offset_real.x;
                    m_string_position.y = mid_pts.Y + m_string_offset_real.y;
                    m_mouse_string_down_pt = m_mouse_string_move_pt;

                    m_fStringWidth = (float)(globaldata.m_fontsize * 5.6);
                    m_fStringHeight = (float)globaldata.m_fontsize;
                    ft = new Font(globaldata.m_fontname, (float)globaldata.m_fontsize , FontStyle.Bold);

                    g.DrawString(str, ft, brush, m_string_position.x, m_string_position.y);

                }
            }
            else
            {
                //if (globaldata.isDrawFinish || globaldata.isHandleFinish)
                //{

                //    return;
                //}
                if (m_mouse_string_move_pt.x > 0)
                {
                    m_string_position.x += m_string_offset.x;
                    m_string_position.y += m_string_offset.y;
                }
                else
                {
                    m_string_position.x = mid_pts.X + m_string_offset.x;
                    m_string_position.y = mid_pts.Y + m_string_offset.y;
                }
                m_string_offset_real.x = m_string_position.x - mid_pts.X;
                m_string_offset_real.y = m_string_position.y - mid_pts.Y;
                m_mouse_string_down_pt = m_mouse_string_move_pt;

                m_fStringWidth = (float)(globaldata.m_fontsize * 5.6);
                m_fStringHeight = (float)globaldata.m_fontsize;

                //Debugger.Log(0, null, string.Format("222222 m_string_position = [{0:0.},{1:0.}], strlen = {2}, m_fStringWidth = [{3:0.},{4:0.}]", 
                //    m_string_position.x, m_string_position.y, str.Length, m_fStringWidth, m_fStringHeight));

                g.DrawString(str, ft, brush, m_string_position.x, m_string_position.y);
            }
            
            #endregion
        }
    }
}
