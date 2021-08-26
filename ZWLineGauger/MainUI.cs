
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Data.SqlClient;
using PMSGigE = PMSAPI.PMSGigE;
using PMSSTATUS = PMSAPI.PMSSTATUS_CODES;
using PMSImage = PMSAPI.PMSImage;

using ZWLineGauger.Forms;
using ZWLineGauger.Gaugers;
using ZWLineGauger.Hardwares;
using ZWLineGauger.Misc;

using HalconDotNet;

namespace ZWLineGauger
{
    public partial class MainUI : Form
    {
        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool get_theta(double start_x, double start_y, double end_x, double end_y, double[] out_doubles);

        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool rotate_crd(double[] in_crds, double[] out_crds, double rotate_angle);

        [DllImport("pxflow.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool read_image_for_ui_mainview(char[] path, int view_width, int view_height, byte[] pRetBuf);

        [DllImport("pxflow.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static void set_mv_width_height(int width, int height);

        [DllImport("pxflow.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool find_line_in_main_view(double[] in_crds, double[] out_crds);

        const string   VERSION = "0.8.23.2";
        
        const double DEFAULT_ROTATED_RECT_WH_RATIO = 1.7;

        string   m_file_path_for_14_normal = "icons\\14线宽.png";
        string   m_file_path_for_14_hovered = "icons\\14线宽-hovered.png";
        string   m_file_path_for_14_pressed = "icons\\14线宽-pressed.png";
        string   m_file_path_for_23_normal = "icons\\23线宽.png";
        string   m_file_path_for_23_hovered = "icons\\23线宽-hovered.png";
        string   m_file_path_for_23_pressed = "icons\\23线宽-pressed.png";
        string   m_file_path_for_13_normal = "icons\\13线宽.png";
        string   m_file_path_for_13_hovered = "icons\\13线宽-hovered.png";
        string   m_file_path_for_13_pressed = "icons\\13线宽-pressed.png";
        string   m_file_path_for_1234_normal = "icons\\1234线宽.png";
        string   m_file_path_for_1234_hovered = "icons\\1234线宽-hovered.png";
        string   m_file_path_for_1234_pressed = "icons\\1234线宽-pressed.png";
        string   m_file_path_for_linespace_normal = "icons\\线距.png";
        string   m_file_path_for_linespace_hovered = "icons\\线距-hovered.png";
        string   m_file_path_for_linespace_pressed = "icons\\线距-pressed.png";
        string   m_file_path_for_CircleOuter2Inner_normal = "icons\\由外向内找圆.png";
        string   m_file_path_for_CircleOuter2Inner_hovered = "icons\\由外向内找圆-hovered.png";
        string   m_file_path_for_CircleOuter2Inner_pressed = "icons\\由外向内找圆-pressed.png";
        string   m_file_path_for_CircleInner2Outer_normal = "icons\\由内向外找圆.png";
        string   m_file_path_for_CircleInner2Outer_hovered = "icons\\由内向外找圆-hovered.png";
        string   m_file_path_for_CircleInner2Outer_pressed = "icons\\由内向外找圆-pressed.png";
        string   m_file_path_for_Pt2Pt_normal = "icons\\二点距离.png";
        string   m_file_path_for_Pt2Pt_hovered = "icons\\二点距离-hovered.png";
        string   m_file_path_for_Pt2Pt_pressed = "icons\\二点距离-pressed.png";
        string   m_file_path_for_HandPickCircle_normal = "icons\\手动三点选圆.png";
        string   m_file_path_for_HandPickCircle_hovered = "icons\\手动三点选圆-hovered.png";
        string   m_file_path_for_HandPickCircle_pressed = "icons\\手动三点选圆-pressed.png";
        string   m_file_path_for_light_off = "icons\\light-off.bmp";
        string   m_file_path_for_light_on = "icons\\light-on.bmp";
        string m_file_path_for_HandLL_level_hovered = "icons\\水平线-hovered.png";
        string m_file_path_for_HandLL_level_normal = "icons\\水平线-pressed.png";
        string m_file_path_for_HandLL_vertical_hovered = "icons\\竖直线-hovered.png";
        string m_file_path_for_HandLL_vertical_normal = "icons\\竖直线-pressed.png";
        string m_file_path_for_HandLL_hovered = "icons\\角度平行线-hovered.png";
        string m_file_path_for_HandLL_normal = "icons\\角度平行线-pressed.png";
        string m_file_path_for_HandPL_level_hovered = "icons\\点水平线-hovered.png";
        string m_file_path_for_HandPL_level_normal = "icons\\点水平线-pressed.png";
        string m_file_path_for_HandPL_vertical_hovered = "icons\\点竖直线-hovered.png";
        string m_file_path_for_HandPL_vertical_normal = "icons\\点竖直线-pressed.png";
        string m_file_path_for_HandPL_hovered = "icons\\点角度平行线-hovered.png";
        string m_file_path_for_HandPL_normal = "icons\\点角度平行线-pressed.png";

        Image[] m_images_for_line_width_14 = new Image[3];
        Image[] m_images_for_line_width_23 = new Image[3];
        public Image[] m_images_for_line_width_13 = new Image[3];
        Image[] m_images_for_line_width_1234 = new Image[3];
        Image[] m_images_for_line = new Image[3];
        Image[] m_images_for_arc_line_width = new Image[3];
        Image[] m_images_for_line_space = new Image[3];
        Image[] m_images_for_arc_line_space = new Image[3];
        Image[] m_images_for_short_space_between_two_empty_circles = new Image[3];
        Image[] m_images_for_circle_outer_to_inner = new Image[3];
        Image[] m_images_for_circle_inner_to_outer = new Image[3];
        Image[] m_images_for_pt_2_pt_distance = new Image[3];
        Image[] m_images_for_hand_pick_circle = new Image[3];
        Image m_images_for_light_on;
        Image m_images_for_light_off;

        Image[] m_images_for_HandLL = new Image[6];
        Image[] m_images_for_HandPL = new Image[6];
        Bitmap  m_mv_bitmap;

        public ToolTip   m_tooltip_for_measure_item = new ToolTip();
        ToolTip   m_tooltip_for_top_light_button = new ToolTip();
        ToolTip   m_tooltip_for_bottom_light_button = new ToolTip();
        ToolTip   m_tooltip_top_light_value = new ToolTip();
        ToolTip   m_tooltip_bottom_light_value = new ToolTip();
        ToolTip   m_tooltip_small_search_frame_value = new ToolTip();
        ToolTip   m_tooltip_big_search_frame_value = new ToolTip();
        
        bool   m_bTriggerSaveGaugeImage = false;
        
        public bool   m_bSaveGaugeResultImage = false;
        public bool   m_bSaveGaugeResultExcelReport = false;

        int   m_nSmallSearchFrameExtent = 1;
        int   m_nBigSearchFrameExtent = 1;

        int   m_nJogSpeedRatio = 2;

        // 图纸
        static public String m_strGraphFilePath = "";
        static public int   m_nGraphZoomRatio = 80000;
        
        public String   m_strAppParamsPath = "configs\\sys.ini";
        public String   m_strCalibDataPath = "configs\\calib.cfg";
        public String   m_strMotionParamsPath = "configs\\motion.cfg";
        public String   m_strMainCameraFile = "configs\\main_cam.cfg";
        public String   m_strGuideCameraFile = "configs\\guide_cam.cfg";
        public String   m_strTopLightFile = "configs\\top_light.cfg";
        public String   m_strBottomLightFile = "configs\\bottom_light.cfg";
        public String   m_strIOConfigPath = "configs\\IO.cfg";
        public String   m_strLenConfigPath = "configs\\len.cfg";
        public String   m_strUserMessage = "configs\\User.cfg";  //用户
        public String   m_strTaskFileSavingDir = "";
        public String   m_strImageSavingDir = "";
        public String   m_strExcelSavingDir = "";
        public String   m_strGraphBrowseDir = "";
        public String   m_strOfflineFileBrowseDir = "";
        public String   m_strOpenImageBrowseDir = "";
        public String   m_strSaveImageBrowseDir = "";

        String m_strGeneralInfo = "";

        APP_Params m_app_params = new APP_Params();

        public bool m_bUseDatabase = true;
        public bool m_bIsSQLConnected = false;
        public SqlConnection   m_SQL_conn_measure_task;
        
        public delegate bool dl_message_sender(string info, bool bIsKeyInfo, object param1, object param2);
        public delegate int InvokeDraw();

        static public AutoResetEvent   m_reset_event_for_updating_thumbnail_progress = new AutoResetEvent(false);
        static public AutoResetEvent   m_reset_event_for_updating_graphview_progress = new AutoResetEvent(false);
        static public AutoResetEvent   m_event_suspend_hardware_init = new AutoResetEvent(false);
        static public AutoResetEvent   m_event_wait_for_confirm_during_autorun = new AutoResetEvent(false);
        static public AutoResetEvent   m_event_wait_for_confirm_during_creation = new AutoResetEvent(false);
        static public AutoResetEvent   m_event_wait_for_manual_gauge = new AutoResetEvent(false);
        static public AutoResetEvent   m_event_wait_for_finish_measurement = new AutoResetEvent(false);
        static public AutoResetEvent   m_event_wait_for_hardware_init = new AutoResetEvent(false);
        static public bool   m_bIsMonitorStarted = false;
        static public bool   m_bIsFormActivated = false;

        static public bool   m_bExitProgram = false;

        public Form_GraphOrientation   m_form_orientation;
        Form_ProgressInfo   m_form_graph_progress;
        Form_ProgressInfo   m_form_progress;

        static public GraphView m_graph_view;

        static public MotionOps   m_motion;
        public CameraOps   m_main_camera;
        public CameraOps   m_guide_camera;
        public LightOps   m_top_light;
        public LightOps   m_bottom_light;
        public LenOps     m_len;
        public IOOps       m_IO;
        public ImgOperators   m_image_operator;

        static public int   m_nCameraType = 1;                              // 0为Pomeas，1为MindVision

        public int   m_nCurrentUser = 0;                                        // 0为普通操作员，1为管理员，2位超级管理员
        public int   m_nFeedDogCounter = 0;                                // 如果看门狗计数达到120，即2分钟，那么退回普通操作员模式

        public int[]   m_nMeasureResultDigits = new int[3];         // 测量结果小数点位数

        public int   m_nUnitType = 0;

        public int   m_nLineMeasurementMethod = 0;                  // 0为按平行线段测量，1为按非平行线段测量，2为按实际轮廓测量
        public int   m_nAlgorithm = 0;                                           // 0为标准算法，1为陶瓷板、白底板专用算法

        public bool   m_bEmergencyExit = false;

        // 任务相关
        public double[]   m_calib_data = new double[6];
        public string       m_strTaskRunningStartingTime = "";     // 每次开始执行自动测量的时间
        public string       m_strTaskRunningStartingDataTime = "";     // 自动测量的日期
        public string       m_strTaskRunningStartingHMSTime = "";     // 自动测量的时分秒
        public bool   m_bIsRunningTask = false;                           // 是否正在执行自动测量
        public bool   m_bIsCreatingTask = false;                           // 是否正在制作首件
        public bool   m_bIsPreparingToCreateTask = false;          // 是否准备开始制作首件
        public bool   m_bStopTask = false;                                    // 是否停止任务
        public bool   m_bIsMeasuringDuringCreation = false;      // 是否正在进行首件测量
        public bool   m_bRepeatRunningTask = false;                   // 是否启用循环测量
        public int      m_nCreateTaskMode = 0;                             // 首件制作方式，0为配合图纸创建，1为手动创建，2为配合txt创建，3为ODB自动模式创建
        public int      m_nSourceOfStandardValue = 0;                  // 图纸测量项标准值的来源，0为来自图纸原始数据，1为来自图纸成像识别  
        public int      m_nCurrentMeasureItemIdx = 0;                  // 自动测量时当前测量项的索引号
        public int      m_nNumOfTaskRepetition = 10;                  // 循环测量次数
        
        double[]       m_triangle_trans_matrix = new double[10];  // 变换矩阵

        public int      m_nSmallSelectionFrameExtension = 50;    // 小搜索框的延长尺度
        public int      m_nBigSelectionFrameExtension = 30;        // 大搜索框的延长尺度

        public int      m_nMainCamUpperBrightness = 200;         // 图纸模式做首件时主图像的亮度上限
        public int      m_nMainCamLowerBrightness = 150;         // 图纸模式做首件时主图像的亮度下限
        public int      m_nGuideCamUpperBrightness = 200;       // 图纸模式做首件时导航图像的亮度上限
        public int      m_nGuideCamLowerBrightness = 150;       // 图纸模式做首件时导航图像的亮度下限
        public int      m_nLightTypeForGuideCamForMarkPt = 0; // 图纸模式做首件时，在导航图像中寻找定位孔所使用的光源类型，0为上环光，1为下环光
        public int      m_nLightTypeFor14Line = 0;                        // 图纸模式做首件时，测量下线宽默认使用的光源类型，0为上环光，1为下环光
        public int      m_nLightTypeForLineSpace = 0;                  // 图纸模式做首件时，测量线距默认使用的光源类型，0为上环光，1为下环光
        public int      m_nLightTypeForBGA = 0;                           // 图纸模式做首件时，测量BGA默认使用的光源类型，0为上环光，1为下环光

        public bool   m_bAutoAdjustLightDuringTaskCreation = true;     // 做首件时是否自动调节光源
        public bool   m_bLockLightWhenRunningTask = false;                // 跑任务时锁定光源，不按首件亮度

        public bool   m_bAbsoluteAllowance = true;                     // 上下限修改方式：true为按绝对值，false为按百分比
        public bool   m_bShowCoarseMark = false;                      // 是否在导航图像上高亮显示通过粗定位找到的定位孔，精度较粗糙
        public bool   m_bShowAccurateMark = false;                   // 是否在主图像上高亮显示识别到的定位孔，精度较高
        public bool   m_bIsWaitingForConfirm = false;                  // 自动测量线程是否处于挂起等待用户确认的状态
        public bool   m_bIsWaitingForUserManualGauge = false; // 自动测量线程是否处于挂起等待用户手动测量的状态
        public bool   m_bNeedConfirmFiducialMark;                     // 自动测量时是否需要确认定位孔
        public bool   m_bNeedConfirmMeasureResult;                  // 自动测量时是否需要确认测量结果
        public bool   m_bNeedConfirmNGWhenRunningTask = true;         // 自动测量时是否需要确认NG测量项
        public bool   m_bDoNotConfirmMarkAtCreation = false;               // 图纸模式做首件时，不需要用户确认定位孔
        public bool   m_bDoNotConfirmMeasureItemAtCreation = false;   // 图纸模式做首件时，不需要用户确认测量项
        public bool   m_bApplySameDeltaToBothLimits = false; // 标准值上下限自动同比例调整
        public bool   m_bShowSmallSelectionFrame = false;       // 是否在主图像上绘制小搜索框
        public bool   m_bShowBigSelectionFrame = false;           // 是否在主图像上绘制大搜索框
        public double    m_dbCoarseMarkRadius = 0;                                        // 在导航图像上识别到的定位孔的半径，精度较粗糙
        public double    m_dbAccurateMarkRadius = 0;                                     // 在主图像上识别到的定位孔的半径，精度较高
        public Point2d   m_ptAccurateMarkCenter = new Point2d(0, 0);            // 在主图像上识别到的定位孔的中心，精度较高
        public Point2d   m_ptSelectionFrameCenter = new Point2d(0, 0);         // 搜索框的中心

        public bool           m_bPlacedNewBoard = true;                                     // 如果光栅信号被触发，则认为操作员放入了新板子，在对焦之前需要重新测高

        Point3d                m_before_crd = new Point3d(0,0,0);

        public Point2d[]   m_len_ratios_offsets = new Point2d[10];                  // 不同倍率之间的偏移，以70倍为基准

        public bool   m_bShowFrameDuringTaskCreation = false;                 // 做首件时是否在主图像上绘制小搜索框
        MeasurePointData   m_current_data;                                                   // 做首件时当前测量项数据

        public bool   m_bIsAddingNewItemsToExistingTask = false;            // 是否处于正在添加新测量项到现有任务的状态

        public bool   m_bIsExecutingInBatch = false;                     // 正在批量执行
        public bool   m_bStopExecution = false;                            // 停止图纸测量项的批量测量

        bool              m_bMeasureNG = false;

        public double   m_dbDelaySecondsBeforeMeasure = 0.2; // 自动测量时，到达点位后的测量延迟时间，单位秒

        public bool   m_bDetectHeightOnce = false;
        public bool   m_bUseHeightSensor = false;                       // 自动对焦时是否使用高度传感器
        public double   m_dbStageHeightGap = 0;                        // 平台触发高度和对焦清晰平面高度之间的差值
        public double   m_dbStageTriggerHeight = 0;                    // 平台触发高度
        public double   m_dbClearPlanePosZ = 0;                         // 当前清晰高度
        public double   m_dbCameraHeightSensorOffsetX = 10;  // 相机与高度传感器之间的偏移X
        public double   m_dbCameraHeightSensorOffsetY = 10;  // 相机与高度传感器之间的偏移Y
        public double   m_dbBoardThickness = 0;                         // 基于高度传感器触发高度和原始标定高度所算出的板厚

        public bool   m_bUseRedDot = false;                                 // 使用红点对位
        public double m_dbRedDotOffsetX = 0;                             // 红点对位X偏移
        public double m_dbRedDotOffsetY = 0;                             // 红点对位Y偏移

        public int   m_nMeasureTaskDelayTime = 1000;                // 开始运行任务时，为确保吸附到位，第一次测量延迟时间(毫秒)

        public bool       m_bSelectivelySkipAutofocus = true;
        public bool       m_bUseAutofocusWhenRunningTask = false;
        public int          m_nThresForSkippingAutofocus = 88;

        public bool       m_bPopupModifyFormForODBTaskCreation = false;
        public int          m_nMaximumNumOfMeasureItemsForPopupModifyForm = 50;

        public static   int      m_nGraphOrientation = 0;                  // 图纸方向

        public bool       m_bFreezeCameraImage = false;               // 冻结相机图像
        public bool       m_bLocateLinesInRealTime = true;          // 是否显示实时抓边效果

        public List<string> m_vec_SQL_table_names = new List<string>();

        public int   m_nTaskInfoSourceType = 1;                          // 任务获取来源类型，0为从数据库获取，1为从文件目录获取
        public bool   m_bIsLoadedByBrowsingDir = false;

        public string m_strCurrentTaskFileFullPath = "";               // 通过浏览目录加载的任务文件的全路径名
        public string m_strCurrentTaskName = "";                        // 当前任务名
        public string m_strCurrentProductModel = "";                   // 当前料号
        public string m_strCurrentProductLayer = "";                    // 当前层别
        public string m_strCurrentProductStep = "";                    // 当前Step
        public string m_strCurrentProductNumber = "";                // 当前工单号

        public string m_strUseringName = "";                                //当前用户名
        public string m_strBatchNum = "";                                     //当前批次
        public string m_strUserRemark = "";                                     //当前用户备注

        public int   m_nUnitTypeBeforeRunningTask = 0;             // 运行任务之前的单位
        public string[] m_strUnits = new string[] { "mm", "um", "mil" };

        public double   m_current_sharpness_of_main_cam = 0;
        public double   m_current_brightness_of_main_cam = 0;
        public double   m_current_brightness_of_guide_cam = 0;

        public object   m_main_cam_lock = new object();
        public object   m_guide_cam_lock = new object();

        bool   m_bIsAppInited = false;
        bool   m_bIsManualVersion = false;
        public bool   m_bOfflineMode = false;
        public bool   m_bIgnoreHardwaresRelease = false;
        public bool   m_bPause = false;

        public ExcelUtil m_excel_ops = new ExcelUtil();

        public bool m_doubleclick_manual_measure = false;   //二次测量时判断是否需要手动测量
        public int m_doubleclick_manual_measure_index;

        // 消息处理
        public bool CBD_SendMessage(string info, bool bIsKeyInfo, object param1, object param2)
        {
            if (true == m_bExitProgram)
                return false;

            if (this.InvokeRequired)
            {
                dl_message_sender callback = new dl_message_sender(CBD_SendMessage);
                return (bool)(this.Invoke(callback, info, bIsKeyInfo, param1, param2));
            }
            else
            {
                if (true == bIsKeyInfo)
                {
                    string time = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + " " + DateTime.Now.ToLongTimeString().ToString();
                    textBox_KeyInfo.AppendText(time + " " + info + Environment.NewLine);
                }
                
                // 使按钮有效或无效
                if ("EnableButton(bool)" == info)
                {
                    ((Button)param1).Enabled = (bool)param2;
                }

                if ("刷新主图像" == info)
                {
                    ui_MainImage.Refresh();
                }
                else if ("登录" == info)
                {
                    switch ((int)param1)
                    {
                        case 1:
                            m_nCurrentUser = 0;
                            m_nFeedDogCounter = 0;
                            timer_UserFeedDogCounter.Enabled = false;

                            this.Text = "PixelLiner " + VERSION + " (操作员)";

                            //textBox_NumOfTaskRepetitions.Visible = false;
                            //checkBox_RepeatRunningTask.Visible = false;

                            //ui_menu_setting_Calibration.Enabled = false;
                            //ui_menu_setting_MotionControl.Enabled = false;
                            //ui_menu_setting_IO.Enabled = false;
                            //ui_menu_setting_CameraAndLight.Enabled = false;
                            //ui_menu_setting_HeightSensor.Enabled = false;
                            //ui_menu_setting_FileAndReport.Enabled = false;
                            //ui_menu_setting_Database.Enabled = false;
                            //ui_menu_setting_Graph.Enabled = false;
                            //ui_menu_setting_Misc.Enabled = false;
                            ////ui_menu_setting_CrossLine.Enabled = false;
                            //checkBox_UseAutofocus.Enabled = false;
                            //btn_Settings.Enabled = false;
                            m_strUseringName = (string)param2;
                            break;

                        case 0:
                            m_nCurrentUser = 1;
                            timer_UserFeedDogCounter.Enabled = true;

                            this.Text = "PixelLiner " + VERSION + " (管理员)";

                            //textBox_NumOfTaskRepetitions.Visible = true;
                            //checkBox_RepeatRunningTask.Visible = true;

                            //ui_menu_setting_Calibration.Enabled = true;
                            //ui_menu_setting_MotionControl.Enabled = true;
                            //ui_menu_setting_IO.Enabled = true;
                            //ui_menu_setting_CameraAndLight.Enabled = true;
                            //ui_menu_setting_HeightSensor.Enabled = true;
                            //ui_menu_setting_FileAndReport.Enabled = true;
                            //ui_menu_setting_Database.Enabled = true;
                            //ui_menu_setting_Graph.Enabled = true;
                            //ui_menu_setting_Misc.Enabled = true;
                            ////ui_menu_setting_CrossLine.Enabled = true;
                            //checkBox_UseAutofocus.Enabled = true;
                            //btn_Settings.Enabled = true;
                            m_strUseringName = (string)param2;
                            break;
                    }
                }
                else if("获取当前Steps与Layer" == info)
                {
                    m_strCurrentProductStep = (string)param1;
                    m_strCurrentProductLayer = (string)param2;
                }
                else if ("设置文本框内容" == info)
                {
                    ((TextBox)param1).Text = (string)param2;
                }
                else if ("开启或关闭十字显示" == info)
                {
                    if (true == (bool)param1)
                        ui_menu_setting_CrossLine.Text = "十字线";
                    else
                        ui_menu_setting_CrossLine.Text = "十字线    √";
                    ui_menu_setting_CrossLine_Click(new object(), new EventArgs());
                }
                else if ("恢复出厂设置" == info)
                {
                    return restore_factory_default();
                }
                else if("刷新图纸测量项列表" == info)
                {
                    if (3 == m_nCreateTaskMode)
                    {
                        make_visible_gridview_row_N(gridview_GraphMeasureItems, m_current_task_data.Count);
                    }
                    else if (2 == m_nCreateTaskMode)
                    {
                        show_task_on_gridview(gridview_GraphMeasureItems, m_measure_items_from_txt);
                        //Debugger.Log(0, null, string.Format("222222 (bool)param1 = {0}", (bool)param1));

                        make_visible_gridview_row_N(gridview_GraphMeasureItems, m_current_task_data.Count);
                    }
                    else
                    {
                        show_task_on_gridview(gridview_GraphMeasureItems, m_measure_items_on_graph);
                        //Debugger.Log(0, null, string.Format("222222 (bool)param1 = {0}", (bool)param1));

                        make_gridview_last_row_visible(gridview_GraphMeasureItems);

                        m_graph_view.refresh_image(true);
                    }
                }
                else if ("将gridview选中在第N行" == info)
                {
                    make_visible_gridview_row_N(gridview_GraphMeasureItems, (int)param1);

                    m_graph_view.refresh_image(true);
                }
                else if (info.Contains("图纸模式首件制作过程中添加测量项"))
                {
                    #region
                    MeasurePointData data = (MeasurePointData)param1;

                    switch (data.m_mes_type)
                    {
                        case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                        case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                            // 绘制搜索框
                            m_current_data = data;
                            m_bShowFrameDuringTaskCreation = true;
                            m_dbAccurateMarkRadius = data.m_metric_radius[0] * m_calib_data[data.m_len_ratio];
                            m_ptSelectionFrameCenter.x = ui_MainImage.Image.Width / 2;
                            m_ptSelectionFrameCenter.y = ui_MainImage.Image.Height / 2;

                            if (true == ((Gauger_CircleOuterToInner)m_gauger).gauge(ui_MainImage.Image, m_main_cam_lock,
                                m_ptSelectionFrameCenter, m_dbAccurateMarkRadius, m_nSmallSelectionFrameExtension))
                            {
                                //m_bShowAccurateMark = true;
                                m_ptAccurateMarkCenter = m_gauger.m_gauged_circle_center;

                                return true;
                            }
                            else
                            {
                                MessageBox.Show(this, "测量失败，请手动拉框测量，或者添加其它测量项。", "提示");
                                return false;
                            }
                            break;

                        case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                            // 绘制搜索框
                            m_current_data = data;
                            m_bShowFrameDuringTaskCreation = true;
                            m_dbAccurateMarkRadius = data.m_metric_radius[0] * m_calib_data[data.m_len_ratio];
                            m_ptSelectionFrameCenter.x = ui_MainImage.Image.Width / 2;
                            m_ptSelectionFrameCenter.y = ui_MainImage.Image.Height / 2;

                            if (true == ((Gauger_CircleInnerToOuter)m_gauger).gauge(ui_MainImage.Image, m_main_cam_lock,
                                m_ptSelectionFrameCenter, m_dbAccurateMarkRadius, m_nSmallSelectionFrameExtension))
                            {
                                //m_bShowAccurateMark = true;
                                m_ptAccurateMarkCenter = m_gauger.m_gauged_circle_center;

                                return true;
                            }
                            else
                            {
                                MessageBox.Show(this, "测量失败，请手动拉框测量，或者添加其它测量项。", "提示");
                                return false;
                            }
                            break;

                        case MEASURE_TYPE.LINE_WIDTH_14:
                        case MEASURE_TYPE.LINE_WIDTH_23:
                        case MEASURE_TYPE.LINE_WIDTH_13:
                        case MEASURE_TYPE.LINE_WIDTH_1234:
                        case MEASURE_TYPE.LINE_SPACE:
                        case MEASURE_TYPE.ARC_LINE_SPACE:
                        case MEASURE_TYPE.LINE:
                        case MEASURE_TYPE.ARC_LINE_WIDTH:
                        case MEASURE_TYPE.SHORT_SPACE_BETWEEN_TWO_EMPTY_CIRCLES:
                        case MEASURE_TYPE.L_SHAPE:
                        case MEASURE_TYPE.BULGE:
                        case MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR:
                        case MEASURE_TYPE.LINE_TO_EDGE:
                        case MEASURE_TYPE.ETCH_DOWN:
                        case MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE:
                        case MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE:
                        case MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE:
                        case MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE:
                            // 绘制搜索框
                            m_current_data = data;
                            m_bShowFrameDuringTaskCreation = true;
                            m_ptSelectionFrameCenter.x = ui_MainImage.Image.Width / 2;
                            m_ptSelectionFrameCenter.y = ui_MainImage.Image.Height / 2;

                            Point2d[] rect = new Point2d[4];
                            for (int n = 0; n < 4; n++)
                            {
                                double offset_x = data.m_graphmade_ROI_rect[n].x - data.m_center_x_in_metric;
                                double offset_y = data.m_graphmade_ROI_rect[n].y - data.m_center_y_in_metric;
                                rect[n].x = offset_x * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir);
                                rect[n].y = offset_y * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir);
                                rect[n].x = rect[n].x + ui_MainImage.Image.Width / 2;
                                rect[n].y = rect[n].y + ui_MainImage.Image.Height / 2;
                            }

                            ui_MainImage.Refresh();

                            try
                            {
                                bool bSuccess = false;
                                if (MEASURE_TYPE.LINE_SPACE == data.m_mes_type)
                                    bSuccess = ((Gauger_LineSpace)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension);
                                else if (MEASURE_TYPE.ARC_LINE_SPACE == data.m_mes_type)
                                    bSuccess = ((Gauger_ArcLineSpace)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension);
                                else if (MEASURE_TYPE.ARC_LINE_WIDTH == data.m_mes_type)
                                    bSuccess = ((Gauger_ArcLineWidth)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension);
                                else if (MEASURE_TYPE.SHORT_SPACE_BETWEEN_TWO_EMPTY_CIRCLES == data.m_mes_type)
                                    bSuccess = ((Gauger_ShortSpaceBetweenTwoEmptyCircles)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension);
                                else
                                    bSuccess = ((Gauger_LineWidth)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm);
                                
                                if (true == bSuccess)
                                {
                                    ui_MainImage.Refresh();
                                    return true;
                                }
                                else
                                {
                                    MessageBox.Show(this, "测量失败，请手动拉框测量，或者添加其它测量项。", "提示");
                                    return false;
                                }
                            }
                            catch (Exception ex)
                            {
                                Debugger.Log(0, null, string.Format("222222 测量过程发生异常，异常信息：{0}", ex.Message));
                                MessageBox.Show(this, "测量失败，请手动拉框测量，或者添加其它测量项。", "提示");
                                return false;
                            }
                            break;
                    }
                    #endregion
                }
                else if (info.Contains("图纸模式首件制作过程中在主图像找到测量对象"))
                {
                    #region
                    MeasurePointData data = (MeasurePointData)param1;
                    DialogResult reply = DialogResult.Yes;
                    double dbGaugedValue = 0;
                    bool bIsResultOK = true;

                    if (true == m_bNeedConfirmNGWhenRunningTask)
                    {
                        switch (data.m_mes_type)
                        {
                            case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                            case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                                dbGaugedValue = m_gauger.m_gauged_circle_radius / m_calib_data[data.m_len_ratio];
                                if ((dbGaugedValue >= data.m_metric_radius_lower[0]) && (dbGaugedValue <= data.m_metric_radius_upper[0]))
                                    ;
                                else
                                    bIsResultOK = false;
                                break;

                            case MEASURE_TYPE.LINE_WIDTH_14:
                            case MEASURE_TYPE.LINE_WIDTH_23:
                            case MEASURE_TYPE.LINE_WIDTH_13:
                            case MEASURE_TYPE.LINE_WIDTH_1234:
                            case MEASURE_TYPE.HAND_PICK_LINE:
                            case MEASURE_TYPE.LINE:
                            case MEASURE_TYPE.ARC_LINE_WIDTH:
                            case MEASURE_TYPE.SHORT_SPACE_BETWEEN_TWO_EMPTY_CIRCLES:
                            case MEASURE_TYPE.L_SHAPE:
                            case MEASURE_TYPE.BULGE:
                            case MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR:
                            case MEASURE_TYPE.LINE_TO_EDGE:
                            case MEASURE_TYPE.ETCH_DOWN:
                            case MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE:
                            case MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE:
                            case MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE:
                            case MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE:
                                dbGaugedValue = m_gauger.m_gauged_line_width / m_calib_data[data.m_len_ratio];
                                if ((dbGaugedValue >= data.m_metric_line_width_lower[0]) && (dbGaugedValue <= data.m_metric_line_width_upper[0]))
                                    ;
                                else
                                    bIsResultOK = false;
                                break;
                            case MEASURE_TYPE.LINE_SPACE:
                            case MEASURE_TYPE.ARC_LINE_SPACE:
                                dbGaugedValue = m_gauger.m_gauged_line_space / m_calib_data[data.m_len_ratio];
                                if ((dbGaugedValue >= data.m_metric_line_width_lower[0]) && (dbGaugedValue <= data.m_metric_line_width_upper[0]))
                                    ;
                                else
                                    bIsResultOK = false;
                                break;
                        }
                    }
                    
                    if (false == m_bDoNotConfirmMeasureItemAtCreation)
                        reply = MessageBox.Show(this, "是否添加该测量项到任务表?", "提示", MessageBoxButtons.YesNo);
                    else if ((true == m_bNeedConfirmNGWhenRunningTask) && (false == bIsResultOK))
                        reply = MessageBox.Show(this, "是否添加该测量项到任务表?", "提示", MessageBoxButtons.YesNo);

                    if (DialogResult.Yes == reply)
                    {
                        data.m_strStepsFileName = m_strCurrentProductStep;
                        data.m_strLayerFileName = m_strCurrentProductLayer;
                    
                        switch (data.m_mes_type)
                        {
                            case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                            case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                            case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                                if (true)
                                {
                                    Point2d center = m_gauger.m_gauged_circle_center;
                                    Point2d offset = new Point2d(0, 0);

                                    offset.x = center.x - (double)(m_main_camera.m_nCamWidth / 2);
                                    offset.y = center.y - (double)(m_main_camera.m_nCamHeight / 2);

                                    offset.x = (offset.x / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                                    offset.y = (offset.y / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                                    m_motion.get_xyz_crds(ref data.m_real_machine_crd);
                                    data.m_real_machine_crd.x += offset.x;
                                    data.m_real_machine_crd.y += offset.y;
                                    data.m_theory_machine_crd = data.m_real_machine_crd;
                                }
                                break;

                            case MEASURE_TYPE.LINE_WIDTH_14:
                            case MEASURE_TYPE.LINE_WIDTH_23:
                            case MEASURE_TYPE.LINE_WIDTH_13:
                            case MEASURE_TYPE.LINE_WIDTH_1234:
                            case MEASURE_TYPE.LINE_SPACE:
                            case MEASURE_TYPE.ARC_LINE_SPACE:
                            case MEASURE_TYPE.LINE:
                            case MEASURE_TYPE.ARC_LINE_WIDTH:
                            case MEASURE_TYPE.SHORT_SPACE_BETWEEN_TWO_EMPTY_CIRCLES:
                            case MEASURE_TYPE.L_SHAPE:
                            case MEASURE_TYPE.BULGE:
                            case MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR:
                            case MEASURE_TYPE.LINE_TO_EDGE:
                            case MEASURE_TYPE.ETCH_DOWN:
                            case MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE:
                            case MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE:
                            case MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE:
                            case MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE:
                                if (true)
                                {
                                    Point2d center = m_gauger.m_object_center;
                                    Point2d offset = new Point2d(0, 0);

                                    offset.x = center.x - (double)(m_main_camera.m_nCamWidth / 2);
                                    offset.y = center.y - (double)(m_main_camera.m_nCamHeight / 2);

                                    offset.x = (offset.x / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                                    offset.y = (offset.y / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                                    m_motion.get_xyz_crds(ref data.m_real_machine_crd);
                                    data.m_real_machine_crd.x += offset.x;
                                    data.m_real_machine_crd.y += offset.y;
                                    data.m_theory_machine_crd = data.m_real_machine_crd;
                                }
                                break;
                        }

                        data.m_thres_for_skipping_autofocus = m_nThresForSkippingAutofocus;
                        double sharpness = 0;
                        if (true == Gaugers.ImgOperators.get_image_sharpness(m_main_camera.m_pImageBuf,
                            m_main_camera.m_nCamWidth, m_main_camera.m_nCamHeight, ref sharpness))
                            data.m_sharpness_at_creation = sharpness;

                        data.m_ID = m_current_task_data.Count + 1;
                        data.m_unit = m_nUnitType;
                        data.m_bIsTopLightOn = m_top_light.m_bOn;
                        data.m_nTopBrightness = m_top_light.m_nBrightness;
                        data.m_bIsBottomLightOn = m_bottom_light.m_bOn;
                        data.m_nBottomBrightness = m_bottom_light.m_nBrightness;

                        data.m_strStepsFileName = m_strCurrentProductStep;
                        data.m_strLayerFileName = m_strCurrentProductLayer;

                        Debugger.Log(0, null, string.Format("222222 data.m_strStepsFileName = {0}", data.m_strStepsFileName));
                        Debugger.Log(0, null, string.Format("222222 data.m_strLayerFileName = {0}", data.m_strLayerFileName));

                        if (m_bIsMeasuringInArrayMode && m_bApplySameParamsToOtherUnits)
                        {
                            data.m_name = m_base_data_for_other_units.m_name;
                            data.m_metric_line_width[0] = m_base_data_for_other_units.m_metric_line_width[0];
                            data.m_metric_line_width_upper[0] = m_base_data_for_other_units.m_metric_line_width_upper[0];
                            data.m_metric_line_width_lower[0] = m_base_data_for_other_units.m_metric_line_width_lower[0];
                            data.m_metric_line_width[1] = m_base_data_for_other_units.m_metric_line_width[1];
                            data.m_metric_line_width_upper[1] = m_base_data_for_other_units.m_metric_line_width_upper[1];
                            data.m_metric_line_width_lower[1] = m_base_data_for_other_units.m_metric_line_width_lower[1];

                            data.m_metric_radius[0] = m_base_data_for_other_units.m_metric_radius[0];
                            data.m_metric_radius_upper[0] = m_base_data_for_other_units.m_metric_radius_upper[0];
                            data.m_metric_radius_lower[0] = m_base_data_for_other_units.m_metric_radius_lower[0];
                        }

                        m_current_task_data.Add(data);

                        // 在 gridview_MeasureTask 上显示刚添加的测量项数据
                        add_entry_to_task_gridview(gridview_MeasureTask, data);
                        make_gridview_last_row_visible(gridview_MeasureTask);
                        
                        if ((false == m_bIsMeasuringInArrayMode) || (false == m_bApplySameParamsToOtherUnits))
                        {
                            if ((false == data.m_bIsFromODBAttribute)
                                || ((true == data.m_bIsFromODBAttribute) && (true == m_bPopupModifyFormForODBTaskCreation) && (m_ODB_measure_items.Count < m_nMaximumNumOfMeasureItemsForPopupModifyForm)))
                            {
                                // 弹出标准值设置窗口
                                menuitem_ModifyTask_Click(new object(), new EventArgs());

                                if (false == Form_ModifyTask.m_bChooseAdd)
                                {
                                    m_current_task_data.RemoveAt(m_current_task_data.Count - 1);

                                    try
                                    {
                                        gridview_MeasureTask.Rows.RemoveAt(gridview_MeasureTask.Rows.Count - 2);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debugger.Log(0, null, string.Format("222222 弹出标准值设置窗口 ex = {0}", ex.Message));
                                    }
                                    return false;
                                }
                            }
                        }
                        
                        return true;
                    }
                    else
                    {
                        Debugger.Log(0, null, string.Format("222222 return false"));
                        return false;
                    }
                    #endregion
                }
                else if (info.Contains("图纸模式首件制作过程中测量定位孔"))
                {
                    #region
                    MeasurePointData data = (MeasurePointData)param1;
                    data.m_strStepsFileName = m_strCurrentProductStep;
                    data.m_strLayerFileName = m_strCurrentProductLayer;

                    // 绘制搜索框
                    m_current_data = data;
                    m_bShowFrameDuringTaskCreation = true;
                    m_dbAccurateMarkRadius = data.m_metric_radius[0] * m_calib_data[data.m_len_ratio];
                    m_ptSelectionFrameCenter.x = ui_MainImage.Image.Width / 2;
                    m_ptSelectionFrameCenter.y = ui_MainImage.Image.Height / 2;

                    ui_MainImage.Refresh();
                    
                    if (true == ((Gauger_CircleOuterToInner)m_gauger).gauge(ui_MainImage.Image, m_main_cam_lock,
                        m_ptSelectionFrameCenter, m_dbAccurateMarkRadius, m_nSmallSelectionFrameExtension))
                    {
                        //m_bShowAccurateMark = true;
                        m_ptAccurateMarkCenter = m_gauger.m_gauged_circle_center;

                        return true;
                    }
                    else
                        return false;
                    #endregion
                }
                else if (info.Contains("图纸模式首件制作过程中找到定位孔"))
                {
                    #region
                    DialogResult reply = DialogResult.Yes;
                    if (false == m_bDoNotConfirmMarkAtCreation)
                        reply = MessageBox.Show(this, "是否采用该定位孔?", "提示", MessageBoxButtons.YesNo);
                    
                    if (DialogResult.Yes == reply)
                    {
                        MeasurePointData data = (MeasurePointData)param1;
                        data.m_strStepsFileName = m_strCurrentProductStep;
                        data.m_strLayerFileName = m_strCurrentProductLayer;
                        Point2d center = m_gauger.m_gauged_circle_center;
                        Point2d offset = new Point2d(0, 0);

                        offset.x = center.x - (double)(m_main_camera.m_nCamWidth / 2);
                        offset.y = center.y - (double)(m_main_camera.m_nCamHeight / 2);

                        offset.x = (offset.x / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                        offset.y = (offset.y / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                        m_motion.get_xyz_crds(ref data.m_real_machine_crd);
                        data.m_real_machine_crd.x += offset.x;
                        data.m_real_machine_crd.y += offset.y;
                        data.m_theory_machine_crd = data.m_real_machine_crd;

                        data.m_thres_for_skipping_autofocus = m_nThresForSkippingAutofocus;
                        double sharpness = 0;
                        if (true == Gaugers.ImgOperators.get_image_sharpness(m_main_camera.m_pImageBuf,
                            m_main_camera.m_nCamWidth, m_main_camera.m_nCamHeight, ref sharpness))
                            data.m_sharpness_at_creation = sharpness;

                        data.m_unit = m_nUnitType;
                        data.m_bIsTopLightOn = m_top_light.m_bOn;
                        data.m_nTopBrightness = m_top_light.m_nBrightness;
                        data.m_bIsBottomLightOn = m_bottom_light.m_bOn;
                        data.m_nBottomBrightness = m_bottom_light.m_nBrightness;

                        data.m_strStepsFileName = m_strCurrentProductStep;
                        data.m_strLayerFileName = m_strCurrentProductLayer;

                        Debugger.Log(0, null, string.Format("222222 data.m_strStepsFileName = {0}", data.m_strStepsFileName));
                        Debugger.Log(0, null, string.Format("222222 data.m_strLayerFileName = {0}", data.m_strLayerFileName));

                        m_current_task_data.Add(data);

                        // 在 gridview_MeasureTask 上显示刚添加的测量项数据
                        add_entry_to_task_gridview(gridview_MeasureTask, data);
                        make_gridview_last_row_visible(gridview_MeasureTask);

                        // 更新 图纸区域
                        if (true == m_graph_view.m_bHasValidImage)
                        {
                            double x = 0, y = 0;
                            double zoom_ratio = m_graph_view.m_zoom_ratio_min * 7;
                            double current_view_width = (double)m_graph_view.m_bitmap_32bits.Width / zoom_ratio;
                            double current_view_height = (double)m_graph_view.m_bitmap_32bits.Height / zoom_ratio;
                            switch (get_fiducial_mark_count(m_current_task_data))
                            {
                                case 1:
                                    if (true == m_graph_view.find_corner_pos(1, ref x, ref y))
                                    {
                                        double left = x - current_view_width * 0.33;
                                        double top = y - current_view_height * 0.33;
                                        double x2 = left > 0 ? left : 0;
                                        double y2 = top > 0 ? top : 0;
                                        m_graph_view.set_view_ratio_and_crd(zoom_ratio, x2, y2);
                                    }
                                    else
                                    {
                                        x = (double)(m_nGraphOffsetX);
                                        y = (double)(m_nGraphOffsetY);
                                        m_graph_view.find_corner_pos(1, ref x, ref y);
                                        m_graph_view.set_view_ratio_and_crd(m_graph_view.m_zoom_ratio_min * 7, x, y);
                                    }
                                    break;

                                case 2:
                                    if (true == m_graph_view.find_corner_pos(2, ref x, ref y))
                                    {
                                        double left = x - current_view_width * 0.66;
                                        double top = y - current_view_height * 0.66;
                                        double x2 = left > 0 ? left : 0;
                                        double y2 = top > 0 ? top : 0;
                                        m_graph_view.set_view_ratio_and_crd(zoom_ratio, x2, y2);
                                    }
                                    else
                                    {
                                        x = (double)(m_graph_view.m_bitmap_1bit.Width - m_nGraphOffsetX * 2) * 6 / 7;
                                        y = (double)(m_nGraphOffsetY);
                                        m_graph_view.set_view_ratio_and_crd(m_graph_view.m_zoom_ratio_min * 7, m_graph_view.m_bitmap_1bit.Width * 6 / 7, 0);
                                    }
                                    break;

                                case 3:
                                    m_graph_view.set_view_ratio_and_crd(m_graph_view.m_zoom_ratio_min, 0, 0);
                                    break;
                            }
                            m_graph_view.refresh_image();
                        }

                        if (true == m_bDoNotConfirmMarkAtCreation)
                            ui_MainImage.Refresh();

                        return true;
                    }
                    else
                        return false;
                    #endregion
                }
                else if (info.Contains("准备测量 第"))
                {
                    #region
                    MeasurePointData data = (MeasurePointData)param1;

                    data.m_strStepsFileName = m_strCurrentProductStep;
                    data.m_strLayerFileName = m_strCurrentProductLayer;

                    add_entry_to_measure_result_gridview(gridview_measure_results, data);

                    if (gridview_measure_results.Rows.Count > 1)
                    {
                        gridview_measure_results.FirstDisplayedScrollingRowIndex = gridview_measure_results.Rows.Count - 1;
                    }
                    foreach (DataGridViewRow r in gridview_measure_results.Rows)
                    {
                        if (r.Index == (gridview_measure_results.RowCount - 2))
                            r.Selected = true;
                        else
                            r.Selected = false;
                    }
                    #endregion
                }
                else if (info.Contains("执行测量 第"))
                {
                    #region
                    MeasurePointData data = (MeasurePointData)param1;
                    int index = (int)param2;
                    toolStripStatusLabel_GraphInfo.Text = $"执行测量";

                    m_gauger.m_string_offset_real= data.m_string_Position_offset; 
                    data.m_strStepsFileName = m_strCurrentProductStep;
                    data.m_strLayerFileName = m_strCurrentProductLayer;

                    m_doubleclick_manual_measure_index = index;

                    switch (data.m_mes_type)
                    {
                        case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                        case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                        case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                        case MEASURE_TYPE.HAND_PICK_CIRCLE:
                            // 绘制搜索框
                            #region
                            m_bShowSmallSelectionFrame = true;
                            m_dbAccurateMarkRadius = data.m_metric_radius[0] * m_calib_data[data.m_len_ratio];
                            m_ptSelectionFrameCenter.x = ui_MainImage.Image.Width / 2;
                            m_ptSelectionFrameCenter.y = ui_MainImage.Image.Height / 2;

                            if (true == m_bOfflineMode)
                                ui_MainImage.Refresh();

                            if (MEASURE_TYPE.HAND_PICK_CIRCLE == data.m_mes_type)
                                return false;

                            bool bOK = false;
                            //Debugger.Log(0, null, string.Format("222222 bOK{0}", bOK));
                            if (MEASURE_TYPE.CIRCLE_INNER_TO_OUTER == data.m_mes_type)
                                bOK = ((Gauger_CircleInnerToOuter)m_gauger).gauge(ui_MainImage.Image, m_main_cam_lock,
                                        m_ptSelectionFrameCenter, m_dbAccurateMarkRadius, m_nSmallSelectionFrameExtension);
                            else
                                bOK = ((Gauger_CircleOuterToInner)m_gauger).gauge(ui_MainImage.Image, m_main_cam_lock,
                                        m_ptSelectionFrameCenter, m_dbAccurateMarkRadius, m_nSmallSelectionFrameExtension);
                            Debugger.Log(0, null, string.Format("222222 bOK{0}", bOK));
                            if (true == bOK)
                            {
                                if (false == m_bThreadIsRunning_MeasureOnceMore)
                                {
                                    double res = m_gauger.m_gauged_circle_radius / m_calib_data[data.m_len_ratio];

                                    if (false == m_bIsAddingNewItemsToExistingTask)
                                    {
                                        gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, gridview_measure_results.RowCount - 2].Value
                                            = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                        if ((res >= data.m_metric_radius_lower[0]) && (res <= data.m_metric_radius_upper[0]))
                                        {
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Style.BackColor = Color.FromArgb(0, 255, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Value = "OK";
                                        }
                                        else
                                        {
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Style.BackColor = Color.FromArgb(255, 0, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Value = "NG";
                                        }
                                    }

                                    m_ptAccurateMarkCenter = m_gauger.m_gauged_circle_center;

                                    //Debugger.Log(0, null, string.Format("222222 m_gauger.m_gauged_circle_center = [{0:0.000},{1:0.000}]",
                                    //    m_gauger.m_gauged_circle_center.x, m_gauger.m_gauged_circle_center.y));

                                    if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE != data.m_mes_type)
                                        m_bTriggerSaveGaugeImage = true;
                                }
                                else if (true == m_bThreadIsRunning_MeasureOnceMore)      // 重新测量
                                {
                                    if (DialogResult.Yes == MessageBox.Show(this, "是否采用新的测量结果?", "提示", MessageBoxButtons.YesNo))
                                    {
                                        double res = m_gauger.m_gauged_circle_radius / m_calib_data[data.m_len_ratio];
                                        gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, index].Value
                                                = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                        if ((res >= data.m_metric_radius_lower[0]) && (res <= data.m_metric_radius_upper[0]))
                                        {
                                            gridview_measure_results[RESULT_COLUMN_OKNG, index].Style.BackColor = Color.FromArgb(0, 255, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, index].Value = "OK";
                                        }
                                        else
                                        {
                                            gridview_measure_results[RESULT_COLUMN_OKNG, index].Style.BackColor = Color.FromArgb(255, 0, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, index].Value = "NG";
                                        }
                                    }
                                }
                                
                                ui_MainImage.Refresh();
                                return true;
                            }
                            else
                            {
                                if (false == m_bIsAddingNewItemsToExistingTask)
                                {
                                    gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, gridview_measure_results.RowCount - 2].Value = "0";
                                    gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Style.BackColor = Color.FromArgb(255, 0, 0);
                                    gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Value = "NG";
                                }
                                else if (true == m_bIsAddingNewItemsToExistingTask)//重新测量
                                {
                                    gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, index].Value = "0";
                                    gridview_measure_results[RESULT_COLUMN_OKNG, index].Style.BackColor = Color.FromArgb(255, 0, 0);
                                    gridview_measure_results[RESULT_COLUMN_OKNG, index].Value = "NG";
                                }
                                return false;
                            }
                            #endregion
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
                        case MEASURE_TYPE.SHORT_SPACE_BETWEEN_TWO_EMPTY_CIRCLES:
                        case MEASURE_TYPE.L_SHAPE:
                        case MEASURE_TYPE.BULGE:
                        case MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR:
                        case MEASURE_TYPE.LINE_TO_EDGE:
                        case MEASURE_TYPE.ETCH_DOWN:
                        case MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE:
                        case MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE:
                        case MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE:
                        case MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE:
                            #region
                            // 绘制搜索框
                            m_bShowSmallSelectionFrame = true;
                            m_ptSelectionFrameCenter.x = ui_MainImage.Image.Width / 2;
                            m_ptSelectionFrameCenter.y = ui_MainImage.Image.Height / 2;
                            
                            Point2d[] rect = new Point2d[4];
                            for (int n = 0; n < 4; n++)
                            {
                                Debugger.Log(0, null, string.Format("222222 data.m_handmade_ROI_rect[n] [{0:0.},{1:0.}]", data.m_handmade_ROI_rect[n].x, data.m_handmade_ROI_rect[n].y));

                                //if (0 == data.m_create_mode)
                                if (true)
                                {
                                    // 学习模式下的测量点
                                    rect[n].x = data.m_handmade_ROI_rect[n].x + m_base_template_pos.m_matched_pos.x;
                                    rect[n].y = data.m_handmade_ROI_rect[n].y + m_base_template_pos.m_matched_pos.y;
                                    
                                    //rect[n].x = data.m_handmade_ROI_rect[n].x * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir);
                                    //rect[n].y = data.m_handmade_ROI_rect[n].y * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir);
                                }
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                    {
                                        //Debugger.Log(0, null, string.Format("222222 双击测量来了5"));
                                        // 图纸模式自动生成的测量点
                                        double offset_x = data.m_graphmade_ROI_rect[n].x - data.m_center_x_in_metric;
                                        double offset_y = data.m_graphmade_ROI_rect[n].y - data.m_center_y_in_metric;
                                        rect[n].x = offset_x * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir);
                                        rect[n].y = offset_y * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir);
                                    }
                                    else
                                    {
                                        //Debugger.Log(0, null, string.Format("222222 双击测量来了6"));
                                        // 图纸模式下手动拉的测量点
                                        rect[n].x = data.m_handmade_ROI_rect[n].x * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir);
                                        rect[n].y = data.m_handmade_ROI_rect[n].y * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir);
                                    }
                                }
                                //rect[n].x = rect[n].x + ui_MainImage.Image.Width / 2;
                                //rect[n].y = rect[n].y + ui_MainImage.Image.Height / 2;
                                //Debugger.Log(0, null, string.Format("222222 双击测量来了7"));
                            }

                            ui_MainImage.Refresh();

                            switch (data.m_mes_type)
                            {
                                case MEASURE_TYPE.HAND_PICK_LINE:
                                case MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE:
                                case MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE:
                                case MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE:
                                case MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE:
                                    return false;
                                    break;
                            }

                            Debugger.Log(0, null, string.Format("222222 gauge 111"));
                            //Debugger.Log(0, null, string.Format("222222 gauge 111 data.m_mes_type = {0}, data.m_create_mode = {1}", data.m_mes_type, data.m_create_mode));
                            bool bSuccess = false;
                            if (MEASURE_TYPE.LINE_SPACE == data.m_mes_type)
                            {
                                if (0 == data.m_create_mode)
                                    bSuccess = ((Gauger_LineSpace)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, true);
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                        bSuccess = ((Gauger_LineSpace)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension);
                                    else
                                        bSuccess = ((Gauger_LineSpace)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, true);
                                }
                                //bSuccess = ((Gauger_LineSpace)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension);
                            }
                            else if (MEASURE_TYPE.ARC_LINE_SPACE == data.m_mes_type)
                            {
                                if (0 == data.m_create_mode)
                                    bSuccess = ((Gauger_ArcLineSpace)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, true);
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                        bSuccess = ((Gauger_ArcLineSpace)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension);
                                    else
                                        bSuccess = ((Gauger_ArcLineSpace)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, true);
                                }
                                //bSuccess = ((Gauger_LineSpace)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension);
                            }
                            else if (MEASURE_TYPE.LINE == data.m_mes_type)
                            {
                                if (0 == data.m_create_mode)
                                    bSuccess = ((Gauger_Line)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, true);
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                        bSuccess = ((Gauger_Line)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension);
                                    else
                                        bSuccess = ((Gauger_Line)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, true);
                                }
                            }
                            else if (MEASURE_TYPE.ARC_LINE_WIDTH == data.m_mes_type)
                            {
                                //Debugger.Log(0, null, string.Format("222222 data.m_create_mode = {0}, data.m_nGraphWidth = {1}", data.m_create_mode, data.m_nGraphWidth));
                                if (0 == data.m_create_mode)
                                    bSuccess = ((Gauger_ArcLineWidth)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, true);
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                        bSuccess = ((Gauger_ArcLineWidth)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension);
                                    else
                                        bSuccess = ((Gauger_ArcLineWidth)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, true);
                                }
                            }
                            else if (MEASURE_TYPE.SHORT_SPACE_BETWEEN_TWO_EMPTY_CIRCLES == data.m_mes_type)
                            {
                                //Debugger.Log(0, null, string.Format("222222 data.m_create_mode = {0}, data.m_nGraphWidth = {1}", data.m_create_mode, data.m_nGraphWidth));
                                if (0 == data.m_create_mode)
                                    bSuccess = ((Gauger_ShortSpaceBetweenTwoEmptyCircles)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, true);
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                        bSuccess = ((Gauger_ShortSpaceBetweenTwoEmptyCircles)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension);
                                    else
                                        bSuccess = ((Gauger_ShortSpaceBetweenTwoEmptyCircles)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, true);
                                }
                            }
                            else if (MEASURE_TYPE.L_SHAPE == data.m_mes_type)
                            {
                                if (0 == data.m_create_mode)
                                    bSuccess = ((Gauger_LShapeItem)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                        bSuccess = ((Gauger_LShapeItem)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm);
                                    else
                                        bSuccess = ((Gauger_LShapeItem)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                }
                            }
                            else if (MEASURE_TYPE.ETCH_DOWN == data.m_mes_type)
                            {
                                if (0 == data.m_create_mode)
                                    bSuccess = ((Gauger_EtchDown)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                        bSuccess = ((Gauger_EtchDown)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm);
                                    else
                                        bSuccess = ((Gauger_EtchDown)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                }
                            }
                            else if (MEASURE_TYPE.BULGE == data.m_mes_type)
                            {
                                if (0 == data.m_create_mode)
                                    bSuccess = ((Gauger_Bulge)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                        bSuccess = ((Gauger_Bulge)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm);
                                    else
                                        bSuccess = ((Gauger_Bulge)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                }
                            }
                            else if (MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR == data.m_mes_type)
                            {
                                if (0 == data.m_create_mode)
                                    bSuccess = ((Gauger_LineWidthByContour)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                        bSuccess = ((Gauger_LineWidthByContour)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm);
                                    else
                                        bSuccess = ((Gauger_LineWidthByContour)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                }
                            }
                            else if (MEASURE_TYPE.LINE_TO_EDGE == data.m_mes_type)
                            {
                                if (0 == data.m_create_mode)
                                    bSuccess = ((Gauger_LineToEdge)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                        bSuccess = ((Gauger_LineToEdge)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm);
                                    else
                                        bSuccess = ((Gauger_LineToEdge)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                }
                            }
                            else
                            {
                                if (0 == data.m_create_mode)
                                    bSuccess = ((Gauger_LineWidth)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                else
                                {
                                    if ((Math.Abs(data.m_handmade_ROI_rect[0].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[0].y) < 0.00001))
                                        bSuccess = ((Gauger_LineWidth)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm);
                                    else
                                        bSuccess = ((Gauger_LineWidth)m_gauger).gauge(ui_MainImage.Image, rect, m_nSmallSelectionFrameExtension, data.m_nAlgorithm, true);
                                }
                            }
                            Debugger.Log(0, null, string.Format("222222 gauge 222, bSuccess = {0}", bSuccess));
                            
                            if (true == bSuccess)
                            {
                                double res = 0;

                                if (false == m_bThreadIsRunning_MeasureOnceMore)
                                {
                                    #region
                                    if ((MEASURE_TYPE.LINE_WIDTH_1234 == data.m_mes_type))
                                        //|| (MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR == data.m_mes_type)
                                        //|| (MEASURE_TYPE.LINE_TO_EDGE == data.m_mes_type))
                                    {
                                        res = m_gauger.m_gauged_line_width / m_calib_data[data.m_len_ratio];
                                        gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, gridview_measure_results.RowCount - 2 - 1].Value 
                                            = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                        if ((res >= data.m_metric_line_width_lower[0]) && (res <= data.m_metric_line_width_upper[0]))
                                        {
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2 - 1].Style.BackColor = Color.FromArgb(0, 255, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2 - 1].Value = "OK";
                                        }
                                        else
                                        {
                                            m_bMeasureNG = true;

                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2 - 1].Style.BackColor = Color.FromArgb(255, 0, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2 - 1].Value = "NG";
                                        }

                                        res = m_gauger.m_gauged_line_width2 / m_calib_data[data.m_len_ratio];
                                        gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, gridview_measure_results.RowCount - 2].Value 
                                            = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                        if ((res >= data.m_metric_line_width_lower[1]) && (res <= data.m_metric_line_width_upper[1]))
                                        {
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Style.BackColor = Color.FromArgb(0, 255, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Value = "OK";
                                        }
                                        else
                                        {
                                            m_bMeasureNG = true;

                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Style.BackColor = Color.FromArgb(255, 0, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Value = "NG";
                                        }
                                    }
                                    else if ((MEASURE_TYPE.LINE_SPACE == data.m_mes_type) || (MEASURE_TYPE.ARC_LINE_SPACE == data.m_mes_type))
                                    {
                                        res = m_gauger.m_gauged_line_space / m_calib_data[data.m_len_ratio];
                                        gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, gridview_measure_results.RowCount - 1].Value 
                                            = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));
                                        
                                        if ((res >= data.m_metric_line_width_lower[0]) && (res <= data.m_metric_line_width_upper[0]))
                                        {
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 1].Style.BackColor = Color.FromArgb(0, 255, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 1].Value = "OK";
                                        }
                                        else
                                        {
                                            m_bMeasureNG = true;

                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 1].Style.BackColor = Color.FromArgb(255, 0, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 1].Value = "NG";
                                        }
                                    }
                                    else
                                    {
                                        res = m_gauger.m_gauged_line_width / m_calib_data[data.m_len_ratio];

                                        gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, gridview_measure_results.RowCount - 2].Value 
                                            = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));
                                        
