using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZWLineGauger.Forms;
using ZWLineGauger.Gaugers;

namespace ZWLineGauger
{
    public class StageGraphCrdPair
    {
        public Point2d stage_crd = new Point2d(0, 0);
        public Point2d graph_crd = new Point2d(0, 0);
    }

    public partial class MainUI : Form
    {
        Gauger m_gauger;

        public MEASURE_TYPE   m_current_measure_type = MEASURE_TYPE.NONE;
        MEASURE_TYPE             m_prev_measure_type = MEASURE_TYPE.NONE;

        public MeasurePointData            m_current_measure_graph_item = null;

        List<StageGraphCrdPair>            m_list_stage_graph_crd_pairs = new List<StageGraphCrdPair>();     // 用于仿射变换的坐标对集合
        public List<MeasureResult>        m_list_measure_results = new List<MeasureResult>();
        public List<MeasurePointData>   m_measure_items_on_graph = new List<MeasurePointData>();
        public List<MeasurePointData>   m_measure_items_from_txt = new List<MeasurePointData>();
        public List<MeasurePointData>   m_current_task_data = new List<MeasurePointData>();
        public List<int>   m_vec_task_gridview_data_indices_for_rows = new List<int>();        // 保存任务表每一行对应的数据索引号，数目与任务表行数相同，但大于等于数据的数目
        public List<int>   m_vec_task_gridview_selected_data_indices = new List<int>();         // 保存任务表选中的数据索引号
        public List<int>   m_vec_graph_measure_item_gridview_data_indices_for_rows = new List<int>();  // 保存图纸测量项列表每一行对应的数据索引号，数目与列表行数相同，但大于等于数据的数目
        public List<int>   m_vec_graph_measure_item_gridview_selected_data_indices = new List<int>();         // 保存图纸测量项列表选中的数据索引号
        
        public enum_customer m_customer = enum_customer.none;   // 客户代号

        public bool   m_bUseSameThreeMarks = false;
        public List<ThreeMarksRecord> m_list_three_marks_records = new List<ThreeMarksRecord>(); // 珠海方正的特殊需求，同一个料号奇/偶层的三个定位孔位置相同

        static public List<ODBMeasureItem> m_ODB_measure_items = new List<ODBMeasureItem>();
        static public List<ODBMeasureItem> m_ODB_invalid_measure_items = new List<ODBMeasureItem>();

        int m_nCurrentArrayOrderIdx = -1;                                                // 当前阵列测量点批次序号
        bool        m_bCancelAndRemoveUnfinishedItems = false;                      // 撤销并移除当前批次测量点
        public bool   m_bIsMeasuringInArrayMode = false;                                // 当前是否处于阵列测量模式
        public bool   m_bApplySameParamsToOtherUnits = false;                     // 是否将本次设置的标准值和名称自动应用于阵列其它units的同一个测量点
        public MeasurePointData   m_base_data_for_other_units = new MeasurePointData();             // 储存自动应用的标准值和名称信息的测量点数据

        double    m_dbUpperDeltaPercent = 10;            // 上限百分比增量
        double    m_dbLowerDeltaPercent = 10;            // 下限百分比减量

        public bool        m_bIsAlignmentPtSet = false;
        Point2d   m_pcb_alignment_pt_on_graph = new Point2d(0, 0);
        Point2d   m_pcb_alignment_pt_on_machine = new Point2d(0, 0);
        Point2d   m_pcb_alignment_offset = new Point2d(0, 0);

