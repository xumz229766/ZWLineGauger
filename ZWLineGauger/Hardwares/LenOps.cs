using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ZWLineGauger.Hardwares
{
    enum LEN_COMMAND
    {
        LEN_NONE,
        LEN_GO_HOME,
        LEN_READ_INFO,
        LEN_CHANGE_RATIO,
        LEN_SET_ACC,
        LEN_SET_SPEED
    }

    public class LenOps
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder returned_value, int nSize, string path);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string value, string path);

        MainUI form_parent;

        public string   m_config_path = "";

        public int   m_nComIndex = 0;

        public SerialPort   m_port;

        string   m_received_data = "";
        byte[]   m_pLenReceivedData = new byte[256];

        public bool   m_bLenIsChangingRatio = false;
        bool   m_bLenIsGoingHome = false;
        bool   m_bLenCommandInProcess = false;
        bool   m_bLenIsReadingInfo = false;
        int   m_nLenReceivedBytesCount = 0;
        int   m_nLenPos = 0;
        int   m_nLenSpeed = 0;
        int   m_nLenAcc = 0;
        
        public int   m_nRatio = 0;

        LEN_COMMAND   m_current_len_command = LEN_COMMAND.LEN_NONE;
        
        public AutoResetEvent   m_reset_event_for_len_go_home = new AutoResetEvent(false);

        public bool   m_bHomed = false;
        public bool   m_bInitialized = false;

        public LenOps(MainUI parent, SerialPort port)
        {
            this.form_parent = parent;
            m_port = port;
        }

        // 恢复出厂设置
        public void save_default()
        {
            m_nComIndex = 0;

            save_params();
        }

        // 初始化镜头参数
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

                Debugger.Log(0, null, string.Format("222222 初始化镜头参数 m_config_path {0}, m_nComIndex = {1}", m_config_path, m_nComIndex));
            }
            else
            {
                File.Create(m_config_path).Close();
            }
        }

        // 保存镜头参数
        public bool save_params()
        {
            if (m_config_path.Length <= 0)
                return false;
            
            StreamWriter writer = new StreamWriter(m_config_path, false);

            string field = string.Format("串口号");
            String str = String.Format("{0}={1}", field, m_nComIndex);
            writer.WriteLine(str);
            
            writer.Close();

            return true;
        }

        // 镜头初始化
        public bool init()
        {
            Debugger.Log(0, null, string.Format("222222 变倍镜头初始化 aaa"));
            if (false == home())
                return false;
            m_bHomed = true;
            Debugger.Log(0, null, string.Format("222222 变倍镜头初始化 bbb"));
            m_reset_event_for_len_go_home.WaitOne();
            Debugger.Log(0, null, string.Format("222222 变倍镜头初始化 ccc"));
            set_user_mode();

            Thread.Sleep(100);

            set_acc(0x350);
            Thread.Sleep(100);
            set_speed(0x6800);
            Thread.Sleep(1000);
            
            m_bInitialized = true;

            return true;
        }

        public void release()
        {
            m_reset_event_for_len_go_home.Set();
        }

        // 初始化串口参数，strName是串口名（如“COM3”），后面
        public bool init_port(string strName, int nBaudRate, int nDataBits, Parity par, StopBits stop_bit)
        {
            m_port.PortName = strName;
            m_port.BaudRate = nBaudRate;
            m_port.DataBits = nDataBits;
            m_port.Parity = par;
            m_port.StopBits = stop_bit;

            try
            {
                m_port.Open();

                m_port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.on_receive_data);
            }
            catch (Exception e)
            {
                MessageBox.Show(null, "串口打开失败！错误信息：" + e.Message, "提示");
                return false;
            }

            return true;
        }

        // 收到数据时触发的回调函数
        private void on_receive_data(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int len = m_port.BytesToRead;
            byte[] buf = new byte[len];
            m_port.Read(buf, 0, len);

            if (true)
            {
                string str_nums = "";
                for (int n = 0; n < len; n++)
                {
                    string digit = string.Format("{0:X2}.", buf[n]);
                    str_nums += digit;
                }

                //string msg2 = string.Format("222222 len = {0}: str_nums {1}", len, str_nums);
                //Debugger.Log(0, null, msg2);
            }

            switch (m_current_len_command)
            {
                case LEN_COMMAND.LEN_GO_HOME:
                    for (int n = 0; n < len; n++)
                    {
                        int idx = m_nLenReceivedBytesCount + n;
                        if ((idx < 0) || (idx >= m_pLenReceivedData.Length))
                            ;
                        else
                            m_pLenReceivedData[idx] = buf[n];
                    }

                    m_nLenReceivedBytesCount += len;

                    if (m_nLenReceivedBytesCount >= 30)
                    {
                        string str_nums = "";
                        for (int n = 0; n < m_nLenReceivedBytesCount; n++)
                        {
                            string digit = string.Format("{0:X2}.", m_pLenReceivedData[n]);
                            str_nums += digit;
                        }

                        //string msg2 = string.Format("222222 total len = {0}: str_nums {1}", m_nLenReceivedBytesCount, str_nums);
                        //Debugger.Log(0, null, msg2);

                        if ((0xFF == m_pLenReceivedData[0]) && (0x5A == m_pLenReceivedData[1]))
                            m_nLenPos = (m_pLenReceivedData[2] << 16) + (m_pLenReceivedData[3] << 8) + m_pLenReceivedData[4];
                        if ((0xFF == m_pLenReceivedData[10]) && (0x5E == m_pLenReceivedData[11]))
                            m_nLenAcc = (m_pLenReceivedData[12] << 8) + m_pLenReceivedData[13];

                        //Debugger.Log(0, null, string.Format("222222 镜头复位完成，当前镜头位置 = {0:X}，加速度 = {1:X}", m_nLenPos, m_nLenAcc));

                        m_nLenReceivedBytesCount = 0;
                        for (int n = 0; n < m_pLenReceivedData.Length; n++)
                            m_pLenReceivedData[n] = 0;

                        m_bLenIsGoingHome = false;
                        m_bLenCommandInProcess = false;

                        m_reset_event_for_len_go_home.Set();

                        //m_nLenRatio = 1;
                        //comboBox_Len.SelectedIndex = m_nLenRatio;
                    }
                    break;

                case LEN_COMMAND.LEN_READ_INFO:
                    for (int n = 0; n < len; n++)
                    {
                        int idx = m_nLenReceivedBytesCount + n;
                        if ((idx < 0) || (idx >= m_pLenReceivedData.Length))
                            ;
                        else
                            m_pLenReceivedData[idx] = buf[n];
                    }

                    m_nLenReceivedBytesCount += len;

                    if (m_nLenReceivedBytesCount >= 25)
                    {
                        string str_nums = "";
                        for (int n = 0; n < m_nLenReceivedBytesCount; n++)
                        {
                            string digit = string.Format("{0:X2}.", m_pLenReceivedData[n]);
                            str_nums += digit;
                        }

                        //string msg2 = string.Format("222222 total len = {0}: str_nums {1}", m_nLenReceivedBytesCount, str_nums);
                        //Debugger.Log(0, null, msg2);

                        if ((0xFF == m_pLenReceivedData[0]) && (0x5A == m_pLenReceivedData[1]))
                            m_nLenPos = (m_pLenReceivedData[2] << 16) + (m_pLenReceivedData[3] << 8) + m_pLenReceivedData[4];
                        if ((0xFF == m_pLenReceivedData[10]) && (0x5E == m_pLenReceivedData[11]))
                            m_nLenAcc = (m_pLenReceivedData[12] << 8) + m_pLenReceivedData[13];

                        //Debugger.Log(0, null, string.Format("222222 镜头读信息完成，当前镜头位置 = {0:X}，加速度 = {1:X}", m_nLenPos, m_nLenAcc));

                        m_nLenReceivedBytesCount = 0;
                        for (int n = 0; n < m_pLenReceivedData.Length; n++)
                            m_pLenReceivedData[n] = 0;

                        m_nRatio = 1;
                        m_bLenIsReadingInfo = false;
                        m_bLenCommandInProcess = false;
                    }
                    break;

                case LEN_COMMAND.LEN_CHANGE_RATIO:
                    for (int n = 0; n < len; n++)
                    {
                        int idx = m_nLenReceivedBytesCount + n;
                        if ((idx < 0) || (idx >= m_pLenReceivedData.Length))
                            ;
                        else
                            m_pLenReceivedData[idx] = buf[n];
                    }

                    m_nLenReceivedBytesCount += len;

                    if (m_nLenReceivedBytesCount >= 25)
                    {
                        string str_nums = "";
                        for (int n = 0; n < m_nLenReceivedBytesCount; n++)
                        {
                            string digit = string.Format("{0:X2}.", m_pLenReceivedData[n]);
                            str_nums += digit;
                        }

                        //string msg2 = string.Format("222222 total len = {0}: str_nums {1}", m_nLenReceivedBytesCount, str_nums);
                        //Debugger.Log(0, null, msg2);

                        if ((0xFF == m_pLenReceivedData[0]) && (0x5A == m_pLenReceivedData[1]))
                            m_nLenPos = (m_pLenReceivedData[2] << 16) + (m_pLenReceivedData[3] << 8) + m_pLenReceivedData[4];

                        //Debugger.Log(0, null, string.Format("222222 镜头变倍完成，当前镜头位置 = {0:X}", m_nLenPos));

                        m_nLenReceivedBytesCount = 0;
                        for (int n = 0; n < m_pLenReceivedData.Length; n++)
                            m_pLenReceivedData[n] = 0;
                        
                        m_bLenIsChangingRatio = false;
                        m_bLenCommandInProcess = false;
                    }
                    break;
            }
        }
        
        // 镜头复位
        public bool home()
        {
            byte[] data = new byte[] { 0xFF, 0x99, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int sum = 0;
            for (int n = 1; n <= 5; n++)
                sum += data[n];
            data[6] = (byte)(sum & 0xFF);

            m_current_len_command = LEN_COMMAND.LEN_GO_HOME;
            m_bLenCommandInProcess = true;

            return send(data, 7, "home");
        }

        // 设置用户模式
        public bool set_user_mode()
        {
            byte[] data = new byte[] { 0xFF, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int sum = 0;
            for (int n = 1; n <= 5; n++)
                sum += data[n];
            data[6] = (byte)(sum & 0xFF);

            return send(data, 7, "set_user_mode");
        }

        // 设置电机加速度
        public bool set_acc(int acc)
        {
            byte b1 = (byte)(acc >> 8);
            byte b2 = (byte)(acc & 0xFF);

            byte[] data = new byte[] { 0xFF, 0x04, b1, b2, b1, b2, 0x00 };
            int sum = 0;
            for (int n = 1; n <= 5; n++)
                sum += data[n];
            data[6] = (byte)(sum & 0xFF);

            m_current_len_command = LEN_COMMAND.LEN_SET_ACC;
            
            return send(data, 7, "set_acc");
        }

        // 设置电机速度
        public bool set_speed(int speed)
        {
            byte b1 = (byte)(speed >> 8);
            byte b2 = (byte)(speed & 0xFF);

            byte[] data = new byte[] { 0xFF, 0x03, b1, b2, b1, b2, 0x00 };
            int sum = 0;
            for (int n = 1; n <= 5; n++)
                sum += data[n];
            data[6] = (byte)(sum & 0xFF);

            m_current_len_command = LEN_COMMAND.LEN_SET_SPEED;

            return send(data, 7, "set_speed");
        }

        // 读取电机数据
        public bool read_info(int speed)
        {
            byte[] data = new byte[] { 0xFF, 0x65, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int sum = 0;
            for (int n = 1; n <= 5; n++)
                sum += data[n];
            data[6] = (byte)(sum & 0xFF);

            m_current_len_command = LEN_COMMAND.LEN_READ_INFO;
            m_bLenCommandInProcess = true;
            m_bLenIsReadingInfo = true;

            return send(data, 7, "read_info");
        }
        
        // 改变倍率
        public bool set_ratio(int ratio_index)
        {
            byte[] data = new byte[] { 0xFF, (byte)(0x20 + ratio_index), 0x00, 0x00, 0x00, 0x00, 0x00 };
            int sum = 0;
            for (int n = 1; n <= 5; n++)
                sum += data[n];
            data[6] = (byte)(sum & 0xFF);

            m_bLenIsChangingRatio = true;

            m_current_len_command = LEN_COMMAND.LEN_CHANGE_RATIO;
            //m_nRatio = ratio_index;
            //Debugger.Log(0, null, "222222 改变倍率");

            return send(data, 7, "set_ratio");
        }

        // 发送数据
        public bool send(byte[] data, int len, string func_name)
        {
            try
            {
                Debugger.Log(0, null, string.Format("222222 变倍镜头初始化 eee"));
                m_port.Write(data, 0, len);
                Debugger.Log(0, null, string.Format("222222 变倍镜头初始化 fff"));
            }
            catch (Exception e)
            {
                Debugger.Log(0, null, string.Format("222222 变倍镜头函数 {0}() 失败！错误信息: {1}", func_name, e.Message));

                return false;
            }
            Debugger.Log(0, null, string.Format("222222 变倍镜头初始化 ggg"));
            return true;
        }
    }
}
