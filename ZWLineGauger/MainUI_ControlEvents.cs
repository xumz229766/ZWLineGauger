using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZWLineGauger.Forms;
using ZWLineGauger.Gaugers;
using ZWLineGauger.Hardwares;

namespace ZWLineGauger
{
    public partial class MainUI : Form
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_set_picture_box_wh_for_ODB(int[] in_values);

        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_set_picture_box_wh_for_gerber(int[] in_values);
        
        public bool   m_bIsCtrlKeyPressed = false;

        bool   m_bIsVacuumOn = false;

        private void comboBox_Unit_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void comboBox_Len_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        // 按钮：创建任务
        private void btn_CreateTask_Click(object sender, EventArgs e)
        {
            if (true == m_bIsAddingNewItemsToExistingTask)
            {
                MessageBox.Show(this, "当前处于添加测量项状态，无法创建任务。请先完成添加，或者重启程序，再创建新任务。", "数据库未连接");
                return;
            }
            globaldata.isRun = false;
            Form form2 = null;
            if (GeneralUtils.check_if_form_is_open("Form_CreateTask", ref form2))
            {
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.Name == "Form_CreateTask")
                    {
                        frm.Focus();
                    }
                }
                return;
            }

            m_bUseSameThreeMarks = false;

            if (false == m_bIsCreatingTask)
            {
                if ((false == m_bOfflineMode) && (false == m_bIsSQLConnected) && m_bUseDatabase)
                {
                    MessageBox.Show(this, "尚未连接数据库，无法创建任务。", "数据库未连接");
                    return;
                }

                m_gauger.clear_gauger_state();

                // 设置倍率
                if (false == m_bOfflineMode)
                {
                    dl_message_sender send_message = CBD_SendMessage;
                    send_message("设置倍率", false, 0, null);
                }

                if (true == m_motion.m_bHomed)
                {
                    GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
                    if (DialogResult.Yes == MessageBox.Show(this, "是否移动到左下角?", "提示", MessageBoxButtons.YesNo))
                    {
                        btn_GotoPCBLeftBottom_Click(new object(), new EventArgs());
                    }
                }

                m_bIsPreparingToCreateTask = true;
                
                Form_SelectCreationMode form = new Form_SelectCreationMode(this);
                form.ShowInTaskbar = false;
                form.ShowDialog();
                
                if ((true == m_bIsCreatingTask) && (2 == m_nCreateTaskMode))
                {
                    btn_LoadTaskTxtFile_Click(new object(), new EventArgs());
                }
                
                m_bIsPreparingToCreateTask = false;

                // 珠海方正，判断是否可以复用已有的奇偶层定位孔记录
                #region
                if (m_customer == enum_customer.customer_fangzheng_F3)
                {
                    string strGraphFileName = Path.GetFileName(m_strGraphFilePath);
                    if (strGraphFileName.LastIndexOf(".") != -1)
                    {
                        string strOdbFileName = strGraphFileName.Substring(0, strGraphFileName.LastIndexOf("."));
                        string strLayerName = m_form_orientation.m_strLayerName;
                        int nLayerIdx = -1;

                        if ((strLayerName.IndexOf('l') >= 0) && (strLayerName.IndexOf('l') < (strLayerName.Length - 1)))
                        {
                            string str = strLayerName.Substring(strLayerName.IndexOf('l') + 1);
                            string strNum = "";
                            if (1 == str.Length)
                            {
                                if ((str[0] >= '0') && (str[0] <= '9'))
                                    strNum += str[0];
                            }
                            else
                            {
                                if ((str[0] >= '0') && (str[0] <= '9'))
                                {
                                    if ((str[1] < '0') || (str[1] > '9'))
                                        strNum += str[0];
                                    else
                                    {
                                        strNum += str[0];
                                        strNum += str[1];
                                    }
                                }
                            }

                            if (strNum.Length > 0)
                            {
                                nLayerIdx = Convert.ToInt32(strNum);
                                //Debugger.Log(0, null, string.Format("222222 nLayerIdx = {0}", nLayerIdx));

                                bool bIsOnList = false;
                                int nRecordIndex = -1;
                                for (int n = 0; n < m_list_three_marks_records.Count; n++)
                                {
                                    if (strOdbFileName == m_list_three_marks_records[n].m_strOdbFileName)
                                    {
                                        if ((nLayerIdx % 2) == m_list_three_marks_records[n].m_nEvenOddFlag)
                                        {
                                            bIsOnList = true;
                                            nRecordIndex = n;
                                            break;
                                        }
                                    }
                                }

                                if (true == bIsOnList)
                                {
                                    if (DialogResult.Yes == MessageBox.Show(this, 
                                        "当前料号层别 有可用的奇偶层定位孔记录，是否使用已有的定位孔记录？", "提示", MessageBoxButtons.YesNo))
                                    {
                                        (new Thread(thread_use_history_three_marks_record)).Start(nRecordIndex);

                                        Thread.Sleep(100);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                //Form_CreateTask form = new Form_CreateTask(this);
                //form.ShowInTaskbar = false;
                //form.ShowDialog();
            }
            else
            {
                Form_CreateTaskFinished form = new Form_CreateTaskFinished(this);
                form.ShowInTaskbar = false;
                form.ShowDialog();

                //btn_CreateTask.Text = "创建完成";
            }
        }

        // 按钮：完成创建
        private void btn_FinishCreateTask_Click(object sender, EventArgs e)
        {
            Form_CreateTaskFinished form = new Form_CreateTaskFinished(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();

            btn_CreateTask.Enabled = true;
        }

        // 按钮：添加（测量项）
        private void btn_AddMeasureItem_Click(object sender, EventArgs e)
        {
            if ("添加" == btn_AddMeasureItem.Text)
            {
                if (get_fiducial_mark_count(m_current_task_data) >= 3)
                {
                    btn_AddMeasureItem.Text = "完成添加";
                    btn_LoadTask.Enabled = false;
                    btn_UpdateTask.Enabled = false;

                    MessageBox.Show(this, "进入任务添加状态，请在确认三个定位孔后，手动加入新的测量项。", "提示");

                    m_bIsAddingNewItemsToExistingTask = true;
                    btn_RunTask_Click(new object(), new EventArgs());
                }
                else
                    MessageBox.Show(this, "任务资料有误。请先加载任务，或者检查任务定位孔数目。", "提示");
            }
            else
            {
                btn_UpdateTask_Click(new object(), new EventArgs());

                btn_AddMeasureItem.Text = "添加";
                m_bIsAddingNewItemsToExistingTask = false;

                bool bOK = m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, 0, 20);
                Thread.Sleep(200);
                bOK = m_motion.linear_XY_wait_until_stop(0, 0, 500, 1, false);
                
                m_bShowSmallSelectionFrame = false;
                m_bShowFrameDuringTaskCreation = false;
                m_bShowCoarseMark = false;
                m_bShowAccurateMark = false;

                m_bIsCreatingTask = false;
                m_bIsRunningTask = false;

                m_gauger.clear_gauger_state();

                dl_message_sender send_message = CBD_SendMessage;

                // 设置单位
                send_message("设置单位", false, m_nUnitTypeBeforeRunningTask, null);

                send_message("EnableButton(bool)", false, btn_RunTask, true);

                btn_LoadTask.Enabled = true;
                btn_UpdateTask.Enabled = true;

                // 关绿灯，开黄灯
                m_IO.set_IO_output(m_IO.m_output_green_light, Hardwares.IO_STATE.IO_HIGH);
                m_IO.set_IO_output(m_IO.m_output_yellow_light, Hardwares.IO_STATE.IO_LOW);
            }
        }

        // 按钮：更新任务（到数据库）
        private void btn_UpdateTask_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == MessageBox.Show(this, "请确认是否更新保存任务内容?", "提示", MessageBoxButtons.OKCancel))
            {
                bool bOK = true;

                // 保存到数据库
                //if (false == create_table_and_save_task_to_table(m_SQL_conn_measure_task, m_current_task_data, m_strCurrentTaskName, true))
                //{
                //    bOK = false;
                //    MessageBox.Show(this, "数据库保存失败！", "提示");
                //}

                // 保存到文件
                if (false == save_task_to_file(m_current_task_data, m_strCurrentTaskName))
                {
                    bOK = false;
                    MessageBox.Show(this, "文件保存失败！", "提示");
                }

                // 保存到文件
                if (m_bIsLoadedByBrowsingDir && (m_strCurrentTaskFileFullPath.Length > 0))
                {
                    if (false == save_task_to_file(m_current_task_data, m_strCurrentTaskFileFullPath))
                    {
                        bOK = false;
                        MessageBox.Show(this, "文件保存失败！", "提示");
                    }
                }

                if (true == bOK)
                    MessageBox.Show(this, "更新成功", "提示");
            }
        }

        // 按钮：加载任务
        private void btn_LoadTask_Click(object sender, EventArgs e)
        {
            if (true == m_bIsCreatingTask)
            {
                MessageBox.Show(this, "当前正在创建任务，请结束创建后再加载。", "提示", MessageBoxButtons.OK);
                return;
            }

            Form_LoadTask form = new Form_LoadTask(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 按钮：运行
        private void btn_RunTask_Click(object sender, EventArgs e)
        {
            m_bStopTask = false;
            m_bPause = false;
            m_bIsRunningTask = true;
            m_bExecuteInBatch = false;

            btn_RunTask.Enabled = false;

            m_strTaskRunningStartingTime = DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString()
                + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();

            m_strTaskRunningStartingDataTime = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString();

            m_strTaskRunningStartingHMSTime = DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();
            
            if (false == m_bIsAddingNewItemsToExistingTask)
                this.tabControl_Task.SelectedIndex = 2;
            
            m_list_measure_results.Clear();
            gridview_measure_results.Rows.Clear();

            // 清除历史数据，重绘图像
            m_vec_history_gaugers.Clear();
            ui_MainImage.Refresh();

            // 清除手拉测量框
            if (null != m_gauger)
                m_gauger.on_mouse_leave(e, ui_MainImage);

            (new Thread(thread_run_Chenling_measure_task)).Start(false);

            return;

            if (true == m_bLockLightWhenRunningTask)
            {
                if (DialogResult.No == MessageBox.Show(this, "选项“跑任务时锁定光源，不按首件亮度”当前处于激活状态，可能会影响任务效果。\r\n如果不想激活该选项，请点否，然后取消该选项，再运行任务。\r\n请确认是否继续执行任务？", "提示", MessageBoxButtons.YesNo))
                    return;
            }

            if (true == m_bPause)
            {
                m_bPause = false;
                btn_RunTask.Enabled = false;
                btn_Pause.Enabled = true;
                return;
            }

            if ((false == m_motion.m_bHomed) && (false == m_bOfflineMode))
                return;
            if (comboBox_Len.Items.Count <= 0)
            {
                MessageBox.Show(this, "变倍镜头初始化尚未完成，请稍等再试。", "提示");
                return;
            }
            
            m_gauger.clear_gauger_state();

            // 检查任务最前面3个测量类型是不是都是定位孔，如果不是，则视为任务无效，不执行
            if (m_current_task_data.Count < 3)
            {
                MessageBox.Show(this, "任务测量项目少于3个，任务无效。", "提示");
                return;
            }
            for (int n = 0; n < 3; n++)
            {
                if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE != m_current_task_data[n].m_mes_type)
                {
                    MessageBox.Show(this, "任务最前面3个测量类型不全是定位孔，任务无效。", "提示");
                    return;
                }
            }
            
            m_strTaskRunningStartingTime = DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString()
                + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();

            m_strTaskRunningStartingDataTime = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString();

            m_strTaskRunningStartingHMSTime = DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();
            
            m_bStopTask = false;
            m_bPause = false;
            m_bIsRunningTask = true;
            
            btn_RunTask.Enabled = false;

            if (false == m_bIsAddingNewItemsToExistingTask)
                this.tabControl_Task.SelectedIndex = 2;
            
            // 打开吸附
            if (false == m_bOfflineMode)
                CBD_SendMessage("开启吸附", true, null, null);

            m_list_measure_results.Clear();
            gridview_measure_results.Rows.Clear();
            
            if (0 == m_current_task_data[0].m_create_mode)
                (new Thread(thread_run_handmade_task)).Start(false);
            else
                (new Thread(thread_run_CAD_task)).Start(false);
        }

        // 按钮：停止
        private void btn_StopTask_Click(object sender, EventArgs e)
        {
            m_bStopExecution = true;
            globaldata.isRun = false;
            if (DialogResult.Yes == MessageBox.Show(this, "是否停止测量任务?", "提示", MessageBoxButtons.YesNo))
            {
                //hardware_ops_enable_vacuum(false);

                m_bStopTask = true;
                m_bIsRunningTask = false;
                m_bShowAccurateMark = false;
                m_bShowCoarseMark = false;

                m_bIsWaitingForConfirm = false;
                m_bIsWaitingForUserManualGauge = false;

                m_event_wait_for_manual_gauge.Set();
                m_event_wait_for_next_image.Set();
                m_event_wait_for_confirm_during_autorun.Set();
                m_event_wait_for_confirm_during_creation.Set();
                m_reset_event_for_updating_thumbnail_progress.Set();
                m_reset_event_for_updating_graphview_progress.Set();

                m_gauger.clear_gauger_state();

                btn_RunTask.Enabled = true;
                btn_Pause.Enabled = true;
            }
        }

        // 按钮：暂停
        private void btn_Pause_Click(object sender, EventArgs e)
        {
            if (true == m_bIsRunningTask)
            {
                m_bPause = true;
                btn_RunTask.Enabled = true;
                btn_Pause.Enabled = false;
            }
        }

        // 按钮：输出报表
        private void btn_SaveResultsToExcel_Click(object sender, EventArgs e)
        {
            thread_save_gauge_result_excel_report(new object());
            //thread_save_gauge_result_image(new object());
            return;

            if (gridview_measure_results.Rows.Count <= 1)
                return;
            if (m_strCurrentTaskName.Length <= 0)
                return;

            string dir = m_strExcelSavingDir;
            if ("" == dir)
            {
                dir = "测量结果报表";
                if (false == Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            if (Directory.Exists(dir))
            {
                // 创建料号目录
                //dir = dir + "\\" + m_strCurrentTaskName;
                Debugger.Log(0, null, string.Format("222222  测量结果111  m_strTaskRunningStartingDataTime = {0}", m_strTaskRunningStartingDataTime));
                dir = dir + "\\" + m_strTaskRunningStartingDataTime + "\\" + m_strUseringName;
                if (false == Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                Debugger.Log(0, null, string.Format("222222  测量结果222  dir = {0}", dir));
                // 生成基于时间点的文件名
                //string filepath = dir + "\\" + m_strCurrentTaskName + " " + m_strTaskRunningStartingTime + ".xlsx";
                string filepath = dir + "\\" + m_strCurrentTaskName + ".xlsx";
                Debugger.Log(0, null, string.Format("222222  222  filepath = {0}", filepath));

                string strTime = DateTime.Now.Hour.ToString()
                    + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();

                //判断改报表是否存在
                if (File.Exists(filepath))
                {

                    //存在
                    Debugger.Log(0, null, string.Format("222222 测量结果  存在报表"));

                    try
                    {
                        string[] fields = new string[] { "序号", "名称", "标准值", "上限", "下限", "单位", "测量值", "结果", "倍率", "类型", "层别", "批次", "操作人", "料号" };

                        int nRowsCount = gridview_measure_results.Rows.Count - 1;

                        m_excel_ops.creat_old_excel_file(filepath, fields, nRowsCount + 2, strTime);

                        m_excel_ops.open_old_file(filepath);

                        for (int n = 0; n < nRowsCount; n++)
                        {
                            ArrayList data = new ArrayList();
                            Debugger.Log(0, null, string.Format("222222 报表保存 333 "));
                            for (int k = 0; k < fields.Length - 4; k++)
                            {
                                if (0 == k)
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + k, n].Value.ToString());
                                else if ((fields.Length - 1 - 4) == k)
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + 1, n].Value.ToString());
                                else
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + k + 1, n].Value.ToString());
                            }

                            data.Add(m_strCurrentProductLayer);
                            data.Add(m_strBatchNum);
                            data.Add(m_strUseringName);
                            data.Add(m_strCurrentTaskName);

                            m_excel_ops.update_row(n + 1, data);
                        }

                        m_excel_ops.save_current_file();
                        m_excel_ops.close_current_file();

                        dl_message_sender send_message = CBD_SendMessage;
                        send_message("报表保存成功", true, null, null);
                    }
                    catch (Exception ex)
                    {
                        Debugger.Log(0, null, string.Format("222222 测量结果报表保存失败！异常信息：{0}", ex.Message));

                        dl_message_sender send_message = CBD_SendMessage;
                        send_message(string.Format("测量结果报表保存失败！异常信息：{0}", ex.Message), true, null, null);
                    }

                }
                else
                {
                    //不存在
                    Debugger.Log(0, null, string.Format("222222 测量结果  不存在报表"));
                    try
                    {
                        string[] fields = new string[] { "序号", "名称", "标准值", "上限", "下限", "单位", "测量值", "结果", "倍率", "类型", "层别", "批次", "操作人", "料号" };

                        int nRowsCount = gridview_measure_results.Rows.Count - 1;

                        m_excel_ops.create_excel_file(filepath, fields, nRowsCount + 2, strTime);

                        m_excel_ops.open(filepath);

                        for (int n = 0; n < nRowsCount; n++)
                        {
                            ArrayList data = new ArrayList();

                            for (int k = 0; k < fields.Length - 4; k++)
                            {
                                if (0 == k)
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + k, n].Value.ToString());
                                else if ((fields.Length - 1 - 4) == k)
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + 1, n].Value.ToString());
                                else
                                    data.Add(gridview_measure_results[RESULT_COLUMN_IDX + k + 1, n].Value.ToString());
                            }

                            data.Add(m_strCurrentProductLayer);
                            data.Add(m_strBatchNum);
                            data.Add(m_strUseringName);
                            data.Add(m_strCurrentTaskName);
                            m_excel_ops.update_row(n + 1, data);
                        }

                        m_excel_ops.save_current_file();
                        m_excel_ops.close_current_file();

                        dl_message_sender send_message = CBD_SendMessage;
                        send_message("报表保存成功", true, null, null);
                    }
                    catch (Exception ex)
                    {
                        Debugger.Log(0, null, string.Format("222222 测量结果报表保存失败！异常信息：{0}", ex.Message));

                        dl_message_sender send_message = CBD_SendMessage;
                        send_message(string.Format("测量结果报表保存失败！异常信息：{0}", ex.Message), true, null, null);
                    }
                }



                /*
                // 创建料号目录
                dir = dir + "\\" + m_strCurrentTaskName;
                if (false == Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // 生成基于时间点的文件名
                string strTime = DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString()
                    + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString();
                string filepath = dir + "\\" + strTime + ".xlsx";
                try
                {
                    string[] fields = new string[] { "序号", "名称", "标准值", "上限", "下限", "单位", "测量值", "结果", "倍率", "类型" };

                    int nRowsCount = gridview_measure_results.Rows.Count - 1;

                    m_excel_ops.create_excel_file(filepath, fields, nRowsCount, m_strTaskRunningStartingHMSTime);

                    m_excel_ops.open(filepath);
                    
                    for (int n = 0; n < nRowsCount; n++)
                    {
                        ArrayList data = new ArrayList();

                        for (int k = 0; k < fields.Length; k++)
                        {
                            if (0 == k)
                                data.Add(gridview_measure_results[RESULT_COLUMN_IDX + k, n].Value.ToString());
                            else if ((fields.Length - 1) == k)
                                data.Add(gridview_measure_results[RESULT_COLUMN_IDX + 1, n].Value.ToString());
                            else
                                data.Add(gridview_measure_results[RESULT_COLUMN_IDX + k + 1, n].Value.ToString());
                        }

                        m_excel_ops.update_row(n + 1, data);
                    }

                    m_excel_ops.save_current_file();
                    m_excel_ops.close_current_file();

                    MessageBox.Show(this, "报表保存完毕。", "提示");
                }
                catch (Exception ex)
                {
                    Debugger.Log(0, null, string.Format("222222 测量结果报表保存失败！异常信息：{0}", ex.Message));

                    dl_message_sender send_message = CBD_SendMessage;
                    send_message(string.Format("测量结果报表保存失败！异常信息：{0}", ex.Message), true, null, null);
                }

    */
            }
            else
            {
                dl_message_sender send_message = CBD_SendMessage;
                string info = string.Format("测量结果报表保存失败！异常信息：保存目录 {0} 不存在！", dir);

                send_message(info, true, null, null);
                MessageBox.Show(this, info, "提示");
            }
        }

        // 按钮：上环光
        public void btn_top_light_Click(object sender, EventArgs e)
        {
            m_top_light.m_bOn = !m_top_light.m_bOn;
            if (false == m_bOfflineMode)
            {
                //Debugger.Log(0, null, string.Format("222222 m_top_light.m_bOn = {0}", m_top_light.m_bOn));
                if (true == m_top_light.m_bOn)
                    m_top_light.open_light();
                else
                    m_top_light.close_light();
            }

            refresh_light_icons();
        }

        private void btn_top_light_MouseEnter(object sender, EventArgs e)
        {
            m_tooltip_for_top_light_button.ShowAlways = true;
            m_tooltip_for_top_light_button.SetToolTip(this.btn_top_light, "上环光");
        }

        private void btn_top_light_MouseLeave(object sender, EventArgs e)
        {
            m_tooltip_for_top_light_button.ShowAlways = false;
        }

        // 按钮：下环光
        public void btn_bottom_light_Click(object sender, EventArgs e)
        {
            m_bottom_light.m_bOn = !m_bottom_light.m_bOn;
            if (false == m_bOfflineMode)
            {
                if (true == m_bottom_light.m_bOn)
                    m_bottom_light.open_light();
                else
                    m_bottom_light.close_light();
            }

            refresh_light_icons();
        }

        private void btn_bottom_light_MouseEnter(object sender, EventArgs e)
        {
            m_tooltip_for_bottom_light_button.ShowAlways = true;
            m_tooltip_for_bottom_light_button.SetToolTip(this.btn_bottom_light, "下环光");
        }

        private void btn_bottom_light_MouseLeave(object sender, EventArgs e)
        {
            m_tooltip_for_bottom_light_button.ShowAlways = false;
        }

        // 按钮：Z轴向上运动
        private void btn_Z_up_MouseDown(object sender, MouseEventArgs e)
        {
            m_gauger.clear_gauger_state();
            
            m_nJogAxis = MotionOps.AXIS_Z;
            m_nJogDir = 1;
            m_bJogAxis = true;

            btn_Z_up.Image = Image.FromFile("icons\\up_pressed.bmp");

            dl_message_sender messenger1 = CBD_SendMessage;
            Thread thrd1 = new Thread(thread_jog_axis);
            thrd1.Start(messenger1);
        }
        private void btn_Z_up_MouseUp(object sender, MouseEventArgs e)
        {
            m_bJogAxis = false;

            btn_Z_up.Image = Image.FromFile("icons\\up_normal.bmp");
        }

        // 按钮：Z轴向下运动
        private void btn_Z_down_MouseDown(object sender, MouseEventArgs e)
        {
            m_gauger.clear_gauger_state();

            m_nJogAxis = MotionOps.AXIS_Z;
            m_nJogDir = -1;
            m_bJogAxis = true;

            btn_Z_down.Image = Image.FromFile("icons\\down_pressed.bmp");

            dl_message_sender messenger1 = CBD_SendMessage;
            Thread thrd1 = new Thread(thread_jog_axis);
            thrd1.Start(messenger1);
        }
        private void btn_Z_down_MouseUp(object sender, MouseEventArgs e)
        {
            m_bJogAxis = false;

            btn_Z_down.Image = Image.FromFile("icons\\down_normal.bmp");
        }

        // 按钮：Y轴向前运动
        private void btn_Forward_MouseDown(object sender, MouseEventArgs e)
        {
            m_gauger.clear_gauger_state();

            m_nJogAxis = MotionOps.AXIS_Y;
            m_nJogDir = 1;
            m_bJogAxis = true;

            btn_Forward.Image = Image.FromFile("icons\\forward_pressed.bmp");

            dl_message_sender messenger1 = CBD_SendMessage;
            Thread thrd1 = new Thread(thread_jog_axis);
            thrd1.Start(messenger1);
        }
        private void btn_Forward_MouseUp(object sender, MouseEventArgs e)
        {
            m_bJogAxis = false;

            btn_Forward.Image = Image.FromFile("icons\\forward_normal.bmp");
        }

        // 按钮：Y轴向后运动
        private void btn_Backward_MouseDown(object sender, MouseEventArgs e)
        {
            m_gauger.clear_gauger_state();

            m_nJogAxis = MotionOps.AXIS_Y;
            m_nJogDir = -1;
            m_bJogAxis = true;

            btn_Backward.Image = Image.FromFile("icons\\backward_pressed.bmp");

            dl_message_sender messenger1 = CBD_SendMessage;
            Thread thrd1 = new Thread(thread_jog_axis);
            thrd1.Start(messenger1);
        }
        private void btn_Backward_MouseUp(object sender, MouseEventArgs e)
        {
            m_bJogAxis = false;

            btn_Backward.Image = Image.FromFile("icons\\backward_normal.bmp");
        }

        // 按钮：X轴向左运动
        private void btn_Left_MouseDown(object sender, MouseEventArgs e)
        {
            m_gauger.clear_gauger_state();

            m_nJogAxis = MotionOps.AXIS_X;
            m_nJogDir = -1;
            m_bJogAxis = true;

            btn_Left.Image = Image.FromFile("icons\\left_pressed.bmp");
            
            dl_message_sender messenger1 = CBD_SendMessage;
            Thread thrd1 = new Thread(thread_jog_axis);
            thrd1.Start(messenger1);
        }
        private void btn_Left_MouseUp(object sender, MouseEventArgs e)
        {
            m_bJogAxis = false;
            
            btn_Left.Image = Image.FromFile("icons\\left_normal.bmp");
        }

        // 按钮：X轴向右运动
        private void btn_Right_MouseDown(object sender, MouseEventArgs e)
        {
            m_nJogAxis = MotionOps.AXIS_X;
            m_nJogDir = 1;
            m_bJogAxis = true;

            btn_Right.Image = Image.FromFile("icons\\right_pressed.bmp");

            dl_message_sender messenger1 = CBD_SendMessage;
            Thread thrd1 = new Thread(thread_jog_axis);
            thrd1.Start(messenger1);
        }
        private void btn_Right_MouseUp(object sender, MouseEventArgs e)
        {
            m_bJogAxis = false;

            btn_Right.Image = Image.FromFile("icons\\right_normal.bmp");
        }

        // 按钮：红点
        private void btn_RedDot_Click(object sender, EventArgs e)
        {
            if ((false == m_bOfflineMode) && (m_motion.m_bHomed))
            {
                Point3d crd = new Point3d(0, 0, 0);
                MainUI.m_motion.get_xyz_crds(ref crd);
                
                MainUI.m_motion.m_threaded_move_dest_X = crd.x + m_dbRedDotOffsetX;
                MainUI.m_motion.m_threaded_move_dest_Y = crd.y + m_dbRedDotOffsetY;
                MainUI.m_motion.m_threaded_move_dest_Z = crd.z;

                dl_message_sender messenger = CBD_SendMessage;
                (new Thread(m_motion.threaded_linear_XYZ_wait_until_stop)).Start(messenger);
            }
        }

        // 按钮：导入图纸
        public void btn_LoadGraph_Click(object sender, EventArgs e)
        {
            if (false == Form_GraphOrientation.m_bIsReadingGraphForThumbnail)
            {
                bool bHasDefaultDir = false;
                if (null != m_strGraphBrowseDir)
                {
                    if ((m_strGraphBrowseDir.Length > 0) && (Directory.Exists(m_strGraphBrowseDir)))
                        bHasDefaultDir = true;
                }

                OpenFileDialog dlg = new OpenFileDialog();
                if (bHasDefaultDir)
                    dlg.InitialDirectory = m_strGraphBrowseDir;
                else
                    dlg.InitialDirectory = ".";
                dlg.Filter = "图纸文件|*.*";
                dlg.ShowDialog();
                if (dlg.FileName != string.Empty)
                {
                    int[] in_values = new int[10];
                    in_values[0] = Form_GraphOrientation.m_thumbnail_views[8].Width;
                    in_values[1] = Form_GraphOrientation.m_thumbnail_views[8].Height;
                    dllapi_set_picture_box_wh_for_ODB(in_values);

                    m_strGraphBrowseDir = System.IO.Path.GetDirectoryName(dlg.FileName);
                    m_strGraphFilePath = dlg.FileName;

                    string info = string.Format("图纸信息：{0}", Path.GetFileName(m_strGraphFilePath));
                    toolStripStatusLabel_GraphInfo.Text = info;
                    
                    m_form_orientation.set_path(m_strGraphFilePath);
                    //m_form_orientation.Show();
                    m_form_orientation.ShowDialog();
                    if (true == m_form_orientation.m_bLoadGraph)
                    {
                        // 珠海方正
                        if ((m_customer == MainUI.enum_customer.customer_fangzheng_F3) && (0 == Form_GraphOrientation.m_nGraphType))
                        {
                            #region
                            string strGraphFileName = Path.GetFileName(m_strGraphFilePath);
                            if (strGraphFileName.LastIndexOf(".") != -1)
                            {
                                string strOdbFileName = strGraphFileName.Substring(0, strGraphFileName.LastIndexOf("."));
                                string strLayerName = m_form_orientation.m_strLayerName;
                                int nLayerIdx = -1;

                                if ((strLayerName.IndexOf('l') >= 0) && (strLayerName.IndexOf('l') < (strLayerName.Length - 1)))
                                {
                                    string str = strLayerName.Substring(strLayerName.IndexOf('l') + 1);
                                    string strNum = "";
                                    if (1 == str.Length)
                                    {
                                        if ((str[0] >= '0') && (str[0] <= '9'))
                                            strNum += str[0];
                                    }
                                    else
                                    {
                                        if ((str[0] >= '0') && (str[0] <= '9'))
                                        {
                                            if ((str[1] < '0') || (str[1] > '9'))
                                                strNum += str[0];
                                            else
                                            {
                                                strNum += str[0];
                                                strNum += str[1];
                                            }
                                        }
                                    }

                                    if (strNum.Length > 0)
                                    {
                                        nLayerIdx = Convert.ToInt32(strNum);
                                        //Debugger.Log(0, null, string.Format("222222 nLayerIdx = {0}", nLayerIdx));

                                        bool bIsOnList = false;
                                        for (int n = 0; n < m_list_three_marks_records.Count; n++)
                                        {
                                            if (strOdbFileName == m_list_three_marks_records[n].m_strOdbFileName)
                                            {
                                                if ((nLayerIdx % 2) == m_list_three_marks_records[n].m_nEvenOddFlag)
                                                {
                                                    bIsOnList = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (true == bIsOnList)
                                        {
                                            if (0 == (nLayerIdx % 2))
                                            {
                                                if (DialogResult.Yes == MessageBox.Show(this, "已经有相同图纸文件的偶数层定位孔记录，是否复用该记录？", "提示", MessageBoxButtons.YesNo))
                                                    m_bUseSameThreeMarks = true;
                                            }
                                            else
                                            {
                                                if (DialogResult.Yes == MessageBox.Show(this, "已经有相同图纸文件的奇数层定位孔记录，是否复用该记录？", "提示", MessageBoxButtons.YesNo))
                                                    m_bUseSameThreeMarks = true;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }

                        Form_GraphOrientation.m_reading_state = GRAPH_READING_STATE.NONE;

                        m_graph_view.clear_array_rects(ref GraphView.m_ODB_thumbnail_array_rects);

                        m_measure_items_on_graph.Clear();
                        m_current_task_data.Clear();
                        m_list_measure_results.Clear();
                        gridview_GraphMeasureItems.Rows.Clear();
                        gridview_MeasureTask.Rows.Clear();
                        gridview_measure_results.Rows.Clear();

                        // 启动进度监控线程
                        if (false == m_bIsMonitorStarted)
                        {
                            m_bIsMonitorStarted = true;
                            dl_message_sender messenger1 = CBD_SendMessage;
                            Thread thrd1 = new Thread(thread_monitor_graph_reading_progress);
                            thrd1.Start(messenger1);
                        }
                        
                        in_values[0] = ui_GraphView.Width;
                        in_values[1] = ui_GraphView.Height;
                        if (0 == Form_GraphOrientation.m_nGraphType)
                            dllapi_set_picture_box_wh_for_ODB(in_values);
                        else if (1 == Form_GraphOrientation.m_nGraphType)
                            dllapi_set_picture_box_wh_for_gerber(in_values);

                        // 启动图纸读取线程
                        if (0 == Form_GraphOrientation.m_nGraphType)
                        {
                            dl_message_sender messenger = CBD_SendMessage;
                            Thread thrd = new Thread(thread_get_ODB_big_image);
                            thrd.Start(messenger);
                        }
                        else if (1 == Form_GraphOrientation.m_nGraphType)
                        {
                            dl_message_sender messenger = CBD_SendMessage;
                            Thread thrd = new Thread(thread_get_gerber_big_image);
                            thrd.Start(messenger);
                        }

                        m_form_graph_progress = new Form_ProgressInfo(this);
                        m_form_graph_progress.ShowDialog();
                        m_form_graph_progress.Dispose();

                        // 对ODB自动测量项进行排序，使最前面三个测量项都是定位孔，并且符合左下、左上、右上的顺序
                        if (m_ODB_measure_items.Count > 0)
                            rearrange_ODB_measure_items_order(ref m_ODB_measure_items);

                        CBD_SendMessage("刷新ODB自动生成的测量项", false, null, null);
                        
                        // 通告主线程
                        if (true == m_bIsPreparingToCreateTask)
                            CBD_SendMessage(string.Format("开始创建任务"), false, null, null);
                        
                        // ODB自动模式创建首件
                        //if ((m_ODB_measure_items.Count > 0) && (false == m_bOfflineMode))
                        if (m_ODB_measure_items.Count > 0)
                        {
                            //Thread thrd = new Thread(thread_create_task_by_ODB_data);
                            //thrd.Start();
                        }
                    }
                }
            }
        }

        // 按钮：初始化系统
        private void btn_InitSystem_Click(object sender, EventArgs e)
        {
            if (false == m_bOfflineMode)
            {
                if (DialogResult.Yes == MessageBox.Show(this, "是否初始化系统?", "提示", MessageBoxButtons.YesNo))
                {
                    m_form_progress = new Form_ProgressInfo(this);
                    m_form_progress.set_title("PixelLiner");
                    m_form_progress.set_tip_info("正在初始化运动系统......");
                    m_form_progress.set_infinite_wait_mode(PROGRESS_WAIT_MODE.NORMAL);
                    //m_form_progress.disable_closing_window();

                    m_motion.m_bHomed = false;
                    m_motion.m_bInitialized = false;

                    btn_Home.Enabled = false;

                    dl_message_sender messenger = CBD_SendMessage;
                    (new Thread(thread_init_motion)).Start(messenger);

                    m_form_progress.ShowDialog();

                    if (true == m_motion.m_bHomed)
                        btn_Home.Enabled = true;
                }
            }
        }

        // 按钮：回原点
        private void btn_Home_Click(object sender, EventArgs e)
        {
            if (false == m_motion.m_bHomed)
                return;

            m_gauger.clear_gauger_state();

            Point3d crd = new Point3d(0, 0, 0);
            m_motion.get_xyz_crds(ref crd);

            if (crd.z < -20)
            {
                m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, crd.z + 10, 30);
                Thread.Sleep(500);
            }
            
            //bOK = m_motion.linear_XY_wait_until_stop(0, 0, 380, 1, false);

            m_motion.m_threaded_move_dest_X = 0;
            m_motion.m_threaded_move_dest_Y = 0;
            m_motion.m_threaded_move_dest_Z = 0;

            dl_message_sender messenger = CBD_SendMessage;
            (new Thread(m_motion.threaded_linear_XYZ_wait_until_stop)).Start(messenger);
        }

        // 按钮：自动对焦（探高）
        private void btn_AutoFocus_Click(object sender, EventArgs e)
        {
            if (false == m_motion.m_bHomed)
                return;

            m_bDetectHeightOnce = true;

            m_gauger.clear_gauger_state();
            m_image_operator.do_auto_focus();
        }

        // 按钮：到PCB左下角
        private void btn_GotoPCBLeftBottom_Click(object sender, EventArgs e)
        {
            if (false == m_motion.m_bHomed)
                return;

            m_gauger.clear_gauger_state();

            double offset = 15 * m_motion.m_axes[MotionOps.AXIS_Z - 1].nDir;

            if (false)
            {
                m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, m_motion.m_axes[MotionOps.AXIS_Z - 1].negative_limit - offset, 20);
                m_motion.linear_XY_wait_until_stop(m_motion.m_PCB_leftbottom_crd.x + m_pcb_alignment_offset.x,
                    m_motion.m_PCB_leftbottom_crd.y + m_pcb_alignment_offset.y, 500, 1, false);
            }

            m_motion.m_threaded_move_dest_X = m_motion.m_PCB_leftbottom_crd.x + m_pcb_alignment_offset.x;
            m_motion.m_threaded_move_dest_Y = m_motion.m_PCB_leftbottom_crd.y + m_pcb_alignment_offset.y;
            m_motion.m_threaded_move_dest_Z = m_motion.m_axes[MotionOps.AXIS_Z - 1].negative_limit - offset;
            
            dl_message_sender messenger = CBD_SendMessage;
            (new Thread(m_motion.threaded_linear_XYZ_wait_until_stop)).Start(messenger);
        }

        // 按钮：导入首件txt文件
        private void btn_LoadTaskTxtFile_Click(object sender, EventArgs e)
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
            dlg.Filter = "离线文件|*.txt";
            dlg.ShowDialog();
            if (dlg.FileName != string.Empty)
            {
                m_strOfflineFileBrowseDir = System.IO.Path.GetDirectoryName(dlg.FileName);

                m_current_task_data.Clear();
                m_measure_items_on_graph.Clear();
                m_measure_items_from_txt.Clear();

                if (true == read_task_txt_file(dlg.FileName, enum_customer.customer_fangzheng_F3, m_measure_items_from_txt))
                {
                    show_task_on_gridview(gridview_GraphMeasureItems, m_measure_items_from_txt);
                    this.tabControl_Task.SelectedIndex = 1;
                }
            }
        }

        // 按钮：批量执行
        private void btn_ExecuteInBatch_Click(object sender, EventArgs e)
        {
            if ((true == m_bOfflineMode) || (false == m_bIsCreatingTask))
                return;
            
            if ((m_measure_items_from_txt.Count <= 0) && (m_measure_items_on_graph.Count > 0))
            {
                if ((null == m_graph_view.Image) || ((m_graph_view.Image.Width * m_graph_view.Image.Height) <= 0))
                    return;
            }
            
            if (2 != m_nCreateTaskMode)
            {
                if (false == m_bIsAlignmentPtSet)
                {
                    MessageBox.Show(this, "请先对齐图纸和实物板坐标系，再进行批量执行。", "提示");
                    return;
                }
            }
            
            if (true == m_bIsMeasuringDuringCreation)
                return;
            m_bIsMeasuringDuringCreation = true;
            m_bStopExecution = false;

            if (1 == m_nAlgorithm)
            {
                if (DialogResult.No == MessageBox.Show(this, 
                    "您选择了陶瓷板、白底板模式，请确认是否继续？\r\n如果是普通板，请点否，取消勾选陶瓷板、白底板选项，再继续作业。", "提示", MessageBoxButtons.YesNo))
                    return;
            }
            
            dl_message_sender messenger = CBD_SendMessage;
            (new Thread(execute_in_batch)).Start(messenger);
        }

        // 按钮：停止批量执行
        private void btn_StopExecution_Click(object sender, EventArgs e)
        {
            m_bStopExecution = true;

            m_event_wait_for_finish_measurement.Set();
        }

        // 按钮：开启/关闭吸附
        private void btn_Vacuum_Click(object sender, EventArgs e)
        {

        }

        // 按钮：参数设置
        private void btn_Settings_Click(object sender, EventArgs e)
        {
            Form_Settings form = new Form_Settings(this);
            form.ShowInTaskbar = false;
            form.ShowDialog();
        }

        // 按钮：初始化图标
        private void btn_InitSystem_MouseEnter(object sender, EventArgs e)
        {
            btn_InitSystem.Image = Image.FromFile("icons\\init-hovered.png");

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.btn_InitSystem, "平台初始化");
        }

        private void btn_InitSystem_MouseLeave(object sender, EventArgs e)
        {
            btn_InitSystem.Image = Image.FromFile("icons\\init.png");
        }

        // 按钮：回原点图标
        private void btn_Home_MouseEnter(object sender, EventArgs e)
        {
            btn_Home.Image = Image.FromFile("icons\\home-hovered.png");

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.btn_Home, "回原点");
        }

        private void btn_Home_MouseLeave(object sender, EventArgs e)
        {
            btn_Home.Image = Image.FromFile("icons\\home.png");
        }

        // 按钮：创建任务图标
        private void btn_CreateTask_MouseEnter(object sender, EventArgs e)
        {
            if (true == m_bIsAddingNewItemsToExistingTask)
                return;

            if (false == m_bIsCreatingTask)
            {
                btn_CreateTask.Image = Image.FromFile("icons\\create-hovered.png");

                m_tooltip_for_measure_item.ShowAlways = true;
                m_tooltip_for_measure_item.SetToolTip(this.btn_CreateTask, "创建任务");
            }
            else
            {
                btn_CreateTask.Image = Image.FromFile("icons\\save-hovered.png");

                m_tooltip_for_measure_item.ShowAlways = true;
                m_tooltip_for_measure_item.SetToolTip(this.btn_CreateTask, "创建完成");
            }
        }

        private void btn_CreateTask_MouseLeave(object sender, EventArgs e)
        {
            if (true == m_bIsAddingNewItemsToExistingTask)
                return;

            if (false == m_bIsCreatingTask)
                btn_CreateTask.Image = Image.FromFile("icons\\create.png");
            else
                btn_CreateTask.Image = Image.FromFile("icons\\save.png");
        }

        // 按钮：自动对焦图标
        private void btn_AutoFocus_MouseEnter(object sender, EventArgs e)
        {
            btn_AutoFocus.Image = Image.FromFile("icons\\focus-hovered.png");

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.btn_AutoFocus, "自动对焦（探高）");
        }

        private void btn_AutoFocus_MouseLeave(object sender, EventArgs e)
        {
            btn_AutoFocus.Image = Image.FromFile("icons\\focus.png");
        }

        // 按钮：加载任务图标
        private void btn_LoadTask_MouseEnter(object sender, EventArgs e)
        {
            btn_LoadTask.Image = Image.FromFile("icons\\load-hovered.png");

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.btn_LoadTask, "加载任务");
        }
        
        private void btn_LoadTask_MouseLeave(object sender, EventArgs e)
        {
            btn_LoadTask.Image = Image.FromFile("icons\\load.png");
        }

        // 按钮：导入图纸图标
        private void btn_LoadGraph_MouseEnter(object sender, EventArgs e)
        {
            btn_LoadGraph.Image = Image.FromFile("icons\\cam-hovered.png");

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.btn_LoadGraph, "导入图纸");
        }

        private void btn_LoadGraph_MouseLeave(object sender, EventArgs e)
        {
            btn_LoadGraph.Image = Image.FromFile("icons\\cam.png");
        }

        // 按钮：暂停图标
        private void btn_Pause_MouseEnter(object sender, EventArgs e)
        {
            btn_Pause.Image = Image.FromFile("icons\\pause-hovered.png");

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.btn_Pause, "暂停");
        }

        private void btn_Pause_MouseLeave(object sender, EventArgs e)
        {
            btn_Pause.Image = Image.FromFile("icons\\pause.png");
        }

        // 按钮：运行图标
        private void btn_RunTask_MouseEnter(object sender, EventArgs e)
        {
            btn_RunTask.Image = Image.FromFile("icons\\run-hovered.png");

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.btn_RunTask, "执行任务");
        }

        private void btn_RunTask_MouseLeave(object sender, EventArgs e)
        {
            btn_RunTask.Image = Image.FromFile("icons\\run.png");
        }

        // 按钮：停止图标
        private void btn_StopTask_MouseEnter(object sender, EventArgs e)
        {
            btn_StopTask.Image = Image.FromFile("icons\\stop-hovered.png");

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.btn_StopTask, "停止任务");
        }

        private void btn_StopTask_MouseLeave(object sender, EventArgs e)
        {
            btn_StopTask.Image = Image.FromFile("icons\\stop.png");
        }

        // 按钮：到左下角图标
        private void btn_GotoPCBLeftBottom_MouseEnter(object sender, EventArgs e)
        {
            btn_GotoPCBLeftBottom.Image = Image.FromFile("icons\\leftbottom-hovered.png");

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.btn_GotoPCBLeftBottom, "到左下角");
        }

        private void btn_GotoPCBLeftBottom_MouseLeave(object sender, EventArgs e)
        {
            btn_GotoPCBLeftBottom.Image = Image.FromFile("icons\\leftbottom.png");
        }

        // 按钮：设置图标
        private void btn_Settings_MouseEnter(object sender, EventArgs e)
        {
            btn_Settings.Image = Image.FromFile("icons\\settings-hovered.png");

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.btn_Settings, "设置");
        }

        private void btn_Settings_MouseLeave(object sender, EventArgs e)
        {
            btn_Settings.Image = Image.FromFile("icons\\settings.png");
        }

        #region 测量工具图标
        // 按钮：点击下线宽图标
        private void ui_btn_14LineWidth_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_14 != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.LINE_WIDTH_14);
            }
        }

        private void ui_btn_14LineWidth_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_14 != m_current_measure_type)
                this.ui_btn_14LineWidth.Image = m_images_for_line_width_14[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_14LineWidth, "下线宽\\14线宽");
        }

        private void ui_btn_14LineWidth_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_14 != m_current_measure_type)
                this.ui_btn_14LineWidth.Image = m_images_for_line_width_14[0];
        }

        private void ui_btn_Line_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.LINE);
            }
        }

        private void ui_btn_Line_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE != m_current_measure_type)
                this.ui_btn_Line.Image = m_images_for_line[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_Line, "单线寻边");
        }

        private void ui_btn_Line_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE != m_current_measure_type)
                this.ui_btn_Line.Image = m_images_for_line[0];
        }
        
        // 按钮：点击上线宽图标
        private void ui_btn_23LineWidth_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.L_SHAPE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.L_SHAPE);
            }
        }

        private void ui_btn_23LineWidth_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.L_SHAPE != m_current_measure_type)
                this.ui_btn_23LineWidth.Image = m_images_for_line_width_23[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            //m_tooltip_for_measure_item.SetToolTip(this.ui_btn_23LineWidth, "上线宽\\23线宽");
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_23LineWidth, "L型拐角测量");
        }

        private void ui_btn_23LineWidth_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.L_SHAPE != m_current_measure_type)
                this.ui_btn_23LineWidth.Image = m_images_for_line_width_23[0];
        }

        // 按钮：点击 13线宽 图标
        private void ui_btn_13LineWidth_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_13 != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.LINE_WIDTH_13);
            }
        }

        private void ui_btn_13LineWidth_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_13 != m_current_measure_type)
                this.ui_btn_13LineWidth.Image = m_images_for_line_width_13[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_13LineWidth, "13线宽");
        }

        private void ui_btn_13LineWidth_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_13 != m_current_measure_type)
                this.ui_btn_13LineWidth.Image = m_images_for_line_width_13[0];
        }

        // 按钮：点击上下线宽图标
        private void ui_btn_1234LineWidth_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.BULGE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.BULGE);
            }
        }

        private void ui_btn_1234LineWidth_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.BULGE != m_current_measure_type)
                this.ui_btn_1234LineWidth.Image = m_images_for_line_width_1234[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_1234LineWidth, "鼓包");
        }

        private void ui_btn_1234LineWidth_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.BULGE != m_current_measure_type)
                this.ui_btn_1234LineWidth.Image = m_images_for_line_width_1234[0];
        }

        // 按钮：点击弧形线宽图标
        private void ui_btn_ArcLineWidth_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR);
            }
        }
        
        private void ui_btn_ArcLineWidth_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR != m_current_measure_type)
                this.ui_btn_ArcLineWidth.Image = m_images_for_arc_line_width[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_ArcLineWidth, "基于轮廓的线宽");
        }

        private void ui_btn_ArcLineWidth_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR != m_current_measure_type)
                this.ui_btn_ArcLineWidth.Image = m_images_for_arc_line_width[0];
        }

        // 按钮：点击线距图标
        private void ui_btn_LineSpace_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_SPACE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.LINE_SPACE);
            }
        }

        private void ui_btn_LineSpace_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_SPACE != m_current_measure_type)
                this.ui_btn_LineSpace.Image = m_images_for_line_space[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_LineSpace, "线距");
        }

        private void ui_btn_LineSpace_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_SPACE != m_current_measure_type)
                this.ui_btn_LineSpace.Image = m_images_for_line_space[0];
        }

        // 按钮：点击弧形线距图标
        private void ui_btn_ArcLineSpace_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_TO_EDGE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.LINE_TO_EDGE);
            }
        }

        private void ui_btn_ArcLineSpace_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_TO_EDGE != m_current_measure_type)
                this.ui_btn_ArcLineSpace.Image = m_images_for_line_space[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_ArcLineSpace, "暗线到边缘");
        }

        private void ui_btn_ArcLineSpace_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.LINE_TO_EDGE != m_current_measure_type)
                this.ui_btn_ArcLineSpace.Image = m_images_for_line_space[0];
        }

        // 按钮：点击空心圆间距(短)图标
        private void ui_btn_ShortSpaceBetweenTwoEmptyCircles_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.ETCH_DOWN != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.ETCH_DOWN);
            }
        }

        private void ui_btn_ShortSpaceBetweenTwoEmptyCircles_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.ETCH_DOWN != m_current_measure_type)
                this.ui_btn_ShortSpaceBetweenTwoEmptyCircles.Image = m_images_for_short_space_between_two_empty_circles[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_ShortSpaceBetweenTwoEmptyCircles, "Etch Down");
        }

        private void ui_btn_ShortSpaceBetweenTwoEmptyCircles_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.ETCH_DOWN != m_current_measure_type)
                this.ui_btn_ShortSpaceBetweenTwoEmptyCircles.Image = m_images_for_short_space_between_two_empty_circles[0];
        }

        // 按钮：点击 由外向内找圆 图标 改水平线
        private void ui_btn_CircleOuterToInner_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE);
            }
        }

        private void ui_btn_CircleOuterToInner_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE != m_current_measure_type)
                this.ui_btn_CircleOuterToInner.Image = m_images_for_HandLL[3];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_CircleOuterToInner, "手拉平行线(水平)");
        }

        private void ui_btn_CircleOuterToInner_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE != m_current_measure_type)
                this.ui_btn_CircleOuterToInner.Image = m_images_for_HandLL[2];
        }
        #region 手动竖直线响应图标
        // 按钮：手动竖直线响应图标
        private void ui_btn_HandLL_vertical_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE);
            }
        }

        private void ui_btn_HandLL_vertical_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE != m_current_measure_type)
                this.ui_btn_handLL_vertical.Image = m_images_for_HandLL[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_handLL_vertical, "手拉平行线(竖直)");
        }

        private void ui_btn_HandLL_vertical_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE != m_current_measure_type)
                this.ui_btn_handLL_vertical.Image = m_images_for_HandLL[0];
        }
        #endregion

        #region 手动角度平行线响应图标
        // 按钮：手动角度平行响应图标
        private void ui_btn_HandLL_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_PARALLEL_LINE_TO_LINE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.HAND_DRAWN_PARALLEL_LINE_TO_LINE);
            }
        }

        private void ui_btn_HandLL_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_PARALLEL_LINE_TO_LINE != m_current_measure_type)
                this.ui_btn_handLL_vertical.Image = m_images_for_HandLL[5];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_handLL_vertical, "手拉平行线(角度)");
        }

        private void ui_btn_HandLL_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_PARALLEL_LINE_TO_LINE != m_current_measure_type)
                this.ui_btn_handLL_vertical.Image = m_images_for_HandLL[4];
        }
        #endregion

        #region 手动点竖直线响应图标
        // 按钮：手动点竖直线响应图标
        private void ui_btn_HandPL_vertical_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE);
            }
        }

        private void ui_btn_HandPL_vertical_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE != m_current_measure_type)
                this.ui_btn_handPL_vertical.Image = m_images_for_HandPL[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_handPL_vertical, "手拉点平行线(竖直)");
        }

        private void ui_btn_HandPL_vertical_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE != m_current_measure_type)
                this.ui_btn_handPL_vertical.Image = m_images_for_HandPL[0];
        }
        #endregion
        #region 手动点水平线响应图标
        // 按钮：手动点水平线响应图标
        private void ui_btn_HandPL_horizontal_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE);
            }
        }

        private void ui_btn_HandPL_horizontal_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE != m_current_measure_type)
                this.ui_btn_handPL_horizontal.Image = m_images_for_HandPL[3];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_handPL_horizontal, "手拉点平行线(水平)");
        }

        private void ui_btn_HandPL_horizontal_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE != m_current_measure_type)
                this.ui_btn_handPL_horizontal.Image = m_images_for_HandPL[2];
        }
        #endregion
        // 按钮：点击 由内向外找圆 图标
        private void ui_btn_CircleInnerToOuter_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE);
            }
        }

        private void ui_btn_CircleInnerToOuter_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.CIRCLE_INNER_TO_OUTER != m_current_measure_type)
                this.ui_btn_CircleInnerToOuter.Image = m_images_for_circle_inner_to_outer[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_CircleInnerToOuter, "由内向外找圆");
        }

        private void ui_btn_CircleInnerToOuter_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.CIRCLE_INNER_TO_OUTER != m_current_measure_type)
                this.ui_btn_CircleInnerToOuter.Image = m_images_for_circle_inner_to_outer[0];
        }

        // 按钮：点击 手动三点选圆 图标
        private void ui_btn_HandPickCircle_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_PICK_CIRCLE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.HAND_PICK_CIRCLE);
            }
        }

        private void ui_btn_HandPickCircle_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_PICK_CIRCLE != m_current_measure_type)
                this.ui_btn_HandPickCircle.Image = m_images_for_hand_pick_circle[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_HandPickCircle, "手动三点选圆");
        }

        private void ui_btn_HandPickCircle_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_PICK_CIRCLE != m_current_measure_type)
                this.ui_btn_HandPickCircle.Image = m_images_for_hand_pick_circle[0];
        }

        // 按钮：手动拉线
        private void ui_btn_Pt2PtDistance_Click(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_PICK_LINE != m_current_measure_type)
            {
                set_gauger(MEASURE_TYPE.HAND_PICK_LINE);
            }
        }

        private void ui_btn_Pt2PtDistance_MouseEnter(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_PICK_LINE != m_current_measure_type)
                this.ui_btn_Pt2PtDistance.Image = m_images_for_pt_2_pt_distance[1];

            m_tooltip_for_measure_item.ShowAlways = true;
            m_tooltip_for_measure_item.SetToolTip(this.ui_btn_Pt2PtDistance, "手动拉线");
        }

        private void ui_btn_Pt2PtDistance_MouseLeave(object sender, EventArgs e)
        {
            if (MEASURE_TYPE.HAND_PICK_LINE != m_current_measure_type)
                this.ui_btn_Pt2PtDistance.Image = m_images_for_pt_2_pt_distance[0];
        }
        #endregion
        // 拖动条：上环光亮度
        private void ui_trackBar_TopLight_ValueChanged(object sender, EventArgs e)
        {
            m_top_light.m_nBrightness = ui_trackBar_TopLight.Value;

            if ((false == m_bOfflineMode) && (true == m_top_light.m_bOn))
                m_top_light.change_brightness(m_top_light.m_nBrightness);

            m_tooltip_top_light_value.ShowAlways = true;
            m_tooltip_top_light_value.SetToolTip(this.ui_trackBar_TopLight, m_top_light.m_nBrightness.ToString());
        }

        // 拖动条：下环光亮度
        private void ui_trackBar_BottomLight_ValueChanged(object sender, EventArgs e)
        {
            m_bottom_light.m_nBrightness = ui_trackBar_BottomLight.Value;

            if ((false == m_bOfflineMode) && (true == m_bottom_light.m_bOn))
                m_bottom_light.change_brightness(m_bottom_light.m_nBrightness);

            m_tooltip_bottom_light_value.ShowAlways = true;
            m_tooltip_bottom_light_value.SetToolTip(this.ui_trackBar_BottomLight, m_bottom_light.m_nBrightness.ToString());
        }

        // 拖动条：设置小搜索框的大小
        private void ui_trackBar_SmallSearchFrameExtent_ValueChanged(object sender, EventArgs e)
        {
            m_nSmallSearchFrameExtent = ui_trackBar_SmallSearchFrameExtent.Value;

            m_tooltip_small_search_frame_value.ShowAlways = true;
            m_tooltip_small_search_frame_value.SetToolTip(this.ui_trackBar_SmallSearchFrameExtent, m_nSmallSearchFrameExtent.ToString());
        }

        // 拖动条：设置大搜索框的大小
        private void ui_trackBar_BigSearchFrameExtent_ValueChanged(object sender, EventArgs e)
        {
            m_nBigSearchFrameExtent = ui_trackBar_BigSearchFrameExtent.Value;

            m_tooltip_big_search_frame_value.ShowAlways = true;
            m_tooltip_big_search_frame_value.SetToolTip(this.ui_trackBar_BigSearchFrameExtent, m_nBigSearchFrameExtent.ToString());
        }

        // 鼠标事件：主图像鼠标按下
        private void ui_MainImage_MouseDown(object sender, MouseEventArgs e)
        {
            textBox_KeyInfo.Focus();

            if (null != ui_MainImage.Image)
            {
                if (true == m_bSelectBaseTemplate)
                    m_base_template_click_pt = new Point(e.X, e.Y);
                else
                {
                    if (true == m_gauger.is_mouse_in_string_rect(e))
                    {
                        m_gauger.set_mouse_down_on_string_state(true);

                        m_gauger.set_mouse_string_down_pt(e);
                    }
                    else
                    {
                        Debugger.Log(0, null, string.Format("222222 鼠标不在文字框内"));
                        m_gauger.on_mouse_down(e, ui_MainImage);
                    }
                }
            }
        }

        // 鼠标事件：主图像鼠标移动
        private void ui_MainImage_MouseMove(object sender, MouseEventArgs e)
        {
            feed_dog();

            if (null != ui_MainImage.Image)
            {
                if ((true == m_bSelectBaseTemplate) && (m_base_template_click_pt.X > 0))
                {
                    int left = Math.Min(e.X, m_base_template_click_pt.X);
                    int top = Math.Min(e.Y, m_base_template_click_pt.Y);
                    int right = Math.Max(e.X, m_base_template_click_pt.X);
                    int bottom = Math.Max(e.Y, m_base_template_click_pt.Y);

                    m_base_template_lefttop = new Point(left, top);
                    m_base_template_rightbottom = new Point(right, bottom);

                    ui_MainImage.Refresh();
                }
                else
                {
                    if (true == m_gauger.m_bIsMouseClickDownOnString)
                    {
                        m_gauger.set_mouse_string_move_pt(e);

                        ui_MainImage.Refresh();
                    }
                    else
                        m_gauger.on_mouse_move(e, ui_MainImage, m_bSetHorizonModeForGaugerRect);
                }
            }
        }

        // 鼠标事件：主图像鼠标松开
        private void ui_MainImage_MouseUp(object sender, MouseEventArgs e)
        {
            feed_dog();
            
            if (null != ui_MainImage.Image)
            {
                if (true == m_bSelectBaseTemplate)
                {
                    if ((m_base_template_lefttop.X + m_base_template_lefttop.Y) > 0)
                    {
                        if (DialogResult.Yes == MessageBox.Show(this, "是否采用该基准模板?", "提示", MessageBoxButtons.YesNo))
                        {
                            create_base_template();

                            ui_MainImage.Refresh();

                            m_bSelectBaseTemplate = false;
                        }

                        return;
                    }
                }

                m_gauger.set_mouse_down_on_string_state(false);
                m_gauger.on_mouse_up(e, ui_MainImage);
                
                // 右键鼠标松开
                if (MouseButtons.Right == e.Button)
                {
                    if (m_gauger.m_bHasValidGaugeResult)
                    {
                        // 处于正在创建任务的状态
                        //Debugger.Log(0, null, string.Format("222222 m_bIsCreatingTask = {0}", m_bIsCreatingTask));
                        if (true == m_bIsCreatingTask)
                        {
                            if (false == m_hBaseTemplateImage.IsInitialized())
                            {
                                MessageBox.Show(this, "尚未创建基准模板！", "提示", MessageBoxButtons.YesNo);

                                m_gauger.clear_gauger_state();
                                return;
                            }

                            #region
                            MeasurePointData mes_pt = new MeasurePointData();

                            mes_pt.m_strStepsFileName = m_strCurrentProductStep;
                            mes_pt.m_strLayerFileName = m_strCurrentProductLayer;

                            if ((null != m_current_measure_graph_item) && ((0 == m_nCreateTaskMode) || (3 == m_nCreateTaskMode)))
                            {
                                mes_pt = m_current_measure_graph_item;

                                if (get_fiducial_mark_count(m_current_task_data) >= 3)
                                    mes_pt.m_center_x_in_metric = 0;                       // 这里置0表明是手动拉出来的测量框
                            }
                            else if ((null != m_current_measure_graph_item) &&(2 == m_nCreateTaskMode))
                                mes_pt = m_current_measure_graph_item;

                            // 判断是否 3个定位孔 还没完成
                            //if (get_fiducial_mark_count(m_current_task_data) < 3)
                            //{
                            //    if ((MEASURE_TYPE.CIRCLE_OUTER_TO_INNER == m_gauger.m_measure_type)
                            //        || (MEASURE_TYPE.HAND_PICK_CIRCLE == m_gauger.m_measure_type))
                            //    {
                            //        Debugger.Log(0, null, string.Format("222222 右键鼠标松开 111"));
                            //        if (DialogResult.Yes == MessageBox.Show(this, "是否采用该定位孔?", "提示", MessageBoxButtons.YesNo))
                            //        {
                            //            Debugger.Log(0, null, string.Format("222222 右键鼠标松开 222"));
                            //            mes_pt.m_mes_type = MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE;
                            //        }
                            //        else
                            //            return;
                            //    }
                            //    else
                            //    {
                            //        MessageBox.Show(this, "请先选定3个定位孔。", "提示");
                            //        return;
                            //    }
                            //}
                            //else
                            //{
                                if (DialogResult.Yes == MessageBox.Show(this, "是否采用并添加该测量结果到任务中?", "提示", MessageBoxButtons.YesNo))
                                    mes_pt.m_mes_type = m_gauger.m_measure_type;
                                else
                                    return;
                            //}
                            mes_pt.m_string_Position_offset = m_gauger.m_string_offset_real;
                            mes_pt.m_ID = m_current_task_data.Count + 1;
                            mes_pt.m_name = get_measure_type_name(mes_pt.m_mes_type);
                            
                            // 记录是手动模式还是图纸模式
                            if (1 == m_nCreateTaskMode)
                                mes_pt.m_create_mode = 0;
                            else if (2 == m_nCreateTaskMode)
                                mes_pt.m_create_mode = 3;
                            else
                                mes_pt.m_create_mode = m_nGraphType;
                            
                            if (2 != m_nCreateTaskMode)
                            {
                                bool bUseStandardValueFromODB = false;
                                if (true == mes_pt.m_bIsFromODBAttribute)
                                {
                                    if (DialogResult.Yes == MessageBox.Show(this, "是否采用ODB图纸标准值?\r\n如需手动输入标准值，请点否。", "提示", MessageBoxButtons.YesNo))
                                        bUseStandardValueFromODB = true;
                                }

                                switch (mes_pt.m_mes_type)
                                {
                                    case MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE:
                                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                                    case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                                    case MEASURE_TYPE.HAND_PICK_CIRCLE:
                                        if (false == bUseStandardValueFromODB)
                                        {
                                            mes_pt.m_metric_radius[0] = m_gauger.m_gauged_circle_radius / m_calib_data[comboBox_Len.SelectedIndex];
                                            mes_pt.m_metric_radius_upper[0] = mes_pt.m_metric_radius[0] * (100 + m_dbUpperDeltaPercent) / 100;
                                            mes_pt.m_metric_radius_lower[0] = mes_pt.m_metric_radius[0] * (100 - m_dbLowerDeltaPercent) / 100;
                                        }
                                        break;

                                    case MEASURE_TYPE.LINE_WIDTH_14:
                                    case MEASURE_TYPE.LINE_WIDTH_23:
                                    case MEASURE_TYPE.LINE_WIDTH_13:
                                    case MEASURE_TYPE.HAND_PICK_LINE:
                                    case MEASURE_TYPE.LINE:
                                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                                    case MEASURE_TYPE.SHORT_SPACE_BETWEEN_TWO_EMPTY_CIRCLES:
                                    case MEASURE_TYPE.L_SHAPE:
                                    case MEASURE_TYPE.BULGE:
                                    case MEASURE_TYPE.ETCH_DOWN:
                                    case MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE:
                                    case MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE:
                                    case MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE:
                                    case MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE:
                                        if (false == bUseStandardValueFromODB)
                                        {
                                            mes_pt.m_metric_line_width[0] = m_gauger.m_gauged_line_width / m_calib_data[comboBox_Len.SelectedIndex];
                                            mes_pt.m_metric_line_width_upper[0] = mes_pt.m_metric_line_width[0] * (100 + m_dbUpperDeltaPercent) / 100;
                                            mes_pt.m_metric_line_width_lower[0] = mes_pt.m_metric_line_width[0] * (100 - m_dbLowerDeltaPercent) / 100;
                                        }
                                        break;
                                    case MEASURE_TYPE.LINE_WIDTH_1234:
                                    case MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR:
                                    case MEASURE_TYPE.LINE_TO_EDGE:
                                        if (false == bUseStandardValueFromODB)
                                        {
                                            mes_pt.m_metric_line_width[0] = m_gauger.m_gauged_line_width / m_calib_data[comboBox_Len.SelectedIndex];
                                            mes_pt.m_metric_line_width_upper[0] = mes_pt.m_metric_line_width[0] * (100 + m_dbUpperDeltaPercent) / 100;
                                            mes_pt.m_metric_line_width_lower[0] = mes_pt.m_metric_line_width[0] * (100 - m_dbLowerDeltaPercent) / 100;
                                            mes_pt.m_metric_line_width[1] = m_gauger.m_gauged_line_width2 / m_calib_data[comboBox_Len.SelectedIndex];
                                            mes_pt.m_metric_line_width_upper[1] = mes_pt.m_metric_line_width[1] * (100 + m_dbUpperDeltaPercent) / 100;
                                            mes_pt.m_metric_line_width_lower[1] = mes_pt.m_metric_line_width[1] * (100 - m_dbLowerDeltaPercent) / 100;
                                        }
                                        break;

                                    case MEASURE_TYPE.LINE_SPACE:
                                    case MEASURE_TYPE.ARC_LINE_SPACE:
                                        if (false == bUseStandardValueFromODB)
                                        {
                                            mes_pt.m_metric_line_width[0] = m_gauger.m_gauged_line_space / m_calib_data[comboBox_Len.SelectedIndex];
                                            mes_pt.m_metric_line_width_upper[0] = mes_pt.m_metric_line_width[0] * (100 + m_dbUpperDeltaPercent) / 100;
                                            mes_pt.m_metric_line_width_lower[0] = mes_pt.m_metric_line_width[0] * (100 - m_dbLowerDeltaPercent) / 100;
                                        }
                                        break;
                                }
                            }
                            
                            m_motion.get_xyz_crds(ref mes_pt.m_theory_machine_crd);
                            //Debugger.Log(0, null, string.Format("222222 m_mes_type.x = [{0:0.000},{1:0.000}]", mes_pt.m_theory_machine_crd.x, mes_pt.m_theory_machine_crd.y));
                            mes_pt.m_len_ratio = comboBox_Len.SelectedIndex;
                            
                            // 计算测量对象中心和主图像中心之间的坐标偏移
                            if (true)
                            {
                                for (int k = 0; k < 4; k++)
                                {
                                    Point2d pt = new Point2d(0, 0);
                                    pt.x = m_gauger.m_ROI_rect[k].x - m_gauger.m_object_center.x;
                                    pt.y = m_gauger.m_ROI_rect[k].y - m_gauger.m_object_center.y;
                                    pt.x = (pt.x / m_calib_data[comboBox_Len.SelectedIndex]) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                                    pt.y = (pt.y / m_calib_data[comboBox_Len.SelectedIndex]) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;
                                    
                                    mes_pt.m_handmade_ROI_rect[k].x = m_gauger.m_ROI_rect[k].x - m_base_template_center_in_image.x;
                                    mes_pt.m_handmade_ROI_rect[k].y = m_gauger.m_ROI_rect[k].y - m_base_template_center_in_image.y;

                                    Debugger.Log(0, null, string.Format("222222 {0}: m_gauger.m_ROI_rect = [{1:0.000},{2:0.000}], m_gauger.m_object_center = [{3:0.000},{4:0.000}]", 
                                        k, m_gauger.m_ROI_rect[k].x, m_gauger.m_ROI_rect[k].y, m_gauger.m_object_center.x, m_gauger.m_object_center.y));
                                }

                                // 手拉框中心
                                mes_pt.m_hand_drawn_center.x = m_gauger.m_object_center.x - m_base_template_center_in_image.x;
                                mes_pt.m_hand_drawn_center.y = m_gauger.m_object_center.y - m_base_template_center_in_image.y;

                                //Debugger.Log(0, null, string.Format("222222 m_object_center = [{0:0.000},{1:0.000}]", m_gauger.m_object_center.x, m_gauger.m_object_center.y));

                                Point2d offset = new Point2d(0, 0);
                                offset.x = m_gauger.m_object_center.x - (double)(ui_MainImage.Image.Width / 2);
                                offset.y = m_gauger.m_object_center.y - (double)(ui_MainImage.Image.Height / 2);
                                offset.x = (offset.x / m_calib_data[comboBox_Len.SelectedIndex]) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                                offset.y = (offset.y / m_calib_data[comboBox_Len.SelectedIndex]) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                                if ((Math.Abs(offset.x) + Math.Abs(offset.y)) > 0.06)
                                {
                                    Point3d crd = new Point3d();
                                    m_motion.get_xyz_crds(ref crd);

                                    crd.x += offset.x;
                                    crd.y += offset.y;

                                    m_motion.linear_XYZ_wait_until_stop(crd.x, crd.y, crd.z, false);
                                }

                                mes_pt.m_theory_machine_crd.x += offset.x;
                                mes_pt.m_theory_machine_crd.y += offset.y;
                                if (mes_pt.m_len_ratio > 0)
                                {
                                    mes_pt.m_theory_machine_crd.x -= m_len_ratios_offsets[mes_pt.m_len_ratio].x;
                                    mes_pt.m_theory_machine_crd.y -= m_len_ratios_offsets[mes_pt.m_len_ratio].y;
                                }
                                mes_pt.m_real_machine_crd = mes_pt.m_theory_machine_crd;

                                if (MEASURE_TYPE.LINE == mes_pt.m_mes_type)
                                {
                                    for (int p = 0; p < 2; p++)
                                    {
                                        offset = ((Gauger_Line)m_gauger).m_gauge_result_pts[p];
                                        offset.x -= (double)(ui_MainImage.Image.Width / 2);
                                        offset.y -= (double)(ui_MainImage.Image.Height / 2);
                                        offset.x = (offset.x / m_calib_data[comboBox_Len.SelectedIndex]) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                                        offset.y = (offset.y / m_calib_data[comboBox_Len.SelectedIndex]) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                                        mes_pt.m_contour_vertex[p].x = mes_pt.m_theory_machine_crd.x + offset.x;
                                        mes_pt.m_contour_vertex[p].y = mes_pt.m_theory_machine_crd.y + offset.y;
                                    }
                                }
                            }
                            
                            mes_pt.m_thres_for_skipping_autofocus = m_nThresForSkippingAutofocus;

                            if (false == m_bOfflineMode)
                            {
                                double sharpness = 0;
                                if (true == Gaugers.ImgOperators.get_image_sharpness(m_main_camera.m_pImageBuf,
                                    m_main_camera.m_nCamWidth, m_main_camera.m_nCamHeight, ref sharpness))
                                    mes_pt.m_sharpness_at_creation = sharpness;
                            }
                            
                            mes_pt.m_unit = m_nUnitType;
                            mes_pt.m_bIsTopLightOn = m_top_light.m_bOn;
                            mes_pt.m_nTopBrightness = m_top_light.m_nBrightness;
                            mes_pt.m_bIsBottomLightOn = m_bottom_light.m_bOn;
                            mes_pt.m_nBottomBrightness = m_bottom_light.m_nBrightness;

                            for (int k = 3; k < m_current_task_data.Count; k++)
                            {
                                Debugger.Log(0, null, string.Format("222222 k {0}: mes_pt.m_theory_machine_crd.x = [{1:0.000},{2:0.000}]", 
                                    k, m_current_task_data[k].m_theory_machine_crd.x, m_current_task_data[k].m_theory_machine_crd.y));
                            }

                            Debugger.Log(0, null, string.Format("222222 mes_pt.m_theory_machine_crd.x = [{0:0.000},{1:0.000}]", mes_pt.m_theory_machine_crd.x,
                                mes_pt.m_theory_machine_crd.y));

                            m_current_task_data.Add(mes_pt);
                            
                            if ((0 != mes_pt.m_create_mode))
                            {
                                // 获取三角变换矩阵
                                if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == mes_pt.m_mes_type)
                                {
                                    // 获取三角变换矩阵
                                    if (get_fiducial_mark_count(m_current_task_data) >= 3)
                                    {
                                        m_triangle_trans_matrix = new double[10];

                                        generate_transform_matrix_by_three_pts(m_current_task_data, ref m_triangle_trans_matrix);

                                        Debugger.Log(0, null, string.Format("222222 图纸模式首件制作过程中找到定位孔 获取三角变换矩阵"));
                                    }

                                    // 更新 图纸区域
                                    //if (true == m_graph_view.m_bHasValidImage)
                                    //{
                                    //    switch (get_fiducial_mark_count(m_current_task_data))
                                    //    {
                                    //        case 1:
                                    //            m_graph_view.set_view_ratio_and_crd(m_graph_view.m_zoom_ratio_min * 7, 0, 0);
                                    //            break;
                                    //        case 2:
                                    //            m_graph_view.set_view_ratio_and_crd(m_graph_view.m_zoom_ratio_min * 7, m_graph_view.m_bitmap_1bit.Width * 6 / 7, 0);
                                    //            break;
                                    //        case 3:
                                    //            m_graph_view.set_view_ratio_and_crd(m_graph_view.m_zoom_ratio_min, 0, 0);

                                    //            if (get_fiducial_mark_count(m_current_task_data) >= 3)
                                    //            {
                                    //                m_triangle_trans_matrix = new double[10];
                                    //                generate_transform_matrix_by_three_pts(m_current_task_data, ref m_triangle_trans_matrix);
                                    //            }
                                    //            break;
                                    //    }
                                    //    m_graph_view.refresh_image();
                                    //}
                                }
                            }
                            
                            // 在 gridview_MeasureTask 上显示刚添加的测量项数据
                            add_entry_to_task_gridview(gridview_MeasureTask, mes_pt);
                            make_gridview_last_row_visible(gridview_MeasureTask);

                            // 弹出标准值设置窗口
                            if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE != mes_pt.m_mes_type)
                                menuitem_ModifyTask_Click(new object(), new EventArgs());

                            m_event_wait_for_confirm_during_creation.Set();
                            m_event_wait_for_manual_gauge.Set();

                            #endregion
                        }

                        // 处于自动测量过程中等待用户确认的状态，或处于挂起等待用户手动测量的状态
                        #region
                        int measure_update_index = 0;
                        if (m_doubleclick_manual_measure == false)
                        {
                            measure_update_index = gridview_measure_results.RowCount - 2;
                        }
                        else if (m_doubleclick_manual_measure == true)
                        {
                            measure_update_index = m_doubleclick_manual_measure_index;
                            m_doubleclick_manual_measure = false;
                        }

                        // 等待手动测量确认
                        if (((true == m_bIsWaitingForConfirm) || (true == m_bIsWaitingForUserManualGauge)) )
                        {
                            globaldata.isHandleFinish = true;
                            MeasurePointData data = m_current_task_data[m_nCurrentMeasureItemIdx];
                            data.m_strStepsFileName = m_strCurrentProductStep;
                            data.m_strLayerFileName = m_strCurrentProductLayer;
                            if (MEASURE_TYPE.FIDUCIAL_MARK_CIRCLE == data.m_mes_type)
                            {
                                switch (m_gauger.m_measure_type)
                                {
                                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                                    case MEASURE_TYPE.HAND_PICK_CIRCLE:
                                        if (DialogResult.Yes == MessageBox.Show(this, "是否采用该定位孔?", "提示", MessageBoxButtons.YesNo))
                                        {
                                            #region
                                            Point3d crd = new Point3d(0, 0, 0);
                                            m_motion.get_xyz_crds(ref crd);

                                            Point2d offset = new Point2d(0, 0);
                                            offset.x = m_gauger.m_gauged_circle_center.x - (double)(ui_MainImage.Image.Width / 2);
                                            offset.y = m_gauger.m_gauged_circle_center.y - (double)(ui_MainImage.Image.Height / 2);

                                            offset.x = (offset.x / m_calib_data[comboBox_Len.SelectedIndex]) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                                            offset.y = (offset.y / m_calib_data[comboBox_Len.SelectedIndex]) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                                            crd.x += offset.x;
                                            crd.y += offset.y;

                                            data.m_real_machine_crd = crd;

                                            double res = m_gauger.m_gauged_circle_radius / m_calib_data[data.m_len_ratio];
                                            gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, measure_update_index].Value = string.Format("{0:0.000}", res);

                                            //Debugger.Log(0, null, string.Format("222222 idx = {0}, offset = [{1:0.000},{2:0.000}]", m_nCurrentMeasureItemIdx, offset.x, offset.y));
                                            //Debugger.Log(0, null, string.Format("222222 real = [{0:0.000},{1:0.000}]", data.m_real_machine_crd.x, data.m_real_machine_crd.y));

                                            m_event_wait_for_confirm_during_autorun.Set();
                                            m_event_wait_for_manual_gauge.Set();

                                            m_bIsWaitingForUserManualGauge = false;
                                            #endregion
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                switch (data.m_mes_type)
                                {
                                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                                    case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                                    case MEASURE_TYPE.HAND_PICK_CIRCLE:
                                        if (DialogResult.Yes == MessageBox.Show(this, "是否采用该测量结果?", "提示", MessageBoxButtons.YesNo))
                                        {
                                            double res = m_gauger.m_gauged_circle_radius / m_calib_data[data.m_len_ratio];
                                            gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, measure_update_index].Value
                                                = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                            if ((res >= data.m_metric_radius_lower[0]) && (res <= data.m_metric_radius_upper[0]))
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Style.BackColor = Color.FromArgb(0, 255, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Value = "OK";
                                            }
                                            else
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Style.BackColor = Color.FromArgb(255, 0, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Value = "NG";
                                            }

                                            m_event_wait_for_confirm_during_autorun.Set();
                                            m_event_wait_for_manual_gauge.Set();

                                            m_bIsWaitingForUserManualGauge = false;
                                            
                                            m_bTriggerSaveGaugeImage = true;
                                         
                                            ui_MainImage.Refresh();
                                        }
                                        break;

                                    case MEASURE_TYPE.LINE_WIDTH_14:
                                    case MEASURE_TYPE.LINE_WIDTH_23:
                                    case MEASURE_TYPE.LINE_WIDTH_13:
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
                                        if (DialogResult.Yes == MessageBox.Show(this, "是否采用该测量结果?", "提示", MessageBoxButtons.YesNo))
                                        {
                                           
                                            double res = m_gauger.m_gauged_line_width / m_calib_data[data.m_len_ratio];
                                            gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, measure_update_index].Value
                                                = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                            if ((MEASURE_TYPE.LINE == data.m_mes_type) && (true == data.m_bIsPartOfComboMeasure))
                                            {

                                            }
                                            else
                                            {
                                                if ((res >= data.m_metric_line_width_lower[0]) && (res <= data.m_metric_line_width_upper[0]))
                                                {
                                                    gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Style.BackColor = Color.FromArgb(0, 255, 0);
                                                    gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Value = "OK";
                                                }
                                                else
                                                {
                                                    gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Style.BackColor = Color.FromArgb(255, 0, 0);
                                                    gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Value = "NG";
                                                }
                                            }

                                            // 纠偏
                                            if (true)
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
                                                        case MEASURE_TYPE.SHORT_SPACE_BETWEEN_TWO_EMPTY_CIRCLES:
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
                                            }
                                            
                                            m_event_wait_for_confirm_during_autorun.Set();
                                            m_event_wait_for_manual_gauge.Set();

                                            // 拷贝测量结果数据
                                            if (true)
                                            {
                                                #region
                                                Gauger gauger = new Gauger_LineWidth(this, ui_MainImage, m_gauger.m_measure_type);

                                                Debugger.Log(0, null, string.Format("222222 data.m_mes_type {0}", data.m_mes_type));

                                                if (MEASURE_TYPE.HAND_PICK_LINE == m_gauger.m_measure_type)
                                                    gauger = new Gauger_HandPickLine(this, ui_MainImage);
                                                else
                                                {
                                                    switch (data.m_mes_type)
                                                    {
                                                        case MEASURE_TYPE.LINE_WIDTH_14:
                                                            gauger = new Gauger_LineWidth(this, ui_MainImage, data.m_mes_type);
                                                            break;
                                                        case MEASURE_TYPE.LINE_WIDTH_23:
                                                            gauger = new Gauger_LineWidth(this, ui_MainImage, data.m_mes_type);
                                                            break;
                                                        case MEASURE_TYPE.L_SHAPE:
                                                            gauger = new Gauger_LShapeItem(this, ui_MainImage, data.m_mes_type);
                                                            break;
                                                        case MEASURE_TYPE.ETCH_DOWN:
                                                            gauger = new Gauger_EtchDown(this, ui_MainImage, data.m_mes_type);
                                                            break;
                                                        case MEASURE_TYPE.BULGE:
                                                            gauger = new Gauger_Bulge(this, ui_MainImage, data.m_mes_type);
                                                            break;
                                                        case MEASURE_TYPE.LINE_WIDTH_BY_CONTOUR:
                                                            gauger = new Gauger_LineWidthByContour(this, ui_MainImage, data.m_mes_type);
                                                            break;
                                                        case MEASURE_TYPE.LINE_TO_EDGE:
                                                            gauger = new Gauger_LineToEdge(this, ui_MainImage, data.m_mes_type);
                                                            break;
                                                        case MEASURE_TYPE.LINE_WIDTH_13:
                                                            gauger = new Gauger_LineWidth(this, ui_MainImage, data.m_mes_type);
                                                            break;
                                                        case MEASURE_TYPE.LINE_WIDTH_1234:
                                                            gauger = new Gauger_LineWidth(this, ui_MainImage, data.m_mes_type);
                                                            break;
                                                        case MEASURE_TYPE.LINE_SPACE:
                                                            gauger = new Gauger_LineSpace(this, ui_MainImage, data.m_mes_type);
                                                            break;
                                                        case MEASURE_TYPE.ARC_LINE_SPACE:
                                                            gauger = new Gauger_ArcLineSpace(this, ui_MainImage, data.m_mes_type);
                                                            break;
                                                        case MEASURE_TYPE.HAND_PICK_LINE:
                                                            gauger = new Gauger_HandPickLine(this, ui_MainImage);
                                                            break;
                                                        case MEASURE_TYPE.HAND_DRAWN_HORIZON_PARALLEL_LINE_TO_LINE:
                                                            gauger = new Gauger_HandDrawnHorizonParallelLineToLine(this, ui_MainImage);
                                                            break;
                                                        case MEASURE_TYPE.HAND_DRAWN_VERTICAL_PARALLEL_LINE_TO_LINE:
                                                            gauger = new Gauger_HandDrawnVerticalParallelLineToLine(this, ui_MainImage);
                                                            break;
                                                        case MEASURE_TYPE.HAND_DRAWN_HORIZON_POINT_TO_LINE:
                                                            gauger = new Gauger_HandDrawnHorizonParallelPointToLine(this, ui_MainImage);
                                                            break;
                                                        case MEASURE_TYPE.HAND_DRAWN_VERTICAL_POINT_TO_LINE:
                                                            gauger = new Gauger_HandDrawnVerticalParallelPointToLine(this, ui_MainImage);
                                                            break;
                                                        case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                                                            gauger = new Gauger_CircleOuterToInner(this, ui_MainImage);
                                                            break;
                                                        case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                                                            gauger = new Gauger_CircleInnerToOuter(this, ui_MainImage);
                                                            break;
                                                        case MEASURE_TYPE.HAND_PICK_CIRCLE:
                                                            gauger = new Gauger_HandPickCircle(this, ui_MainImage);
                                                            break;
                                                    }
                                                }

                                                m_gauger.copy_measure_result_data(ref gauger);
                                                gauger.m_bIsClonedObject = true;

                                                //Thread.Sleep(1000);
                                                Debugger.Log(0, null, string.Format("222222 ...........................m_vec_history_gaugers.Add(gauger);"));
                                                m_vec_history_gaugers.Add(gauger);
                                                #endregion
                                            }

                                            m_bIsWaitingForUserManualGauge = false;
                                            m_bShowSelectionFrameOnNG = false;

                                            m_bTriggerSaveGaugeImage = true;
                                            ui_MainImage.Refresh();
                                          
                                        }
                                        break;
                                    #endregion
                                    case MEASURE_TYPE.LINE_WIDTH_1234:
                                        if (DialogResult.Yes == MessageBox.Show(this, "是否采用该测量结果?", "提示", MessageBoxButtons.YesNo))
                                        {
                                            double res = m_gauger.m_gauged_line_width / m_calib_data[data.m_len_ratio];
                                            gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, measure_update_index].Value
                                                = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                            res = m_gauger.m_gauged_line_width2 / m_calib_data[data.m_len_ratio];
                                            gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, measure_update_index + 1].Value
                                                = string.Format("{0:0.000}", GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                            m_event_wait_for_confirm_during_autorun.Set();
                                            m_event_wait_for_manual_gauge.Set();

                                            m_bIsWaitingForUserManualGauge = false;

                                            m_bTriggerSaveGaugeImage = true;
                                            ui_MainImage.Refresh();
                                        }
                                        break;

                                    case MEASURE_TYPE.LINE_SPACE:
                                    case MEASURE_TYPE.ARC_LINE_SPACE:
                                        if (DialogResult.Yes == MessageBox.Show(this, "是否采用该测量结果?", "提示", MessageBoxButtons.YesNo))
                                        {
                                            double res = m_gauger.m_gauged_line_space / m_calib_data[data.m_len_ratio];
                                            gridview_measure_results[RESULT_COLUMN_GAUGED_VALUE, measure_update_index].Value = string.Format("{0:0.000}",
                                                GeneralUtils.convert_um_value_by_unit(res, data.m_unit));

                                            if ((res >= data.m_metric_line_width_lower[0]) && (res <= data.m_metric_line_width_upper[0]))
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Style.BackColor = Color.FromArgb(0, 255, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Value = "OK";
                                            }
                                            else
                                            {
                                                gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Style.BackColor = Color.FromArgb(255, 0, 0);
                                                gridview_measure_results[RESULT_COLUMN_OKNG, measure_update_index].Value = "NG";
                                            }

                                            m_event_wait_for_confirm_during_autorun.Set();
                                            m_event_wait_for_manual_gauge.Set();

                                            m_bIsWaitingForUserManualGauge = false;

                                            m_bTriggerSaveGaugeImage = true;
                                            ui_MainImage.Refresh();
                                        }
                                        break;
                                }
                            }
                        }
                        #endregion
                        
                        // 检查是否处于标定状态
                        if ((false == m_bIsCreatingTask) && (false == m_bIsWaitingForConfirm))
                        {
                            Form form = null;
                            if (true == GeneralUtils.check_if_form_is_open("Form_Calibration", ref form))
                            {
                                if (DialogResult.Yes == MessageBox.Show(form, "是否采用该标定结果?", "提示", MessageBoxButtons.YesNo))
                                {
                                    Form_Calibration form_calib = (Form_Calibration)form;
                                    form_calib.add_calib_result(m_gauger.m_gauged_line_width);
                                }
                            }
                        }
                    }
                    else
                    {
                        int nTimeGap = GetTickCount() - m_gauger.m_nLastFailureTime;
                        if (nTimeGap > 100)
                        {
                            //Debugger.Log(0, null, string.Format("222222 {0}, {1}, {2}", m_gauger.m_nLastFailureTime, GetTickCount(), nTimeGap));

                          if(!globaldata.isRun)  menu_MainImage.Show(ui_MainImage.Location.X + e.X, ui_MainImage.Location.Y + e.Y + 25);
                        }
                    }
                }
            }
        }

        // 鼠标事件：主图像鼠标滚动
        private void ui_MainImage_MouseWheel(object sender, MouseEventArgs e)
        {
            if (null != m_gauger)
                m_gauger.on_mouse_wheel(e, ui_MainImage, m_bSetHorizonModeForGaugerRect);
        }

        // 鼠标事件：主图像鼠标移出
        private void ui_MainImage_MouseLeave(object sender, EventArgs e)
        {
            if (null != m_gauger)
                m_gauger.on_mouse_leave(e, ui_MainImage);
        }

        // 主图像鼠标双击事件
        private void ui_MainImage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left == e.Button)
            {
                if (null != m_gauger)
                    m_gauger.on_mouse_leave(e, ui_MainImage);

                if (false == m_motion.m_bHomed)
                    return;

                m_gauger.clear_gauger_state();

                if (true == m_bUseHeightSensor)
                {
                    //move_height_sensor_to_camera_pos_and_detect_height();
                }

                m_image_operator.do_auto_focus();
            }
        }

        // 主图像鼠标滚轮单击/按下事件
        private void ui_MainImage_MouseClick(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Middle == e.Button)
            {
                if (false == m_motion.m_bHomed)
                    return;
                if (null == ui_MainImage.Image)
                    return;

                m_gauger.clear_gauger_state();

                double factor_x = (double)ui_MainImage.Width / (double)ui_MainImage.Image.Width;
                double factor_y = (double)ui_MainImage.Height / (double)ui_MainImage.Image.Height;

                Point2d offset = new Point2d(e.X - (ui_MainImage.Width / 2), e.Y - (ui_MainImage.Height / 2));
                offset.x = (offset.x / (m_calib_data[m_len.m_nRatio] * factor_x)) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                offset.y = (offset.y / (m_calib_data[m_len.m_nRatio] * factor_y)) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                Point3d current_crd = new Point3d(0, 0, 0);
                m_motion.get_xyz_crds(ref current_crd);

                Point3d dest_crd = new Point3d(current_crd.x + offset.x, current_crd.y + offset.y, current_crd.z);

                //Debugger.Log(0, null, string.Format("222222 offset = [{0},{1}]", offset.x, offset.y));

                m_motion.linear_XYZ_wait_until_stop(dest_crd.x, dest_crd.y, dest_crd.z, 50, 0.125, false);
            }
        }

        // 导航图像鼠标双击事件
        private void ui_GuideImage_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Left == e.Button)
            {
                if (false == m_motion.m_bHomed)
                    return;
                if (null == ui_GuideImage.Image)
                    return;

                m_gauger.clear_gauger_state();

                double factor_x = (double)ui_GuideImage.Width / (double)ui_GuideImage.Image.Width;
                double factor_y = (double)ui_GuideImage.Height / (double)ui_GuideImage.Image.Height;

                Point2d offset = new Point2d(e.X - (ui_GuideImage.Width / 2), e.Y - (ui_GuideImage.Height / 2));
                offset.x = (offset.x / (m_guide_camera.m_pixels_per_um * factor_x)) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                offset.y = (offset.y / (m_guide_camera.m_pixels_per_um * factor_y)) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                Point3d current_crd = new Point3d(0, 0, 0);
                m_motion.get_xyz_crds(ref current_crd);

                Point3d dest_crd = new Point3d(current_crd.x + offset.x, current_crd.y + offset.y, current_crd.z);

                //Debugger.Log(0, null, string.Format("222222 offset = [{0},{1}]", offset.x, offset.y));

                m_motion.linear_XYZ_wait_until_stop(dest_crd.x, dest_crd.y, dest_crd.z, 50, 0.25, false);
            }
        }

        // 导航图像鼠标滚轮单击/按下事件
        private void ui_GuideImage_MouseClick(object sender, MouseEventArgs e)
        {
            if (MouseButtons.Middle == e.Button)
            {
                if (false == m_motion.m_bHomed)
                    return;
                if (null == ui_GuideImage.Image)
                    return;

                m_gauger.clear_gauger_state();

                double factor_x = (double)ui_GuideImage.Width / (double)ui_GuideImage.Image.Width;
                double factor_y = (double)ui_GuideImage.Height / (double)ui_GuideImage.Image.Height;

                Point2d offset = new Point2d(e.X - (ui_GuideImage.Width / 2), e.Y - (ui_GuideImage.Height / 2));
                offset.x = (offset.x / (m_guide_camera.m_pixels_per_um * factor_x)) * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                offset.y = (offset.y / (m_guide_camera.m_pixels_per_um * factor_y)) * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                Point3d current_crd = new Point3d(0, 0, 0);
                m_motion.get_xyz_crds(ref current_crd);

                Point3d dest_crd = new Point3d(current_crd.x + offset.x, current_crd.y + offset.y, current_crd.z);

                //Debugger.Log(0, null, string.Format("222222 offset = [{0},{1}]", offset.x, offset.y));

                m_motion.linear_XYZ_wait_until_stop(dest_crd.x, dest_crd.y, dest_crd.z, 50, 0.125, false);
            }
        }

        // 鼠标事件：双击小地图
        private void pictureBox_MotionPad_MouseDoubleClick(object sender, MouseEventArgs me)
        {
            if (MouseButtons.Left == me.Button)
            {
                m_gauger.clear_gauger_state();

                double click_x = me.X;
                double click_y = me.Y;
                double pic_width = pictureBox_MotionPad.Width;
                double pic_height = pictureBox_MotionPad.Height;
                double ratio_x = click_x / pic_width;
                double ratio_y = click_y / pic_height;

                Point2d leftbottom_crd = m_motion.m_pad_leftbottom_crd;
                Point2d righttop_crd = m_motion.m_pad_righttop_crd;

                double pos_x = leftbottom_crd.x + (righttop_crd.x - leftbottom_crd.x) * ratio_x;
                double pos_y = righttop_crd.y + (leftbottom_crd.y - righttop_crd.y) * ratio_y;

                //Debugger.Log(0, null, string.Format("222222 点击位置 = {0:0.000},{1:0.000}, pos_x = [{2:0.000},{3:0.000}]", ratio_x, ratio_y, pos_x, pos_y));

                m_motion.linear_XYZ_wait_until_stop(pos_x, pos_y, m_current_xyz.z, false);
            }
        }

        // 组合框：改变镜头倍率
        private void comboBox_Len_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((false == m_bOfflineMode) && (true == m_len.m_bInitialized))
            {
                m_len.set_ratio(comboBox_Len.SelectedIndex);
                m_len.m_nRatio = comboBox_Len.SelectedIndex;
            }
        }

        // 组合框：改变单位
        private void comboBox_Unit_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_nUnitType = comboBox_Unit.SelectedIndex;
        }

        // 组合框：寸动速率
        private void comboBox_JogSpeedRatio_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_nJogSpeedRatio = comboBox_JogSpeedRatio.SelectedIndex;
            textBox_KeyInfo.Focus();
        }

        // 组合框：产品类型
        private void comboBox_ProductType_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_nProductType = comboBox_ProductType.SelectedIndex;
        }

        // Tab
        private void tabControl_Task_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.tabControl_Task.SelectedIndex)
            {
                case 0:
                    gridview_MeasureTask.Show();
                    gridview_GraphMeasureItems.Hide();
                    gridview_measure_results.Hide();
                    break;
                case 1:
                    gridview_MeasureTask.Hide();
                    gridview_GraphMeasureItems.Show();
                    gridview_measure_results.Hide();
                    break;
                case 2:
                    gridview_MeasureTask.Hide();
                    gridview_GraphMeasureItems.Hide();
                    gridview_measure_results.Show();
                    break;
            }
        }
        
        // 勾选框：确认定位孔
        private void checkBox_ConfirmFiducialMark_CheckedChanged(object sender, EventArgs e)
        {
            m_bNeedConfirmFiducialMark = this.checkBox_ConfirmFiducialMark.Checked;
        }

        // 勾选框：确认测量结果
        private void checkBox_ConfirmMeasureResult_CheckedChanged(object sender, EventArgs e)
        {
            m_bNeedConfirmMeasureResult = this.checkBox_ConfirmMeasureResult.Checked;
        }

        // 勾选框：任务NG确认
        private void checkBox_ConfirmNGWhenRunningTask_CheckedChanged(object sender, EventArgs e)
        {
            m_bNeedConfirmNGWhenRunningTask = this.checkBox_ConfirmNGWhenRunningTask.Checked;
        }

        // 勾选框：做首件时自动调节光源
        private void checkBox_AutoAdjustLightDuringTaskCreation_CheckedChanged(object sender, EventArgs e)
        {
            m_bAutoAdjustLightDuringTaskCreation = this.checkBox_AutoAdjustLightDuringTaskCreation.Checked;
        }

        // 勾选框：跑任务时锁定光源，不按首件亮度
        private void checkBox_LockLightWhenRunningTask_CheckedChanged(object sender, EventArgs e)
        {
            m_bLockLightWhenRunningTask = this.checkBox_AutoAdjustLightDuringTaskCreation.Checked;
        }

        // 勾选框：陶瓷板、白底板
        private void checkBox_SpecialAlgorithm_CheckedChanged(object sender, EventArgs e)
        {
            if (true == checkBox_SpecialAlgorithm.Checked)
                m_nAlgorithm = 1;
            else
                m_nAlgorithm = 0;
        }

        // 勾选框：启用自动对焦
        private void checkBox_UseAutofocus_CheckedChanged(object sender, EventArgs e)
        {
            m_bUseAutofocusWhenRunningTask = this.checkBox_UseAutofocus.Checked;
        }

        // 勾选框：开启/关闭吸附
        private void checkBox_Vacuum_CheckedChanged(object sender, EventArgs e)
        {
            if (true == m_bOfflineMode)
                return;
            
            if (true == checkBox_Vacuum.Checked)
            {
                hardware_ops_enable_vacuum(true);
            }
            else
            {
                hardware_ops_enable_vacuum(false);
            }
        }

        // 勾选框：启用循环测量
        private void checkBox_RepeatRunningTask_CheckedChanged(object sender, EventArgs e)
        {
            if (true == m_bIsAppInited)
            {
                m_bRepeatRunningTask = checkBox_RepeatRunningTask.Checked;
            }
        }

        // 勾选框：测量框水平模式
        private void checkBox_SetHorizonModeForGaugerRect_CheckedChanged(object sender, EventArgs e)
        {
            m_bSetHorizonModeForGaugerRect = checkBox_SetHorizonModeForGaugerRect.Checked;
        }

        // 勾选框：批量测量时逐张确认效果
        private void checkBox_ConfirmEachImageResultDuringBatch_CheckedChanged(object sender, EventArgs e)
        {
            m_bConfirmEachImageResultDuringBatch = checkBox_ConfirmEachImageResultDuringBatch.Checked;
        }

        // 按钮：继续下一张
        private void btn_ProceedToNextImage_Click(object sender, EventArgs e)
        {
            m_event_wait_for_next_image.Set();
        }

        // 文本框：循环测量次数
        private void textBox_NumOfTaskRepetitions_TextChanged(object sender, EventArgs e)
        {
            if (true == m_bIsAppInited)
                m_nNumOfTaskRepetition = Convert.ToInt32(textBox_NumOfTaskRepetitions.Text);
        }

        // 键盘事件：键盘按下
        private void MainUI_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keys.ControlKey == e.KeyCode)
                m_bIsCtrlKeyPressed = true;
            
            if ((Keys.Left == e.KeyCode) || (Keys.Right == e.KeyCode) || (Keys.Up == e.KeyCode) || (Keys.Down == e.KeyCode))
            {
                if (true == gridview_MeasureTask.Focused)
                    return;

                if (true == m_bJogAxis)
                    return;
                m_bJogAxis = true;

                if (true == m_bIsCtrlKeyPressed)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Down:
                            btn_Z_down_MouseDown(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                            break;
                        case Keys.Up:
                            btn_Z_up_MouseDown(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                            break;
                    }
                }
                else
                {
                    switch (e.KeyCode)
                    {
                        case Keys.Left:
                            btn_Left_MouseDown(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                            break;
                        case Keys.Right:
                            btn_Right_MouseDown(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                            break;
                        case Keys.Down:
                            btn_Backward_MouseDown(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                            break;
                        case Keys.Up:
                            btn_Forward_MouseDown(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                            break;
                    }
                }
            }
        }

        // 键盘事件：键盘松开
        private void MainUI_KeyUp(object sender, KeyEventArgs e)
        {
            if (Keys.Home == e.KeyCode)
            {
                m_graph_view.init_params(m_graph_view.m_zoom_ratio_min, m_graph_view.m_zoom_ratio_min, m_graph_view.m_zoom_ratio_min);
                m_graph_view.refresh_image();
            }

            if (Keys.ControlKey == e.KeyCode)
                m_bIsCtrlKeyPressed = false;

            if ((Keys.Left == e.KeyCode) || (Keys.Right == e.KeyCode) || (Keys.Up == e.KeyCode) || (Keys.Down == e.KeyCode))
            {
                m_bJogAxis = false;
                btn_Left_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                btn_Right_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                btn_Forward_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                btn_Backward_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                btn_Z_down_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
                btn_Z_up_MouseUp(new object(), new MouseEventArgs(new MouseButtons(), 0, 0, 0, 0));
            }
        }
        
        // 事件：恢复出厂设置
        private bool restore_factory_default()
        {
            // 主类参数
            #region
            APP_Params param = new APP_Params();

            m_nSmallSearchFrameExtent = param.m_small_search_frame_extent_value;
            m_nBigSearchFrameExtent = param.m_big_search_frame_extent_value;
            m_nGraphZoomRatio = param.m_graph_zoom_ratio;
            m_nGraphOrientation = param.m_graph_orientation;
            m_nCreateTaskMode = param.m_create_task_mode;

            m_nJogSpeedRatio = param.m_create_task_mode;
            m_bSetHorizonModeForGaugerRect = param.m_bSetHorizonModeForGaugerRect;
            m_bNeedConfirmFiducialMark = param.m_bNeedConfirmFiducialMark;
            m_bNeedConfirmMeasureResult = param.m_bNeedConfirmMeasureResult;
            m_bNeedConfirmNGWhenRunningTask = param.m_bNeedConfirmNGWhenRunningTask;
            m_bAbsoluteAllowance = param.m_bAbsoluteAllowance;
            m_pcb_alignment_offset.x = param.m_pcb_alignment_offset_x;
            m_pcb_alignment_offset.y = param.m_pcb_alignment_offset_y;

            m_strTaskFileSavingDir = param.m_str_task_file_saving_dir;
            m_strImageSavingDir = param.m_str_image_saving_dir;
            m_strExcelSavingDir = param.m_str_excel_saving_dir;
            m_strGraphBrowseDir = param.m_str_graph_browse_dir;
            m_strOfflineFileBrowseDir = param.m_str_offline_file_browse_dir;
            m_strOpenImageBrowseDir = param.m_str_open_image_browse_dir;
            m_strSaveImageBrowseDir = param.m_str_save_image_browse_dir;
            m_bSaveGaugeResultImage = param.m_bSaveGaugeResultImage;

            m_bSaveGaugeResultExcelReport = param.m_bSaveGaugeResultExcelReport;
            m_bOfflineMode = param.m_bOfflineMode;
            Form_Database.m_strDataSource = param.m_str_data_source;
            Form_Database.m_strDatabaseTask = param.m_str_database_task;
            Form_Database.m_strDatabaseStdLib = param.m_str_database_stdlib;
            Form_Database.m_strSQLUser = param.m_str_SQL_user;
            Form_Database.m_strSQLPwd = param.m_str_SQL_pwd;

            m_nUnitType = param.m_unit;
            for (int n = 0; n < m_nMeasureResultDigits.Length; n++)
                m_nMeasureResultDigits[n] = param.m_measure_result_digits[n];
            m_nTaskInfoSourceType = param.m_task_info_source_type;
            m_bUseRedDot = param.m_use_red_dot;
            m_dbRedDotOffsetX = param.m_red_dot_offset_x;
            m_dbRedDotOffsetY = param.m_red_dot_offset_y;

            m_nMeasureTaskDelayTime = param.m_measure_task_delay_time;
            m_bUseHeightSensor = param.m_use_height_sensor;
            m_dbStageHeightGap = param.m_stage_height_gap;
            m_dbStageTriggerHeight = param.m_stage_trigger_height;
            m_bSelectivelySkipAutofocus = param.m_selectively_skip_autofocus;
            m_nThresForSkippingAutofocus = param.m_thres_for_skipping_autofocus;
            
            m_bDoNotConfirmMeasureItemAtCreation = param.m_do_not_confirm_measure_item_at_creation;
            m_bDoNotConfirmMarkAtCreation = param.m_do_not_confirm_mark_at_creation;
            m_nMainCamUpperBrightness = param.m_nMainCamUpperBrightness;
            m_nMainCamLowerBrightness = param.m_nMainCamLowerBrightness;
            m_nGuideCamUpperBrightness = param.m_nGuideCamUpperBrightness;
            m_nGuideCamLowerBrightness = param.m_nGuideCamLowerBrightness;
            m_nLightTypeForGuideCamForMarkPt = param.m_nLightTypeForGuideCamForMarkPt;
            m_nLightTypeFor14Line = param.m_nLightTypeFor14Line;
            m_nLightTypeForLineSpace = param.m_nLightTypeForLineSpace;
            m_nLightTypeForBGA = param.m_nLightTypeForBGA;
            #endregion
            SaveAppParams();

            //m_IO.save_default();
            //m_len.save_default();
            m_motion.save_default();
            m_main_camera.save_default();
            m_guide_camera.save_default();

            return true;
        }
    }
}
