using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;

namespace ZWLineGauger.Hardwares
{
    public enum LIGHT_TYPE
    {
        TOP_LIGHT,
        BOTTOM_LIGHT
    }

    public class LightOps
    {
        MainUI   parent;

        public bool   m_bInitialized = false;

        public string   m_config_path = "";
        string m_received_data = "";

        public int   m_nComIndex = 0;            // 串口号
        public int   m_nBrightness = 1;
        public bool m_bOn = false;
        
        AutoResetEvent m_reset_event = new AutoResetEvent(false);

        //static public SerialPort m_port = new SerialPort();
        static public SerialPort   m_port;

        LIGHT_TYPE   m_type = LIGHT_TYPE.TOP_LIGHT;
        
        public LightOps(MainUI parent, SerialPort port, LIGHT_TYPE type)
        {
            this.parent = parent;
            m_port = port;
            m_type = type;
        }
        
        // 初始化串口参数，strName是串口名（如“COM3”）
        public bool init_port(string strName, int nBaudRate, int nDataBits, Parity par, StopBits stop_bit)
        {
            m_port.PortName = strName;
            m_port.BaudRate = nBaudRate;
            m_port.DataBits = nDataBits;
            m_port.Parity = par;
            m_port.StopBits = stop_bit;

            try
            {
                Debugger.Log(0, null, string.Format("222222 init_port 111"));
                m_port.Open();
                Debugger.Log(0, null, string.Format("222222 init_port 222"));
                m_port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.receive);
                Debugger.Log(0, null, string.Format("222222 init_port 333"));
            }
            catch (Exception e)
            {
                MessageBox.Show(null, "串口打开失败！错误信息：" + e.Message, "提示");
                return false;
            }

            return true;
        }

        // 接收串口数据
        private void receive(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int len = m_port.BytesToRead;
            byte[] buf = new byte[len];
            m_port.Read(buf, 0, len);

            if (true)
            {
                string str_nums = "";
                for (int n = 0; n < len; n++)
                {
                    string digit = string.Format("{0:X2}", buf[n]);
                    str_nums += digit;
                }

                m_received_data = System.Text.Encoding.UTF8.GetString(buf);

                //string msg = string.Format("222222 接收串口数据 len = {0}: {1}", len, m_received_data);
                //Debugger.Log(0, null, msg);
                //string msg = string.Format("222222 接收串口数据 len = {0}: str_nums {1}, m_received_data = {2}", len, str_nums, m_received_data);
                //Debugger.Log(0, null, msg);
            }

            m_reset_event.Set();
        }

        // 发送串口命令
        public bool send(string command)
        {
            char[] chars = command.ToCharArray();
            //byte[] data = new byte[chars.Length + 1];
            //for (int n = 0; n < chars.Length; n++)
            //    data[n] = Convert.ToByte(chars[n]);
            //data[chars.Length] = 0x0D;

            byte[] plaintextbuf = Encoding.UTF8.GetBytes(command);

            try
            {
                m_port.Write(plaintextbuf, 0, plaintextbuf.Length);
            }
            catch (Exception e)
            {
                string msg = string.Format("222222 发送串口命令“{0}”失败！错误信息: {1}", command, e.Message);
                Debugger.Log(0, null, msg);

                return false;
            }

            return true;
        }

        // 初始化光源参数
        public void load_params(string strConfigFilePath)
        {
            m_config_path = strConfigFilePath;
            
            if (File.Exists(m_config_path))
            {
                string content = File.ReadAllText(m_config_path);
                string value = "";

                string str = string.Format("串口号");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_nComIndex = Convert.ToInt32(value);

                str = string.Format("亮度");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_nBrightness = Convert.ToInt32(value);
                if (m_nBrightness < 1)
                    m_nBrightness = 1;
                if (m_nBrightness > 255)
                    m_nBrightness = 255;

                str = string.Format("点亮");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_bOn = (1 == Convert.ToInt32(value)) ? true : false;
            }
            else
            {
                File.Create(m_config_path).Close();
            }
        }

        // 保存光源参数
        public bool save_params()
        {
            if (m_config_path.Length <= 0)
                return false;

            StreamWriter writer = new StreamWriter(m_config_path, false);

            string field = string.Format("串口号");
            String str = String.Format("{0}={1}", field, m_nComIndex);
            writer.WriteLine(str);

            field = string.Format("亮度");
            str = String.Format("{0}={1}", field, m_nBrightness);
            writer.WriteLine(str);

            field = string.Format("点亮");
            str = String.Format("{0}={1}", field, (true == m_bOn) ? 1 : 0);
            writer.WriteLine(str);
            
            writer.Close();

            return true;
        }

        // 打开光源
        public bool open_light()
        {
            return change_brightness(m_nBrightness);
        }

        // 关闭光源
        public bool close_light()
        {
            return change_brightness(0);
        }

