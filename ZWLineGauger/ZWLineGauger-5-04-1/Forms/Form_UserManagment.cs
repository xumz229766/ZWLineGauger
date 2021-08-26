using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZWLineGauger.Forms
{
    public partial class Form_UserManagment : Form//显示所有
    {

        public String m_user_path;
        private int gridview_user_message_delete_index = -1;

        int user_num = 0;
        string name;

        string str;//用作读取

        public Form_UserManagment(Form parent)
        {
            InitializeComponent();

            m_user_path = "configs\\User.cfg";

            gridview_user_message.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridview_user_message.RowHeadersVisible = false;
            gridview_user_message.ReadOnly = true;
            gridview_user_message.ColumnCount = 3;
            gridview_user_message.ColumnHeadersVisible = true;
            gridview_user_message.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_user_message.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_user_message.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_user_message.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridview_user_message.Columns[0].Name = "账号";
            gridview_user_message.Columns[1].Name = "密码";
            gridview_user_message.Columns[2].Name = "身份";

            comboBox_User.Items.Add("操作员");
            comboBox_User.Items.Add("管理员");
            comboBox_User.SelectedIndex = 0;

            int x = (System.Windows.Forms.SystemInformation.WorkingArea.Width - this.Size.Width) / 2;
            int y = (System.Windows.Forms.SystemInformation.WorkingArea.Height - this.Size.Height) / 2;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = (Point)new Size(x,y);

            //判断文件是否存在
            if (File.Exists(m_user_path))
            {
                string name_value = "", pass_value = "", status_value = "";
                int n = 0;

                //单独一行进行读取
                foreach (string str in System.IO.File.ReadAllLines(m_user_path, Encoding.Default))
                {
                    Console.WriteLine(str); // str就是每一行数据

                    if (n == 0)//第一行为账户个数
                    {
                        //MessageBox.Show(str);
                        
                        user_num = Convert.ToInt32(str);
                    }
                    else
                    {
                        GetKeyValue(str, "name", ref name_value);
                        GetKeyValue(str, "pass", ref pass_value);
                        GetKeyValue(str, "status", ref status_value);

                        if(status_value == "0")
                        {
                            status_value = "管理员";
                        }
                        else if(status_value == "1")
                        {
                            status_value = "操作员";
                        }

                        String[] row = new String[3] { name_value, pass_value, status_value };

                        gridview_user_message.Rows.Add(row);

                    }

                    n++;
                }
            }
            else
            {
                File.Create(m_user_path).Close();
            }

        }



        //截取文件中的数据
        static public bool GetKeyValue(string buf, string key, ref string value)
        {
            int index = buf.IndexOf(key);

            if (index < 0)
                return false;

            string sub = buf.Substring(index, key.Length);

            if (sub == key)
            {
                int start_idx = index + key.Length + 1;
                if (buf.Length > start_idx)
                {
                    int idx = buf.IndexOf(",", start_idx);
                    if (idx > 0)
                    {
                        value = buf.Substring(start_idx, idx - start_idx);
                        return true;
                    }
                }
            }

            return false;
        }

        //添加新账户
        private void add_user_btn_Click(object sender, EventArgs e)
        {
            String[] row = new String[3] { this.text_name.Text.ToString(), this.text_pass.Text.ToString(), "" };
            int n = 0, same_name_flag = 0;
            string name_value = "";

            //判断管理员还是操作员
            switch (comboBox_User.SelectedIndex)
            {
                case 0:
                    row[2] = "管理员";

                    break;

                case 1:
                    row[2] = "操作员";

                    break;
            }

            //判断重复的账号
            foreach (string str in System.IO.File.ReadAllLines(m_user_path, Encoding.Default))
            {
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
                    //添加入gridview_user_message
                    this.gridview_user_message.Rows.Add(row);
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
                this.text_pass_sure.Clear();
            }
        }

        //删除账户
        private void delete_user_btn_Click(object sender, EventArgs e)
        {
            if (gridview_user_message_delete_index >= 0 && gridview_user_message_delete_index < this.gridview_user_message.Rows.Count - 1)
            {
                if (DialogResult.Yes == MessageBox.Show("是否删除选中的账号？", "提示", MessageBoxButtons.YesNo))
                {
                    this.gridview_user_message.Rows.RemoveAt(gridview_user_message_delete_index);
                }
            }
            else
            {
                MessageBox.Show("空白行不可删除！", "警告");
            }
        }

        //保存账户信息
        private void user_message_save_Click(object sender, EventArgs e)
        {
            if (m_user_path.Length <= 0)
                return;

            String str = "";
            StreamWriter writer = new StreamWriter(m_user_path);

            //清空文档
            writer.Write(string.Empty);

            //首行账户个数
            user_num = gridview_user_message.RowCount - 1;
            writer.WriteLine(user_num.ToString());

            for (int i = 0; i < this.gridview_user_message.Rows.Count - 1; i++)
            {
                str = String.Format("name={0},pass={1},status={2},", this.gridview_user_message.Rows[i].Cells[0].Value.ToString(), this.gridview_user_message.Rows[i].Cells[1].Value.ToString(), this.gridview_user_message.Rows[i].Cells[2].Value.ToString());
                writer.WriteLine(str);
            }

            writer.Close();

            MessageBox.Show("保存成功！", "提示");
            this.Close();
        }

        //关闭窗口
        private void user_message_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
            
        }

        //单击账户
        private void gridview_GraphMeasureItems_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            gridview_user_message_delete_index = e.RowIndex;
        }

    }
}
