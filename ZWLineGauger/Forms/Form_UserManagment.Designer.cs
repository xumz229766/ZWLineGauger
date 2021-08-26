namespace ZWLineGauger.Forms
{
    partial class Form_UserManagment
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
            this.gridview_user_message = new System.Windows.Forms.DataGridView();
            this.add_user_btn = new System.Windows.Forms.Button();
            this.delete_user_btn = new System.Windows.Forms.Button();
            this.user_message_save = new System.Windows.Forms.Button();
            this.user_message_cancel = new System.Windows.Forms.Button();
            this.comboBox_User = new System.Windows.Forms.ComboBox();
            this.label_status = new System.Windows.Forms.Label();
            this.label_pass = new System.Windows.Forms.Label();
            this.text_pass = new System.Windows.Forms.TextBox();
            this.label_name = new System.Windows.Forms.Label();
            this.text_name = new System.Windows.Forms.TextBox();
            this.text_pass_sure = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.gridview_user_message)).BeginInit();
            this.SuspendLayout();
            // 
            // gridview_user_message
            // 
            this.gridview_user_message.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.gridview_user_message.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridview_user_message.Location = new System.Drawing.Point(42, 216);
            this.gridview_user_message.Margin = new System.Windows.Forms.Padding(2);
            this.gridview_user_message.Name = "gridview_user_message";
            this.gridview_user_message.RowTemplate.Height = 27;
            this.gridview_user_message.Size = new System.Drawing.Size(298, 230);
            this.gridview_user_message.TabIndex = 42;
            this.gridview_user_message.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.gridview_GraphMeasureItems_CellMouseClick);
            // 
            // add_user_btn
            // 
            this.add_user_btn.Location = new System.Drawing.Point(259, 146);
            this.add_user_btn.Name = "add_user_btn";
            this.add_user_btn.Size = new System.Drawing.Size(75, 23);
            this.add_user_btn.TabIndex = 43;
            this.add_user_btn.Text = "添加用户";
            this.add_user_btn.UseVisualStyleBackColor = true;
            this.add_user_btn.Click += new System.EventHandler(this.add_user_btn_Click);
            // 
            // delete_user_btn
            // 
            this.delete_user_btn.Location = new System.Drawing.Point(42, 467);
            this.delete_user_btn.Name = "delete_user_btn";
            this.delete_user_btn.Size = new System.Drawing.Size(75, 23);
            this.delete_user_btn.TabIndex = 44;
            this.delete_user_btn.Text = "删除用户";
            this.delete_user_btn.UseVisualStyleBackColor = true;
            this.delete_user_btn.Click += new System.EventHandler(this.delete_user_btn_Click);
            // 
            // user_message_save
            // 
            this.user_message_save.Location = new System.Drawing.Point(375, 487);
            this.user_message_save.Name = "user_message_save";
            this.user_message_save.Size = new System.Drawing.Size(75, 23);
            this.user_message_save.TabIndex = 45;
            this.user_message_save.Text = "保存";
            this.user_message_save.UseVisualStyleBackColor = true;
            this.user_message_save.Click += new System.EventHandler(this.user_message_save_Click);
            // 
            // user_message_cancel
            // 
            this.user_message_cancel.Location = new System.Drawing.Point(485, 487);
            this.user_message_cancel.Name = "user_message_cancel";
            this.user_message_cancel.Size = new System.Drawing.Size(75, 23);
            this.user_message_cancel.TabIndex = 46;
            this.user_message_cancel.Text = "取消";
            this.user_message_cancel.UseVisualStyleBackColor = true;
            this.user_message_cancel.Click += new System.EventHandler(this.user_message_cancel_Click);
            // 
            // comboBox_User
            // 
            this.comboBox_User.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_User.FormattingEnabled = true;
            this.comboBox_User.Location = new System.Drawing.Point(92, 149);
            this.comboBox_User.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_User.Name = "comboBox_User";
            this.comboBox_User.Size = new System.Drawing.Size(100, 20);
            this.comboBox_User.TabIndex = 58;
            // 
            // label_status
            // 
            this.label_status.AutoSize = true;
            this.label_status.Location = new System.Drawing.Point(45, 157);
            this.label_status.Name = "label_status";
            this.label_status.Size = new System.Drawing.Size(41, 12);
            this.label_status.TabIndex = 54;
            this.label_status.Text = "身份：";
            // 
            // label_pass
            // 
            this.label_pass.AutoSize = true;
            this.label_pass.Location = new System.Drawing.Point(45, 74);
            this.label_pass.Name = "label_pass";
            this.label_pass.Size = new System.Drawing.Size(41, 12);
            this.label_pass.TabIndex = 55;
            this.label_pass.Text = "密码：";
            // 
            // text_pass
            // 
            this.text_pass.Location = new System.Drawing.Point(92, 71);
            this.text_pass.Name = "text_pass";
            this.text_pass.Size = new System.Drawing.Size(100, 21);
            this.text_pass.TabIndex = 56;
            // 
            // label_name
            // 
            this.label_name.AutoSize = true;
            this.label_name.Location = new System.Drawing.Point(45, 33);
            this.label_name.Name = "label_name";
            this.label_name.Size = new System.Drawing.Size(41, 12);
            this.label_name.TabIndex = 57;
            this.label_name.Text = "账户：";
            // 
            // text_name
            // 
            this.text_name.Location = new System.Drawing.Point(92, 30);
            this.text_name.Name = "text_name";
            this.text_name.Size = new System.Drawing.Size(100, 21);
            this.text_name.TabIndex = 53;
            // 
            // text_pass_sure
            // 
            this.text_pass_sure.Location = new System.Drawing.Point(92, 111);
            this.text_pass_sure.Name = "text_pass_sure";
            this.text_pass_sure.Size = new System.Drawing.Size(100, 21);
            this.text_pass_sure.TabIndex = 56;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 55;
            this.label1.Text = "确认密码：";
            // 
            // Form_UserManagment
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 522);
            this.Controls.Add(this.comboBox_User);
            this.Controls.Add(this.label_status);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label_pass);
            this.Controls.Add(this.text_pass_sure);
            this.Controls.Add(this.text_pass);
            this.Controls.Add(this.label_name);
            this.Controls.Add(this.text_name);
            this.Controls.Add(this.user_message_cancel);
            this.Controls.Add(this.user_message_save);
            this.Controls.Add(this.delete_user_btn);
            this.Controls.Add(this.add_user_btn);
            this.Controls.Add(this.gridview_user_message);
            this.Name = "Form_UserManagment";
            this.Text = "Form_UserManagment";
            ((System.ComponentModel.ISupportInitialize)(this.gridview_user_message)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView gridview_user_message;
        private System.Windows.Forms.Button add_user_btn;
        private System.Windows.Forms.Button delete_user_btn;
        private System.Windows.Forms.Button user_message_save;
        private System.Windows.Forms.Button user_message_cancel;
        private System.Windows.Forms.ComboBox comboBox_User;
        private System.Windows.Forms.Label label_status;
        private System.Windows.Forms.Label label_pass;
        private System.Windows.Forms.TextBox text_pass;
        private System.Windows.Forms.Label label_name;
        private System.Windows.Forms.TextBox text_name;
        private System.Windows.Forms.TextBox text_pass_sure;
        private System.Windows.Forms.Label label1;
    }
}