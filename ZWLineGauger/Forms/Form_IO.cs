using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ZWLineGauger.Hardwares;
using System.Diagnostics;
using System.Threading;

namespace ZWLineGauger.Forms
{
    public partial class Form_IO : Form
    {
        static MainUI form_parent;

        public Thread thrd;

        public delegate void io_form_input(IO_STATE info, string str);

        public Form_IO(MainUI parent)
        {
            Form_IO.form_parent = parent;
            InitializeComponent();

            render_buttun_bgcolor(btn_Status_Beeper, Color.FromArgb(172, 172, 172));
            render_buttun_bgcolor(btn_Status_Vacuum, Color.FromArgb(172, 172, 172));
            render_buttun_bgcolor(btn_Status_RedDot, Color.FromArgb(172, 172, 172));

            textBox_Beeper.Text = Convert.ToString(form_parent.m_IO.m_output_beeper);
            textBox_Vacuum.Text = Convert.ToString(form_parent.m_IO.m_output_vacuum);
            textBox_RedDot.Text = Convert.ToString(form_parent.m_IO.m_output_red_dot);

            textBox_GreenLight.Text = Convert.ToString(form_parent.m_IO.m_output_green_light);
            textBox_YellowLight.Text = Convert.ToString(form_parent.m_IO.m_output_yellow_light);
            textBox_RedLight.Text = Convert.ToString(form_parent.m_IO.m_output_red_light);

            textBox_Input_Vacuum.Text = Convert.ToString(form_parent.m_IO.m_input_vacuum);
            textBox_Input_Emergency.Text = Convert.ToString(form_parent.m_IO.m_input_emergency);
            textBox_Input_HeightSensor.Text = Convert.ToString(form_parent.m_IO.m_input_height_sensor);
            textBox_Input_Start.Text = Convert.ToString(form_parent.m_IO.m_input_start_button);

            IO_STATE state = IO_STATE.NONE;
            parent.m_IO.get_IO_output_state(parent.m_IO.m_output_vacuum, ref state);
            if (IO_STATE.IO_LOW == state)
            {
                btn_Vacuum.Text = "关";
                render_buttun_bgcolor(btn_Status_Vacuum, Color.FromArgb(0, 235, 0));
            }

            state = IO_STATE.NONE;
            parent.m_IO.get_IO_output_state(parent.m_IO.m_output_red_dot, ref state);
            if (IO_STATE.IO_LOW == state)
            {
                btn_RedDot.Text = "关";
                render_buttun_bgcolor(btn_Status_RedDot, Color.FromArgb(0, 235, 0));
            }
            else if (IO_STATE.IO_HIGH == state)
            {

            }

            state = IO_STATE.NONE;
            parent.m_IO.get_IO_output_state(parent.m_IO.m_output_green_light, ref state);
            if (IO_STATE.IO_LOW == state)
            {
                btn_GreenLight.Text = "关";
                render_buttun_bgcolor(btn_Status_GreenLight, Color.FromArgb(0, 235, 0));
            }
            else if (IO_STATE.IO_HIGH == state)
            {

            }

            state = IO_STATE.NONE;
            parent.m_IO.get_IO_output_state(parent.m_IO.m_output_yellow_light, ref state);
            if (IO_STATE.IO_LOW == state)
            {
                btn_YellowLight.Text = "关";
                render_buttun_bgcolor(btn_Status_YellowLight, Color.FromArgb(0, 235, 0));
            }
            else if (IO_STATE.IO_HIGH == state)
            {

            }

            state = IO_STATE.NONE;
            parent.m_IO.get_IO_output_state(parent.m_IO.m_output_red_light, ref state);
            if (IO_STATE.IO_LOW == state)
            {
                btn_RedLight.Text = "关";
                render_buttun_bgcolor(btn_Status_RedLight, Color.FromArgb(0, 235, 0));
            }
            else if (IO_STATE.IO_HIGH == state)
            {

            }

            state = IO_STATE.NONE;
            parent.m_IO.get_IO_input(parent.m_IO.m_input_vacuum, ref state);
            if (IO_STATE.IO_LOW == state)
                render_buttun_bgcolor(btn_InputStatus_Vacuum, Color.FromArgb(0, 235, 0));

            state = IO_STATE.NONE;
            parent.m_IO.get_IO_input(parent.m_IO.m_input_emergency, ref state);
            if (IO_STATE.IO_LOW == state)
                render_buttun_bgcolor(btn_InputStatus_Emergency, Color.FromArgb(0, 235, 0));

            state = IO_STATE.NONE;
            parent.m_IO.get_IO_input(parent.m_IO.m_input_height_sensor, ref state);
            if (IO_STATE.IO_LOW == state)
                render_buttun_bgcolor(btn_InputStatus_HeightSensor, Color.FromArgb(0, 235, 0));

            state = IO_STATE.NONE;
            parent.m_IO.get_IO_input(parent.m_IO.m_input_start_button, ref state);
            if (IO_STATE.IO_LOW == state)
                render_buttun_bgcolor(btn_InputStatus_Start, Color.FromArgb(0, 235, 0));
        }

