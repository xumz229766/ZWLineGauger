using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using HalconDotNet;
using System.Drawing.Imaging;
using ZWLineGauger.Gaugers;

namespace ZWLineGauger.Forms
{
    public partial class Form_CameraAndLights : Form
    {
        MainUI parent;

        ToolTip m_tooltip_top_light_value = new ToolTip();
        ToolTip m_tooltip_bottom_light_value = new ToolTip();

        double m_maincam_saturation = 1;
        double m_maincam_gamma = 1;
        double m_maincam_red = 1;
        double m_maincam_blue = 1;
        double m_maincam_green = 1;
        double m_maincam_gain = 1;
        double m_maincam_exposure = 20;

        double m_guidecam_saturation = 1;
        double m_guidecam_gamma = 1;
        double m_guidecam_red = 1;
        double m_guidecam_blue = 1;
        double m_guidecam_green = 1;
        double m_guidecam_gain = 1;
        double m_guidecam_exposure = 20;

        bool m_bInitDone = false;

        // 表单初始化
        public Form_CameraAndLights(MainUI parent)//主相机
        {
            this.parent = parent;
            InitializeComponent();

            this.StartPosition = FormStartPosition.Manual;
            this.Location = (Point)new Size(2, parent.ui_MainImage.Location.Y + 86);

            if (true)
            {
                /*
                if(MainUI.m_nCameraType == 0)//Pomeas
                {
                    this.parent.m_main_camera.m_red = 2;
                    this.parent.m_main_camera.m_blue = 1;
                    this.parent.m_main_camera.m_green = 2;
                    this.parent.m_main_camera.m_gain = 13;
                }
                else if(MainUI.m_nCameraType == 1)//MindVision
                {
                    this.parent.m_main_camera.m_red = 300;
                    this.parent.m_main_camera.m_blue = 300;
                    this.parent.m_main_camera.m_green = 300;
                    this.parent.m_main_camera.m_gain = 300;
                }*/
                m_maincam_red = this.parent.m_main_camera.m_red;
                m_maincam_blue = this.parent.m_main_camera.m_blue;
                m_maincam_green = this.parent.m_main_camera.m_green;
                m_maincam_gain = this.parent.m_main_camera.m_gain;
                m_maincam_saturation = this.parent.m_main_camera.m_saturation;
                m_maincam_gamma = this.parent.m_main_camera.m_gamma;              
                m_maincam_exposure = this.parent.m_main_camera.m_exposure;

                textBox_MainCam_Saturation.Text = string.Format("{0:0.0}", m_maincam_saturation);
                textBox_MainCam_Gamma.Text = string.Format("{0:0.0}", m_maincam_gamma);
                textBox_MainCam_Red.Text = string.Format("{0:0.0}", m_maincam_red);
                textBox_MainCam_Blue.Text = string.Format("{0:0.0}", m_maincam_blue);
                textBox_MainCam_Green.Text = string.Format("{0:0.0}", m_maincam_green);
                textBox_MainCam_Gain.Text = string.Format("{0:0.0}", m_maincam_gain);
                textBox_MainCam_Exposure.Text = string.Format("{0:0.}", m_maincam_exposure);

                double pos1 = (m_maincam_saturation - parent.m_main_camera.MIN_SATURATION) * 255 / parent.m_main_camera.MAX_SATURATION;
                double pos2 = (m_maincam_gamma - parent.m_main_camera.MIN_GAMMA) * 255 / parent.m_main_camera.MAX_GAMMA;
                double pos3 = 0, pos4 = 0, pos5 = 0, pos6 = 0;

                if (MainUI.m_nCameraType == 0)//Pomeas
                {
                    pos3 = (m_maincam_red - parent.m_main_camera.MIN_RED) * 255 / parent.m_main_camera.MAX_RED;
                    pos4 = (m_maincam_blue - parent.m_main_camera.MIN_BLUE) * 255 / parent.m_main_camera.MAX_BLUE;
                    pos5 = (m_maincam_green - parent.m_main_camera.MIN_GREEN) * 255 / parent.m_main_camera.MAX_GREEN;
                    pos6 = (m_maincam_gain - parent.m_main_camera.MIN_GAIN) * 255 / parent.m_main_camera.MAX_GAIN;
                }
                else if (MainUI.m_nCameraType == 1)//MindVision
                {
                    pos3 = (m_maincam_red - parent.m_main_camera.MIN_RED) * 255 / parent.m_main_camera.MDVS_MAX_RED;
                    pos4 = (m_maincam_blue - parent.m_main_camera.MIN_BLUE) * 255 / parent.m_main_camera.MDVS_MAX_BLUE;
                    pos5 = (m_maincam_green - parent.m_main_camera.MIN_GREEN) * 255 / parent.m_main_camera.MDVS_MAX_GREEN;
                    pos6 = (m_maincam_gain - parent.m_main_camera.MIN_GAIN) * 255 / parent.m_main_camera.MDVS_MAX_GAIN;
                }
                
                double pos7 = (m_maincam_exposure - parent.m_main_camera.MIN_EXPOSURE) * 255 / parent.m_main_camera.MAX_EXPOSURE;

                check_and_set_trackbar_value(trackBar_MainCam_Saturation, (int)pos1);
                check_and_set_trackbar_value(trackBar_MainCam_Gamma, (int)pos2);
                check_and_set_trackbar_value(trackBar_MainCam_Red, (int)pos3);
                check_and_set_trackbar_value(trackBar_MainCam_Blue, (int)pos4);
                check_and_set_trackbar_value(trackBar_MainCam_Green, (int)pos5);
                check_and_set_trackbar_value(trackBar_MainCam_Gain, (int)pos6);
                check_and_set_trackbar_value(trackBar_MainCam_Exposure, (int)pos7);
            }

            if (true)//导航相机
            {
                m_guidecam_saturation = this.parent.m_guide_camera.m_saturation;
                m_guidecam_gamma = this.parent.m_guide_camera.m_gamma;
                m_guidecam_red = this.parent.m_guide_camera.m_red;
                m_guidecam_blue = this.parent.m_guide_camera.m_blue;
                m_guidecam_green = this.parent.m_guide_camera.m_green;
                m_guidecam_gain = this.parent.m_guide_camera.m_gain;
                m_guidecam_exposure = this.parent.m_guide_camera.m_exposure;

                textBox_GuideCam_Saturation.Text = string.Format("{0:0.0}", m_guidecam_saturation);
                textBox_GuideCam_Gamma.Text = string.Format("{0:0.0}", m_guidecam_gamma);
                textBox_GuideCam_Red.Text = string.Format("{0:0.0}", m_guidecam_red);
                textBox_GuideCam_Blue.Text = string.Format("{0:0.0}", m_guidecam_blue);
                textBox_GuideCam_Green.Text = string.Format("{0:0.0}", m_guidecam_green);
                textBox_GuideCam_Gain.Text = string.Format("{0:0.0}", m_guidecam_gain);
                textBox_GuideCam_Exposure.Text = string.Format("{0:0.}", m_guidecam_exposure);

                double pos1 = (m_guidecam_saturation - parent.m_guide_camera.MIN_SATURATION) * 255 / parent.m_guide_camera.MAX_SATURATION;
                double pos2 = (m_guidecam_gamma - parent.m_guide_camera.MIN_GAMMA) * 255 / parent.m_guide_camera.MAX_GAMMA;

                double pos3 = 0, pos4 = 0, pos5 = 0, pos6 = 0;

                if (MainUI.m_nCameraType == 0)//Pomeas
                {
                    pos3 = (m_maincam_red - parent.m_guide_camera.MIN_RED) * 255 / parent.m_guide_camera.MAX_RED;
                    pos4 = (m_maincam_blue - parent.m_guide_camera.MIN_BLUE) * 255 / parent.m_guide_camera.MAX_BLUE;
                    pos5 = (m_maincam_green - parent.m_guide_camera.MIN_GREEN) * 255 / parent.m_guide_camera.MAX_GREEN;
                    pos6 = (m_maincam_gain - parent.m_guide_camera.MIN_GAIN) * 255 / parent.m_guide_camera.MAX_GAIN;
                }
                else if (MainUI.m_nCameraType == 1)//MindVision
                {
                    pos3 = (m_maincam_red - parent.m_guide_camera.MIN_RED) * 255 / parent.m_guide_camera.MDVS_MAX_RED;
                    pos4 = (m_maincam_blue - parent.m_guide_camera.MIN_BLUE) * 255 / parent.m_guide_camera.MDVS_MAX_BLUE;
                    pos5 = (m_maincam_green - parent.m_guide_camera.MIN_GREEN) * 255 / parent.m_guide_camera.MDVS_MAX_GREEN;
                    pos6 = (m_maincam_gain - parent.m_guide_camera.MIN_GAIN) * 255 / parent.m_guide_camera.MDVS_MAX_GAIN;
                }

                double pos7 = (m_guidecam_exposure - parent.m_guide_camera.MIN_EXPOSURE) * 255 / parent.m_guide_camera.MAX_EXPOSURE;

                check_and_set_trackbar_value(trackBar_GuideCam_Saturation, (int)pos1);
                check_and_set_trackbar_value(trackBar_GuideCam_Gamma, (int)pos2);
                check_and_set_trackbar_value(trackBar_GuideCam_Red, (int)pos3);
                check_and_set_trackbar_value(trackBar_GuideCam_Blue, (int)pos4);
                check_and_set_trackbar_value(trackBar_GuideCam_Green, (int)pos5);
                check_and_set_trackbar_value(trackBar_GuideCam_Gain, (int)pos6);
                check_and_set_trackbar_value(trackBar_GuideCam_Exposure, (int)pos7);

                textBox_GuideCam_PixelsPerUm.Text = Convert.ToString(parent.m_guide_camera.m_pixels_per_um);
            }

            string[] ports = new string[] { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9" };
            for (int n = 0; n < ports.Length; n++)
            {
                comboBox_LenCommNo.Items.Add(ports[n]);
                comboBox_TopLightPortNo.Items.Add(ports[n]);
                comboBox_BottomLightPortNo.Items.Add(ports[n]);
            }
            comboBox_LenCommNo.SelectedIndex = this.parent.m_len.m_nComIndex;
            comboBox_TopLightPortNo.SelectedIndex = this.parent.m_top_light.m_nComIndex;
            comboBox_BottomLightPortNo.SelectedIndex = this.parent.m_bottom_light.m_nComIndex;

            btn_TopLightOn.Text = (true == this.parent.m_top_light.m_bOn) ? "关闭" : "打开";
            btn_BottomLightOn.Text = (true == this.parent.m_bottom_light.m_bOn) ? "关闭" : "打开";

            bar_TopLight.Value = this.parent.m_top_light.m_nBrightness;
            bar_BottomLight.Value = this.parent.m_bottom_light.m_nBrightness;

            string[] ratios = new string[] { "70X", "100X", "200X", "300X", "400X", "500X" };
            if (true == this.parent.m_len.m_bHomed)
            {
                for (int n = 0; n < ratios.Length; n++)
                    comboBox_LenRatio.Items.Add(ratios[n]);
            }
            else
                comboBox_LenRatio.Enabled = false;

            gridview_OffsetsBetweenRatios.RowHeadersVisible = false;
            gridview_OffsetsBetweenRatios.ReadOnly = true;
            gridview_OffsetsBetweenRatios.ColumnCount = 3;
            gridview_OffsetsBetweenRatios.ColumnHeadersVisible = true;
            gridview_OffsetsBetweenRatios.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_OffsetsBetweenRatios.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_OffsetsBetweenRatios.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_OffsetsBetweenRatios.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_OffsetsBetweenRatios.Columns[0].Width = 66;
            gridview_OffsetsBetweenRatios.Columns[1].Width = 90;
            gridview_OffsetsBetweenRatios.Columns[2].Width = 90;
            gridview_OffsetsBetweenRatios.Columns[0].Name = "倍率";
            gridview_OffsetsBetweenRatios.Columns[1].Name = "偏移X(mm)";
            gridview_OffsetsBetweenRatios.Columns[2].Name = "偏移Y(mm)";
            for (int n = 1; n < 6; n++)
            {
                String strX = string.Format("{0:0.000}", parent.m_len_ratios_offsets[n].x);
                String strY = string.Format("{0:0.000}", parent.m_len_ratios_offsets[n].y);
                String[] row = new String[3] { ratios[n], strX, strY };

                gridview_OffsetsBetweenRatios.Rows.Add(row);
            }

            textBox_GuideCamOffsetX.Text = parent.m_len_ratios_offsets[0].x.ToString();
            textBox_GuideCamOffsetY.Text = parent.m_len_ratios_offsets[0].y.ToString();

            comboBox_CameraType.Items.Add("Pomeas");
            comboBox_CameraType.Items.Add("MindVision");
            comboBox_CameraType.SelectedIndex = MainUI.m_nCameraType;

            if (1 == MainUI.m_nCameraType)
            {
                trackBar_MainCam_Gain.Enabled = false;
                textBox_MainCam_Gain.Enabled = false;

                trackBar_GuideCam_Gain.Enabled = false;
                textBox_GuideCam_Gain.Enabled = false;
            }

            m_bInitDone = true;
        }

        private void Form_CameraAndLights_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainUI.dl_message_sender send_message = parent.CBD_SendMessage;

            // 关闭十字显示
            send_message("开启或关闭十字显示", false, false, 0);
        }

