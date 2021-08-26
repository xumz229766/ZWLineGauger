namespace ZWLineGauger
{
    partial class Form_RedDot
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_RedDot));
            this.checkBox_ActivateRedDot = new System.Windows.Forms.CheckBox();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_Save = new System.Windows.Forms.Button();
            this.textBox_OffsetX = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_OffsetY = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_Test = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkBox_ActivateRedDot
            // 
            this.checkBox_ActivateRedDot.AutoSize = true;
            this.checkBox_ActivateRedDot.Location = new System.Drawing.Point(87, 27);
            this.checkBox_ActivateRedDot.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_ActivateRedDot.Name = "checkBox_ActivateRedDot";
            this.checkBox_ActivateRedDot.Size = new System.Drawing.Size(96, 16);
            this.checkBox_ActivateRedDot.TabIndex = 4;
            this.checkBox_ActivateRedDot.Text = "启用红点对位";
            this.checkBox_ActivateRedDot.UseVisualStyleBackColor = true;
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Location = new System.Drawing.Point(301, 246);
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
            this.btn_Save.Location = new System.Drawing.Point(179, 246);
            this.btn_Save.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(70, 30);
            this.btn_Save.TabIndex = 5;
            this.btn_Save.Text = "保存";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // textBox_OffsetX
            // 
            this.textBox_OffsetX.Location = new System.Drawing.Point(178, 85);
            this.textBox_OffsetX.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_OffsetX.Name = "textBox_OffsetX";
            this.textBox_OffsetX.Size = new System.Drawing.Size(80, 21);
            this.textBox_OffsetX.TabIndex = 47;
            this.textBox_OffsetX.TabStop = false;
            this.textBox_OffsetX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(84, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 46;
            this.label2.Text = "红点偏移X(mm):";
            // 
            // textBox_OffsetY
            // 
            this.textBox_OffsetY.Location = new System.Drawing.Point(178, 120);
            this.textBox_OffsetY.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_OffsetY.Name = "textBox_OffsetY";
            this.textBox_OffsetY.Size = new System.Drawing.Size(80, 21);
            this.textBox_OffsetY.TabIndex = 49;
            this.textBox_OffsetY.TabStop = false;
            this.textBox_OffsetY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(84, 124);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 48;
            this.label1.Text = "红点偏移Y(mm):";
            // 
            // btn_Test
            // 
            this.btn_Test.Location = new System.Drawing.Point(301, 85);
            this.btn_Test.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Test.Name = "btn_Test";
            this.btn_Test.Size = new System.Drawing.Size(70, 30);
            this.btn_Test.TabIndex = 50;
            this.btn_Test.Text = "测试";
            this.btn_Test.UseVisualStyleBackColor = true;
            this.btn_Test.Click += new System.EventHandler(this.btn_Test_Click);
            // 
            // Form_RedDot
            // 
            this.AcceptButton = this.btn_Save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(549, 324);
            this.Controls.Add(this.btn_Test);
            this.Controls.Add(this.textBox_OffsetY);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_OffsetX);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.checkBox_ActivateRedDot);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_RedDot";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "红点对位";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form_RedDot_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_ActivateRedDot;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.TextBox textBox_OffsetX;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_OffsetY;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_Test;
    }
}