        private void Form_IO_Load(object sender, EventArgs e)
        {
            GeneralUtils.set_cursor_pos(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
            //监控io线程启动
            //Thread thrd = new Thread(thread_form_io);
            thrd = new Thread(thread_form_io);
            thrd.Start();
            Form_IO.form_parent.m_bPause = true;//使运行按钮暂时失效
            this.FormClosing += new FormClosingEventHandler(form_io_closing);
        }

        //线程：打开IO窗体监测输入
        public void thread_form_io(object obj)
        {
            IO_STATE vacuum_state = IO_STATE.NONE;
            IO_STATE height_sensor_state = IO_STATE.NONE;
            IO_STATE stop_btn_state = IO_STATE.NONE;
            IO_STATE start_btn_state = IO_STATE.NONE;

            while (true)
            {
                form_parent.m_IO.get_IO_input(form_parent.m_IO.m_input_vacuum, ref vacuum_state);
                set_input_io(vacuum_state, "吸附按钮");

                form_parent.m_IO.get_IO_input(form_parent.m_IO.m_input_height_sensor, ref height_sensor_state);
                set_input_io(height_sensor_state, "超声波");

                form_parent.m_IO.get_IO_input(form_parent.m_IO.m_input_emergency, ref stop_btn_state);
                set_input_io(stop_btn_state, "急停按钮");

                form_parent.m_IO.get_IO_input(form_parent.m_IO.m_input_start_button, ref start_btn_state);
                set_input_io(start_btn_state, "运行按钮");

                for (int k = 0; k < 20; k++)
                    Thread.Sleep(10);
            }

        }

        //监控io口所执行的程序
        private void set_input_io(IO_STATE info, string str)
        {
            //Debugger.Log(0, null, string.Format("222222 info = {0}  str = {1}", info, str));
            if (this.InvokeRequired)
            {
                io_form_input io_Form_Input = new io_form_input(set_input_io);

                this.Invoke(io_Form_Input, info, str);
            }
            else
            {
                if(str == "吸附按钮")
                {
                    
                    if (true == form_parent.checkBox_Vacuum.Checked)
                        render_buttun_bgcolor(btn_InputStatus_Vacuum, Color.FromArgb(0, 235, 0));//开
                    else
                        render_buttun_bgcolor(btn_InputStatus_Vacuum, Color.FromArgb(172, 172, 172));//关
                }
                else if(str == "超声波")
                {
                    if (info == IO_STATE.IO_LOW)
                        render_buttun_bgcolor(btn_InputStatus_HeightSensor, Color.FromArgb(0, 235, 0));//开
                    else
                        render_buttun_bgcolor(btn_InputStatus_HeightSensor, Color.FromArgb(172, 172, 172));//关
                }
                else if (str == "急停按钮")
                {
                    if (info == IO_STATE.IO_HIGH)
                        render_buttun_bgcolor(btn_InputStatus_Emergency, Color.FromArgb(0, 235, 0));//开
                    else
                        render_buttun_bgcolor(btn_InputStatus_Emergency, Color.FromArgb(172, 172, 172));//关
                }
                else if (str == "运行按钮")
                {
                    
                    if (info == IO_STATE.IO_LOW)
                        render_buttun_bgcolor(btn_InputStatus_Start, Color.FromArgb(0, 235, 0));//开
                    else
                        render_buttun_bgcolor(btn_InputStatus_Start, Color.FromArgb(172, 172, 172));//关
                }


            }
        }

        

        // 设置按钮背景色
        void render_buttun_bgcolor(Button btn, Color color)
        {
            Bitmap bmp = new Bitmap(btn.Width, btn.Height);
            Graphics g = Graphics.FromImage(bmp);
            SolidBrush b = new SolidBrush(color);
            g.FillRectangle(b, 0, 0, btn.Width, btn.Height);

            btn.Image = bmp;
        }

        // 保存
        private void btn_Save_Click(object sender, EventArgs e)
        {
            form_parent.m_IO.m_output_beeper = Convert.ToInt32(textBox_Beeper.Text);
            form_parent.m_IO.m_output_vacuum = Convert.ToInt32(textBox_Vacuum.Text);
            form_parent.m_IO.m_output_red_dot = Convert.ToInt32(textBox_RedDot.Text);

            form_parent.m_IO.m_output_green_light = Convert.ToInt32(textBox_GreenLight.Text);
            form_parent.m_IO.m_output_yellow_light = Convert.ToInt32(textBox_YellowLight.Text);
            form_parent.m_IO.m_output_red_light = Convert.ToInt32(textBox_RedLight.Text);

            form_parent.m_IO.m_input_vacuum = Convert.ToInt32(textBox_Input_Vacuum.Text);
            form_parent.m_IO.m_input_emergency = Convert.ToInt32(textBox_Input_Emergency.Text);
            form_parent.m_IO.m_input_height_sensor = Convert.ToInt32(textBox_Input_HeightSensor.Text);
            form_parent.m_IO.m_input_start_button = Convert.ToInt32(textBox_Input_Start.Text);

            thrd.Abort();

            if (true == form_parent.m_IO.save_params())
            {
                MessageBox.Show(this, "保存成功", "提示", MessageBoxButtons.OK);
                Close();
            }
            else
            {
                MessageBox.Show(this, "IO参数有误，请检查修改再保存。", "提示", MessageBoxButtons.OK);
            }
        }

        //用于关闭检测线程
        private void form_io_closing(object sender, EventArgs e)
        {
            Form_IO.form_parent.m_bPause = false;
            thrd.Abort();
        }


        // 取消
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            thrd.Abort();
            Close();
        }

