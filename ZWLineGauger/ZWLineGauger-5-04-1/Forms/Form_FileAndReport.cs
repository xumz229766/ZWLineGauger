using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace ZWLineGauger
{
    public partial class Form_FileAndReport : Form
    {
        MainUI parent;

        public Form_FileAndReport(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();

            this.textBox_TaskFileSavingDir.Text = parent.m_strTaskFileSavingDir;
            
            this.textBox_ImageSavingDir.Text = parent.m_strImageSavingDir;
            this.checkBox_AutoSaveResultImage.Checked = parent.m_bSaveGaugeResultImage;

            this.textBox_ExcelSavingDir.Text = parent.m_strExcelSavingDir;
            this.checkBox_AutoSaveResultExcel.Checked = parent.m_bSaveGaugeResultExcelReport;
        }

        private void btn_SelectImageSavingDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir = new FolderBrowserDialog();
            dir.ShowDialog();
            if (dir.SelectedPath.Length > 0)
            {
                this.textBox_ImageSavingDir.Text = dir.SelectedPath;
            }
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            parent.m_strTaskFileSavingDir = this.textBox_TaskFileSavingDir.Text;
            parent.m_strImageSavingDir = this.textBox_ImageSavingDir.Text;
            parent.m_strExcelSavingDir = this.textBox_ExcelSavingDir.Text;
            
            parent.SaveAppParams();

            Close();
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Form_FileAndReport_Load(object sender, EventArgs e)
        {
            
        }

        private void checkBox_AutoSaveResultImage_CheckedChanged(object sender, EventArgs e)
        {
            parent.m_bSaveGaugeResultImage = checkBox_AutoSaveResultImage.Checked;
        }

        private void btn_SelectExcelSavingDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir = new FolderBrowserDialog();
            dir.ShowDialog();
            if (dir.SelectedPath.Length > 0)
            {
                this.textBox_ExcelSavingDir.Text = dir.SelectedPath;
            }
        }

        private void checkBox_AutoSaveResultExcel_CheckedChanged(object sender, EventArgs e)
        {
            parent.m_bSaveGaugeResultExcelReport = checkBox_AutoSaveResultExcel.Checked;
        }

        private void btn_SelectTaskFileSavingDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dir = new FolderBrowserDialog();
            dir.ShowDialog();
            if (dir.SelectedPath.Length > 0)
            {
                this.textBox_TaskFileSavingDir.Text = dir.SelectedPath;
            }
        }
    }
}
