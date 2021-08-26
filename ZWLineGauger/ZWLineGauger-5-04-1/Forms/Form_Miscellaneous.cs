using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZWLineGauger.Forms;

namespace ZWLineGauger
{
    public partial class Form_Miscellaneous : Form
    {
        MainUI parent;
        bool m_bOriginalOfflineMode = false;

        public Form_Miscellaneous(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();

            m_bOriginalOfflineMode = parent.m_bOfflineMode;

            textBox_MeasureTaskDelayTime.Text = Convert.ToString(parent.m_nMeasureTaskDelayTime);
        }

        private void checkBox_OfflineMode_CheckedChanged(object sender, EventArgs e)
        {
            parent.m_bOfflineMode = checkBox_OfflineMode.Checked;
        }

        private void Form_Miscellaneous_Load(object sender, EventArgs e)
        {
            this.checkBox_OfflineMode.Checked = parent.m_bOfflineMode;
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            parent.m_nMeasureTaskDelayTime = Convert.ToInt32(textBox_MeasureTaskDelayTime.Text);

            parent.SaveAppParams();

            MessageBox.Show(this, "保存成功", "提示");

            if (m_bOriginalOfflineMode != parent.m_bOfflineMode)
            {
                parent.m_bIgnoreHardwaresRelease = true;
                MessageBox.Show(this, "因为修改了离线模式设置，程序即将关闭，重启程序后设置生效。", "提示");
                Application.Exit();
            }

            Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btn_RestoreFactoryDefaults_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(this, "此操作会重置所有参数到出厂默认值，请选择是否继续?", "提示", MessageBoxButtons.YesNo);
            if (DialogResult.Yes == res)
            {
                MainUI.dl_message_sender send_message = parent.CBD_SendMessage;
                send_message("登录", false, 0, "null");

                Form_Login form = new Form_Login(parent);
                form.ShowInTaskbar = false;
                form.ShowDialog();

                if (parent.m_nCurrentUser > 0)
                {
                    send_message("恢复出厂设置", false, 0, 0);

                    MessageBox.Show(this, "出厂设置恢复完毕，程序即将关闭，重启程序后设置生效。", "提示");
                    Application.Exit();
                }
            }
        }
    }
}
