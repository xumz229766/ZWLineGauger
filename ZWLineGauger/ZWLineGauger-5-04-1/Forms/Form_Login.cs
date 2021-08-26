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
    public partial class Form_Login : Form
    {
        int close_flag = 0;
        MainUI parent;
        public string m_user_path = "configs\\User.cfg";
        List<User> users = new List<User>();
        //存放账户信息
        struct User
        {
            public string name;
            public string pass;
            public string status;
        };

        //界面加载
        public Form_Login(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();
            string first_name = "";
            //textBox_name.Clear();
            //textBox_name.Focus();
            textBox_Status.ReadOnly = true;

            //判断文件是否存在
            if (File.Exists(m_user_path))
            {
                string name_value = "", pass_value = "", status_value = "";
                int n = 0;

                //单独一行进行读取
                foreach (string str in System.IO.File.ReadAllLines(m_user_path, Encoding.Default))
                {
                    Console.WriteLine(str); // str就是每一行数据

                    if (n != 0)//第一行为账户个数
                    {
                        Form_UserManagment.GetKeyValue(str, "name", ref name_value);
                        Form_UserManagment.GetKeyValue(str, "pass", ref pass_value);
                        Form_UserManagment.GetKeyValue(str, "status", ref status_value);

                        comboBox_Name.Items.Add(name_value);

                        if (n == 1)
                            first_name = name_value;

                        String[] row = new String[3] { name_value, pass_value, status_value };

                        //gridview_user_message.Rows.Add(row);
                        User u1 = new User { name = name_value, pass = pass_value, status = status_value };
                        users.Add(u1);
                    }
                    n++;
                }
            }
            else
            {
                File.Create(m_user_path).Close();
            }

            //comboBox_Name.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox_Name.Text = first_name;
        }

        //登录按钮
        private void btn_OK_Click(object sender, EventArgs e)
        {
            MainUI.dl_message_sender send_message = parent.CBD_SendMessage;
            int n = 0;
            foreach (User use in users)
            {
                if (use.name == this.comboBox_Name.Text && use.pass == this.textBox_Password.Text)
                {
                    n++;
                    switch (Convert.ToInt32(use.status))
                    {
                        case 0:
                            close_flag = 1;
                            send_message("登录", false, 0, use.name);
                            this.Close();
                            break;

                        case 1:
                            close_flag = 1;
                            send_message("登录", false, 1, use.name);
                            this.Close();
                            break;
                    }
                }
            }

            if (0 == n)
            {
                MessageBox.Show("账号或密码错误，请重新输入！", "警告");
                //textBox_name.Clear();
                textBox_Password.Clear();
                textBox_Status.Clear();
            }
                
        }

        //取消按钮
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();  
        }

        //显示账户的身份
        private void textBox_name_change(object sender, EventArgs e)
        {
            //
            foreach (User use in users)
            {
                if (use.name == this.comboBox_Name.Text)
                {
                    switch (Convert.ToInt32(use.status))
                    {
                        case 0:
                            textBox_Status.Text = "管理员";
                            break;

                        case 1:
                            textBox_Status.Text = "操作员";
                            break;
                    }
                }
                
            }
        }

        private void Form_Login_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }

        
        
        

        
        //关闭事件
        private void Form_Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(close_flag == 0)
                parent.Close();
        }

        //用户管理
        private void btn_Managment_Click(object sender, EventArgs e)
        {
            int n = 0;
            foreach (User use in users)
            {
                if (use.name == this.comboBox_Name.Text && use.pass == this.textBox_Password.Text)
                {
                    n++;
                    switch (Convert.ToInt32(use.status))
                    {
                        case 0:
                            //this.Hide();

                            Form_UserManagment form = new Form_UserManagment(this);
                            form.ShowInTaskbar = false;
                            form.ShowDialog();
                            
                            break;

                        case 1:
                            MessageBox.Show("登录失败，操作员无法管理用户！", "警告");
                            break;
                    }
                }
            }

            if (0 == n)
            {
                MessageBox.Show("账号或密码错误，请重新输入！", "警告");
                //textBox_name.Clear();
                textBox_Password.Clear();
                textBox_Status.Clear();
            }
        }
    }
}