        // 保存
        private void btn_Save_Click(object sender, EventArgs e)
        {
            parent.m_main_camera.m_saturation = m_maincam_saturation;
            parent.m_main_camera.m_gamma = m_maincam_gamma;
            parent.m_main_camera.m_red = m_maincam_red;
            parent.m_main_camera.m_blue = m_maincam_blue;
            parent.m_main_camera.m_green = m_maincam_green;
            parent.m_main_camera.m_gain = m_maincam_gain;
            parent.m_main_camera.m_exposure = m_maincam_exposure;

            parent.m_guide_camera.m_saturation = m_guidecam_saturation;
            parent.m_guide_camera.m_gamma = m_guidecam_gamma;
            parent.m_guide_camera.m_red = m_guidecam_red;
            parent.m_guide_camera.m_blue = m_guidecam_blue;
            parent.m_guide_camera.m_green = m_guidecam_green;
            parent.m_guide_camera.m_gain = m_guidecam_gain;
            parent.m_guide_camera.m_exposure = m_guidecam_exposure;

            parent.m_guide_camera.m_pixels_per_um = Convert.ToDouble(textBox_GuideCam_PixelsPerUm.Text);

            parent.m_len.m_nComIndex = comboBox_LenCommNo.SelectedIndex;
            
            parent.m_top_light.m_nComIndex = comboBox_TopLightPortNo.SelectedIndex;
            parent.m_bottom_light.m_nComIndex = comboBox_BottomLightPortNo.SelectedIndex;

            parent.SaveAppParams();
            if ((true == parent.m_main_camera.save_params())
                && (true == parent.m_guide_camera.save_params())
                && (true == parent.m_top_light.save_params())
                && (true == parent.m_bottom_light.save_params())
                && (true == parent.m_len.save_params()))
            {
                MessageBox.Show(this, "保存成功", "提示", MessageBoxButtons.OK);
                Close();
            }
            else
            {
                MessageBox.Show(this, "相机参数有误，请检查修改再保存。", "提示", MessageBoxButtons.OK);
            }
        }

