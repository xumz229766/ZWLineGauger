using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZWLineGauger.Forms
{
    public partial class Form_Settings : Form
    {
        MainUI parent;

        public Form_Settings(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();

            textBox_MainCamBrightnessLower.Text = parent.m_nMainCamLowerBrightness.ToString();
            textBox_MainCamBrightnessUpper.Text = parent.m_nMainCamUpperBrightness.ToString();
            textBox_GuideCamBrightnessLower.Text = parent.m_nGuideCamLowerBrightness.ToString();
            textBox_GuideCamBrightnessUpper.Text = parent.m_nGuideCamUpperBrightness.ToString();
            textBox_DelaySecondsBeforeMeasure.Text = parent.m_dbDelaySecondsBeforeMeasure.ToString();
            

            // 导航光源选择
            if (0 == parent.m_nLightTypeForGuideCamForMarkPt)
            {
                radio_TopLightForGuideCam.Checked = true;
                radio_BottomLightForGuideCam.Checked = false;
            }
            else
            {
                radio_TopLightForGuideCam.Checked = false;
                radio_BottomLightForGuideCam.Checked = true;
            }

            // 下线宽光源选择
            if (0 == parent.m_nLightTypeFor14Line)
            {
                radio_TopLightFor14Line.Checked = true;
                radio_BottomLightFor14Line.Checked = false;
            }
            else
            {
                radio_TopLightFor14Line.Checked = false;
                radio_BottomLightFor14Line.Checked = true;
            }

            // 线距光源选择
            if (0 == parent.m_nLightTypeForLineSpace)
            {
                radio_TopLightForLineSpace.Checked = true;
                radio_BottomLightForLineSpace.Checked = false;
            }
            else
            {
                radio_TopLightForLineSpace.Checked = false;
                radio_BottomLightForLineSpace.Checked = true;
            }

            // BGA光源选择
            if (0 == parent.m_nLightTypeForBGA)
            {
                radio_TopLightForBGA.Checked = true;
                radio_BottomLightForBGA.Checked = false;
            }
            else
            {
                radio_TopLightForBGA.Checked = false;
                radio_BottomLightForBGA.Checked = true;
            }

            checkBox_NoConfirmMarkAtCreation.Checked = parent.m_bDoNotConfirmMarkAtCreation;
            checkBox_NoConfirmMeasureItemAtCreation.Checked = parent.m_bDoNotConfirmMeasureItemAtCreation;
            
            checkBox_SelectivelySkipAutofocus.Checked = parent.m_bSelectivelySkipAutofocus;
            textBox_ThresForSkippingAutofocus.Text = parent.m_nThresForSkippingAutofocus.ToString();

            // 测量结果小数点位数mm
            comboBox_MeasureResultDigitsForMM.Items.Add("1");
            comboBox_MeasureResultDigitsForMM.Items.Add("2");
            comboBox_MeasureResultDigitsForMM.Items.Add("3");
            comboBox_MeasureResultDigitsForMM.Items.Add("4");
            comboBox_MeasureResultDigitsForMM.Items.Add("5");
            comboBox_MeasureResultDigitsForMM.Items.Add("6");
            comboBox_MeasureResultDigitsForMM.SelectedIndex = parent.m_nMeasureResultDigits[0] - 1;

            // 测量结果小数点位数um
            comboBox_MeasureResultDigitsForUM.Items.Add("1");
            comboBox_MeasureResultDigitsForUM.Items.Add("2");
            comboBox_MeasureResultDigitsForUM.Items.Add("3");
            comboBox_MeasureResultDigitsForUM.Items.Add("4");
            comboBox_MeasureResultDigitsForUM.Items.Add("5");
            comboBox_MeasureResultDigitsForUM.Items.Add("6");
            comboBox_MeasureResultDigitsForUM.SelectedIndex = parent.m_nMeasureResultDigits[1] - 1;

            // 测量结果小数点位数mil
            comboBox_MeasureResultDigitsForMIL.Items.Add("1");
            comboBox_MeasureResultDigitsForMIL.Items.Add("2");
            comboBox_MeasureResultDigitsForMIL.Items.Add("3");
            comboBox_MeasureResultDigitsForMIL.Items.Add("4");
            comboBox_MeasureResultDigitsForMIL.Items.Add("5");
            comboBox_MeasureResultDigitsForMIL.Items.Add("6");
            comboBox_MeasureResultDigitsForMIL.SelectedIndex = parent.m_nMeasureResultDigits[2] - 1;

            // 测量方式
            comboBox_MeasureLineMethod.Items.Add("按平行线段测量");
            comboBox_MeasureLineMethod.Items.Add("按非平行线段测量");
            //comboBox_MeasureLineMethod.Items.Add("按实际轮廓测量");
            comboBox_MeasureLineMethod.SelectedIndex = parent.m_nLineMeasurementMethod;

            // 图纸测量项标准值的来源
            comboBox_SourceOfStandardValue.Items.Add("基于图纸原始数据");
            comboBox_SourceOfStandardValue.Items.Add("基于图纸成像识别");
            comboBox_SourceOfStandardValue.SelectedIndex = parent.m_nSourceOfStandardValue;
        }
        
        // 按钮：保存
        private void btn_OK_Click(object sender, EventArgs e)
        {
            int value = Convert.ToInt32(textBox_MainCamBrightnessLower.Text);
            if (true == GeneralUtils.between_two(value, 0, 255))
                parent.m_nMainCamLowerBrightness = value;

            value = Convert.ToInt32(textBox_MainCamBrightnessUpper.Text);
            if (true == GeneralUtils.between_two(value, 0, 255))
                parent.m_nMainCamUpperBrightness = value;

            value = Convert.ToInt32(textBox_GuideCamBrightnessLower.Text);
            if (true == GeneralUtils.between_two(value, 0, 255))
                parent.m_nGuideCamLowerBrightness = value;

            value = Convert.ToInt32(textBox_GuideCamBrightnessUpper.Text);
            if (true == GeneralUtils.between_two(value, 0, 255))
                parent.m_nGuideCamUpperBrightness = value;

            if (true == radio_TopLightForGuideCam.Checked)
                parent.m_nLightTypeForGuideCamForMarkPt = 0;
            else
                parent.m_nLightTypeForGuideCamForMarkPt = 1;

            if (true == radio_TopLightFor14Line.Checked)
                parent.m_nLightTypeFor14Line = 0;
            else
                parent.m_nLightTypeFor14Line = 1;

            if (true == radio_TopLightForLineSpace.Checked)
                parent.m_nLightTypeForLineSpace = 0;
            else
                parent.m_nLightTypeForLineSpace = 1;

            if (true == radio_TopLightForBGA.Checked)
                parent.m_nLightTypeForBGA = 0;
            else
                parent.m_nLightTypeForBGA = 1;

            parent.m_nMeasureResultDigits[0] = comboBox_MeasureResultDigitsForMM.SelectedIndex + 1;
            parent.m_nMeasureResultDigits[1] = comboBox_MeasureResultDigitsForUM.SelectedIndex + 1;
            parent.m_nMeasureResultDigits[2] = comboBox_MeasureResultDigitsForMIL.SelectedIndex + 1;

            parent.m_nLineMeasurementMethod = comboBox_MeasureLineMethod.SelectedIndex;

            parent.m_nSourceOfStandardValue = comboBox_SourceOfStandardValue.SelectedIndex;

            parent.m_bDoNotConfirmMarkAtCreation = checkBox_NoConfirmMarkAtCreation.Checked;

            parent.m_bDoNotConfirmMeasureItemAtCreation = checkBox_NoConfirmMeasureItemAtCreation.Checked;
            
            parent.m_bSelectivelySkipAutofocus = checkBox_SelectivelySkipAutofocus.Checked;
            parent.m_nThresForSkippingAutofocus = Convert.ToInt32(textBox_ThresForSkippingAutofocus.Text);

            parent.SaveAppParams();

            MessageBox.Show(this, "保存成功。", "提示");

            Close();
        }

        // 按钮：取消
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        // 单选框：导航光源上环光
        private void radio_TopLightForGuideCam_CheckedChanged(object sender, EventArgs e)
        {
            if (true == radio_TopLightForGuideCam.Checked)
                radio_BottomLightForGuideCam.Checked = false;
            else
                radio_BottomLightForGuideCam.Checked = true;
        }

        // 单选框：导航光源下环光
        private void radio_BottomLightForGuideCam_CheckedChanged(object sender, EventArgs e)
        {
            if (true == radio_BottomLightForGuideCam.Checked)
                radio_TopLightForGuideCam.Checked = false;
            else
                radio_TopLightForGuideCam.Checked = true;
        }

        // 单选框：下线宽光源上环光
        private void radio_TopLightFor14Line_CheckedChanged(object sender, EventArgs e)
        {
            if (true == radio_TopLightFor14Line.Checked)
                radio_BottomLightFor14Line.Checked = false;
            else
                radio_BottomLightFor14Line.Checked = true;
        }

        // 单选框：下线宽光源下环光
        private void radio_BottomLightFor14Line_CheckedChanged(object sender, EventArgs e)
        {
            if (true == radio_BottomLightFor14Line.Checked)
                radio_TopLightFor14Line.Checked = false;
            else
                radio_TopLightFor14Line.Checked = true;
        }

        // 单选框：线距光源上环光
        private void radio_TopLightForLineSpace_CheckedChanged(object sender, EventArgs e)
        {
            if (true == radio_TopLightForLineSpace.Checked)
                radio_BottomLightForLineSpace.Checked = false;
            else
                radio_BottomLightForLineSpace.Checked = true;
        }

        // 单选框：线距光源下环光
        private void radio_BottomLightForLineSpace_CheckedChanged(object sender, EventArgs e)
        {
            if (true == radio_BottomLightForLineSpace.Checked)
                radio_TopLightForLineSpace.Checked = false;
            else
                radio_TopLightForLineSpace.Checked = true;
        }

        // 单选框：BGA光源上环光
        private void radio_TopLightForBGA_CheckedChanged(object sender, EventArgs e)
        {
            if (true == radio_TopLightForBGA.Checked)
                radio_BottomLightForBGA.Checked = false;
            else
                radio_BottomLightForBGA.Checked = true;
        }

        // 单选框：BGA光源下环光
        private void radio_BottomLightForBGA_CheckedChanged(object sender, EventArgs e)
        {
            if (true == radio_BottomLightForBGA.Checked)
                radio_TopLightForBGA.Checked = false;
            else
                radio_TopLightForBGA.Checked = true;
        }

        private void checkBox_SelectivelySkipAutofocus_CheckedChanged(object sender, EventArgs e)
        {
            //parent.m_bSelectivelySkipAutofocus = checkBox_SelectivelySkipAutofocus.Checked;
        }

        private void checkBox_NoConfirmMarkAtCreation_CheckedChanged(object sender, EventArgs e)
        {

        }
        
        private void textBox_DelaySecondsBeforeMeasure_TextChanged(object sender, EventArgs e)
        {
            parent.m_dbDelaySecondsBeforeMeasure = Convert.ToDouble(textBox_DelaySecondsBeforeMeasure.Text);
        }
        
    }
}