                                        if ((res >= data.m_metric_line_width_lower[0]) && (res <= data.m_metric_line_width_upper[0]))
                                        {
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Style.BackColor = Color.FromArgb(0, 255, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Value = "OK";
                                        }
                                        else
                                        {
                                            m_bMeasureNG = true;

                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Style.BackColor = Color.FromArgb(255, 0, 0);
                                            gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Value = "NG";
                                        }
                                    }
                                    #endregion
                                }
                                else if (true == m_bThreadIsRunning_MeasureOnceMore)//再次测量
                                {
                                    #region
                                    //MessageBox.Show(this, index.ToString(), "提示");
                                    if (DialogResult.Yes == MessageBox.Show(this, "是否采用新的测量结果?", "提示", MessageBoxButtons.YesNo))
                                    {
                                        if (MEASURE_TYPE.LINE_WIDTH_1234 == data.m_mes_type)
                                        {
                                            res = m_gauger.m_gauged_line_width / m_calib_data[data.m_len_ratio];
                                            gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, index].Value
                                                = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                            if ((res >= data.m_metric_line_width_lower[0]) && (res <= data.m_metric_line_width_upper[0]))
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Style.BackColor = Color.FromArgb(0, 255, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Value = "OK";
                                            }
                                            else
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Style.BackColor = Color.FromArgb(255, 0, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Value = "NG";
                                            }

                                            res = m_gauger.m_gauged_line_width2 / m_calib_data[data.m_len_ratio];
                                            gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, index].Value
                                                = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                            if ((res >= data.m_metric_line_width_lower[1]) && (res <= data.m_metric_line_width_upper[1]))
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Style.BackColor = Color.FromArgb(0, 255, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Value = "OK";
                                            }
                                            else
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Style.BackColor = Color.FromArgb(255, 0, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Value = "NG";
                                            }
                                        }
                                        else if ((MEASURE_TYPE.LINE_SPACE == data.m_mes_type) || (MEASURE_TYPE.ARC_LINE_SPACE == data.m_mes_type))
                                        {
                                            res = m_gauger.m_gauged_line_space / m_calib_data[data.m_len_ratio];
                                            gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, index].Value
                                                = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                            if ((res >= data.m_metric_line_width_lower[0]) && (res <= data.m_metric_line_width_upper[0]))
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Style.BackColor = Color.FromArgb(0, 255, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Value = "OK";
                                            }
                                            else
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Style.BackColor = Color.FromArgb(255, 0, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Value = "NG";
                                            }
                                        }
                                        else
                                        {
                                            res = m_gauger.m_gauged_line_width / m_calib_data[data.m_len_ratio];
                                            gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, index].Value
                                                = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                            if ((res >= data.m_metric_line_width_lower[0]) && (res <= data.m_metric_line_width_upper[0]))
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Style.BackColor = Color.FromArgb(0, 255, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Value = "OK";
                                            }
                                            else
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Style.BackColor = Color.FromArgb(255, 0, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, index].Value = "NG";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (DialogResult.Yes == MessageBox.Show(this, "是否手动进行测量?", "提示", MessageBoxButtons.YesNo))
                                        {
                                            // 手动二次测量
                                            m_doubleclick_manual_measure = true;
                                            m_bIsWaitingForUserManualGauge = true;
                                            MessageBox.Show(this, "请手动测量。", "提示", MessageBoxButtons.OK);
                                        }
                                    }
                                    #endregion
                                }

                                //Debugger.Log(0, null, string.Format("222222 m_gauger.m_gauged_circle_center = [{0:0.000},{1:0.000}]",
                                //    m_gauger.m_gauged_circle_center.x, m_gauger.m_gauged_circle_center.y));

                                gridview_measure_results[RESULT_COLUMN_IS_AUTO_OR_MANUAL, gridview_measure_results.RowCount - 2].Value = "自动";

                                ui_MainImage.Refresh();

                                return true;
                            }
                            else
                            {
                                gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, gridview_measure_results.RowCount - 2].Value = "0";
                                gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Style.BackColor = Color.FromArgb(255, 0, 0);
                                gridview_measure_results[RESULT_COLUMN_OKNG, gridview_measure_results.RowCount - 2].Value = "NG";
                                gridview_measure_results[RESULT_COLUMN_IS_AUTO_OR_MANUAL, gridview_measure_results.RowCount - 2].Value = "手";

                                return false;
                            }
                            break;
                        #endregion
                    }
                    #endregion
                }
                else if ("请手动操作" == info)
                {
                    #region
                    MeasurePointData data = (MeasurePointData)param1;
                    data.m_strStepsFileName = m_strCurrentProductStep;
                    data.m_strLayerFileName = m_strCurrentProductLayer;
                    //m_gauger.m_string_offset_real = data.m_string_Position_offset;//

                    // 显示手拉区域
                    m_bShowHandDrawnRegion = true;
                    ui_MainImage.Refresh();
                    
                    switch (data.m_mes_type)
                    {
                        case MEASURE_TYPE.HAND_PICK_LINE:
                            //MessageBox.Show(m_form_progress, "当前测量项为手拉线，请手动操作。", "提示");
                            toolStripStatusLabel_GraphInfo.Text =$"当前测量项为手拉线，请手动操作。";
                            break;
                        case MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE:
                            //MessageBox.Show(m_form_progress, "当前测量项为手拉平行线(水平)，请手动操作。", "提示");
                            toolStripStatusLabel_GraphInfo.Text = $"当前测量项为手拉平行线(水平)，请手动操作。";
                            break;
                        case MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE:
                            //MessageBox.Show(m_form_progress, "当前测量项为手拉平行线(竖直)，请手动操作。", "提示");
                            toolStripStatusLabel_GraphInfo.Text = $"当前测量项为手拉平行线(竖直)，请手动操作。";
                            break;
                        case MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE:
                            //MessageBox.Show(m_form_progress, "当前测量项为手拉点平行线(水平)，请手动操作。", "提示");
                            toolStripStatusLabel_GraphInfo.Text = $"当前测量项为手拉点平行线(水平)，请手动操作。";
                            break;
                        case MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE:
                            //MessageBox.Show(m_form_progress, "当前测量项为手拉点平行线(竖直)，请手动操作。", "提示");
                            toolStripStatusLabel_GraphInfo.Text = $"当前测量项为手拉点平行线(竖直)，请手动操作。";
                            break;
                        case MEASURE_TYPE.HAND_PICK_CIRCLE:
                            //MessageBox.Show(m_form_progress, "当前测量项为手动三点选圆，请手动操作。", "提示");
                            toolStripStatusLabel_GraphInfo.Text = $"当前测量项为手动三点选圆，请手动操作。";
                            break;
                    }
                    m_bIsWaitingForUserManualGauge = true;
                    #endregion
                }
                else if ("等待用户确认定位孔" == info)
                {
                    #region
                    if (DialogResult.Yes == MessageBox.Show(this, "是否采用该定位孔?", "提示", MessageBoxButtons.YesNo))
                    {
                        m_motion.get_xyz_crds(ref m_current_task_data[m_nCurrentMeasureItemIdx].m_real_machine_crd);

                        //Debugger.Log(0, null, string.Format("222222 m_ptAccurateMarkCenter = [{0:0.000},{1:0.000}]",
                        //        m_ptAccurateMarkCenter.x, m_ptAccurateMarkCenter.y));
                        //Debugger.Log(0, null, string.Format("222222 m_real_machine_crd = [{0:0.000},{1:0.000}]",
                        //        m_current_task_data[m_nCurrentMeasureItemIdx].m_real_machine_crd.x,
                        //        m_current_task_data[m_nCurrentMeasureItemIdx].m_real_machine_crd.y));

                        Point2d offset = new Point2d(0, 0);
                        offset.x = m_ptAccurateMarkCenter.x - (double)(m_main_camera.m_nCamWidth / 2);
                        offset.y = m_ptAccurateMarkCenter.y - (double)(m_main_camera.m_nCamHeight / 2);
                        offset.x = (offset.x / m_calib_data[comboBox_Len.SelectedIndex]) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                        offset.y = (offset.y / m_calib_data[comboBox_Len.SelectedIndex]) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                        m_current_task_data[m_nCurrentMeasureItemIdx].m_real_machine_crd.x += offset.x;
                        m_current_task_data[m_nCurrentMeasureItemIdx].m_real_machine_crd.y += offset.y;

                        m_current_task_data[m_nCurrentMeasureItemIdx].m_strStepsFileName = m_strCurrentProductStep;
                        m_current_task_data[m_nCurrentMeasureItemIdx].m_strLayerFileName = m_strCurrentProductLayer;

                        Debugger.Log(0, null, string.Format("222222 手动采用该定位孔 333"));

                        //Debugger.Log(0, null, string.Format("222222 m_real_machine_crd = [{0:0.000},{1:0.000}]",
                        //        m_current_task_data[m_nCurrentMeasureItemIdx].m_real_machine_crd.x,
                        //        m_current_task_data[m_nCurrentMeasureItemIdx].m_real_machine_crd.y));

                        m_event_wait_for_confirm_during_autorun.Set();
                    }
                    else
                        m_bIsWaitingForConfirm = true;
                    #endregion
                }
                else if ("等待用户确认测量NG" == info)
                {
                    #region
                    MeasurePointData data = (MeasurePointData)param1;
                    if (DialogResult.Yes == MessageBox.Show(this, "是否确认该测量项NG?", "提示", MessageBoxButtons.YesNo))
                    {
                        m_event_wait_for_confirm_during_autorun.Set();
                    }
                    else
                    {
                        m_gauger.clear_gauger_state();
                        m_bIsWaitingForConfirm = true;
                    }
                    #endregion
                }
                else if ("等待用户确认测量结果" == info)
                {
                    #region
                    MeasurePointData data = (MeasurePointData)param1;
                    if (DialogResult.Yes == MessageBox.Show(this, "是否采用该测量结果?", "提示", MessageBoxButtons.YesNo))
                    {
                        bool bTreatAsCorrectionPoint = false;

                        // 小于0.01代表是手动拉出来的测量框
                        if (Math.Abs(data.m_center_x_in_metric) < 0.01)
                            bTreatAsCorrectionPoint = false;
                        else
                        {
                            Point3d after_crd = new Point3d();
                            m_motion.get_xyz_crds(ref after_crd);

                            if (Math.Abs(after_crd.x - m_before_crd.x) > 0.005 || Math.Abs(after_crd.y - m_before_crd.y) > 0.005)
                                bTreatAsCorrectionPoint = false;
                            else
                                bTreatAsCorrectionPoint = true;
                        }

                        if (true == bTreatAsCorrectionPoint)
                        {
                            switch (data.m_mes_type)
                            {
                                case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                                case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                                case MEASURE_TYPE.LINE_WIDTH_14:
                                case MEASURE_TYPE.LINE_WIDTH_23:
                                case MEASURE_TYPE.LINE_WIDTH_13:
                                case MEASURE_TYPE.LINE_WIDTH_1234:
                                case MEASURE_TYPE.LINE_SPACE:
                                case MEASURE_TYPE.ARC_LINE_SPACE:
                                case MEASURE_TYPE.ARC_LINE_WIDTH:
                                case MEASURE_TYPE.L_SHAPE:
                                case MEASURE_TYPE.BULGE:
                                case MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR:
                                case MEASURE_TYPE.LINE_TO_EDGE:
                                case MEASURE_TYPE.ETCH_DOWN:
                                    Point2d center = m_gauger.m_object_center;
                                    Point2d offset = new Point2d(0, 0);

                                    offset.x = center.x - (double)(m_main_camera.m_nCamWidth / 2);
                                    offset.y = center.y - (double)(m_main_camera.m_nCamHeight / 2);

                                    offset.x = (offset.x / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                                    offset.y = (offset.y / m_calib_data[data.m_len_ratio]) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                                    if ((Math.Abs(offset.x) + Math.Abs(offset.y)) < 0.06)
                                    {
                                        Point3d machine_crd = new Point3d(0, 0, 0);
                                        m_motion.get_xyz_crds(ref machine_crd);
                                        data.m_real_machine_crd.x = machine_crd.x + offset.x;
                                        data.m_real_machine_crd.y = machine_crd.y + offset.y;

                                        StageGraphCrdPair pair = new StageGraphCrdPair();
                                        pair.graph_crd.x = data.m_center_x_in_metric;
                                        pair.graph_crd.y = data.m_center_y_in_metric;
                                        pair.stage_crd.x = data.m_real_machine_crd.x;
                                        pair.stage_crd.y = data.m_real_machine_crd.y;
                                        if (data.m_len_ratio > 0)
                                        {
                                            pair.stage_crd.x -= m_len_ratios_offsets[data.m_len_ratio].x;
                                            pair.stage_crd.y -= m_len_ratios_offsets[data.m_len_ratio].y;
                                        }
                                        Debugger.Log(0, null, string.Format("222222 graph_crd [{0:0.000},{1:0.000}], [{2:0.000},{3:0.000}]", pair.graph_crd.x, pair.graph_crd.y, pair.stage_crd.x, pair.stage_crd.y));
                                        m_list_stage_graph_crd_pairs.Add(pair);
                                    }
                                    
                                    break;
                            }
                        }

                        m_bTriggerSaveGaugeImage = true;
                        ui_MainImage.Refresh();

                        m_event_wait_for_confirm_during_autorun.Set();
                    }
                    else
                    {
                        m_gauger.clear_gauger_state();
                        m_bIsWaitingForConfirm = true;
                    }
                    #endregion
                }
                else if (info.Contains("需要用户进行手动测量"))
                {
                    MeasurePointData data = (MeasurePointData)param1;

                    for (int n = 0; n < 4; n++)
                    {
                        // 学习模式下的测量点
                        m_selection_frame_rect[n].x = data.m_handmade_ROI_rect[n].x + m_base_template_pos.m_matched_pos.x;
                        m_selection_frame_rect[n].y = data.m_handmade_ROI_rect[n].y + m_base_template_pos.m_matched_pos.y;
                    }

                    m_bShowSelectionFrameOnNG = true;
                    ui_MainImage.Refresh();

                    MessageBox.Show(this, "当前测量项目异常，请手动测量。", "提示", MessageBoxButtons.OK);
                    m_bIsWaitingForUserManualGauge = true;

                    //if (DialogResult.Yes == MessageBox.Show(this, "当前测量项目异常，请手动测量。", "提示", MessageBoxButtons.OK))
                    //{
                    //    m_bIsWaitingForUserManualGauge = true;
                    //    return true;
                    //}
                    //else
                    //    return false;
                }
                else if ("设置测量类型" == info)
                {
                    if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == (MEASURE_TYPE)param1)
                        set_gauger(MEASURE_TYPE.CIRCLE_OUTER_TO_INNER);
                    else
                        set_gauger((MEASURE_TYPE)param1);
                }
                else if ("设置倍率" == info)
                {
                    if (comboBox_Len.Items.Count > 0)
                    {
                    if (comboBox_Len.SelectedIndex != (int)param1)
                        comboBox_Len.SelectedIndex = (int)param1;
                }
                }
                else if ("设置单位" == info)
                {
                    //if (comboBox_Unit.SelectedIndex != (int)param1)
                    //    comboBox_Unit.SelectedIndex = (int)param1;
                    comboBox_Unit.SelectedIndex = 1;
                }
                else if ("设置光源和亮度" == info)
                {
                    #region
                    MeasurePointData data = (MeasurePointData)param1;
                    data.m_strStepsFileName = m_strCurrentProductStep;
                    data.m_strLayerFileName = m_strCurrentProductLayer;
                    if ((true == data.m_bIsTopLightOn) && (false == m_top_light.m_bOn))
                    {
                        m_top_light.m_bOn = true;
                        m_top_light.open_light();
                    }
                    else if ((false == data.m_bIsTopLightOn) && (true == m_top_light.m_bOn))
                    {
                        m_top_light.m_bOn = false;
                        m_top_light.close_light();
                    }
                    if (true == m_top_light.m_bOn)
                        ui_trackBar_TopLight.Value = data.m_nTopBrightness;

                    if ((true == data.m_bIsBottomLightOn) && (false == m_bottom_light.m_bOn))
                    {
                        m_bottom_light.m_bOn = true;
                        m_bottom_light.open_light();
                    }
                    else if ((false == data.m_bIsBottomLightOn) && (true == m_bottom_light.m_bOn))
                    {
                        m_bottom_light.m_bOn = false;
                        m_bottom_light.close_light();
                    }
                    if (true == m_bottom_light.m_bOn)
                        ui_trackBar_BottomLight.Value = data.m_nBottomBrightness;

                    refresh_light_icons();
                    #endregion
                }
                else if ("设置亮度" == info)
                {
                    LIGHT_TYPE type = (LIGHT_TYPE)param1;
                    int value = (int)param2;
                    
                    if (LIGHT_TYPE.TOP_LIGHT == type)
                        ui_trackBar_TopLight.Value = value;
                    else
                        ui_trackBar_BottomLight.Value = value;
                }
                else if ("开始创建任务" == info)
                {
                    #region
                    m_bIsCreatingTask = true;
                    
                    m_list_measure_results.Clear();
                    m_current_task_data.Clear();
                    gridview_MeasureTask.Rows.Clear();

                    m_hBaseTemplateImage.Dispose();

                    clear_ChenLing_contents();
                    
                    if (0 == m_ODB_measure_items.Count)
                    {
                        m_measure_items_on_graph.Clear();
                        gridview_GraphMeasureItems.Rows.Clear();
                    }
                    
                    btn_LoadTask.Enabled = false;
                    btn_UpdateTask.Enabled = false;

                    m_nCurrentArrayOrderIdx = 0;

                    if (0 == m_ODB_measure_items.Count)
                        this.tabControl_Task.SelectedIndex = 0;

                    if ((true == m_graph_view.m_bHasValidImage) && (0 == m_ODB_measure_items.Count))
                    {
                        double x = (double)m_nGraphOffsetX;
                        double y = (double)(m_graph_view.m_bitmap_1bit.Height - m_nGraphOffsetY * 2) * 0.9;
                        m_graph_view.set_view_ratio_and_crd(m_graph_view.m_zoom_ratio_min * 7, x, y);
                        m_graph_view.refresh_image();
                    }

                    if (m_strCurrentProductLayer.Length > 0)
                    {
                        //m_strCurrentTaskName = m_strCurrentProductModel + "_" + m_strCurrentProductLayer;
                        m_strCurrentTaskName = Path.GetFileName(m_strGraphFilePath) + "_" + m_strCurrentProductLayer;
                    }
                    else
                        m_strCurrentTaskName = m_strCurrentProductModel;
                    this.toolStripStatusLabel_TaskInfo.Text = "正在创建任务：" + m_strCurrentTaskName;
                    
                    // 将 由外向内找圆 测量项设为当前项
                    //set_gauger(MEASURE_TYPE.CIRCLE_OUTER_TO_INNER);

                    // 打开吸附
                    if (false == m_bOfflineMode)
                    {
                        CBD_SendMessage("开启吸附", true, null, null);

                        // 开绿灯，关黄灯
                        m_IO.set_IO_output(m_IO.m_output_green_light, Hardwares.IO_STATE.IO_LOW);
                        m_IO.set_IO_output(m_IO.m_output_yellow_light, Hardwares.IO_STATE.IO_HIGH);
                    }

                    //btn_CreateTask.Text = "创建完成";
                    btn_CreateTask.Image = Image.FromFile("icons\\save.png");

                    ui_MainImage.Refresh();

                    #endregion
                }
                else if (info.Contains("任务") && info.Contains("成功创建并写入数据库"))
                {
                    #region
                    m_bIsCreatingTask = false;
                    m_bShowSmallSelectionFrame = false;
                    m_bShowFrameDuringTaskCreation = false;
                    m_bShowCoarseMark = false;
                    m_bShowAccurateMark = false;
                    m_bApplySameParamsToOtherUnits = false;

                    m_bCancelAndRemoveUnfinishedItems = true;
                    m_event_wait_for_manual_gauge.Set();
                    m_event_wait_for_confirm_during_creation.Set();

                    m_gauger.clear_gauger_state();

                    this.toolStripStatusLabel_TaskInfo.Text = "当前任务：" + m_strCurrentTaskName;

                    m_event_wait_for_manual_gauge.Set();
                    m_event_wait_for_confirm_during_creation.Set();

                    //btn_CreateTask.Text = "创建任务";
                    btn_CreateTask.Image = Image.FromFile("icons\\create.png");

                    btn_LoadTask.Enabled = true;
                    btn_UpdateTask.Enabled = true;

                    // 关闭吸附
                    if (false == m_bOfflineMode)
                    {
                        //CBD_SendMessage("关闭吸附", true, null, null);

                        // 关绿灯，开黄灯
                        m_IO.set_IO_output(m_IO.m_output_green_light, Hardwares.IO_STATE.IO_HIGH);
                        m_IO.set_IO_output(m_IO.m_output_yellow_light, Hardwares.IO_STATE.IO_LOW);
                    }

                    //MessageBox.Show(this, "任务保存成功。", "提示", MessageBoxButtons.OK);
                    #endregion
                }
                else if ((info.Contains("任务") && info.Contains("创建失败")) || ("中止创建任务" == info))
                {
                    #region
                    m_bStopExecution = true;
                    m_bIsWaitingForConfirm = false;
                    m_bIsCreatingTask = false;
                    m_bShowSmallSelectionFrame = false;
                    m_bShowFrameDuringTaskCreation = false;
                    m_bShowCoarseMark = false;
                    m_bShowAccurateMark = false;
                    m_bCancelAndRemoveUnfinishedItems = false;
                    m_bApplySameParamsToOtherUnits = false;

                    m_bCancelAndRemoveUnfinishedItems = true;
                    m_event_wait_for_manual_gauge.Set();
                    m_event_wait_for_confirm_during_creation.Set();

                    m_gauger.clear_gauger_state();

                    m_list_measure_results.Clear();
                    m_current_task_data.Clear();
                    m_measure_items_on_graph.Clear();
                    gridview_MeasureTask.Rows.Clear();

                    //m_strCurrentProductModel = "";
                    //m_strCurrentProductLayer = "";
                    m_strCurrentProductNumber = "";
                    m_strCurrentTaskName = "";
                    this.toolStripStatusLabel_TaskInfo.Text = "";

                    m_event_wait_for_manual_gauge.Set();
                    m_event_wait_for_confirm_during_creation.Set();

                    //btn_CreateTask.Text = "创建任务";
                    btn_CreateTask.Image = Image.FromFile("icons\\create.png");

                    btn_LoadTask.Enabled = true;
                    btn_UpdateTask.Enabled = true;

                    // 关闭吸附
                    if (false == m_bOfflineMode)
                    {
                        CBD_SendMessage("关闭吸附", true, null, null);

                        // 关绿灯，开黄灯
                        m_IO.set_IO_output(m_IO.m_output_green_light, Hardwares.IO_STATE.IO_HIGH);
                        m_IO.set_IO_output(m_IO.m_output_yellow_light, Hardwares.IO_STATE.IO_LOW);
                    }
                    #endregion
                }
                else if ("加载任务" == info)
                {
                    #region
                    m_list_measure_results.Clear();
                    m_current_task_data.Clear();
                    m_measure_items_on_graph.Clear();
                    gridview_MeasureTask.Rows.Clear();
                    gridview_GraphMeasureItems.Rows.Clear();
                    gridview_measure_results.Rows.Clear();

                    m_bIsLoadedByBrowsingDir = false;
                    m_strCurrentTaskFileFullPath = "";

                    if (0 == (int)param1)
                        load_task_table(m_strCurrentTaskName, m_current_task_data);
                    else if (1 == (int)param1)
                    {
                        read_task_from_file_for_Chenling((string)param2 + "\\" + m_strCurrentTaskName + ".dat", m_current_task_data);

                        //read_task_from_file((string)param2 + "\\" + m_strCurrentTaskName + ".dat", m_current_task_data);
                    }

                    show_task_on_gridview(gridview_MeasureTask, m_current_task_data);

                    if (m_current_task_data.Count > 0)
                    {
                        this.tabControl_Task.SelectedIndex = 0;

                        this.toolStripStatusLabel_TaskInfo.Text = "当前任务：" + m_strCurrentTaskName;

                        string time = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + " " + DateTime.Now.ToLongTimeString().ToString();
                        textBox_KeyInfo.AppendText(time + " " + info + " " + m_strCurrentTaskName + Environment.NewLine);

                        comboBox_Len.SelectedIndex = m_current_task_data[0].m_len_ratio;
                    }
                    #endregion
                }
                else if ("直接从文件加载任务" == info)
                {
                    #region
                    m_list_measure_results.Clear();
                    m_measure_items_on_graph.Clear();
                    gridview_MeasureTask.Rows.Clear();
                    gridview_GraphMeasureItems.Rows.Clear();
                    gridview_measure_results.Rows.Clear();
                    
                    show_task_on_gridview(gridview_MeasureTask, m_current_task_data);

                    m_bIsLoadedByBrowsingDir = true;

                    m_strCurrentTaskFileFullPath = (string)param1;
                    m_strCurrentTaskName = (string)param2;

                    if (m_current_task_data.Count > 0)
                    {
                        this.tabControl_Task.SelectedIndex = 0;

                        this.toolStripStatusLabel_TaskInfo.Text = "当前任务：" + (string)param2;

                        string time = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + " " + DateTime.Now.ToLongTimeString().ToString();
                        textBox_KeyInfo.AppendText(time + " " + info + " " + m_strCurrentTaskName + Environment.NewLine);

                        comboBox_Len.SelectedIndex = m_current_task_data[0].m_len_ratio;
                    }
                    #endregion
                }
                else if (info.Contains("刷新光源图标"))
                {
                    refresh_light_icons();
                }
                else if ("更新XYZ坐标信息" == info)
                {
                    string crd = string.Format("机械坐标：[{0:0.000}, {1:0.000}, {2:0.000}]", m_current_xyz.x, m_current_xyz.y, m_current_xyz.z);
                    toolStripStatusLabel_MachineCrd.Text = crd;
                }
                else if ((info == "变倍镜头初始化成功") || (info == "变倍镜头初始化失败"))
                {
                    if (info == "变倍镜头初始化成功")
                    {
                        string[] ratios = new string[] { "50X", "100X", "200X", "500X", "1000X", "1500X" };
                        for (int n = 0; n < ratios.Length; n++)
                            comboBox_Len.Items.Add(ratios[n]);

                        comboBox_Len.Enabled = true;
                        comboBox_Len.SelectedIndex = m_len.m_nRatio;
                    }
                    else
                    {
                        MessageBox.Show(m_form_progress, "变倍镜头初始化失败，请检查原因!", "提示");
                    }
                }
                else if ((info == "运动系统初始化成功") || (info == "运动系统初始化失败"))
                {
                    if ((info == "运动系统初始化失败") && (false == m_motion.m_bStopHomingThread))
                    {
                        MessageBox.Show(m_form_progress, "运动系统初始化失败，请检查原因!", "提示");
                        m_event_suspend_hardware_init.Set();
                    }
                    m_motion.m_bStopHomingThread = false;
                    //m_form_progress.reset_progress();
                    m_form_progress.set_tip_info("正在初始化其它硬件资源.....");
                    m_form_progress.enable_closing_window();
                    m_form_progress.Close();
                }
                else if ((info == "光源初始化成功") || (info == "光源初始化失败"))
                {
                    if (info == "光源初始化失败")
                    {
                        MessageBox.Show(m_form_progress, "光源初始化失败，请检查原因!", "提示");
                        m_event_suspend_hardware_init.Set();
                    }
                    //m_form_progress.reset_progress();
                    m_form_progress.set_tip_info("正在初始化主相机.....");
                }
                else if ((info == "主相机初始化成功") || (info == "主相机初始化失败"))
                {
                    if (info == "主相机初始化失败")
                    {
                        MessageBox.Show(m_form_progress, "主相机初始化失败，请检查原因!", "提示");
                        m_event_suspend_hardware_init.Set();
                    }
                    //m_form_progress.reset_progress();
                    m_form_progress.set_tip_info("正在初始化导航相机.....");
                }
                else if ((info == "导航相机初始化成功") || (info == "导航相机初始化失败"))
                {
                    if (info == "导航相机初始化失败")
                    {
                        MessageBox.Show(m_form_progress, "导航相机初始化失败，请检查原因!", "提示");
                        m_event_suspend_hardware_init.Set();
                    }
                    //m_form_progress.reset_progress();
                    m_form_progress.enable_closing_window();
                    if (true == m_bNeedToInitMotionSystem)
                    m_form_progress.set_tip_info("正在初始化运动系统.....");
                }
                else if (info.Contains("吸附"))
                {
                    if ("开启吸附" == info)
                    {
                        checkBox_Vacuum.Checked = true;
                        hardware_ops_enable_vacuum(true);
                        //hardware_ops_enable_vacuum(true);
                    }
                    else if ("关闭吸附" == info)
                    {
                        checkBox_Vacuum.Checked = false;
                        hardware_ops_enable_vacuum(false);
                        //hardware_ops_enable_vacuum(false);
                    }
                }
                else if (info == "从dll得到GraphView大图")
                {
                    m_graph_view.refresh_image();
                }
                else if (info == "刷新ODB自动生成的测量项")
                {
                    if (m_ODB_measure_items.Count > 0)
                    {
                        for (int n = 0; n < m_ODB_measure_items.Count; n++)
                        {
                            double graph_x = m_ODB_measure_items[n].dbGraphCrdX * m_pixels_per_mm + m_nGraphOffsetX;
                            double graph_y = m_nGraphHeight - (m_ODB_measure_items[n].dbGraphCrdY * m_pixels_per_mm + m_nGraphOffsetY);

                            //if (n >= 3 && n != 43)
                            //    continue;
                            
                            MeasurePointData data = new MeasurePointData();
                            data.m_strStepsFileName = m_strCurrentProductStep;
                            data.m_strLayerFileName = m_strCurrentProductLayer;
                            data.m_bIsFromODBAttribute = true;
                            data.m_metric_line_width[0] = m_ODB_measure_items[n].dbStandardValue * 1000;
                            if (false == add_measure_point(m_ODB_measure_items[n].nMeasureType, (int)graph_x, (int)graph_y, ref data, false, false, true))
                            {
                                Debugger.Log(0, null, string.Format("222222 add_measure_point() failed! n={0} crd [{1:0.000},{2:0.000}]", n, graph_x, graph_y));

                                data.m_ID = m_measure_items_on_graph.Count + 1;
                                data.m_mes_type = m_ODB_measure_items[n].nMeasureType;
                                data.m_bIsInvalidItem = true;

                                data.m_center_x_on_graph = graph_x;
                                data.m_center_y_on_graph = graph_y;

                                m_measure_items_on_graph.Add(data);
                            }
                            else
                            {
                                //Debugger.Log(0, null, string.Format("222222 add_measure_point() success"));

                                tabControl_Task.SelectedIndex = 1;
                                m_event_wait_for_manual_gauge.Set();

                                data.m_mes_type = m_ODB_measure_items[n].nMeasureType;
                                data.m_bHasBeenMeasured = false;
                                data.m_bIsFromODBAttribute = true;
                                
                                switch (data.m_mes_type)
                                {
                                    case MEASURE_TYPE.LINE_WIDTH_14:
                                    case MEASURE_TYPE.LINE_WIDTH_23:
                                    case MEASURE_TYPE.LINE_WIDTH_13:
                                    case MEASURE_TYPE.LINE_WIDTH_1234:
                                    case MEASURE_TYPE.LINE_SPACE:
                                    case MEASURE_TYPE.ARC_LINE_SPACE:
                                    case MEASURE_TYPE.HAND_PICK_LINE:
                                    case MEASURE_TYPE.LINE:
                                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                                    case MEASURE_TYPE.SHORT_SPACE_BETWEEN_TWO_EMPTY_CIRCLES:
                                    case MEASURE_TYPE.L_SHAPE:
                                    case MEASURE_TYPE.BULGE:
                                    case MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR:
                                    case MEASURE_TYPE.LINE_TO_EDGE:
                                    case MEASURE_TYPE.ETCH_DOWN:
                                    case MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE:
                                    case MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE:
                                    case MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE:
                                    case MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE:
                                        data.m_metric_line_width[0] = m_ODB_measure_items[n].dbStandardValue * 1000;
                                        data.m_metric_line_width_upper[0] = m_ODB_measure_items[n].dbUpper * 1000;
                                        data.m_metric_line_width_lower[0] = m_ODB_measure_items[n].dbLower * 1000;
                                        break;
                                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                                    case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                                    case MEASURE_TYPE.HAND_PICK_CIRCLE:
                                        data.m_metric_radius[0] = m_ODB_measure_items[n].dbStandardValue * 1000;
                                        data.m_metric_radius_upper[0] = m_ODB_measure_items[n].dbUpper * 1000;
                                        data.m_metric_radius_lower[0] = m_ODB_measure_items[n].dbLower * 1000;
                                        break;
                                }

                                m_measure_items_on_graph.Add(data);
                            }
                        }

                        dl_message_sender send_message = CBD_SendMessage;
                        send_message("刷新图纸测量项列表", false, true, null);

                        m_graph_view.refresh_image();
                    }
                }
                else if (info == "图纸读取进度")
                {
                    #region
                    if (null != m_form_graph_progress)
                    {
                        int nProgress = Form_GraphOrientation.m_nThumbnailProgress;
                        m_form_graph_progress.progressBar1.Value = nProgress;

                        int nElapsedSeconds = m_form_graph_progress.m_nTimer2Counter * m_form_graph_progress.m_nTimer2Interval / 1000;

                        string text;
                        switch (Form_GraphOrientation.m_reading_state)
                        {
                            case GRAPH_READING_STATE.READING:
                                //text = String.Format("正在读取文件: {0}%   已耗时: {1}秒", nProgress, nElapsedSeconds);
                                text = String.Format("正在读取文件: {0}%", nProgress);
                                m_form_graph_progress.label_ProgressInfo.Text = text;
                                break;
                            case GRAPH_READING_STATE.PARSING_1:
                                //text = String.Format("解析第一阶段: {0}%   已耗时: {1}秒", nProgress, nElapsedSeconds);
                                text = String.Format("解析第一阶段: {0}%", nProgress);
                                m_form_graph_progress.label_ProgressInfo.Text = text;
                                break;
                            case GRAPH_READING_STATE.PARSING_2:
                                //text = String.Format("解析第二阶段: {0}%   已耗时: {1}秒", nProgress, nElapsedSeconds);
                                text = String.Format("解析第二阶段: {0}%", nProgress);
                                m_form_graph_progress.label_ProgressInfo.Text = text;
                                break;
                            case GRAPH_READING_STATE.RENDERING:
                                //text = String.Format("正在渲染图纸: {0}%   已耗时: {1}秒", nProgress, nElapsedSeconds);
                                text = String.Format("正在渲染图纸: {0}%", nProgress);
                                m_form_graph_progress.label_ProgressInfo.Text = text;
                                break;
                        }

                        if (false == Form_GraphOrientation.m_bIsReadingGraphForThumbnail)
                            m_form_graph_progress.Close();
                    }
                    #endregion
                }
            }

            return true;
        }
        
        // 主界面初始化
        public MainUI()
        {
            Debugger.Log(0, null, string.Format("222222 程序启动    版本号 = {0}", VERSION));
            
            InitializeComponent();

            fontDialog1.Font = lblFont.Font;
            
            for (int n = 0; n < m_len_ratios_offsets.Length; n++)
                m_len_ratios_offsets[n] = new Point2d(0, 0);

            for (int n = 0; n < m_nMeasureResultDigits.Length; n++)
                m_nMeasureResultDigits[n] = 3;

            statusStrip.Items.Insert(1, new ToolStripSeparator());
            statusStrip.Items.Insert(3, new ToolStripSeparator());
            statusStrip.Items.Insert(5, new ToolStripSeparator());
            //this.BackColor = Color.FromArgb(250, 250, 250);
            menuStrip1.BackColor = Color.FromArgb(161, 185, 209);
            textBox_KeyInfo.BackColor = Color.FromArgb(252, 252, 252);
            
            if (!Directory.Exists("configs"))
                Directory.CreateDirectory("configs");
            
            m_IO = new IOOps(this, m_strIOConfigPath);
            m_len = new LenOps(this, serialPort_Len);
            m_motion = new MotionOps(this);
            m_main_camera = new CameraOps(this, ui_MainImage);
            m_guide_camera = new CameraOps(this, ui_GuideImage);
            m_top_light = new LightOps(this, serialPort_TopLight, LIGHT_TYPE.TOP_LIGHT);
            m_bottom_light = new LightOps(this, serialPort_BottomLight, LIGHT_TYPE.BOTTOM_LIGHT);
            m_image_operator = new ImgOperators(this, m_motion);
            
            // 读取配置文件
            if (true)
            {
                if (File.Exists(m_strAppParamsPath))
                    LoadAppParams(m_strAppParamsPath, m_app_params);
                else
                    File.Create(m_strAppParamsPath).Close();

                for (int n = 0; n < m_calib_data.Length; n++)
                    m_calib_data[n] = 1;
                if (File.Exists(m_strCalibDataPath))
                    LoadCalibData(m_strCalibDataPath, ref m_calib_data);
                else
                    File.Create(m_strCalibDataPath).Close();
            }
            
            // 登录框
            if (false == m_bOfflineMode)
            {
                Form_Login form = new Form_Login(this);
                form.ShowInTaskbar = false;
                form.ShowDialog();
            }
            else
                CBD_SendMessage("登录", false, 0, m_strUseringName);
            
            if (true == m_bOfflineMode)
            {
                string[] ratios = new string[] { "50X", "100X", "200X", "500X", "1000X", "1500X" };
                for (int n = 0; n < ratios.Length; n++)
                    comboBox_Len.Items.Add(ratios[n]);
                comboBox_Len.SelectedIndex = 2;
            }
            
            //if (true == m_bOfflineMode)
            {
                m_IO.load_params();
                m_len.load_params(m_strLenConfigPath);
                m_motion.load_params(m_strMotionParamsPath);
                m_main_camera.load_params(m_strMainCameraFile);
                m_guide_camera.load_params(m_strGuideCameraFile);
                m_top_light.load_params(m_strTopLightFile);
                m_bottom_light.load_params(m_strBottomLightFile);

                //初始化账户记录 创建一个账户
                if (!File.Exists(m_strUserMessage))
                {
                    File.Create(m_strUserMessage).Close();

                    //初始化生成一个账户
                    StreamWriter writer = new StreamWriter(m_strUserMessage);
                    writer.WriteLine("1");
                    writer.WriteLine(String.Format("name={0},pass={1},status={2},",111, 222, 1));
                    writer.Close();
                }
            }
            
            Debugger.Log(0, null, string.Format("222222 离线模式 {0}", m_bOfflineMode));

            // XYZ坐标信息刷新线程
            if (true)
            {
                dl_message_sender messenger = CBD_SendMessage;
                Thread thrd = new Thread(thread_update_xyz_crd);
                thrd.Start(messenger);
            }
            
            // IO监控线程
            if (false == m_bOfflineMode)
            {
                dl_message_sender messenger = CBD_SendMessage;
                Thread thrd = new Thread(thread_monitor_IO);
                thrd.Start(messenger);
            }

            // 图像信息刷新线程
            //if (false == m_bOfflineMode)
            {
                dl_message_sender messenger = CBD_SendMessage;
                Thread thrd = new Thread(thread_update_camera_image_info);
                thrd.Start(messenger);
            }

            //System.Environment.Exit(0);
            
            // 初始化硬件
            if (false == m_bOfflineMode)
            {
                if (DialogResult.No == MessageBox.Show(this, "是否初始化运动系统?", "提示", MessageBoxButtons.YesNo))
                    m_bNeedToInitMotionSystem = false;

                m_form_progress = new Form_ProgressInfo(this);
                m_form_progress.set_title("PixelLiner");
                m_form_progress.set_tip_info("正在初始化光源......");
                m_form_progress.set_infinite_wait_mode(PROGRESS_WAIT_MODE.NORMAL);
                m_form_progress.disable_closing_window();

                dl_message_sender messenger = CBD_SendMessage;
                Thread thrd = new Thread(thread_init_len);
                thrd.Start(messenger);

                dl_message_sender messenger2 = CBD_SendMessage;
                Thread thrd2 = new Thread(thread_init_hardwares);
                thrd2.Start(messenger2);

                m_form_progress.ShowDialog();

                if (true == m_motion.m_bHomed)
                    btn_Home.Enabled = true;
            }
            
            // 启动 数据库连接 线程
            if ((false == m_bOfflineMode) && m_bUseDatabase)
            {
                dl_message_sender messenger = CBD_SendMessage;
                Thread thrd = new Thread(thread_connect_to_SQL);
                thrd.Start(messenger);
            }

            if (true)
            {
                dl_message_sender messenger = CBD_SendMessage;
                //new Thread(thread_trigger_init_third_party_libraries).Start(messenger);
                new Thread(thread_create_and_format_disk).Start(messenger);
            }

            // 启动 光栅监控 线程
            if (false == m_bOfflineMode)
            {
                dl_message_sender messenger3 = CBD_SendMessage;
                Thread thrd3 = new Thread(thread_monitor_grating);
                thrd3.Start(messenger3);
            }
            
            set_mv_width_height(ui_MainImage.Width, ui_MainImage.Height);
            Debugger.Log(0, null, string.Format("222222 程序启动 777"));
        }
        
        // 主窗体加载完成
        private void MainUI_Load(object sender, EventArgs e)
        {
            try
            {
                Debugger.Log(0, null, string.Format("222222 MainUI_Load 111"));

                if (false == m_bOfflineMode)
                    m_event_wait_for_hardware_init.WaitOne();
                Debugger.Log(0, null, string.Format("222222 MainUI_Load 222"));
                m_images_for_line_width_14[0] = Image.FromFile(m_file_path_for_14_normal);
                m_images_for_line_width_14[1] = Image.FromFile(m_file_path_for_14_hovered);
                m_images_for_line_width_14[2] = Image.FromFile(m_file_path_for_14_pressed);
                m_images_for_line_width_23[0] = Image.FromFile(m_file_path_for_23_normal);
                m_images_for_line_width_23[1] = Image.FromFile(m_file_path_for_23_hovered);
                m_images_for_line_width_23[2] = Image.FromFile(m_file_path_for_23_pressed);
                m_images_for_line_width_13[0] = Image.FromFile(m_file_path_for_13_normal);
                m_images_for_line_width_13[1] = Image.FromFile(m_file_path_for_13_hovered);
                m_images_for_line_width_13[2] = Image.FromFile(m_file_path_for_13_pressed);
                m_images_for_line_width_1234[0] = Image.FromFile(m_file_path_for_1234_normal);
                m_images_for_line_width_1234[1] = Image.FromFile(m_file_path_for_1234_hovered);
                m_images_for_line_width_1234[2] = Image.FromFile(m_file_path_for_1234_pressed);
                m_images_for_arc_line_width[0] = Image.FromFile(m_file_path_for_14_normal);
                m_images_for_arc_line_width[1] = Image.FromFile(m_file_path_for_14_hovered);
                m_images_for_arc_line_width[2] = Image.FromFile(m_file_path_for_14_pressed);
                m_images_for_line_space[0] = Image.FromFile(m_file_path_for_linespace_normal);
                m_images_for_line_space[1] = Image.FromFile(m_file_path_for_linespace_hovered);
                m_images_for_line_space[2] = Image.FromFile(m_file_path_for_linespace_pressed);
                m_images_for_arc_line_space[0] = Image.FromFile(m_file_path_for_linespace_normal);
                m_images_for_arc_line_space[1] = Image.FromFile(m_file_path_for_linespace_hovered);
                m_images_for_arc_line_space[2] = Image.FromFile(m_file_path_for_linespace_pressed);
                m_images_for_short_space_between_two_empty_circles[0] = Image.FromFile(m_file_path_for_linespace_normal);
                m_images_for_short_space_between_two_empty_circles[1] = Image.FromFile(m_file_path_for_linespace_hovered);
                m_images_for_short_space_between_two_empty_circles[2] = Image.FromFile(m_file_path_for_linespace_pressed);
                m_images_for_line[0] = Image.FromFile(m_file_path_for_14_normal);
                m_images_for_line[1] = Image.FromFile(m_file_path_for_14_hovered);
                m_images_for_line[2] = Image.FromFile(m_file_path_for_14_pressed);
                m_images_for_circle_outer_to_inner[0] = Image.FromFile(m_file_path_for_CircleOuter2Inner_normal);
                m_images_for_circle_outer_to_inner[1] = Image.FromFile(m_file_path_for_CircleOuter2Inner_hovered);
                m_images_for_circle_outer_to_inner[2] = Image.FromFile(m_file_path_for_CircleOuter2Inner_pressed);
                m_images_for_circle_inner_to_outer[0] = Image.FromFile(m_file_path_for_CircleInner2Outer_normal);
                m_images_for_circle_inner_to_outer[1] = Image.FromFile(m_file_path_for_CircleInner2Outer_hovered);
                m_images_for_circle_inner_to_outer[2] = Image.FromFile(m_file_path_for_CircleInner2Outer_pressed);
                m_images_for_pt_2_pt_distance[0] = Image.FromFile(m_file_path_for_Pt2Pt_normal);
                m_images_for_pt_2_pt_distance[1] = Image.FromFile(m_file_path_for_Pt2Pt_hovered);
                m_images_for_pt_2_pt_distance[2] = Image.FromFile(m_file_path_for_Pt2Pt_pressed);
                m_images_for_hand_pick_circle[0] = Image.FromFile(m_file_path_for_HandPickCircle_normal);
                m_images_for_hand_pick_circle[1] = Image.FromFile(m_file_path_for_HandPickCircle_hovered);
                m_images_for_hand_pick_circle[2] = Image.FromFile(m_file_path_for_HandPickCircle_pressed);
                m_images_for_light_on = Image.FromFile(m_file_path_for_light_on);
                m_images_for_light_off = Image.FromFile(m_file_path_for_light_off);
                m_images_for_HandLL[0] =Image.FromFile(m_file_path_for_HandLL_level_hovered);
                m_images_for_HandLL[1] = Image.FromFile(m_file_path_for_HandLL_level_normal);
                m_images_for_HandLL[2] = Image.FromFile(m_file_path_for_HandLL_vertical_hovered);
                m_images_for_HandLL[3] = Image.FromFile(m_file_path_for_HandLL_vertical_normal);
                m_images_for_HandLL[4] = Image.FromFile(m_file_path_for_HandLL_hovered);
                m_images_for_HandLL[5] = Image.FromFile(m_file_path_for_HandLL_normal);
                m_images_for_HandPL[0] = Image.FromFile(m_file_path_for_HandPL_level_hovered);
                m_images_for_HandPL[1] = Image.FromFile(m_file_path_for_HandPL_level_normal);
                m_images_for_HandPL[2] = Image.FromFile(m_file_path_for_HandPL_vertical_hovered);
                m_images_for_HandPL[3] = Image.FromFile(m_file_path_for_HandPL_vertical_normal);
                m_images_for_HandPL[4] = Image.FromFile(m_file_path_for_HandPL_hovered);
                m_images_for_HandPL[5] = Image.FromFile(m_file_path_for_HandPL_normal);
                Debugger.Log(0, null, string.Format("222222 MainUI_Load 333"));
                m_mv_bitmap = new Bitmap(ui_MainImage.Width, ui_MainImage.Height);
                Debugger.Log(0, null, string.Format("222222 MainUI_Load aaa"));
                this.ui_btn_14LineWidth.Image = m_images_for_line_width_14[0];
                this.ui_btn_23LineWidth.Image = m_images_for_line_width_23[0];
                this.ui_btn_13LineWidth.Image = m_images_for_line_width_13[0];
                this.ui_btn_1234LineWidth.Image = m_images_for_line_width_1234[0];
                this.ui_btn_LineSpace.Image = m_images_for_line_space[0];
                this.ui_btn_ArcLineSpace.Image = m_images_for_arc_line_space[0];
                this.ui_btn_ShortSpaceBetweenTwoEmptyCircles.Image = m_images_for_short_space_between_two_empty_circles[0];
                this.ui_btn_Line.Image = m_images_for_line[0];
                this.ui_btn_ArcLineWidth.Image = m_images_for_arc_line_width[0];
                this.ui_btn_Pt2PtDistance.Image = m_images_for_pt_2_pt_distance[0];
                this.ui_btn_CircleOuterToInner.Image = m_images_for_HandLL[2];
                
                this.ui_btn_CircleInnerToOuter.Image = m_images_for_circle_inner_to_outer[0];
                this.ui_btn_HandPickCircle.Image = m_images_for_hand_pick_circle[0];
                this.pictureBox_MotionPad.Image = Image.FromFile("icons\\小地图.bmp");
                //this.ui_GuideImage.Image = Image.FromFile("icons\\guideview.bmp");
                //this.ui_GuideImageMark.Image = Image.FromFile("icons\\guideviewmark.bmp");
                //this.ui_GraphView.Image = Image.FromFile("icons\\graph.png");
                Debugger.Log(0, null, string.Format("222222 MainUI_Load bbb"));
                this.btn_InitSystem.Image = Image.FromFile("icons\\init.png");
                this.btn_Home.Image = Image.FromFile("icons\\home.png");
                this.btn_CreateTask.Image = Image.FromFile("icons\\create.png");
                this.btn_LoadTask.Image = Image.FromFile("icons\\load.png");
                this.btn_LoadGraph.Image = Image.FromFile("icons\\cam.png");
                this.btn_Pause.Image = Image.FromFile("icons\\pause.png");
                this.btn_RunTask.Image = Image.FromFile("icons\\run.png");
                this.btn_StopTask.Image = Image.FromFile("icons\\stop.png");
                this.btn_GotoPCBLeftBottom.Image = Image.FromFile("icons\\leftbottom.png");
                this.btn_Settings.Image = Image.FromFile("icons\\settings.png");
                this.btn_AutoFocus.Image = Image.FromFile("icons\\focus.png");
                this.ui_btn_handLL_vertical.Image = m_images_for_HandLL[0];
                ui_btn_handLL_vertical.Image = m_images_for_HandPL[0];
                ui_btn_handPL_horizontal.Image = m_images_for_HandPL[2];
                Debugger.Log(0, null, string.Format("222222 MainUI_Load ccc"));
                set_gauger(MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR);
                Debugger.Log(0, null, string.Format("222222 MainUI_Load 444"));
                refresh_light_icons();

                ui_MainImage.MouseWheel += new MouseEventHandler(ui_MainImage_MouseWheel);

                gridview_MeasureTask.Size = new Size(730, gridview_MeasureTask.Height);
                gridview_GraphMeasureItems.Width = gridview_MeasureTask.Width;
                gridview_measure_results.Width = gridview_MeasureTask.Width;

                gridview_MeasureTask.ContextMenuStrip = this.menu_TaskDataView;
                gridview_MeasureTask.RowHeadersVisible = false;
                gridview_MeasureTask.ReadOnly = true;
                gridview_MeasureTask.ColumnCount = 11;
                gridview_MeasureTask.ColumnHeadersHeight = 30;
                gridview_MeasureTask.ColumnHeadersVisible = true;
                gridview_MeasureTask.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                gridview_MeasureTask.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                gridview_MeasureTask.Columns[0].Width = 38;
                gridview_MeasureTask.Columns[1].Width = 100;
                gridview_MeasureTask.Columns[2].Width = 80;
                gridview_MeasureTask.Columns[3].Width = 70;
                gridview_MeasureTask.Columns[4].Width = 66;
                gridview_MeasureTask.Columns[5].Width = 66;
                gridview_MeasureTask.Columns[6].Width = 45;
                gridview_MeasureTask.Columns[7].Width = 45;
                gridview_MeasureTask.Columns[8].Width = 80;
                gridview_MeasureTask.Columns[9].Width = 80;
                gridview_MeasureTask.Columns[10].Width = 77;
                gridview_MeasureTask.Columns[0].Name = "序号";
                gridview_MeasureTask.Columns[1].Name = "类型";
                gridview_MeasureTask.Columns[2].Name = "测量名称";
                gridview_MeasureTask.Columns[3].Name = "标准值";
                gridview_MeasureTask.Columns[4].Name = "上限";
                gridview_MeasureTask.Columns[5].Name = "下限";
                gridview_MeasureTask.Columns[6].Name = "单位";
                gridview_MeasureTask.Columns[7].Name = "倍率";
                gridview_MeasureTask.Columns[8].Name = "上环光";
                gridview_MeasureTask.Columns[9].Name = "下环光";
                gridview_MeasureTask.Columns[10].Name = "首件坐标";
                gridview_MeasureTask.Show();
                for (int n = 0; n < gridview_MeasureTask.ColumnCount; n++)
                    gridview_MeasureTask.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
                Debugger.Log(0, null, string.Format("222222 MainUI_Load 555"));
                gridview_GraphMeasureItems.ContextMenuStrip = this.menu_GraphMeasureItems;
                gridview_GraphMeasureItems.RowHeadersVisible = false;
                gridview_GraphMeasureItems.ReadOnly = true;
                gridview_GraphMeasureItems.ColumnCount = 11;
                gridview_GraphMeasureItems.ColumnHeadersHeight = 30;
                gridview_GraphMeasureItems.ColumnHeadersVisible = true;
                gridview_GraphMeasureItems.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                gridview_GraphMeasureItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                gridview_GraphMeasureItems.Columns[0].Width = 38;
                gridview_GraphMeasureItems.Columns[1].Width = 80;
                gridview_GraphMeasureItems.Columns[2].Width = 80;
                gridview_GraphMeasureItems.Columns[3].Width = 70;
                gridview_GraphMeasureItems.Columns[4].Width = 66;
                gridview_GraphMeasureItems.Columns[5].Width = 66;
                gridview_GraphMeasureItems.Columns[6].Width = 45;
                gridview_GraphMeasureItems.Columns[7].Width = 45;
                gridview_GraphMeasureItems.Columns[8].Width = 80;
                gridview_GraphMeasureItems.Columns[9].Width = 80;
                gridview_GraphMeasureItems.Columns[10].Width = 77;
                gridview_GraphMeasureItems.Columns[0].Name = "序号";
                gridview_GraphMeasureItems.Columns[1].Name = "类型";
                gridview_GraphMeasureItems.Columns[2].Name = "测量名称";
                gridview_GraphMeasureItems.Columns[3].Name = "标准值";
                gridview_GraphMeasureItems.Columns[4].Name = "上限";
                gridview_GraphMeasureItems.Columns[5].Name = "下限";
                gridview_GraphMeasureItems.Columns[6].Name = "单位";
                gridview_GraphMeasureItems.Columns[7].Name = "倍率";
                gridview_GraphMeasureItems.Columns[8].Name = "上环光";
                gridview_GraphMeasureItems.Columns[9].Name = "下环光";
                gridview_GraphMeasureItems.Columns[10].Name = "首件坐标";
                gridview_GraphMeasureItems.Hide();
                for (int n = 0; n < gridview_GraphMeasureItems.ColumnCount; n++)
                    gridview_GraphMeasureItems.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
                Debugger.Log(0, null, string.Format("222222 MainUI_Load 666"));
                gridview_measure_results.RowHeadersVisible = false;
                gridview_measure_results.ReadOnly = true;
                gridview_measure_results.ColumnCount = 11;
                gridview_measure_results.ColumnHeadersHeight = 30;
                gridview_measure_results.ColumnHeadersVisible = true;
                gridview_measure_results.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                gridview_measure_results.Columns[0].Width = 38;
                gridview_measure_results.Columns[1].Width = 112;
                gridview_measure_results.Columns[2].Width = 100;
                //gridview_measure_results.Columns[3].Width = 80;
                gridview_measure_results.Columns[3].Width = 70;
                gridview_measure_results.Columns[4].Width = 66;
                gridview_measure_results.Columns[5].Width = 66;
                gridview_measure_results.Columns[6].Width = 45;
                gridview_measure_results.Columns[7].Width = 65;
                gridview_measure_results.Columns[8].Width = 50;
                gridview_measure_results.Columns[9].Width = 55;
                gridview_measure_results.Columns[10].Width = 80;
                gridview_measure_results.Columns[0].Name = "序号";
                gridview_measure_results.Columns[1].Name = "图片名称";
                gridview_measure_results.Columns[2].Name = "类型";
                //gridview_measure_results.Columns[3].Name = "测量名称";
                gridview_measure_results.Columns[3].Name = "标准值";
                gridview_measure_results.Columns[4].Name = "上限";
                gridview_measure_results.Columns[5].Name = "下限";
                gridview_measure_results.Columns[6].Name = "单位";
                gridview_measure_results.Columns[7].Name = "测量值";
                gridview_measure_results.Columns[8].Name = "结果";
                gridview_measure_results.Columns[9].Name = "倍率";
                gridview_measure_results.Columns[10].Name = "自动/手动";
                gridview_measure_results.Hide();
                for (int n = 0; n < gridview_measure_results.ColumnCount; n++)
                    gridview_measure_results.Columns[n].SortMode = DataGridViewColumnSortMode.NotSortable;
                Debugger.Log(0, null, string.Format("222222 MainUI_Load 777"));
                //this.tabControl_Task.SelectedIndex = 1;

                ui_trackBar_TopLight.Value = m_top_light.m_nBrightness;
                ui_trackBar_BottomLight.Value = m_bottom_light.m_nBrightness;

                ui_trackBar_SmallSearchFrameExtent.Value = m_nSmallSearchFrameExtent;
                ui_trackBar_BigSearchFrameExtent.Value = m_nBigSearchFrameExtent;
                Debugger.Log(0, null, string.Format("222222 MainUI_Load 888"));
                ui_GraphView.Hide();

                m_tooltip_for_top_light_button.InitialDelay = 50;
                m_tooltip_for_top_light_button.ReshowDelay = 50;
                m_tooltip_for_bottom_light_button.InitialDelay = 50;
                m_tooltip_for_bottom_light_button.ReshowDelay = 50;
                m_tooltip_for_measure_item.InitialDelay = 50;
                m_tooltip_for_measure_item.ReshowDelay = 50;

                m_graph_view = new GraphView(this, 0);
                m_graph_view.Width = ui_GraphView.Width;
                m_graph_view.Height = ui_GraphView.Height;
                m_graph_view.Top = ui_GraphView.Top;
                m_graph_view.Left = ui_GraphView.Left;
                m_graph_view.BorderStyle = BorderStyle.FixedSingle;
                m_graph_view.SizeMode = PictureBoxSizeMode.Zoom;
                m_graph_view.BackColor = Color.Black;
                m_graph_view.ContextMenuStrip = this.menu_GraphView;
                //this.Controls.Add(m_graph_view);
                Debugger.Log(0, null, string.Format("222222 MainUI_Load 999"));
                this.checkBox_ConfirmFiducialMark.Checked = m_bNeedConfirmFiducialMark;
                this.checkBox_ConfirmMeasureResult.Checked = m_bNeedConfirmMeasureResult;
                this.checkBox_ConfirmNGWhenRunningTask.Checked = m_bNeedConfirmNGWhenRunningTask;
                this.checkBox_UseAutofocus.Checked = m_bUseAutofocusWhenRunningTask;
                this.checkBox_AutoAdjustLightDuringTaskCreation.Checked = m_bAutoAdjustLightDuringTaskCreation;
                
                string[] jog_ratios = new string[] { "慢", "较慢", "标准", "较快", "快" };
                for (int n = 0; n < jog_ratios.Length; n++)
                    comboBox_JogSpeedRatio.Items.Add(jog_ratios[n]);
                comboBox_JogSpeedRatio.SelectedIndex = m_nJogSpeedRatio;

                comboBox_Unit.Items.Add(m_strUnits[0]);
                comboBox_Unit.Items.Add(m_strUnits[1]);
                comboBox_Unit.Items.Add(m_strUnits[2]);
                comboBox_Unit.SelectedIndex = m_nUnitType;
                Debugger.Log(0, null, string.Format("222222 MainUI_Load aaa"));
                m_form_orientation = new Form_GraphOrientation(this);

                comboBox_ProductType.Items.Add("盲孔");
                comboBox_ProductType.Items.Add("通孔");
                comboBox_ProductType.SelectedIndex = 0;

                textBox_NumOfTaskRepetitions.Text = m_nNumOfTaskRepetition.ToString();
                checkBox_RepeatRunningTask.Checked = m_bRepeatRunningTask;
                checkBox_SetHorizonModeForGaugerRect.Checked = m_bSetHorizonModeForGaugerRect;

                m_excel_ops.init();

                Debugger.Log(0, null, "222222 程序启动 end");
                m_bIsAppInited = true;

                gridview_MeasureTask.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                //read_three_marks_records_from_file(System.Environment.CurrentDirectory + "\\three_marks_records.txt", m_list_three_marks_records);

                if (true)
                {
                    //dl_message_sender messenger = CBD_SendMessage;
                    //new Thread(thread_trigger_init_third_party_libraries).Start(messenger);
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 程序启动 MainUI_Load() 异常信息: {0}", ex.Message));
            }
        }
        
        // 主窗口关闭
        private void MainUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((true == m_motion.m_bHomed) && (false == m_bIgnoreHardwaresRelease))
            {
                if ((Math.Abs(m_current_xyz.x) > 0.01) || (Math.Abs(m_current_xyz.y) > 0.01) || (Math.Abs(m_current_xyz.z) > 0.01))
                {
                    if (DialogResult.Yes == MessageBox.Show(this, "是否让运动系统返回原点?", "提示", MessageBoxButtons.YesNo))
                    {
                        bool bOK = m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, 0, 20);

                        Thread.Sleep(200);

                        bOK = m_motion.linear_XY_wait_until_stop(0, 0, 500, 1, false);
                    }
                }
            }
            
            m_bExitProgram = true;
            m_bStopTask = true;
            m_bStopExecution = true;

            Thread.Sleep(80);

            if (true == m_bIsAppInited)
            {
                m_IO.save_params();
                if (true == m_len.m_bInitialized)
                    m_len.save_params();
                m_motion.save_params();
                m_main_camera.save_params();
                m_guide_camera.save_params();
                m_top_light.save_params();
                m_top_light.close_light();
                m_bottom_light.save_params();
                m_bottom_light.close_light();
            }
            
            if (false == m_bOfflineMode)
            {
                m_main_camera.release();
                m_guide_camera.release();
            }
            if (0 == m_nCameraType)
                PMSGigE.PMSTerminateLib();
            else if (1 == m_nCameraType)
                ;

            m_len.release();

            if (m_bIsSQLConnected && m_bUseDatabase)
            {
                m_SQL_conn_measure_task.Close();
            }

            if ((false == m_bOfflineMode) && (false == m_bIgnoreHardwaresRelease))
            {
                hardware_ops_enable_vacuum(false);
                
                // 关闭红点指示器
                if (false == m_IO.set_IO_output(m_IO.m_output_red_dot, Hardwares.IO_STATE.IO_HIGH))
                    MessageBox.Show(this, "红点指示器IO设置失败，请检查原因。", "提示");

                // 关闭颜色灯
                m_IO.set_IO_output(m_IO.m_output_green_light, Hardwares.IO_STATE.IO_HIGH);
                m_IO.set_IO_output(m_IO.m_output_yellow_light, Hardwares.IO_STATE.IO_HIGH);
                m_IO.set_IO_output(m_IO.m_output_red_light, Hardwares.IO_STATE.IO_HIGH);
            }
            
            m_motion.release();

            if (true == m_bIsAppInited)
            {
                SaveAppParams();

                SaveCalibData(m_strCalibDataPath, m_calib_data);
            }

            m_event_wait_for_manual_gauge.Set();
            m_event_wait_for_next_image.Set();
            m_event_wait_for_confirm_during_autorun.Set();
            m_event_wait_for_confirm_during_creation.Set();
            m_reset_event_for_updating_thumbnail_progress.Set();
            m_reset_event_for_updating_graphview_progress.Set();

            m_excel_ops.release();

            Thread.Sleep(280);
            
            //System.Environment.Exit(0);
        }
        
        // 主图像绘制Paint
        private void ui_MainImage_Paint(object sender, PaintEventArgs e)
        {
            if (false == m_bIsAppInited)
                return;
            if (null == ui_MainImage.Image)
                return;

            m_gauger.set_horizon_mode(m_bSetHorizonModeForGaugerRect);
            m_gauger.on_paint(e, ui_MainImage);

            refresh_ChenLing_contents(e);

            Graphics g = e.Graphics;

            double ratio_x = (double)(ui_MainImage.Image.Width) / (double)(ui_MainImage.Width);
            double ratio_y = (double)(ui_MainImage.Image.Height) / (double)(ui_MainImage.Height);

            // 显示手拉区域  蓝色
            if ((true == m_bShowHandDrawnRegion) && (m_nCurrentMeasureItemIdx < m_current_task_data.Count))
            {
                MeasurePointData data = m_current_task_data[m_nCurrentMeasureItemIdx];

                Pen p = new Pen(Color.FromArgb(0, 0, 255), 2);
                double radius = 80;

                g.DrawEllipse(p, (float)(((data.m_hand_drawn_center.x + m_base_template_pos.m_matched_pos.x) / ratio_x) - radius), 
                    (float)(((data.m_hand_drawn_center.y + m_base_template_pos.m_matched_pos.y) / ratio_y) - radius), (float)radius * 2, (float)radius * 2);
            }

            // NG时显示NG项的测量位置
            if (true == m_bShowSelectionFrameOnNG)
            {
                Pen p = new Pen(Color.FromArgb(99, 188, 242), 2);

                Point[] pts = new Point[4];
                for (int n = 0; n < 4; n++)
                {
                    pts[n].X = (int)(m_selection_frame_rect[n].x / ratio_x);
                    pts[n].Y = (int)(m_selection_frame_rect[n].y / ratio_y);
                }

                for (int n = 0; n < 4; n++)
                    g.DrawLine(p, pts[n], pts[(n + 1) % 4]);
            }

            // 显示定位到的基准特征模板位置
            if (true == m_bShowBaseTemplateLocation)
            {
                Pen p = new Pen(Color.FromArgb(255, 0, 0), 2);
                
                Point[] pts = new Point[4];
                for (int n = 0; n < 4; n++)
                {
                    pts[n].X = (int)(m_base_template_pos.m_rect[n].x / ratio_x);
                    pts[n].Y = (int)(m_base_template_pos.m_rect[n].y / ratio_y);
                }

                for (int n = 0; n < 4; n++)
                    g.DrawLine(p, pts[n], pts[(n + 1) % 4]);
            }
            
            //Debugger.Log(0, null, string.Format("222222 m_bSaveGaugeResultImage = {0},  {1},  {2}", m_bSaveGaugeResultImage, m_bTriggerSaveGaugeImage, m_bIsRunningTask));
            // 保存测量结果图片
            if (m_bSaveGaugeResultImage && m_bTriggerSaveGaugeImage && m_bIsRunningTask)
            {
                m_bTriggerSaveGaugeImage = false;
                
                for (int n = 0; n < m_vec_history_gaugers.Count; n++)
                    m_vec_history_gaugers[n].on_paint(e, ui_MainImage, m_picture_control_backup_image, true);

                ui_MainImage.DrawToBitmap(m_mv_bitmap, ui_MainImage.ClientRectangle);

                //Thread t = new Thread(thread_save_gauge_result_image);
                //t.Start();
                thread_save_gauge_result_image("");
            }

            // 显示清晰度
            if (true)
            {
                String str = string.Format("清晰度: {0:0.}", m_current_sharpness_of_main_cam);
                String str2 = string.Format("亮度: {0:0.}/{1:0.}  FPS: {2:0.}/{3:0.} ", m_current_brightness_of_main_cam, m_current_brightness_of_guide_cam, m_main_camera.m_fps, m_guide_camera.m_fps);
                String str3 = "当前倍率: ";
                String str4 = "当前单位: ";
                Font ft = new Font("宋体", 13, FontStyle.Bold);
                Font ft2 = new Font("宋体", 12, FontStyle.Bold);
                Brush brush = new SolidBrush(Color.FromArgb(0, 178, 0));
                Brush brush2 = new SolidBrush(Color.FromArgb(0, 178, 0));
                ////g.DrawString(str, ft, bush, ui_MainImage.Width - 135, ui_MainImage.Height - 65);
                ////g.DrawString(str2, ft, bush, ui_MainImage.Width - 135, ui_MainImage.Height - 40);
                ////g.DrawString(str3, ft2, bush2, ui_MainImage.Width - 148, 3);
                ////g.DrawString(str4, ft2, bush2, ui_MainImage.Width - 148, 42);
                //g.DrawString(str, ft, brush, 0, ui_MainImage.Height - 55);
                //g.DrawString(str2, ft, brush, 0, ui_MainImage.Height - 30);
                g.DrawString(str3, ft2, brush2, 0, 3);
                g.DrawString(str4, ft2, brush2, 0, 38);
            }

            // 显示十字线
            if ((true == m_bShowCrossLine) && (null != ui_MainImage.Image))
            {
                Pen p = new Pen(Color.FromArgb(0, 255, 0), 2);
                g.DrawLine(p, new Point(0, ui_MainImage.Height / 2), new Point(ui_MainImage.Width, ui_MainImage.Height / 2));
                g.DrawLine(p, new Point(ui_MainImage.Width / 2, 0), new Point(ui_MainImage.Width / 2, ui_MainImage.Height));
            }

            // 显示搜索框
            if ((true == m_bShowSmallSelectionFrame) && (m_current_task_data.Count > 0) && (m_nCurrentMeasureItemIdx < m_current_task_data.Count))
            {
                Pen p = new Pen(Color.FromArgb(0, 255, 0), 2);

                float factor_x = (float)ui_MainImage.Width / (float)ui_MainImage.Image.Width;
                float factor_y = (float)ui_MainImage.Height / (float)ui_MainImage.Image.Height;

                MeasurePointData data = m_current_task_data[m_nCurrentMeasureItemIdx];
                data.m_strStepsFileName = m_strCurrentProductStep;
                data.m_strLayerFileName = m_strCurrentProductLayer;
                switch (data.m_mes_type)
                {
                    case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                    case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                        double extend = m_nSmallSelectionFrameExtension;
                        double radius = m_dbAccurateMarkRadius / 2;
                        Point2d center = new Point2d(m_ptSelectionFrameCenter.x, m_ptSelectionFrameCenter.y);
                        Point2f start = new Point2f((center.x - radius - extend) * factor_x, (center.y - radius - extend) * factor_y);
                        Point2f end = new Point2f((center.x + radius + extend) * factor_x, (center.y + radius + extend) * factor_y);
                        g.DrawRectangle(p, start.x, start.y, end.x - start.x, end.y - start.y);
                        break;

                    case MEASURE_TYPE.LINE_WIDTH_14:
                    case MEASURE_TYPE.LINE_WIDTH_23:
                    case MEASURE_TYPE.LINE_WIDTH_13:
                    case MEASURE_TYPE.LINE_WIDTH_1234:
                    case MEASURE_TYPE.LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_SPACE:
                    case MEASURE_TYPE.LINE:
                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                    case MEASURE_TYPE.L_SHAPE:
                    case MEASURE_TYPE.BULGE:
                    case MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR:
                    case MEASURE_TYPE.LINE_TO_EDGE:
                    case MEASURE_TYPE.ETCH_DOWN:
                        Point2d[] rect = new Point2d[4];

                        for (int n = 0; n < 4; n++)
                        {
                            //Debugger.Log(0, null, string.Format("222222 n {0}: [{1:0.000},{2:0.000}]", n, data.m_handmade_ROI_rect[n].x, data.m_handmade_ROI_rect[n].y));

                            if (0 == data.m_create_mode)
                            {
                                rect[n].x = data.m_handmade_ROI_rect[n].x * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir);
                                rect[n].y = data.m_handmade_ROI_rect[n].y * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir);
                            }
                            else
                            {
                                if ((Math.Abs(data.m_handmade_ROI_rect[n].x) < 0.00001) && (Math.Abs(data.m_handmade_ROI_rect[n].y) < 0.00001))
                            {
                                    //Debugger.Log(0, null, string.Format("222222 eee"));
                                    
                                double offset_x = data.m_graphmade_ROI_rect[n].x - data.m_center_x_in_metric;
                                double offset_y = data.m_graphmade_ROI_rect[n].y - data.m_center_y_in_metric;

                                rect[n].x = offset_x * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir);
                                rect[n].y = offset_y * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir);
                            }
                            else
                            {
                                //Debugger.Log(0, null, string.Format("222222 fff"));
                                rect[n].x = data.m_handmade_ROI_rect[n].x * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir);
                                rect[n].y = data.m_handmade_ROI_rect[n].y * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir);
                                }
                            }

                            rect[n].x = (rect[n].x + ui_MainImage.Image.Width / 2) * factor_x;
                            rect[n].y = (rect[n].y + ui_MainImage.Image.Height / 2) * factor_y;
                        }
                        for (int n = 0; n < 4; n++)
                            g.DrawLine(p, new PointF((float)rect[n].x, (float)rect[n].y), new PointF((float)rect[(n + 1) % 4].x, (float)rect[(n + 1) % 4].y));
                        
                        break;
                }
            }
            
            // 做首件时显示搜索框
            if ((true == m_bShowFrameDuringTaskCreation) && (null != m_current_data))
            {
                if (0 != m_current_data.m_create_mode)
                {
                    Pen p = new Pen(Color.FromArgb(0, 255, 0), 2);

                    float factor_x = (float)ui_MainImage.Width / (float)ui_MainImage.Image.Width;
                    float factor_y = (float)ui_MainImage.Height / (float)ui_MainImage.Image.Height;

                    switch (m_current_data.m_mes_type)
                    {
                        case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                        case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                        case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                        case MEASURE_TYPE.LINE_WIDTH_14:
                        case MEASURE_TYPE.LINE_WIDTH_23:
                        case MEASURE_TYPE.LINE_WIDTH_13:
                        case MEASURE_TYPE.LINE_WIDTH_1234:
                        case MEASURE_TYPE.LINE_SPACE:
                        case MEASURE_TYPE.ARC_LINE_SPACE:
                        case MEASURE_TYPE.LINE:
                        case MEASURE_TYPE.ARC_LINE_WIDTH:
                        case MEASURE_TYPE.L_SHAPE:
                        case MEASURE_TYPE.BULGE:
                        case MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR:
                        case MEASURE_TYPE.LINE_TO_EDGE:
                        case MEASURE_TYPE.ETCH_DOWN:
                            Point2d[] rect = new Point2d[4];
                            for (int n = 0; n < 4; n++)
                            {
                                double offset_x = m_current_data.m_graphmade_ROI_rect[n].x - m_current_data.m_center_x_in_metric;
                                double offset_y = m_current_data.m_graphmade_ROI_rect[n].y - m_current_data.m_center_y_in_metric;
                                
                                rect[n].x = offset_x * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir);
                                rect[n].y = offset_y * 1000 * m_calib_data[comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir);
                                
                                rect[n].x = (rect[n].x + ui_MainImage.Image.Width / 2) * factor_x;
                                rect[n].y = (rect[n].y + ui_MainImage.Image.Height / 2) * factor_y;
                                //Debugger.Log(0, null, string.Format("222222 n {0}: rect[n].x [{1:0.000},{2:0.000}], [{3:0.000},{4:0.000}]", n, rect[n].x, rect[n].y,
                                //    m_current_data.m_graphmade_ROI_rect[n].x, m_current_data.m_graphmade_ROI_rect[n].y));
                            }
                            for (int n = 0; n < 4; n++)
                                g.DrawLine(p, new PointF((float)rect[n].x, (float)rect[n].y), new PointF((float)rect[(n + 1) % 4].x, (float)rect[(n + 1) % 4].y));

                            break;
                    }
                }
            }

            // 在主图像上高亮显示通过精定位找到的定位孔
            if (true == m_bShowAccurateMark)
            {
                Pen p = new Pen(Color.FromArgb(0, 255, 0), 2);

                float factor_x = (float)ui_MainImage.Width / (float)ui_MainImage.Image.Width;
                float factor_y = (float)ui_MainImage.Height / (float)ui_MainImage.Image.Height;
                float radius = (float)m_dbAccurateMarkRadius * (factor_x + factor_y) / 2;

                float center_x = (float)m_ptAccurateMarkCenter.x * factor_x;
                float center_y = (float)m_ptAccurateMarkCenter.y * factor_y;

                float x = center_x - radius;
                float y = center_y - radius;

                //Debugger.Log(0, null, string.Format("222222 通过粗定位找到的定位孔 radius {0:0.000}", radius));
                g.DrawEllipse(p, x, y, radius, radius);
            }

            return;
        }

        // 导航图像绘制Paint
        private void ui_GuideImage_Paint(object sender, PaintEventArgs e)
        {
            if (false == m_bIsAppInited)
                return;

            // 显示十字线
            if ((true == m_bShowCrossLine) && (null != ui_GuideImage.Image))
            {
                Graphics g = e.Graphics;
                Pen p = new Pen(Color.FromArgb(0, 255, 0), 2);
                g.DrawLine(p, new Point(0, ui_GuideImage.Height / 2), new Point(ui_GuideImage.Width, ui_GuideImage.Height / 2));
                g.DrawLine(p, new Point(ui_GuideImage.Width / 2, 0), new Point(ui_GuideImage.Width / 2, ui_GuideImage.Height));
            }

            // 在导航图像上高亮显示通过粗定位找到的定位孔
            if (true == m_bShowCoarseMark)
            {
                Graphics g = e.Graphics;
                Pen p = new Pen(Color.FromArgb(0, 255, 0), 2);

                float factor_x = (float)ui_GuideImage.Width / (float)ui_GuideImage.Image.Width;
                float factor_y = (float)ui_GuideImage.Height / (float)ui_GuideImage.Image.Height;
                float radius = (float)(m_dbCoarseMarkRadius * m_guide_camera.m_pixels_per_um);

                //Debugger.Log(0, null, string.Format("222222 通过粗定位找到的定位孔 radius {0:0.000}", radius));
                g.DrawEllipse(p, ((ui_GuideImage.Image.Width - radius) / 2) * factor_x, ((ui_GuideImage.Image.Height - radius) / 2) * factor_y, 
                    radius * 1 * factor_x, radius * 1 * factor_y);
            }
        }

        // 用户权限看门狗定时器
        private void timer_UserFeedDogCounter_Tick(object sender, EventArgs e)
        {
            if (true == m_bRepeatRunningTask)
                return;

            m_nFeedDogCounter++;
            if (m_nFeedDogCounter > 180)
                CBD_SendMessage("登录", false, 0, m_strUseringName);
        }

        // 喂狗
        public void feed_dog()
        {
            m_nFeedDogCounter = 0;
        }

        // 测试
        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(ui_MainImage.Width, ui_MainImage.Height, PixelFormat.Format1bppIndexed);

            int nLineWidth = (((bmp.Width + 31) / 32) * 4);
            byte[] bmpDataBuf = new byte[nLineWidth * bmp.Height];

            int h = bmp.Height;
            int w = bmp.Width;
            for (int a = 0; a < h; a++)
            {
                int offset = a * nLineWidth;
                for (int b = 0; b < w; b++)
                {
                    bmpDataBuf[offset + (b / 8)] = 0xFF;
                    if (b > 0 && (b + 1) % 8 == 0)
                        b += 8;
                }
            }

            Rectangle rect = new Rectangle(new Point(0, 0), bmp.Size);
            BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            System.Runtime.InteropServices.Marshal.Copy(bmpDataBuf, 0, bmp_data.Scan0, bmpDataBuf.Length);
            bmp.UnlockBits(bmp_data);

            string msg = string.Format("222222 bmp.Width = [{0},{1}], nLineWidth = {2}, len = {3}", bmp.Width, bmp.Height, nLineWidth, bmpDataBuf.Length);
            Debugger.Log(0, null, msg);

            ColorPalette palette = bmp.Palette;
            Color[] colors = palette.Entries;
            colors[0] = Color.FromArgb(255, 0, 0, 0);
            colors[1] = Color.FromArgb(255, 0, 255, 0);

            bmp.Palette = palette;
            //Bitmap bmp = new Bitmap(ui_MainImage.Width, ui_MainImage.Height);
            //Graphics g = Graphics.FromImage(bmp);
            //g.Clear(Color.White);
            //if (null != m_main_image)
            //{
            //    m_main_image.Dispose();
            //    GC.Collect();
            //}
            this.ui_MainImage.Image = bmp;
            //this.ui_MainImage.Image = m_main_image;
        }
        
        private void MainUI_Activated(object sender, EventArgs e)
        {
            m_bIsFormActivated = true;
        }

        private void MainUI_Deactivate(object sender, EventArgs e)
        {
            m_bIsFormActivated = false;

            m_bJogAxis = false;
            m_bIsCtrlKeyPressed = false;
            btn_Left_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
            btn_Right_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
            btn_Forward_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
            btn_Backward_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
            btn_Z_down_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
            btn_Z_up_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
        }

        // 刷新光源图标
        public void refresh_light_icons()
        {
            this.btn_top_light.Image = m_top_light.m_bOn == true ? m_images_for_light_on : m_images_for_light_off;
            this.btn_bottom_light.Image = m_bottom_light.m_bOn == true ? m_images_for_light_on : m_images_for_light_off;
        }

        private void ui_MainImage_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            //HObject obj;
            //HOperatorSet.ReadImage(out obj, "C:\\00-ZWLineGauger\\ZWLineGauger\\ZWLineGauger\\bin\\x64\\Release\\0.bmp");
            //HOperatorSet.WriteImage(obj, "bmp", 0, "dd.bmp");

            new Thread(thread_find_shape_model_test).Start();
            //new Thread(thread_zip_test).Start();

            if (false)
            {
                if (true == m_graph_view.m_bHasValidImage)
                {
                    Debugger.Log(0, null, string.Format("222222 Width = {0}, Height = {1}, offset = [{2},{3}]",
                            m_graph_view.m_bitmap_1bit.Width, m_graph_view.m_bitmap_1bit.Height, m_nGraphOffsetX, m_nGraphOffsetY));

                    double x = (double)m_nGraphOffsetX;
                    double y = (double)(m_graph_view.m_bitmap_1bit.Height - m_nGraphOffsetY * 2) * 0.9;
                    m_graph_view.set_view_ratio_and_crd(m_graph_view.m_zoom_ratio_min * 7, x, y);
                    m_graph_view.refresh_image();
                }
            }
            
            if (false)
            {
                string strFileName = "C:\\00-ZWLineGauger\\ZWLineGauger\\ZWLineGauger\\bin\\x64\\Release\\测试33.xlsx";

                try
                {
                    string[] names = new string[] { "1", "2咧", "abs", "2咧", "abs" };

                    m_excel_ops.create_excel_file(strFileName, names, 10, m_strTaskRunningStartingHMSTime);

                    m_excel_ops.open(strFileName);

                    m_excel_ops.close_current_file();
                }
                catch (Exception ex)
                {
                    Debugger.Log(0, null, string.Format("222222 office excel error: {0}", ex.Message));
                }
            }
        }

        private void textBox_BatchNum_TextChanged(object sender, EventArgs e)
        {
            //this.textBox_BatchNum.Text = "1";
            m_strBatchNum = this.textBox_BatchNum.Text;
            
        }

        private void btn_FontSet_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowColor = true;//这行代码一定要出现在fontDialog1.ShowDialog()之前。

            fontDialog1.ShowDialog();
            lblFont.Font = fontDialog1.Font;
            lblFont.ForeColor = fontDialog1.Color;
            globaldata.m_fontname = fontDialog1.Font.Name;
            globaldata.m_fontsize = fontDialog1.Font.Size;
            globaldata.m_fontcolor = fontDialog1.Color.ToArgb().ToString("X8");
            globaldata.m_fontstyle = (int)fontDialog1.Font.Style; ;
            //Color color = colorDialog1.Color;
            // string a = color.ToArgb().ToString(“X8”);
            // color = ColorTranslator.FromHtml(“#”+a);
            //this.BackColor = color;

        }

       
    }
}