        // 调节光源
        public bool change_brightness(int nBrightness)
        {
            bool bSuccess = false;

            if (true == m_port.IsOpen)
            {
                byte[] buffer = new byte[8] { 0x40, 0x05, 0x01, 0x00, 0x1A, 0x00, 0x00, 0x00 };

                switch (m_type)
                {
                    case LIGHT_TYPE.TOP_LIGHT:
                        buffer[5] = 0x0;
                        break;
                    case LIGHT_TYPE.BOTTOM_LIGHT:
                        buffer[5] = 0x1;
                        break;
                }
                buffer[6] = (byte)nBrightness;

                int sum = 0;
                for (int n = 0; n < buffer.Length - 1; n++)
                    sum += buffer[n];
                buffer[7] = (byte)(sum & 0xFF);

                try
                {
                    m_port.Write(buffer, 0, buffer.Length);
                    bSuccess = true;
                }
                catch (Exception e)
                {
                    string msg = string.Format("222222 打开光源失败！错误信息: {1}", e.Message);
                    Debugger.Log(0, null, msg);
                    MessageBox.Show(null, msg, "提示");
                }
            }

            return bSuccess;
        }

        // 调节光源
        public bool change_brightness2(int nBrightness)
        {
            bool bSuccess = false;

            if (false == m_bOn)
                return false;

            if (true == m_port.IsOpen)
            {
                Debugger.Log(0, null, "222222 调节光源 2");
                byte[] buffer = new byte[4];
                buffer[0] = 0x43;
                buffer[2] = 0x11;
                if (0 == nBrightness)
                    buffer[2] = 0x0;

                switch (m_type)
                {
                    case LIGHT_TYPE.TOP_LIGHT:
                        buffer[1] = 0x1;
                        break;
                    case LIGHT_TYPE.BOTTOM_LIGHT:
                        buffer[1] = 0x10;
                        break;
                }
                buffer[3] = (byte)nBrightness;
                
                try
                {
                    m_port.Write(buffer, 0, buffer.Length);
                    bSuccess = true;
                }
                catch (Exception e)
                {
                    string msg = string.Format("222222 打开光源失败！错误信息: {1}", e.Message);
                    Debugger.Log(0, null, msg);
                    MessageBox.Show(null, msg, "提示");
                }
            }

            return bSuccess;
        }

        // 自动调节光源，以达到指定图像亮度
        public bool auto_adjust_brightness(CameraOps camera, int lower, int upper)
        {
            int value = m_nBrightness;
            int iter_upper = 100;
            int low = 0, high = 255;
            int step = 6;
            
            double current_brightness = camera.m_current_brightness;
            if (current_brightness < lower)
            {
                if (value < 10)
                {
                    for (int n = value; n < 10; n++)
                    {
                        parent.CBD_SendMessage("设置亮度", false, m_type, n);
                        
                        Thread.Sleep(200);
                        
                        current_brightness = camera.m_current_brightness;
                        if (current_brightness > lower)
                            break;
                    }
                }

                if (current_brightness < lower)
                {
                    for (int n = 1; n < iter_upper; n++)
                    {
                        int new_value = 0;
                        if (value < 10)
                            new_value = 10 + n * 3;
                        else
                            new_value = value + n * step;
                        if (new_value > high)
                            break;

                        parent.CBD_SendMessage("设置亮度", false, m_type, new_value);

                        Thread.Sleep(200);

                        current_brightness = camera.m_current_brightness;
                        if (current_brightness > lower)
                            break;
                    }
                }
            }
            else if (current_brightness > upper)
            {
                if (value < 10)
                {
                    for (int n = value - 1; n > 0; n--)
                    {
                        parent.CBD_SendMessage("设置亮度", false, m_type, n);

                        Thread.Sleep(200);

                        current_brightness = camera.m_current_brightness;
                        if (current_brightness < lower)
                            break;
                    }
                }

                if (current_brightness > upper)
                {
                    for (int n = 1; n < iter_upper; n++)
                    {
                        int new_value = value - n * 5;
                        if ((new_value < 0) && (n > 1))
                            break;

                        parent.CBD_SendMessage("设置亮度", false, m_type, new_value);

                        Thread.Sleep(200);

                        current_brightness = camera.m_current_brightness;
                        if (current_brightness < upper)
                            break;
                        else if (new_value <= 10)
                        {
                            for (int m = new_value - 1; m > 0; m--)
                            {
                                new_value = m;
                                if (new_value < 0)
                                    break;

                                parent.CBD_SendMessage("设置亮度", false, m_type, new_value);

                                Thread.Sleep(200);

                                current_brightness = camera.m_current_brightness;
                                if (current_brightness < upper)
                                    break;
                            }
                            break;
                        }
                    }
                }
            }
            
            return true;
        }
        
    }
}
