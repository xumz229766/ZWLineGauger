namespace ZWLineGauger
{
    partial class Form_Miscellaneous
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
            this.checkBox_OfflineMode = new System.Windows.Forms.CheckBox();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_Save = new System.Windows.Forms.Button();
            this.textBox_MeasureTaskDelayTime = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_RestoreFactoryDefaults = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkBox_OfflineMode
            // 
            this.checkBox_OfflineMode.AutoSize = true;
            this.checkBox_OfflineMode.Location = new System.Drawing.Point(77, 44);
            this.checkBox_OfflineMode.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_OfflineMode.Name = "checkBox_OfflineMode";
            this.checkBox_OfflineMode.Size = new System.Drawing.Size(72, 16);
            this.checkBox_OfflineMode.TabIndex = 4;
            this.checkBox_OfflineMode.Text = "离线模式";
            this.checkBox_OfflineMode.UseVisualStyleBackColor = true;
            this.checkBox_OfflineMode.CheckedChanged += new System.EventHandler(this.checkBox_OfflineMode_CheckedChanged);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(364, 244);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(70, 30);
            this.btn_Cancel.TabIndex = 6;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(242, 244);
            this.btn_Save.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(70, 30);
            this.btn_Save.TabIndex = 5;
            this.btn_Save.Text = "保存";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // textBox_MeasureTaskDelayTime
            // 
            this.textBox_MeasureTaskDelayTime.Location = new System.Drawing.Point(427, 98);
            this.textBox_MeasureTaskDelayTime.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_MeasureTaskDelayTime.Name = "textBox_MeasureTaskDelayTime";
            this.textBox_MeasureTaskDelayTime.Size = new System.Drawing.Size(80, 21);
            this.textBox_MeasureTaskDelayTime.TabIndex = 49;
            this.textBox_MeasureTaskDelayTime.TabStop = false;
            this.textBox_MeasureTaskDelayTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(75, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(347, 12);
            this.label2.TabIndex = 48;
            this.label2.Text = "开始运行任务时，为确保吸附到位，第一次测量延迟时间(毫秒):";
            // 
            // btn_RestoreFactoryDefaults
            // 
            this.btn_RestoreFactoryDefaults.Location = new System.Drawing.Point(77, 166);
            this.btn_RestoreFactoryDefaults.Margin = new System.Windows.Forms.Padding(2);
            this.btn_RestoreFactoryDefaults.Name = "btn_RestoreFactoryDefaults";
            this.btn_RestoreFactoryDefaults.Size = new System.Drawing.Size(108, 35);
            this.btn_RestoreFactoryDefaults.TabIndex = 50;
            this.btn_RestoreFactoryDefaults.Text = "恢复出厂设置";
            this.btn_RestoreFactoryDefaults.UseVisualStyleBackColor = true;
            this.btn_RestoreFactoryDefaults.Click += new System.EventHandler(this.btn_RestoreFactoryDefaults_Click);
            // 
            // Form_Miscellaneous
            // 
            this.AcceptButton = this.btn_Save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(695, 304);
            this.Controls.Add(this.btn_RestoreFactoryDefaults);
            this.Controls.Add(this.textBox_MeasureTaskDelayTime);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.checkBox_OfflineMode);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Miscellaneous";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "设置--杂项";
            this.Load += new System.EventHandler(this.Form_Miscellaneous_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_OfflineMode;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.TextBox textBox_MeasureTaskDelayTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_RestoreFactoryDefaults;
    }
}