        // 取消
        private void btn_Cancel_Click(object sender, EventArgs e)
        {

            if (MainUI.m_nCameraType == 0)//Pomeas
            {
            parent.m_main_camera.set_red(parent.m_main_camera.m_red);
            parent.m_main_camera.set_blue(parent.m_main_camera.m_blue);
            parent.m_main_camera.set_green(parent.m_main_camera.m_green);
            parent.m_main_camera.set_gain(parent.m_main_camera.m_gain);
            parent.m_main_camera.set_exposure(parent.m_main_camera.m_exposure);

            parent.m_guide_camera.set_red(parent.m_guide_camera.m_red);
            parent.m_guide_camera.set_blue(parent.m_guide_camera.m_blue);
            parent.m_guide_camera.set_green(parent.m_guide_camera.m_green);
            parent.m_guide_camera.set_gain(parent.m_guide_camera.m_gain);
            parent.m_guide_camera.set_exposure(parent.m_guide_camera.m_exposure);
            }
            else if (MainUI.m_nCameraType == 1)//MindVision
            {
                parent.m_main_camera.set_gain(parent.m_main_camera.m_gain);
                parent.m_main_camera.set_red(parent.m_main_camera.m_red);
                parent.m_main_camera.set_blue(parent.m_main_camera.m_blue);
                parent.m_main_camera.set_green(parent.m_main_camera.m_green);
                parent.m_main_camera.set_exposure(parent.m_main_camera.m_exposure);

                parent.m_guide_camera.set_gain(parent.m_guide_camera.m_gain);
                parent.m_guide_camera.set_red(parent.m_guide_camera.m_red);
                parent.m_guide_camera.set_blue(parent.m_guide_camera.m_blue);
                parent.m_guide_camera.set_green(parent.m_guide_camera.m_green);
                parent.m_guide_camera.set_exposure(parent.m_guide_camera.m_exposure);
            }
            

            Close();
        }

        // 上环光打开/关闭
        private void btn_TopLightOn_Click(object sender, EventArgs e)
        {
            if ("打开" == btn_TopLightOn.Text)
            {
                btn_TopLightOn.Text = "关闭";
                parent.btn_top_light_Click(sender, e);
            }
            else
            {
                btn_TopLightOn.Text = "打开";
                parent.btn_top_light_Click(sender, e);
            }
        }

        // 下环光打开/关闭
        private void btn_BottomLightOn_Click(object sender, EventArgs e)
        {
            if ("打开" == btn_BottomLightOn.Text)
            {
                btn_BottomLightOn.Text = "关闭";
                parent.btn_bottom_light_Click(sender, e);
            }
            else
            {
                btn_BottomLightOn.Text = "打开";
                parent.btn_bottom_light_Click(sender, e);
            }
        }

        // 上环光亮度调节
        private void bar_TopLight_ValueChanged(object sender, EventArgs e)
        {
            parent.m_top_light.m_nBrightness = bar_TopLight.Value;
            parent.ui_trackBar_TopLight.Value = bar_TopLight.Value;

            m_tooltip_top_light_value.ShowAlways = true;
            m_tooltip_top_light_value.SetToolTip(bar_TopLight, bar_TopLight.Value.ToString());
        }

