using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZWLineGauger.Forms
{
    public partial class Form_LoadTask : Form
    {
        MainUI parent;

        List<string> m_vec_task_names = new List<string>();
        
        public Form_LoadTask(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();

            m_vec_task_names.Clear();

            // 读取数据库里面的任务名
            parent.m_vec_SQL_table_names.Clear();
            parent.get_task_tables_from_database(parent.m_vec_SQL_table_names);

            comboBox_TaskInfoSource.Items.Add("数据库");
            comboBox_TaskInfoSource.Items.Add("文件目录");
            comboBox_TaskInfoSource.SelectedIndex = parent.m_nTaskInfoSourceType;

            // 读取任务目录下的任务名
            #region
            if (1 == parent.m_nTaskInfoSourceType)
            {
                if ("" == parent.m_strTaskFileSavingDir)
                    label_SourceDir.Text = Directory.GetCurrentDirectory() + "\\任务文件";
                else
                    label_SourceDir.Text = parent.m_strTaskFileSavingDir;

                if (0 == m_vec_task_names.Count)
                {
                    string dir = "";
                    if ("" == parent.m_strTaskFileSavingDir)
                        dir = "任务文件";
                    else
                    {
                        if (!Directory.Exists(parent.m_strTaskFileSavingDir))
                            dir = "任务文件";
                        else
                            dir = parent.m_strTaskFileSavingDir;
                    }

                    DirectoryInfo info = new DirectoryInfo(dir);
                    if (info.Exists)
                    {
                        foreach (FileInfo file_info in info.GetFiles())
                        {
                            m_vec_task_names.Add(Path.GetFileNameWithoutExtension(file_info.Name));
                        }
                    }
                }
            }
            else
            {
                label_SourceDir.Text = "";
            }
            #endregion
            
            textBox_Input.Focus();
        }

        // 按钮：确定
        private void btn_OK_Click(object sender, EventArgs e)
        {
            parent.m_nTaskInfoSourceType = comboBox_TaskInfoSource.SelectedIndex;
            parent.SaveAppParams();

            if (textBox_Input.Text.Length > 0)
            {
                bool bExist = false;
                if (0 == comboBox_TaskInfoSource.SelectedIndex)
                {
                    for (int n = 0; n < parent.m_vec_SQL_table_names.Count; n++)
                    {
                        if (textBox_Input.Text == parent.m_vec_SQL_table_names[n])
                        {
                            bExist = true;
                            break;
                        }
                    }
                }
                else if (1 == comboBox_TaskInfoSource.SelectedIndex)
                {
                    for (int n = 0; n < m_vec_task_names.Count; n++)
                    {
                        if (textBox_Input.Text == m_vec_task_names[n])
                        {
                            bExist = true;
                            break;
                        }
                    }
                }

                if (true == bExist)
                {
                    parent.m_strCurrentTaskName = textBox_Input.Text;

                    MainUI.dl_message_sender send_message = parent.CBD_SendMessage;
                    send_message("加载任务", false, comboBox_TaskInfoSource.SelectedIndex, label_SourceDir.Text);
                    
                    this.Close();
                }
                else
                {
                    MessageBox.Show(this, "输入任务不存在！", "提示");
                    textBox_Input.Focus();
                    check_textbox_changed();
                }
            }
        }

        // 按钮：取消
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void check_textbox_changed()
        {
            if (textBox_Input.Text.Length > 0)
            {
                comboBox_ShowTasks.Items.Clear();

                List<string> matched_names = new List<string>();

                if (0 == comboBox_TaskInfoSource.SelectedIndex)
                {
                    for (int n = 0; n < parent.m_vec_SQL_table_names.Count; n++)
                    {
                        if (parent.m_vec_SQL_table_names[n].Contains(textBox_Input.Text))
                            matched_names.Add(parent.m_vec_SQL_table_names[n]);
                    }
                }
                else if (1 == comboBox_TaskInfoSource.SelectedIndex)
                {
                    for (int n = 0; n < m_vec_task_names.Count; n++)
                    {
                        if (m_vec_task_names[n].Contains(textBox_Input.Text))
                            matched_names.Add(m_vec_task_names[n]);
                    }
                }

                if (matched_names.Count > 0)
                {
                    for (int n = 0; n < matched_names.Count; n++)
                        comboBox_ShowTasks.Items.Add(matched_names[n]);

                    int x = 0, y = 0;
                    GeneralUtils.get_cursor_pos(ref x, ref y);
                    if ((x > this.Location.X) && (x < (this.Location.X + this.Width)) && (y > this.Location.Y) && (y < (this.Location.Y + this.Height)))
                        ;
                    else
                        GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
                    
                    comboBox_ShowTasks.DroppedDown = true;
                    
                    textBox_Input.Focus();

                    Cursor = Cursors.Default;
                }
            }
        }

        // 文本框内容变更事件
        private void textBox_Input_TextChanged(object sender, EventArgs e)
        {
            check_textbox_changed();
        }

        // 组合框选择变更事件
        private void comboBox_ShowTasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox_Input.Text = comboBox_ShowTasks.Text;
            textBox_Input.Select(textBox_Input.TextLength, 0);               // 光标定位到文本最后
        }

        private void comboBox_TaskInfoSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (1 == comboBox_TaskInfoSource.SelectedIndex)
            {
                if ("" == parent.m_strTaskFileSavingDir)
                    label_SourceDir.Text = Directory.GetCurrentDirectory() + "\\任务文件";
                else
                    label_SourceDir.Text = parent.m_strTaskFileSavingDir;

                if (0 == m_vec_task_names.Count)
                {
                    string dir = "";
                    if ("" == parent.m_strTaskFileSavingDir)
                        dir = "任务文件";
                    else
                    {
                        if (!Directory.Exists(parent.m_strTaskFileSavingDir))
                            dir = "任务文件";
                        else
                            dir = parent.m_strTaskFileSavingDir;
                    }

                    DirectoryInfo info = new DirectoryInfo(dir);
                    if (info.Exists)
                    {
                        foreach (FileInfo file_info in info.GetFiles())
                        {
                            m_vec_task_names.Add(Path.GetFileNameWithoutExtension(file_info.Name));
                        }
                    }
                }
            }
            else
                label_SourceDir.Text = "";
        }

        private void Form_LoadTask_Load(object sender, EventArgs e)
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
            
            OpenFileDialog dlg = new OpenFileDialog();
            if (bHasDefaultDir)
                dlg.InitialDirectory = parent.m_strTaskFileSavingDir;
            else
                dlg.InitialDirectory = ".";
            dlg.Filter = "任务文件|*.dat";
            dlg.ShowDialog();
            if (dlg.FileName != string.Empty)
            {
                parent.m_strTaskFileSavingDir = System.IO.Path.GetDirectoryName(dlg.FileName);
                
                List<MeasurePointData> task_data = new List<MeasurePointData>();
                //parent.read_task_from_file(dlg.FileName, task_data);
                parent.read_task_from_file_for_Chenling(dlg.FileName, task_data);

                //if (3 == parent.get_fiducial_mark_count(task_data))
                {
                    parent.m_current_task_data = new List<MeasurePointData>(task_data);

                    //Debugger.Log(0, null, string.Format("222222 parent.m_current_task_data = {0}", parent.m_current_task_data.Count));

                    MainUI.dl_message_sender send_message = parent.CBD_SendMessage;
                    send_message("直接从文件加载任务", false, dlg.FileName, System.IO.Path.GetFileNameWithoutExtension(dlg.FileName));

                    this.Close();
                }
            }
        }
    }
}
