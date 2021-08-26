using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZWLineGauger.Forms
{
    public partial class Form_SelectCreationMode : Form
    {
        MainUI parent;

        public Form_SelectCreationMode(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();

            bool bHasGraph = true;
            if (null == MainUI.m_graph_view.Image)
                bHasGraph = false;
            else
            {
                if ((MainUI.m_graph_view.Image.Width * MainUI.m_graph_view.Image.Height) <= 0)
                    bHasGraph = false;
            }
            if (MainUI.m_graph_view.m_zoom_ratio < 0.000001)
                bHasGraph = false;
            
            if (false == bHasGraph)
                checkBox_UseCurrentGraph.Enabled = false;

            comboBox_ProductType.Items.Add("盲孔");
            comboBox_ProductType.Items.Add("通孔");
            comboBox_ProductType.SelectedIndex = 0;
        }

        private void btn_GraphMode_Click(object sender, EventArgs e)
        {
            Close();

            parent.m_nCreateTaskMode = 0;
            //parent.m_nProductType = comboBox_ProductType.SelectedIndex;
            
            if (true == checkBox_UseCurrentGraph.Checked)
            {
                MainUI.dl_message_sender messenger = parent.CBD_SendMessage;
                messenger(string.Format("开始创建任务"), false, null, null);
            }
            else
                parent.btn_LoadGraph_Click(new object(), new EventArgs());
        }

        private void btn_ManualMode_Click(object sender, EventArgs e)
        {
            Close();

            parent.m_nCreateTaskMode = 1;

            // 通告主线程
            string msg = string.Format("开始创建任务");
            parent.CBD_SendMessage(msg, false, null, null);

            //Form_CreateTask form = new Form_CreateTask(parent);
            //form.ShowInTaskbar = false;
            //form.ShowDialog();
        }

        private void Form_SelectCreationMode_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void Form_SelectCreationMode_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }

        private void btn_TxtMode_Click(object sender, EventArgs e)
        {
            Close();

            parent.m_nCreateTaskMode = 2;

            // 通告主线程
            string msg = string.Format("开始创建任务");
            parent.CBD_SendMessage(msg, false, null, null);
        }

        private void comboBox_ProductType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
