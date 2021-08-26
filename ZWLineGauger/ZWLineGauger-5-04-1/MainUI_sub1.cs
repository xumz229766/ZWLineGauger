using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using ZWLineGauger.Gaugers;

using ZWLineGauger.Forms;
using System.Data.SqlClient;
using ZWLineGauger.Hardwares;

namespace ZWLineGauger
{
    public enum enum_graphic_element_type
    {
        none,
        graphic_line,
        graphic_line_space,
        graphic_arc,
        graphic_circle,
        graphic_aperture_rect,
        graphic_aperture_circle,
        graphic_aperture_circle_line,
        graphic_padding_line,
        graphic_block
    }
    
    public struct MeasureResult
    {
        public MeasureResult(int idx, MEASURE_TYPE type, double result)
        {
            this.index = idx;
            this.type = type;
            this.result = result;
        }
        public int index;
        public MEASURE_TYPE type;
        public double result;
    }

    public class ThreeMarksRecord
    {
        public ThreeMarksRecord()
        {
            m_strOdbFileName = "";

            m_nLayerIdx = -1;
            m_nEvenOddFlag = 0;

            for (int n = 0; n < 3; n++)
            {
                m_dbDiameterInMM[n] = 0;
                m_nTopBrightness[n] = 0;
                m_nBottomBrightness[n] = 0;
                m_bIsTopLightOn[n] = true;
                m_bIsBottomLightOn[n] = false;
                m_marks_pt_on_graph[n] = new Point2d(0, 0);
                m_marks_pt_on_stage[n] = new Point3d(0, 0, 0);
            }
        }

        public string   m_strOdbFileName = "";
        public int       m_nLayerIdx = -1;
        public int       m_nEvenOddFlag = 0;                                              // 奇偶标志，0为偶数层，1为奇数层

        public double[]   m_dbDiameterInMM = new double[3];              // 定位孔直径，单位mm
        public int[]          m_nTopBrightness = new int[3];
        public int[]          m_nBottomBrightness = new int[3];
        public bool[]       m_bIsTopLightOn = new bool[3];
        public bool[]       m_bIsBottomLightOn = new bool[3];
        
        public Point2d[]   m_marks_pt_on_graph = new Point2d[3];
        public Point3d[]   m_marks_pt_on_stage = new Point3d[3];
    }

    public struct ODBMeasureItem
    {
        public ODBMeasureItem(int n)
        {
            idx = 0;
            nMeasureType = MEASURE_TYPE.NONE;
            dbGraphCrdX = 0;
            dbGraphCrdY = 0;
            dbStandardValue = 0;
            dbUpper = 0;
            dbLower = 0;
        }

        public int idx;

        public MEASURE_TYPE nMeasureType;

        public double dbGraphCrdX;
        public double dbGraphCrdY;
        public double dbStandardValue;
        public double dbUpper;
        public double dbLower;
    }

    public class MeasurePointData
    {
        public MeasurePointData()
        {
            m_mes_type = MEASURE_TYPE.NONE;
            m_graphic_type = enum_graphic_element_type.none;
            
            m_name = "";

            m_line_width_in_pixels = 0;
            m_line_width_in_metric = 0;
            m_line_angle = 0;

            m_center_x_on_graph = 0;
            m_center_y_on_graph = 0;
            m_center_x_in_metric = 0;
            m_center_y_in_metric = 0;
            m_pixels_per_mm = 1;

            m_line_start.x = 0;
            m_line_start.y = 0;
            m_line_end.x = 0;
            m_line_end.y = 0;
            m_circle_center.x = 0;
            m_circle_center.y = 0;
            
            m_nOrientation = 0;
            m_nGraphBytesPerLine = 0;
            m_nGraphWidth = 0;
            m_nGraphHeight = 0;
            m_nGraphOffsetX = 0;
            m_nGraphOffsetY = 0;
            m_len_ratio = 0;
            
            m_bIsNormalLine = false;

            m_nVersion = 100;

            m_ID = 1;

            m_create_mode = 0;
            
            m_nTopBrightness = 1;
            m_nBottomBrightness = 1;

            m_bIsTopLightOn = false;
            m_bIsBottomLightOn = false;

            m_bAutoFindMarkCircleInGuideCam = true;

            m_theory_machine_crd = new Point3d(0, 0, 0);
            m_real_machine_crd = new Point3d(0, 0, 0);

            for (int k = 0; k < 8; k++)
            {
                m_metric_radius[k] = 0;
                m_metric_radius_upper[k] = 0;
                m_metric_radius_lower[k] = 0;
                m_metric_line_width[k] = 0;
                m_metric_line_width_upper[k] = 0;
                m_metric_line_width_lower[k] = 0;
            }

            for (int k = 0; k < 4; k++)
            {
                m_handmade_ROI_rect[k].x = 0;
                m_handmade_ROI_rect[k].y = 0;
                m_graphmade_ROI_rect[k].x = 0;
                m_graphmade_ROI_rect[k].y = 0;
            }

            for (int k = 0; k < 10; k++)
            {
                m_contour_vertex[k].x = 0;
                m_contour_vertex[k].y = 0;
            }
        }
        
        public MEASURE_TYPE m_mes_type;
        public enum_graphic_element_type m_graphic_type;

        public string m_name;
        public string m_strGraphFileName = "";
        public string m_strStepsFileName = "";
        public string m_strLayerFileName = "";

        public double m_line_width_in_pixels;
        public double m_line_width_in_metric;
        public double m_line_angle = 0;
        public double m_line_angle_on_graph = 0;

        public double m_center_x_on_graph;
        public double m_center_y_on_graph;
        public double m_center_x_in_metric;
        public double m_center_y_in_metric;

        public double m_pixels_per_mm;

        public Point2d m_line_start;
        public Point2d m_line_end;
        public Point2d m_circle_center;

        public int m_nOrientation;               // 记录8个方向中选择了哪个方向
        public int m_nGraphBytesPerLine;
        public int m_nGraphWidth;
        public int m_nGraphHeight;
        public int m_nGraphOffsetX;
        public int m_nGraphOffsetY;
        public int m_len_ratio;
        public int m_unit = 0;
        public int m_nGraphZoomRatio = 1;

        public bool m_bIsNormalLine;
        public bool m_bIsDrawnByHand = false;                      // 是否是在图纸上用手拉出来的测量框

        public bool m_bIsPartOfComboMeasure = false;          // 是否是构成组合测量的一部分

        public bool m_bIsMadeForOfflineFile = false;
        public bool m_bHasBeenMeasured = false;

        public bool m_bIsFromODBAttribute = false;               // 是否来自于ODB属性
        public bool m_bIsInvalidItem = false;

        public int m_nAlgorithm = 0;         // 0为标准算法，1为陶瓷板、白底板专用算法

        public int m_ID;

        public int m_nVersion;

        public int m_create_mode;              // 首件制作方式，0为手动创建，1为配合gerber创建，2为配合ODB++创建，3为基于txt坐标文件建立（并且无图纸）
        
        public int m_nArrayOrderIdx = -1; // 阵列模式下测量点的批次序号

        public int m_nTopBrightness;         // 上环光亮度
        public int m_nBottomBrightness;   // 下环光亮度

        public bool m_bIsTopLightOn;       // 上环光是否打亮
        public bool m_bIsBottomLightOn; // 下环光是否打亮

        public bool m_bAutoFindMarkCircleInGuideCam;        // 寻找定位孔时，是否先在导航图像里对定位孔进行粗定位

        public Point3d m_theory_machine_crd;     // 首件的机械坐标（手动模式）

        public Point3d m_real_machine_crd;         // 自动测量时的实际机械坐标（手动模式）

        public double m_camera_exposure = 10;  // 相机曝光时间，单位毫秒
        public double m_camera_gain = 10;          // 相机增益

        public double[] m_metric_radius = new double[8];             // 圆或定位孔的物理半径，单位mm
        public double[] m_metric_radius_upper = new double[8];
        public double[] m_metric_radius_lower = new double[8];

        public double[] m_metric_line_width = new double[8];       // 线宽或线距，单位mm
        public double[] m_metric_line_width_upper = new double[8];
        public double[] m_metric_line_width_lower = new double[8];

        public Point2d[] m_handmade_ROI_rect = new Point2d[4];         // 待测线的ROI包围矩形（非垂直外包矩形），使用时需先转换为图像坐标
        public Point2d[] m_graphmade_ROI_rect = new Point2d[4];        // 图纸物理坐标
        public Point2d[] m_fit_rect_on_graph = new Point2d[4];              // 图纸图片坐标

        public Point2d[] m_contour_vertex = new Point2d[10];                // 线的两个端点

        public int m_thres_for_skipping_autofocus = 88;
        public double m_sharpness_at_creation = 50;                             // 制作首件时该测量项的图像清晰度

        public MeasurePointData cloneClass()
        {
            return (MeasurePointData)this.MemberwiseClone();
        }
    }

    public struct Point2d
    {
        public Point2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public void set(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public double x;
        public double y;
    }

    public struct Point2f
    {
        public Point2f(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public Point2f(double x, double y)
        {
            this.x = (float)x;
            this.y = (float)y;
        }
        public void set(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public float x;
        public float y;
    }

    public struct Point3d
    {
        public Point3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public void set(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public double x;
        public double y;
        public double z;
    }

    public class APP_Params
    {
        public int m_small_search_frame_extent_value = 3;
        public int m_big_search_frame_extent_value = 2;

        public int m_graph_zoom_ratio = 80000;                      // 图纸放大系数

        public int m_create_task_mode = 0;                             // 首件制作方式，0为配合图纸创建，1为手动创建

        public int m_jog_speed_ratio = 2;

        public int m_line_measurement_method = 0;               // 0为按平行线段测量，1为按非平行线段测量，2为按实际轮廓测量

        public int m_task_info_source_type = 1;                      // 任务获取来源类型，0为从数据库获取，1为从文件目录获取

        public int m_nMainCamUpperBrightness = 200;          // 图纸模式做首件时主图像的亮度上限
        public int m_nMainCamLowerBrightness = 150;          // 图纸模式做首件时主图像的亮度下限
        public int m_nGuideCamUpperBrightness = 200;        // 图纸模式做首件时导航图像的亮度上限
        public int m_nGuideCamLowerBrightness = 150;        // 图纸模式做首件时导航图像的亮度下限
        public int m_nLightTypeForGuideCamForMarkPt = 0; // 图纸模式做首件时，在导航图像中寻找定位孔所使用的光源类型，0为上环光，1为下环光
        public int m_nLightTypeFor14Line = 0;                        // 图纸模式做首件时，测量下线宽默认使用的光源类型，0为上环光，1为下环光
        public int m_nLightTypeForLineSpace = 0;                  // 图纸模式做首件时，测量线距默认使用的光源类型，0为上环光，1为下环光
        public int m_nLightTypeForBGA = 0;                           // 图纸模式做首件时，测量BGA默认使用的光源类型，0为上环光，1为下环光
        public int m_nSourceOfStandardValue;                         // 图纸测量项标准值的来源，0为来自图纸原始数据，1为来自图纸成像识别
        public bool m_bAutoAdjustLightDuringTaskCreation = true;     // 做首件时是否自动调节光源

        public int m_unit = 0;                                                    // 当前所用单位

        public int m_graph_orientation = 0;                              // 图纸方向

        public int[] m_measure_result_digits = new int[3];      // 测量结果小数点位数

        public bool m_bAbsoluteAllowance = true;                  // 上下限修改方式：true为按绝对值，false为按百分比

        public bool m_bNeedConfirmFiducialMark = true;       // 自动测量时是否需要确认定位孔
        public bool m_bNeedConfirmMeasureResult = true;   // 自动测量时是否需要确认测量结果
        public bool m_bUseAutofocusWhenRunningTask = true;   // 自动测量时是否启用自动对焦

        public bool m_do_not_confirm_mark_at_creation = false;                    // 图纸模式做首件时，不需要用户确认定位孔
        public bool m_do_not_confirm_measure_item_at_creation = false;      // 图纸模式做首件时，不需要用户确认测量项
        
        public bool m_use_height_sensor = false;             // 自动对焦时是否使用高度传感器
        public double m_stage_height_gap = 0;                // 平台触发高度和对焦清晰平面高度之间的差值
        public double m_stage_trigger_height = 0;           // 平台触发高度
        public double m_dbCameraHeightSensorOffsetX = 10;  // 相机与高度传感器之间的偏移X
        public double m_dbCameraHeightSensorOffsetY = 10;  // 相机与高度传感器之间的偏移Y

        public bool m_use_red_dot = false;                       // 使用红点对位
        public double m_red_dot_offset_x = 0;                 // 红点对位X偏移
        public double m_red_dot_offset_y = 0;                 // 红点对位Y偏移

        public double m_delay_seconds_before_measure = 0.2;         // 自动测量时，到达点位后的测量延迟时间，单位秒

        Point2d[]   m_len_ratios_offsets = new Point2d[10];               // 不同倍率之间的偏移，以70倍为基准

        public int m_measure_task_delay_time = 1000;   // 开始运行任务时，为确保吸附到位，第一次测量延迟时间(毫秒)

        public bool m_selectively_skip_autofocus = true;
        public int m_thres_for_skipping_autofocus = 88;

        public bool m_bSaveGaugeResultImage = true;
        public bool m_bSaveGaugeResultExcelReport = true;
        
        public bool m_bOfflineMode = true;

        public bool m_bUseDatabase = true;

        public double m_pcb_alignment_offset_x = 0;
        public double m_pcb_alignment_offset_y = 0;

        public String m_str_task_file_saving_dir = "";
        public String m_str_image_saving_dir = "";
        public String m_str_excel_saving_dir = "";
        public String m_str_graph_browse_dir = "";
        public String m_str_offline_file_browse_dir = "";
        public String m_str_open_image_browse_dir = "";
        public String m_str_save_image_browse_dir = "";
        public String m_str_data_source = "";
        public String m_str_database_task = "";
        public String m_str_database_stdlib = "";
        public String m_str_SQL_user = "";
        public String m_str_SQL_pwd = "";

        public APP_Params()
        {
            for (int n = 0; n < m_len_ratios_offsets.Length; n++)
                m_len_ratios_offsets[n] = new Point2d(0, 0);

            for (int n = 0; n < m_measure_result_digits.Length; n++)
                m_measure_result_digits[n] = 3;
        }
    }

    public partial class MainUI : Form
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder returned_value, int nSize, string path);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string path);

        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_is_green_point_in_gerber(int x, int y, int[] ret_values);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_is_green_point_in_ODB(int x, int y, int[] ret_values);

        // ret_ints[] 0是成功标志，1是线的类型，2是图纸每行数据所占字节数，3是offset_x，4是offset_y，5和6是graph的宽和高
        // ret_doubles[] 0和1是起点，2和3是终点，4和5是中心点，6是线宽或半径(单位像素)，7是线宽或半径(单位mm)，8是线的角度，9是pixels_per_mm
        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_line_width_in_gerber(MEASURE_TYPE mes_type, int crd_x, int crd_y, int[] ret_ints, double[] ret_doubles);

        // ret_ints[] 0是成功标志，1是线的类型，2是图纸每行数据所占字节数，3是offset_x，4是offset_y，5和6是graph的宽和高
        // ret_doubles[] 0和1是起点，2和3是终点，4和5是中心点，6是线宽或半径(单位像素)，7是线宽或半径(单位mm)，8是线的角度，9是pixels_per_mm
        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_circle_info_in_gerber_hv(MEASURE_TYPE mes_type, int crd_x, int crd_y, int[] ret_ints, double[] ret_doubles);

        // ret_ints[] 0是成功标志，1是线的类型，2是图纸每行数据所占字节数，3是offset_x，4是offset_y，5和6是graph的宽和高
        // ret_doubles[] 0和1是起点，2和3是终点，4和5是中心点，6是线宽或半径(单位像素)，7是线宽或半径(单位mm)，8是线的角度，9是pixels_per_mm
        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_circle_info_in_ODB_hv(MEASURE_TYPE mes_type, int crd_x, int crd_y, int[] ret_ints, double[] ret_doubles);
        
        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_line_space_info_in_gerber_hv(MEASURE_TYPE mes_type, int crd_x, int crd_y, int[] ret_ints, double[] ret_doubles);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_line_space_info_in_ODB_hv(MEASURE_TYPE mes_type, bool bIsFromODBAttribute, double dbStandardValue, int crd_x, int crd_y, int[] ret_ints, double[] ret_doubles);

        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_line_info_in_gerber_hv(MEASURE_TYPE mes_type, int crd_x, int crd_y, int[] ret_ints, double[] ret_doubles);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_line_info_in_ODB_hv(MEASURE_TYPE mes_type, bool bIsFromODBAttribute, double dbStandardValue, int crd_x, int crd_y, int[] ret_ints, double[] ret_doubles);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_line_info_in_ODB(MEASURE_TYPE mes_type, int crd_x, int crd_y, int[] ret_ints, double[] ret_doubles);

        const int TASK_COLUMN_IDX = 0;
        const int TASK_COLUMN_TYPE = 1;
        const int TASK_COLUMN_NAME = 2;
        const int TASK_COLUMN_STD_VALUE = 3;
        const int TASK_COLUMN_UPPER = 4;
        const int TASK_COLUMN_LOWER = 5;
        const int TASK_COLUMN_UNIT = 6;
        const int TASK_COLUMN_RATIO = 7;
        const int TASK_COLUMN_TOP_LIGHT = 8;
        const int TASK_COLUMN_BOTTOM_LIGHT = 9;
        const int TASK_COLUMN_THEORY_CRD = 10;

        const int RESULT_COLUMN_IDX = 0;
        const int RESULT_COLUMN_TYPE = 1;
        const int RESULT_COLUMN_NAME = 2;
        const int RESULT_COLUMN_STD_VALUE = 3;
        const int RESULT_COLUMN_UPPER = 4;
        const int RESULT_COLUMN_LOWER = 5;
        const int RESULT_COLUMN_UNIT = 6;
        const int RESULT_COLUMN_GAUGED_VALUE = 7;
        const int RESULT_COLUMN_OKNG = 8;
        const int RESULT_COLUMN_RATIO = 9;

        public const int GRAPH_Y_OFFSET_SUM = 200;

        static bool   m_bIsJogThreadStarted = false;
        static bool   m_bJogAxis = false;
        static short  m_nJogAxis = 1;
        static short  m_nJogDir = 1;

        bool   m_bShowCrossLine = false;

