namespace ZWLineGauger.Forms
{
    partial class Form_ModifyTask
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_ModifyTask));
            this.comboBox_AllowanceModifyMode = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.btn_OK = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_LowerDelta = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_UpperDelta = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_StandardValue = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_Name = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_LowerDelta2 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_UpperDelta2 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_StandardValue2 = new System.Windows.Forms.TextBox();
            this.textBox_LowerValue = new System.Windows.Forms.TextBox();
            this.textBox_UpperValue = new System.Windows.Forms.TextBox();
            this.label_UpperPercent = new System.Windows.Forms.Label();
            this.label_LowerPercent = new System.Windows.Forms.Label();
            this.label_LowerPercent2 = new System.Windows.Forms.Label();
            this.label_UpperPercent2 = new System.Windows.Forms.Label();
            this.textBox_LowerValue2 = new System.Windows.Forms.TextBox();
            this.textBox_UpperValue2 = new System.Windows.Forms.TextBox();
            this.label_unit1 = new System.Windows.Forms.Label();
            this.label_unit2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBox_AllowanceModifyMode
            // 
            this.comboBox_AllowanceModifyMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_AllowanceModifyMode.FormattingEnabled = true;
            this.comboBox_AllowanceModifyMode.Location = new System.Drawing.Point(166, 35);
            this.comboBox_AllowanceModifyMode.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_AllowanceModifyMode.Name = "comboBox_AllowanceModifyMode";
            this.comboBox_AllowanceModifyMode.Size = new System.Drawing.Size(83, 20);
            this.comboBox_AllowanceModifyMode.TabIndex = 44;
            this.comboBox_AllowanceModifyMode.SelectedIndexChanged += new System.EventHandler(this.comboBox_AllowanceModifyMode_SelectedIndexChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(68, 38);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(95, 12);
            this.label16.TabIndex = 43;
            this.label16.Text = "上下限修改方式:";
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Font = new System.Drawing.Font("宋体", 10F);
            this.btn_Cancel.Location = new System.Drawing.Point(431, 267);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(76, 33);
            this.btn_Cancel.TabIndex = 42;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // btn_OK
            // 
            this.btn_OK.Font = new System.Drawing.Font("宋体", 10F);
            this.btn_OK.Location = new System.Drawing.Point(286, 267);
            this.btn_OK.Margin = new System.Windows.Forms.Padding(2);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(76, 33);
            this.btn_OK.TabIndex = 41;
            this.btn_OK.Text = "确定";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(68, 198);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 40;
            this.label3.Text = "下限(-):";
            // 
            // textBox_LowerDelta
            // 
            this.textBox_LowerDelta.Location = new System.Drawing.Point(137, 195);
            this.textBox_LowerDelta.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_LowerDelta.Name = "textBox_LowerDelta";
            this.textBox_LowerDelta.Size = new System.Drawing.Size(99, 21);
            this.textBox_LowerDelta.TabIndex = 39;
            this.textBox_LowerDelta.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_LowerDelta.TextChanged += new System.EventHandler(this.textBox_LowerDelta_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(68, 163);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 38;
            this.label2.Text = "上限(+):";
            // 
            // textBox_UpperDelta
            // 
            this.textBox_UpperDelta.Location = new System.Drawing.Point(137, 161);
            this.textBox_UpperDelta.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_UpperDelta.Name = "textBox_UpperDelta";
            this.textBox_UpperDelta.Size = new System.Drawing.Size(99, 21);
            this.textBox_UpperDelta.TabIndex = 37;
            this.textBox_UpperDelta.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_UpperDelta.TextChanged += new System.EventHandler(this.textBox_UpperDelta_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(68, 130);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 36;
            this.label1.Text = "标准值1:";
            // 
            // textBox_StandardValue
            // 
            this.textBox_StandardValue.Location = new System.Drawing.Point(137, 127);
            this.textBox_StandardValue.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_StandardValue.Name = "textBox_StandardValue";
            this.textBox_StandardValue.Size = new System.Drawing.Size(99, 21);
            this.textBox_StandardValue.TabIndex = 32;
            this.textBox_StandardValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_StandardValue.TextChanged += new System.EventHandler(this.textBox_StandardValue_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(68, 85);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 12);
            this.label4.TabIndex = 46;
            this.label4.Text = "名称:";
            // 
            // textBox_Name
            // 
            this.textBox_Name.Location = new System.Drawing.Point(137, 82);
            this.textBox_Name.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_Name.Name = "textBox_Name";
            this.textBox_Name.Size = new System.Drawing.Size(99, 21);
            this.textBox_Name.TabIndex = 31;
            this.textBox_Name.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(440, 198);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 52;
            this.label5.Text = "下限(-):";
            // 
            // textBox_LowerDelta2
            // 
            this.textBox_LowerDelta2.Location = new System.Drawing.Point(509, 195);
            this.textBox_LowerDelta2.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_LowerDelta2.Name = "textBox_LowerDelta2";
            this.textBox_LowerDelta2.Size = new System.Drawing.Size(99, 21);
            this.textBox_LowerDelta2.TabIndex = 51;
            this.textBox_LowerDelta2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_LowerDelta2.TextChanged += new System.EventHandler(this.textBox_LowerDelta2_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(440, 163);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 50;
            this.label6.Text = "上限(+):";
            // 
            // textBox_UpperDelta2
            // 
            this.textBox_UpperDelta2.Location = new System.Drawing.Point(509, 161);
            this.textBox_UpperDelta2.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_UpperDelta2.Name = "textBox_UpperDelta2";
            this.textBox_UpperDelta2.Size = new System.Drawing.Size(99, 21);
            this.textBox_UpperDelta2.TabIndex = 49;
            this.textBox_UpperDelta2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_UpperDelta2.TextChanged += new System.EventHandler(this.textBox_UpperDelta2_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(440, 130);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 48;
            this.label7.Text = "标准值2:";
            // 
            // textBox_StandardValue2
            // 
            this.textBox_StandardValue2.Location = new System.Drawing.Point(509, 127);
            this.textBox_StandardValue2.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_StandardValue2.Name = "textBox_StandardValue2";
            this.textBox_StandardValue2.Size = new System.Drawing.Size(99, 21);
            this.textBox_StandardValue2.TabIndex = 47;
            this.textBox_StandardValue2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBox_StandardValue2.TextChanged += new System.EventHandler(this.textBox_StandardValue2_TextChanged);
            // 
            // textBox_LowerValue
            // 
            this.textBox_LowerValue.Location = new System.Drawing.Point(281, 195);
            this.textBox_LowerValue.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_LowerValue.Name = "textBox_LowerValue";
            this.textBox_LowerValue.Size = new System.Drawing.Size(99, 21);
            this.textBox_LowerValue.TabIndex = 54;
            this.textBox_LowerValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBox_UpperValue
            // 
            this.textBox_UpperValue.Location = new System.Drawing.Point(281, 161);
            this.textBox_UpperValue.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_UpperValue.Name = "textBox_UpperValue";
            this.textBox_UpperValue.Size = new System.Drawing.Size(99, 21);
            this.textBox_UpperValue.TabIndex = 53;
            this.textBox_UpperValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label_UpperPercent
            // 
            this.label_UpperPercent.AutoSize = true;
            this.label_UpperPercent.Location = new System.Drawing.Point(240, 165);
            this.label_UpperPercent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_UpperPercent.Name = "label_UpperPercent";
            this.label_UpperPercent.Size = new System.Drawing.Size(11, 12);
            this.label_UpperPercent.TabIndex = 55;
            this.label_UpperPercent.Text = "%";
            // 
            // label_LowerPercent
            // 
            this.label_LowerPercent.AutoSize = true;
            this.label_LowerPercent.Location = new System.Drawing.Point(240, 199);
            this.label_LowerPercent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_LowerPercent.Name = "label_LowerPercent";
            this.label_LowerPercent.Size = new System.Drawing.Size(11, 12);
            this.label_LowerPercent.TabIndex = 56;
            this.label_LowerPercent.Text = "%";
            // 
            // label_LowerPercent2
            // 
            this.label_LowerPercent2.AutoSize = true;
            this.label_LowerPercent2.Location = new System.Drawing.Point(612, 199);
            this.label_LowerPercent2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_LowerPercent2.Name = "label_LowerPercent2";
            this.label_LowerPercent2.Size = new System.Drawing.Size(11, 12);
            this.label_LowerPercent2.TabIndex = 58;
            this.label_LowerPercent2.Text = "%";
            // 
            // label_UpperPercent2
            // 
            this.label_UpperPercent2.AutoSize = true;
            this.label_UpperPercent2.Location = new System.Drawing.Point(612, 165);
            this.label_UpperPercent2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_UpperPercent2.Name = "label_UpperPercent2";
            this.label_UpperPercent2.Size = new System.Drawing.Size(11, 12);
            this.label_UpperPercent2.TabIndex = 57;
            this.label_UpperPercent2.Text = "%";
            // 
            // textBox_LowerValue2
            // 
            this.textBox_LowerValue2.Location = new System.Drawing.Point(653, 195);
            this.textBox_LowerValue2.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_LowerValue2.Name = "textBox_LowerValue2";
            this.textBox_LowerValue2.Size = new System.Drawing.Size(99, 21);
            this.textBox_LowerValue2.TabIndex = 60;
            this.textBox_LowerValue2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBox_UpperValue2
            // 
            this.textBox_UpperValue2.Location = new System.Drawing.Point(653, 161);
            this.textBox_UpperValue2.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_UpperValue2.Name = "textBox_UpperValue2";
            this.textBox_UpperValue2.Size = new System.Drawing.Size(99, 21);
            this.textBox_UpperValue2.TabIndex = 59;
            this.textBox_UpperValue2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label_unit1
            // 
            this.label_unit1.AutoSize = true;
            this.label_unit1.Location = new System.Drawing.Point(240, 131);
            this.label_unit1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_unit1.Name = "label_unit1";
            this.label_unit1.Size = new System.Drawing.Size(17, 12);
            this.label_unit1.TabIndex = 61;
            this.label_unit1.Text = "mm";
            // 
            // label_unit2
            // 
            this.label_unit2.AutoSize = true;
            this.label_unit2.Location = new System.Drawing.Point(612, 130);
            this.label_unit2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_unit2.Name = "label_unit2";
            this.label_unit2.Size = new System.Drawing.Size(17, 12);
            this.label_unit2.TabIndex = 62;
            this.label_unit2.Text = "mm";
            // 
            // Form_ModifyTask
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(807, 341);
            this.Controls.Add(this.label_unit2);
            this.Controls.Add(this.label_unit1);
            this.Controls.Add(this.textBox_LowerValue2);
            this.Controls.Add(this.textBox_UpperValue2);
            this.Controls.Add(this.label_LowerPercent2);
            this.Controls.Add(this.label_UpperPercent2);
            this.Controls.Add(this.label_LowerPercent);
            this.Controls.Add(this.label_UpperPercent);
            this.Controls.Add(this.textBox_LowerValue);
            this.Controls.Add(this.textBox_UpperValue);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_LowerDelta2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox_UpperDelta2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox_StandardValue2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_Name);
            this.Controls.Add(this.comboBox_AllowanceModifyMode);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_LowerDelta);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_UpperDelta);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_StandardValue);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_ModifyTask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "修改测量任务数据";
            this.Load += new System.EventHandler(this.Form_ModifyTask_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_AllowanceModifyMode;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_LowerDelta;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_UpperDelta;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_StandardValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_Name;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_LowerDelta2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_UpperDelta2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_StandardValue2;
        private System.Windows.Forms.TextBox textBox_LowerValue;
        private System.Windows.Forms.TextBox textBox_UpperValue;
        private System.Windows.Forms.Label label_UpperPercent;
        private System.Windows.Forms.Label label_LowerPercent;
        private System.Windows.Forms.Label label_LowerPercent2;
        private System.Windows.Forms.Label label_UpperPercent2;
        private System.Windows.Forms.TextBox textBox_LowerValue2;
        private System.Windows.Forms.TextBox textBox_UpperValue2;
        private System.Windows.Forms.Label label_unit1;
        private System.Windows.Forms.Label label_unit2;
    }
}