        // 菜单项：打开图片文件
        private void ui_menu_OpenImageFile_Click(object sender, EventArgs e)
        {
            bool bHasDefaultDir = false;
            if (null != m_strOpenImageBrowseDir)
            {
                if ((m_strOpenImageBrowseDir.Length > 0) && (Directory.Exists(m_strOpenImageBrowseDir)))
                    bHasDefaultDir = true;
            }

            OpenFileDialog dlg = new OpenFileDialog();
            if (bHasDefaultDir)
                dlg.InitialDirectory = m_strOpenImageBrowseDir;
            else
                dlg.InitialDirectory = ".";
            dlg.Filter = "图片文件|*.bmp;*.jpg;*.jpeg;*.png";
            dlg.ShowDialog();
            if (dlg.FileName != string.Empty)
            {
                m_strOpenImageBrowseDir = System.IO.Path.GetDirectoryName(dlg.FileName);
                try
                {
                    this.ui_MainImage.Image = Image.FromFile(dlg.FileName);
                    //this.ui_MainImage.Image = m_main_image;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // 菜单项：保存主图像原图
        private void ui_menu_SaveMainImage_Click(object sender, EventArgs e)
        {
            if (null == ui_MainImage.Image)
                return;

            bool bHasDefaultDir = false;
            if (null != m_strSaveImageBrowseDir)
            {
                if ((m_strSaveImageBrowseDir.Length > 0) && (Directory.Exists(m_strSaveImageBrowseDir)))
                    bHasDefaultDir = true;
            }

            SaveFileDialog dlg = new SaveFileDialog();
            if (bHasDefaultDir)
                dlg.InitialDirectory = m_strSaveImageBrowseDir;
            else
                dlg.InitialDirectory = ".";

            dlg.Filter = "图片文件|*.bmp";
            dlg.ShowDialog();
            if (dlg.FileName != string.Empty)
            {
                m_strSaveImageBrowseDir = System.IO.Path.GetDirectoryName(dlg.FileName);

                //Debugger.Log(0, null, string.Format("222222 dlg.FileName = {0}", dlg.FileName));

                lock (m_main_cam_lock)
                {
                    ui_MainImage.Image.Save(dlg.FileName);
                }
            }
        }

        // 菜单项：保存主图像界面截图
        private void ui_menu_SaveMainImageScreenshot_Click(object sender, EventArgs e)
        {
            bool bHasDefaultDir = false;
            if (null != m_strSaveImageBrowseDir)
            {
                if ((m_strSaveImageBrowseDir.Length > 0) && (Directory.Exists(m_strSaveImageBrowseDir)))
                    bHasDefaultDir = true;
            }

            SaveFileDialog dlg = new SaveFileDialog();
            if (bHasDefaultDir)
                dlg.InitialDirectory = m_strSaveImageBrowseDir;
            else
                dlg.InitialDirectory = ".";

            dlg.Filter = "图片文件|*.bmp";
            dlg.ShowDialog();
            if (dlg.FileName != string.Empty)
            {
                m_strSaveImageBrowseDir = System.IO.Path.GetDirectoryName(dlg.FileName);

                Thread.Sleep(300);

                Image image = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Graphics g = Graphics.FromImage(image);
                g.CopyFromScreen(new Point(0, 0), new Point(0, 0), Screen.PrimaryScreen.Bounds.Size); //截取;
                //g.DrawImage(image, 50, 50, 100, 100); //在指定范围内画出截取到的图片;
                image.Save(dlg.FileName);
                g.Dispose();
            }
        }

        // 菜单项：保存导航图像原图
        private void ui_menu_SaveGuideImage_Click(object sender, EventArgs e)
        {
            if (null == ui_GuideImage.Image)
                return;

            bool bHasDefaultDir = false;
            if (null != m_strSaveImageBrowseDir)
            {
                if ((m_strSaveImageBrowseDir.Length > 0) && (Directory.Exists(m_strSaveImageBrowseDir)))
                    bHasDefaultDir = true;
            }

            SaveFileDialog dlg = new SaveFileDialog();
            if (bHasDefaultDir)
                dlg.InitialDirectory = m_strSaveImageBrowseDir;
            else
                dlg.InitialDirectory = ".";

            dlg.Filter = "图片文件|*.bmp";
            dlg.ShowDialog();
            if (dlg.FileName != string.Empty)
            {
                m_strSaveImageBrowseDir = System.IO.Path.GetDirectoryName(dlg.FileName);

                lock (m_guide_cam_lock)
                {
                    ui_GuideImage.Image.Save(dlg.FileName);
                }
            }
        }

        // 菜单项：保存导航图像界面截图
        private void ui_menu_SaveGuideImageScreenshot_Click(object sender, EventArgs e)
        {

        }

        // 菜单项：冻结相机图像
        private void ui_menu_setting_FreezeCameraImage_Click(object sender, EventArgs e)
        {
            if ("冻结相机图像" == ui_menu_setting_FreezeCameraImage.Text)
            {
                m_bFreezeCameraImage = true;
                ui_menu_setting_FreezeCameraImage.Text = "取消冻结相机图像";
            }
            else
            {
                m_bFreezeCameraImage = false;
                ui_menu_setting_FreezeCameraImage.Text = "冻结相机图像";
            }
        }

        // 菜单项：标定
        private void ui_menu_setting_Calibration_Click(object sender, EventArgs e)
        {
            Form_Calibration form = new Form_Calibration(this);
            form.ShowInTaskbar = false;
            form.Show();
        }

        // 菜单项：文件和报表
        private void ui_menu_setting_FileAndReport_Click(object sender, EventArgs e)
        {
            Form_FileAndReport form = new Form_FileAndReport(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 菜单项：运动控制和轴的设置
        private void ui_menu_setting_MotionControl_Click(object sender, EventArgs e)
        {
            Form_MotionAndAxes form = new Form_MotionAndAxes(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 菜单项：相机设置
        private void ui_menu_setting_CameraAndLight_Click(object sender, EventArgs e)
        {
            Form_CameraAndLights form = new Form_CameraAndLights(this);
           
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 菜单项：高度传感器设置和标定
        private void ui_menu_setting_HeightSensor_Click(object sender, EventArgs e)
        {
            Form_HeightSensor form = new Form_HeightSensor(this);
            form.ShowInTaskbar = false;
            form.Show();
        }

        // 菜单项：IO设置
        private void ui_menu_setting_IO_Click(object sender, EventArgs e)
        {
            Form_IO form = new Form_IO(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 菜单项：数据库设置
        private void ui_menu_setting_Database_Click(object sender, EventArgs e)
        {
            Form_Database form = new Form_Database(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 菜单项：图纸设置
        private void ui_menu_setting_Graph_Click(object sender, EventArgs e)
        {
            Form_GraphSetting form = new Form_GraphSetting(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 菜单项：用户管理
        private void ui_menu_user_management_Click(object sender, EventArgs e)
        {

            Form_UserManagment form = new Form_UserManagment(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 菜单项：杂项
        private void ui_menu_setting_Misc_Click(object sender, EventArgs e)
        {
            Form_Miscellaneous form = new Form_Miscellaneous(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 菜单项：十字线
        private void ui_menu_setting_CrossLine_Click(object sender, EventArgs e)
        {
            if ("十字线" == ui_menu_setting_CrossLine.Text)
            {
                ui_menu_setting_CrossLine.Text = "十字线    √";
                m_bShowCrossLine = true;
            }
            else if ("十字线    √" == ui_menu_setting_CrossLine.Text)
            {
                ui_menu_setting_CrossLine.Text = "十字线";
                m_bShowCrossLine = false;
            }

            ui_MainImage.Refresh();
            ui_GuideImage.Refresh();
        }

        // 菜单项：红点对位
        private void ui_menu_setting_RedDot_Click(object sender, EventArgs e)
        {
            Form_RedDot form = new Form_RedDot(this);
            form.ShowInTaskbar = false;
            form.Show();
        }

        // 菜单项：登录
        private void ui_menu_Login_Click(object sender, EventArgs e)
        {
            Form_Login form = new Form_Login(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 菜单项：操作说明
        private void ui_menu_User_Manual_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("manual"))
                MessageBox.Show(this, "找不到操作说明目录，请检查原因。", "提示");
            else
            {
                string path = System.Windows.Forms.Application.StartupPath + "\\manual\\";

                Process.Start("explorer.exe", path);
            }
        }

        // 图纸菜单项点击事件：对齐坐标
        private void menuitem_AlignCrdSystems_Click(object sender, EventArgs e)
        {
            if (null == m_graph_view.Image)
                return;
            if ((m_graph_view.Image.Width * m_graph_view.Image.Height) <= 0)
                return;
            if ((false == m_motion.m_bHomed) && (false == m_bOfflineMode))
                return;

            Point3d machine_crd = new Point3d();
            m_motion.get_xyz_crds(ref machine_crd);
            
            if (((Math.Abs(machine_crd.x - m_motion.m_PCB_leftbottom_crd.x) < 100) 
                && (Math.Abs(machine_crd.y - m_motion.m_PCB_leftbottom_crd.y) < 100))
                || (true == m_bOfflineMode))
            {
                int graph_x = 0, graph_y = 0;
                m_graph_view.get_mouse_crd_on_graph(ref graph_x, ref graph_y);
                if ((graph_x <= 0) || (graph_y <= 0))
                    return;
                if ((graph_x >= m_graph_view.m_bitmap_1bit.Width) || (graph_y >= m_graph_view.m_bitmap_1bit.Height))
                    return;
                
                m_bIsAlignmentPtSet = true;
                m_pcb_alignment_pt_on_graph.x = (double)(graph_x - m_nGraphOffsetX) / m_pixels_per_mm;
                m_pcb_alignment_pt_on_graph.y = (double)((m_nGraphHeight - GRAPH_Y_OFFSET_SUM) - (graph_y - m_nGraphOffsetY)) / m_pixels_per_mm;
                m_pcb_alignment_pt_on_machine.x = machine_crd.x;
                m_pcb_alignment_pt_on_machine.y = machine_crd.y;
                
                m_pcb_alignment_offset.x = m_pcb_alignment_pt_on_machine.x - m_motion.m_PCB_leftbottom_crd.x;
                m_pcb_alignment_offset.y = m_pcb_alignment_pt_on_machine.y - m_motion.m_PCB_leftbottom_crd.y;

                Debugger.Log(0, null, string.Format("222222 graph_x [{0},{1}], align on graph [{2:0.000},{3:0.000}], align offset [{4:0.000},{5:0.000}], m_nGraphHeight = {6}, m_nGraphOffsetY = {7}",
                    graph_x, graph_y, m_pcb_alignment_pt_on_graph.x, m_pcb_alignment_pt_on_graph.y, m_pcb_alignment_offset.x, m_pcb_alignment_offset.y, m_nGraphHeight, m_nGraphOffsetY));

                m_graph_view.m_alignment_crd_on_graph.x = graph_x;
                m_graph_view.m_alignment_crd_on_graph.y = graph_y;
                m_graph_view.refresh_image(true);
            }
            else
                MessageBox.Show(this, "当前相机位置不在机器平台左下角，无法将此处选为对齐点，请重新操作。", "提示");
        }
        
        // 图纸菜单项点击事件：添加定位孔
        private void menuitem_AddMarkCircle_Click(object sender, EventArgs e)
        {
            //Debugger.Log(0, null, string.Format("222222 添加定位孔 111"));
            m_bIsWaitingForConfirm = false;
            m_event_wait_for_confirm_during_creation.Set();
            m_event_wait_for_manual_gauge.Set();
            Thread.Sleep(50);
            
            if (true == m_bIsMeasuringDuringCreation)
                return;
            m_bIsMeasuringDuringCreation = true;
            
            if ((null == m_graph_view.Image) || ((m_graph_view.Image.Width * m_graph_view.Image.Height) <= 0))
            {
                m_bIsMeasuringDuringCreation = false;
                return;
            }
            
            if (false == m_bOfflineMode)
            {
                if (false == m_bIsAlignmentPtSet)
                {
                    MessageBox.Show(this, "请先对齐图纸和实物板坐标系，再添加定位孔。", "提示");
                    m_bIsMeasuringDuringCreation = false;
                    return;
                }
            }
            
            if (get_fiducial_mark_count(m_current_task_data) >= 3)
            {
                MessageBox.Show(this, "当前已有3个定位孔，如需修改，请先删除已有的定位孔。", "提示");
                m_bIsMeasuringDuringCreation = false;
                return;
            }

            int graph_x = 0, graph_y = 0;
            m_graph_view.get_mouse_crd_on_graph(ref graph_x, ref graph_y);

            MeasurePointData data = new MeasurePointData();
            data.m_strStepsFileName = m_strCurrentProductStep;
            data.m_strLayerFileName = m_strCurrentProductLayer;
            if (false == add_measure_point(MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE, graph_x, graph_y, ref data, false, false, true))
            {
                m_bIsMeasuringDuringCreation = false;
                return;
            }

            if ((0 == get_fiducial_mark_count(m_current_task_data)) && (1 == m_nAlgorithm))
            {
                if (DialogResult.No == MessageBox.Show(this,
                    "您选择了陶瓷板、白底板模式，请确认是否继续？\r\n如果是普通板，请点否，取消勾选陶瓷板、白底板选项，再继续作业。", "提示", MessageBoxButtons.YesNo))
                    return;
            }

            //Debugger.Log(0, null, string.Format("222222 m_zoom_ratio = {0}, width = {1}", 
            //    m_graph_view.m_zoom_ratio, ));

            if (false)
            {
                double x = 0, y = 0;
                double zoom_ratio = m_graph_view.m_zoom_ratio_min * 7;
                double current_view_width = (double)m_graph_view.m_bitmap_32bits.Width / zoom_ratio;
                double current_view_height = (double)m_graph_view.m_bitmap_32bits.Height / zoom_ratio;
                switch (get_fiducial_mark_count(m_measure_items_on_graph))
                {
                    case 0:
                        if (true == m_graph_view.find_corner_pos(1, ref x, ref y))
                        {
                            double left = x - current_view_width * 0.33;
                            double top = y - current_view_height * 0.33;
                            double x2 = left > 0 ? left : 0;
                            double y2 = top > 0 ? top : 0;
                            m_graph_view.set_view_ratio_and_crd(zoom_ratio, x2, y2);
                        }
                        break;
                    case 1:
                        if (true == m_graph_view.find_corner_pos(2, ref x, ref y))
                        {
                            double left = x - current_view_width * 0.66;
                            double top = y - current_view_height * 0.66;
                            double x2 = left > 0 ? left : 0;
                            double y2 = top > 0 ? top : 0;
                            m_graph_view.set_view_ratio_and_crd(zoom_ratio, x2, y2);
                        }
                        break;
                }
            }

            if (false == m_bOfflineMode)
            {
                if (get_fiducial_mark_count(m_current_task_data) < get_fiducial_mark_count(m_measure_items_on_graph))
                {
                    m_measure_items_on_graph.RemoveAt(get_fiducial_mark_count(m_measure_items_on_graph) - 1);
                }
            }

            tabControl_Task.SelectedIndex = 1;
            data.m_len_ratio = 0;
            if (false == m_bOfflineMode)
                data.m_ID = get_fiducial_mark_count(m_current_task_data) + 1;
            m_measure_items_on_graph.Insert(data.m_ID - 1, data);

            m_event_wait_for_confirm_during_creation.Set();

            MeasurePointData new_data = data.cloneClass();
            data.m_strStepsFileName = m_strCurrentProductStep;
            data.m_strLayerFileName = m_strCurrentProductLayer;
            m_current_measure_graph_item = new_data;
            
            new Thread(thread_locate_and_measure_mark_pt).Start(new_data);
        }

        // 图纸菜单项点击事件：测量下线宽
        private void menuitem_AddBottomLine_Click(object sender, EventArgs e)
        {
            m_bIsWaitingForConfirm = false;
            m_event_wait_for_confirm_during_creation.Set();
            m_event_wait_for_manual_gauge.Set();
            Thread.Sleep(50);
            
            if (true == m_bIsMeasuringDuringCreation)
                return;
            m_bIsMeasuringDuringCreation = true;

            int graph_x = 0, graph_y = 0;
            m_graph_view.get_mouse_crd_on_graph(ref graph_x, ref graph_y);

            MeasurePointData data = new MeasurePointData();
            if (false == add_measure_point(MEASURE_TYPE.LINE_WIDTH_14, graph_x, graph_y, ref data))
            {
                m_bIsMeasuringDuringCreation = false;
                return;
            }

            tabControl_Task.SelectedIndex = 1;
            m_event_wait_for_manual_gauge.Set();

            MeasurePointData new_data = data.cloneClass();
            m_current_measure_graph_item = new_data;

            if (get_array_rects_count(GraphView.m_ODB_thumbnail_array_rects) > 1)
                new Thread(thread_locate_and_measure_item_in_array_mode).Start(new_data);
            else
            {
                m_measure_items_on_graph.Add(data);
                
                dl_message_sender send_message = CBD_SendMessage;
                send_message("刷新图纸测量项列表", false, true, null);
                new Thread(thread_locate_and_measure_item).Start(new_data);
            }
        }

        // 图纸菜单项点击事件：测量上线宽
        private void menuitem_AddTopLine_Click(object sender, EventArgs e)
        {
            m_bIsWaitingForConfirm = false;
            m_event_wait_for_confirm_during_creation.Set();
            m_event_wait_for_manual_gauge.Set();
            Thread.Sleep(50);

            if (true == m_bIsMeasuringDuringCreation)
                return;
            m_bIsMeasuringDuringCreation = true;

            int graph_x = 0, graph_y = 0;
            m_graph_view.get_mouse_crd_on_graph(ref graph_x, ref graph_y);

            MeasurePointData data = new MeasurePointData();
            if (false == add_measure_point(MEASURE_TYPE.LINE_WIDTH_23, graph_x, graph_y, ref data))
            {
                m_bIsMeasuringDuringCreation = false;
                return;
            }

            tabControl_Task.SelectedIndex = 1;
            m_event_wait_for_manual_gauge.Set();

            MeasurePointData new_data = data.cloneClass();
            m_current_measure_graph_item = new_data;

            if (get_array_rects_count(GraphView.m_ODB_thumbnail_array_rects) > 1)
                new Thread(thread_locate_and_measure_item_in_array_mode).Start(new_data);
            else
            {
                m_measure_items_on_graph.Add(data);

                dl_message_sender send_message = CBD_SendMessage;
                send_message("刷新图纸测量项列表", false, true, null);
                new Thread(thread_locate_and_measure_item).Start(new_data);
            }
        }

        // 图纸菜单项点击事件：测量上下线宽
        private void menuitem_AddTopBottomLine_Click(object sender, EventArgs e)
        {
            m_bIsWaitingForConfirm = false;
            m_event_wait_for_confirm_during_creation.Set();
            m_event_wait_for_manual_gauge.Set();
            Thread.Sleep(50);

            if (true == m_bIsMeasuringDuringCreation)
                return;
            m_bIsMeasuringDuringCreation = true;

            int graph_x = 0, graph_y = 0;
            m_graph_view.get_mouse_crd_on_graph(ref graph_x, ref graph_y);

            MeasurePointData data = new MeasurePointData();
            if (false == add_measure_point(MEASURE_TYPE.LINE_WIDTH_1234, graph_x, graph_y, ref data))
            {
                m_bIsMeasuringDuringCreation = false;
                return;
            }

            tabControl_Task.SelectedIndex = 1;
            m_event_wait_for_manual_gauge.Set();

            MeasurePointData new_data = data.cloneClass();
            m_current_measure_graph_item = new_data;

            if (get_array_rects_count(GraphView.m_ODB_thumbnail_array_rects) > 1)
                new Thread(thread_locate_and_measure_item_in_array_mode).Start(new_data);
            else
            {
                m_measure_items_on_graph.Add(data);

                dl_message_sender send_message = CBD_SendMessage;
                send_message("刷新图纸测量项列表", false, true, null);
                new Thread(thread_locate_and_measure_item).Start(new_data);
            }
        }

        // 图纸菜单项点击事件：测量13线宽
        private void menuitem_AddT13Line_Click(object sender, EventArgs e)
        {
            m_bIsWaitingForConfirm = false;
            m_event_wait_for_confirm_during_creation.Set();
            m_event_wait_for_manual_gauge.Set();
            Thread.Sleep(50);

            if (true == m_bIsMeasuringDuringCreation)
                return;
            m_bIsMeasuringDuringCreation = true;

            int graph_x = 0, graph_y = 0;
            m_graph_view.get_mouse_crd_on_graph(ref graph_x, ref graph_y);

            MeasurePointData data = new MeasurePointData();
            if (false == add_measure_point(MEASURE_TYPE.LINE_WIDTH_13, graph_x, graph_y, ref data))
            {
                m_bIsMeasuringDuringCreation = false;
                return;
            }

            tabControl_Task.SelectedIndex = 1;
            m_event_wait_for_manual_gauge.Set();

            MeasurePointData new_data = data.cloneClass();
            m_current_measure_graph_item = new_data;

            if (get_array_rects_count(GraphView.m_ODB_thumbnail_array_rects) > 1)
                new Thread(thread_locate_and_measure_item_in_array_mode).Start(new_data);
            else
            {
                m_measure_items_on_graph.Add(data);

                dl_message_sender send_message = CBD_SendMessage;
                send_message("刷新图纸测量项列表", false, true, null);
                new Thread(thread_locate_and_measure_item).Start(new_data);
            }
        }

        // 图纸菜单项点击事件：测量弧形线宽
        private void menuitem_AddArcLine_Click(object sender, EventArgs e)
        {
            m_bIsWaitingForConfirm = false;
            m_event_wait_for_confirm_during_creation.Set();
            m_event_wait_for_manual_gauge.Set();
            Thread.Sleep(50);

            if (true == m_bIsMeasuringDuringCreation)
                return;
            m_bIsMeasuringDuringCreation = true;

            int graph_x = 0, graph_y = 0;
            m_graph_view.get_mouse_crd_on_graph(ref graph_x, ref graph_y);

            MeasurePointData data = new MeasurePointData();
            if (false == add_measure_point(MEASURE_TYPE.ARC_LINE_WIDTH, graph_x, graph_y, ref data))
            {
                m_bIsMeasuringDuringCreation = false;
                return;
            }

            tabControl_Task.SelectedIndex = 1;
            m_event_wait_for_manual_gauge.Set();

            MeasurePointData new_data = data.cloneClass();
            m_current_measure_graph_item = new_data;

            if (get_array_rects_count(GraphView.m_ODB_thumbnail_array_rects) > 1)
                new Thread(thread_locate_and_measure_item_in_array_mode).Start(new_data);
            else
            {
                m_measure_items_on_graph.Add(data);

                dl_message_sender send_message = CBD_SendMessage;
                send_message("刷新图纸测量项列表", false, true, null);
                new Thread(thread_locate_and_measure_item).Start(new_data);
            }
        }

        // 图纸菜单项点击事件：测量BGA
        private void menuitem_AddBGA_Click(object sender, EventArgs e)
        {
            m_bIsWaitingForConfirm = false;
            m_event_wait_for_confirm_during_creation.Set();
            m_event_wait_for_manual_gauge.Set();
            Thread.Sleep(50);

            if (true == m_bIsMeasuringDuringCreation)
                return;
            m_bIsMeasuringDuringCreation = true;

            int graph_x = 0, graph_y = 0;
            m_graph_view.get_mouse_crd_on_graph(ref graph_x, ref graph_y);

            MeasurePointData data = new MeasurePointData();
            if (false == add_measure_point(MEASURE_TYPE.CIRCLE_OUTER_TO_INNER, graph_x, graph_y, ref data))
            {
                m_bIsMeasuringDuringCreation = false;
                return;
            }

            //data.m_len_ratio = 0;
            tabControl_Task.SelectedIndex = 1;
            m_event_wait_for_manual_gauge.Set();

            MeasurePointData new_data = data.cloneClass();
            m_current_measure_graph_item = new_data;

            if (get_array_rects_count(GraphView.m_ODB_thumbnail_array_rects) > 1)
                new Thread(thread_locate_and_measure_item_in_array_mode).Start(new_data);
            else
            {
                m_measure_items_on_graph.Add(data);

                dl_message_sender send_message = CBD_SendMessage;
                send_message("刷新图纸测量项列表", false, true, null);
                new Thread(thread_locate_and_measure_item).Start(new_data);
            }
        }

        // 图纸菜单项点击事件：测量线距
        private void menuitem_AddLineSpace_Click(object sender, EventArgs e)
        {
            m_bIsWaitingForConfirm = false;
            m_event_wait_for_confirm_during_creation.Set();
            m_event_wait_for_manual_gauge.Set();
            Thread.Sleep(50);

            if (true == m_bIsMeasuringDuringCreation)
                return;
            m_bIsMeasuringDuringCreation = true;

            int graph_x = 0, graph_y = 0;
            m_graph_view.get_mouse_crd_on_graph(ref graph_x, ref graph_y);

            MeasurePointData data = new MeasurePointData();
            if (false == add_measure_point(MEASURE_TYPE.LINE_SPACE, graph_x, graph_y, ref data))
            {
                m_bIsMeasuringDuringCreation = false;
                return;
            }

            tabControl_Task.SelectedIndex = 1;
            m_event_wait_for_manual_gauge.Set();

            MeasurePointData new_data = data.cloneClass();
            m_current_measure_graph_item = new_data;

            if (get_array_rects_count(GraphView.m_ODB_thumbnail_array_rects) > 1)
                new Thread(thread_locate_and_measure_item_in_array_mode).Start(new_data);
            else
            {
                m_measure_items_on_graph.Add(data);

                dl_message_sender send_message = CBD_SendMessage;
                send_message("刷新图纸测量项列表", false, true, null);
                new Thread(thread_locate_and_measure_item).Start(new_data);
            }
        }

        // 图纸菜单项点击事件：测量弧形线距
        private void menuitem_AddArcLineSpace_Click(object sender, EventArgs e)
        {
            m_bIsWaitingForConfirm = false;
            m_event_wait_for_confirm_during_creation.Set();
            m_event_wait_for_manual_gauge.Set();
            Thread.Sleep(50);

            if (true == m_bIsMeasuringDuringCreation)
                return;
            m_bIsMeasuringDuringCreation = true;

            int graph_x = 0, graph_y = 0;
            m_graph_view.get_mouse_crd_on_graph(ref graph_x, ref graph_y);

            MeasurePointData data = new MeasurePointData();
            if (false == add_measure_point(MEASURE_TYPE.ARC_LINE_SPACE, graph_x, graph_y, ref data))
            {
                m_bIsMeasuringDuringCreation = false;
                return;
            }

            tabControl_Task.SelectedIndex = 1;
            m_event_wait_for_manual_gauge.Set();

            MeasurePointData new_data = data.cloneClass();
            m_current_measure_graph_item = new_data;

            if (get_array_rects_count(GraphView.m_ODB_thumbnail_array_rects) > 1)
                new Thread(thread_locate_and_measure_item_in_array_mode).Start(new_data);
            else
            {
                m_measure_items_on_graph.Add(data);

                dl_message_sender send_message = CBD_SendMessage;
                send_message("刷新图纸测量项列表", false, true, null);
                new Thread(thread_locate_and_measure_item).Start(new_data);
            }
        }

        // 图纸菜单项点击事件：阵列操作----全选
        private void menuitem_SelectAllArrayUnits_Click(object sender, EventArgs e)
        {
            for (int n = 0; n < GraphView.m_ODB_thumbnail_array_rects.Count; n++)
            {
                List<rotated_array_rect> vec_rects = GraphView.m_ODB_thumbnail_array_rects[n];
                for (int m = 0; m < vec_rects.Count; m++)
                {
                    vec_rects[m].m_nSelectOrder = -1;
                    vec_rects[m].bSelected = true;
                }
            }
            rotated_array_rect.m_nSelectCount = get_array_rects_count(GraphView.m_ODB_thumbnail_array_rects);

            m_graph_view.refresh_image(true);
        }

        // 图纸菜单项点击事件：阵列操作----全部不选
        private void menuitem_UnselectAllArrayUnits_Click(object sender, EventArgs e)
        {
            for (int n = 0; n < GraphView.m_ODB_thumbnail_array_rects.Count; n++)
            {
                List<rotated_array_rect> vec_rects = GraphView.m_ODB_thumbnail_array_rects[n];
                for (int m = 0; m < vec_rects.Count; m++)
                    vec_rects[m].bSelected = false;
            }
            rotated_array_rect.m_nSelectCount = 0;

            m_graph_view.refresh_image(true);
        }

        // 图纸菜单项点击事件：阵列操作----撤销并移除当前批次测量点
        private void menuitem_CancelAndRemoveUnfinishedItems_Click(object sender, EventArgs e)
        {
            m_bCancelAndRemoveUnfinishedItems = true;

            m_event_wait_for_manual_gauge.Set();
            m_event_wait_for_confirm_during_creation.Set();
        }

        // 图纸菜单项点击事件：阵列操作----选定左上角点
        private void menuitem_SetUnitLeftTopPos_Click(object sender, EventArgs e)
        {
            if (false == m_graph_view.pre_check_before_array_recognition())
                return;
            
            m_graph_view.add_array_unit_lefttop_pos();
            m_graph_view.m_bDrawArrayRects = true;
            m_graph_view.refresh_image(true);

            if (m_graph_view.m_list_array_unit_anchor_pts.Count >= 2)
            {
                if (true == m_graph_view.run_array_recognition())
                {
                    m_graph_view.refresh_image(true);
                }
            }
        }

        // 图纸菜单项点击事件：阵列操作----选定右下角点
        private void menuitem_SetUnitRightBotPos_Click(object sender, EventArgs e)
        {
            if (false == m_graph_view.pre_check_before_array_recognition())
                return;

            m_graph_view.add_array_unit_rightbottom_pos();
            m_graph_view.m_bDrawArrayRects = true;
            m_graph_view.refresh_image(true);

            if (m_graph_view.m_list_array_unit_anchor_pts.Count >= 2)
            {
                if (true == m_graph_view.run_array_recognition())
                {
                    m_graph_view.refresh_image(true);
                }
            }
        }

        // 图纸菜单项点击事件：手拉框----下线宽
        private void menuitem_AddBottomLineByHand_Click(object sender, EventArgs e)
        {
            m_graph_view.m_bDrawLineFrameByHand = true;
            m_graph_view.m_nTypeOfDrawnLine = MEASURE_TYPE.LINE_WIDTH_14;

            m_graph_view.m_start_pt_for_drawn_line = new Point2d(0, 0);
            m_graph_view.m_end_pt_for_drawn_line = new Point2d(0, 0);
        }

        // 图纸菜单项点击事件：将平台移动到此处
        private void menuitem_MoveCameraToHere_Click(object sender, EventArgs e)
        {
            if (false == m_bOfflineMode)
            {
                if (m_bIsCreatingTask && (0 == m_nCreateTaskMode) && (get_fiducial_mark_count(m_current_task_data) >= 3))
                {
                    int graph_x = 0, graph_y = 0;
                    m_graph_view.get_mouse_crd_on_graph(ref graph_x, ref graph_y);
                    if ((graph_x <= 0) || (graph_y <= 0)) return;
                    if ((graph_x >= m_graph_view.m_bitmap_1bit.Width) || (graph_y >= m_graph_view.m_bitmap_1bit.Height)) return;

                    // 将图形坐标换算成物理坐标
                    double center_x_in_metric = (graph_x - (double)m_nGraphOffsetX) / m_pixels_per_mm;
                    double center_y_in_metric = ((double)(m_nGraphHeight - GRAPH_Y_OFFSET_SUM) - (graph_y - (double)m_nGraphOffsetY)) / m_pixels_per_mm;

                    Point3d target_crd = new Point3d();
                    m_motion.get_xyz_crds(ref target_crd);

                    target_crd.x = m_triangle_trans_matrix[0] * center_x_in_metric + m_triangle_trans_matrix[1] * center_y_in_metric + m_triangle_trans_matrix[2];
                    target_crd.y = m_triangle_trans_matrix[3] * center_x_in_metric + m_triangle_trans_matrix[4] * center_y_in_metric + m_triangle_trans_matrix[5];
                    
                    target_crd.x += m_len_ratios_offsets[comboBox_Len.SelectedIndex].x;
                    target_crd.y += m_len_ratios_offsets[comboBox_Len.SelectedIndex].y;

                    m_motion.m_threaded_move_dest_X = target_crd.x;
                    m_motion.m_threaded_move_dest_Y = target_crd.y;
                    m_motion.m_threaded_move_dest_Z = target_crd.z;

                    dl_message_sender messenger = CBD_SendMessage;
                    (new Thread(m_motion.threaded_linear_XYZ_wait_until_stop)).Start(messenger);
                }
            }
        }

        // 图纸菜单项点击事件：载入离线文件
        private void menuitem_LoadOfflineFile_Click(object sender, EventArgs e)
        {
            bool bHasDefaultDir = false;
            if ("" != m_strOfflineFileBrowseDir)
            {
                if ((m_strOfflineFileBrowseDir.Length > 0) && (Directory.Exists(m_strOfflineFileBrowseDir)))
                    bHasDefaultDir = true;
            }

            OpenFileDialog dlg = new OpenFileDialog();
            if (bHasDefaultDir)
                dlg.InitialDirectory = m_strOfflineFileBrowseDir;
            else
                dlg.InitialDirectory = ".";
            dlg.Filter = "离线文件|*.dat";
            dlg.ShowDialog();
            if (dlg.FileName != string.Empty)
            {
                m_strOfflineFileBrowseDir = System.IO.Path.GetDirectoryName(dlg.FileName);

                m_measure_items_on_graph.Clear();
                m_measure_items_from_txt.Clear();

                read_task_from_file("", m_measure_items_on_graph, dlg.FileName, true);

                show_task_on_gridview(gridview_GraphMeasureItems, m_measure_items_on_graph);
                this.tabControl_Task.SelectedIndex = 1;

                bool bHasGraph = true;
                if (null == MainUI.m_graph_view.Image)
                    bHasGraph = false;
                else
                {
                    if ((MainUI.m_graph_view.Image.Width * MainUI.m_graph_view.Image.Height) <= 0)
                        bHasGraph = false;
                }
                if (MainUI.m_graph_view.m_zoom_ratio < 0.000001)
                    bHasGraph = false;

                if (true == bHasGraph)
                {
                    double ratio = m_graph_view.m_zoom_ratio_min * 25;
                    double center_x = m_measure_items_on_graph[0].m_center_x_on_graph - (m_graph_view.Width / 2) / ratio;
                    double center_y = m_measure_items_on_graph[0].m_center_y_on_graph - (m_graph_view.Height / 2) / ratio;

                    m_graph_view.set_view_ratio_and_crd(ratio, center_x, center_y);

                    m_graph_view.refresh_image();
                }
            }
        }

        // 图纸菜单项点击事件：基于图纸测量项生成离线文件
        private void menuitem_SaveAsOfflineFile_Click(object sender, EventArgs e)
        {
            bool bHasDefaultDir = false;
            if ("" != m_strOfflineFileBrowseDir)
            {
                if ((m_strOfflineFileBrowseDir.Length > 0) && (Directory.Exists(m_strOfflineFileBrowseDir)))
                    bHasDefaultDir = true;
            }
            else
            {
                if (!Directory.Exists("离线文件"))
                    Directory.CreateDirectory("离线文件");
            }
            
            SaveFileDialog dlg = new SaveFileDialog();
            if (bHasDefaultDir)
                dlg.InitialDirectory = m_strOfflineFileBrowseDir;
            else
                dlg.InitialDirectory = System.Environment.CurrentDirectory + "\\离线文件";
            
            dlg.Filter = "离线文件|*.dat";
            dlg.ShowDialog();
            if (dlg.FileName != string.Empty)
            {
                m_strOfflineFileBrowseDir = System.IO.Path.GetDirectoryName(dlg.FileName);

                if (true == save_task_to_file(m_measure_items_on_graph, "", dlg.FileName, true))
                    MessageBox.Show(this, "保存成功", "提示");
                else
                    MessageBox.Show(this, "保存失败！请查看日志分析原因。", "提示");
            }
        }

        // 测量任务列表右键菜单项点击事件：修改
        private void menuitem_ModifyTask_Click(object sender, EventArgs e)
        {
            if (gridview_MeasureTask.SelectedRows.Count <= 0)
                return;
            if (gridview_MeasureTask.Rows.Count <= 1)
                return;
            if ("" == gridview_MeasureTask[0, 0].Value.ToString())
                return;
            
            m_vec_task_gridview_data_indices_for_rows.Clear();
            m_vec_task_gridview_selected_data_indices.Clear();

            // 检查是否有选中最后一行（最后一行是空白行，仅起排版作用）
            for (int n = 0; n < gridview_MeasureTask.SelectedRows.Count; n++)
            {
                if ((gridview_MeasureTask.Rows.Count - 1) == gridview_MeasureTask.SelectedRows[n].Index)
                {
                    MessageBox.Show(this, "最后一行是空白行，请不要选中。", "提示");
                    return;
                }
            }
            
            // 保存任务表每一行对应的数据索引号，数目等于任务表行数减去一（因为任务表最后一行内容为空），但大于等于数据的数目
            int prev_index = -1;
            for (int n = 0; n < gridview_MeasureTask.Rows.Count - 1; n++)
            {
                //Debugger.Log(0, null, string.Format("222222 aaa n = {0}, prev_index = {1}", n, prev_index));
                if ("" == gridview_MeasureTask[0, 0].Value.ToString())
                {
                    if (-1 == prev_index)
                        return;
                    else
                        m_vec_task_gridview_data_indices_for_rows.Add(prev_index);
                }
                //Debugger.Log(0, null, string.Format("222222 bbb"));
                int index = 0;
                if ("" == gridview_MeasureTask[0, n].Value.ToString())
                    index = prev_index;
                else
                    index = Convert.ToInt32(gridview_MeasureTask[0, n].Value.ToString()) - 1;

                //Debugger.Log(0, null, string.Format("222222 bbb index = {0}", index));
                if ((index < 0) || (index >= m_current_task_data.Count))
                {
                    MessageBox.Show(this, "任务表数据序号超出范围，请检查原因！", "提示");
                    return;
                }

                m_vec_task_gridview_data_indices_for_rows.Add(index);
                prev_index = index;
            }
            
            // 保存任务表选中的数据索引号
            for (int n = 0; n < gridview_MeasureTask.SelectedRows.Count; n++)
            {
                int index = gridview_MeasureTask.SelectedRows[n].Index;
                if (0 == m_vec_task_gridview_selected_data_indices.Count)
                    m_vec_task_gridview_selected_data_indices.Add(m_vec_task_gridview_data_indices_for_rows[index]);
                else
                {
                    bool bRepeated = false;
                    for (int k = 0; k < m_vec_task_gridview_selected_data_indices.Count; k++)
                    {
                        if (m_vec_task_gridview_data_indices_for_rows[index] == m_vec_task_gridview_selected_data_indices[k])
                        {
                            bRepeated = true;
                            break;
                        }
                    }
                    if (false == bRepeated)
                        m_vec_task_gridview_selected_data_indices.Add(m_vec_task_gridview_data_indices_for_rows[index]);
                }
            }
            
            // 检查选中行是否都属于同一个测量类型
            MEASURE_TYPE type = m_current_task_data[m_vec_task_gridview_selected_data_indices[0]].m_mes_type;
            for (int n = 0; n < m_vec_task_gridview_selected_data_indices.Count; n++)
            {
                if (m_current_task_data[m_vec_task_gridview_selected_data_indices[n]].m_mes_type != type)
                {
                    MessageBox.Show(this, "所选数据不全是同一种测量类型，无法统一修改！\r请选择同类数据进行修改。", "提示");
                    return;
                }
            }
            
            // 排序
            m_vec_task_gridview_selected_data_indices.Sort();

            if (false)
            {
                for (int n = 0; n < m_vec_task_gridview_selected_data_indices.Count; n++)
                    Debugger.Log(0, null, string.Format("222222 n= {0}, indices = {1}", n, m_vec_task_gridview_selected_data_indices[n]));
            }
            
            Form_ModifyTask form = new Form_ModifyTask(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 测量任务列表右键菜单项点击事件：移除
        private void menuitem_RemoveTaskItem_Click(object sender, EventArgs e)
        {
            if (gridview_MeasureTask.SelectedRows.Count <= 0)
                return;
            if (gridview_MeasureTask.Rows.Count <= 1)
                return;
            if ("" == gridview_MeasureTask[0, 0].Value.ToString())
                return;

            m_vec_task_gridview_data_indices_for_rows.Clear();
            m_vec_task_gridview_selected_data_indices.Clear();

            // 检查是否有选中最后一行（最后一行是空白行，仅起排版作用）
            for (int n = 0; n < gridview_MeasureTask.SelectedRows.Count; n++)
            {
                if ((gridview_MeasureTask.Rows.Count - 1) == gridview_MeasureTask.SelectedRows[n].Index)
                {
                    MessageBox.Show(this, "最后一行是空白行，请不要选中。", "提示");
                    return;
                }
            }

            // 保存任务表每一行对应的数据索引号，数目等于任务表行数减去一（因为任务表最后一行内容为空），但大于等于数据的数目
            int prev_index = -1;
            for (int n = 0; n < gridview_MeasureTask.Rows.Count - 1; n++)
            {
                //Debugger.Log(0, null, string.Format("222222 aaa n = {0}, prev_index = {1}", n, prev_index));
                if ("" == gridview_MeasureTask[0, 0].Value.ToString())
                {
                    if (-1 == prev_index)
                        return;
                    else
                        m_vec_task_gridview_data_indices_for_rows.Add(prev_index);
                }
                //Debugger.Log(0, null, string.Format("222222 bbb"));
                int index = 0;
                if ("" == gridview_MeasureTask[0, n].Value.ToString())
                    index = prev_index;
                else
                    index = Convert.ToInt32(gridview_MeasureTask[0, n].Value.ToString()) - 1;

                //Debugger.Log(0, null, string.Format("222222 bbb index = {0}", index));
                if ((index < 0) || (index >= m_current_task_data.Count))
                {
                    MessageBox.Show(this, "任务表数据序号超出范围，请检查原因！", "提示");
                    return;
                }

                m_vec_task_gridview_data_indices_for_rows.Add(index);
                prev_index = index;
            }

            // 保存任务表选中的数据索引号
            for (int n = 0; n < gridview_MeasureTask.SelectedRows.Count; n++)
            {
                int index = gridview_MeasureTask.SelectedRows[n].Index;
                if (0 == m_vec_task_gridview_selected_data_indices.Count)
                    m_vec_task_gridview_selected_data_indices.Add(m_vec_task_gridview_data_indices_for_rows[index]);
                else
                {
                    bool bRepeated = false;
                    for (int k = 0; k < m_vec_task_gridview_selected_data_indices.Count; k++)
                    {
                        if (m_vec_task_gridview_data_indices_for_rows[index] == m_vec_task_gridview_selected_data_indices[k])
                        {
                            bRepeated = true;
                            break;
                        }
                    }
                    if (false == bRepeated)
                        m_vec_task_gridview_selected_data_indices.Add(m_vec_task_gridview_data_indices_for_rows[index]);
                }
            }
            
            // 排序
            m_vec_task_gridview_selected_data_indices.Sort();

            List<MeasurePointData> temp_list = new List<MeasurePointData>();
            for (int n = 0; n < m_current_task_data.Count; n++)
            {
                bool bOnList = false;

                for (int k = 0; k < m_vec_task_gridview_selected_data_indices.Count; k++)
                {
                    if (n == m_vec_task_gridview_selected_data_indices[k])
                    {
                        bOnList = true;
                        break;
                    }
                }

                if (false == bOnList)
                    temp_list.Add(m_current_task_data[n]);
            }
            m_current_task_data.Clear();
            m_current_task_data = temp_list;

            for (int n = 0; n < m_current_task_data.Count; n++)
                m_current_task_data[n].m_ID = n + 1;

            show_task_on_gridview(gridview_MeasureTask, m_current_task_data);
        }

        // 测量任务列表右键菜单项点击事件：构造组合测量---线到线
        private void menuitem_ComboMeasure_LineToLine_Click(object sender, EventArgs e)
        {
            if (gridview_MeasureTask.SelectedRows.Count <= 1)
                return;
            if (gridview_MeasureTask.Rows.Count <= 2)
                return;
            if ("" == gridview_MeasureTask[0, 0].Value.ToString())
                return;

            #region
            if (true)
            {
                m_vec_task_gridview_data_indices_for_rows.Clear();
                m_vec_task_gridview_selected_data_indices.Clear();

                // 检查是否有选中最后一行（最后一行是空白行，仅起排版作用）
                for (int n = 0; n < gridview_MeasureTask.SelectedRows.Count; n++)
                {
                    if ((gridview_MeasureTask.Rows.Count - 1) == gridview_MeasureTask.SelectedRows[n].Index)
                    {
                        MessageBox.Show(this, "最后一行是空白行，请不要选中。", "提示");
                        return;
                    }
                }

                // 保存任务表每一行对应的数据索引号，数目等于任务表行数减去一（因为任务表最后一行内容为空），但大于等于数据的数目
                int prev_index = -1;
                for (int n = 0; n < gridview_MeasureTask.Rows.Count - 1; n++)
                {
                    //Debugger.Log(0, null, string.Format("222222 aaa n = {0}, prev_index = {1}", n, prev_index));
                    if ("" == gridview_MeasureTask[0, 0].Value.ToString())
                    {
                        if (-1 == prev_index)
                            return;
                        else
                            m_vec_task_gridview_data_indices_for_rows.Add(prev_index);
                    }
                    //Debugger.Log(0, null, string.Format("222222 bbb"));
                    int index = 0;
                    if ("" == gridview_MeasureTask[0, n].Value.ToString())
                        index = prev_index;
                    else
                        index = Convert.ToInt32(gridview_MeasureTask[0, n].Value.ToString()) - 1;

                    //Debugger.Log(0, null, string.Format("222222 bbb index = {0}", index));
                    if ((index < 0) || (index >= m_current_task_data.Count))
                    {
                        MessageBox.Show(this, "任务表数据序号超出范围，请检查原因！", "提示");
                        return;
                    }

                    m_vec_task_gridview_data_indices_for_rows.Add(index);
                    prev_index = index;
                }

                // 保存任务表选中的数据索引号
                for (int n = 0; n < gridview_MeasureTask.SelectedRows.Count; n++)
                {
                    int index = gridview_MeasureTask.SelectedRows[n].Index;
                    if (0 == m_vec_task_gridview_selected_data_indices.Count)
                        m_vec_task_gridview_selected_data_indices.Add(m_vec_task_gridview_data_indices_for_rows[index]);
                    else
                    {
                        bool bRepeated = false;
                        for (int k = 0; k < m_vec_task_gridview_selected_data_indices.Count; k++)
                        {
                            if (m_vec_task_gridview_data_indices_for_rows[index] == m_vec_task_gridview_selected_data_indices[k])
                            {
                                bRepeated = true;
                                break;
                            }
                        }
                        if (false == bRepeated)
                            m_vec_task_gridview_selected_data_indices.Add(m_vec_task_gridview_data_indices_for_rows[index]);
                    }
                }

                // 排序
                m_vec_task_gridview_selected_data_indices.Sort();
            }
            #endregion

            if (2 == m_vec_task_gridview_selected_data_indices.Count)
            {
                for (int n = 0; n < 2; n++)
                {
                    if (MEASURE_TYPE.LINE != m_current_task_data[n].m_mes_type)
                    {
                        MessageBox.Show(this, "线到线组合测量要求两个测量项的类型都是\"单线寻边\"。", "提示");
                        return;
                    }
                }

                for (int n = 0; n < 2; n++)
                    m_current_task_data[n].m_bIsPartOfComboMeasure = true;

                Gauger_ComboLineToLine gauger = new Gauger_ComboLineToLine(this, ui_MainImage, MEASURE_TYPE.COMBO_LINE_TO_LINE);

                Debugger.Log(0, null, string.Format("222222 m_vec_task_gridview_selected_data_indices = {0}, {1}",
                    m_vec_task_gridview_selected_data_indices[0], m_vec_task_gridview_selected_data_indices[1]));
            }
        }
        
        // 测量任务列表鼠标单击事件
        private void gridview_MeasureTask_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (gridview_MeasureTask.SelectedRows.Count <= 0)
                return;
            if (gridview_MeasureTask.Rows.Count <= 1)
                return;
            if ("" == gridview_MeasureTask[0, 0].Value.ToString())
                return;

            // 显示在状态栏的展开项
            if (gridview_MeasureTask.SelectedRows.Count > 0)
            {
                string info = "展开：" + gridview_MeasureTask.SelectedRows[0].Cells[0].Value;
                info += ", " + gridview_MeasureTask.SelectedRows[0].Cells[1].Value;
                info += ", " + gridview_MeasureTask.SelectedRows[0].Cells[2].Value;
                info += ", 标准值 " + gridview_MeasureTask.SelectedRows[0].Cells[3].Value + gridview_MeasureTask.SelectedRows[0].Cells[6].Value;
                info += ", " + gridview_MeasureTask.SelectedRows[0].Cells[4].Value;
                info += ", " + gridview_MeasureTask.SelectedRows[0].Cells[5].Value;
                info += ", " + gridview_MeasureTask.SelectedRows[0].Cells[7].Value;

                this.toolStripStatusLabel_GridViewExpandInfo.Text = info;
            }

            if (null == m_graph_view.Image) return;
            if (m_graph_view.m_zoom_ratio < 0.000001) return;
            if ((m_graph_view.Image.Width * m_graph_view.Image.Height) <= 0) return;

            m_vec_task_gridview_data_indices_for_rows.Clear();
            m_vec_task_gridview_selected_data_indices.Clear();

            // 检查是否有选中最后一行（最后一行是空白行，仅起排版作用）
            for (int n = 0; n < gridview_MeasureTask.SelectedRows.Count; n++)
            {
                if ((gridview_MeasureTask.Rows.Count - 1) == gridview_MeasureTask.SelectedRows[n].Index)
                {
                    MessageBox.Show(this, "最后一行是空白行，请不要选中。", "提示");
                    return;
                }
            }
            
            // 保存任务表每一行对应的数据索引号，数目等于任务表行数减去一（因为任务表最后一行内容为空），但大于等于数据的数目
            int prev_index = -1;
            for (int n = 0; n < gridview_MeasureTask.Rows.Count - 1; n++)
            {
                //Debugger.Log(0, null, string.Format("222222 aaa n = {0}, prev_index = {1}", n, prev_index));
                if ("" == gridview_MeasureTask[0, 0].Value.ToString())
                {
                    if (-1 == prev_index)
                        return;
                    else
                        m_vec_task_gridview_data_indices_for_rows.Add(prev_index);
                }
                //Debugger.Log(0, null, string.Format("222222 bbb"));
                int index = 0;
                if ("" == gridview_MeasureTask[0, n].Value.ToString())
                    index = prev_index;
                else
                    index = Convert.ToInt32(gridview_MeasureTask[0, n].Value.ToString()) - 1;

                //Debugger.Log(0, null, string.Format("222222 bbb index = {0}", index));
                if ((index < 0) || (index >= m_current_task_data.Count))
                {
                    MessageBox.Show(this, "任务表数据序号超出范围，请检查原因！", "提示");
                    return;
                }

                m_vec_task_gridview_data_indices_for_rows.Add(index);
                prev_index = index;
            }

            // 保存任务表选中的数据索引号
            for (int n = 0; n < gridview_MeasureTask.SelectedRows.Count; n++)
            {
                int index = gridview_MeasureTask.SelectedRows[n].Index;
                if (0 == m_vec_task_gridview_selected_data_indices.Count)
                    m_vec_task_gridview_selected_data_indices.Add(m_vec_task_gridview_data_indices_for_rows[index]);
                else
                {
                    bool bRepeated = false;
                    for (int k = 0; k < m_vec_task_gridview_selected_data_indices.Count; k++)
                    {
                        if (m_vec_task_gridview_data_indices_for_rows[index] == m_vec_task_gridview_selected_data_indices[k])
                        {
                            bRepeated = true;
                            break;
                        }
                    }
                    if (false == bRepeated)
                        m_vec_task_gridview_selected_data_indices.Add(m_vec_task_gridview_data_indices_for_rows[index]);
                }
            }

            // 检查选中行是否都属于同一个测量类型
            MEASURE_TYPE type = m_current_task_data[m_vec_task_gridview_selected_data_indices[0]].m_mes_type;
            for (int n = 0; n < m_vec_task_gridview_selected_data_indices.Count; n++)
            {
                if (m_current_task_data[m_vec_task_gridview_selected_data_indices[n]].m_mes_type != type)
                {
                    MessageBox.Show(this, "所选数据不全是同一种测量类型，无法统一修改！\r请选择同类数据进行修改。", "提示");
                    return;
                }
            }

            if (m_vec_task_gridview_selected_data_indices.Count > 0)
            {
                // 排序
                m_vec_task_gridview_selected_data_indices.Sort();

                int idx = m_vec_task_gridview_selected_data_indices[0];

                if (idx >= m_measure_items_on_graph.Count)
                    return;

                double ratio = m_graph_view.m_zoom_ratio_min * 25;
                double center_x = m_measure_items_on_graph[idx].m_center_x_on_graph - (m_graph_view.Width / 2) / ratio;
                double center_y = m_measure_items_on_graph[idx].m_center_y_on_graph - (m_graph_view.Height / 2) / ratio;
                
                m_graph_view.set_view_ratio_and_crd(ratio, center_x, center_y);
                m_graph_view.refresh_image();
            }
        }

        // 测量任务列表鼠标事件：鼠标双击
        private void gridview_MeasureTask_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (gridview_MeasureTask.RowCount <= 1)
                return;
            if (e.RowIndex == (gridview_MeasureTask.RowCount - 1))
                return;

            int idx = -1;
            if (0 == gridview_MeasureTask[0, e.RowIndex].Value.ToString().Length)
            {
                for (int n = e.RowIndex - 1; n >= 0; n--)
                {
                    if (gridview_MeasureTask[0, n].Value.ToString().Length > 0)
                    {
                        idx = Convert.ToInt32(gridview_MeasureTask[0, n].Value.ToString()) - 1;
                        break;
                    }
                }
            }
            else
                idx = Convert.ToInt32(gridview_MeasureTask[0, e.RowIndex].Value.ToString()) - 1;

            if (idx < m_current_task_data.Count)
            {
                if (DialogResult.No == MessageBox.Show(this, "是否移动到该测量项位置?", "提示", MessageBoxButtons.YesNo))
                    return;

                if (false == m_bOfflineMode)
                {
                    (new Thread(thread_measure_an_item_once_more2)).Start(idx);
                }
            }
        }

        // 图纸测量项列表右键菜单项点击事件：移除
        private void menuitem_DeleteGraphMeasureItem_Click(object sender, EventArgs e)
        {
            if (gridview_GraphMeasureItems.SelectedRows.Count <= 0)
                return;
            if (gridview_GraphMeasureItems.Rows.Count <= 1)
                return;
            if ("" == gridview_GraphMeasureItems[0, 0].Value.ToString())
                return;
            
            m_vec_graph_measure_item_gridview_data_indices_for_rows.Clear();
            m_vec_graph_measure_item_gridview_selected_data_indices.Clear();

            // 检查是否有选中最后一行（最后一行是空白行，仅起排版作用）
            for (int n = 0; n < gridview_GraphMeasureItems.SelectedRows.Count; n++)
            {
                if ((gridview_GraphMeasureItems.Rows.Count - 1) == gridview_GraphMeasureItems.SelectedRows[n].Index)
                {
                    MessageBox.Show(this, "最后一行是空白行，请不要选中。", "提示");
                    return;
                }
            }

            // 保存图纸测量项列表每一行对应的数据索引号，数目等于列表行数减去一（因为列表最后一行内容为空），但大于等于数据的数目
            int prev_index = -1;
            for (int n = 0; n < gridview_GraphMeasureItems.Rows.Count - 1; n++)
            {
                //Debugger.Log(0, null, string.Format("222222 aaa n = {0}, prev_index = {1}", n, prev_index));
                if ("" == gridview_GraphMeasureItems[0, 0].Value.ToString())
                {
                    if (-1 == prev_index)
                        return;
                    else
                        m_vec_graph_measure_item_gridview_data_indices_for_rows.Add(prev_index);
                }
                //Debugger.Log(0, null, string.Format("222222 bbb"));
                int index = 0;
                if ("" == gridview_GraphMeasureItems[0, n].Value.ToString())
                    index = prev_index;
                else
                    index = Convert.ToInt32(gridview_GraphMeasureItems[0, n].Value.ToString()) - 1;

                //Debugger.Log(0, null, string.Format("222222 bbb index = {0}", index));
                if ((index < 0) || (index >= m_measure_items_on_graph.Count))
                {
                    MessageBox.Show(this, "图纸测量项列表数据序号超出范围，请检查原因！", "提示");
                    return;
                }

                m_vec_graph_measure_item_gridview_data_indices_for_rows.Add(index);
                prev_index = index;
            }

            // 保存图纸测量项列表选中的数据索引号
            for (int n = 0; n < gridview_GraphMeasureItems.SelectedRows.Count; n++)
            {
                int index = gridview_GraphMeasureItems.SelectedRows[n].Index;
                if (0 == m_vec_graph_measure_item_gridview_selected_data_indices.Count)
                    m_vec_graph_measure_item_gridview_selected_data_indices.Add(m_vec_graph_measure_item_gridview_data_indices_for_rows[index]);
                else
                {
                    bool bRepeated = false;
                    for (int k = 0; k < m_vec_graph_measure_item_gridview_selected_data_indices.Count; k++)
                    {
                        if (m_vec_graph_measure_item_gridview_data_indices_for_rows[index] == m_vec_graph_measure_item_gridview_selected_data_indices[k])
                        {
                            bRepeated = true;
                            break;
                        }
                    }
                    if (false == bRepeated)
                        m_vec_graph_measure_item_gridview_selected_data_indices.Add(m_vec_graph_measure_item_gridview_data_indices_for_rows[index]);
                }
            }

            // 排序
            m_vec_graph_measure_item_gridview_selected_data_indices.Sort();

            if (false)
            {
                for (int n = 0; n < m_vec_graph_measure_item_gridview_selected_data_indices.Count; n++)
                    Debugger.Log(0, null, string.Format("222222 n= {0}, indices = {1}", n, m_vec_graph_measure_item_gridview_selected_data_indices[n]));
            }

            for (int n = 0; n < m_vec_graph_measure_item_gridview_selected_data_indices.Count; n++)
            {
                int idx = m_vec_graph_measure_item_gridview_selected_data_indices[n];

                if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == m_measure_items_on_graph[idx].m_mes_type)
                {
                    if ((idx + 1) > get_fiducial_mark_count(m_current_task_data))
                        m_measure_items_on_graph.RemoveAt(idx);
                    else
                    {
                        MessageBox.Show(this, string.Format("第 {0} 个定位孔已经被确认，无法移除！\r如需重选定位孔，请重新创建任务。", idx + 1), "提示");
                        return;
                    }
                }
                else
                    m_measure_items_on_graph.RemoveAt(idx);

                // 如果正在进行批量执行，那么中止批量执行
                if (true == m_bIsExecutingInBatch)
                    m_bStopExecution = true;
            }
            
            show_task_on_gridview(gridview_GraphMeasureItems, m_measure_items_on_graph);

            m_graph_view.refresh_image();
        }

        // 图纸测量项列表鼠标单击事件
        private void gridview_GraphMeasureItems_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (gridview_GraphMeasureItems.SelectedRows.Count <= 0)
                return;
            if (gridview_GraphMeasureItems.Rows.Count <= 1)
                return;
            if ("" == gridview_GraphMeasureItems[0, 0].Value.ToString())
                return;
            
            if (null == m_graph_view.Image) return;
            if (m_graph_view.m_zoom_ratio < 0.000001) return;
            if ((m_graph_view.Image.Width * m_graph_view.Image.Height) <= 0) return;

            m_vec_graph_measure_item_gridview_data_indices_for_rows.Clear();
            m_vec_graph_measure_item_gridview_selected_data_indices.Clear();

            // 检查是否有选中最后一行（最后一行是空白行，仅起排版作用）
            for (int n = 0; n < gridview_GraphMeasureItems.SelectedRows.Count; n++)
            {
                if ((gridview_GraphMeasureItems.Rows.Count - 1) == gridview_GraphMeasureItems.SelectedRows[n].Index)
                {
                    MessageBox.Show(this, "最后一行是空白行，请不要选中。", "提示");
                    return;
                }
            }
            
            // 保存图纸测量项列表每一行对应的数据索引号，数目等于列表行数减去一（因为列表最后一行内容为空），但大于等于数据的数目
            int prev_index = -1;
            for (int n = 0; n < gridview_GraphMeasureItems.Rows.Count - 1; n++)
            {
                //Debugger.Log(0, null, string.Format("222222 aaa n = {0}, prev_index = {1}", n, prev_index));
                if ("" == gridview_GraphMeasureItems[0, 0].Value.ToString())
                {
                    if (-1 == prev_index)
                        return;
                    else
                        m_vec_graph_measure_item_gridview_data_indices_for_rows.Add(prev_index);
                }
                //Debugger.Log(0, null, string.Format("222222 bbb"));
                int index = 0;
                if ("" == gridview_GraphMeasureItems[0, n].Value.ToString())
                    index = prev_index;
                else
                    index = Convert.ToInt32(gridview_GraphMeasureItems[0, n].Value.ToString()) - 1;

                //Debugger.Log(0, null, string.Format("222222 bbb index = {0}", index));
                if ((index < 0) || (index >= m_measure_items_on_graph.Count))
                {
                    MessageBox.Show(this, "图纸测量项列表数据序号超出范围，请检查原因！", "提示");
                    return;
                }

                m_vec_graph_measure_item_gridview_data_indices_for_rows.Add(index);
                prev_index = index;
            }

            // 保存图纸测量项列表选中的数据索引号
            for (int n = 0; n < gridview_GraphMeasureItems.SelectedRows.Count; n++)
            {
                int index = gridview_GraphMeasureItems.SelectedRows[n].Index;
                if (0 == m_vec_graph_measure_item_gridview_selected_data_indices.Count)
                    m_vec_graph_measure_item_gridview_selected_data_indices.Add(m_vec_graph_measure_item_gridview_data_indices_for_rows[index]);
                else
                {
                    bool bRepeated = false;
                    for (int k = 0; k < m_vec_graph_measure_item_gridview_selected_data_indices.Count; k++)
                    {
                        if (m_vec_graph_measure_item_gridview_data_indices_for_rows[index] == m_vec_graph_measure_item_gridview_selected_data_indices[k])
                        {
                            bRepeated = true;
                            break;
                        }
                    }
                    if (false == bRepeated)
                        m_vec_graph_measure_item_gridview_selected_data_indices.Add(m_vec_graph_measure_item_gridview_data_indices_for_rows[index]);
                }
            }

            if (m_vec_graph_measure_item_gridview_selected_data_indices.Count > 0)
            {
                // 排序
                m_vec_graph_measure_item_gridview_selected_data_indices.Sort();

                int idx = m_vec_graph_measure_item_gridview_selected_data_indices[0];
                double ratio = m_graph_view.m_zoom_ratio_min * 25;
                double center_x = m_measure_items_on_graph[idx].m_center_x_on_graph - (m_graph_view.Width / 2) / ratio;
                double center_y = m_measure_items_on_graph[idx].m_center_y_on_graph - (m_graph_view.Height / 2) / ratio;

                //Debugger.Log(0, null, string.Format("222222 m_center_x_on_graph = [{0:0.},{1:0.}], center_x = [{2:0.},{3:0.}]",
                //    m_measure_items_on_graph[idx].m_center_x_on_graph, m_measure_items_on_graph[idx].m_center_y_on_graph,
                //    center_x, center_y));

                m_graph_view.set_view_ratio_and_crd(ratio, center_x, center_y);
                m_graph_view.refresh_image();
            }
        }

        // 测量结果列表鼠标单击事件
        private void gridview_measure_results_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // 显示在状态栏的展开项
            if (gridview_measure_results.SelectedRows.Count > 0)
            {
                string info = "展开：" + gridview_measure_results.SelectedRows[0].Cells[0].Value;
                info += ", " + gridview_measure_results.SelectedRows[0].Cells[1].Value;
                info += ", " + gridview_measure_results.SelectedRows[0].Cells[2].Value;
                info += ", 标准值 " + gridview_measure_results.SelectedRows[0].Cells[3].Value + gridview_measure_results.SelectedRows[0].Cells[6].Value;
                info += ", " + gridview_measure_results.SelectedRows[0].Cells[4].Value;
                info += ", " + gridview_measure_results.SelectedRows[0].Cells[5].Value;
                info += ", 测量值 " + gridview_measure_results.SelectedRows[0].Cells[7].Value;
                info += ", 结果 " + gridview_measure_results.SelectedRows[0].Cells[8].Value;

                this.toolStripStatusLabel_GridViewExpandInfo.Text = info;
            }
        }

        // 测量结果列表鼠标事件：鼠标双击
        private void gridview_measure_results_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (gridview_measure_results.RowCount <= 1)
                return;
            if (e.RowIndex == (gridview_measure_results.RowCount - 1))
                return;

            int idx = -1;
            if (0 == gridview_measure_results[0, e.RowIndex].Value.ToString().Length)
            {
                for (int n = e.RowIndex - 1; n >= 0; n--)
                {
                    if (gridview_measure_results[0, n].Value.ToString().Length > 0)
                    {
                        idx = Convert.ToInt32(gridview_measure_results[0, n].Value.ToString()) - 1;
                        break;
                    }
                }
            }
            else
                idx = Convert.ToInt32(gridview_measure_results[0, e.RowIndex].Value.ToString()) - 1;

            //Debugger.Log(0, null, string.Format("222222 gridview事件：鼠标双击, e.RowIndex = {0}, idx = {1}", e.RowIndex, idx));

            if (idx < m_current_task_data.Count)
            {
                if (DialogResult.No == MessageBox.Show(this, "是否移动到该测量项位置?", "提示", MessageBoxButtons.YesNo))
                    return;

                if (false == m_bOfflineMode)
                {
                    (new Thread(thread_measure_an_item_once_more)).Start(idx);
                }
            }
        }

        // 主图像右键菜单项点击事件：设置当前位置为平台左下角
        private void menuitem_SetCurrentPosAsStageLeftBottom_Click(object sender, EventArgs e)
        {
            Point3d crd = new Point3d(0, 0, 0);
            if (true == MainUI.m_motion.get_xyz_crds(ref crd))
            {
                m_motion.m_PCB_leftbottom_crd.x = crd.x;
                m_motion.m_PCB_leftbottom_crd.y = crd.y;
                m_motion.m_PCB_leftbottom_crd.z = (crd.z + 30) < m_motion.m_axes[MotionOps.AXIS_Z - 1].positive_limit ? (crd.z + 30) : m_motion.m_axes[MotionOps.AXIS_Z - 1].positive_limit;

                m_pcb_alignment_offset.x = 0;
                m_pcb_alignment_offset.y = 0;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ("Delete" == keyData.ToString())
                return true;
            else
                return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