        // 下环光亮度调节
        private void bar_BottomLight_ValueChanged(object sender, EventArgs e)
        {
            parent.m_bottom_light.m_nBrightness = bar_BottomLight.Value;
            parent.ui_trackBar_BottomLight.Value = bar_BottomLight.Value;

            m_tooltip_bottom_light_value.ShowAlways = true;
            m_tooltip_bottom_light_value.SetToolTip(bar_BottomLight, bar_BottomLight.Value.ToString());
        }

        // 检查输入值是否在 TrackBar 的有效范围内，如超出范围，则进行调整，并设置 TrackBar 的值
        private void check_and_set_trackbar_value(TrackBar bar, int value)
        {
            if (value < bar.Minimum)
                bar.Value = bar.Minimum;
            else if (value > bar.Maximum)
                bar.Value = bar.Maximum;
            else
                bar.Value = value;
        }

        // 主相机饱和度调节
        private void trackBar_MainCam_Saturation_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            double min = parent.m_main_camera.MIN_SATURATION;
            double max = parent.m_main_camera.MAX_SATURATION;
            double ratio = (double)(trackBar_MainCam_Saturation.Value) / 255;

            if (trackBar_MainCam_Saturation.Minimum == trackBar_MainCam_Saturation.Value)
                m_maincam_saturation = min;
            else if (trackBar_MainCam_Saturation.Maximum == trackBar_MainCam_Saturation.Value)
                m_maincam_saturation = max;
            else
                m_maincam_saturation = min + (max - min) * ratio;

            if (m_maincam_saturation > max)
                m_maincam_saturation = max;

            textBox_MainCam_Saturation.Text = string.Format("{0:0.0}", m_maincam_saturation);
        }

        // 主相机伽马值调节
        private void trackBar_MainCam_Gamma_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            double min = parent.m_main_camera.MIN_GAMMA;
            double max = parent.m_main_camera.MAX_GAMMA;
            double ratio = (double)(trackBar_MainCam_Gamma.Value) / 255;

            if (trackBar_MainCam_Gamma.Minimum == trackBar_MainCam_Gamma.Value)
                m_maincam_gamma = min;
            else if (trackBar_MainCam_Gamma.Maximum == trackBar_MainCam_Gamma.Value)
                m_maincam_gamma = max;
            else
                m_maincam_gamma = min + (max - min) * ratio;

            if (m_maincam_gamma > max)
                m_maincam_gamma = max;

