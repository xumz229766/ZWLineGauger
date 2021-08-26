namespace ZWLineGauger
{
    partial class Form_ProgressInfo
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_ProgressInfo));
            this.label_ProgressInfo = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label_ElapsedTime = new System.Windows.Forms.Label();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.label_Time = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label_ProgressInfo
            // 
            this.label_ProgressInfo.AutoSize = true;
            this.label_ProgressInfo.Font = new System.Drawing.Font("宋体", 10F);
            this.label_ProgressInfo.Location = new System.Drawing.Point(71, 34);
            this.label_ProgressInfo.Name = "label_ProgressInfo";
            this.label_ProgressInfo.Size = new System.Drawing.Size(170, 17);
            this.label_ProgressInfo.TabIndex = 0;
            this.label_ProgressInfo.Text = "label_ProgressInfo";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(74, 69);
            this.progressBar1.MarqueeAnimationSpeed = 30;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(666, 58);
            this.progressBar1.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label_ElapsedTime
            // 
            this.label_ElapsedTime.AutoSize = true;
            this.label_ElapsedTime.Font = new System.Drawing.Font("宋体", 10F);
            this.label_ElapsedTime.Location = new System.Drawing.Point(760, 89);
            this.label_ElapsedTime.Name = "label_ElapsedTime";
            this.label_ElapsedTime.Size = new System.Drawing.Size(0, 17);
            this.label_ElapsedTime.TabIndex = 2;
            // 
            // timer2
            // 
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // label_Time
            // 
            this.label_Time.AutoSize = true;
            this.label_Time.Font = new System.Drawing.Font("宋体", 10F);
            this.label_Time.Location = new System.Drawing.Point(364, 34);
            this.label_Time.Name = "label_Time";
            this.label_Time.Size = new System.Drawing.Size(98, 17);
            this.label_Time.TabIndex = 3;
            this.label_Time.Text = "label_Time";
            // 
            // Form_ProgressInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(825, 176);
            this.Controls.Add(this.label_Time);
            this.Controls.Add(this.label_ElapsedTime);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label_ProgressInfo);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_ProgressInfo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "进度提示";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_ProgressInfo_FormClosing);
            this.Load += new System.EventHandler(this.Form_ProgressInfo_Load);
            this.Shown += new System.EventHandler(this.Form_ProgressInfo_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.ProgressBar progressBar1;
        public System.Windows.Forms.Label label_ProgressInfo;
        private System.Windows.Forms.Timer timer1;
        public System.Windows.Forms.Label label_ElapsedTime;
        private System.Windows.Forms.Timer timer2;
        public System.Windows.Forms.Label label_Time;
    }
}