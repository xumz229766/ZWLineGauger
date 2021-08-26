using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZWLineGauger.Forms
{
    public partial class Form_HeightSensor : Form
    {
        MainUI parent;

        double m_dbSensorTriggerHeightForStage = 0;
        double m_dbSensorTriggerHeightGapAboveStage = 0;

        double m_dbStageHeightGap = 0;
        double m_dbStageTriggerHeight = 0;

        bool m_bIsCalibThreadRunning = false;
        bool m_bExitCalibThread = false;
        int m_nCalibType = 0;

        public delegate bool dl_message_sender(string info, bool bIsKeyInfo, object param1, object param2);

        // 消息处理
        public bool CBD_SendMessage(string info, bool bIsKeyInfo, object param1, object param2)
        {
            if (this.InvokeRequired)
            {
                dl_message_sender callback = new dl_message_sender(CBD_SendMessage);
                return (bool)(this.Invoke(callback, info, bIsKeyInfo, param1, param2));
            }
            else
            {
                if ("触发高度和对焦清晰高度之间的差值标定完成" == info)
                {
                    textBox_Average.Text = string.Format("{0:0.000}", m_dbStageHeightGap);
                }
                else if ("平台高度标定完成" == info)
                {
                    textBox_Stage_Average.Text = string.Format("{0:0.000}", m_dbStageHeightGap);
                    textBox_Stage_Triggering_Height.Text = string.Format("{0:0.000}", m_dbStageTriggerHeight);
                }
            }

            return true;
        }

        public Form_HeightSensor(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();

            gridview_MeasureResults.RowHeadersVisible = false;
            gridview_MeasureResults.ReadOnly = true;
            gridview_MeasureResults.ColumnCount = 4;
            gridview_MeasureResults.ColumnHeadersVisible = true;
            gridview_MeasureResults.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_MeasureResults.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_MeasureResults.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_MeasureResults.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_MeasureResults.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_MeasureResults.Columns[0].Width = 60;
            gridview_MeasureResults.Columns[1].Width = 105;
            gridview_MeasureResults.Columns[2].Width = 105;
            gridview_MeasureResults.Columns[3].Width = 105;
            gridview_MeasureResults.Columns[0].Name = "序号";
            gridview_MeasureResults.Columns[1].Name = "触发高度(mm)";
            gridview_MeasureResults.Columns[2].Name = "对焦高度(mm)";
            gridview_MeasureResults.Columns[3].Name = "高度差(mm)";

            checkBox_UseHeightSensor.Checked = parent.m_bUseHeightSensor;

            textBox_Average.Text = string.Format("{0:0.000}", parent.m_dbStageHeightGap);
            textBox_Stage_Average.Text = string.Format("{0:0.000}", parent.m_dbStageHeightGap);
            textBox_Stage_Triggering_Height.Text = string.Format("{0:0.000}", parent.m_dbStageTriggerHeight);

            textBox_CameraHeightSensorOffsetX.Text = string.Format("{0:0.000}", parent.m_dbCameraHeightSensorOffsetX);
            textBox_CameraHeightSensorOffsetY.Text = string.Format("{0:0.000}", parent.m_dbCameraHeightSensorOffsetY);

            m_dbStageHeightGap = parent.m_dbStageHeightGap;
            m_dbStageTriggerHeight = parent.m_dbStageTriggerHeight;
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            parent.m_bUseHeightSensor = checkBox_UseHeightSensor.Checked;
            parent.m_dbStageHeightGap = m_dbStageHeightGap;
            parent.m_dbStageTriggerHeight = m_dbStageTriggerHeight;

            parent.m_dbCameraHeightSensorOffsetX = Convert.ToDouble(textBox_CameraHeightSensorOffsetX.Text);
            parent.m_dbCameraHeightSensorOffsetY = Convert.ToDouble(textBox_CameraHeightSensorOffsetY.Text);

            MessageBox.Show(this, "保存成功", "提示");

            Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void thread_calibrate_sensor_height(object obj)
        {
            const int TIMES = 10;
            double average_gap = 0;
            double average_height = 0;
            double neg_limit = MainUI.m_motion.m_axes[MotionOps.AXIS_Z - 1].negative_limit;

            for (int n = 0; n < TIMES; n++)
            {
                if (true == m_bExitCalibThread)
                {
                    m_bIsCalibThreadRunning = false;
                    return;
                }

                MainUI.m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, neg_limit - 50 * MainUI.m_motion.m_axes[MotionOps.AXIS_Z - 1].nDir, 20);
                if (true == parent.m_IO.is_height_sensor_activated())
                {
                    Debugger.Log(0, null, string.Format("222222 h 111"));
                    if (true == m_bExitCalibThread)
                    {
                        m_bIsCalibThreadRunning = false;
                        return;
                    }
                    Debugger.Log(0, null, string.Format("222222 h 222"));
                    MainUI.m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, neg_limit - 1 * MainUI.m_motion.m_axes[MotionOps.AXIS_Z - 1].nDir, 5);
                    Debugger.Log(0, null, string.Format("222222 h 333"));
                    for (int m = 0; m < 1000; m++)
                    {
                        bool bActivated = parent.m_IO.is_height_sensor_activated();

                        if (true == m_bExitCalibThread)
                        {
                            m_bIsCalibThreadRunning = false;
                            return;
                        }

                        if (false == bActivated)
                        {
                            double activate_z = 0;
                            double focus_z = 0;
                            double gap = 0;
                            Debugger.Log(0, null, string.Format("222222 h 444"));
                            Point3d crd = new Point3d(0, 0, 0);
                            MainUI.m_motion.get_xyz_crds(ref crd);
                            activate_z = crd.z;
                            Debugger.Log(0, null, string.Format("222222 h 555"));
                            MainUI.m_motion.stop_axis(MotionOps.AXIS_Z);
                            Thread.Sleep(1000);

                            if (true == m_bExitCalibThread)
                            {
                                m_bIsCalibThreadRunning = false;
                                return;
                            }
                            Debugger.Log(0, null, string.Format("222222 h 666"));
                            parent.m_image_operator.thread_auto_focus(new object());
                            Debugger.Log(0, null, string.Format("222222 h 777"));
                            if (true == m_bExitCalibThread)
                            {
                                m_bIsCalibThreadRunning = false;
                                return;
                            }
                            Debugger.Log(0, null, string.Format("222222 h 888"));
                            MainUI.m_motion.stop_axis(MotionOps.AXIS_Z);
                            Thread.Sleep(500);
                            Debugger.Log(0, null, string.Format("222222 h aaa"));
                            if (true == m_bExitCalibThread)
                            {
                                m_bIsCalibThreadRunning = false;
                                return;
                            }

                            MainUI.m_motion.get_xyz_crds(ref crd);
                            focus_z = crd.z;
                            Debugger.Log(0, null, string.Format("222222 h bbb"));
                            gap = activate_z - focus_z;
                            average_gap += gap;
                            average_height += activate_z;

                            String str1 = string.Format("{0}", n + 1);
                            String str2 = string.Format("{0:0.000}", activate_z);
                            String str3 = string.Format("{0:0.000}", focus_z);
                            String str4 = string.Format("{0:0.000}", gap);
                            String[] row = new String[4] { str1, str2, str3, str4};
                            Debugger.Log(0, null, string.Format("222222 h ccc"));
                            if (true == m_bExitCalibThread)
                            {
                                m_bIsCalibThreadRunning = false;
                                return;
                            }
                            
                            gridview_MeasureResults.Rows.Add(row);

                            break;
                        }

                        Thread.Sleep(5);
                    }
                }
                else
                {
                    MessageBox.Show(this, "传感器触发高度设置有问题，可能触发高度过大或过小，请检查。", "提示");

                    m_bIsCalibThreadRunning = false;
                    return;
                }
            }

            if (Math.Abs(average_gap) > 0.1)
            {
                dl_message_sender send_message = obj as dl_message_sender;

                switch (m_nCalibType)
                {
                    case 0:
                        m_dbStageHeightGap = average_gap / TIMES;

                        send_message("触发高度和对焦清晰高度之间的差值标定完成", false, null, null);
                        break;

                    case 1:
                        m_dbStageHeightGap = average_gap / TIMES;
                        m_dbStageTriggerHeight = average_height / TIMES;

                        send_message("平台高度标定完成", false, null, null);
                        break;
                }
            }

            m_bIsCalibThreadRunning = false;
        }

        private void btn_CalibrateHeightGap_Click(object sender, EventArgs e)
        {
            if (false == m_bIsCalibThreadRunning)
            {
                gridview_MeasureResults.Rows.Clear();

                m_bIsCalibThreadRunning = true;
                m_nCalibType = 0;

                dl_message_sender messenger = CBD_SendMessage;
                (new Thread(thread_calibrate_sensor_height)).Start(messenger);
            }
        }

        private void btn_CalibrateStage_Click(object sender, EventArgs e)
        {
            if (false == m_bIsCalibThreadRunning)
            {
                gridview_MeasureResults.Rows.Clear();

                m_bIsCalibThreadRunning = true;
                m_nCalibType = 1;

                dl_message_sender messenger = CBD_SendMessage;
                (new Thread(thread_calibrate_sensor_height)).Start(messenger);
            }
        }

        private void checkBox_UseHeightSensor_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void Form_HeightSensor_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }

        private void Form_HeightSensor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debugger.Log(0, null, string.Format("222222 Form_HeightSensor_FormClosing"));
            if (true == m_bIsCalibThreadRunning)
            {
                m_bExitCalibThread = true;
                while (true)
                {
                    Thread.Sleep(20);
                    if (false == m_bIsCalibThreadRunning)
                        break;
                }
            }
        }
    }
}
