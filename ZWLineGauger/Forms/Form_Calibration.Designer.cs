namespace ZWLineGauger.Forms
{
    partial class Form_Calibration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Calibration));
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_Save = new System.Windows.Forms.Button();
            this.comboBox_LenRatio = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.textBox_PhysicalLength = new System.Windows.Forms.TextBox();
            this.gridview_MeasureResults = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_CalibResult = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_Clear = new System.Windows.Forms.Button();
            this.gridview_RatiosAndResults = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ui_btn_13LineWidth = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.nudmmpixcel = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.btnUpdata = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gridview_MeasureResults)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridview_RatiosAndResults)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudmmpixcel)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(395, 512);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(70, 30);
            this.btn_Cancel.TabIndex = 30;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(272, 512);
            this.btn_Save.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(70, 30);
            this.btn_Save.TabIndex = 29;
            this.btn_Save.Text = "保存";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // comboBox_LenRatio
            // 
            this.comboBox_LenRatio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_LenRatio.FormattingEnabled = true;
            this.comboBox_LenRatio.Location = new System.Drawing.Point(367, 43);
            this.comboBox_LenRatio.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_LenRatio.Name = "comboBox_LenRatio";
            this.comboBox_LenRatio.Size = new System.Drawing.Size(66, 20);
            this.comboBox_LenRatio.TabIndex = 32;
            this.comboBox_LenRatio.SelectedIndexChanged += new System.EventHandler(this.comboBox_LenRatio_SelectedIndexChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(326, 46);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(35, 12);
            this.label16.TabIndex = 31;
            this.label16.Text = "倍率:";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(195, 391);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(83, 12);
            this.label13.TabIndex = 34;
            this.label13.Text = "对应物理长度:";
            // 
            // textBox_PhysicalLength
            // 
            this.textBox_PhysicalLength.Location = new System.Drawing.Point(278, 388);
            this.textBox_PhysicalLength.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_PhysicalLength.Name = "textBox_PhysicalLength";
            this.textBox_PhysicalLength.Size = new System.Drawing.Size(61, 21);
            this.textBox_PhysicalLength.TabIndex = 33;
            this.textBox_PhysicalLength.TabStop = false;
            this.textBox_PhysicalLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // gridview_MeasureResults
            // 
            this.gridview_MeasureResults.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.gridview_MeasureResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridview_MeasureResults.Location = new System.Drawing.Point(77, 94);
            this.gridview_MeasureResults.Margin = new System.Windows.Forms.Padding(2);
            this.gridview_MeasureResults.Name = "gridview_MeasureResults";
            this.gridview_MeasureResults.RowTemplate.Height = 27;
            this.gridview_MeasureResults.Size = new System.Drawing.Size(273, 270);
            this.gridview_MeasureResults.TabIndex = 35;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(195, 428);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 37;
            this.label1.Text = "标定结果:";
            // 
            // textBox_CalibResult
            // 
            this.textBox_CalibResult.Enabled = false;
            this.textBox_CalibResult.Location = new System.Drawing.Point(278, 425);
            this.textBox_CalibResult.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_CalibResult.Name = "textBox_CalibResult";
            this.textBox_CalibResult.Size = new System.Drawing.Size(61, 21);
            this.textBox_CalibResult.TabIndex = 36;
            this.textBox_CalibResult.TabStop = false;
            this.textBox_CalibResult.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(342, 428);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 38;
            this.label2.Text = "像素/微米";
            // 
            // btn_Clear
            // 
            this.btn_Clear.Location = new System.Drawing.Point(89, 388);
            this.btn_Clear.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Clear.Name = "btn_Clear";
            this.btn_Clear.Size = new System.Drawing.Size(56, 29);
            this.btn_Clear.TabIndex = 39;
            this.btn_Clear.Text = "清空";
            this.btn_Clear.UseVisualStyleBackColor = true;
            this.btn_Clear.Click += new System.EventHandler(this.btn_Clear_Click);
            // 
            // gridview_RatiosAndResults
            // 
            this.gridview_RatiosAndResults.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.gridview_RatiosAndResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridview_RatiosAndResults.Location = new System.Drawing.Point(386, 94);
            this.gridview_RatiosAndResults.Margin = new System.Windows.Forms.Padding(2);
            this.gridview_RatiosAndResults.Name = "gridview_RatiosAndResults";
            this.gridview_RatiosAndResults.RowTemplate.Height = 27;
            this.gridview_RatiosAndResults.Size = new System.Drawing.Size(266, 270);
            this.gridview_RatiosAndResults.TabIndex = 40;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(342, 391);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 41;
            this.label3.Text = "微米";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(87, 472);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(377, 12);
            this.label4.TabIndex = 42;
            this.label4.Text = "提示: 请使用13线宽工具进行标定。标定时主图像显示单位PX为像素。";
            // 
            // ui_btn_13LineWidth
            // 
            this.ui_btn_13LineWidth.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.ui_btn_13LineWidth.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ui_btn_13LineWidth.FlatAppearance.BorderSize = 0;
            this.ui_btn_13LineWidth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ui_btn_13LineWidth.Image = ((System.Drawing.Image)(resources.GetObject("ui_btn_13LineWidth.Image")));
            this.ui_btn_13LineWidth.Location = new System.Drawing.Point(216, 29);
            this.ui_btn_13LineWidth.Margin = new System.Windows.Forms.Padding(2);
            this.ui_btn_13LineWidth.Name = "ui_btn_13LineWidth";
            this.ui_btn_13LineWidth.Size = new System.Drawing.Size(47, 39);
            this.ui_btn_13LineWidth.TabIndex = 43;
            this.ui_btn_13LineWidth.TabStop = false;
            this.ui_btn_13LineWidth.UseVisualStyleBackColor = false;
            this.ui_btn_13LineWidth.Click += new System.EventHandler(this.ui_btn_13LineWidth_Click);
            this.ui_btn_13LineWidth.MouseEnter += new System.EventHandler(this.ui_btn_13LineWidth_MouseEnter);
            this.ui_btn_13LineWidth.MouseLeave += new System.EventHandler(this.ui_btn_13LineWidth_MouseLeave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(75, 46);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(137, 12);
            this.label5.TabIndex = 44;
            this.label5.Text = "请选择此标定专用工具：";
            // 
            // nudmmpixcel
            // 
            this.nudmmpixcel.DecimalPlaces = 3;
            this.nudmmpixcel.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.nudmmpixcel.Location = new System.Drawing.Point(501, 388);
            this.nudmmpixcel.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.nudmmpixcel.Name = "nudmmpixcel";
            this.nudmmpixcel.Size = new System.Drawing.Size(88, 21);
            this.nudmmpixcel.TabIndex = 45;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(413, 391);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 12);
            this.label6.TabIndex = 37;
            this.label6.Text = "手动修改结果:";
            // 
            // btnUpdata
            // 
            this.btnUpdata.Location = new System.Drawing.Point(603, 382);
            this.btnUpdata.Margin = new System.Windows.Forms.Padding(2);
            this.btnUpdata.Name = "btnUpdata";
            this.btnUpdata.Size = new System.Drawing.Size(49, 30);
            this.btnUpdata.TabIndex = 29;
            this.btnUpdata.Text = "更新";
            this.btnUpdata.UseVisualStyleBackColor = true;
            this.btnUpdata.Click += new System.EventHandler(this.btnUpdata_Click);
            // 
            // Form_Calibration
            // 
            this.AcceptButton = this.btn_Save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(725, 571);
            this.Controls.Add(this.nudmmpixcel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ui_btn_13LineWidth);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.gridview_RatiosAndResults);
            this.Controls.Add(this.btn_Clear);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_CalibResult);
            this.Controls.Add(this.gridview_MeasureResults);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.textBox_PhysicalLength);
            this.Controls.Add(this.comboBox_LenRatio);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btnUpdata);
            this.Controls.Add(this.btn_Save);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Calibration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "标定";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form_Calibration_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridview_MeasureResults)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridview_RatiosAndResults)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudmmpixcel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.ComboBox comboBox_LenRatio;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBox_PhysicalLength;
        private System.Windows.Forms.DataGridView gridview_MeasureResults;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_CalibResult;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_Clear;
        private System.Windows.Forms.DataGridView gridview_RatiosAndResults;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.Button ui_btn_13LineWidth;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nudmmpixcel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnUpdata;
    }
}