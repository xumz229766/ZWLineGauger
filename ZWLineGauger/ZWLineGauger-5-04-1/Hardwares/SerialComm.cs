using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace ZWLineGauger.Hardwares
{
    public class SerialComm
    {
        public SerialPort m_port;

        string m_received_data = "";

        AutoResetEvent m_reset_event = new AutoResetEvent(false);

        public SerialComm(SerialPort port)
        {
            m_port = port;
        }

        public SerialComm()
        {

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

                m_port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.receive);
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
                
                string msg = string.Format("222222 接收串口数据 len = {0}: {1}", len, m_received_data);
                Debugger.Log(0, null, msg);
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
    }
}
