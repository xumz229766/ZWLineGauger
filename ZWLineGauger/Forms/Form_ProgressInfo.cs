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
using System.Threading;

namespace ZWLineGauger
{
    public enum PROGRESS_WAIT_MODE
    {
        NONE = 0,
        NORMAL,
        WAIT_FOR_LOADING_ODB,
        WAIT_FOR_RENDERING
    }

    public partial class Form_ProgressInfo : Form
    {
        MainUI parent;

        bool   m_bInfiniteWait = false;
        int      m_nCounter = 0;

        public int   m_nTimeCounter = 0;

        public int   m_nTimer2Counter = 0;
        public int   m_nTimer2Interval = 100;

        private string   m_strInfo;
        private bool     m_bDisableClosing = false;

        public PROGRESS_WAIT_MODE   m_wait_mode = 0;

        public Form_ProgressInfo(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();

            label_ProgressInfo.Text = "";
            label_Time.Text = "";

            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            progressBar1.MarqueeAnimationSpeed = 100;
            progressBar1.Step = 5;
        }

        private void Form_ProgressInfo_Load(object sender, EventArgs e)
        {

        }

        private void Form_ProgressInfo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (true == Form_GraphOrientation.m_bIsReadingGraphForThumbnail)
                e.Cancel = true;
            
            if (true == m_bDisableClosing)
                e.Cancel = true;
            else
            {
                if ((false == parent.m_bOfflineMode) && (false == MainUI.m_motion.m_bHomed) && (true == parent.m_bNeedToInitMotionSystem))
                {
                    if (DialogResult.Yes == MessageBox.Show(this, "是否停止初始化?", "提示", MessageBoxButtons.YesNo))
                    {
                        MainUI.m_motion.m_bStopHomingThread = true;
                        Thread.Sleep(300);
                    }
                    else
                        e.Cancel = true;
                }
            }
        }

        public void set_infinite_wait_mode(PROGRESS_WAIT_MODE mode)
        {
            m_bInfiniteWait = true;
            m_wait_mode = mode;
        }

        // 设置对话框标题
        public void set_title(string title)
        {
            this.Text = title;
        }

        // 设置提示信息
        public void set_tip_info(string info)
        {
            m_strInfo = info;
            this.label_ProgressInfo.Text = m_strInfo;
        }

        // 设置无法手动关闭窗口
        public void disable_closing_window()
        {
            m_bDisableClosing = true;
        }
        public void enable_closing_window()
        {
            m_bDisableClosing = false;
        }

        // 重置进度条
        public void reset_progress()
        {
            m_nCounter = 0;
            progressBar1.Value = 0;
        }

        private void Form_ProgressInfo_Shown(object sender, EventArgs e)
        {
            if (true == m_bInfiniteWait)
            {
                m_nCounter = 0;
                m_nTimeCounter = 0;

                timer1.Interval = 360;
                timer1.Start();
            }

            m_nTimer2Counter = 0;
            timer2.Interval = m_nTimer2Interval;
            timer2.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //string msg = string.Format("222222 progressBar1 value = {0}", progressBar1.Value);
            //Debugger.Log(0, null, msg);

            m_nTimeCounter++;

            switch (m_wait_mode)
            {
                case PROGRESS_WAIT_MODE.NORMAL:
                    label_ProgressInfo.Text = m_strInfo;
                    //label_Time.Text = string.Format("已耗时: {0}秒", (m_nTimeCounter * timer1.Interval / 1000));
                    break;
                case PROGRESS_WAIT_MODE.WAIT_FOR_LOADING_ODB:
                    label_ProgressInfo.Text = "正在加载ODB文件，请稍等......";
                    //label_Time.Text = string.Format("已耗时: {0}秒", (m_nTimeCounter * timer1.Interval / 1000));
                    break;
                case PROGRESS_WAIT_MODE.WAIT_FOR_RENDERING:
                    label_ProgressInfo.Text = "正在解析和渲染图层文件......";
                    //label_Time.Text = string.Format("已耗时: {0}秒", (m_nTimeCounter * timer1.Interval / 1000));
                    break;
            }
            
            int temp = progressBar1.Value + progressBar1.Step;
            if (temp > 100)
            {
                m_nCounter++;
                if (m_nCounter > 5)
                {
                    m_nCounter = 0;
                    progressBar1.Value = 0;
                }
            }
            else
                progressBar1.PerformStep();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            m_nTimer2Counter++;
        }
    }
}
