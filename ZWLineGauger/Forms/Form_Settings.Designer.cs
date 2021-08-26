namespace ZWLineGauger.Forms
{
    partial class Form_Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Settings));
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_OK = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_MainCamBrightnessLower = new System.Windows.Forms.TextBox();
            this.textBox_MainCamBrightnessUpper = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_GuideCamBrightnessUpper = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_GuideCamBrightnessLower = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.radio_TopLightForGuideCam = new System.Windows.Forms.RadioButton();
            this.radio_BottomLightForGuideCam = new System.Windows.Forms.RadioButton();
            this.checkBox_SelectivelySkipAutofocus = new System.Windows.Forms.CheckBox();
            this.textBox_ThresForSkippingAutofocus = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.radio_BottomLightFor14Line = new System.Windows.Forms.RadioButton();
            this.radio_TopLightFor14Line = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.radio_BottomLightForBGA = new System.Windows.Forms.RadioButton();
            this.radio_TopLightForBGA = new System.Windows.Forms.RadioButton();
            this.label8 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.checkBox_NoConfirmMarkAtCreation = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.comboBox_MeasureResultDigitsForMM = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBox_DelaySecondsBeforeMeasure = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.comboBox_MeasureLineMethod = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBox_MeasureResultDigitsForUM = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.comboBox_MeasureResultDigitsForMIL = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.comboBox_SourceOfStandardValue = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.checkBox_NoConfirmMeasureItemAtCreation = new System.Windows.Forms.CheckBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.radio_TopLightForLineSpace = new System.Windows.Forms.RadioButton();
            this.radio_BottomLightForLineSpace = new System.Windows.Forms.RadioButton();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.textBox_MaximumNumOfMeasureItemsForPopupModifyForm = new System.Windows.Forms.TextBox();
            this.checkBox_PopupModifyFormForODBTaskCreation = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Font = new System.Drawing.Font("宋体", 10F);
            this.btn_Cancel.Location = new System.Drawing.Point(433, 581);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(76, 33);
            this.btn_Cancel.TabIndex = 15;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_OK
            // 
            this.btn_OK.Font = new System.Drawing.Font("宋体", 10F);
            this.btn_OK.Location = new System.Drawing.Point(301, 581);
            this.btn_OK.Margin = new System.Windows.Forms.Padding(2);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(76, 33);
            this.btn_OK.TabIndex = 14;
            this.btn_OK.Text = "保存";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(66, 44);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(203, 12);
            this.label4.TabIndex = 20;
            this.label4.Text = "图纸模式做首件时主图像的亮度下限:";
            // 
            // textBox_MainCamBrightnessLower
            // 
            this.textBox_MainCamBrightnessLower.Location = new System.Drawing.Point(283, 40);
            this.textBox_MainCamBrightnessLower.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_MainCamBrightnessLower.Name = "textBox_MainCamBrightnessLower";
            this.textBox_MainCamBrightnessLower.Size = new System.Drawing.Size(61, 21);
            this.textBox_MainCamBrightnessLower.TabIndex = 19;
            this.textBox_MainCamBrightnessLower.TabStop = false;
            // 
            // textBox_MainCamBrightnessUpper
            // 
            this.textBox_MainCamBrightnessUpper.Location = new System.Drawing.Point(398, 40);
            this.textBox_MainCamBrightnessUpper.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_MainCamBrightnessUpper.Name = "textBox_MainCamBrightnessUpper";
            this.textBox_MainCamBrightnessUpper.Size = new System.Drawing.Size(61, 21);
            this.textBox_MainCamBrightnessUpper.TabIndex = 21;
            this.textBox_MainCamBrightnessUpper.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(359, 44);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 22;
            this.label1.Text = "上限:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(359, 77);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 26;
            this.label2.Text = "上限:";
            // 
            // textBox_GuideCamBrightnessUpper
            // 
            this.textBox_GuideCamBrightnessUpper.Location = new System.Drawing.Point(398, 73);
            this.textBox_GuideCamBrightnessUpper.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_GuideCamBrightnessUpper.Name = "textBox_GuideCamBrightnessUpper";
            this.textBox_GuideCamBrightnessUpper.Size = new System.Drawing.Size(61, 21);
            this.textBox_GuideCamBrightnessUpper.TabIndex = 25;
            this.textBox_GuideCamBrightnessUpper.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(66, 77);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(215, 12);
            this.label3.TabIndex = 24;
            this.label3.Text = "图纸模式做首件时导航图像的亮度下限:";
            // 
            // textBox_GuideCamBrightnessLower
            // 
            this.textBox_GuideCamBrightnessLower.Location = new System.Drawing.Point(283, 73);
            this.textBox_GuideCamBrightnessLower.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_GuideCamBrightnessLower.Name = "textBox_GuideCamBrightnessLower";
            this.textBox_GuideCamBrightnessLower.Size = new System.Drawing.Size(61, 21);
            this.textBox_GuideCamBrightnessLower.TabIndex = 23;
            this.textBox_GuideCamBrightnessLower.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(66, 161);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(191, 12);
            this.label5.TabIndex = 27;
            this.label5.Text = "导航图像识别定位孔默认使用光源:";
            // 
            // radio_TopLightForGuideCam
            // 
            this.radio_TopLightForGuideCam.AutoSize = true;
            this.radio_TopLightForGuideCam.Location = new System.Drawing.Point(15, 3);
            this.radio_TopLightForGuideCam.Name = "radio_TopLightForGuideCam";
            this.radio_TopLightForGuideCam.Size = new System.Drawing.Size(59, 16);
            this.radio_TopLightForGuideCam.TabIndex = 28;
            this.radio_TopLightForGuideCam.TabStop = true;
            this.radio_TopLightForGuideCam.Text = "上环光";
            this.radio_TopLightForGuideCam.UseVisualStyleBackColor = true;
            this.radio_TopLightForGuideCam.CheckedChanged += new System.EventHandler(this.radio_TopLightForGuideCam_CheckedChanged);
            // 
            // radio_BottomLightForGuideCam
            // 
            this.radio_BottomLightForGuideCam.AutoSize = true;
            this.radio_BottomLightForGuideCam.Location = new System.Drawing.Point(82, 3);
            this.radio_BottomLightForGuideCam.Name = "radio_BottomLightForGuideCam";
            this.radio_BottomLightForGuideCam.Size = new System.Drawing.Size(59, 16);
            this.radio_BottomLightForGuideCam.TabIndex = 29;
            this.radio_BottomLightForGuideCam.TabStop = true;
            this.radio_BottomLightForGuideCam.Text = "下环光";
            this.radio_BottomLightForGuideCam.UseVisualStyleBackColor = true;
            this.radio_BottomLightForGuideCam.CheckedChanged += new System.EventHandler(this.radio_BottomLightForGuideCam_CheckedChanged);
            // 
            // checkBox_SelectivelySkipAutofocus
            // 
            this.checkBox_SelectivelySkipAutofocus.AutoSize = true;
            this.checkBox_SelectivelySkipAutofocus.Location = new System.Drawing.Point(68, 382);
            this.checkBox_SelectivelySkipAutofocus.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_SelectivelySkipAutofocus.Name = "checkBox_SelectivelySkipAutofocus";
            this.checkBox_SelectivelySkipAutofocus.Size = new System.Drawing.Size(324, 16);
            this.checkBox_SelectivelySkipAutofocus.TabIndex = 32;
            this.checkBox_SelectivelySkipAutofocus.Text = "自动测量过程中，如果测量位置清晰度达到首件清晰度的";
            this.checkBox_SelectivelySkipAutofocus.UseVisualStyleBackColor = true;
            this.checkBox_SelectivelySkipAutofocus.CheckedChanged += new System.EventHandler(this.checkBox_SelectivelySkipAutofocus_CheckedChanged);
            // 
            // textBox_ThresForSkippingAutofocus
            // 
            this.textBox_ThresForSkippingAutofocus.Location = new System.Drawing.Point(390, 379);
            this.textBox_ThresForSkippingAutofocus.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_ThresForSkippingAutofocus.Name = "textBox_ThresForSkippingAutofocus";
            this.textBox_ThresForSkippingAutofocus.Size = new System.Drawing.Size(61, 21);
            this.textBox_ThresForSkippingAutofocus.TabIndex = 33;
            this.textBox_ThresForSkippingAutofocus.TabStop = false;
            this.textBox_ThresForSkippingAutofocus.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(455, 383);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(119, 12);
            this.label6.TabIndex = 34;
            this.label6.Text = "%，则不进行自动对焦";
            // 
            // radio_BottomLightFor14Line
            // 
            this.radio_BottomLightFor14Line.AutoSize = true;
            this.radio_BottomLightFor14Line.Location = new System.Drawing.Point(82, 3);
            this.radio_BottomLightFor14Line.Name = "radio_BottomLightFor14Line";
            this.radio_BottomLightFor14Line.Size = new System.Drawing.Size(59, 16);
            this.radio_BottomLightFor14Line.TabIndex = 37;
            this.radio_BottomLightFor14Line.TabStop = true;
            this.radio_BottomLightFor14Line.Text = "下环光";
            this.radio_BottomLightFor14Line.UseVisualStyleBackColor = true;
            this.radio_BottomLightFor14Line.CheckedChanged += new System.EventHandler(this.radio_BottomLightFor14Line_CheckedChanged);
            // 
            // radio_TopLightFor14Line
            // 
            this.radio_TopLightFor14Line.AutoSize = true;
            this.radio_TopLightFor14Line.Location = new System.Drawing.Point(15, 3);
            this.radio_TopLightFor14Line.Name = "radio_TopLightFor14Line";
            this.radio_TopLightFor14Line.Size = new System.Drawing.Size(59, 16);
            this.radio_TopLightFor14Line.TabIndex = 36;
            this.radio_TopLightFor14Line.TabStop = true;
            this.radio_TopLightFor14Line.Text = "上环光";
            this.radio_TopLightFor14Line.UseVisualStyleBackColor = true;
            this.radio_TopLightFor14Line.CheckedChanged += new System.EventHandler(this.radio_TopLightFor14Line_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(66, 193);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(191, 12);
            this.label7.TabIndex = 35;
            this.label7.Text = "做首件时测量下线宽默认使用光源:";
            // 
            // radio_BottomLightForBGA
            // 
            this.radio_BottomLightForBGA.AutoSize = true;
            this.radio_BottomLightForBGA.Location = new System.Drawing.Point(82, 3);
            this.radio_BottomLightForBGA.Name = "radio_BottomLightForBGA";
            this.radio_BottomLightForBGA.Size = new System.Drawing.Size(59, 16);
            this.radio_BottomLightForBGA.TabIndex = 40;
            this.radio_BottomLightForBGA.TabStop = true;
            this.radio_BottomLightForBGA.Text = "下环光";
            this.radio_BottomLightForBGA.UseVisualStyleBackColor = true;
            this.radio_BottomLightForBGA.CheckedChanged += new System.EventHandler(this.radio_BottomLightForBGA_CheckedChanged);
            // 
            // radio_TopLightForBGA
            // 
            this.radio_TopLightForBGA.AutoSize = true;
            this.radio_TopLightForBGA.Location = new System.Drawing.Point(15, 3);
            this.radio_TopLightForBGA.Name = "radio_TopLightForBGA";
            this.radio_TopLightForBGA.Size = new System.Drawing.Size(59, 16);
            this.radio_TopLightForBGA.TabIndex = 39;
            this.radio_TopLightForBGA.TabStop = true;
            this.radio_TopLightForBGA.Text = "上环光";
            this.radio_TopLightForBGA.UseVisualStyleBackColor = true;
            this.radio_TopLightForBGA.CheckedChanged += new System.EventHandler(this.radio_TopLightForBGA_CheckedChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(66, 257);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(173, 12);
            this.label8.TabIndex = 38;
            this.label8.Text = "做首件时测量BGA默认使用光源:";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radio_BottomLightForGuideCam);
            this.panel1.Controls.Add(this.radio_TopLightForGuideCam);
            this.panel1.Location = new System.Drawing.Point(261, 157);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(165, 22);
            this.panel1.TabIndex = 41;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.radio_TopLightFor14Line);
            this.panel2.Controls.Add(this.radio_BottomLightFor14Line);
            this.panel2.Location = new System.Drawing.Point(261, 189);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(152, 26);
            this.panel2.TabIndex = 42;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.radio_BottomLightForBGA);
            this.panel3.Controls.Add(this.radio_TopLightForBGA);
            this.panel3.Location = new System.Drawing.Point(261, 253);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(146, 25);
            this.panel3.TabIndex = 43;
            // 
            // checkBox_NoConfirmMarkAtCreation
            // 
            this.checkBox_NoConfirmMarkAtCreation.AutoSize = true;
            this.checkBox_NoConfirmMarkAtCreation.Location = new System.Drawing.Point(68, 300);
            this.checkBox_NoConfirmMarkAtCreation.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_NoConfirmMarkAtCreation.Name = "checkBox_NoConfirmMarkAtCreation";
            this.checkBox_NoConfirmMarkAtCreation.Size = new System.Drawing.Size(306, 16);
            this.checkBox_NoConfirmMarkAtCreation.TabIndex = 44;
            this.checkBox_NoConfirmMarkAtCreation.Text = "制作首件时，不需要用户确认“是否采用该定位孔?”";
            this.checkBox_NoConfirmMarkAtCreation.UseVisualStyleBackColor = true;
            this.checkBox_NoConfirmMarkAtCreation.CheckedChanged += new System.EventHandler(this.checkBox_NoConfirmMarkAtCreation_CheckedChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(66, 426);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(143, 12);
            this.label9.TabIndex = 46;
            this.label9.Text = "测量结果小数点位数(mm):";
            // 
            // comboBox_MeasureResultDigitsForMM
            // 
            this.comboBox_MeasureResultDigitsForMM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_MeasureResultDigitsForMM.FormattingEnabled = true;
            this.comboBox_MeasureResultDigitsForMM.Location = new System.Drawing.Point(215, 423);
            this.comboBox_MeasureResultDigitsForMM.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_MeasureResultDigitsForMM.Name = "comboBox_MeasureResultDigitsForMM";
            this.comboBox_MeasureResultDigitsForMM.Size = new System.Drawing.Size(38, 20);
            this.comboBox_MeasureResultDigitsForMM.TabIndex = 47;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(66, 529);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(197, 12);
            this.label10.TabIndex = 49;
            this.label10.Text = "自动测量时，到达测量点位置后延迟";
            // 
            // textBox_DelaySecondsBeforeMeasure
            // 
            this.textBox_DelaySecondsBeforeMeasure.Location = new System.Drawing.Point(269, 525);
            this.textBox_DelaySecondsBeforeMeasure.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_DelaySecondsBeforeMeasure.Name = "textBox_DelaySecondsBeforeMeasure";
            this.textBox_DelaySecondsBeforeMeasure.Size = new System.Drawing.Size(53, 21);
            this.textBox_DelaySecondsBeforeMeasure.TabIndex = 48;
            this.textBox_DelaySecondsBeforeMeasure.TabStop = false;
            this.textBox_DelaySecondsBeforeMeasure.TextChanged += new System.EventHandler(this.textBox_DelaySecondsBeforeMeasure_TextChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(326, 529);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(281, 12);
            this.label11.TabIndex = 50;
            this.label11.Text = "秒才开始测量(设置适当延迟可以使测量结果更稳定)";
            // 
            // comboBox_MeasureLineMethod
            // 
            this.comboBox_MeasureLineMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_MeasureLineMethod.FormattingEnabled = true;
            this.comboBox_MeasureLineMethod.Location = new System.Drawing.Point(129, 116);
            this.comboBox_MeasureLineMethod.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_MeasureLineMethod.Name = "comboBox_MeasureLineMethod";
            this.comboBox_MeasureLineMethod.Size = new System.Drawing.Size(128, 20);
            this.comboBox_MeasureLineMethod.TabIndex = 52;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(66, 119);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(59, 12);
            this.label12.TabIndex = 51;
            this.label12.Text = "测量方式:";
            // 
            // comboBox_MeasureResultDigitsForUM
            // 
            this.comboBox_MeasureResultDigitsForUM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_MeasureResultDigitsForUM.FormattingEnabled = true;
            this.comboBox_MeasureResultDigitsForUM.Location = new System.Drawing.Point(215, 453);
            this.comboBox_MeasureResultDigitsForUM.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_MeasureResultDigitsForUM.Name = "comboBox_MeasureResultDigitsForUM";
            this.comboBox_MeasureResultDigitsForUM.Size = new System.Drawing.Size(38, 20);
            this.comboBox_MeasureResultDigitsForUM.TabIndex = 54;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(66, 456);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(143, 12);
            this.label13.TabIndex = 53;
            this.label13.Text = "测量结果小数点位数(mm):";
            // 
            // comboBox_MeasureResultDigitsForMIL
            // 
            this.comboBox_MeasureResultDigitsForMIL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_MeasureResultDigitsForMIL.FormattingEnabled = true;
            this.comboBox_MeasureResultDigitsForMIL.Location = new System.Drawing.Point(215, 483);
            this.comboBox_MeasureResultDigitsForMIL.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_MeasureResultDigitsForMIL.Name = "comboBox_MeasureResultDigitsForMIL";
            this.comboBox_MeasureResultDigitsForMIL.Size = new System.Drawing.Size(38, 20);
            this.comboBox_MeasureResultDigitsForMIL.TabIndex = 56;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(66, 486);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(143, 12);
            this.label14.TabIndex = 55;
            this.label14.Text = "测量结果小数点位数(mm):";
            // 
            // comboBox_SourceOfStandardValue
            // 
            this.comboBox_SourceOfStandardValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_SourceOfStandardValue.FormattingEnabled = true;
            this.comboBox_SourceOfStandardValue.Location = new System.Drawing.Point(539, 423);
            this.comboBox_SourceOfStandardValue.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_SourceOfStandardValue.Name = "comboBox_SourceOfStandardValue";
            this.comboBox_SourceOfStandardValue.Size = new System.Drawing.Size(126, 20);
            this.comboBox_SourceOfStandardValue.TabIndex = 58;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(394, 426);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(143, 12);
            this.label15.TabIndex = 57;
            this.label15.Text = "图纸测量项目标准值来源:";
            // 
            // checkBox_NoConfirmMeasureItemAtCreation
            // 
            this.checkBox_NoConfirmMeasureItemAtCreation.AutoSize = true;
            this.checkBox_NoConfirmMeasureItemAtCreation.Location = new System.Drawing.Point(68, 327);
            this.checkBox_NoConfirmMeasureItemAtCreation.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_NoConfirmMeasureItemAtCreation.Name = "checkBox_NoConfirmMeasureItemAtCreation";
            this.checkBox_NoConfirmMeasureItemAtCreation.Size = new System.Drawing.Size(354, 16);
            this.checkBox_NoConfirmMeasureItemAtCreation.TabIndex = 59;
            this.checkBox_NoConfirmMeasureItemAtCreation.Text = "制作首件时，不需要用户确认“是否添加该测量项到任务表?”";
            this.checkBox_NoConfirmMeasureItemAtCreation.UseVisualStyleBackColor = true;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.radio_TopLightForLineSpace);
            this.panel4.Controls.Add(this.radio_BottomLightForLineSpace);
            this.panel4.Location = new System.Drawing.Point(261, 222);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(152, 26);
            this.panel4.TabIndex = 44;
            // 
            // radio_TopLightForLineSpace
            // 
            this.radio_TopLightForLineSpace.AutoSize = true;
            this.radio_TopLightForLineSpace.Location = new System.Drawing.Point(15, 3);
            this.radio_TopLightForLineSpace.Name = "radio_TopLightForLineSpace";
            this.radio_TopLightForLineSpace.Size = new System.Drawing.Size(59, 16);
            this.radio_TopLightForLineSpace.TabIndex = 36;
            this.radio_TopLightForLineSpace.TabStop = true;
            this.radio_TopLightForLineSpace.Text = "上环光";
            this.radio_TopLightForLineSpace.UseVisualStyleBackColor = true;
            this.radio_TopLightForLineSpace.CheckedChanged += new System.EventHandler(this.radio_TopLightForLineSpace_CheckedChanged);
            // 
            // radio_BottomLightForLineSpace
            // 
            this.radio_BottomLightForLineSpace.AutoSize = true;
            this.radio_BottomLightForLineSpace.Location = new System.Drawing.Point(82, 3);
            this.radio_BottomLightForLineSpace.Name = "radio_BottomLightForLineSpace";
            this.radio_BottomLightForLineSpace.Size = new System.Drawing.Size(59, 16);
            this.radio_BottomLightForLineSpace.TabIndex = 37;
            this.radio_BottomLightForLineSpace.TabStop = true;
            this.radio_BottomLightForLineSpace.Text = "下环光";
            this.radio_BottomLightForLineSpace.UseVisualStyleBackColor = true;
            this.radio_BottomLightForLineSpace.CheckedChanged += new System.EventHandler(this.radio_BottomLightForLineSpace_CheckedChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(66, 226);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(179, 12);
            this.label16.TabIndex = 43;
            this.label16.Text = "做首件时测量线距默认使用光源:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(438, 355);
            this.label17.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(125, 12);
            this.label17.TabIndex = 62;
            this.label17.Text = "个，则弹出名称修改框";
            // 
            // textBox_MaximumNumOfMeasureItemsForPopupModifyForm
            // 
            this.textBox_MaximumNumOfMeasureItemsForPopupModifyForm.Location = new System.Drawing.Point(373, 351);
            this.textBox_MaximumNumOfMeasureItemsForPopupModifyForm.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_MaximumNumOfMeasureItemsForPopupModifyForm.Name = "textBox_MaximumNumOfMeasureItemsForPopupModifyForm";
            this.textBox_MaximumNumOfMeasureItemsForPopupModifyForm.Size = new System.Drawing.Size(61, 21);
            this.textBox_MaximumNumOfMeasureItemsForPopupModifyForm.TabIndex = 61;
            this.textBox_MaximumNumOfMeasureItemsForPopupModifyForm.TabStop = false;
            this.textBox_MaximumNumOfMeasureItemsForPopupModifyForm.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // checkBox_PopupModifyFormForODBTaskCreation
            // 
            this.checkBox_PopupModifyFormForODBTaskCreation.AutoSize = true;
            this.checkBox_PopupModifyFormForODBTaskCreation.Location = new System.Drawing.Point(68, 354);
            this.checkBox_PopupModifyFormForODBTaskCreation.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_PopupModifyFormForODBTaskCreation.Name = "checkBox_PopupModifyFormForODBTaskCreation";
            this.checkBox_PopupModifyFormForODBTaskCreation.Size = new System.Drawing.Size(306, 16);
            this.checkBox_PopupModifyFormForODBTaskCreation.TabIndex = 60;
            this.checkBox_PopupModifyFormForODBTaskCreation.Text = "制作基于ODB标记资料的首件时，如果测量项数量少于";
            this.checkBox_PopupModifyFormForODBTaskCreation.UseVisualStyleBackColor = true;
            this.checkBox_PopupModifyFormForODBTaskCreation.CheckedChanged += new System.EventHandler(this.checkBox_PopupModifyFormForODBTaskCreation_CheckedChanged);
            // 
            // Form_Settings
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(829, 638);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.textBox_MaximumNumOfMeasureItemsForPopupModifyForm);
            this.Controls.Add(this.checkBox_PopupModifyFormForODBTaskCreation);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.checkBox_NoConfirmMeasureItemAtCreation);
            this.Controls.Add(this.comboBox_SourceOfStandardValue);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.comboBox_MeasureResultDigitsForMIL);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.comboBox_MeasureResultDigitsForUM);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.comboBox_MeasureLineMethod);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.textBox_DelaySecondsBeforeMeasure);
            this.Controls.Add(this.comboBox_MeasureResultDigitsForMM);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.checkBox_NoConfirmMarkAtCreation);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox_ThresForSkippingAutofocus);
            this.Controls.Add(this.checkBox_SelectivelySkipAutofocus);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_GuideCamBrightnessUpper);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_GuideCamBrightnessLower);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_MainCamBrightnessUpper);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_MainCamBrightnessLower);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_Settings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "设置";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_MainCamBrightnessLower;
        private System.Windows.Forms.TextBox textBox_MainCamBrightnessUpper;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_GuideCamBrightnessUpper;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_GuideCamBrightnessLower;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton radio_TopLightForGuideCam;
        private System.Windows.Forms.RadioButton radio_BottomLightForGuideCam;
        private System.Windows.Forms.CheckBox checkBox_SelectivelySkipAutofocus;
        private System.Windows.Forms.TextBox textBox_ThresForSkippingAutofocus;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton radio_BottomLightFor14Line;
        private System.Windows.Forms.RadioButton radio_TopLightFor14Line;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RadioButton radio_BottomLightForBGA;
        private System.Windows.Forms.RadioButton radio_TopLightForBGA;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.CheckBox checkBox_NoConfirmMarkAtCreation;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox comboBox_MeasureResultDigitsForMM;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox_DelaySecondsBeforeMeasure;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox comboBox_MeasureLineMethod;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox comboBox_MeasureResultDigitsForUM;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox comboBox_MeasureResultDigitsForMIL;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox comboBox_SourceOfStandardValue;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.CheckBox checkBox_NoConfirmMeasureItemAtCreation;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.RadioButton radio_TopLightForLineSpace;
        private System.Windows.Forms.RadioButton radio_BottomLightForLineSpace;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox textBox_MaximumNumOfMeasureItemsForPopupModifyForm;
        private System.Windows.Forms.CheckBox checkBox_PopupModifyFormForODBTaskCreation;
    }
}