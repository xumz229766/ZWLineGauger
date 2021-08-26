namespace ZWLineGauger
{
    partial class Form_CreateTaskFinished
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
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_OK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_TaskName = new System.Windows.Forms.TextBox();
            this.btn_TerminateCreation = new System.Windows.Forms.Button();
            this.btn_Browse = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Font = new System.Drawing.Font("宋体", 10F);
            this.btn_Cancel.Location = new System.Drawing.Point(231, 159);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(76, 33);
            this.btn_Cancel.TabIndex = 13;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_OK
            // 
            this.btn_OK.Font = new System.Drawing.Font("宋体", 10F);
            this.btn_OK.Location = new System.Drawing.Point(99, 159);
            this.btn_OK.Margin = new System.Windows.Forms.Padding(2);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(76, 33);
            this.btn_OK.TabIndex = 12;
            this.btn_OK.Text = "确定";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(126, 70);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 15;
            this.label1.Text = "任务名称:";
            // 
            // textBox_TaskName
            // 
            this.textBox_TaskName.Location = new System.Drawing.Point(187, 66);
            this.textBox_TaskName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_TaskName.Name = "textBox_TaskName";
            this.textBox_TaskName.Size = new System.Drawing.Size(162, 21);
            this.textBox_TaskName.TabIndex = 11;
            // 
            // btn_TerminateCreation
            // 
            this.btn_TerminateCreation.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_TerminateCreation.Font = new System.Drawing.Font("宋体", 10F);
            this.btn_TerminateCreation.Location = new System.Drawing.Point(369, 159);
            this.btn_TerminateCreation.Margin = new System.Windows.Forms.Padding(2);
            this.btn_TerminateCreation.Name = "btn_TerminateCreation";
            this.btn_TerminateCreation.Size = new System.Drawing.Size(94, 33);
            this.btn_TerminateCreation.TabIndex = 16;
            this.btn_TerminateCreation.Text = "中止创建";
            this.btn_TerminateCreation.UseVisualStyleBackColor = true;
            this.btn_TerminateCreation.Click += new System.EventHandler(this.btn_TerminateCreation_Click);
            // 
            // btn_Browse
            // 
            this.btn_Browse.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Browse.Location = new System.Drawing.Point(378, 62);
            this.btn_Browse.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Browse.Name = "btn_Browse";
            this.btn_Browse.Size = new System.Drawing.Size(72, 30);
            this.btn_Browse.TabIndex = 49;
            this.btn_Browse.Text = "浏览......";
            this.btn_Browse.UseVisualStyleBackColor = true;
            this.btn_Browse.Click += new System.EventHandler(this.btn_Browse_Click);
            // 
            // Form_CreateTaskFinished
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(564, 244);
            this.Controls.Add(this.btn_Browse);
            this.Controls.Add(this.btn_TerminateCreation);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_TaskName);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form_CreateTaskFinished";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "创建任务完成确认";
            this.Load += new System.EventHandler(this.Form_CreateTaskFinished_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_TaskName;
        private System.Windows.Forms.Button btn_TerminateCreation;
        private System.Windows.Forms.Button btn_Browse;
    }
}