        // 输出：蜂鸣器
        private void btn_Beeper_Click(object sender, EventArgs e)
        {
            if ("开" == btn_Beeper.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_Beeper.Text), Hardwares.IO_STATE.IO_LOW))
                        MessageBox.Show(this, "蜂鸣器IO设置失败，请检查原因。", "提示");
                }
                btn_Beeper.Text = "关";
                render_buttun_bgcolor(btn_Status_Beeper, Color.FromArgb(0, 235, 0));
            }
            else if ("关" == btn_Beeper.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_Beeper.Text), Hardwares.IO_STATE.IO_HIGH))
                        MessageBox.Show(this, "蜂鸣器IO设置失败，请检查原因。", "提示");
                }
                btn_Beeper.Text = "开";
                render_buttun_bgcolor(btn_Status_Beeper, Color.FromArgb(172, 172, 172));
            }
        }

        // 输出：吸附
        private void btn_Vacuum_Click(object sender, EventArgs e)
        {
            if ("开" == btn_Vacuum.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_Vacuum.Text), Hardwares.IO_STATE.IO_LOW))
                        MessageBox.Show(this, "吸附IO设置失败，请检查原因。", "提示");
                }
                btn_Vacuum.Text = "关";
                render_buttun_bgcolor(btn_Status_Vacuum, Color.FromArgb(0, 235, 0));
            }
            else if ("关" == btn_Vacuum.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_Vacuum.Text), Hardwares.IO_STATE.IO_HIGH))
                        MessageBox.Show(this, "吸附IO设置失败，请检查原因。", "提示");
                }
                btn_Vacuum.Text = "开";
                render_buttun_bgcolor(btn_Status_Vacuum, Color.FromArgb(172, 172, 172));
            }
        }

        // 输出：红点指示器
        private void btn_RedDot_Click(object sender, EventArgs e)
        {
            if ("开" == btn_RedDot.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_RedDot.Text), Hardwares.IO_STATE.IO_LOW))
                        MessageBox.Show(this, "红点指示器IO设置失败，请检查原因。", "提示");
                }
                btn_RedDot.Text = "关";
                render_buttun_bgcolor(btn_Status_RedDot, Color.FromArgb(0, 235, 0));
            }
            else if ("关" == btn_RedDot.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_RedDot.Text), Hardwares.IO_STATE.IO_HIGH))
                        MessageBox.Show(this, "红点指示器IO设置失败，请检查原因。", "提示");
                }
                btn_RedDot.Text = "开";
                render_buttun_bgcolor(btn_Status_RedDot, Color.FromArgb(172, 172, 172));
            }
        }

        // 输出：绿灯
        private void btn_GreenLight_Click(object sender, EventArgs e)
        {
            if ("开" == btn_GreenLight.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_GreenLight.Text), Hardwares.IO_STATE.IO_LOW))
                        MessageBox.Show(this, "绿灯IO设置失败，请检查原因。", "提示");
                }
                btn_GreenLight.Text = "关";
                render_buttun_bgcolor(btn_Status_GreenLight, Color.FromArgb(0, 235, 0));
            }
            else if ("关" == btn_GreenLight.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_GreenLight.Text), Hardwares.IO_STATE.IO_HIGH))
                        MessageBox.Show(this, "绿灯IO设置失败，请检查原因。", "提示");
                }
                btn_GreenLight.Text = "开";
                render_buttun_bgcolor(btn_Status_GreenLight, Color.FromArgb(172, 172, 172));
            }
        }

        // 输出：黄灯
        private void btn_YellowLight_Click(object sender, EventArgs e)
        {
            if ("开" == btn_YellowLight.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_YellowLight.Text), Hardwares.IO_STATE.IO_LOW))
                        MessageBox.Show(this, "黄灯IO设置失败，请检查原因。", "提示");
                }
                btn_YellowLight.Text = "关";
                render_buttun_bgcolor(btn_Status_YellowLight, Color.FromArgb(0, 235, 0));
            }
            else if ("关" == btn_YellowLight.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_YellowLight.Text), Hardwares.IO_STATE.IO_HIGH))
                        MessageBox.Show(this, "黄灯IO设置失败，请检查原因。", "提示");
                }
                btn_YellowLight.Text = "开";
                render_buttun_bgcolor(btn_Status_YellowLight, Color.FromArgb(172, 172, 172));
            }
        }

        // 输出：红灯
        private void btn_RedLight_Click(object sender, EventArgs e)
        {
            if ("开" == btn_RedLight.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_RedLight.Text), Hardwares.IO_STATE.IO_LOW))
                        MessageBox.Show(this, "红灯IO设置失败，请检查原因。", "提示");
                }
                btn_RedLight.Text = "关";
                render_buttun_bgcolor(btn_Status_RedLight, Color.FromArgb(0, 235, 0));
            }
            else if ("关" == btn_RedLight.Text)
            {
                if (false == form_parent.m_bOfflineMode)
                {
                    if (false == form_parent.m_IO.set_IO_output(Convert.ToInt32(textBox_RedLight.Text), Hardwares.IO_STATE.IO_HIGH))
                        MessageBox.Show(this, "红灯IO设置失败，请检查原因。", "提示");
                }
                btn_RedLight.Text = "开";
                render_buttun_bgcolor(btn_Status_RedLight, Color.FromArgb(172, 172, 172));
            }
        }
    }
}
