namespace ZWLineGauger
{
    partial class Form_CreateTask
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_CreateTask));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_ProductModel = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_Layer = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_ProductNumber = new System.Windows.Forms.TextBox();
            this.btn_OK = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.comboBox_CreateMode = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(130, 110);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "料号:";
            // 
            // textBox_ProductModel
            // 
            this.textBox_ProductModel.Location = new System.Drawing.Point(177, 106);
            this.textBox_ProductModel.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_ProductModel.Name = "textBox_ProductModel";
            this.textBox_ProductModel.Size = new System.Drawing.Size(162, 21);
            this.textBox_ProductModel.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(374, 110);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "层别:";
            // 
            // textBox_Layer
            // 
            this.textBox_Layer.Location = new System.Drawing.Point(414, 106);
            this.textBox_Layer.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_Layer.Name = "textBox_Layer";
            this.textBox_Layer.Size = new System.Drawing.Size(62, 21);
            this.textBox_Layer.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(130, 147);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "工单号:";
            // 
            // textBox_ProductNumber
            // 
            this.textBox_ProductNumber.Location = new System.Drawing.Point(177, 143);
            this.textBox_ProductNumber.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_ProductNumber.Name = "textBox_ProductNumber";
            this.textBox_ProductNumber.Size = new System.Drawing.Size(162, 21);
            this.textBox_ProductNumber.TabIndex = 7;
            // 
            // btn_OK
            // 
            this.btn_OK.Font = new System.Drawing.Font("宋体", 10F);
            this.btn_OK.Location = new System.Drawing.Point(213, 277);
            this.btn_OK.Margin = new System.Windows.Forms.Padding(2);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(76, 33);
            this.btn_OK.TabIndex = 10;
            this.btn_OK.Text = "确定";
            this.btn_OK.UseVisualStyleBackColor = true;
            this.btn_OK.Click += new System.EventHandler(this.btn_OK_Click);
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Cancel.Font = new System.Drawing.Font("宋体", 10F);
            this.btn_Cancel.Location = new System.Drawing.Point(358, 277);
            this.btn_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(76, 33);
            this.btn_Cancel.TabIndex = 11;
            this.btn_Cancel.Text = "取消";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            this.btn_Cancel.Click += new System.EventHandler(this.btn_Cancel_Click);
            // 
            // comboBox_CreateMode
            // 
            this.comboBox_CreateMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_CreateMode.FormattingEnabled = true;
            this.comboBox_CreateMode.Location = new System.Drawing.Point(177, 48);
            this.comboBox_CreateMode.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_CreateMode.Name = "comboBox_CreateMode";
            this.comboBox_CreateMode.Size = new System.Drawing.Size(98, 20);
            this.comboBox_CreateMode.TabIndex = 34;
            this.comboBox_CreateMode.Visible = false;
            this.comboBox_CreateMode.SelectedIndexChanged += new System.EventHandler(this.comboBox_CreateMode_SelectedIndexChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(130, 51);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(35, 12);
            this.label16.TabIndex = 33;
            this.label16.Text = "模式:";
            this.label16.Visible = false;
            // 
            // Form_CreateTask
            // 
            this.AcceptButton = this.btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_Cancel;
            this.ClientSize = new System.Drawing.Size(670, 356);
            this.Controls.Add(this.comboBox_CreateMode);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_ProductNumber);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_Layer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_ProductModel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_CreateTask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "创建任务";
            this.Load += new System.EventHandler(this.Form_CreateTask_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_ProductModel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_Layer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_ProductNumber;
        private System.Windows.Forms.Button btn_OK;
        private System.Windows.Forms.Button btn_Cancel;
        private System.Windows.Forms.ComboBox comboBox_CreateMode;
        private System.Windows.Forms.Label label16;
    }
}