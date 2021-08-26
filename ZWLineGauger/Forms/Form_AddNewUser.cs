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
    public partial class Form_AddNewUser : Form
    {
        Form par;
        string m_user_path;//文件地址

        public Form_AddNewUser(Form parent, string par_path)
        {
            InitializeComponent();

            par = parent;
            m_user_path = par_path;
            text_name.Focus();

            comboBox_User.Items.Add("操作员");
            comboBox_User.Items.Add("管理员");
            comboBox_User.SelectedIndex = 0;

        }

        //确定添加
        private void add_user_btn_Click(object sender, EventArgs e)
        {
            DataGridView dataGridView = (DataGridView)par.Controls.Find("gridview_user_message", false)[0];
            String[] row = new String[3] { this.text_name.Text.ToString(), this.text_pass.Text.ToString(), "" };
            int n = 0, same_name_flag = 0;
            string name_value = "";

            //判断管理员还是操作员
            switch (comboBox_User.SelectedIndex)
            {
                case 0:
                    row[2] = "0";

                    break;

                case 1:
                    row[2] = "0";

                    break;
            }

            //判断重复的账号
            foreach (string str in System.IO.File.ReadAllLines(m_user_path, Encoding.Default))
            {
                //Console.WriteLine(str); // str就是每一行数据

                if (n > 0)//第一行为账户个数
                {
                    Form_UserManagment.GetKeyValue(str, "name", ref name_value);

                    if (name_value == this.text_name.Text.ToString())
                    {
                        same_name_flag = 1;
                        MessageBox.Show("不能有相同的账号！", "警告");
                        break;
                    }
                }

                n++;
            }

            //无相同的账户名
            if (same_name_flag == 0)
            {
                //判断text是否为空
                if (text_name.Text != string.Empty && text_pass.Text != string.Empty)
                {
                    dataGridView.Rows.Add(row);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("输入框不能为空！", "警告");
                }
            }
            else//存在相同的账户名
            {
                this.text_name.Clear();
                this.text_pass.Clear();
                //this.text_status.Clear();
            }
        }

        //取消
        private void cancel_btn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
