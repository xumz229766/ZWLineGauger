using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ZWLineGauger
{
    public partial class Form_CreateTask : Form
    {
        MainUI parent;

        public Form_CreateTask(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();

            textBox_ProductModel.Text = parent.m_strCurrentProductModel;
            textBox_Layer.Text = parent.m_strCurrentProductLayer;
            textBox_ProductNumber.Text = parent.m_strCurrentProductNumber;

            //comboBox_CreateMode.Items.Add("CAM图纸模式");
            //comboBox_CreateMode.Items.Add("手动学习模式");
            //comboBox_CreateMode.SelectedIndex = (0 == parent.m_nCreateTaskMode) ? 0 : 1;
        }

        // 按钮：取消
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // 按钮：确定
        private void btn_OK_Click(object sender, EventArgs e)
        {
            parent.m_strCurrentProductModel = textBox_ProductModel.Text;
            parent.m_strCurrentProductLayer = textBox_Layer.Text;
            parent.m_strCurrentProductNumber = textBox_ProductNumber.Text;

            if (parent.m_strCurrentProductModel.Length > 0)
            {
                // 先判断是否存在同名任务
                string strTaskName;
                if (parent.m_strCurrentProductLayer.Length > 0)
                    strTaskName = "s" + parent.m_strCurrentProductModel + "_" + parent.m_strCurrentProductLayer;
                else
                    strTaskName = "s" + parent.m_strCurrentProductModel;

                foreach (string name in parent.m_vec_SQL_table_names)
                {
                    if (strTaskName == "s" + name)
                    {
                        if (MessageBox.Show(this, "已存在同名任务，是否继续创建?", "提示", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                            return;
                    }
                }

                // 通告主线程
                string msg = string.Format("开始创建任务");
                parent.CBD_SendMessage(msg, false, null, null);

                this.Close();
            }
            else
            {
                MessageBox.Show(this, "料号为空，无法创建！", "提示", MessageBoxButtons.OK);

                this.textBox_ProductModel.Focus();
            }
        }

        // 组合框：首件制作方式选择变更
        private void comboBox_CreateMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            //parent.m_nCreateTaskMode = comboBox_CreateMode.SelectedIndex;
        }

        private void Form_CreateTask_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }
    }
}
