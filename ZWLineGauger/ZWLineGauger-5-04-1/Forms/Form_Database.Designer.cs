namespace ZWLineGauger
{
    partial class Form_Database
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_DataSource = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_DatabaseTask = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_UserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_Pwd = new System.Windows.Forms.TextBox();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_Save = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_DatabaseStdLib = new System.Windows.Forms.TextBox();
            this.checkBox_UseDatabase = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(115, 45);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "数据源:";
            // 
            // textBox_DataSource
            // 
            this.textBox_DataSource.Location = new System.Drawing.Point(208, 41);
            this.textBox_DataSource.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_DataSource.Name = "textBox_DataSource";
            this.textBox_DataSource.Size = new System.Drawing.Size(259, 21);
            this.textBox_DataSource.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(115, 78);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "测量任务数据库:";
            // 
            // textBox_DatabaseTask
            // 
            this.textBox_DatabaseTask.Location = new System.Drawing.Point(208, 74);
            this.textBox_DatabaseTask.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_DatabaseTask.Name = "textBox_DatabaseTask";
            this.textBox_DatabaseTask.Size = new System.Drawing.Size(259, 21);
            this.textBox_DatabaseTask.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(115, 144);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "用户名:";
            // 
            // textBox_UserName
            // 
            this.textBox_UserName.Location = new System.Drawing.Point(208, 140);
            this.textBox_UserName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_UserName.Name = "textBox_UserName";
            this.textBox_UserName.Size = new System.Drawing.Size(259, 21);
            this.textBox_UserName.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(115, 177);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "密码:";
            // 
            // textBox_Pwd
            // 
            this.textBox_Pwd.Location = new System.Drawing.Point(208, 173);
            this.textBox_Pwd.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_Pwd.Name = "textBox_Pwd";
            this.textBox_Pwd.Size = new System.Drawing.Size(259, 21);
            this.textBox_Pwd.TabIndex = 9;
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(336, 322);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(70, 30);
            this.btn_Cancel.TabIndex = 12;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(214, 322);
            this.btn_Save.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(70, 30);
            this.btn_Save.TabIndex = 11;
            this.btn_Save.Text = "保存";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(115, 110);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 12);
            this.label5.TabIndex = 14;
            this.label5.Text = "标准库数据库:";
            // 
            // textBox_DatabaseStdLib
            // 
            this.textBox_DatabaseStdLib.Location = new System.Drawing.Point(208, 106);
            this.textBox_DatabaseStdLib.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_DatabaseStdLib.Name = "textBox_DatabaseStdLib";
            this.textBox_DatabaseStdLib.Size = new System.Drawing.Size(259, 21);
            this.textBox_DatabaseStdLib.TabIndex = 13;
            // 
            // checkBox_UseDatabase
            // 
            this.checkBox_UseDatabase.AutoSize = true;
            this.checkBox_UseDatabase.Location = new System.Drawing.Point(117, 217);
            this.checkBox_UseDatabase.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_UseDatabase.Name = "checkBox_UseDatabase";
            this.checkBox_UseDatabase.Size = new System.Drawing.Size(84, 16);
            this.checkBox_UseDatabase.TabIndex = 91;
            this.checkBox_UseDatabase.TabStop = false;
            this.checkBox_UseDatabase.Text = "启用数据库";
            this.checkBox_UseDatabase.UseVisualStyleBackColor = true;
            // 
            // Form_Database
            // 
            this.AcceptButton = this.btn_Save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(628, 385);
            this.Controls.Add(this.checkBox_UseDatabase);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_DatabaseStdLib);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_Pwd);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_UserName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_DatabaseTask);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_DataSource);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form_Database";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "设置--数据库";
            this.Load += new System.EventHandler(this.Form_Database_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_DataSource;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_DatabaseTask;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_UserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_Pwd;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_DatabaseStdLib;
        private System.Windows.Forms.CheckBox checkBox_UseDatabase;
    }
}