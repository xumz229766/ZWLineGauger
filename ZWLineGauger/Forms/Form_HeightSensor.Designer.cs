namespace ZWLineGauger.Forms
{
    partial class Form_HeightSensor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_HeightSensor));
            this.gridview_MeasureResults = new System.Windows.Forms.DataGridView();
            this.btn_CalibrateHeightGap = new System.Windows.Forms.Button();
            this.btn_CalibrateStage = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_Average = new System.Windows.Forms.TextBox();
            this.textBox_Stage_Average = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_Stage_Triggering_Height = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_Save = new System.Windows.Forms.Button();
            this.checkBox_UseHeightSensor = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBox_CameraHeightSensorOffsetX = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_CameraHeightSensorOffsetY = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridview_MeasureResults)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridview_MeasureResults
            // 
            this.gridview_MeasureResults.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.gridview_MeasureResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridview_MeasureResults.Location = new System.Drawing.Point(42, 76);
            this.gridview_MeasureResults.Margin = new System.Windows.Forms.Padding(2);
            this.gridview_MeasureResults.Name = "gridview_MeasureResults";
            this.gridview_MeasureResults.RowTemplate.Height = 27;
            this.gridview_MeasureResults.Size = new System.Drawing.Size(381, 306);
            this.gridview_MeasureResults.TabIndex = 36;
            // 
            // btn_CalibrateHeightGap
            // 
            this.btn_CalibrateHeightGap.Location = new System.Drawing.Point(42, 401);
            this.btn_CalibrateHeightGap.Margin = new System.Windows.Forms.Padding(2);
            this.btn_CalibrateHeightGap.Name = "btn_CalibrateHeightGap";
            this.btn_CalibrateHeightGap.Size = new System.Drawing.Size(131, 44);
            this.btn_CalibrateHeightGap.TabIndex = 40;
            this.btn_CalibrateHeightGap.Text = "标定触发高度和对焦清晰高度之间的差值";
            this.btn_CalibrateHeightGap.UseVisualStyleBackColor = true;
            this.btn_CalibrateHeightGap.Click += new System.EventHandler(this.btn_CalibrateHeightGap_Click);
            // 
            // btn_CalibrateStage
            // 
            this.btn_CalibrateStage.Location = new System.Drawing.Point(42, 459);
            this.btn_CalibrateStage.Margin = new System.Windows.Forms.Padding(2);
            this.btn_CalibrateStage.Name = "btn_CalibrateStage";
            this.btn_CalibrateStage.Size = new System.Drawing.Size(131, 42);
            this.btn_CalibrateStage.TabIndex = 41;
            this.btn_CalibrateStage.Text = "标定平台高度";
            this.btn_CalibrateStage.UseVisualStyleBackColor = true;
            this.btn_CalibrateStage.Click += new System.EventHandler(this.btn_CalibrateStage_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(201, 417);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 42;
            this.label1.Text = "高度差均值(mm):";
            // 
            // textBox_Average
            // 
            this.textBox_Average.Location = new System.Drawing.Point(327, 414);
            this.textBox_Average.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_Average.Name = "textBox_Average";
            this.textBox_Average.Size = new System.Drawing.Size(67, 21);
            this.textBox_Average.TabIndex = 43;
            this.textBox_Average.TabStop = false;
            this.textBox_Average.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBox_Stage_Average
            // 
            this.textBox_Stage_Average.Location = new System.Drawing.Point(327, 455);
            this.textBox_Stage_Average.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_Stage_Average.Name = "textBox_Stage_Average";
            this.textBox_Stage_Average.Size = new System.Drawing.Size(67, 21);
            this.textBox_Stage_Average.TabIndex = 45;
            this.textBox_Stage_Average.TabStop = false;
            this.textBox_Stage_Average.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(201, 459);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 12);
            this.label2.TabIndex = 44;
            this.label2.Text = "平台高度差均值(mm):";
            // 
            // textBox_Stage_Triggering_Height
            // 
            this.textBox_Stage_Triggering_Height.Location = new System.Drawing.Point(327, 487);
            this.textBox_Stage_Triggering_Height.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_Stage_Triggering_Height.Name = "textBox_Stage_Triggering_Height";
            this.textBox_Stage_Triggering_Height.Size = new System.Drawing.Size(67, 21);
            this.textBox_Stage_Triggering_Height.TabIndex = 47;
            this.textBox_Stage_Triggering_Height.TabStop = false;
            this.textBox_Stage_Triggering_Height.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(201, 490);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 12);
            this.label3.TabIndex = 46;
            this.label3.Text = "平台触发高度(mm):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(41, 538);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(473, 12);
            this.label4.TabIndex = 48;
            this.label4.Text = "注：标定之前，需要把相机系统移动到一处能够产生图像信息并能有效对焦的平坦位置。";
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(550, 424);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(70, 30);
            this.btn_Cancel.TabIndex = 50;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(449, 424);
            this.btn_Save.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(70, 30);
            this.btn_Save.TabIndex = 49;
            this.btn_Save.Text = "确定";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // checkBox_UseHeightSensor
            // 
            this.checkBox_UseHeightSensor.AutoSize = true;
            this.checkBox_UseHeightSensor.Location = new System.Drawing.Point(64, 38);
            this.checkBox_UseHeightSensor.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_UseHeightSensor.Name = "checkBox_UseHeightSensor";
            this.checkBox_UseHeightSensor.Size = new System.Drawing.Size(168, 16);
            this.checkBox_UseHeightSensor.TabIndex = 59;
            this.checkBox_UseHeightSensor.TabStop = false;
            this.checkBox_UseHeightSensor.Text = "自动对焦时使用高度传感器";
            this.checkBox_UseHeightSensor.UseVisualStyleBackColor = true;
            this.checkBox_UseHeightSensor.CheckedChanged += new System.EventHandler(this.checkBox_UseHeightSensor_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.textBox_CameraHeightSensorOffsetY);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.textBox_CameraHeightSensorOffsetX);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(457, 69);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(182, 110);
            this.groupBox2.TabIndex = 60;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "相机与高度传感器之间的偏移";
            // 
            // textBox_CameraHeightSensorOffsetX
            // 
            this.textBox_CameraHeightSensorOffsetX.Location = new System.Drawing.Point(93, 27);
            this.textBox_CameraHeightSensorOffsetX.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_CameraHeightSensorOffsetX.Name = "textBox_CameraHeightSensorOffsetX";
            this.textBox_CameraHeightSensorOffsetX.Size = new System.Drawing.Size(67, 21);
            this.textBox_CameraHeightSensorOffsetX.TabIndex = 62;
            this.textBox_CameraHeightSensorOffsetX.TabStop = false;
            this.textBox_CameraHeightSensorOffsetX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 61;
            this.label5.Text = "偏移X(mm):";
            // 
            // textBox_CameraHeightSensorOffsetY
            // 
            this.textBox_CameraHeightSensorOffsetY.Location = new System.Drawing.Point(93, 57);
            this.textBox_CameraHeightSensorOffsetY.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_CameraHeightSensorOffsetY.Name = "textBox_CameraHeightSensorOffsetY";
            this.textBox_CameraHeightSensorOffsetY.Size = new System.Drawing.Size(67, 21);
            this.textBox_CameraHeightSensorOffsetY.TabIndex = 64;
            this.textBox_CameraHeightSensorOffsetY.TabStop = false;
            this.textBox_CameraHeightSensorOffsetY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 63;
            this.label6.Text = "偏移Y(mm):";
            // 
            // Form_HeightSensor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(676, 597);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.checkBox_UseHeightSensor);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_Stage_Triggering_Height);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_Stage_Average);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_Average);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_CalibrateStage);
            this.Controls.Add(this.btn_CalibrateHeightGap);
            this.Controls.Add(this.gridview_MeasureResults);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_HeightSensor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "高度传感器设置和标定";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_HeightSensor_FormClosing);
            this.Load += new System.EventHandler(this.Form_HeightSensor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridview_MeasureResults)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView gridview_MeasureResults;
        private System.Windows.Forms.Button btn_CalibrateHeightGap;
        private System.Windows.Forms.Button btn_CalibrateStage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_Average;
        private System.Windows.Forms.TextBox textBox_Stage_Average;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_Stage_Triggering_Height;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.CheckBox checkBox_UseHeightSensor;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBox_CameraHeightSensorOffsetY;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_CameraHeightSensorOffsetX;
        private System.Windows.Forms.Label label5;
    }
}