        // 保存程序参数
        public void SaveAppParams()
        {
            if (m_bIsAppInited)
            {
                m_app_params.m_small_search_frame_extent_value = m_nSmallSearchFrameExtent;
                m_app_params.m_big_search_frame_extent_value = m_nBigSearchFrameExtent;
                m_app_params.m_graph_zoom_ratio = m_nGraphZoomRatio;
                m_app_params.m_graph_orientation = m_nGraphOrientation;
                m_app_params.m_create_task_mode = m_nCreateTaskMode;
                m_app_params.m_jog_speed_ratio = m_nJogSpeedRatio;
                m_app_params.m_bNeedConfirmFiducialMark = m_bNeedConfirmFiducialMark;
                m_app_params.m_bNeedConfirmMeasureResult = m_bNeedConfirmMeasureResult;
                m_app_params.m_bUseAutofocusWhenRunningTask = m_bUseAutofocusWhenRunningTask;
                m_app_params.m_bAbsoluteAllowance = m_bAbsoluteAllowance;
                m_app_params.m_pcb_alignment_offset_x = m_pcb_alignment_offset.x;
                m_app_params.m_pcb_alignment_offset_y = m_pcb_alignment_offset.y;
                m_app_params.m_str_task_file_saving_dir = m_strTaskFileSavingDir;
                m_app_params.m_str_image_saving_dir = m_strImageSavingDir;
                m_app_params.m_str_excel_saving_dir = m_strExcelSavingDir;
                m_app_params.m_str_graph_browse_dir = m_strGraphBrowseDir;
                m_app_params.m_str_offline_file_browse_dir = m_strOfflineFileBrowseDir;
                m_app_params.m_str_open_image_browse_dir = m_strOpenImageBrowseDir;
                m_app_params.m_str_save_image_browse_dir = m_strSaveImageBrowseDir;
                m_app_params.m_bSaveGaugeResultImage = m_bSaveGaugeResultImage;
                m_app_params.m_bSaveGaugeResultExcelReport = m_bSaveGaugeResultExcelReport;
                m_app_params.m_bOfflineMode = m_bOfflineMode;
                m_app_params.m_bUseDatabase = m_bUseDatabase;
                m_app_params.m_str_data_source = Form_Database.m_strDataSource;
                m_app_params.m_str_database_task = Form_Database.m_strDatabaseTask;
                m_app_params.m_str_database_stdlib = Form_Database.m_strDatabaseStdLib;
                m_app_params.m_str_SQL_user = Form_Database.m_strSQLUser;
                m_app_params.m_str_SQL_pwd = Form_Database.m_strSQLPwd;

                m_app_params.m_unit = m_nUnitType;

                m_app_params.m_line_measurement_method = m_nLineMeasurementMethod;

                m_app_params.m_nSourceOfStandardValue = m_nSourceOfStandardValue;

                m_app_params.m_measure_result_digits = m_nMeasureResultDigits;

                m_app_params.m_task_info_source_type = m_nTaskInfoSourceType;

                m_app_params.m_use_red_dot = m_bUseRedDot;
                m_app_params.m_red_dot_offset_x = m_dbRedDotOffsetX;
                m_app_params.m_red_dot_offset_y = m_dbRedDotOffsetY;

                m_app_params.m_measure_task_delay_time = m_nMeasureTaskDelayTime;
                m_app_params.m_delay_seconds_before_measure = m_dbDelaySecondsBeforeMeasure;

                m_app_params.m_use_height_sensor = m_bUseHeightSensor;
                m_app_params.m_stage_height_gap = m_dbStageHeightGap;
                m_app_params.m_stage_trigger_height = m_dbStageTriggerHeight;
                m_app_params.m_dbCameraHeightSensorOffsetX = m_dbCameraHeightSensorOffsetX;
                m_app_params.m_dbCameraHeightSensorOffsetY = m_dbCameraHeightSensorOffsetY;

                m_app_params.m_selectively_skip_autofocus = m_bSelectivelySkipAutofocus;
                m_app_params.m_thres_for_skipping_autofocus = m_nThresForSkippingAutofocus;

                m_app_params.m_do_not_confirm_mark_at_creation = m_bDoNotConfirmMarkAtCreation;
                m_app_params.m_do_not_confirm_measure_item_at_creation = m_bDoNotConfirmMeasureItemAtCreation;
                
                m_app_params.m_nMainCamUpperBrightness = m_nMainCamUpperBrightness;
                m_app_params.m_nMainCamLowerBrightness = m_nMainCamLowerBrightness;
                m_app_params.m_nGuideCamUpperBrightness = m_nGuideCamUpperBrightness;
                m_app_params.m_nGuideCamLowerBrightness = m_nGuideCamLowerBrightness;
                m_app_params.m_nLightTypeForGuideCamForMarkPt = m_nLightTypeForGuideCamForMarkPt;
                m_app_params.m_nLightTypeFor14Line = m_nLightTypeFor14Line;
                m_app_params.m_nLightTypeForLineSpace = m_nLightTypeForLineSpace;
                m_app_params.m_nLightTypeForBGA = m_nLightTypeForBGA;
                m_app_params.m_bAutoAdjustLightDuringTaskCreation = m_bAutoAdjustLightDuringTaskCreation;
                
                string filename = "configs\\sys.ini";

                for (int n = 1; n <= 5; n++)
                {
                    WritePrivateProfileString("镜头倍率偏移", string.Format("{0}00倍x", n), m_len_ratios_offsets[n].x.ToString(), filename);
                    WritePrivateProfileString("镜头倍率偏移", string.Format("{0}00倍y", n), m_len_ratios_offsets[n].y.ToString(), filename);
                }

                WritePrivateProfileString("数据库", "启用数据库", m_app_params.m_bUseDatabase.ToString(), filename);
                WritePrivateProfileString("数据库", "数据源", m_app_params.m_str_data_source, filename);
                WritePrivateProfileString("数据库", "测量任务数据库", m_app_params.m_str_database_task, filename);
                WritePrivateProfileString("数据库", "标准库数据库", m_app_params.m_str_database_stdlib, filename);
                WritePrivateProfileString("数据库", "数据库用户名", m_app_params.m_str_SQL_user, filename);
                WritePrivateProfileString("数据库", "数据库密码", m_app_params.m_str_SQL_pwd, filename);

                WritePrivateProfileString("模式设置", "离线模式", m_app_params.m_bOfflineMode.ToString(), filename);
                WritePrivateProfileString("模式设置", "首件制作方式", m_app_params.m_create_task_mode.ToString(), filename);

                WritePrivateProfileString("图纸", "图纸初始浏览目录", m_app_params.m_str_graph_browse_dir.ToString(), filename);
                WritePrivateProfileString("图纸", "图纸方向", m_app_params.m_graph_orientation.ToString(), filename);
                WritePrivateProfileString("图纸", "图纸放大比例", m_app_params.m_graph_zoom_ratio.ToString(), filename);

                WritePrivateProfileString("红点辅助", "启用红点对位", m_app_params.m_use_red_dot.ToString(), filename);
                WritePrivateProfileString("红点辅助", "红点对位X偏移", m_app_params.m_red_dot_offset_x.ToString(), filename);
                WritePrivateProfileString("红点辅助", "红点对位Y偏移", m_app_params.m_red_dot_offset_y.ToString(), filename);

                WritePrivateProfileString("杂项", "上下限修改方式", m_app_params.m_bAbsoluteAllowance.ToString(), filename);
                WritePrivateProfileString("杂项", "寸动速率", m_app_params.m_jog_speed_ratio.ToString(), filename);
                WritePrivateProfileString("杂项", "当前所用单位", m_app_params.m_unit.ToString(), filename);
                WritePrivateProfileString("杂项", "测量方式", m_app_params.m_line_measurement_method.ToString(), filename);
                WritePrivateProfileString("杂项", "测量结果小数点位数mm", m_app_params.m_measure_result_digits[0].ToString(), filename);
                WritePrivateProfileString("杂项", "测量结果小数点位数um", m_app_params.m_measure_result_digits[1].ToString(), filename);
                WritePrivateProfileString("杂项", "测量结果小数点位数mil", m_app_params.m_measure_result_digits[2].ToString(), filename);
                WritePrivateProfileString("杂项", "第一次测量延迟时间", m_app_params.m_measure_task_delay_time.ToString(), filename);
                WritePrivateProfileString("杂项", "小搜索框大小", m_app_params.m_small_search_frame_extent_value.ToString(), filename);
                WritePrivateProfileString("杂项", "大搜索框大小", m_app_params.m_big_search_frame_extent_value.ToString(), filename);
                WritePrivateProfileString("杂项", "打开图片初始浏览目录", m_app_params.m_str_open_image_browse_dir.ToString(), filename);
                WritePrivateProfileString("杂项", "保存图片初始浏览目录", m_app_params.m_str_save_image_browse_dir.ToString(), filename);
                WritePrivateProfileString("杂项", "相机品牌", m_nCameraType.ToString(), filename);

                WritePrivateProfileString("自动对焦相关", "启用自动对焦", m_app_params.m_bUseAutofocusWhenRunningTask.ToString(), filename);
                WritePrivateProfileString("自动对焦相关", "选择性跳过自动对焦", m_app_params.m_selectively_skip_autofocus.ToString(), filename);
                WritePrivateProfileString("自动对焦相关", "跳过自动对焦所需百分比阈值", m_app_params.m_thres_for_skipping_autofocus.ToString(), filename);
                WritePrivateProfileString("自动对焦相关", "自动对焦时是否使用高度传感器", m_app_params.m_use_height_sensor.ToString(), filename);
                WritePrivateProfileString("自动对焦相关", "平台触发高度", m_app_params.m_stage_trigger_height.ToString(), filename);
                WritePrivateProfileString("自动对焦相关", "平台触发高度和对焦清晰平面高度之间的差值", m_app_params.m_stage_height_gap.ToString(), filename);
                WritePrivateProfileString("自动对焦相关", "相机与高度传感器之间的偏移X", m_app_params.m_dbCameraHeightSensorOffsetX.ToString(), filename);
                WritePrivateProfileString("自动对焦相关", "相机与高度传感器之间的偏移Y", m_app_params.m_dbCameraHeightSensorOffsetY.ToString(), filename);

                WritePrivateProfileString("首件相关", "PCB实物板和图纸对齐点偏移量X", m_app_params.m_pcb_alignment_offset_x.ToString(), filename);
                WritePrivateProfileString("首件相关", "PCB实物板和图纸对齐点偏移量Y", m_app_params.m_pcb_alignment_offset_y.ToString(), filename);
                WritePrivateProfileString("首件相关", "图纸模式做首件时不需要用户确认定位孔", m_app_params.m_do_not_confirm_mark_at_creation.ToString(), filename);
                WritePrivateProfileString("首件相关", "图纸模式做首件时不需要用户确认测量项", m_app_params.m_do_not_confirm_measure_item_at_creation.ToString(), filename);
                WritePrivateProfileString("首件相关", "图纸模式做首件时主图像的亮度上限", m_app_params.m_nMainCamUpperBrightness.ToString(), filename);
                WritePrivateProfileString("首件相关", "图纸模式做首件时主图像的亮度下限", m_app_params.m_nMainCamLowerBrightness.ToString(), filename);
                WritePrivateProfileString("首件相关", "图纸模式做首件时导航图像的亮度上限", m_app_params.m_nGuideCamUpperBrightness.ToString(), filename);
                WritePrivateProfileString("首件相关", "图纸模式做首件时导航图像的亮度下限", m_app_params.m_nGuideCamLowerBrightness.ToString(), filename);
                WritePrivateProfileString("首件相关", "在导航图像中寻找定位孔时使用的光源类型", m_app_params.m_nLightTypeForGuideCamForMarkPt.ToString(), filename);
                WritePrivateProfileString("首件相关", "做首件时测量下线宽默认使用的光源类型", m_app_params.m_nLightTypeFor14Line.ToString(), filename);
                WritePrivateProfileString("首件相关", "做首件时测量线距默认使用的光源类型", m_app_params.m_nLightTypeForLineSpace.ToString(), filename);
                WritePrivateProfileString("首件相关", "做首件时测量BGA默认使用的光源类型", m_app_params.m_nLightTypeForBGA.ToString(), filename);
                WritePrivateProfileString("首件相关", "做首件时是否自动调节光源", m_app_params.m_bAutoAdjustLightDuringTaskCreation.ToString(), filename);
                WritePrivateProfileString("首件相关", "图纸测量项标准值的来源", m_app_params.m_nSourceOfStandardValue.ToString(), filename);
                WritePrivateProfileString("首件相关", "离线文件初始浏览目录", m_app_params.m_str_offline_file_browse_dir.ToString(), filename);
                WritePrivateProfileString("首件相关", "任务获取来源类型", m_app_params.m_task_info_source_type.ToString(), filename);
                WritePrivateProfileString("首件相关", "任务文件保存目录", m_app_params.m_str_task_file_saving_dir.ToString(), filename);
                
                WritePrivateProfileString("测量", "确认定位孔", m_app_params.m_bNeedConfirmFiducialMark.ToString(), filename);
                WritePrivateProfileString("测量", "确认测量结果", m_app_params.m_bNeedConfirmMeasureResult.ToString(), filename);
                WritePrivateProfileString("测量", "是否保存测量图片", m_app_params.m_bSaveGaugeResultImage.ToString(), filename);
                WritePrivateProfileString("测量", "测量图片保存目录", m_app_params.m_str_image_saving_dir.ToString(), filename);
                WritePrivateProfileString("测量", "自动测量结束时是否自动保存测量结果报表", m_app_params.m_bSaveGaugeResultExcelReport.ToString(), filename);
                WritePrivateProfileString("测量", "测量结果报表保存目录", m_app_params.m_str_excel_saving_dir.ToString(), filename);
                WritePrivateProfileString("测量", "测量延迟秒数", m_app_params.m_delay_seconds_before_measure.ToString(), filename);

            }
        }
        
