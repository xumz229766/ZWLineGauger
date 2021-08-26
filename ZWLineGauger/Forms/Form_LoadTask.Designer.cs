namespace ZWLineGauger.Forms
{
    partial class Form_LoadTask
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_LoadTask));
            this.label1 = new System.Windows.Forms.Label();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_OK = new System.Windows.Forms.Button();
            this.comboBox_ShowTasks = new System.Windows.Forms.ComboBox();
            this.textBox_Input = new System.Windows.Forms.TextBox();
            this.comboBox_TaskInfoSource = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label_SourceDir = new System.Windows.Forms.Label();
            this.btn_Browse = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(147, 67);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 18;
            this.label1.Text = "任务名称:";
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Font = new System.Drawing.Font("宋体", 10F);
            this.btn_Cancel.Location = new System.Drawing.Point(384, 227);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(76, 33);
            this.btn_Cancel.TabIndex = 17;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_OK
            // 
            this.btn_OK.Font = new System.Drawing.Font("宋体", 10F);
            this.btn_OK.Location = new System.Drawing.Point(252, 227);
            this.btn_OK.Margin = new System.Windows.Forms.Padding(2);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(76, 33);
            this.btn_OK.TabIndex = 16;
            this.btn_OK.Text = "确定";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // comboBox_ShowTasks
            // 
            this.comboBox_ShowTasks.FormattingEnabled = true;
            this.comboBox_ShowTasks.Location = new System.Drawing.Point(211, 64);
            this.comboBox_ShowTasks.Name = "comboBox_ShowTasks";
            this.comboBox_ShowTasks.Size = new System.Drawing.Size(219, 20);
            this.comboBox_ShowTasks.TabIndex = 10;
            this.comboBox_ShowTasks.SelectedIndexChanged += new System.EventHandler(this.comboBox_ShowTasks_SelectedIndexChanged);
            // 
            // textBox_Input
            // 
            this.textBox_Input.Location = new System.Drawing.Point(211, 64);
            this.textBox_Input.Name = "textBox_Input";
            this.textBox_Input.Size = new System.Drawing.Size(219, 21);
            this.textBox_Input.TabIndex = 1;
            this.textBox_Input.TextChanged += new System.EventHandler(this.textBox_Input_TextChanged);
            // 
            // comboBox_TaskInfoSource
            // 
            this.comboBox_TaskInfoSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_TaskInfoSource.FormattingEnabled = true;
            this.comboBox_TaskInfoSource.Location = new System.Drawing.Point(211, 104);
            this.comboBox_TaskInfoSource.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_TaskInfoSource.Name = "comboBox_TaskInfoSource";
            this.comboBox_TaskInfoSource.Size = new System.Drawing.Size(83, 20);
            this.comboBox_TaskInfoSource.TabIndex = 45;
            this.comboBox_TaskInfoSource.SelectedIndexChanged += new System.EventHandler(this.comboBox_TaskInfoSource_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(147, 108);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 46;
            this.label2.Text = "来源:";
            // 
            // label_SourceDir
            // 
            this.label_SourceDir.AutoSize = true;
            this.label_SourceDir.Location = new System.Drawing.Point(209, 141);
            this.label_SourceDir.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_SourceDir.Name = "label_SourceDir";
            this.label_SourceDir.Size = new System.Drawing.Size(29, 12);
            this.label_SourceDir.TabIndex = 47;
            this.label_SourceDir.Text = "来源";
            // 
            // btn_Browse
            // 
            this.btn_Browse.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Browse.Location = new System.Drawing.Point(459, 60);
            this.btn_Browse.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Browse.Name = "btn_Browse";
            this.btn_Browse.Size = new System.Drawing.Size(72, 30);
            this.btn_Browse.TabIndex = 48;
            this.btn_Browse.Text = "浏览......";
            this.btn_Browse.UseVisualStyleBackColor = true;
            this.btn_Browse.Click += new System.EventHandler(this.btn_Browse_Click);
            // 
            // Form_LoadTask
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(709, 331);
            this.Controls.Add(this.btn_Browse);
            this.Controls.Add(this.label_SourceDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBox_TaskInfoSource);
            this.Controls.Add(this.textBox_Input);
            this.Controls.Add(this.comboBox_ShowTasks);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_LoadTask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_LoadTask";
            this.Load += new System.EventHandler(this.Form_LoadTask_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.ComboBox comboBox_ShowTasks;
        private System.Windows.Forms.TextBox textBox_Input;
        private System.Windows.Forms.ComboBox comboBox_TaskInfoSource;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_SourceDir;
        private System.Windows.Forms.Button btn_Browse;
    }
}