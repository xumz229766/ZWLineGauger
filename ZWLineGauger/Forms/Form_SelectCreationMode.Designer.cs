namespace ZWLineGauger.Forms
{
    partial class Form_SelectCreationMode
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_SelectCreationMode));
            this.btn_GraphMode = new System.Windows.Forms.Button();
            this.btn_ManualMode = new System.Windows.Forms.Button();
            this.checkBox_UseCurrentGraph = new System.Windows.Forms.CheckBox();
            this.btn_TxtMode = new System.Windows.Forms.Button();
            this.comboBox_ProductType = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_GraphMode
            // 
            this.btn_GraphMode.Font = new System.Drawing.Font("宋体", 11F);
            this.btn_GraphMode.Location = new System.Drawing.Point(44, 71);
            this.btn_GraphMode.Margin = new System.Windows.Forms.Padding(2);
            this.btn_GraphMode.Name = "btn_GraphMode";
            this.btn_GraphMode.Size = new System.Drawing.Size(107, 60);
            this.btn_GraphMode.TabIndex = 66;
            this.btn_GraphMode.Text = "Cam图纸模式";
            this.btn_GraphMode.UseVisualStyleBackColor = true;
            this.btn_GraphMode.Click += new System.EventHandler(this.btn_GraphMode_Click);
            // 
            // btn_ManualMode
            // 
            this.btn_ManualMode.Font = new System.Drawing.Font("宋体", 11F);
            this.btn_ManualMode.Location = new System.Drawing.Point(215, 71);
            this.btn_ManualMode.Margin = new System.Windows.Forms.Padding(2);
            this.btn_ManualMode.Name = "btn_ManualMode";
            this.btn_ManualMode.Size = new System.Drawing.Size(106, 60);
            this.btn_ManualMode.TabIndex = 7;
            this.btn_ManualMode.Text = "手动学习模式";
            this.btn_ManualMode.UseVisualStyleBackColor = true;
            this.btn_ManualMode.Click += new System.EventHandler(this.btn_ManualMode_Click);
            // 
            // checkBox_UseCurrentGraph
            // 
            this.checkBox_UseCurrentGraph.AutoSize = true;
            this.checkBox_UseCurrentGraph.Location = new System.Drawing.Point(44, 149);
            this.checkBox_UseCurrentGraph.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_UseCurrentGraph.Name = "checkBox_UseCurrentGraph";
            this.checkBox_UseCurrentGraph.Size = new System.Drawing.Size(96, 16);
            this.checkBox_UseCurrentGraph.TabIndex = 91;
            this.checkBox_UseCurrentGraph.TabStop = false;
            this.checkBox_UseCurrentGraph.Text = "使用当前图纸";
            this.checkBox_UseCurrentGraph.UseVisualStyleBackColor = true;
            // 
            // btn_TxtMode
            // 
            this.btn_TxtMode.Font = new System.Drawing.Font("宋体", 11F);
            this.btn_TxtMode.Location = new System.Drawing.Point(392, 71);
            this.btn_TxtMode.Margin = new System.Windows.Forms.Padding(2);
            this.btn_TxtMode.Name = "btn_TxtMode";
            this.btn_TxtMode.Size = new System.Drawing.Size(106, 60);
            this.btn_TxtMode.TabIndex = 92;
            this.btn_TxtMode.Text = "TXT模式";
            this.btn_TxtMode.UseVisualStyleBackColor = true;
            this.btn_TxtMode.Click += new System.EventHandler(this.btn_TxtMode_Click);
            // 
            // comboBox_ProductType
            // 
            this.comboBox_ProductType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_ProductType.FormattingEnabled = true;
            this.comboBox_ProductType.Location = new System.Drawing.Point(245, 169);
            this.comboBox_ProductType.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_ProductType.Name = "comboBox_ProductType";
            this.comboBox_ProductType.Size = new System.Drawing.Size(98, 20);
            this.comboBox_ProductType.TabIndex = 94;
            this.comboBox_ProductType.SelectedIndexChanged += new System.EventHandler(this.comboBox_ProductType_SelectedIndexChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(181, 172);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(59, 12);
            this.label16.TabIndex = 93;
            this.label16.Text = "产品类别:";
            // 
            // Form_SelectCreationMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(589, 265);
            this.Controls.Add(this.comboBox_ProductType);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.btn_TxtMode);
            this.Controls.Add(this.checkBox_UseCurrentGraph);
            this.Controls.Add(this.btn_ManualMode);
            this.Controls.Add(this.btn_GraphMode);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_SelectCreationMode";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "选择创建模式";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_SelectCreationMode_FormClosing);
            this.Load += new System.EventHandler(this.Form_SelectCreationMode_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_GraphMode;
        private System.Windows.Forms.Button btn_ManualMode;
        private System.Windows.Forms.CheckBox checkBox_UseCurrentGraph;
        private System.Windows.Forms.Button btn_TxtMode;
        private System.Windows.Forms.ComboBox comboBox_ProductType;
        private System.Windows.Forms.Label label16;
    }
}