        // 加载程序参数
        public void LoadAppParams(string path, APP_Params pr)
        {
            if (File.Exists(path))
            {
                string filename = "configs\\sys.ini";

                StringBuilder strTemp = new StringBuilder(255);

                for (int n = 1; n <= 5; n++)
                {
                    GetPrivateProfileString("镜头倍率偏移", string.Format("{0}00倍x", n), "0", strTemp, 255, filename);
                    if (strTemp.Length > 0)
                        m_len_ratios_offsets[n].x = Convert.ToDouble(strTemp.ToString());
                    GetPrivateProfileString("镜头倍率偏移", string.Format("{0}00倍y", n), "0", strTemp, 255, filename);
                    if (strTemp.Length > 0)
                        m_len_ratios_offsets[n].y = Convert.ToDouble(strTemp.ToString());
                }

                GetPrivateProfileString("数据库", "启用数据库", "True", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_bUseDatabase = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("数据库", "数据源", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_data_source = strTemp.ToString();
                GetPrivateProfileString("数据库", "测量任务数据库", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_database_task = strTemp.ToString();
                GetPrivateProfileString("数据库", "标准库数据库", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_database_stdlib = strTemp.ToString();
                GetPrivateProfileString("数据库", "数据库用户名", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_SQL_user = strTemp.ToString();
                GetPrivateProfileString("数据库", "数据库密码", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_SQL_pwd = strTemp.ToString();

                GetPrivateProfileString("模式设置", "离线模式", "False", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_bOfflineMode = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("模式设置", "首件制作方式", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_create_task_mode = Convert.ToInt32(strTemp.ToString());

                GetPrivateProfileString("图纸", "图纸初始浏览目录", "", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_graph_browse_dir = strTemp.ToString();
                GetPrivateProfileString("图纸", "图纸方向", "", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_graph_orientation = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("图纸", "图纸放大比例", "", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_graph_zoom_ratio = Convert.ToInt32(strTemp.ToString());

                GetPrivateProfileString("红点辅助", "启用红点对位", "False", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_use_red_dot = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("红点辅助", "红点对位X偏移", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_red_dot_offset_x = Convert.ToDouble(strTemp.ToString());
                GetPrivateProfileString("红点辅助", "红点对位Y偏移", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_red_dot_offset_y = Convert.ToDouble(strTemp.ToString());

                GetPrivateProfileString("杂项", "上下限修改方式", "False", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_bAbsoluteAllowance = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("杂项", "寸动速率", "2", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_jog_speed_ratio = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("杂项", "当前所用单位", "2", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_unit = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("杂项", "测量结果小数点位数mm", "3", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_measure_result_digits[0] = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("杂项", "测量结果小数点位数um", "3", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_measure_result_digits[1] = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("杂项", "测量结果小数点位数mil", "3", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_measure_result_digits[2] = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("杂项", "测量方式", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_line_measurement_method = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("杂项", "第一次测量延迟时间", "2000", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_measure_task_delay_time = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("杂项", "小搜索框大小", "3", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_small_search_frame_extent_value = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("杂项", "大搜索框大小", "3", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_big_search_frame_extent_value = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("杂项", "打开图片初始浏览目录", "", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_open_image_browse_dir = strTemp.ToString();
                GetPrivateProfileString("杂项", "保存图片初始浏览目录", "", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_save_image_browse_dir = strTemp.ToString();
                GetPrivateProfileString("杂项", "相机品牌", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_nCameraType = Convert.ToInt32(strTemp.ToString());
                
                GetPrivateProfileString("自动对焦相关", "启用自动对焦", "True", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_bUseAutofocusWhenRunningTask = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("自动对焦相关", "选择性跳过自动对焦", "True", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_selectively_skip_autofocus = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("自动对焦相关", "跳过自动对焦所需百分比阈值", "90", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_thres_for_skipping_autofocus = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("自动对焦相关", "自动对焦时是否使用高度传感器", "True", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_use_height_sensor = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("自动对焦相关", "平台触发高度", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_stage_trigger_height = Convert.ToDouble(strTemp.ToString());
                GetPrivateProfileString("自动对焦相关", "平台触发高度和对焦清晰平面高度之间的差值", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_stage_height_gap = Convert.ToDouble(strTemp.ToString());
                GetPrivateProfileString("自动对焦相关", "相机与高度传感器之间的偏移X", "10", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_dbCameraHeightSensorOffsetX = Convert.ToDouble(strTemp.ToString());
                GetPrivateProfileString("自动对焦相关", "相机与高度传感器之间的偏移Y", "10", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_dbCameraHeightSensorOffsetY = Convert.ToDouble(strTemp.ToString());

                GetPrivateProfileString("首件相关", "PCB实物板和图纸对齐点偏移量X", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_pcb_alignment_offset_x = Convert.ToDouble(strTemp.ToString());
                GetPrivateProfileString("首件相关", "PCB实物板和图纸对齐点偏移量Y", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_pcb_alignment_offset_y = Convert.ToDouble(strTemp.ToString());
                GetPrivateProfileString("首件相关", "图纸模式做首件时不需要用户确认定位孔", "False", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_do_not_confirm_mark_at_creation = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("首件相关", "图纸模式做首件时不需要用户确认测量项", "False", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_do_not_confirm_measure_item_at_creation = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("首件相关", "图纸模式做首件时主图像的亮度上限", "200", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_nMainCamUpperBrightness = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("首件相关", "图纸模式做首件时主图像的亮度下限", "150", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_nMainCamLowerBrightness = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("首件相关", "图纸模式做首件时导航图像的亮度上限", "200", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_nGuideCamUpperBrightness = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("首件相关", "图纸模式做首件时导航图像的亮度下限", "150", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_nGuideCamLowerBrightness = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("首件相关", "在导航图像中寻找定位孔时使用的光源类型", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_nLightTypeForGuideCamForMarkPt = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("首件相关", "做首件时测量下线宽默认使用的光源类型", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_nLightTypeFor14Line = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("首件相关", "做首件时测量线距默认使用的光源类型", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_nLightTypeForLineSpace = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("首件相关", "做首件时测量BGA默认使用的光源类型", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_nLightTypeForBGA = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("首件相关", "做首件时是否自动调节光源", "True", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_bAutoAdjustLightDuringTaskCreation = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("首件相关", "图纸测量项标准值的来源", "0", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_nSourceOfStandardValue = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("首件相关", "离线文件初始浏览目录", "", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_offline_file_browse_dir = strTemp.ToString();
                GetPrivateProfileString("首件相关", "任务获取来源类型", "1", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_task_info_source_type = Convert.ToInt32(strTemp.ToString());
                GetPrivateProfileString("首件相关", "任务文件保存目录", "", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_task_file_saving_dir = strTemp.ToString();
                
                GetPrivateProfileString("测量", "确认定位孔", "True", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_bNeedConfirmFiducialMark = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("测量", "确认测量结果", "True", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_bNeedConfirmMeasureResult = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("测量", "是否保存测量图片", "False", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_bSaveGaugeResultImage = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("测量", "测量图片保存目录", "", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_image_saving_dir = strTemp.ToString();
                GetPrivateProfileString("测量", "自动测量结束时是否自动保存测量结果报表", "False", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_bSaveGaugeResultExcelReport = Convert.ToBoolean(strTemp.ToString());
                GetPrivateProfileString("测量", "测量结果报表保存目录", "", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_str_excel_saving_dir = strTemp.ToString();
                GetPrivateProfileString("测量", "测量延迟秒数", "0.2", strTemp, 255, filename);
                if (strTemp.Length > 0)
                    m_app_params.m_delay_seconds_before_measure = Convert.ToDouble(strTemp.ToString());

                
                m_nGraphZoomRatio = m_app_params.m_graph_zoom_ratio;
                m_nGraphOrientation = m_app_params.m_graph_orientation;
                
                m_nSmallSearchFrameExtent = m_app_params.m_small_search_frame_extent_value;
                m_nBigSearchFrameExtent = m_app_params.m_big_search_frame_extent_value;

                m_pcb_alignment_offset.x = m_app_params.m_pcb_alignment_offset_x;
                m_pcb_alignment_offset.y = m_app_params.m_pcb_alignment_offset_y;

                m_bUseHeightSensor = m_app_params.m_use_height_sensor;
                m_dbStageHeightGap = m_app_params.m_stage_height_gap;
                m_dbStageTriggerHeight = m_app_params.m_stage_trigger_height;
                m_dbCameraHeightSensorOffsetX = m_app_params.m_dbCameraHeightSensorOffsetX;
                m_dbCameraHeightSensorOffsetY = m_app_params.m_dbCameraHeightSensorOffsetY;

                m_dbDelaySecondsBeforeMeasure = m_app_params.m_delay_seconds_before_measure;

                m_nTaskInfoSourceType = m_app_params.m_task_info_source_type;

                m_bUseRedDot = m_app_params.m_use_red_dot;
                m_dbRedDotOffsetX = m_app_params.m_red_dot_offset_x;
                m_dbRedDotOffsetY = m_app_params.m_red_dot_offset_y;

                m_nMeasureTaskDelayTime = m_app_params.m_measure_task_delay_time;

                //m_nCreateTaskMode = m_app_params.m_create_task_mode;

                m_nUnitType = m_app_params.m_unit;

                m_nMeasureResultDigits = m_app_params.m_measure_result_digits;

                m_nLineMeasurementMethod = m_app_params.m_line_measurement_method;

                m_nJogSpeedRatio = m_app_params.m_jog_speed_ratio;

                m_nMainCamUpperBrightness = m_app_params.m_nMainCamUpperBrightness;
                m_nMainCamLowerBrightness = m_app_params.m_nMainCamLowerBrightness;
                m_nGuideCamUpperBrightness = m_app_params.m_nGuideCamUpperBrightness;
                m_nGuideCamLowerBrightness = m_app_params.m_nGuideCamLowerBrightness;
                m_nLightTypeForGuideCamForMarkPt = m_app_params.m_nLightTypeForGuideCamForMarkPt;
                m_nLightTypeFor14Line = m_app_params.m_nLightTypeFor14Line;
                m_nLightTypeForLineSpace = m_app_params.m_nLightTypeForLineSpace;
                m_nLightTypeForBGA = m_app_params.m_nLightTypeForBGA;
                m_nSourceOfStandardValue = m_app_params.m_nSourceOfStandardValue;
                m_bAutoAdjustLightDuringTaskCreation = m_app_params.m_bAutoAdjustLightDuringTaskCreation;

                m_bNeedConfirmFiducialMark = m_app_params.m_bNeedConfirmFiducialMark;
                m_bNeedConfirmMeasureResult = m_app_params.m_bNeedConfirmMeasureResult;
                m_bUseAutofocusWhenRunningTask = m_app_params.m_bUseAutofocusWhenRunningTask;
                m_bDoNotConfirmMarkAtCreation = m_app_params.m_do_not_confirm_mark_at_creation;
                m_bDoNotConfirmMeasureItemAtCreation = m_app_params.m_do_not_confirm_measure_item_at_creation;
                
                m_strTaskFileSavingDir = m_app_params.m_str_task_file_saving_dir;
                m_strImageSavingDir = m_app_params.m_str_image_saving_dir;
                m_strExcelSavingDir = m_app_params.m_str_excel_saving_dir;
                m_strGraphBrowseDir = m_app_params.m_str_graph_browse_dir;
                m_strOpenImageBrowseDir = m_app_params.m_str_open_image_browse_dir;
                m_strSaveImageBrowseDir = m_app_params.m_str_save_image_browse_dir;
                m_bSaveGaugeResultImage = m_app_params.m_bSaveGaugeResultImage;
                m_bSaveGaugeResultExcelReport = m_app_params.m_bSaveGaugeResultExcelReport;
                m_bOfflineMode = m_app_params.m_bOfflineMode;
                m_bUseDatabase = m_app_params.m_bUseDatabase;

                //Debugger.Log(0, null, string.Format("222222 m_strTaskFileSavingDir = {0}", m_strTaskFileSavingDir));
                
                m_bAbsoluteAllowance = m_app_params.m_bAbsoluteAllowance;

                m_bSelectivelySkipAutofocus = m_app_params.m_selectively_skip_autofocus;
                m_nThresForSkippingAutofocus = m_app_params.m_thres_for_skipping_autofocus;

                m_strOfflineFileBrowseDir = m_app_params.m_str_offline_file_browse_dir;

                Form_Database.m_strDataSource = m_app_params.m_str_data_source;
            }

            return;
        }

        // 加载标定数据
        public void LoadCalibData(string path, ref double[] data)
        {
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                string value = "";

                string[] ratios = new string[] { "70X", "100X", "200X", "300X", "400X", "500X" };
                for (int n = 0; n < ratios.Length; n++)
                {
                    if (true == GeneralUtils.GetKeyValue(content, ratios[n], ref value))
                        data[n] = Convert.ToDouble(value);
                }
            }

            return;
        }

        // 保存标定数据
        public void SaveCalibData(string path, double[] data)
        {
            if (m_bIsAppInited)
            {
                StreamWriter writer = new StreamWriter(path, false);

                string[] ratios = new string[] { "70X", "100X", "200X", "300X", "400X", "500X" };
                for (int n = 0; n < ratios.Length; n++)
                {
                    String str = String.Format("{0}={1}", ratios[n], data[n]);
                    writer.WriteLine(str);
                }

                writer.Close();
            }
        }
        
        // 从数据库中获取所有任务数据表
        public bool get_task_tables_from_database(List<string> list_names)
        {
            if ((true == m_bIsSQLConnected) && m_bUseDatabase)
            {
                DataTable tables = m_SQL_conn_measure_task.GetSchema("Tables");
                foreach (DataRow row in tables.Rows)
                {
                    list_names.Add(((string)row[2]).Substring(1));
                }

                return true;
            }
            else
                return false;
        }

        // 加载数据表
        public bool load_task_table(string table_name, List<MeasurePointData> data_holder)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = m_SQL_conn_measure_task;
            cmd.CommandText = "select * from s" + table_name;
            cmd.CommandType = CommandType.Text;

            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                MeasurePointData data = new MeasurePointData();

                data.m_ID = Convert.ToInt32(reader[0]);
                data.m_nVersion = Convert.ToInt32(reader[1]);
                data.m_create_mode = Convert.ToInt32(reader[2]);
                data.m_mes_type = (MEASURE_TYPE)(Convert.ToInt32(reader[3]));
                data.m_name = Convert.ToString(reader[4]);
                data.m_strGraphFileName = Convert.ToString(reader[5]);
                data.m_bIsTopLightOn = (0 == Convert.ToInt32(reader[6])) ? false : true;
                data.m_nTopBrightness = Convert.ToInt32(reader[7]);
                data.m_bIsBottomLightOn = (0 == Convert.ToInt32(reader[8])) ? false : true;
                data.m_nBottomBrightness = Convert.ToInt32(reader[9]);
                data.m_unit = Convert.ToInt32(reader[10]);
                data.m_len_ratio = Convert.ToInt32(reader[11]);
                data.m_camera_exposure = Convert.ToDouble(reader[12]);
                data.m_camera_gain = Convert.ToDouble(reader[13]);
                data.m_thres_for_skipping_autofocus = Convert.ToInt32(reader[14]);
                data.m_sharpness_at_creation = Convert.ToInt32(reader[15]);
                data.m_line_angle = Convert.ToDouble(reader[16]);
                data.m_line_angle_on_graph = Convert.ToDouble(reader[17]);
                data.m_theory_machine_crd.x = Convert.ToDouble(reader[18]);
                data.m_theory_machine_crd.y = Convert.ToDouble(reader[19]);
                data.m_theory_machine_crd.z = Convert.ToDouble(reader[20]);
                data.m_center_x_in_metric = Convert.ToDouble(reader[21]);
                data.m_center_y_in_metric = Convert.ToDouble(reader[22]);
                data.m_center_x_on_graph = Convert.ToDouble(reader[23]);
                data.m_center_y_on_graph = Convert.ToDouble(reader[24]);
                data.m_pixels_per_mm = Convert.ToDouble(reader[25]);
                data.m_nGraphOffsetX = Convert.ToInt32(reader[26]);
                data.m_nGraphOffsetY = Convert.ToInt32(reader[27]);
                data.m_nGraphZoomRatio = Convert.ToInt32(reader[28]);
                data.m_nOrientation = Convert.ToInt32(reader[29]);
                data.m_bIsMadeForOfflineFile = (0 == Convert.ToInt32(reader[30])) ? false : true;
                for (int n = 0; n < 8; n++)
                {
                    data.m_metric_radius[n] = Convert.ToDouble(reader[31 + n * 3]);
                    data.m_metric_radius_upper[n] = Convert.ToDouble(reader[32 + n * 3]);
                    data.m_metric_radius_lower[n] = Convert.ToDouble(reader[33 + n * 3]);
                }
                for (int n = 0; n < 8; n++)
                {
                    data.m_metric_line_width[n] = Convert.ToDouble(reader[55 + n * 3]);
                    data.m_metric_line_width_upper[n] = Convert.ToDouble(reader[56 + n * 3]);
                    data.m_metric_line_width_lower[n] = Convert.ToDouble(reader[57 + n * 3]);
                }
                for (int n = 0; n < 4; n++)
                {
                    data.m_handmade_ROI_rect[n].x = Convert.ToDouble(reader[79 + n * 2]);
                    data.m_handmade_ROI_rect[n].y = Convert.ToDouble(reader[80 + n * 2]);
                }
                for (int n = 0; n < 4; n++)
                {
                    data.m_graphmade_ROI_rect[n].x = Convert.ToDouble(reader[87 + n * 2]);
                    data.m_graphmade_ROI_rect[n].y = Convert.ToDouble(reader[88 + n * 2]);
                }
                for (int n = 0; n < 4; n++)
                {
                    data.m_fit_rect_on_graph[n].x = Convert.ToDouble(reader[95 + n * 2]);
                    data.m_fit_rect_on_graph[n].y = Convert.ToDouble(reader[96 + n * 2]);
                }

                data_holder.Add(data);
            }

            reader.Close();

            return true;
        }

        // 读取txt任务文件
        public bool read_task_txt_file(string strTaskFile, enum_customer customer, List<MeasurePointData> data_holder)
        {
            string path = strTaskFile;

            if (!File.Exists(path))
            {
                MessageBox.Show(this, string.Format("找不到任务文件 {0}，请检查原因!", path), "提示");
                return false;
            }

            StreamReader reader = new StreamReader(path, false);

            Debugger.Log(0, null, string.Format("222222 path = {0}", path));

            double[] array_ratio_widths = new double[] { 2900, 2300, 1700, 1200, 900, 550 };
            string[] array_ratios = new string[] { "70X", "100X", "200X", "300X", "400X", "500X" };

            int counter = 0;
            int nUnitIndex = 0;
            string strTaskName = "";
            while (true)
            {
                string line = reader.ReadLine();
                if (null == line)
                    break;

                bool bIsCorrectFormat = false;
                try
                {
                    if (0 == counter)
                    {
                        if (line.Contains("A/B/C"))
                        {
                            int pos1 = line.IndexOf(',');
                            if (pos1 > 0)
                            {
                                int pos2 = line.IndexOf(',', pos1 + 1);
                                if (pos2 > 0)
                                {
                                    strTaskName = line.Substring(pos1 + 1, pos2 - pos1 - 1);

                                    Debugger.Log(0, null, string.Format("222222 pos = {0}, {1}, strTaskName = {2}", pos1, pos2, strTaskName));
                                    bIsCorrectFormat = true;
                                }
                            }
                            //Debugger.Log(0, null, string.Format("222222 {0}: {1}", counter, line));
                        }
                        
                        if (false == bIsCorrectFormat)
                        {
                            Debugger.Log(0, null, string.Format("222222 首件txt格式错误"));
                            MessageBox.Show(this, "首件txt格式错误，请检查文件内容。", "提示");

                            return false;
                        }
                    }
                    else
                    {
                        if (line.Contains("Size"))                                                              // 板子尺寸
                        {

                        }
                        else if (line.Contains("F#"))                                                         // 定位孔
                        {
                            int index = Convert.ToInt32(line.ElementAt(2)) - 48;             // 阿斯克码

                            string substr = "&0 &";
                            int pos1 = line.IndexOf(substr);
                            if (pos1 > 0)
                            {
                                string info = line.Substring(pos1 + substr.Length);

                                string[] array = info.Split(',');
                                if (3 == array.Length)
                                {
                                    MeasurePointData data = new MeasurePointData();

                                    data.m_ID = data_holder.Count + 1;
                                    data.m_create_mode = 3;
                                    data.m_mes_type = MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE;
                                    data.m_unit = 1;
                                    data.m_name = string.Format("定位孔{0}", index);
                                    
                                    data.m_center_x_in_metric = Convert.ToDouble(array[0]) / 1000;
                                    data.m_center_y_in_metric = Convert.ToDouble(array[1]) / 1000;
                                    data.m_metric_radius[0] = Convert.ToDouble(array[2]);

                                    data.m_len_ratio = 0;
                                    data.m_bHasBeenMeasured = false;

                                    // 生成ROI选取框
                                    double factor = 0.7 + (double)m_nSmallSearchFrameExtent * 0.05;
                                    double radius = data.m_metric_radius[0] * factor / 1000;

                                    data.m_graphmade_ROI_rect[0].x = data.m_center_x_in_metric - (radius / 2);
                                    data.m_graphmade_ROI_rect[0].y = data.m_center_y_in_metric - (radius / 2);
                                    data.m_graphmade_ROI_rect[1].x = data.m_center_x_in_metric + (radius / 2);
                                    data.m_graphmade_ROI_rect[1].y = data.m_center_y_in_metric - (radius / 2);
                                    data.m_graphmade_ROI_rect[2].x = data.m_center_x_in_metric + (radius / 2);
                                    data.m_graphmade_ROI_rect[2].y = data.m_center_y_in_metric + (radius / 2);
                                    data.m_graphmade_ROI_rect[3].x = data.m_center_x_in_metric - (radius / 2);
                                    data.m_graphmade_ROI_rect[3].y = data.m_center_y_in_metric + (radius / 2);

                                    data_holder.Add(data);
                                }

                                //Debugger.Log(0, null, string.Format("222222 {0}: info = {1}, array.Length = {2}", counter, info, array.Length));
                            }
                            //Debugger.Log(0, null, string.Format("222222 {0}: {1}, {2}", counter, line.ElementAt(1), line.ElementAt(2)));
                        }
                        else if (line.Contains("Unit#"))                                                    // 属于哪个unit
                        {

                        }
                        else if (line.Contains("Item#"))                                                    // 测量项的序号
                        {
                            string[] array = line.Split('&');

                            if (4 == array.Length)
                            {
                                //for (int n = 0; n < array.Length; n++)
                                //{
                                //    Debugger.Log(0, null, string.Format("222222 {0}: array.Length = {1}", n, array[n]));
                                //}

                                string[] sub_array1 = array[1].Split(',');
                                string[] sub_array3= array[3].Split(',');

                                if ((5 == sub_array1.Length) && (3 == sub_array3.Length))
                                {
                                    MeasurePointData data = new MeasurePointData();

                                    data.m_ID = data_holder.Count + 1;
                                    data.m_create_mode = 3;
                                    data.m_mes_type = MEASURE_TYPE.LINE_WIDTH_14;
                                    data.m_unit = 1;
                                    data.m_name = sub_array1[0];

                                    data.m_center_x_in_metric = Convert.ToDouble(sub_array3[0]);
                                    data.m_center_y_in_metric = Convert.ToDouble(sub_array3[1]);
                                    data.m_line_angle = Convert.ToDouble(sub_array3[2]);

                                    data.m_metric_line_width_lower[0] = Convert.ToDouble(sub_array1[2]);
                                    data.m_metric_line_width[0] = Convert.ToDouble(sub_array1[3]);
                                    data.m_metric_line_width_upper[0] = Convert.ToDouble(sub_array1[4]);

                                    if (0 == m_nLightTypeFor14Line)
                                    {
                                        data.m_bIsTopLightOn = true;
                                        data.m_bIsBottomLightOn = false;
                                    }
                                    else
                                    {
                                        data.m_bIsTopLightOn = false;
                                        data.m_bIsBottomLightOn = true;
                                    }

                                    // 计算最合适的倍率
                                    double min_dist = 100000;
                                    int min_dist_index = -1;
                                    for (int k = 0; k < array_ratio_widths.Length - 1; k++)
                                    {
                                        double dist = Math.Abs(array_ratio_widths[k] - data.m_metric_line_width[0]);
                                        if (dist < min_dist)
                                        {
                                            min_dist = dist;
                                            min_dist_index = k;
                                        }
                                    }
                                    if (-1 != min_dist_index)
                                        data.m_len_ratio = min_dist_index;
                                    //if (data.m_len_ratio >= 3)
                                    //    data.m_len_ratio -= 1;

                                    // 生成ROI选取框
                                    double w_factor = 0.35 + (double)m_nSmallSearchFrameExtent / 10;
                                    double h_factor = 0.35 + (double)m_nBigSearchFrameExtent / 10;
                                    double window_width = w_factor * data.m_metric_line_width[0] / 1000;
                                    double window_height = h_factor * data.m_metric_line_width[0] / 1000;
                                    
                                    Point2d[] pts = new Point2d[4];
                                    Point2d[] new_pts = new Point2d[4];
                                    new_pts[0].x = -window_width / 2.0;
                                    new_pts[0].y = -window_height / 2.0;
                                    new_pts[1].x = window_width / 2.0;
                                    new_pts[1].y = new_pts[0].y;
                                    new_pts[2].x = new_pts[1].x;
                                    new_pts[2].y = window_height / 2.0;
                                    new_pts[3].x = new_pts[0].x;
                                    new_pts[3].y = new_pts[2].y;

                                    for (int k = 0; k < 4; k++)
                                    {
                                        double[] in_crds = new double[2];
                                        double[] out_crds = new double[2];
                                        in_crds[0] = new_pts[k].x;
                                        in_crds[1] = new_pts[k].y;
                                        rotate_crd(in_crds, out_crds, data.m_line_angle);

                                        pts[k].x = data.m_center_x_in_metric;
                                        pts[k].y = data.m_center_y_in_metric;
                                        pts[k].x += out_crds[0];
                                        pts[k].y += out_crds[1];
                                        
                                        //Debugger.Log(0, null, string.Format("222222 k {0}: pts = [{1:0.000},{2:0.000}]", k, pts[k].x, pts[k].y));
                                    }

                                    data.m_graphmade_ROI_rect[0] = pts[3];
                                    data.m_graphmade_ROI_rect[1] = pts[0];
                                    data.m_graphmade_ROI_rect[2] = pts[1];
                                    data.m_graphmade_ROI_rect[3] = pts[2];

                                    data.m_bHasBeenMeasured = false;

                                    data_holder.Add(data);
                                }
                                else if ((5 == sub_array1.Length) && (2 == sub_array3.Length))
                                {
                                    MeasurePointData data = new MeasurePointData();

                                    data.m_ID = data_holder.Count + 1;
                                    data.m_create_mode = 3;
                                    data.m_mes_type = MEASURE_TYPE.CIRCLE_OUTER_TO_INNER;
                                    data.m_unit = 1;
                                    data.m_name = sub_array1[0];

                                    data.m_center_x_in_metric = Convert.ToDouble(sub_array3[0]);
                                    data.m_center_y_in_metric = Convert.ToDouble(sub_array3[1]);

                                    data.m_metric_radius_lower[0] = Convert.ToDouble(sub_array1[2]);
                                    data.m_metric_radius[0] = Convert.ToDouble(sub_array1[3]);
                                    data.m_metric_radius_upper[0] = Convert.ToDouble(sub_array1[4]);

                                    if (0 == m_nLightTypeForBGA)
                                    {
                                        data.m_bIsTopLightOn = true;
                                        data.m_bIsBottomLightOn = false;
                                    }
                                    else
                                    {
                                        data.m_bIsTopLightOn = false;
                                        data.m_bIsBottomLightOn = true;
                                    }
                                    
                                    // 计算最合适的倍率
                                    double min_dist = 100000;
                                    int min_dist_index = -1;
                                    for (int k = 0; k < array_ratio_widths.Length - 1; k++)
                                    {
                                        double dist = Math.Abs(array_ratio_widths[k] - data.m_metric_radius[0]);
                                        if (dist < min_dist)
                                        {
                                            min_dist = dist;
                                            min_dist_index = k;
                                        }
                                    }
                                    if (-1 != min_dist_index)
                                        data.m_len_ratio = min_dist_index;
                                    if (data.m_len_ratio >= 3)
                                        data.m_len_ratio -= 1;

                                    data.m_bHasBeenMeasured = false;

                                    // 生成ROI选取框
                                    double factor = 0.7 + (double)m_nSmallSearchFrameExtent * 0.05;
                                    double radius = data.m_metric_radius[0] * factor / 1000;
                                    data.m_graphmade_ROI_rect[0].x = data.m_center_x_in_metric - (radius / 2);
                                    data.m_graphmade_ROI_rect[0].y = data.m_center_y_in_metric - (radius / 2);
                                    data.m_graphmade_ROI_rect[1].x = data.m_center_x_in_metric + (radius / 2);
                                    data.m_graphmade_ROI_rect[1].y = data.m_center_y_in_metric - (radius / 2);
                                    data.m_graphmade_ROI_rect[2].x = data.m_center_x_in_metric + (radius / 2);
                                    data.m_graphmade_ROI_rect[2].y = data.m_center_y_in_metric + (radius / 2);
                                    data.m_graphmade_ROI_rect[3].x = data.m_center_x_in_metric - (radius / 2);
                                    data.m_graphmade_ROI_rect[3].y = data.m_center_y_in_metric + (radius / 2);

                                    data_holder.Add(data);
                                }
                            }

                            //Debugger.Log(0, null, string.Format("222222 {0}: array.Length = {1}", counter, array.Length));
                        }
                    }
                }
                catch (Exception ex)
                {

                }

                counter++;
            }

            for (int n = 0; n < data_holder.Count; n++)
            {
                switch (data_holder[n].m_mes_type)
                {
                    case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                        //Debugger.Log(0, null, string.Format("222222 n {0}: {1}, 坐标 [{2:0.000},{3:0.000}], 直径 {4:0.000}", n, data_holder[n].m_name,
                        //    data_holder[n].m_center_x_in_metric, data_holder[n].m_center_y_in_metric, data_holder[n].m_metric_radius[0]));
                        break;

                    case MEASURE_TYPE.LINE_WIDTH_14:
                        //Debugger.Log(0, null, string.Format("222222 n {0}: 下线宽, 坐标 [{1:0.000},{2:0.000}], 标准值 {3:0.000}", n,
                        //    data_holder[n].m_center_x_in_metric, data_holder[n].m_center_y_in_metric, data_holder[n].m_metric_line_width[0]));
                        break;
                }
                
            }

            return true;
        }

        // 读取任务文件
        public bool read_task_from_file(string task_name, List<MeasurePointData> data_holder, string strOfflineFilePath = "", bool bIsOfflineFile = false)
        {
            string path = task_name;

            if (true == bIsOfflineFile)
                path = strOfflineFilePath;

            if (!File.Exists(path))
            {
                MessageBox.Show(this, string.Format("找不到任务文件 {0}，请检查原因!", path), "提示");
                return false;
            }
            
            StreamReader reader = new StreamReader(path, false);

            Debugger.Log(0, null, string.Format("222222 path = {0}", path));

            int counter = 0;
            string strGraphName = "";
            string strStepsName = "";
            string strLayerName = "";
            while (true)
            {
                string line = reader.ReadLine();
                if (null == line)
                    break;

                if (0 == counter)
                    strGraphName = line;
                else if(1 == counter)
                    strStepsName = line;
                else if (2 == counter)
                    strLayerName = line;
                else
                {
                    try
                    {
                        string[] array = line.Split(',');
                        if (array.Length > 160)
                        {
                            MeasurePointData data = new MeasurePointData();

                            string value = "";

                            if (true == GeneralUtils.GetKeyValue2(array[0], "id", ref value))
                                data.m_ID = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[1], "version", ref value))
                                data.m_nVersion = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[2], "create_mode", ref value))
                                data.m_create_mode = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[3], "mes_type", ref value))
                                data.m_mes_type = (MEASURE_TYPE)Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[4], "name", ref value))
                                data.m_name = value;
                            if (true == GeneralUtils.GetKeyValue2(array[5], "graph_file_name", ref value))
                                data.m_strGraphFileName = value;
                            if (true == GeneralUtils.GetKeyValue2(array[6], "top_light_is_on", ref value))
                                data.m_bIsTopLightOn = Convert.ToBoolean(value);
                            if (true == GeneralUtils.GetKeyValue2(array[7], "top_light_brightness", ref value))
                                data.m_nTopBrightness = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[8], "bottom_light_is_on", ref value))
                                data.m_bIsBottomLightOn = Convert.ToBoolean(value);
                            if (true == GeneralUtils.GetKeyValue2(array[9], "bottom_light_brightness", ref value))
                                data.m_nBottomBrightness = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[10], "unit", ref value))
                                data.m_unit = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[11], "len_ratio", ref value))
                                data.m_len_ratio = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[12], "camera_exposure", ref value))
                                data.m_camera_exposure = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[13], "camera_gain", ref value))
                                data.m_camera_gain = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[14], "thres_for_skipping_autofocus", ref value))
                                data.m_thres_for_skipping_autofocus = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[15], "sharpness_at_creation", ref value))
                                data.m_sharpness_at_creation = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[16], "line_angle", ref value))
                                data.m_line_angle = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[17], "line_angle_on_graph", ref value))
                                data.m_line_angle_on_graph = Convert.ToDouble(value);

                            if (true == GeneralUtils.GetKeyValue2(array[18], "machine_crd_x", ref value))
                                data.m_theory_machine_crd.x = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[19], "machine_crd_y", ref value))
                                data.m_theory_machine_crd.y = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[20], "machine_crd_z", ref value))
                                data.m_theory_machine_crd.z = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[21], "center_x_in_metric", ref value))
                                data.m_center_x_in_metric = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[22], "center_y_in_metric", ref value))
                                data.m_center_y_in_metric = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[23], "center_x_on_graph", ref value))
                                data.m_center_x_on_graph = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[24], "center_y_on_graph", ref value))
                                data.m_center_y_on_graph = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[25], "pixels_per_mm", ref value))
                                data.m_pixels_per_mm = Convert.ToDouble(value);
                            if (true == GeneralUtils.GetKeyValue2(array[26], "graph_offset_x", ref value))
                                data.m_nGraphOffsetX = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[27], "graph_offset_y", ref value))
                                data.m_nGraphOffsetY = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[28], "graph_zoom_ratio", ref value))
                                data.m_nGraphZoomRatio = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[29], "orientation", ref value))
                                data.m_nOrientation = Convert.ToInt32(value);
                            if (true == GeneralUtils.GetKeyValue2(array[30], "is_made_for_offline_file", ref value))
                                data.m_bIsMadeForOfflineFile = Convert.ToBoolean(value);

                            for (int k = 0; k < 8; k++)
                            {
                                if (true == GeneralUtils.GetKeyValue2(array[31 + k * 3], string.Format("metric_radius{0}", k), ref value))
                                    data.m_metric_radius[k] = Convert.ToDouble(value);
                                if (true == GeneralUtils.GetKeyValue2(array[32 + k * 3], string.Format("metric_radius_upper{0}", k), ref value))
                                    data.m_metric_radius_upper[k] = Convert.ToDouble(value);
                                if (true == GeneralUtils.GetKeyValue2(array[33 + k * 3], string.Format("metric_radius_lower{0}", k), ref value))
                                    data.m_metric_radius_lower[k] = Convert.ToDouble(value);
                            }

                            for (int k = 0; k < 8; k++)
                            {
                                if (true == GeneralUtils.GetKeyValue2(array[55 + k * 3], string.Format("metric_line_width{0}", k), ref value))
                                    data.m_metric_line_width[k] = Convert.ToDouble(value);
                                if (true == GeneralUtils.GetKeyValue2(array[56 + k * 3], string.Format("metric_line_width_upper{0}", k), ref value))
                                    data.m_metric_line_width_upper[k] = Convert.ToDouble(value);
                                if (true == GeneralUtils.GetKeyValue2(array[57 + k * 3], string.Format("metric_line_width_lower{0}", k), ref value))
                                    data.m_metric_line_width_lower[k] = Convert.ToDouble(value);
                            }

                            for (int k = 0; k < 4; k++)
                            {
                                if (true == GeneralUtils.GetKeyValue2(array[79 + k * 2], string.Format("m_handmade_ROI_rect{0}_x", k), ref value))
                                    data.m_handmade_ROI_rect[k].x = Convert.ToDouble(value);
                                if (true == GeneralUtils.GetKeyValue2(array[80 + k * 2], string.Format("m_handmade_ROI_rect{0}_y", k), ref value))
                                    data.m_handmade_ROI_rect[k].y = Convert.ToDouble(value);
                            }

                            for (int k = 0; k < 4; k++)
                            {
                                if (true == GeneralUtils.GetKeyValue2(array[87 + k * 2], string.Format("m_graphmade_ROI_rect{0}_x", k), ref value))
                                    data.m_graphmade_ROI_rect[k].x = Convert.ToDouble(value);
                                if (true == GeneralUtils.GetKeyValue2(array[88 + k * 2], string.Format("m_graphmade_ROI_rect{0}_y", k), ref value))
                                    data.m_graphmade_ROI_rect[k].y = Convert.ToDouble(value);
                            }

                            for (int k = 0; k < 4; k++)
                            {
                                if (true == GeneralUtils.GetKeyValue2(array[95 + k * 2], string.Format("m_fit_rect_on_graph{0}_x", k), ref value))
                                    data.m_fit_rect_on_graph[k].x = Convert.ToDouble(value);
                                if (true == GeneralUtils.GetKeyValue2(array[96 + k * 2], string.Format("m_fit_rect_on_graph{0}_y", k), ref value))
                                    data.m_fit_rect_on_graph[k].y = Convert.ToDouble(value);
                            }

                            // 算法
                            if (true == GeneralUtils.GetKeyValue2(array[103], string.Format("backup_0"), ref value, false))
                            {
                                if (value.Length > 0)
                                    data.m_nAlgorithm = Convert.ToInt32(value);
                            }
                            
                            for (int k = 1; k < 100; k++)
                            {
                                //if (true == GeneralUtils.GetKeyValue2(array[92 + k * 2], string.Format("backup_{0}", k), ref value, false))
                                    //data.m_graphmade_ROI_rect[k].x = Convert.ToInt32(value);
                            }
                            
                            //Debugger.Log(0, null, string.Format("222222 data.m_ID = {0}, m_fit_rect_in_metric {1}, {2}", 
                            //    data.m_ID, data.m_graphmade_ROI_rect[0].x, data.m_graphmade_ROI_rect[0].y));

                            data_holder.Add(data);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                counter++;
            }

            m_strCurrentProductModel = strGraphName;
            m_strCurrentProductStep = strStepsName;
            m_strCurrentProductLayer = strLayerName;

            Debugger.Log(0, null, string.Format("222222 strGraphName = {0}", strGraphName));
            Debugger.Log(0, null, string.Format("222222 strStepsName = {0}", strStepsName));
            Debugger.Log(0, null, string.Format("222222 m_strCurrentProductLayer = {0}", m_strCurrentProductLayer));

            Debugger.Log(0, null, string.Format("222222 data_holder = {0}", data_holder.Count));

            reader.Close();

            return true;
        }

        // 保存任务文件
        public bool save_task_to_file(List<MeasurePointData> task_data, string task_name, string strOfflineFilePath = "", bool bIsOfflineFile = false, bool bUseOfflineFilePath = false)
        {
            string path = "";
            if ("" == m_strTaskFileSavingDir)
            {
                path = "任务文件";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = path + "\\" + m_strCurrentTaskName + ".dat";
            }
            else
                path = m_strTaskFileSavingDir + "\\" + m_strCurrentTaskName + ".dat";

            if (task_name.Contains(".dat"))
                path = task_name;

            if ((true == bIsOfflineFile) || (true == bUseOfflineFilePath))
                path = strOfflineFilePath;

            if (File.Exists(path))
            {
                if (DialogResult.No == MessageBox.Show(this, "已存在同名任务文件，是否覆盖?", "提示", MessageBoxButtons.YesNo))
                    return true;
            }

            //Debugger.Log(0, null, string.Format("222222 File.Exists path = {0}, task_data len = {1}", path, task_data.Count));

            try {
                StreamWriter writer = new StreamWriter(path, false);

                writer.WriteLine(task_data[0].m_strGraphFileName);
                writer.WriteLine(task_data[0].m_strStepsFileName);
                writer.WriteLine(task_data[0].m_strLayerFileName);

                for (int n = 0; n < task_data.Count; n++)
                {
                    string str = "";
                    str = string.Concat(str, string.Format("id={0},", n + 1));
                    str = string.Concat(str, string.Format("version={0},", task_data[n].m_nVersion));
                    str = string.Concat(str, string.Format("create_mode={0},", task_data[n].m_create_mode));
                    str = string.Concat(str, string.Format("mes_type={0},", (int)(task_data[n].m_mes_type)));
                    str = string.Concat(str, string.Format("name={0},", task_data[n].m_name));
                    str = string.Concat(str, string.Format("graph_file_name={0},", task_data[n].m_strGraphFileName));
                    str = string.Concat(str, string.Format("top_light_is_on={0},", task_data[n].m_bIsTopLightOn));
                    str = string.Concat(str, string.Format("top_light_brightness={0},", task_data[n].m_nTopBrightness));
                    str = string.Concat(str, string.Format("bottom_light_is_on={0},", task_data[n].m_bIsBottomLightOn));
                    str = string.Concat(str, string.Format("bottom_light_brightness={0},", task_data[n].m_nBottomBrightness));
                    str = string.Concat(str, string.Format("unit={0},", task_data[n].m_unit));
                    str = string.Concat(str, string.Format("len_ratio={0},", task_data[n].m_len_ratio));
                    str = string.Concat(str, string.Format("camera_exposure={0},", task_data[n].m_camera_exposure));
                    str = string.Concat(str, string.Format("camera_gain={0},", task_data[n].m_camera_gain));
                    str = string.Concat(str, string.Format("thres_for_skipping_autofocus={0},", task_data[n].m_thres_for_skipping_autofocus));
                    str = string.Concat(str, string.Format("sharpness_at_creation={0},", task_data[n].m_sharpness_at_creation));
                    str = string.Concat(str, string.Format("line_angle={0},", task_data[n].m_line_angle));
                    str = string.Concat(str, string.Format("line_angle_on_graph={0},", task_data[n].m_line_angle_on_graph));
                    str = string.Concat(str, string.Format("machine_crd_x={0},", task_data[n].m_theory_machine_crd.x));
                    str = string.Concat(str, string.Format("machine_crd_y={0},", task_data[n].m_theory_machine_crd.y));
                    str = string.Concat(str, string.Format("machine_crd_z={0},", task_data[n].m_theory_machine_crd.z));
                    str = string.Concat(str, string.Format("center_x_in_metric={0},", task_data[n].m_center_x_in_metric));
                    str = string.Concat(str, string.Format("center_y_in_metric={0},", task_data[n].m_center_y_in_metric));
                    str = string.Concat(str, string.Format("center_x_on_graph={0},", task_data[n].m_center_x_on_graph));
                    str = string.Concat(str, string.Format("center_y_on_graph={0},", task_data[n].m_center_y_on_graph));
                    str = string.Concat(str, string.Format("pixels_per_mm={0},", task_data[n].m_pixels_per_mm));
                    str = string.Concat(str, string.Format("graph_offset_x={0},", task_data[n].m_nGraphOffsetX));
                    str = string.Concat(str, string.Format("graph_offset_y={0},", task_data[n].m_nGraphOffsetY));
                    str = string.Concat(str, string.Format("graph_zoom_ratio={0},", task_data[n].m_nGraphZoomRatio));
                    str = string.Concat(str, string.Format("orientation={0},", task_data[n].m_nOrientation));
                    
                    if (true == bIsOfflineFile)
                        task_data[n].m_bIsMadeForOfflineFile = true;
                    str = string.Concat(str, string.Format("is_made_for_offline_file={0},", task_data[n].m_bIsMadeForOfflineFile));
                    
                    // 圆或定位孔的物理半径，单位mm
                    for (int k = 0; k < 8; k++)
                    {
                        str = string.Concat(str, string.Format("metric_radius{0}={1},", k, task_data[n].m_metric_radius[k]));
                        str = string.Concat(str, string.Format("metric_radius_upper{0}={1},", k, task_data[n].m_metric_radius_upper[k]));
                        str = string.Concat(str, string.Format("metric_radius_lower{0}={1},", k, task_data[n].m_metric_radius_lower[k]));
                    }

                    // 线宽，单位mm
                    for (int k = 0; k < 8; k++)
                    {
                        str = string.Concat(str, string.Format("metric_line_width{0}={1},", k, task_data[n].m_metric_line_width[k]));
                        str = string.Concat(str, string.Format("metric_line_width_upper{0}={1},", k, task_data[n].m_metric_line_width_upper[k]));
                        str = string.Concat(str, string.Format("metric_line_width_lower{0}={1},", k, task_data[n].m_metric_line_width_lower[k]));
                    }

                    // 手动模式下用户拉出来的ROI框四个角点对应的物理位置
                    for (int k = 0; k < 4; k++)
                    {
                        str = string.Concat(str, string.Format("m_handmade_ROI_rect{0}_x={1},", k, task_data[n].m_handmade_ROI_rect[k].x));
                        str = string.Concat(str, string.Format("m_handmade_ROI_rect{0}_y={1},", k, task_data[n].m_handmade_ROI_rect[k].y));
                    }

                    // 图纸模式下ROI框四个角点对应的物理位置
                    for (int k = 0; k < 4; k++)
                    {
                        str = string.Concat(str, string.Format("m_graphmade_ROI_rect{0}_x={1},", k, task_data[n].m_graphmade_ROI_rect[k].x));
                        str = string.Concat(str, string.Format("m_graphmade_ROI_rect{0}_y={1},", k, task_data[n].m_graphmade_ROI_rect[k].y));
                    }

                    // 图纸模式下ROI框四个角点对应的图纸图片坐标
                    for (int k = 0; k < 4; k++)
                    {
                        str = string.Concat(str, string.Format("m_fit_rect_on_graph{0}_x={1},", k, task_data[n].m_fit_rect_on_graph[k].x));
                        str = string.Concat(str, string.Format("m_fit_rect_on_graph{0}_y={1},", k, task_data[n].m_fit_rect_on_graph[k].y));
                    }

                    str = string.Concat(str, string.Format("backup_0={1},", task_data[n].m_nAlgorithm));

                    // 留作备用
                    for (int k = 1; k < 100; k++)
                    {
                        if (k == (100 - 1))
                            str = string.Concat(str, string.Format("backup_{0}=", k));
                        else
                            str = string.Concat(str, string.Format("backup_{0}=,", k));
                    }
                    
                    writer.WriteLine(str);
                    writer.WriteLine("");
                }

                writer.Close();

                //保存完成后清空step与layer
                m_strCurrentProductStep = "";
                m_strCurrentProductLayer = "";

                return true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 save_task_to_file() error message: {0}", ex.Message));
                return false;
            }
        }

        // 保存任务到数据库
        public bool create_table_and_save_task_to_table(SqlConnection sql_conn, List<MeasurePointData> task_data, string task_name, bool bIsUpdate = false)
        {
            // 先判断是否存在同名任务
            #region
            bool bNeedReplace = false;
            string strTaskName = "s" + task_name;

            if (true == bIsUpdate)
                bNeedReplace = true;
            else
            {
                foreach (string name in m_vec_SQL_table_names)
                {
                    //Debugger.Log(0, null, string.Format("222222 strTaskName {0}, name {1}", strTaskName, name));
                    if (strTaskName == ("s" + name))
                    {
                        if (DialogResult.OK == MessageBox.Show(this, "已存在同名任务，是否覆盖?", "提示", MessageBoxButtons.OKCancel))
                            bNeedReplace = true;
                        else
                            return false;
                    }
                }
            }
            
            // 如果存在同名任务，先删除旧任务，再创建，相当于覆盖
            if (true == bNeedReplace)
            {
                try
                {
                    string query = "DROP TABLE s" + task_name;

                    SqlCommand cmd = new SqlCommand(query, sql_conn);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debugger.Log(0, null, string.Format("222222 删除旧任务失败! {0}", ex.Message));

                    CBD_SendMessage(string.Format("删除旧任务 “{0}” 失败, 原因是: {1}", task_name, ex.Message), false, null, null);
                    return false;
                }
            }
            #endregion

            // 创建任务数据表
            try
            {
                string strQuery = "CREATE TABLE s" + task_name + " (id int PRIMARY KEY not null";
                strQuery += ",version int";                         // 版本号
                strQuery += ",create_mode int";               // 首件制作方式
                strQuery += ",mes_type int";                     // 测量类型
                strQuery += ",name varchar(256)";           // 名称
                strQuery += ",graph_file_name varchar(128)";           // 图纸文件名
                strQuery += ",top_light_is_on int";            // 上环光是否打亮
                strQuery += ",top_light_brightness int";    // 上环光亮度
                strQuery += ",bottom_light_is_on int";      // 下环光是否打亮
                strQuery += ",bottom_light_brightness int";    // 下环光亮度
                strQuery += ",unit int";                               // 单位
                strQuery += ",len_ratio int";                       // 倍率
                strQuery += ",camera_exposure float";     // 制作首件时主相机的曝光时间，单位毫秒
                strQuery += ",camera_gain float";             // 制作首件时主相机的增益
                strQuery += ",thres_for_skipping_autofocus int";   // 跳过自动对焦所需清晰度百分比阈值
                strQuery += ",sharpness_at_creation float";          // 制作首件时该测量项的图像清晰度
                strQuery += ",line_angle float";                              // 手动模式下，如果该测量项是线宽或线距，则记录它的角度
                strQuery += ",line_angle_on_graph float";             // 图纸模式下，如果该测量项是线宽或线距，则记录它在图纸上的角度
                strQuery += ",machine_crd_x float";         // 手动模式下的机械坐标，单位mm
                strQuery += ",machine_crd_y float";         // 手动模式下的机械坐标，单位mm
                strQuery += ",machine_crd_z float";         // 手动模式下的机械坐标，单位mm
                strQuery += ",m_center_x_in_metric float";         // 图纸模式下的图纸机械坐标，单位mm
                strQuery += ",m_center_y_in_metric float";         // 图纸模式下的图纸机械坐标，单位mm
                strQuery += ",m_center_x_on_graph float";         // 图纸模式下的图纸坐标，单位像素
                strQuery += ",m_center_y_on_graph float";         // 图纸模式下的图纸坐标，单位像素
                strQuery += ",m_pixels_per_mm float";
                strQuery += ",graph_offset_x int";
                strQuery += ",graph_offset_y int";
                strQuery += ",graph_zoom_ratio int";                   // 图纸放大倍率
                strQuery += ",orientation int";                               // 记录8个方向中选择了哪个方向
                strQuery += ",is_made_for_offline_file int";          // 是否是离线文件
                
                for (int n = 0; n < 8; n++)
                {
                    strQuery += string.Format(",metric_radius{0} float", n);            // 圆或定位孔的物理半径，单位mm
                    strQuery += string.Format(",metric_radius_upper{0} float", n);
                    strQuery += string.Format(",metric_radius_lower{0} float", n);
                }
                for (int n = 0; n < 8; n++)
                {
                    strQuery += string.Format(",metric_line_width{0} float", n);            // 线宽，单位mm
                    strQuery += string.Format(",metric_line_width_upper{0} float", n);
                    strQuery += string.Format(",metric_line_width_lower{0} float", n);
                }
                for (int n = 0; n < 4; n++)
                {
                    strQuery += string.Format(",m_handmade_ROI_rect{0}_x float", n);            // 手动模式下用户拉出来的ROI框四个角点对应的物理位置
                    strQuery += string.Format(",m_handmade_ROI_rect{0}_y float", n);
                }
                for (int n = 0; n < 4; n++)
                {
                    strQuery += string.Format(",m_graphmade_ROI_rect{0}_x float", n);            // 图纸模式下ROI框四个角点对应的物理位置
                    strQuery += string.Format(",m_graphmade_ROI_rect{0}_y float", n);
                }
                for (int n = 0; n < 4; n++)
                {
                    strQuery += string.Format(",m_fit_rect_on_graph{0}_x float", n);                   // 图纸模式下ROI框四个角点对应的图纸图片位置
                    strQuery += string.Format(",m_fit_rect_on_graph{0}_y float", n);
                }

                // 留作备用
                for (int n = 1; n <= 100; n++)
                {
                    strQuery += string.Format(",backup_{0} TEXT", n);
                }

                strQuery += ")";

                SqlCommand command = new SqlCommand(strQuery, sql_conn);
                command.ExecuteNonQuery();

                //DataTable dt = GetTableSchema();
                for (int n = 0; n < task_data.Count; n++)
                {
                    //DataRow dr = dt.NewRow();
                    //Debugger.Log(0, null, string.Format("222222 n {0}: m_metric_radius = {1:0.000}", n, parent.m_list_confirmed_mes_pt_data[n].m_metric_radius));

                    string strInsert = "insert into s" + task_name + "(id,";
                    strInsert += "version,";
                    strInsert += "create_mode,";
                    strInsert += "mes_type,";
                    strInsert += "name,";
                    strInsert += "graph_file_name,";
                    strInsert += "top_light_is_on,";
                    strInsert += "top_light_brightness,";
                    strInsert += "bottom_light_is_on,";
                    strInsert += "bottom_light_brightness,";
                    strInsert += "unit,";
                    strInsert += "len_ratio,";
                    strInsert += "camera_exposure,";
                    strInsert += "camera_gain,";
                    strInsert += "thres_for_skipping_autofocus,";
                    strInsert += "sharpness_at_creation,";
                    strInsert += "line_angle,";
                    strInsert += "line_angle_on_graph,";
                    strInsert += "machine_crd_x,";
                    strInsert += "machine_crd_y,";
                    strInsert += "machine_crd_z,";
                    strInsert += "m_center_x_in_metric,";
                    strInsert += "m_center_y_in_metric,";
                    strInsert += "m_center_x_on_graph,";
                    strInsert += "m_center_y_on_graph,";
                    strInsert += "m_pixels_per_mm,";
                    strInsert += "graph_offset_x,";
                    strInsert += "graph_offset_y,";
                    strInsert += "graph_zoom_ratio,";
                    strInsert += "orientation,";
                    strInsert += "is_made_for_offline_file";
                    for (int k = 0; k < 8; k++)
                    {
                        strInsert += string.Format(",metric_radius{0}", k);            // 圆或定位孔的物理半径，单位mm
                        strInsert += string.Format(",metric_radius_upper{0}", k);
                        strInsert += string.Format(",metric_radius_lower{0}", k);
                    }
                    for (int k = 0; k < 8; k++)
                    {
                        strInsert += string.Format(",metric_line_width{0}", k);            // 线宽，单位mm
                        strInsert += string.Format(",metric_line_width_upper{0}", k);
                        strInsert += string.Format(",metric_line_width_lower{0}", k);
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        strInsert += string.Format(",m_handmade_ROI_rect{0}_x", k);            // 手动模式下用户拉出来的ROI框四个角点对应的物理位置
                        strInsert += string.Format(",m_handmade_ROI_rect{0}_y", k);
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        strInsert += string.Format(",m_graphmade_ROI_rect{0}_x", k);            // 图纸模式下ROI框四个角点对应的物理位置
                        strInsert += string.Format(",m_graphmade_ROI_rect{0}_y", k);
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        strInsert += string.Format(",m_fit_rect_on_graph{0}_x", k);                  // 图纸模式下ROI框四个角点对应的图纸图片位置
                        strInsert += string.Format(",m_fit_rect_on_graph{0}_y", k);
                    }
                    strInsert += ") values(";

                    strInsert += string.Format("{0},", n + 1);
                    strInsert += string.Format("{0},", task_data[n].m_nVersion);
                    strInsert += string.Format("{0},", task_data[n].m_create_mode);
                    strInsert += string.Format("{0},", (int)(task_data[n].m_mes_type));
                    strInsert += string.Format("\'{0}\',", task_data[n].m_name);
                    strInsert += string.Format("\'{0}\',", task_data[n].m_strGraphFileName);
                    strInsert += string.Format("{0},", Convert.ToInt32(task_data[n].m_bIsTopLightOn));
                    strInsert += string.Format("{0},", task_data[n].m_nTopBrightness);
                    strInsert += string.Format("{0},", Convert.ToInt32(task_data[n].m_bIsBottomLightOn));
                    strInsert += string.Format("{0},", task_data[n].m_nBottomBrightness);
                    strInsert += string.Format("{0},", task_data[n].m_unit);
                    strInsert += string.Format("{0},", task_data[n].m_len_ratio);
                    strInsert += string.Format("{0},", task_data[n].m_camera_exposure);
                    strInsert += string.Format("{0},", task_data[n].m_camera_gain);
                    strInsert += string.Format("{0},", task_data[n].m_thres_for_skipping_autofocus);
                    strInsert += string.Format("{0},", task_data[n].m_sharpness_at_creation);
                    strInsert += string.Format("{0},", task_data[n].m_line_angle);
                    strInsert += string.Format("{0},", task_data[n].m_line_angle_on_graph);
                    strInsert += string.Format("{0},", task_data[n].m_theory_machine_crd.x);
                    strInsert += string.Format("{0},", task_data[n].m_theory_machine_crd.y);
                    strInsert += string.Format("{0},", task_data[n].m_theory_machine_crd.z);
                    strInsert += string.Format("{0},", task_data[n].m_center_x_in_metric);
                    strInsert += string.Format("{0},", task_data[n].m_center_y_in_metric);
                    strInsert += string.Format("{0},", task_data[n].m_center_x_on_graph);
                    strInsert += string.Format("{0},", task_data[n].m_center_y_on_graph);
                    strInsert += string.Format("{0},", task_data[n].m_pixels_per_mm);
                    strInsert += string.Format("{0},", task_data[n].m_nGraphOffsetX);
                    strInsert += string.Format("{0},", task_data[n].m_nGraphOffsetY);
                    strInsert += string.Format("{0},", task_data[n].m_nGraphZoomRatio);
                    strInsert += string.Format("{0},", task_data[n].m_nOrientation);
                    strInsert += string.Format("{0}", Convert.ToInt32(task_data[n].m_bIsMadeForOfflineFile));
                    for (int k = 0; k < 8; k++)
                    {
                        strInsert += string.Format(",{0}", task_data[n].m_metric_radius[k]);            // 圆或定位孔的物理半径，单位mm
                        strInsert += string.Format(",{0}", task_data[n].m_metric_radius_upper[k]);
                        strInsert += string.Format(",{0}", task_data[n].m_metric_radius_lower[k]);
                    }
                    for (int k = 0; k < 8; k++)
                    {
                        strInsert += string.Format(",{0}", task_data[n].m_metric_line_width[k]);            // 线宽，单位mm
                        strInsert += string.Format(",{0}", task_data[n].m_metric_line_width_upper[k]);
                        strInsert += string.Format(",{0}", task_data[n].m_metric_line_width_lower[k]);
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        strInsert += string.Format(",{0}", task_data[n].m_handmade_ROI_rect[k].x);            // 手动模式下用户拉出来的ROI框四个角点对应的物理位置
                        strInsert += string.Format(",{0}", task_data[n].m_handmade_ROI_rect[k].y);
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        strInsert += string.Format(",{0}", task_data[n].m_graphmade_ROI_rect[k].x);            // 图纸模式下ROI框四个角点对应的物理位置
                        strInsert += string.Format(",{0}", task_data[n].m_graphmade_ROI_rect[k].y);
                    }
                    for (int k = 0; k < 4; k++)
                    {
                        strInsert += string.Format(",{0}", task_data[n].m_fit_rect_on_graph[k].x);                  // 图纸模式下ROI框四个角点对应的图纸图片位置
                        strInsert += string.Format(",{0}", task_data[n].m_fit_rect_on_graph[k].y);
                    }
                    strInsert += ")";

                    command = new SqlCommand(strInsert, sql_conn);
                    command.ExecuteNonQuery();
                }

                CBD_SendMessage(string.Format("任务“{0}”成功创建并写入数据库", task_name), true, null, null);
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 创建数据表失败! {0}", ex.Message));

                CBD_SendMessage(string.Format("任务 “{0}” 创建失败, 原因是: {1}", task_name, ex.Message), true, null, null);
            }

            return true;
        }

        // 根据测量项数据的索引号，返回该测量项在 gridview 上的行号
        public int get_row_index_by_measure_data_index(DataGridView gridview, List<MeasurePointData> task_data, int nDataIndex)
        {
            int nRows = 0;

            for (int n = 0; n < nDataIndex; n++)
                nRows += get_lines_count_for_measure_type(task_data[n].m_mes_type);

            return nRows;
        }

        // 更新 datagridview 上某行测量项内容
        public void update_row_on_gridview(DataGridView gridview, List<MeasurePointData> task_data, int nDataIndex)
        {
            MeasurePointData data = task_data[nDataIndex];

            string[] ratios = new string[] { "70X", "100X", "200X", "300X", "400X", "500X" };

            string strID = string.Format("{0}", data.m_ID);
            string strType = get_measure_type_name(data.m_mes_type);
            string strName = data.m_name;
            string strStandardValue = "";
            string strUpperLmt = "";
            string strLowerLmt = "";
            string strUnit = m_strUnits[data.m_unit];
            string strLenRatio = ratios[data.m_len_ratio];
            string strTopLight, strBottomLight;
            string strTheoryCrd = string.Format("[{0:0.000}, {1:0.000}, {2:0.000}]", data.m_theory_machine_crd.x, data.m_theory_machine_crd.y, data.m_theory_machine_crd.z);

            switch (data.m_mes_type)
            {
                case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                case MEASURE_TYPE.HAND_PICK_CIRCLE:
                    strStandardValue = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_radius[0], data.m_unit));
                    strUpperLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_radius_upper[0], data.m_unit));
                    strLowerLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_radius_lower[0], data.m_unit));
                    break;

                case MEASURE_TYPE.LINE_WIDTH_14:
                case MEASURE_TYPE.LINE_WIDTH_23:
                case MEASURE_TYPE.LINE_WIDTH_13:
                case MEASURE_TYPE.LINE_WIDTH_1234:
                case MEASURE_TYPE.LINE_SPACE:
                case MEASURE_TYPE.ARC_LINE_SPACE:
                case MEASURE_TYPE.HAND_PICK_LINE:
                case MEASURE_TYPE.LINE:
                case MEASURE_TYPE.ARC_LINE_WIDTH:
                    strStandardValue = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width[0], data.m_unit));
                    strUpperLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_upper[0], data.m_unit));
                    strLowerLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_lower[0], data.m_unit));
                    break;
            }

            if (data.m_bIsTopLightOn)
                strTopLight = string.Format("开-{0}", data.m_nTopBrightness);
            else
                strTopLight = "关";
            if (data.m_bIsBottomLightOn)
                strBottomLight = string.Format("开-{0}", data.m_nBottomBrightness);
            else
                strBottomLight = "关";

            string[] row = new String[11] { strID, strType, strName, strStandardValue, strUpperLmt, strLowerLmt, strUnit, strLenRatio, strTopLight, strBottomLight,
                strTheoryCrd};

            if (data.m_mes_type == MEASURE_TYPE.LINE_WIDTH_1234)
                row[1] = "上下线宽(上)";

            int nRowIndex = get_row_index_by_measure_data_index(gridview, task_data, nDataIndex);
            if (nRowIndex > gridview.Rows.Count)
                return;
            //Debugger.Log(0, null, string.Format("222222 nDataIndex = {0}, nRowIndex = {1}, data.m_ID = {2}", nDataIndex, nRowIndex, data.m_ID));
            for (int n = 0; n < 10; n++)
                gridview[n, nRowIndex].Value = row[n];

            if (data.m_mes_type == MEASURE_TYPE.LINE_WIDTH_1234)
            {
                strStandardValue = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width[1], data.m_unit));
                strUpperLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_upper[1], data.m_unit));
                strLowerLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_lower[1], data.m_unit));

                row = new String[11] { "", "上下线宽(下)", strName, strStandardValue, strUpperLmt, strLowerLmt, strUnit, strLenRatio, strTopLight, strBottomLight, strTheoryCrd };

                for (int n = 0; n < 10; n++)
                    gridview[n, nRowIndex + 1].Value = row[n];
            }
        }

        // 在 datagridview 上显示任务内容
        public void show_task_on_gridview(DataGridView gridview, List<MeasurePointData> task_data)
        {
            string[] ratios = new string[] { "70X", "100X", "200X", "300X", "400X", "500X" };

            gridview.Rows.Clear();
            for (int n = 0; n < task_data.Count; n++)
            {
                add_entry_to_task_gridview(gridview, task_data[n]);
            }
        }

        // 确保 gridview 最后一行在当前视野
        public void make_gridview_last_row_visible(DataGridView gridview)
        {
            if (gridview.Rows.Count > 1)
            {
                gridview.FirstDisplayedScrollingRowIndex = gridview.Rows.Count - 1;
            }
            foreach (DataGridViewRow r in gridview.Rows)
            {
                if (r.Index == (gridview.RowCount - 2))
                    r.Selected = true;
                else
                    r.Selected = false;
            }
        }

        // 选中并显示 gridview 的第N行
        public void make_visible_gridview_row_N(DataGridView gridview, int nRowIdx)
        {
            if (gridview.Rows.Count > nRowIdx)
            {
                gridview.FirstDisplayedScrollingRowIndex = nRowIdx;
            }
            foreach (DataGridViewRow r in gridview.Rows)
            {
                if (r.Index == nRowIdx)
                    r.Selected = true;
                else
                    r.Selected = false;
            }
        }

        // 将单个测量项目添加到 任务表单
        public void add_entry_to_task_gridview(DataGridView gridview, MeasurePointData data)
        {
            string[] ratios = new string[] { "70X", "100X", "200X", "300X", "400X", "500X" };
            
            string strID = string.Format("{0}", data.m_ID);
            string strType = get_measure_type_name(data.m_mes_type);
            string strName = data.m_name;
            string strStandardValue = "";
            string strUpperLmt = "";
            string strLowerLmt = "";
            string strUnit = m_strUnits[data.m_unit];
            string strLenRatio = ratios[data.m_len_ratio];
            string strTopLight, strBottomLight;
            string strTheoryCrd = string.Format("[{0:0.000}, {1:0.000}, {2:0.000}]", data.m_theory_machine_crd.x, data.m_theory_machine_crd.y, data.m_theory_machine_crd.z);
            
            switch (data.m_mes_type)
            {
                case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                case MEASURE_TYPE.HAND_PICK_CIRCLE:
                    strStandardValue = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_radius[0], data.m_unit));
                    strUpperLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_radius_upper[0], data.m_unit));
                    strLowerLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_radius_lower[0], data.m_unit));
                    break;

                case MEASURE_TYPE.LINE_WIDTH_14:
                case MEASURE_TYPE.LINE_WIDTH_23:
                case MEASURE_TYPE.LINE_WIDTH_13:
                case MEASURE_TYPE.LINE_WIDTH_1234:
                case MEASURE_TYPE.LINE_SPACE:
                case MEASURE_TYPE.ARC_LINE_SPACE:
                case MEASURE_TYPE.HAND_PICK_LINE:
                case MEASURE_TYPE.LINE:
                case MEASURE_TYPE.ARC_LINE_WIDTH:
                    strStandardValue = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width[0], data.m_unit));
                    strUpperLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_upper[0], data.m_unit));
                    strLowerLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_lower[0], data.m_unit));
                    break;
            }
            
            if (data.m_bIsTopLightOn)
                strTopLight = string.Format("开-{0}", data.m_nTopBrightness);
            else
                strTopLight = "关";
            if (data.m_bIsBottomLightOn)
                strBottomLight = string.Format("开-{0}", data.m_nBottomBrightness);
            else
                strBottomLight = "关";

            string[] row = new String[11] { strID, strType, strName, strStandardValue, strUpperLmt, strLowerLmt, strUnit, strLenRatio, strTopLight, strBottomLight,
                strTheoryCrd};

            if (data.m_mes_type == MEASURE_TYPE.LINE_WIDTH_1234)
                row[1] = "上下线宽(下)";
            
            if (true == data.m_bIsInvalidItem)
            {
                row = new String[11] { strID, strType, "无效", "N/A", "N/A", "N/A", "", "", "", "", ""};

                gridview.Rows.Add(row);
                return;
            }

            gridview.Rows.Add(row);
            
            if (data.m_mes_type == MEASURE_TYPE.LINE_WIDTH_1234)
            {
                strStandardValue = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width[1], data.m_unit));
                strUpperLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_upper[1], data.m_unit));
                strLowerLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_lower[1], data.m_unit));

                row = new String[11] { "", "上下线宽(上)", strName, strStandardValue, strUpperLmt, strLowerLmt, strUnit, strLenRatio, strTopLight, strBottomLight, strTheoryCrd};
                gridview.Rows.Add(row);
            }
        }

        // 将单个测量结果添加到 结果表单
        public void add_entry_to_measure_result_gridview(DataGridView gridview, MeasurePointData data)
        {
            string[] ratios = new string[] { "70X", "100X", "200X", "300X", "400X", "500X" };

            string strID = string.Format("{0}", data.m_ID);
            string strType = get_measure_type_name(data.m_mes_type);
            string strName = data.m_name;
            string strStandardValue = "";
            string strUpperLmt = "";
            string strLowerLmt = "";
            string strUnit = m_strUnits[data.m_unit];
            string strLenRatio = ratios[data.m_len_ratio];
            string strTopLight, strBottomLight;
            string strTheoryCrd = string.Format("[{0:0.000}, {1:0.000}, {2:0.000}]", data.m_theory_machine_crd.x, data.m_theory_machine_crd.y, data.m_theory_machine_crd.z);

            switch (data.m_mes_type)
            {
                case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                case MEASURE_TYPE.HAND_PICK_CIRCLE:
                    strStandardValue = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_radius[0], data.m_unit));
                    strUpperLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_radius_upper[0], data.m_unit));
                    strLowerLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_radius_lower[0], data.m_unit));
                    break;
                case MEASURE_TYPE.LINE_WIDTH_14:
                case MEASURE_TYPE.LINE_WIDTH_23:
                case MEASURE_TYPE.LINE_WIDTH_13:
                case MEASURE_TYPE.LINE_WIDTH_1234:
                case MEASURE_TYPE.LINE_SPACE:
                case MEASURE_TYPE.ARC_LINE_SPACE:
                case MEASURE_TYPE.HAND_PICK_LINE:
                case MEASURE_TYPE.LINE:
                case MEASURE_TYPE.ARC_LINE_WIDTH:
                    strStandardValue = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width[0], data.m_unit));
                    strUpperLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_upper[0], data.m_unit));
                    strLowerLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_lower[0], data.m_unit));
                    break;
            }

            if (data.m_bIsTopLightOn)
                strTopLight = string.Format("开-{0}", data.m_nTopBrightness);
            else
                strTopLight = "关";
            if (data.m_bIsBottomLightOn)
                strBottomLight = string.Format("开 {0}", data.m_nBottomBrightness);
            else
                strBottomLight = "关";

            string[] row = new String[13] { strID, strType, strName, strStandardValue, strUpperLmt, strLowerLmt, strUnit, "", "", strLenRatio, strTopLight, strBottomLight,
                strTheoryCrd};

            if (data.m_mes_type == MEASURE_TYPE.LINE_WIDTH_1234)
                row[1] = "上下线宽(下)";

            gridview.Rows.Add(row);

            if (MEASURE_TYPE.LINE_WIDTH_1234 == data.m_mes_type)
            {
                strStandardValue = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width[1], data.m_unit));
                strUpperLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_upper[1], data.m_unit));
                strLowerLmt = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(data.m_metric_line_width_lower[1], data.m_unit));

                row = new String[13] { "", "上下线宽(上)", strName, strStandardValue, strUpperLmt, strLowerLmt, strUnit, "", "", strLenRatio, strTopLight, strBottomLight, strTheoryCrd};
                gridview.Rows.Add(row);
            }
        }

        // 获取测量类型的名称
        public string get_measure_type_name(MEASURE_TYPE type)
        {
            switch (type)
            {
                case MEASURE_TYPE.LINE_WIDTH_14:
                    return "下线宽";

                case MEASURE_TYPE.LINE_WIDTH_23:
                    return "上线宽";

                case MEASURE_TYPE.LINE_WIDTH_1234:
                    return "上下线宽";

                case MEASURE_TYPE.LINE_WIDTH_13:
                    return "13线宽";

                case MEASURE_TYPE.ARC_LINE_WIDTH:
                    return "弧形线宽";

                case MEASURE_TYPE.LINE_SPACE:
                    return "线距";

                case MEASURE_TYPE.ARC_LINE_SPACE:
                    return "弧形线距";

                case MEASURE_TYPE.HAND_PICK_LINE:
                    return "手拉线";
                    
                case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                    return "圆(由外向内)";

                case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                    return "圆(由内向外)";

                case MEASURE_TYPE.HAND_PICK_CIRCLE:
                    return "手动三点选圆";

                case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                    return "定位孔";

                case MEASURE_TYPE.LINE:
                    return "单线寻边";

                default:
                    return "未知";
            }
        }

        // 获取测量类型对应的行数（譬如上下线宽占两行）
        public int get_lines_count_for_measure_type(MEASURE_TYPE type)
        {
            switch (type)
            {
                case MEASURE_TYPE.LINE_WIDTH_1234:
                    return 2;
                default:
                    return 1;
            }
        }

        // 获取阵列单元总数
        public int get_array_rects_count(List<List<rotated_array_rect>> array)
        {
            int count = 0;

            for (int n = 0; n < array.Count; n++)
                count += array[n].Count;

            return count;
        }

        // 添加测量点
        public bool add_measure_point(MEASURE_TYPE measure_type, int graph_x, int graph_y, ref MeasurePointData ret_data, 
            bool bAdvancedMode = false, bool bIsDrawnLine = false, bool bAddMarkPt = false)
        {
            m_bShowCoarseMark = false;
            m_bShowFrameDuringTaskCreation = false;
            
            if ((false == m_bOfflineMode) && (0 == m_ODB_measure_items.Count))
            {
                if ((false == m_bIsCreatingTask) || (1 == m_nCreateTaskMode))
                    return false;
            }
            
            if (null == m_graph_view.Image) return false;
            if (m_graph_view.m_zoom_ratio < 0.000001) return false;
            if ((m_graph_view.Image.Width * m_graph_view.Image.Height) <= 0) return false;
            
            if (false == bIsDrawnLine)
            {
                if ((graph_x <= 0) || (graph_y <= 0)) return false;
                if ((graph_x >= m_graph_view.m_bitmap_1bit.Width) || (graph_y >= m_graph_view.m_bitmap_1bit.Height)) return false;
            }
            
            if ((false == m_bOfflineMode) && (0 == m_ODB_measure_items.Count))
            {
                if ((get_fiducial_mark_count(m_current_task_data) < 3) && (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE != measure_type))
                {
                    MessageBox.Show(this, "请选定3个定位孔后，再进行测量。", "提示");
                    return false;
                }
                if ((get_fiducial_mark_count(m_current_task_data) >= 3) && (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == measure_type))
                {
                    MessageBox.Show(this, "已有3个定位孔，无法再添加定位孔。\r如需修改或添加定位孔，请先删除已有的定位孔。", "提示");
                    return false;
                }
            }

            int[] ret_ints = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] ret_doubles = new double[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            //Debugger.Log(0, null, string.Format("222222 graph_x [{0},{1}] 判断是否是绿色点", graph_x, m_nGraphHeight - graph_y));

            // 判断是否是绿色点
            #region
            if (false == bIsDrawnLine)
            {
                if ((MEASURE_TYPE.LINE_WIDTH_14 == measure_type) || (MEASURE_TYPE.LINE_WIDTH_23 == measure_type)
                || (MEASURE_TYPE.LINE_WIDTH_13 == measure_type) || (MEASURE_TYPE.LINE_WIDTH_1234 == measure_type)
                || (MEASURE_TYPE.CIRCLE_OUTER_TO_INNER == measure_type))
                {
                    ret_ints[0] = 0;
                    if (1 == m_nGraphType)
                        dllapi_is_green_point_in_gerber(graph_x, graph_y, ret_ints);
                    else if (2 == m_nGraphType)
                        dllapi_is_green_point_in_ODB(graph_x, graph_y, ret_ints);
                    bool bIsGreen = ret_ints[0] > 0 ? true : false;
                    if (false == bIsGreen)
                    {
                        Debugger.Log(0, null, string.Format("222222 graph_x [{0},{1}] 不是颜色点", graph_x, graph_y));
                        //MessageBox.Show(this, "鼠标位置不是颜色点。", "提示");
                        return false;
                    }
                }
                else if ((MEASURE_TYPE.LINE_SPACE == measure_type) || (MEASURE_TYPE.ARC_LINE_SPACE == measure_type))
                {
                    ret_ints[0] = 0;
                    if (1 == m_nGraphType)
                        dllapi_is_green_point_in_gerber(graph_x, graph_y, ret_ints);
                    else if (2 == m_nGraphType)
                        dllapi_is_green_point_in_ODB(graph_x, graph_y, ret_ints);
                    bool bIsGreen = ret_ints[0] > 0 ? true : false;
                    if (true == bIsGreen)
                    {
                        Debugger.Log(0, null, string.Format("222222 graph_x [{0},{1}] 不是黑色点", graph_x, graph_y));
                        MessageBox.Show(this, "鼠标位置不是黑色点。", "提示");
                        return false;
                    }
                }
            }
            #endregion

            double[] array_ratio_widths = new double[] { 2900, 2300, 1700, 1200, 900, 550 };
            string[] array_ratios = new string[] { "70X", "100X", "200X", "300X", "400X", "500X" };

            //for (int n = 0; n < m_measure_items_on_graph.Count; n++)
            //    Debugger.Log(0, null, string.Format("222222 {0}: {1}", n, m_measure_items_on_graph[n].m_ID));
            
            if (false == m_graph_view.m_bHasValidImage)
            {
                MessageBox.Show(this, "请先导入图形。", "提示");
                return false;
            }

            // 获取鼠标所指图元的信息
            #region
            if (false == bIsDrawnLine)
            {
                ret_ints[0] = 0;
                if (MEASURE_TYPE.LINE_SPACE == measure_type)
                {
                    if (1 == m_nGraphType)
                        dllapi_get_line_space_info_in_gerber_hv(measure_type, graph_x, graph_y, ret_ints, ret_doubles);
                    else if (2 == m_nGraphType)
                    {
                        dllapi_get_line_space_info_in_ODB_hv(measure_type, ret_data.m_bIsFromODBAttribute, ret_data.m_metric_line_width[0], graph_x, graph_y, ret_ints, ret_doubles);
                    }
                }
                else if (MEASURE_TYPE.ARC_LINE_SPACE == measure_type)
                {
                    if (1 == m_nGraphType)
                        dllapi_get_line_space_info_in_gerber_hv(measure_type, graph_x, graph_y, ret_ints, ret_doubles);
                    else if (2 == m_nGraphType)
                    {
                        dllapi_get_line_space_info_in_ODB_hv(measure_type, ret_data.m_bIsFromODBAttribute, ret_data.m_metric_line_width[0], graph_x, graph_y, ret_ints, ret_doubles);
                    }
                }
                else
                {
                    if (1 == m_nGraphType)
                    {
                        if ((MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == measure_type) || (MEASURE_TYPE.CIRCLE_OUTER_TO_INNER == measure_type))
                            dllapi_get_line_width_in_gerber(measure_type, graph_x, graph_y, ret_ints, ret_doubles);
                        else
                        {
                            if (0 == m_nSourceOfStandardValue)
                                dllapi_get_line_width_in_gerber(measure_type, graph_x, graph_y, ret_ints, ret_doubles);
                            else if (1 == m_nSourceOfStandardValue)
                                dllapi_get_line_info_in_gerber_hv(measure_type, graph_x, graph_y, ret_ints, ret_doubles);
                        }
                    }
                    else if (2 == m_nGraphType)
                    {
                        if (0 == m_nSourceOfStandardValue)
                        {
                            dllapi_get_line_info_in_ODB(measure_type, graph_x, graph_y, ret_ints, ret_doubles);
                        }
                    }
                }
                
                //Debugger.Log(0, null, string.Format("222222 获取鼠标所指图元的信息 graph_x [{0},{1}]", graph_x, graph_y));
                //Debugger.Log(0, null, string.Format("222222 data.ret_ints[0] {0}, m_nSourceOfStandardValue = {1}, measure_type = {2}", 
                //    ret_ints[0], m_nSourceOfStandardValue, measure_type));
                
                if (0 == ret_ints[0])
                {
                    switch (measure_type)
                    {
                        case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                        case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                            if (1 == m_nGraphType)
                                dllapi_get_circle_info_in_gerber_hv(measure_type, graph_x, graph_y, ret_ints, ret_doubles);
                            else if (2 == m_nGraphType)
                                dllapi_get_circle_info_in_ODB_hv(measure_type, graph_x, graph_y, ret_ints, ret_doubles);
                            break;

                        case MEASURE_TYPE.LINE_WIDTH_14:
                        case MEASURE_TYPE.LINE_WIDTH_23:
                        case MEASURE_TYPE.LINE_WIDTH_13:
                        case MEASURE_TYPE.LINE_WIDTH_1234:
                        case MEASURE_TYPE.ARC_LINE_WIDTH:
                            if (1 == m_nGraphType)
                                dllapi_get_line_info_in_gerber_hv(measure_type, graph_x, graph_y, ret_ints, ret_doubles);
                            else if (2 == m_nGraphType)
                                dllapi_get_line_info_in_ODB_hv(measure_type, ret_data.m_bIsFromODBAttribute, ret_data.m_metric_line_width[0], graph_x, graph_y, ret_ints, ret_doubles);
                            break;

                        default:
                            return false;
                    }

                    if (0 == ret_ints[0])
                    {
                        MessageBox.Show(this, "获取鼠标位置图元信息失败，请检查原因或使用手拉框。", "提示");
                        return false;
                    }
                }
            }
            #endregion
            
            // 填充测量点数据结构
            MeasurePointData data = new MeasurePointData();
            #region
            if (false == bIsDrawnLine)
            {
                data.m_ID = m_measure_items_on_graph.Count + 1;
                data.m_mes_type = measure_type;
                data.m_name = get_measure_type_name(data.m_mes_type);
                
                data.m_create_mode = (1 == m_nGraphType) ? 1 : 2;
                data.m_nOrientation = m_nGraphOrientation;
                data.m_graphic_type = (enum_graphic_element_type)ret_ints[1];
                data.m_nGraphBytesPerLine = ret_ints[2];
                data.m_nGraphOffsetX = ret_ints[3];
                data.m_nGraphOffsetY = ret_ints[4];
                data.m_nGraphWidth = ret_ints[5];
                data.m_nGraphHeight = ret_ints[6];
                data.m_nGraphZoomRatio = m_nGraphZoomRatio;
                data.m_unit = m_nUnitType;

                data.m_strGraphFileName = Path.GetFileName(m_strGraphFilePath);

                //Debugger.Log(0, null, string.Format("222222 data.m_nGraphOffsetX [{0},{1}]", data.m_nGraphOffsetX, data.m_nGraphOffsetY));
                
                switch (measure_type)
                {
                    case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                        data.m_center_x_on_graph = ret_doubles[4];
                        data.m_center_y_on_graph = ret_doubles[5];
                        data.m_line_width_in_pixels = ret_doubles[6] * 2;
                        data.m_line_width_in_metric = ret_doubles[7] * 2;
                        data.m_pixels_per_mm = ret_doubles[9];
                        data.m_metric_radius[0] = data.m_line_width_in_pixels * 1000 / data.m_pixels_per_mm;
                        data.m_metric_radius_upper[0] = data.m_metric_radius[0] * (100 + m_dbUpperDeltaPercent) / 100;
                        data.m_metric_radius_lower[0] = data.m_metric_radius[0] * (100 - m_dbLowerDeltaPercent) / 100;
                        break;

                    case MEASURE_TYPE.LINE_WIDTH_14:
                    case MEASURE_TYPE.LINE_WIDTH_23:
                    case MEASURE_TYPE.LINE_WIDTH_13:
                    case MEASURE_TYPE.LINE_WIDTH_1234:
                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                        data.m_line_start.x = ret_doubles[0];
                        data.m_line_start.y = ret_doubles[1];
                        data.m_line_end.x = ret_doubles[2];
                        data.m_line_end.y = ret_doubles[3];
                        data.m_center_x_on_graph = ret_doubles[4];
                        data.m_center_y_on_graph = ret_doubles[5];
                        data.m_line_width_in_pixels = ret_doubles[6] * 2;
                        data.m_line_width_in_metric = ret_doubles[7] * 2;
                        data.m_line_angle_on_graph = ret_doubles[8];
                        data.m_pixels_per_mm = ret_doubles[9];
                        data.m_metric_line_width[0] = data.m_line_width_in_pixels * 1000 / data.m_pixels_per_mm;
                        data.m_metric_line_width_upper[0] = data.m_metric_line_width[0] * (100 + m_dbUpperDeltaPercent) / 100;
                        data.m_metric_line_width_lower[0] = data.m_metric_line_width[0] * (100 - m_dbLowerDeltaPercent) / 100;

                        if (MEASURE_TYPE.LINE_WIDTH_1234 == measure_type)
                        {
                            data.m_metric_line_width[1] = data.m_line_width_in_pixels * 1000 / data.m_pixels_per_mm;
                            data.m_metric_line_width_upper[1] = data.m_metric_line_width[0] * (100 + m_dbUpperDeltaPercent) / 100;
                            data.m_metric_line_width_lower[1] = data.m_metric_line_width[0] * (100 - m_dbLowerDeltaPercent) / 100;
                        }
                        break;

                    case MEASURE_TYPE.LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_SPACE:
                        data.m_center_x_on_graph = ret_doubles[4];
                        data.m_center_y_on_graph = ret_doubles[5];
                        data.m_line_width_in_pixels = ret_doubles[6];
                        data.m_line_angle_on_graph = ret_doubles[8];
                        data.m_pixels_per_mm = ret_doubles[9];
                        data.m_line_width_in_metric = (ret_doubles[6] * 1000) / data.m_pixels_per_mm;
                        data.m_metric_line_width[0] = data.m_line_width_in_pixels * 1000 / data.m_pixels_per_mm;
                        data.m_metric_line_width_upper[0] = data.m_metric_line_width[0] * (100 + m_dbUpperDeltaPercent) / 100;
                        data.m_metric_line_width_lower[0] = data.m_metric_line_width[0] * (100 - m_dbLowerDeltaPercent) / 100;
                        break;
                }

                // 将图形坐标换算成物理坐标
                data.m_center_x_in_metric = (data.m_center_x_on_graph - (double)data.m_nGraphOffsetX) / data.m_pixels_per_mm;
                data.m_center_y_in_metric = ((double)(data.m_nGraphHeight - GRAPH_Y_OFFSET_SUM) - (data.m_center_y_on_graph
                    - (double)data.m_nGraphOffsetY)) / data.m_pixels_per_mm;

                //Debugger.Log(0, null, string.Format("222222 data.m_center_x_in_metric = [{0:0.000},{1:0.000}], {2:0.0000000}, {3}, {4}", 
                //    data.m_center_x_in_metric, data.m_center_y_in_metric, data.m_line_width_in_metric, ret_doubles[6], data.m_pixels_per_mm));

                // 是否是标准线
                //if ((enum_graphic_element_type.graphic_line == data.m_graphic_type) || (enum_graphic_element_type.graphic_aperture_circle_line == data.m_graphic_type))
                if (enum_graphic_element_type.graphic_aperture_circle_line == data.m_graphic_type)
                {
                    data.m_bIsNormalLine = true;
                }

                //string msg = string.Format("222222 graph_x [{0},{1}], 线类型 = {2}, 线宽 = {3:0.000} px 或 {4:0.000} mm, 线中心 = [{5:0.0},{6:0.0}], 线中心物理坐标 = [{7:0.000},{8:0.000}]",
                //    graph_x, graph_y, data.m_graphic_type, data.m_line_width_in_pixels, data.m_line_width_in_metric,
                //    data.m_center_x_on_graph, data.m_center_y_on_graph, data.m_center_x_in_metric, data.m_center_y_in_metric);
                //Debugger.Log(0, null, msg);
                //string msg = string.Format("222222 ccccc m_nGraphWidth [{0},{1}], m_nGraphOffsetX [{2},{3}], m_nGraphBytesPerLine {4},  m_pixels_per_mm = {5:0.000}",
                //    data.m_nGraphWidth, data.m_nGraphHeight, data.m_nGraphOffsetX, data.m_nGraphOffsetY,
                //    data.m_nGraphBytesPerLine, data.m_pixels_per_mm);
                //Debugger.Log(0, null, msg);
            }
            #endregion

            // 基于线段宽度，计算适合的倍率，并生成选取框
            #region
            if (false == bIsDrawnLine)
            {
                bool bRequireExit = false;
                if (enum_graphic_element_type.graphic_line_space == data.m_graphic_type)
                {

                }
                else if ((enum_graphic_element_type.graphic_aperture_circle == data.m_graphic_type) ||
                    (enum_graphic_element_type.graphic_circle == data.m_graphic_type))
                {
                    if (data.m_line_width_in_pixels > 0.01)
                    {
                        double factor = 0.7 + (double)m_nSmallSearchFrameExtent * 0.05;
                        double radius = data.m_line_width_in_pixels * factor;

                        data.m_fit_rect_on_graph[0].x = data.m_center_x_on_graph - (radius / 2);
                        data.m_fit_rect_on_graph[0].y = data.m_center_y_on_graph - (radius / 2);
                        data.m_fit_rect_on_graph[1].x = data.m_center_x_on_graph + (radius / 2);
                        data.m_fit_rect_on_graph[1].y = data.m_center_y_on_graph - (radius / 2);
                        data.m_fit_rect_on_graph[2].x = data.m_center_x_on_graph + (radius / 2);
                        data.m_fit_rect_on_graph[2].y = data.m_center_y_on_graph + (radius / 2);
                        data.m_fit_rect_on_graph[3].x = data.m_center_x_on_graph - (radius / 2);
                        data.m_fit_rect_on_graph[3].y = data.m_center_y_on_graph + (radius / 2);

                        for (int k = 0; k < 4; k++)
                        {
                            data.m_graphmade_ROI_rect[k].x = (data.m_fit_rect_on_graph[k].x - (double)data.m_nGraphOffsetX) / data.m_pixels_per_mm;
                            data.m_graphmade_ROI_rect[k].y = ((double)(data.m_nGraphHeight - GRAPH_Y_OFFSET_SUM) -
                                (data.m_fit_rect_on_graph[k].y - (double)data.m_nGraphOffsetY)) / data.m_pixels_per_mm;
                        }

                        double image_width = ((data.m_line_width_in_pixels * 1000 * 2) / data.m_pixels_per_mm);

                        //Debugger.Log(0, null, string.Format("222222 image_width = {0}, {1}", image_width, data.m_line_width_in_pixels));
                        
                        double min_dist = 100000;
                        int min_dist_index = -1;
                        for (int k = 0; k < 6; k++)
                        {
                            double dist = Math.Abs(array_ratio_widths[k] - image_width);
                            if (dist < min_dist)
                            {
                                min_dist = dist;
                                min_dist_index = k;
                            }
                        }

                        if (-1 != min_dist_index)
                        {
                            if (true == bAddMarkPt)
                                data.m_len_ratio = 0;
                            else
                                data.m_len_ratio = min_dist_index;
                            
                            ret_data = data;

                            return true;
                        }
                    }
                }
                else if ((enum_graphic_element_type.graphic_line == data.m_graphic_type)
                    || (enum_graphic_element_type.graphic_aperture_circle_line == data.m_graphic_type))
                {
                    if (data.m_line_width_in_pixels > 0.01)
                    {
                        //double image_width = ((data.m_line_width_in_pixels * 1000 * 2) / data.m_pixels_per_mm) * 4;
                        double image_width = ((data.m_line_width_in_pixels * 1000 * 1) / data.m_pixels_per_mm) * 4;
                        double min_dist = 100000;
                        int min_dist_index = -1;
                        for (int k = 0; k < 6; k++)
                        {
                            double dist = Math.Abs(array_ratio_widths[k] - image_width);
                            if (dist < min_dist)
                            {
                                min_dist = dist;
                                min_dist_index = k;
                            }
                        }
                        if (-1 != min_dist_index)
                        {
                            data.m_len_ratio = min_dist_index;
                            //Debugger.Log(0, null, string.Format("222222 data.m_len_ratio = {0}", data.m_len_ratio));
                        }
                    }
                    
                    // 生成选取框
                    if (data.m_line_width_in_metric > 0.001)
                    {
                        //if (true == data.m_bIsNormalLine)
                        {
                            //double factor = 1.0 + (double)m_nSmallSearchFrameExtent / 10;
                            //double window_height = factor * data.m_line_width_in_pixels;
                            //double window_width = window_height * 1.6;

                            //if (data.m_line_width_in_metric > 1000)
                            //    window_width = window_height * 0.8;

                            double w_factor = 0.35 + (double)m_nSmallSearchFrameExtent / 10;
                            double h_factor = 0.35 + (double)m_nBigSearchFrameExtent / 10;
                            double window_width = w_factor * data.m_line_width_in_pixels;
                            double window_height = h_factor * data.m_line_width_in_pixels;
                            
                            Point2d[] pts = new Point2d[4];
                            Point2d[] new_pts = new Point2d[4];
                            new_pts[0].x = -window_width / 2.0;
                            new_pts[0].y = -window_height / 2.0;
                            new_pts[1].x = window_width / 2.0;
                            new_pts[1].y = new_pts[0].y;
                            new_pts[2].x = new_pts[1].x;
                            new_pts[2].y = window_height / 2.0;
                            new_pts[3].x = new_pts[0].x;
                            new_pts[3].y = new_pts[2].y;

                            for (int k = 0; k < 4; k++)
                            {
                                double[] in_crds = new double[2];
                                double[] out_crds = new double[2];
                                in_crds[0] = new_pts[k].x;
                                in_crds[1] = new_pts[k].y;
                                rotate_crd(in_crds, out_crds, data.m_line_angle_on_graph);

                                pts[k].x = data.m_center_x_on_graph;
                                pts[k].y = data.m_center_y_on_graph;
                                pts[k].x += out_crds[0];
                                pts[k].y += out_crds[1];
                            }

                            data.m_fit_rect_on_graph[0] = pts[0];
                            data.m_fit_rect_on_graph[1] = pts[3];
                            data.m_fit_rect_on_graph[2] = pts[2];
                            data.m_fit_rect_on_graph[3] = pts[1];

                            //for (int k = 0; k < 4; k++)
                            //{
                            //    Debugger.Log(0, null, string.Format("222222 k = {0}: pts [{1:0.000},{2:0.000}]", k, pts[k].x, pts[k].y));
                            //}

                            for (int k = 0; k < 4; k++)
                            {
                                data.m_graphmade_ROI_rect[k].x = (data.m_fit_rect_on_graph[k].x - (double)data.m_nGraphOffsetX) / data.m_pixels_per_mm;
                                data.m_graphmade_ROI_rect[k].y = ((double)(data.m_nGraphHeight - GRAPH_Y_OFFSET_SUM) -
                                    (data.m_fit_rect_on_graph[k].y - (double)data.m_nGraphOffsetY)) / data.m_pixels_per_mm;
                                //Debugger.Log(0, null, string.Format("222222 k = {0}: mes_pt_data.m_fit_rect [{1:0.0},{2:0.0}]",
                                //    k, data.m_graphmade_ROI_rect[k].x, data.m_graphmade_ROI_rect[k].y));
                            }

                            ret_data = data;

                            return true;
                        }
                    }
                }

                if ((MEASURE_TYPE.LINE_SPACE == measure_type) || (MEASURE_TYPE.ARC_LINE_SPACE == measure_type))
                {
                    if (data.m_line_width_in_pixels > 0.01)
                    {
                        double image_width = ((data.m_line_width_in_pixels * 1000 * 2) / data.m_pixels_per_mm) * 5;
                        double min_dist = 100000;
                        int min_dist_index = -1;
                        for (int k = 0; k < 6; k++)
                        {
                            double dist = Math.Abs(array_ratio_widths[k] - image_width);
                            if (dist < min_dist)
                            {
                                min_dist = dist;
                                min_dist_index = k;
                            }
                        }
                        if (-1 != min_dist_index)
                        {
                            data.m_len_ratio = min_dist_index;
                            //Debugger.Log(0, null, string.Format("222222 mes_pt_data.m_len_ratio = {0}", mes_pt_data.m_len_ratio));
                        }
                    }

                    // 生成选取框
                    if (data.m_line_width_in_metric > 0.001)
                    {
                        //double factor = 1.3 + (double)m_nSmallSearchFrameExtent / 10;
                        //double window_height = factor * data.m_line_width_in_pixels;
                        //double window_width = window_height * 1.3;
                        
                        //if (data.m_line_width_in_metric > 1000)
                        //    window_width = window_height * 0.8;
                        //window_height = (factor + 0.3) * data.m_line_width_in_pixels;

                        double w_factor = 0.35 + (double)m_nSmallSearchFrameExtent / 10;
                        double h_factor = 0.35 + (double)m_nBigSearchFrameExtent / 10;
                        double window_width = w_factor * data.m_line_width_in_pixels;
                        double window_height = h_factor * data.m_line_width_in_pixels;

                        Point2d[] pts = new Point2d[4];
                        Point2d[] new_pts = new Point2d[4];
                        new_pts[0].x = -window_width / 2.0;
                        new_pts[0].y = -window_height / 2.0;
                        new_pts[1].x = window_width / 2.0;
                        new_pts[1].y = new_pts[0].y;
                        new_pts[2].x = new_pts[1].x;
                        new_pts[2].y = window_height / 2.0;
                        new_pts[3].x = new_pts[0].x;
                        new_pts[3].y = new_pts[2].y;

                        for (int k = 0; k < 4; k++)
                        {
                            double[] in_crds = new double[2];
                            double[] out_crds = new double[2];
                            in_crds[0] = new_pts[k].x;
                            in_crds[1] = new_pts[k].y;
                            rotate_crd(in_crds, out_crds, data.m_line_angle_on_graph);

                            pts[k].x = data.m_center_x_on_graph;
                            pts[k].y = data.m_center_y_on_graph;
                            pts[k].x += out_crds[0];
                            pts[k].y += out_crds[1];
                        }

                        data.m_fit_rect_on_graph[0] = pts[0];
                        data.m_fit_rect_on_graph[1] = pts[3];
                        data.m_fit_rect_on_graph[2] = pts[2];
                        data.m_fit_rect_on_graph[3] = pts[1];

                        for (int k = 0; k < 4; k++)
                        {
                            data.m_graphmade_ROI_rect[k].x = (data.m_fit_rect_on_graph[k].x - (double)data.m_nGraphOffsetX) / data.m_pixels_per_mm;
                            data.m_graphmade_ROI_rect[k].y = ((double)(data.m_nGraphHeight - GRAPH_Y_OFFSET_SUM) -
                                (data.m_fit_rect_on_graph[k].y - (double)data.m_nGraphOffsetY)) / data.m_pixels_per_mm;
                            //string msg = string.Format("222222 k = {0}: mes_pt_data.m_fit_rect [{1:0.0},{2:0.0}]", k, data.m_fit_rect[k].x, data.m_fit_rect[k].y);
                            //Debugger.Log(0, null, msg);
                        }

                        ret_data = data;

                        return true;
                    }
                }
            }
            #endregion

            if (true == bIsDrawnLine)
            {
                int drawn_line_type = 0;
                Point2d drawn_line_center = new Point2d(0, 0);
                double drawn_line_width = 0;
                double drawn_line_angle = 0;

                data.m_ID = m_measure_items_on_graph.Count + 1;
                data.m_mes_type = measure_type;
                data.m_name = get_measure_type_name(data.m_mes_type);

                data.m_create_mode = (1 == m_nGraphType) ? 1 : 2;
                data.m_nOrientation = m_nGraphOrientation;
                data.m_nGraphBytesPerLine = m_nBytesPerLine;
                data.m_nGraphOffsetX = m_nGraphOffsetX;
                data.m_nGraphOffsetY = m_nGraphOffsetY;
                data.m_nGraphWidth = m_nGraphWidth;
                data.m_nGraphHeight = m_nGraphHeight;
                data.m_nGraphZoomRatio = m_nGraphZoomRatio;
                data.m_unit = m_nUnitType;

                data.m_strGraphFileName = Path.GetFileName(m_strGraphFilePath);

                Point2d start_pt = m_graph_view.m_start_pt_for_drawn_line;
                Point2d end_pt = m_graph_view.m_end_pt_for_drawn_line;
                start_pt.x += graph_x;
                start_pt.y += graph_y;
                end_pt.x += graph_x;
                end_pt.y += graph_y;

                switch (measure_type)
                {
                    case MEASURE_TYPE.LINE_WIDTH_14:
                        data.m_graphic_type = enum_graphic_element_type.graphic_line;
                        data.m_nTopBrightness = m_top_light.m_nBrightness;
                        data.m_nBottomBrightness = m_bottom_light.m_nBrightness;
                        if (0 == m_nLightTypeFor14Line)
                        {
                            data.m_bIsTopLightOn = true;
                            data.m_bIsBottomLightOn = false;
                        }
                        else
                        {
                            data.m_bIsTopLightOn = false;
                            data.m_bIsBottomLightOn = true;
                        }

                        data.m_center_x_on_graph = (start_pt.x + end_pt.x) / 2;
                        data.m_center_y_on_graph = (start_pt.y + end_pt.y) / 2;
                        data.m_line_width_in_pixels = Math.Sqrt((start_pt.x - end_pt.x) * (start_pt.x - end_pt.x) + (start_pt.y - end_pt.y) * (start_pt.y - end_pt.y));
                        data.m_line_width_in_metric = data.m_line_width_in_pixels / m_pixels_per_mm;
                        data.m_pixels_per_mm = m_pixels_per_mm;
                        data.m_metric_line_width[0] = data.m_line_width_in_pixels * 1000 / m_pixels_per_mm;
                        data.m_metric_line_width_upper[0] = data.m_metric_line_width[0] * (100 + m_dbUpperDeltaPercent) / 100;
                        data.m_metric_line_width_lower[0] = data.m_metric_line_width[0] * (100 - m_dbLowerDeltaPercent) / 100;
                        //Debugger.Log(0, null, string.Format("222222 m_center_x_on_graph [{0:0.000},{1:0.000}]", data.m_center_x_on_graph, data.m_center_y_on_graph));
                        
                        // 将图形坐标换算成物理坐标
                        data.m_center_x_in_metric = (data.m_center_x_on_graph - (double)data.m_nGraphOffsetX) / data.m_pixels_per_mm;
                        data.m_center_y_in_metric = ((double)(data.m_nGraphHeight - GRAPH_Y_OFFSET_SUM) - (data.m_center_y_on_graph
                            - (double)data.m_nGraphOffsetY)) / data.m_pixels_per_mm;

                        if (data.m_line_width_in_pixels > 0.01)
                        {
                            //double image_width = ((data.m_line_width_in_pixels * 1000 * 2) / data.m_pixels_per_mm) * 4;
                            double image_width = ((data.m_line_width_in_pixels * 1000 * 1) / data.m_pixels_per_mm) * 4;
                            double min_dist = 100000;
                            int min_dist_index = -1;
                            for (int k = 0; k < 6; k++)
                            {
                                double dist = Math.Abs(array_ratio_widths[k] - image_width);
                                if (dist < min_dist)
                                {
                                    min_dist = dist;
                                    min_dist_index = k;
                                }
                            }
                            if (-1 != min_dist_index)
                            {
                                data.m_len_ratio = min_dist_index;
                                //Debugger.Log(0, null, string.Format("222222 data.m_len_ratio = {0}", data.m_len_ratio));
                            }
                        }

                        // 生成选取框
                        if (data.m_line_width_in_metric > 0.001)
                        {
                            //if (true == data.m_bIsNormalLine)
                            {
                                //double factor = 1.0 + (double)m_nSmallSearchFrameExtent / 10;
                                //double window_height = factor * data.m_line_width_in_pixels;
                                //double window_width = window_height * 1.6;

                                //if (data.m_line_width_in_metric > 1000)
                                //    window_width = window_height * 0.8;

                                double w_factor = 0.35 + (double)m_nSmallSearchFrameExtent / 10;
                                double h_factor = 0.35 + (double)m_nBigSearchFrameExtent / 10;
                                double window_width = w_factor * data.m_line_width_in_pixels;
                                double window_height = h_factor * data.m_line_width_in_pixels;

                                Point2d[] pts = new Point2d[4];
                                Point2d[] new_pts = new Point2d[4];
                                new_pts[0].x = -window_width / 2.0;
                                new_pts[0].y = -window_height / 2.0;
                                new_pts[1].x = window_width / 2.0;
                                new_pts[1].y = new_pts[0].y;
                                new_pts[2].x = new_pts[1].x;
                                new_pts[2].y = window_height / 2.0;
                                new_pts[3].x = new_pts[0].x;
                                new_pts[3].y = new_pts[2].y;

                                for (int k = 0; k < 4; k++)
                                {
                                    double[] in_crds = new double[2];
                                    double[] out_crds = new double[2];
                                    in_crds[0] = new_pts[k].x;
                                    in_crds[1] = new_pts[k].y;
                                    rotate_crd(in_crds, out_crds, data.m_line_angle_on_graph);

                                    pts[k].x = data.m_center_x_on_graph;
                                    pts[k].y = data.m_center_y_on_graph;
                                    pts[k].x += out_crds[0];
                                    pts[k].y += out_crds[1];
                                }

                                data.m_fit_rect_on_graph[0] = m_graph_view.m_drawn_line_pts[1];
                                data.m_fit_rect_on_graph[1] = m_graph_view.m_drawn_line_pts[0];
                                data.m_fit_rect_on_graph[2] = m_graph_view.m_drawn_line_pts[3];
                                data.m_fit_rect_on_graph[3] = m_graph_view.m_drawn_line_pts[2];

                                for (int k = 0; k < 4; k++)
                                {
                                    data.m_fit_rect_on_graph[k].x += graph_x;
                                    data.m_fit_rect_on_graph[k].y += graph_y;
                                }

                                //for (int k = 0; k < 4; k++)
                                //{
                                //    Debugger.Log(0, null, string.Format("222222 k = {0}: m_graph_view.m_drawn_line_pts [{1:0.000},{2:0.000}]",
                                //        k, m_graph_view.m_drawn_line_pts[k].x, m_graph_view.m_drawn_line_pts[k].y));
                                //}

                                for (int k = 0; k < 4; k++)
                                {
                                    data.m_graphmade_ROI_rect[k].x = (data.m_fit_rect_on_graph[k].x - (double)data.m_nGraphOffsetX) / data.m_pixels_per_mm;
                                    data.m_graphmade_ROI_rect[k].y = ((double)(data.m_nGraphHeight - GRAPH_Y_OFFSET_SUM) -
                                        (data.m_fit_rect_on_graph[k].y - (double)data.m_nGraphOffsetY)) / data.m_pixels_per_mm;
                                    //Debugger.Log(0, null, string.Format("222222 k = {0}: mes_pt_data.m_fit_rect [{1:0.0},{2:0.0}]",
                                    //    k, data.m_graphmade_ROI_rect[k].x, data.m_graphmade_ROI_rect[k].y));
                                }

                                ret_data = data;

                                return true;
                            }
                        }
                        break;
                }
            }

            return false;
        }
        
        // 刷新测量项图标
        public void refresh_measure_item_icons()
        {
            if (MEASURE_TYPE.NONE != m_prev_measure_type)
            {
                switch (m_prev_measure_type)
                {
                    case MEASURE_TYPE.LINE:
                        this.ui_btn_Line.Image = m_images_for_line[0];
                        break;
                    case MEASURE_TYPE.LINE_WIDTH_14:
                        this.ui_btn_14LineWidth.Image = m_images_for_line_width_14[0];
                        break;
                    case MEASURE_TYPE.LINE_WIDTH_23:
                        this.ui_btn_23LineWidth.Image = m_images_for_line_width_23[0];
                        break;
                    case MEASURE_TYPE.LINE_WIDTH_13:
                        this.ui_btn_13LineWidth.Image = m_images_for_line_width_13[0];
                        Form form = null;
                        if (true == GeneralUtils.check_if_form_is_open("Form_Calibration", ref form))
                        {
                            ((Form_Calibration)form).ui_btn_13LineWidth.Image = m_images_for_line_width_13[0];
                        }
                        break;
                    case MEASURE_TYPE.LINE_WIDTH_1234:
                        this.ui_btn_1234LineWidth.Image = m_images_for_line_width_1234[0];
                        break;
                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                        this.ui_btn_ArcLineWidth.Image = m_images_for_arc_line_width[0];
                        break;
                    case MEASURE_TYPE.HAND_PICK_LINE:
                        this.ui_btn_Pt2PtDistance.Image = m_images_for_pt_2_pt_distance[0];
                        break;
                    case MEASURE_TYPE.LINE_SPACE:
                        this.ui_btn_LineSpace.Image = m_images_for_line_space[0];
                        break;
                    case MEASURE_TYPE.ARC_LINE_SPACE:
                        this.ui_btn_ArcLineSpace.Image = m_images_for_line_space[0];
                        break;
                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                        this.ui_btn_CircleOuterToInner.Image = m_images_for_circle_outer_to_inner[0];
                        break;
                    case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                        this.ui_btn_CircleInnerToOuter.Image = m_images_for_circle_inner_to_outer[0];
                        break;
                    case MEASURE_TYPE.HAND_PICK_CIRCLE:
                        this.ui_btn_HandPickCircle.Image = m_images_for_hand_pick_circle[0];
                        break;
                }
            }

            switch (m_current_measure_type)
            {
                case MEASURE_TYPE.LINE_WIDTH_14:
                    this.ui_btn_14LineWidth.Image = m_images_for_line_width_14[2];
                    break;
                case MEASURE_TYPE.LINE_WIDTH_23:
                    this.ui_btn_23LineWidth.Image = m_images_for_line_width_23[2];
                    break;
                case MEASURE_TYPE.LINE_WIDTH_13:
                    this.ui_btn_13LineWidth.Image = m_images_for_line_width_13[2];
                    break;
                case MEASURE_TYPE.LINE_WIDTH_1234:
                    this.ui_btn_1234LineWidth.Image = m_images_for_line_width_1234[2];
                    break;
                case MEASURE_TYPE.LINE_SPACE:
                    this.ui_btn_LineSpace.Image = m_images_for_line_space[2];
                    break;
                case MEASURE_TYPE.ARC_LINE_SPACE:
                    this.ui_btn_ArcLineSpace.Image = m_images_for_line_space[2];
                    break;
                case MEASURE_TYPE.HAND_PICK_LINE:
                    this.ui_btn_Pt2PtDistance.Image = m_images_for_pt_2_pt_distance[2];
                    break;
                case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                    this.ui_btn_CircleOuterToInner.Image = m_images_for_circle_outer_to_inner[2];
                    break;
                case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                    this.ui_btn_CircleInnerToOuter.Image = m_images_for_circle_inner_to_outer[2];
                    break;
                case MEASURE_TYPE.HAND_PICK_CIRCLE:
                    this.ui_btn_HandPickCircle.Image = m_images_for_hand_pick_circle[2];
                    break;
                case MEASURE_TYPE.LINE:
                    this.ui_btn_Line.Image = m_images_for_line[2];
                    break;
            }
        }

        // 设置当前测量类，产生测量类的实例
        public void set_gauger(MEASURE_TYPE gauger_type)
        {
            if (null != m_gauger)
                m_gauger.clear_gauger_state();
            ui_MainImage.Refresh();
            
            if (gauger_type != m_current_measure_type)
            {
                if ((MEASURE_TYPE.CIRCLE_OUTER_TO_INNER == gauger_type) && (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == m_current_measure_type))
                    return;
                //Debugger.Log(0, null, string.Format("222222 222 gauger_type = {0}, m_current_measure_type = {1}", gauger_type, m_current_measure_type));

                switch (gauger_type)
                {
                    case MEASURE_TYPE.LINE:
                        m_gauger = new Gauger_Line(this, ui_MainImage, gauger_type);
                        break;
                    case MEASURE_TYPE.LINE_WIDTH_14:
                        m_gauger = new Gauger_LineWidth(this, ui_MainImage, gauger_type);
                        break;
                    case MEASURE_TYPE.LINE_WIDTH_23:
                        m_gauger = new Gauger_LineWidth(this, ui_MainImage, gauger_type);
                        break;
                    case MEASURE_TYPE.LINE_WIDTH_13:
                        m_gauger = new Gauger_LineWidth(this, ui_MainImage, gauger_type);
                        break;
                    case MEASURE_TYPE.LINE_WIDTH_1234:
                        m_gauger = new Gauger_LineWidth(this, ui_MainImage, gauger_type);
                        break;
                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                        m_gauger = new Gauger_ArcLineWidth(this, ui_MainImage, gauger_type);
                        break;
                    case MEASURE_TYPE.LINE_SPACE:
                        m_gauger = new Gauger_LineSpace(this, ui_MainImage, gauger_type);
                        break;
                    case MEASURE_TYPE.ARC_LINE_SPACE:
                        m_gauger = new Gauger_ArcLineSpace(this, ui_MainImage, gauger_type);
                        break;
                    case MEASURE_TYPE.HAND_PICK_LINE:
                        m_gauger = new Gauger_HandPickLine(this, ui_MainImage);
                        break;
                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                        m_gauger = new Gauger_CircleOuterToInner(this, ui_MainImage);
                        break;
                    case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                        m_gauger = new Gauger_CircleInnerToOuter(this, ui_MainImage);
                        break;
                    case MEASURE_TYPE.HAND_PICK_CIRCLE:
                        m_gauger = new Gauger_HandPickCircle(this, ui_MainImage);
                        break;
                }

                m_prev_measure_type = m_current_measure_type;
                m_current_measure_type = gauger_type;

                refresh_measure_item_icons();
            }
        }
        
        // 获取测量点集所包含定位孔的数目
        public int get_fiducial_mark_count(List<MeasurePointData> data)
        {
            int nMarkCount = 0;
            for (int n = 0; n < data.Count; n++)
            {
                if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == data[n].m_mes_type)
                    nMarkCount++;
            }

            return nMarkCount;
        }

        // 计算仿射变换矩阵，list_data前三项必须是定位孔
        public bool generate_transform_matrix_by_three_pts(List<MeasurePointData> list_data, ref double[] matrix)
        {
            int[] out_ints = new int[10];
            double[] in_theory_crds = new double[10];
            double[] in_real_crds = new double[10];
            
            for (int k = 0; k < 3; k++)
            {
                in_theory_crds[k * 2] = list_data[k].m_center_x_in_metric;
                in_theory_crds[k * 2 + 1] = list_data[k].m_center_y_in_metric;
                in_real_crds[k * 2] = list_data[k].m_real_machine_crd.x;
                in_real_crds[k * 2 + 1] = list_data[k].m_real_machine_crd.y;
                Debugger.Log(0, null, string.Format("222222 in_theory_crds = [{0:0.000},{1:0.000}], [{2:0.000},{3:0.000}]",
                            list_data[k].m_center_x_in_metric, list_data[k].m_center_y_in_metric,
                            list_data[k].m_real_machine_crd.x, list_data[k].m_real_machine_crd.y));
            }

            out_ints[0] = 0;
            get_transform_matrix(out_ints, in_theory_crds, in_real_crds, matrix);
            if (1 == out_ints[0])
            {
                return true;
            }
            else
                return false;
        }

        // 计算仿射变换矩阵
        public bool generate_transform_matrix_by_three_pts(List<StageGraphCrdPair> list_pairs, ref double[] matrix)
        {
            int[] out_ints = new int[10];
            double[] in_theory_crds = new double[10];
            double[] in_real_crds = new double[10];

            if (list_pairs.Count < 3)
                return false;

            for (int k = 0; k < 3; k++)
            {
                in_theory_crds[k * 2] = list_pairs[k].graph_crd.x;
                in_theory_crds[k * 2 + 1] = list_pairs[k].graph_crd.y;
                in_real_crds[k * 2] = list_pairs[k].stage_crd.x;
                in_real_crds[k * 2 + 1] = list_pairs[k].stage_crd.y;
                Debugger.Log(0, null, string.Format("222222 in_theory_crds = [{0:0.000},{1:0.000}], [{2:0.000},{3:0.000}]",
                            list_pairs[k].graph_crd.x, list_pairs[k].graph_crd.y, list_pairs[k].stage_crd.x, list_pairs[k].stage_crd.y));
            }

            out_ints[0] = 0;
            get_transform_matrix(out_ints, in_theory_crds, in_real_crds, matrix);
            if (1 == out_ints[0])
            {
                return true;
            }
            else
                return false;
        }

        // 打开/关闭   吸附/鼓风机
        public bool hardware_ops_enable_vacuum(bool bOn)
        {
            if (false == m_IO.set_IO_output(m_IO.m_output_vacuum, (true == bOn) ? IO_STATE.IO_LOW : IO_STATE.IO_HIGH))
            {
                MessageBox.Show(this, "吸附IO设置失败，请检查原因。", "提示");
                return false;
            }

            //吸附按钮灯光
            m_IO.set_IO_output(m_IO.m_output_vacuum_button, (true == bOn) ? IO_STATE.IO_LOW : IO_STATE.IO_HIGH);
            

            return true;
        }

        // 移动测高传感器到相机位置，并执行一次测高
        public void move_height_sensor_to_camera_pos_and_detect_height()
        {
            Point3d target_crd = new Point3d();
            m_motion.get_xyz_crds(ref target_crd);

            target_crd.x += m_dbCameraHeightSensorOffsetX;
            target_crd.y += m_dbCameraHeightSensorOffsetY;

            m_motion.linear_XYZ_wait_until_stop(target_crd.x, target_crd.y, target_crd.z, false);

            double vel = m_motion.m_axes[MotionOps.AXIS_Z - 1].vel_for_long_range;

            m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, m_dbStageTriggerHeight + 15, vel);

            Debugger.Log(0, null, string.Format("222222 m_IO.is_height_sensor_activated() = {0}", m_IO.is_height_sensor_activated()));

            if (true == m_IO.is_height_sensor_activated())
            {
                m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, m_dbStageTriggerHeight - 3, 7);

                for (int m = 0; m < 1000; m++)
                {
                    if (false == m_IO.is_height_sensor_activated())
                    {
                        Point3d crd = new Point3d(0, 0, 0);
                        m_motion.get_xyz_crds(ref crd);

                        m_dbClearPlanePosZ = crd.z - m_dbStageHeightGap;

                        Thread.Sleep(150);
                        m_motion.stop_axis(MotionOps.AXIS_Z);
                        Thread.Sleep(150);

                        break;
                    }

                    Thread.Sleep(5);
                }
            }

            if (true == m_IO.is_height_sensor_activated())
            {
                MessageBox.Show(this, "传感器触发高度设置有问题，可能触发高度过大或过小，请检查。", "提示");

                m_bIsMeasuringDuringCreation = false;
                return;
            }
            else
            {

            }
        }

        // 读取同料号奇偶层三个定位孔记录
        public void read_three_marks_records_from_file(string strFileName, List<ThreeMarksRecord> list_three_marks_records)
        {
            try
            {
                if (!File.Exists(strFileName))
                {
                    Debugger.Log(0, null, string.Format("222222 找不到定位孔记录文件 {0}，请检查原因!", strFileName));
                    return;
                }

                list_three_marks_records.Clear();

                StreamReader reader = new StreamReader(strFileName, false);

                Debugger.Log(0, null, string.Format("222222 path = {0}", strFileName));

                int counter = 0;
                string strGraphName;
                while (true)
                {
                    string line = reader.ReadLine();
                    if (null == line)
                        break;
                    
                    string[] array = line.Split(',');
                    
                    if (array.Length > 15)
                    {
                        ThreeMarksRecord record = new ThreeMarksRecord();

                        record.m_strOdbFileName = array[0];
                        record.m_nEvenOddFlag = Convert.ToInt32(array[1]);

                        for (int k = 0; k < 3; k++)
                            record.m_dbDiameterInMM[k] = Convert.ToDouble(array[2 + k]);

                        for (int k = 0; k < 3; k++)
                            record.m_bIsTopLightOn[k] = Convert.ToBoolean(array[5 + k]);
                        for (int k = 0; k < 3; k++)
                            record.m_bIsBottomLightOn[k] = Convert.ToBoolean(array[8 + k]);
                        for (int k = 0; k < 3; k++)
                            record.m_nTopBrightness[k] = Convert.ToInt32(array[11 + k]);
                        for (int k = 0; k < 3; k++)
                            record.m_nBottomBrightness[k] = Convert.ToInt32(array[14 + k]);
                        
                        for (int k = 0; k < 3; k++)
                        {
                            record.m_marks_pt_on_graph[k].x = Convert.ToDouble(array[17 + k * 2]);
                            record.m_marks_pt_on_graph[k].y = Convert.ToDouble(array[17 + k * 2 + 1]);
                        }

                        for (int k = 0; k < 3; k++)
                        {
                            record.m_marks_pt_on_stage[k].x = Convert.ToDouble(array[23 + k * 3]);
                            record.m_marks_pt_on_stage[k].y = Convert.ToDouble(array[23 + k * 3 + 1]);
                            record.m_marks_pt_on_stage[k].z = Convert.ToDouble(array[23 + k * 3 + 2]);
                        }

                        list_three_marks_records.Add(record);
                    }

                    Debugger.Log(0, null, string.Format("222222 list_three_marks_records.Count {0}", list_three_marks_records.Count));

                    //for (int n = 0; n < array.Length; n++)
                    //{
                    //    Debugger.Log(0, null, string.Format("222222 n {0}: {1}", n, array[n]));
                    //}
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 read_three_marks_records_from_file() error message: {0}", ex.Message));
            }
        }

        // 保存同料号奇偶层三个定位孔记录
        public void save_three_marks_records_to_file(List<ThreeMarksRecord> list_three_marks_records, string strFileName)
        {
            try
            {
                StreamWriter writer = new StreamWriter(strFileName, true);
                
                for (int n = 0; n < list_three_marks_records.Count; n++)
                {
                    string str = "";
                    str = string.Concat(str, string.Format("{0},", list_three_marks_records[n].m_strOdbFileName));
                    str = string.Concat(str, string.Format("{0},", list_three_marks_records[n].m_nEvenOddFlag));

                    for (int k = 0; k < 3; k++)
                        str = string.Concat(str, string.Format("{0:0.000},", list_three_marks_records[n].m_dbDiameterInMM[n]));

                    for (int k = 0; k < 3; k++)
                        str = string.Concat(str, string.Format("{0},", list_three_marks_records[n].m_bIsTopLightOn[n]));
                    for (int k = 0; k < 3; k++)
                        str = string.Concat(str, string.Format("{0},", list_three_marks_records[n].m_bIsBottomLightOn[n]));
                    for (int k = 0; k < 3; k++)
                        str = string.Concat(str, string.Format("{0},", list_three_marks_records[n].m_nTopBrightness[n]));
                    for (int k = 0; k < 3; k++)
                        str = string.Concat(str, string.Format("{0},", list_three_marks_records[n].m_nBottomBrightness[n]));

                    for (int k = 0; k < 3; k++)
                    {
                        str = string.Concat(str, string.Format("{0},", list_three_marks_records[n].m_marks_pt_on_graph[k].x));
                        str = string.Concat(str, string.Format("{0},", list_three_marks_records[n].m_marks_pt_on_graph[k].y));
                    }
                    for (int k = 0; k < 3; k++)
                    {
                        str = string.Concat(str, string.Format("{0},", list_three_marks_records[n].m_marks_pt_on_stage[k].x));
                        str = string.Concat(str, string.Format("{0},", list_three_marks_records[n].m_marks_pt_on_stage[k].y));

                        if ((3 - 1) == k)
                            str = string.Concat(str, string.Format("{0}", list_three_marks_records[n].m_marks_pt_on_stage[k].z));
                        else
                            str = string.Concat(str, string.Format("{0},", list_three_marks_records[n].m_marks_pt_on_stage[k].z));
                    }

                    writer.WriteLine(str);
                }

                writer.Close();
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 save_three_marks_records_to_file() error message: {0}", ex.Message));
            }
        }

        // 对ODB自动测量项进行排序，使最前面三个测量项都是定位孔，并且符合左下、左上、右上的顺序
        public void rearrange_ODB_measure_items_order(ref List<ODBMeasureItem> list_ODB_measure_items)
        {
            List<ODBMeasureItem> list_items = new List<ODBMeasureItem>();
            
            double min_dist = 1000000;
            int nMinDistIdx = -1;
            for (int n = 0; n < list_ODB_measure_items.Count; n++)
            {
                if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == list_ODB_measure_items[n].nMeasureType)
                {
                    Point2d pt1 = new Point2d(list_ODB_measure_items[n].dbGraphCrdX, list_ODB_measure_items[n].dbGraphCrdY);
                    Point2d pt2 = new Point2d(0, 0);

                    double dist = GeneralUtils.get_distance(pt1, pt2);
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        nMinDistIdx = n;
                    }
                    //Debugger.Log(0, null, string.Format("222222 n = {0}: [{1:0.000},{2:0.000}]", n, list_ODB_measure_items[n].dbGraphCrdX, list_ODB_measure_items[n].dbGraphCrdY));
                }
            }
            if (-1 == nMinDistIdx)
                return;
            list_items.Add(list_ODB_measure_items[nMinDistIdx]);
            list_ODB_measure_items.RemoveAt(nMinDistIdx);
            
            min_dist = 1000000;
            nMinDistIdx = -1;
            for (int n = 0; n < list_ODB_measure_items.Count; n++)
            {
                if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == list_ODB_measure_items[n].nMeasureType)
                {
                    Point2d pt1 = new Point2d(list_ODB_measure_items[n].dbGraphCrdX, list_ODB_measure_items[n].dbGraphCrdY);
                    Point2d pt2 = new Point2d(0, 2000);

                    double dist = GeneralUtils.get_distance(pt1, pt2);
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        nMinDistIdx = n;
                    }
                }
            }
            if (-1 == nMinDistIdx)
                return;
            list_items.Add(list_ODB_measure_items[nMinDistIdx]);
            list_ODB_measure_items.RemoveAt(nMinDistIdx);
            
            min_dist = 1000000;
            nMinDistIdx = -1;
            for (int n = 0; n < list_ODB_measure_items.Count; n++)
            {
                if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == list_ODB_measure_items[n].nMeasureType)
                {
                    Point2d pt1 = new Point2d(list_ODB_measure_items[n].dbGraphCrdX, list_ODB_measure_items[n].dbGraphCrdY);
                    Point2d pt2 = new Point2d(2000, 2000);

                    double dist = GeneralUtils.get_distance(pt1, pt2);
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        nMinDistIdx = n;
                    }
                }
            }
            if (-1 == nMinDistIdx)
                return;
            list_items.Add(list_ODB_measure_items[nMinDistIdx]);
            list_ODB_measure_items.RemoveAt(nMinDistIdx);
            
            for (int n = 0; n < list_ODB_measure_items.Count; n++)
            {
                if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == list_ODB_measure_items[n].nMeasureType)
                {
                    list_ODB_measure_items.RemoveAt(n);
                    n--;
                }
                else
                    list_items.Add(list_ODB_measure_items[n]);
            }

            for (int n = 0; n < list_items.Count; n++)
            {
                Debugger.Log(0, null, string.Format("222222 n = {0}: type = {1}, [{2:0.000},{3:0.000}]", n, list_items[n].nMeasureType, list_items[n].dbGraphCrdX, list_items[n].dbGraphCrdY));
            }

            list_ODB_measure_items = list_items;
        }
    }
}
