namespace ZWLineGauger.Forms
{
    partial class Form_AddNewUser
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
            this.text_name = new System.Windows.Forms.TextBox();
            this.label_name = new System.Windows.Forms.Label();
            this.text_pass = new System.Windows.Forms.TextBox();
            this.label_pass = new System.Windows.Forms.Label();
            this.label_status = new System.Windows.Forms.Label();
            this.add_user_btn = new System.Windows.Forms.Button();
            this.cancel_btn = new System.Windows.Forms.Button();
            this.comboBox_User = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // text_name
            // 
            this.text_name.Location = new System.Drawing.Point(192, 47);
            this.text_name.Name = "text_name";
            this.text_name.Size = new System.Drawing.Size(100, 21);
            this.text_name.TabIndex = 0;
            // 
            // label_name
            // 
            this.label_name.AutoSize = true;
            this.label_name.Location = new System.Drawing.Point(145, 50);
            this.label_name.Name = "label_name";
            this.label_name.Size = new System.Drawing.Size(41, 12);
            this.label_name.TabIndex = 1;
            this.label_name.Text = "账户：";
            // 
            // text_pass
            // 
            this.text_pass.Location = new System.Drawing.Point(192, 88);
            this.text_pass.Name = "text_pass";
            this.text_pass.Size = new System.Drawing.Size(100, 21);
            this.text_pass.TabIndex = 1;
            // 
            // label_pass
            // 
            this.label_pass.AutoSize = true;
            this.label_pass.Location = new System.Drawing.Point(145, 91);
            this.label_pass.Name = "label_pass";
            this.label_pass.Size = new System.Drawing.Size(41, 12);
            this.label_pass.TabIndex = 1;
            this.label_pass.Text = "密码：";
            // 
            // label_status
            // 
            this.label_status.AutoSize = true;
            this.label_status.Location = new System.Drawing.Point(145, 137);
            this.label_status.Name = "label_status";
            this.label_status.Size = new System.Drawing.Size(41, 12);
            this.label_status.TabIndex = 1;
            this.label_status.Text = "身份：";
            // 
            // add_user_btn
            // 
            this.add_user_btn.Location = new System.Drawing.Point(127, 208);
            this.add_user_btn.Name = "add_user_btn";
            this.add_user_btn.Size = new System.Drawing.Size(75, 23);
            this.add_user_btn.TabIndex = 3;
            this.add_user_btn.Text = "确定";
            this.add_user_btn.UseVisualStyleBackColor = true;
            this.add_user_btn.Click += new System.EventHandler(this.add_user_btn_Click);
            // 
            // cancel_btn
            // 
            this.cancel_btn.Location = new System.Drawing.Point(248, 208);
            this.cancel_btn.Name = "cancel_btn";
            this.cancel_btn.Size = new System.Drawing.Size(75, 23);
            this.cancel_btn.TabIndex = 4;
            this.cancel_btn.Text = "取消";
            this.cancel_btn.UseVisualStyleBackColor = true;
            this.cancel_btn.Click += new System.EventHandler(this.cancel_btn_Click);
            // 
            // comboBox_User
            // 
            this.comboBox_User.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_User.FormattingEnabled = true;
            this.comboBox_User.Location = new System.Drawing.Point(192, 129);
            this.comboBox_User.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_User.Name = "comboBox_User";
            this.comboBox_User.Size = new System.Drawing.Size(100, 20);
            this.comboBox_User.TabIndex = 52;
            // 
            // Form_AddNewUser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 276);
            this.Controls.Add(this.comboBox_User);
            this.Controls.Add(this.cancel_btn);
            this.Controls.Add(this.add_user_btn);
            this.Controls.Add(this.label_status);
            this.Controls.Add(this.label_pass);
            this.Controls.Add(this.text_pass);
            this.Controls.Add(this.label_name);
            this.Controls.Add(this.text_name);
            this.Name = "Form_AddNewUser";
            this.Text = "Form_AddNewUser";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox text_name;
        private System.Windows.Forms.Label label_name;
        private System.Windows.Forms.TextBox text_pass;
        private System.Windows.Forms.Label label_pass;
        private System.Windows.Forms.Label label_status;
        private System.Windows.Forms.Button add_user_btn;
        private System.Windows.Forms.Button cancel_btn;
        private System.Windows.Forms.ComboBox comboBox_User;
    }
}