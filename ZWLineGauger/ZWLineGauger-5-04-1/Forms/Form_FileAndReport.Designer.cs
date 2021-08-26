namespace ZWLineGauger
{
    partial class Form_FileAndReport
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_AutoSaveResultImage = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_ImageSavingDir = new System.Windows.Forms.TextBox();
            this.btn_SelectImageSavingDir = new System.Windows.Forms.Button();
            this.btn_Save = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox_AutoSaveResultExcel = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_ExcelSavingDir = new System.Windows.Forms.TextBox();
            this.btn_SelectExcelSavingDir = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_TaskFileSavingDir = new System.Windows.Forms.TextBox();
            this.btn_SelectTaskFileSavingDir = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_AutoSaveResultImage);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBox_ImageSavingDir);
            this.groupBox1.Controls.Add(this.btn_SelectImageSavingDir);
            this.groupBox1.Location = new System.Drawing.Point(38, 165);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(737, 103);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "图片设置";
            // 
            // checkBox_AutoSaveResultImage
            // 
            this.checkBox_AutoSaveResultImage.AutoSize = true;
            this.checkBox_AutoSaveResultImage.Location = new System.Drawing.Point(17, 57);
            this.checkBox_AutoSaveResultImage.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_AutoSaveResultImage.Name = "checkBox_AutoSaveResultImage";
            this.checkBox_AutoSaveResultImage.Size = new System.Drawing.Size(228, 16);
            this.checkBox_AutoSaveResultImage.TabIndex = 3;
            this.checkBox_AutoSaveResultImage.Text = "执行测量任务时自动保存测量结果图片";
            this.checkBox_AutoSaveResultImage.UseVisualStyleBackColor = true;
            this.checkBox_AutoSaveResultImage.CheckedChanged += new System.EventHandler(this.checkBox_AutoSaveResultImage_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "测量图片保存目录:";
            // 
            // textBox_ImageSavingDir
            // 
            this.textBox_ImageSavingDir.Location = new System.Drawing.Point(123, 23);
            this.textBox_ImageSavingDir.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_ImageSavingDir.Name = "textBox_ImageSavingDir";
            this.textBox_ImageSavingDir.Size = new System.Drawing.Size(496, 21);
            this.textBox_ImageSavingDir.TabIndex = 1;
            // 
            // btn_SelectImageSavingDir
            // 
            this.btn_SelectImageSavingDir.Location = new System.Drawing.Point(637, 19);
            this.btn_SelectImageSavingDir.Margin = new System.Windows.Forms.Padding(2);
            this.btn_SelectImageSavingDir.Name = "btn_SelectImageSavingDir";
            this.btn_SelectImageSavingDir.Size = new System.Drawing.Size(70, 30);
            this.btn_SelectImageSavingDir.TabIndex = 2;
            this.btn_SelectImageSavingDir.Text = "选择目录";
            this.btn_SelectImageSavingDir.UseVisualStyleBackColor = true;
            this.btn_SelectImageSavingDir.Click += new System.EventHandler(this.btn_SelectImageSavingDir_Click);
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(299, 473);
            this.btn_Save.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(70, 30);
            this.btn_Save.TabIndex = 0;
            this.btn_Save.Text = "保存";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(423, 473);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(70, 30);
            this.btn_Cancel.TabIndex = 1;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox_AutoSaveResultExcel);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.textBox_ExcelSavingDir);
            this.groupBox2.Controls.Add(this.btn_SelectExcelSavingDir);
            this.groupBox2.Location = new System.Drawing.Point(38, 305);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(737, 103);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "报表设置";
            // 
            // checkBox_AutoSaveResultExcel
            // 
            this.checkBox_AutoSaveResultExcel.AutoSize = true;
            this.checkBox_AutoSaveResultExcel.Location = new System.Drawing.Point(17, 57);
            this.checkBox_AutoSaveResultExcel.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_AutoSaveResultExcel.Name = "checkBox_AutoSaveResultExcel";
            this.checkBox_AutoSaveResultExcel.Size = new System.Drawing.Size(252, 16);
            this.checkBox_AutoSaveResultExcel.TabIndex = 3;
            this.checkBox_AutoSaveResultExcel.Text = "测量任务完成时自动保存测量结果电子表格";
            this.checkBox_AutoSaveResultExcel.UseVisualStyleBackColor = true;
            this.checkBox_AutoSaveResultExcel.CheckedChanged += new System.EventHandler(this.checkBox_AutoSaveResultExcel_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 26);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "测量报表保存目录:";
            // 
            // textBox_ExcelSavingDir
            // 
            this.textBox_ExcelSavingDir.Location = new System.Drawing.Point(123, 23);
            this.textBox_ExcelSavingDir.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_ExcelSavingDir.Name = "textBox_ExcelSavingDir";
            this.textBox_ExcelSavingDir.Size = new System.Drawing.Size(496, 21);
            this.textBox_ExcelSavingDir.TabIndex = 1;
            // 
            // btn_SelectExcelSavingDir
            // 
            this.btn_SelectExcelSavingDir.Location = new System.Drawing.Point(637, 19);
            this.btn_SelectExcelSavingDir.Margin = new System.Windows.Forms.Padding(2);
            this.btn_SelectExcelSavingDir.Name = "btn_SelectExcelSavingDir";
            this.btn_SelectExcelSavingDir.Size = new System.Drawing.Size(70, 30);
            this.btn_SelectExcelSavingDir.TabIndex = 2;
            this.btn_SelectExcelSavingDir.Text = "选择目录";
            this.btn_SelectExcelSavingDir.UseVisualStyleBackColor = true;
            this.btn_SelectExcelSavingDir.Click += new System.EventHandler(this.btn_SelectExcelSavingDir_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.textBox_TaskFileSavingDir);
            this.groupBox3.Controls.Add(this.btn_SelectTaskFileSavingDir);
            this.groupBox3.Location = new System.Drawing.Point(38, 26);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(737, 103);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "任务文件设置";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 26);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "任务文件保存目录:";
            // 
            // textBox_TaskFileSavingDir
            // 
            this.textBox_TaskFileSavingDir.Location = new System.Drawing.Point(123, 23);
            this.textBox_TaskFileSavingDir.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_TaskFileSavingDir.Name = "textBox_TaskFileSavingDir";
            this.textBox_TaskFileSavingDir.Size = new System.Drawing.Size(496, 21);
            this.textBox_TaskFileSavingDir.TabIndex = 1;
            // 
            // btn_SelectTaskFileSavingDir
            // 
            this.btn_SelectTaskFileSavingDir.Location = new System.Drawing.Point(637, 19);
            this.btn_SelectTaskFileSavingDir.Margin = new System.Windows.Forms.Padding(2);
            this.btn_SelectTaskFileSavingDir.Name = "btn_SelectTaskFileSavingDir";
            this.btn_SelectTaskFileSavingDir.Size = new System.Drawing.Size(70, 30);
            this.btn_SelectTaskFileSavingDir.TabIndex = 2;
            this.btn_SelectTaskFileSavingDir.Text = "选择目录";
            this.btn_SelectTaskFileSavingDir.UseVisualStyleBackColor = true;
            this.btn_SelectTaskFileSavingDir.Click += new System.EventHandler(this.btn_SelectTaskFileSavingDir_Click);
            // 
            // Form_FileAndReport
            // 
            this.AcceptButton = this.btn_Save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(814, 544);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_FileAndReport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "文件和报表";
            this.Load += new System.EventHandler(this.Form_FileAndReport_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_SelectImageSavingDir;
        private System.Windows.Forms.TextBox textBox_ImageSavingDir;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox_AutoSaveResultImage;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox_AutoSaveResultExcel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_ExcelSavingDir;
        private System.Windows.Forms.Button btn_SelectExcelSavingDir;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_TaskFileSavingDir;
        private System.Windows.Forms.Button btn_SelectTaskFileSavingDir;
    }
}