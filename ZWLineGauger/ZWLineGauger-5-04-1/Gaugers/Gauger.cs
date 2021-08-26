using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

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
        COMBO_LINE_TO_LINE = 16
    }

    public abstract class Gauger
    {
        public MainUI parent;

        public string[]   m_strUnits = new string[] { "mm", "um", "mil" };

        public MEASURE_TYPE   m_measure_type = MEASURE_TYPE.NONE;
        public const double   DEFAULT_ROTATED_RECT_WH_RATIO = 2.0;

        public object     m_real_time_lock = new object();

        public Point2d   m_down_pt = new Point2d(0, 0);
        public Point2d   m_up_pt = new Point2d(0, 0);

        public int   m_nClickCounter = 0;
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

        public Point2d[]   m_click_pts = new Point2d[6];
        
        public abstract bool gauge(Image img, byte[] pImageBuf = null, int nWidth = 0, int nHeight = 0, int nBytesPerLine = 0, int nAlgorithm = 0, bool bLocateLineInRealTime = false);
        
        public abstract void on_mouse_down(MouseEventArgs me, PictureBox pb);

        public abstract void on_mouse_up(MouseEventArgs me, PictureBox pb);

        public abstract void on_mouse_move(MouseEventArgs me, PictureBox pb);

        public abstract void on_mouse_wheel(MouseEventArgs me, PictureBox pb);

        public abstract void on_paint(PaintEventArgs pe, PictureBox pb);

        public void clear_gauger_state()
        {
            m_bHasValidGaugeResult = false;
            m_bShowLineOnly = false;

            m_nClickCounter = 0;
        }
    }
}
