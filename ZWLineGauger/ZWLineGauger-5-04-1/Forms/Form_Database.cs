using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZWLineGauger
{
    public partial class Form_Database : Form
    {
        MainUI m_parent;

        public static string m_strDataSource = "";
        public static string m_strDatabaseTask = "MeasureTask";
        public static string m_strDatabaseStdLib = "StandardLib";
        public static string m_strSQLUser = "sa";
        public static string m_strSQLPwd = "123321";

        public Form_Database(MainUI parent)
        {
            this.m_parent = parent;
            InitializeComponent();
            
            textBox_DataSource.Text = m_strDataSource;
            textBox_DatabaseTask.Text = m_strDatabaseTask;
            textBox_DatabaseStdLib.Text = m_strDatabaseStdLib;
            textBox_UserName.Text = m_strSQLUser;
            textBox_Pwd.Text = m_strSQLPwd;

            checkBox_UseDatabase.Checked = m_parent.m_bUseDatabase;
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            m_strDataSource = textBox_DataSource.Text;
            m_strDatabaseTask = textBox_DatabaseTask.Text;
            m_strDatabaseStdLib = textBox_DatabaseStdLib.Text;
            m_strSQLUser = textBox_UserName.Text;
            m_strSQLPwd = textBox_Pwd.Text;

            m_parent.m_bUseDatabase = checkBox_UseDatabase.Checked;

            m_parent.SaveAppParams();
            Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form_Database_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
        }
    }
}
