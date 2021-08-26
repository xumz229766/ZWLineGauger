using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZWLineGauger
{
    public partial class Form_RedDot : Form
    {
        MainUI parent;

        public Form_RedDot(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();

            checkBox_ActivateRedDot.Checked = parent.m_bUseRedDot;

            textBox_OffsetX.Text = Convert.ToString(parent.m_dbRedDotOffsetX);
            textBox_OffsetY.Text = Convert.ToString(parent.m_dbRedDotOffsetY);
        }
        
        private void btn_Save_Click(object sender, EventArgs e)
        {
            parent.m_bUseRedDot = checkBox_ActivateRedDot.Checked;
            parent.m_dbRedDotOffsetX = Convert.ToDouble(textBox_OffsetX.Text);
            parent.m_dbRedDotOffsetY = Convert.ToDouble(textBox_OffsetY.Text);

            parent.SaveAppParams();

            MessageBox.Show(this, "保存成功", "提示");
            Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btn_Test_Click(object sender, EventArgs e)
        {
            if (false == parent.m_bOfflineMode)
            {
                Point3d crd = new Point3d(0, 0, 0);
                MainUI.m_motion.get_xyz_crds(ref crd);

                double off_x = Convert.ToDouble(textBox_OffsetX.Text);
                double off_y = Convert.ToDouble(textBox_OffsetY.Text);

                MainUI.m_motion.m_threaded_move_dest_X = crd.x + off_x;
                MainUI.m_motion.m_threaded_move_dest_Y = crd.y + off_y;
                MainUI.m_motion.m_threaded_move_dest_Z = crd.z;

                MainUI.dl_message_sender messenger = parent.CBD_SendMessage;
                (new Thread(MainUI.m_motion.threaded_linear_XYZ_wait_until_stop)).Start(messenger);
            }
        }

        private void Form_RedDot_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }
    }
}