            textBox_MainCam_Gamma.Text = string.Format("{0:0.0}", m_maincam_gamma);
        }

        // 主相机红色通道调节
        private void trackBar_MainCam_Red_ValueChanged(object sender, EventArgs e)
        {

            if (false == m_bInitDone)
                return;

            //Debugger.Log(0, null, string.Format("222222  trackBar_MainCam_Red [{0}]", (int)m_maincam_red));


            if (MainUI.m_nCameraType == 0)//Pomeas
            {
            double min = parent.m_main_camera.MIN_RED;
            double max = parent.m_main_camera.MAX_RED;
            double ratio = (double)(trackBar_MainCam_Red.Value) / 255;

            if (trackBar_MainCam_Red.Minimum == trackBar_MainCam_Red.Value)
                m_maincam_red = min;
            else if (trackBar_MainCam_Red.Maximum == trackBar_MainCam_Red.Value)
                m_maincam_red = max;
            else
                m_maincam_red = min + (max - min) * ratio;

            if (m_maincam_red > max)
                m_maincam_red = max;
                //Debugger.Log(0, null, string.Format("222222 Pomeas"));
                parent.m_main_camera.set_red(m_maincam_red);

            textBox_MainCam_Red.Text = string.Format("{0:0.0}", m_maincam_red);
            }
            else if(MainUI.m_nCameraType == 1)//MindVision
            {
                
                Debugger.Log(0, null, string.Format("222222 MindVision"));
                //MVSDK.MvApi.CameraInit();
                int min = parent.m_main_camera.MDVS_MIN_RED;
                int max = parent.m_main_camera.MDVS_MAX_RED;
                double ratio = (double)(trackBar_MainCam_Red.Value) / 255;

                if (trackBar_MainCam_Red.Minimum == trackBar_MainCam_Red.Value)
                    m_maincam_red = min;
                else if (trackBar_MainCam_Red.Maximum == trackBar_MainCam_Red.Value)
                    m_maincam_red = max;
                else
                    m_maincam_red = min + (max - min) * ratio;

                if (m_maincam_red > max)
                    m_maincam_red = max;

                parent.m_main_camera.MDVS_set_red((int)m_maincam_red);
                textBox_MainCam_Red.Text = string.Format("{0:0.0}", m_maincam_red);

            }

            //Debugger.Log(0, null, string.Format("222222 Blue [{0},{1},{2}]", m_maincam_red.ToString(), m_maincam_blue.ToString(), m_maincam_green.ToString()));

        }

        // 主相机蓝色通道调节
        private void trackBar_MainCam_Blue_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            if (MainUI.m_nCameraType == 0)//Pomeas
            {
            double min = parent.m_main_camera.MIN_BLUE;
            double max = parent.m_main_camera.MAX_BLUE;
            double ratio = (double)(trackBar_MainCam_Blue.Value) / 255;

            if (trackBar_MainCam_Blue.Minimum == trackBar_MainCam_Blue.Value)
                m_maincam_blue = min;
            else if (trackBar_MainCam_Blue.Maximum == trackBar_MainCam_Blue.Value)
                m_maincam_blue = max;
            else
                m_maincam_blue = min + (max - min) * ratio;

            if (m_maincam_blue > max)
                m_maincam_blue = max;

            textBox_MainCam_Blue.Text = string.Format("{0:0.0}", m_maincam_blue);

            parent.m_main_camera.set_blue(m_maincam_blue);
        }
            else if (MainUI.m_nCameraType == 1)//MindVision
            {
                Debugger.Log(0, null, string.Format("222222 MindVision"));
                //MVSDK.MvApi.CameraInit();
                int min = parent.m_main_camera.MDVS_MIN_BLUE;
                int max = parent.m_main_camera.MDVS_MAX_BLUE;
                double ratio = (double)(trackBar_MainCam_Blue.Value) / 255;

                if (trackBar_MainCam_Blue.Minimum == trackBar_MainCam_Blue.Value)
                    m_maincam_blue = min;
                else if (trackBar_MainCam_Blue.Maximum == trackBar_MainCam_Blue.Value)
                    m_maincam_blue = max;
                else
                    m_maincam_blue = min + (max - min) * ratio;

                if (m_maincam_blue > max)
                    m_maincam_blue = max;

                parent.m_main_camera.MDVS_set_blue((int)(m_maincam_blue));
                textBox_MainCam_Blue.Text = string.Format("{0:0.0}", m_maincam_blue);
            }
        }

        // 主相机绿色通道调节
        private void trackBar_MainCam_Green_ValueChanged(object sender, EventArgs e)
        {

            if (false == m_bInitDone)
                return;

            if (MainUI.m_nCameraType == 0)//Pomeas
            {
            double min = parent.m_main_camera.MIN_GREEN;
            double max = parent.m_main_camera.MAX_GREEN;
            double ratio = (double)(trackBar_MainCam_Green.Value) / 255;

            if (trackBar_MainCam_Green.Minimum == trackBar_MainCam_Green.Value)
                m_maincam_green = min;
            else if (trackBar_MainCam_Green.Maximum == trackBar_MainCam_Green.Value)
                m_maincam_green = max;
            else
                m_maincam_green = min + (max - min) * ratio;

            if (m_maincam_green > max)
                m_maincam_green = max;

            textBox_MainCam_Green.Text = string.Format("{0:0.0}", m_maincam_green);

            parent.m_main_camera.set_green(m_maincam_green);
        }
            else if (MainUI.m_nCameraType == 1)//MindVision
            {
                Debugger.Log(0, null, string.Format("222222 MindVision"));
                //MVSDK.MvApi.CameraInit();
                int min = parent.m_main_camera.MDVS_MIN_GREEN;
                int max = parent.m_main_camera.MDVS_MAX_GREEN;
                double ratio = (double)(trackBar_MainCam_Green.Value) / 255;

                if (trackBar_MainCam_Green.Minimum == trackBar_MainCam_Green.Value)
                    m_maincam_green = min;
                else if (trackBar_MainCam_Green.Maximum == trackBar_MainCam_Green.Value)
                    m_maincam_green = max;
                else
                    m_maincam_green = min + (max - min) * ratio;

                if (m_maincam_green > max)
                    m_maincam_green = max;

                parent.m_main_camera.MDVS_set_green((int)(m_maincam_green));

                textBox_MainCam_Green.Text = string.Format("{0:0.0}", m_maincam_green);
            }
            

        }

        // 主相机增益调节
        private void trackBar_MainCam_Gain_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            if (MainUI.m_nCameraType == 0)//Pomeas
            {
            double min = parent.m_main_camera.MIN_GAIN;
            double max = parent.m_main_camera.MAX_GAIN;
            double ratio = (double)(trackBar_MainCam_Gain.Value) / 255;

            if (trackBar_MainCam_Gain.Minimum == trackBar_MainCam_Gain.Value)
                m_maincam_gain = min;
            else if (trackBar_MainCam_Gain.Maximum == trackBar_MainCam_Gain.Value)
                m_maincam_gain = max;
            else
                m_maincam_gain = min + (max - min) * ratio;

            if (m_maincam_gain > max)
                m_maincam_gain = max;

            textBox_MainCam_Gain.Text = string.Format("{0:0.0}", m_maincam_gain);

            parent.m_main_camera.set_gain(m_maincam_gain);
        }
            else if (MainUI.m_nCameraType == 1)//MindVision
            {
                double min = parent.m_main_camera.MDVS_MIN_GAIN;
                double max = parent.m_main_camera.MDVS_MAX_GAIN;
                double ratio = (double)(trackBar_MainCam_Gain.Value) / 255;

                if (trackBar_MainCam_Gain.Minimum == trackBar_MainCam_Gain.Value)
                    m_maincam_gain = min;
                else if (trackBar_MainCam_Gain.Maximum == trackBar_MainCam_Gain.Value)
                    m_maincam_gain = max;
                else
                    m_maincam_gain = min + (max - min) * ratio;

                if (m_maincam_gain > max)
                    m_maincam_gain = max;

                textBox_MainCam_Gain.Text = string.Format("{0:0.0}", m_maincam_gain);

                parent.m_main_camera.set_gain(m_maincam_gain);
            }

        }

        // 主相机曝光调节
        private void trackBar_MainCam_Exposure_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            double min = parent.m_main_camera.MIN_EXPOSURE;
            double max = parent.m_main_camera.MAX_EXPOSURE;
            double ratio = (double)(trackBar_MainCam_Exposure.Value) / 255;

            if (trackBar_MainCam_Exposure.Minimum == trackBar_MainCam_Exposure.Value)
                m_maincam_exposure = min;
            else if (trackBar_MainCam_Exposure.Maximum == trackBar_MainCam_Exposure.Value)
                m_maincam_exposure = max;
            else
                m_maincam_exposure = min + (max - min) * ratio;

            if (m_maincam_exposure > max)
                m_maincam_exposure = max;

            textBox_MainCam_Exposure.Text = string.Format("{0:0.}", m_maincam_exposure);

            parent.m_main_camera.set_exposure(trackBar_MainCam_Exposure.Value);
        }

        // 导航相机饱和度调节
        private void trackBar_GuideCam_Saturation_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            double min = parent.m_guide_camera.MIN_SATURATION;
            double max = parent.m_guide_camera.MAX_SATURATION;
            double ratio = (double)(trackBar_GuideCam_Saturation.Value) / 255;

            if (trackBar_GuideCam_Saturation.Minimum == trackBar_GuideCam_Saturation.Value)
                m_guidecam_saturation = min;
            else if (trackBar_GuideCam_Saturation.Maximum == trackBar_GuideCam_Saturation.Value)
                m_guidecam_saturation = max;
            else
                m_guidecam_saturation = min + (max - min) * ratio;

            if (m_guidecam_saturation > max)
                m_guidecam_saturation = max;

            textBox_GuideCam_Saturation.Text = string.Format("{0:0.0}", m_guidecam_saturation);
        }

        // 导航相机伽马值调节
        private void trackBar_GuideCam_Gamma_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            double min = parent.m_guide_camera.MIN_GAMMA;
            double max = parent.m_guide_camera.MAX_GAMMA;
            double ratio = (double)(trackBar_GuideCam_Gamma.Value) / 255;

            if (trackBar_GuideCam_Gamma.Minimum == trackBar_GuideCam_Gamma.Value)
                m_guidecam_gamma = min;
            else if (trackBar_GuideCam_Gamma.Maximum == trackBar_GuideCam_Gamma.Value)
                m_guidecam_gamma = max;
            else
                m_guidecam_gamma = min + (max - min) * ratio;

            if (m_guidecam_gamma > max)
                m_guidecam_gamma = max;

            textBox_GuideCam_Gamma.Text = string.Format("{0:0.0}", m_guidecam_gamma);
        }

        // 导航相机红色通道调节
        private void trackBar_GuideCam_Red_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            if (MainUI.m_nCameraType == 0)//Pomeas
            {
            double min = parent.m_guide_camera.MIN_RED;
            double max = parent.m_guide_camera.MAX_RED;
            double ratio = (double)(trackBar_GuideCam_Red.Value) / 255;

            if (trackBar_GuideCam_Red.Minimum == trackBar_GuideCam_Red.Value)
                m_guidecam_red = min;
            else if (trackBar_GuideCam_Red.Maximum == trackBar_GuideCam_Red.Value)
                m_guidecam_red = max;
            else
                m_guidecam_red = min + (max - min) * ratio;

            if (m_guidecam_red > max)
                m_guidecam_red = max;
                //Debugger.Log(0, null, string.Format("222222 Pomeas"));
                parent.m_guide_camera.set_red(m_guidecam_red);

            textBox_GuideCam_Red.Text = string.Format("{0:0.0}", m_guidecam_red);
            }
            else if (MainUI.m_nCameraType == 1)//MindVision
            {

                Debugger.Log(0, null, string.Format("222222 MindVision"));
                //MVSDK.MvApi.CameraInit();
                int min = parent.m_guide_camera.MDVS_MIN_RED;
                int max = parent.m_guide_camera.MDVS_MAX_RED;
                double ratio = (double)(trackBar_GuideCam_Red.Value) / 255;

                if (trackBar_GuideCam_Red.Minimum == trackBar_GuideCam_Red.Value)
                    m_guidecam_red = min;
                else if (trackBar_GuideCam_Red.Maximum == trackBar_GuideCam_Red.Value)
                    m_guidecam_red = max;
                else
                    m_guidecam_red = min + (max - min) * ratio;

                if (m_guidecam_red > max)
                    m_guidecam_red = max;

                parent.m_guide_camera.MDVS_set_red((int)m_guidecam_red);
                textBox_GuideCam_Red.Text = string.Format("{0:0.0}", m_guidecam_red);

            }

        }

        // 导航相机蓝色通道调节
        private void trackBar_GuideCam_Blue_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            if (MainUI.m_nCameraType == 0)//Pomeas
            {
                double min = parent.m_guide_camera.MIN_RED;
                double max = parent.m_guide_camera.MAX_RED;
            double ratio = (double)(trackBar_GuideCam_Blue.Value) / 255;

            if (trackBar_GuideCam_Blue.Minimum == trackBar_GuideCam_Blue.Value)
                m_guidecam_blue = min;
            else if (trackBar_GuideCam_Blue.Maximum == trackBar_GuideCam_Blue.Value)
                m_guidecam_blue = max;
            else
                m_guidecam_blue = min + (max - min) * ratio;

            if (m_guidecam_blue > max)
                m_guidecam_blue = max;
                //Debugger.Log(0, null, string.Format("222222 Pomeas"));
                parent.m_guide_camera.set_blue(m_guidecam_blue);

            textBox_GuideCam_Blue.Text = string.Format("{0:0.0}", m_guidecam_blue);
            }
            else if (MainUI.m_nCameraType == 1)//MindVision
            {

                Debugger.Log(0, null, string.Format("222222 MindVision"));
                //MVSDK.MvApi.CameraInit();
                int min = parent.m_guide_camera.MDVS_MIN_RED;
                int max = parent.m_guide_camera.MDVS_MAX_RED;
                double ratio = (double)(trackBar_GuideCam_Blue.Value) / 255;

                if (trackBar_GuideCam_Blue.Minimum == trackBar_GuideCam_Blue.Value)
                    m_guidecam_blue = min;
                else if (trackBar_GuideCam_Blue.Maximum == trackBar_GuideCam_Blue.Value)
                    m_guidecam_blue = max;
                else
                    m_guidecam_blue = min + (max - min) * ratio;

                if (m_guidecam_blue > max)
                    m_guidecam_blue = max;

                parent.m_guide_camera.MDVS_set_blue((int)m_guidecam_blue);
                textBox_GuideCam_Blue.Text = string.Format("{0:0.0}", m_guidecam_blue);

            }

        }

        // 导航相机绿色通道调节
        private void trackBar_GuideCam_Green_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            if (MainUI.m_nCameraType == 0)//Pomeas
            {
                double min = parent.m_guide_camera.MIN_RED;
                double max = parent.m_guide_camera.MAX_RED;
            double ratio = (double)(trackBar_GuideCam_Green.Value) / 255;

            if (trackBar_GuideCam_Green.Minimum == trackBar_GuideCam_Green.Value)
                m_guidecam_green = min;
            else if (trackBar_GuideCam_Green.Maximum == trackBar_GuideCam_Green.Value)
                m_guidecam_green = max;
            else
                m_guidecam_green = min + (max - min) * ratio;

            if (m_guidecam_green > max)
                m_guidecam_green = max;
                //Debugger.Log(0, null, string.Format("222222 Pomeas"));
                parent.m_guide_camera.set_green(m_guidecam_green);

            textBox_GuideCam_Green.Text = string.Format("{0:0.0}", m_guidecam_green);
            }
            else if (MainUI.m_nCameraType == 1)//MindVision
            {

                Debugger.Log(0, null, string.Format("222222 MindVision"));
                //MVSDK.MvApi.CameraInit();
                int min = parent.m_guide_camera.MDVS_MIN_RED;
                int max = parent.m_guide_camera.MDVS_MAX_RED;
                double ratio = (double)(trackBar_GuideCam_Green.Value) / 255;

                if (trackBar_GuideCam_Green.Minimum == trackBar_GuideCam_Green.Value)
                    m_guidecam_green = min;
                else if (trackBar_GuideCam_Green.Maximum == trackBar_GuideCam_Green.Value)
                    m_guidecam_green = max;
                else
                    m_guidecam_green = min + (max - min) * ratio;

                if (m_guidecam_green > max)
                    m_guidecam_green = max;

                parent.m_guide_camera.MDVS_set_green((int)m_guidecam_green);
                textBox_GuideCam_Green.Text = string.Format("{0:0.0}", m_guidecam_green);

            }

        }

        // 导航相机增益调节
        private void trackBar_GuideCam_Gain_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            if (MainUI.m_nCameraType == 0)//Pomeas
            {
            double min = parent.m_guide_camera.MIN_GAIN;
            double max = parent.m_guide_camera.MAX_GAIN;
            double ratio = (double)(trackBar_GuideCam_Gain.Value) / 255;

            if (trackBar_GuideCam_Gain.Minimum == trackBar_GuideCam_Gain.Value)
                m_guidecam_gain = min;
            else if (trackBar_GuideCam_Gain.Maximum == trackBar_GuideCam_Gain.Value)
                m_guidecam_gain = max;
            else
                m_guidecam_gain = min + (max - min) * ratio;

            if (m_guidecam_gain > max)
                m_guidecam_gain = max;

            textBox_GuideCam_Gain.Text = string.Format("{0:0.0}", m_guidecam_gain);

                parent.m_guide_camera.set_gain(m_guidecam_gain);
            }
            else if (MainUI.m_nCameraType == 1)//MindVision
            {
                double min = parent.m_guide_camera.MDVS_MIN_GAIN;
                double max = parent.m_guide_camera.MDVS_MAX_GAIN;
                double ratio = (double)(trackBar_GuideCam_Gain.Value) / 255;

                if (trackBar_GuideCam_Gain.Minimum == trackBar_GuideCam_Gain.Value)
                    m_guidecam_gain = min;
                else if (trackBar_GuideCam_Gain.Maximum == trackBar_GuideCam_Gain.Value)
                    m_guidecam_gain = max;
                else
                    m_guidecam_gain = min + (max - min) * ratio;

                if (m_guidecam_gain > max)
                    m_guidecam_gain = max;

                textBox_GuideCam_Gain.Text = string.Format("{0:0.0}", m_guidecam_gain);

                parent.m_guide_camera.set_gain(m_guidecam_gain);
            }

        }

        // 导航相机曝光调节
        private void trackBar_GuideCam_Exposure_ValueChanged(object sender, EventArgs e)
        {
            if (false == m_bInitDone)
                return;

            double min = parent.m_guide_camera.MIN_EXPOSURE;
            double max = parent.m_guide_camera.MAX_EXPOSURE;
            double ratio = (double)(trackBar_GuideCam_Exposure.Value) / 255;

            if (trackBar_GuideCam_Exposure.Minimum == trackBar_GuideCam_Exposure.Value)
                m_guidecam_exposure = min;
            else if (trackBar_GuideCam_Exposure.Maximum == trackBar_GuideCam_Exposure.Value)
                m_guidecam_exposure = max;
            else
                m_guidecam_exposure = min + (max - min) * ratio;

            if (m_guidecam_exposure > max)
                m_guidecam_exposure = max;

            textBox_GuideCam_Exposure.Text = string.Format("{0:0.}", m_guidecam_exposure);

            parent.m_guide_camera.set_exposure(trackBar_GuideCam_Exposure.Value);
        }

        private void Form_CameraAndLights_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }

        private void btn_CalibrateRatioOffsets_Click(object sender, EventArgs e)
        {
            btn_CalibrateRatioOffsets.Enabled = false;

            this.StartPosition = FormStartPosition.Manual;
            this.Location = (Point)new Size(-250, parent.ui_MainImage.Location.Y + 256);

            for (int n = 1; n < 6; n++)
            {
                gridview_OffsetsBetweenRatios[1, n - 1].Value = string.Format("{0:0.000}", 0);
                gridview_OffsetsBetweenRatios[2, n - 1].Value = string.Format("{0:0.000}", 0);
            }

            new Thread(thread_calibrate_ratios_offsets).Start();
        }

        // 线程：校正不同倍率偏移
        public void thread_calibrate_ratios_offsets(object obj)
        {
            MainUI.dl_message_sender send_message = parent.CBD_SendMessage;

            // 开启十字显示
            send_message("开启或关闭十字显示", false, true, 0);

            // 变倍到70X
            #region
            if (false == parent.m_bOfflineMode)
            {
                send_message("设置倍率", false, 0, null);
                for (int k = 0; k < 50; k++)
                {
                    Thread.Sleep(100);
                    if (false == parent.m_len.m_bLenIsChangingRatio)
                        break;
                }
            }
            #endregion

            // 自动对焦
            if (false == parent.m_bOfflineMode)
            {
                parent.m_image_operator.thread_auto_focus(new object());
                Thread.Sleep(300);
            }

            ImgOperators img_operator = new ImgOperators(parent, MainUI.m_motion);
            double width = parent.m_main_camera.m_nCamWidth;
            double height = parent.m_main_camera.m_nCamHeight;
            if (true == parent.m_bOfflineMode)
            {
                width = parent.ui_MainImage.Image.Width;
                height = parent.ui_MainImage.Image.Height;
            }

            // 在70X倍率下定位直角点
            double size_ratio = 0.4;
            Point2d offset = new Point2d(0, 0);
            Point2d left_top = new Point2d((width - width * size_ratio) / 2, (height - height * size_ratio) / 2);
            Point2d right_bottom = new Point2d(left_top.x + width * size_ratio, left_top.y + height * size_ratio);
            Point2d corner_crd = new Point2d(0, 0);
            if (false == img_operator.locate_corner_under_ratio(parent.m_main_camera, 0, left_top, right_bottom, ref corner_crd))
            {
                btn_CalibrateRatioOffsets.Enabled = true;
                send_message("开启或关闭十字显示", false, false, 0);
                MessageBox.Show(this, "无法找到直角，请检查原因。\n校正线程退出。", "提示");
                goto THE_END;
            }
            
            // 如果找到直角点，则将相机中心移动到直角点
            offset.x = corner_crd.x - width / 2;
            offset.y = corner_crd.y - height / 2;
            //Debugger.Log(0, null, string.Format("222222 offset = [{0},{1}]", offset.x, offset.y));

            offset.x = (offset.x / parent.m_calib_data[parent.comboBox_Len.SelectedIndex]) * (double)(MainUI.m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
            offset.y = -(offset.y / parent.m_calib_data[parent.comboBox_Len.SelectedIndex]) * (double)(MainUI.m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;
            //Debugger.Log(0, null, string.Format("222222 offset = [{0},{1}]", offset.x, offset.y));

            Point3d current_crd = new Point3d(0, 0, 0);
            MainUI.m_motion.get_xyz_crds(ref current_crd);
            MainUI.m_motion.linear_XYZ_wait_until_stop(current_crd.x + offset.x, current_crd.y + offset.y, current_crd.z, 50, 0.25, false);
            Thread.Sleep(100);

            //Debugger.Log(0, null, string.Format("222222 offset = [{0},{1}]", offset.x, offset.y));

            if (DialogResult.No == MessageBox.Show(this, "当前图像十字线是否与标定板上的直角完全对齐？\n选是继续标定，选否退出标定线程。", "提示", MessageBoxButtons.YesNo))
                goto THE_END;

            double base_x = current_crd.x + offset.x;
            double base_y = current_crd.y + offset.y;

            // 基于70倍时的直角位置，对其它倍率进行校准
            for (int n = 1; n < parent.comboBox_Len.Items.Count; n++)
            {
                // 变倍
                if (false == parent.m_bOfflineMode)
                {
                    send_message("设置倍率", false, n, null);
                    for (int k = 0; k < 50; k++)
                    {
                        Thread.Sleep(100);
                        if (false == parent.m_len.m_bLenIsChangingRatio)
                            break;
                    }
                }
                Thread.Sleep(300);

                // 自动调节亮度
                if (true == parent.m_top_light.m_bOn)
                    parent.m_top_light.auto_adjust_brightness(parent.m_main_camera, 180, 238);
                else
                    parent.m_bottom_light.auto_adjust_brightness(parent.m_main_camera, 180, 238);
                Thread.Sleep(300);

                // 自动对焦
                if (false == parent.m_bOfflineMode)
                {
                    parent.m_image_operator.thread_auto_focus(new object());
                    Thread.Sleep(300);
                }

                // 定位直角点
                size_ratio = 0.6;
                offset = new Point2d(0, 0);
                left_top = new Point2d((width - width * size_ratio) / 2, (height - height * size_ratio) / 2);
                right_bottom = new Point2d(left_top.x + width * size_ratio, left_top.y + height * size_ratio);
                corner_crd = new Point2d(0, 0);
                if (false == img_operator.locate_corner_under_ratio(parent.m_main_camera, 0, left_top, right_bottom, ref corner_crd))
                {
                    btn_CalibrateRatioOffsets.Enabled = true;
                    send_message("开启或关闭十字显示", false, false, 0);
                    MessageBox.Show(this, "无法找到直角，请检查原因。\n校正线程退出。", "提示");
                    goto THE_END;
                }

                // 如果找到直角点，则将相机中心移动到直角点
                offset.x = corner_crd.x - width / 2;
                offset.y = corner_crd.y - height / 2;
                offset.x = (offset.x / parent.m_calib_data[parent.comboBox_Len.SelectedIndex]) * (double)(MainUI.m_motion.m_axes[MotionOps.AXIS_X - 1].nDir) / 1000;
                offset.y = -(offset.y / parent.m_calib_data[parent.comboBox_Len.SelectedIndex]) * (double)(MainUI.m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir) / 1000;

                current_crd = new Point3d(0, 0, 0);
                MainUI.m_motion.get_xyz_crds(ref current_crd);
                MainUI.m_motion.linear_XYZ_wait_until_stop(current_crd.x + offset.x, current_crd.y + offset.y, current_crd.z, 50, 0.25, false);
                Thread.Sleep(100);

                //Debugger.Log(0, null, string.Format("222222 offset = [{0},{1}]", offset.x, offset.y));

                if (DialogResult.No == MessageBox.Show(this, "当前图像十字线是否与标定板上的直角完全对齐？\n选是继续标定，选否退出标定线程。", "提示", MessageBoxButtons.YesNo))
                    goto THE_END;

                gridview_OffsetsBetweenRatios[1, n - 1].Value = string.Format("{0:0.000}", current_crd.x + offset.x - base_x);
                gridview_OffsetsBetweenRatios[2, n - 1].Value = string.Format("{0:0.000}", current_crd.y + offset.y - base_y);

                //break;
            }

            if (DialogResult.Yes == MessageBox.Show(this, "倍率偏移校准完成，是否保存本次校准数据？", "提示", MessageBoxButtons.YesNo))
            {
                for (int n = 1; n <= 5; n++)
                {
                    parent.m_len_ratios_offsets[n].x = Convert.ToDouble(gridview_OffsetsBetweenRatios[1, n - 1].Value);
                    parent.m_len_ratios_offsets[n].y = Convert.ToDouble(gridview_OffsetsBetweenRatios[2, n - 1].Value);
                }

                parent.SaveAppParams();
            }

            Thread.Sleep(5000);

            THE_END:
            btn_CalibrateRatioOffsets.Enabled = true;                   // 这里可能会导致崩溃
        }

        private void comboBox_CameraType_SelectedIndexChanged(object sender, EventArgs e)
        {
            MainUI.m_nCameraType = comboBox_CameraType.SelectedIndex;
            if (0 == MainUI.m_nCameraType)
            {
                trackBar_MainCam_Gain.Enabled = true;
                textBox_MainCam_Gain.Enabled = true;

                trackBar_GuideCam_Gain.Enabled = true;
                textBox_GuideCam_Gain.Enabled = true;
            }
            else if (1 == MainUI.m_nCameraType)
            {
                trackBar_MainCam_Gain.Enabled = false;
                textBox_MainCam_Gain.Enabled = false;

                trackBar_GuideCam_Gain.Enabled = false;
                textBox_GuideCam_Gain.Enabled = false;
            }
        }
    }
}
