using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;

namespace ZWLineGauger
{
    public partial class Form_CreateTaskFinished : Form
    {
        MainUI parent;

        public Form_CreateTaskFinished(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();

            //待改
            if (parent.m_strCurrentTaskName.Length > 0)
                this.textBox_TaskName.Text = parent.m_strCurrentTaskName;
            else if (MainUI.m_strGraphFilePath.Length > 0)
                this.textBox_TaskName.Text = Path.GetFileName(MainUI.m_strGraphFilePath);

            textBox_TaskName.Focus();
        }

        // 按钮：确定
        private void btn_OK_Click(object sender, EventArgs e)
        {
            MainUI.dl_message_sender CBD_SendMessage = parent.CBD_SendMessage;

            parent.m_strCurrentTaskName = this.textBox_TaskName.Text;
            if (parent.m_strCurrentTaskName.Length > 0)
            {
                if (true == parent.save_task_to_file_for_Chenling(parent.m_current_task_data, parent.m_strCurrentTaskName))
                {
                    MessageBox.Show(this, "任务保存成功。", "提示", MessageBoxButtons.OK);

                    CBD_SendMessage(string.Format("任务“{0}”成功创建并写入数据库", parent.m_strCurrentTaskName), true, null, null);

                    this.Close();
                }
            }

            return;

            // 如果测量点集所包含定位孔的数目少于3个，则视为无效任务，不予处理
            Debugger.Log(0, null, string.Format("222222 parent.m_strCurrentTaskName = {0}", parent.m_strCurrentTaskName));
            if (parent.get_fiducial_mark_count(parent.m_current_task_data) >= 3)
            {
                // 珠海方正
                #region
                if (parent.m_customer == MainUI.enum_customer.customer_fangzheng_F3)
                {
                    if (2 == parent.m_current_task_data[0].m_create_mode)
                    {
                        string strGraphFileName = Path.GetFileName(MainUI.m_strGraphFilePath);
                        if (strGraphFileName.LastIndexOf(".") != -1)
                        {
                            string strOdbFileName = strGraphFileName.Substring(0, strGraphFileName.LastIndexOf("."));
                            string strLayerName = parent.m_form_orientation.m_strLayerName;
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
                                    for (int n = 0; n < parent.m_list_three_marks_records.Count; n++)
                                    {
                                        if (strOdbFileName == parent.m_list_three_marks_records[n].m_strOdbFileName)
                                        {
                                            if ((nLayerIdx % 2) == parent.m_list_three_marks_records[n].m_nEvenOddFlag)
                                            {
                                                bIsOnList = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (false == bIsOnList)
                                    {
                                        ThreeMarksRecord record = new ThreeMarksRecord();
                                        record.m_strOdbFileName = strOdbFileName;
                                        record.m_nEvenOddFlag = nLayerIdx % 2;

                                        for (int n = 0; n < 3; n++)
                                        {
                                            record.m_dbDiameterInMM[n] = parent.m_current_task_data[n].m_metric_radius[0];

                                            record.m_bIsTopLightOn[n] = parent.m_current_task_data[n].m_bIsTopLightOn;
                                            record.m_bIsBottomLightOn[n] = parent.m_current_task_data[n].m_bIsBottomLightOn;
                                            record.m_nTopBrightness[n] = parent.m_current_task_data[n].m_nTopBrightness;
                                            record.m_nBottomBrightness[n] = parent.m_current_task_data[n].m_nBottomBrightness;
                                            
                                            record.m_marks_pt_on_graph[n].x = parent.m_current_task_data[n].m_center_x_in_metric;
                                            record.m_marks_pt_on_graph[n].y = parent.m_current_task_data[n].m_center_y_in_metric;

                                            record.m_marks_pt_on_stage[n] = parent.m_current_task_data[n].m_theory_machine_crd;
                                        }
                                        
                                        parent.m_list_three_marks_records.Add(record);

                                        parent.save_three_marks_records_to_file(parent.m_list_three_marks_records, "three_marks_records.txt");
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                parent.m_strCurrentTaskName = this.textBox_TaskName.Text;
                if (parent.m_strCurrentTaskName.Length > 0)
                {
                    // 保存到数据库
                    //parent.create_table_and_save_task_to_table(parent.m_SQL_conn_measure_task, parent.m_current_task_data, parent.m_strCurrentTaskName);
                    
                    // 保存到文件
                    if (true == parent.save_task_to_file(parent.m_current_task_data, parent.m_strCurrentTaskName))
                    {
                        CBD_SendMessage(string.Format("任务“{0}”成功创建并写入数据库", parent.m_strCurrentTaskName), true, null, null);
                    }

                    parent.m_bIsLoadedByBrowsingDir = false;
                    parent.m_strCurrentTaskFileFullPath = "";
                }
            }
            else
            {
                MessageBox.Show(this, "任务数据有误，无法保存任务，请检查原因。", "提示", MessageBoxButtons.OK);

                parent.CBD_SendMessage(string.Format("任务 “{0}” 创建失败", parent.m_strCurrentTaskName), true, null, null);
            }

            this.Close();
        }

        // 按钮：取消
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 按钮：中止创建
        private void btn_TerminateCreation_Click(object sender, EventArgs e)
        {
            parent.CBD_SendMessage(string.Format("中止创建任务"), true, null, null);
            this.Close();
        }

        private void Form_CreateTaskFinished_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }

        private void btn_Browse_Click(object sender, EventArgs e)
        {
            bool bHasDefaultDir = false;
            if ("" != parent.m_strTaskFileSavingDir)
            {
                if ((parent.m_strTaskFileSavingDir.Length > 0) && (Directory.Exists(parent.m_strTaskFileSavingDir)))
                    bHasDefaultDir = true;
            }

            SaveFileDialog dlg = new SaveFileDialog();
            if (bHasDefaultDir)
                dlg.InitialDirectory = parent.m_strTaskFileSavingDir;
            else
                dlg.InitialDirectory = System.Environment.CurrentDirectory + "\\任务文件";
            dlg.Filter = "任务文件|*.dat";
            dlg.ShowDialog();
            if (dlg.FileName != string.Empty)
            {
                //parent.m_strTaskFileSavingDir = System.IO.Path.GetDirectoryName(dlg.FileName);
                
                if (parent.get_fiducial_mark_count(parent.m_current_task_data) >= 3)
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                    
                    // 保存到数据库
                    //parent.create_table_and_save_task_to_table(parent.m_SQL_conn_measure_task, parent.m_current_task_data, name);
                    
                    // 保存到文件
                    if (true == parent.save_task_to_file(parent.m_current_task_data, "", dlg.FileName, false, true))
                    {
                        this.Close();
                        MessageBox.Show(parent, "任务保存成功。", "提示", MessageBoxButtons.OK);

                        parent.m_bIsLoadedByBrowsingDir = true;
                        parent.m_strCurrentTaskFileFullPath = dlg.FileName;
                    }
                }
                else
                {
                    MessageBox.Show(this, "任务数据有误，无法保存任务，请检查原因。", "提示", MessageBoxButtons.OK);

                    parent.CBD_SendMessage(string.Format("任务 “{0}” 创建失败", parent.m_strCurrentTaskName), true, null, null);
                }
            }
        }
    }
}
