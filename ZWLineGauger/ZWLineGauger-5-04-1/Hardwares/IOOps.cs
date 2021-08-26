using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using gts;

namespace ZWLineGauger.Hardwares
{
    public enum   IO_STATE
    {
        NONE = -1,
        IO_LOW = 0,
        IO_HIGH
    }

    public class IOOps
    {
        MainUI   form_parent;

        public string   m_config_path = "";

        public int   m_output_beeper = 1;
        public int   m_output_vacuum = 2;
        public int   m_output_red_dot = 3;
        public int   m_output_green_light = 5;
        public int   m_output_yellow_light = 6;
        public int   m_output_red_light = 7;
        public int   m_input_vacuum = 4;
        public int   m_input_emergency = 6;
        public int   m_input_height_sensor = 1;

        public int m_input_grating = 12;//光栅输入

        //平台吸附按钮的输出
        public int m_output_vacuum_button = 14;

        //平台运行按钮输入输出
        public int m_input_start_button = 11;
        public int m_output_start_button = 15;

        // 初始化
        public   IOOps(MainUI parent, string strConfigFilePath)
        {
            this.form_parent = parent;

            m_config_path = strConfigFilePath;
        }

        // 恢复出厂设置
        public void save_default()
        {
            m_output_beeper = 1;
            m_output_vacuum = 2;
            m_output_red_dot = 3;
            m_output_green_light = 5;
            m_output_yellow_light = 6;
            m_output_red_light = 7;
            m_input_vacuum = 4;
            m_input_emergency = 6;
            m_input_height_sensor = 1;

            save_params();
        }

        // 初始化IO参数
        public void load_params()
        {
            if (File.Exists(m_config_path))
            {
                string content = File.ReadAllText(m_config_path);
                string value = "";

                string str = string.Format("蜂鸣器");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_output_beeper = Convert.ToInt32(value);

                str = string.Format("鼓风机吸附");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_output_vacuum = Convert.ToInt32(value);

                str = string.Format("红点指示器");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_output_red_dot = Convert.ToInt32(value);

                str = string.Format("绿灯");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_output_green_light = Convert.ToInt32(value);

                str = string.Format("黄灯");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_output_yellow_light = Convert.ToInt32(value);

                str = string.Format("红灯");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_output_red_light = Convert.ToInt32(value);
                
                str = string.Format("吸附按钮");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_input_vacuum = Convert.ToInt32(value);

                str = string.Format("急停按钮");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_input_emergency = Convert.ToInt32(value);

                str = string.Format("高度传感器");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_input_height_sensor = Convert.ToInt32(value);
            }
            else
            {
                File.Create(m_config_path).Close();
            }
        }

        // 保存IO参数信息
        public bool save_params()
        {
            if (m_config_path.Length <= 0)
                return false;

            StreamWriter writer = new StreamWriter(m_config_path, false);

            string field = string.Format("蜂鸣器");
            String str = String.Format("{0}={1}", field, m_output_beeper);
            writer.WriteLine(str);

            field = string.Format("鼓风机吸附");
            str = String.Format("{0}={1}", field, m_output_vacuum);
            writer.WriteLine(str);

            field = string.Format("红点指示器");
            str = String.Format("{0}={1}", field, m_output_red_dot);
            writer.WriteLine(str);

            field = string.Format("绿灯");
            str = String.Format("{0}={1}", field, m_output_green_light);
            writer.WriteLine(str);

            field = string.Format("黄灯");
            str = String.Format("{0}={1}", field, m_output_yellow_light);
            writer.WriteLine(str);

            field = string.Format("红灯");
            str = String.Format("{0}={1}", field, m_output_red_light);
            writer.WriteLine(str);

            writer.WriteLine(str);

            field = string.Format("吸附按钮");
            str = String.Format("{0}={1}", field, m_input_vacuum);
            writer.WriteLine(str);

            field = string.Format("急停按钮");
            str = String.Format("{0}={1}", field, m_input_emergency);
            writer.WriteLine(str);

            field = string.Format("高度传感器");
            str = String.Format("{0}={1}", field, m_input_height_sensor);
            writer.WriteLine(str);

            writer.Close();

            return true;
        }

        // 设置IO输出
        public bool set_IO_output(int nIONo, IO_STATE state)
        {
            short value = 8;
            switch (state)
            {
                case IO_STATE.IO_HIGH:
                    value = 1;
                    break;
                case IO_STATE.IO_LOW:
                    value = 0;
                    break;
            }

            short res = mc.GT_SetDoBit(mc.MC_GPO, (short)nIONo, value);

            if (0 != res)
                return false;
            else
                return true;
        }

        // 获取IO输出口状态
        public bool get_IO_output_state(int nIONo, ref IO_STATE state)
        {
            int value = 0;

            short res = mc.GT_GetDo(mc.MC_GPO, out value);

            if ((value & (1 << (nIONo - 1))) > 0)
                state = IO_STATE.IO_HIGH;
            else
                state = IO_STATE.IO_LOW;

            if (0 != res)
                return false;
            else
                return true;
        }

        // 获取IO输入
        public bool get_IO_input(int nIONo, ref IO_STATE state)
        {
            int value = 0;

            short res = mc.GT_GetDi(mc.MC_GPI, out value);

            value = value & (1 << (nIONo - 1));
            if (value > 0)
                state = IO_STATE.IO_HIGH;
            else
                state = IO_STATE.IO_LOW;

            if (0 != res)
                return false;
            else
                return true;
        }

        // 获取高度传感器触发状态
        public bool is_height_sensor_activated()
        {
            IO_STATE state = IO_STATE.NONE;
            get_IO_input(m_input_height_sensor, ref state);
            //Debugger.Log(0, null, string.Format("222222 高度传感器 state = {0}", state));
            if (IO_STATE.IO_HIGH == state)
                return true;
            else
                return false;
        }


    }
}
