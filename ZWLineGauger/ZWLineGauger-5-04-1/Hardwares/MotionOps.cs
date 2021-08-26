using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using gts;
using static ZWLineGauger.MainUI;

namespace ZWLineGauger
{
    public struct Axis
    {
        public void init()
        {
            nDir = 1;

            motion_unit_scale = 1;

            acc_for_short_range = 0.15;
            dec_for_short_range = 0.15;
            vel_for_short_range = 10;

            acc_for_long_range = 2;
            dec_for_long_range = 2;
            vel_for_long_range = 50;

            negative_limit = 0;
            positive_limit = 0;
            home_speed = 10;
            home_speed_second = 4;
        }

        public int   nDir;
        public double motion_unit_scale;               // 运动当量
        public double acc_for_short_range;           // 短程加速度
        public double dec_for_short_range;           // 短程减速度
        public double vel_for_short_range;           // 短程工作速度
        public double acc_for_long_range;           // 长程加速度
        public double dec_for_long_range;           // 长程减速度
        public double vel_for_long_range;            // 长程工作速度
        public double negative_limit;                     // 负限位
        public double positive_limit;                       // 正限位
        public double home_speed;                       // 回零点速度
        public double home_speed_second;         // 搜索原点速度
    }

    public class MotionOps
    {
        static double LONG_SHORT_RANGE_THRESHOLD = 50;           // 大于等于此值的行程，视为长程

        MainUI form_parent;

        public const short   AXIS_X = 1;
        public const short   AXIS_Y = 2;
        public const short   AXIS_Z = 4;

        public bool   m_bHomed = false;
        public bool   m_bInitialized = false;
        public bool   m_bStopHomingThread = false;
        private bool  m_bIsMoving = false;

        public string m_config_path = "";

        public Axis[] m_axes = new Axis[4];

        public Point2d   m_pad_leftbottom_crd = new Point2d(0, 0);
        public Point2d   m_pad_righttop_crd = new Point2d(0, 0);
        public Point3d   m_PCB_leftbottom_crd = new Point3d(0, 0, 0);
        
        public double   m_autofocus_upper_pos = 0;
        public double   m_autofocus_lower_pos = 0;

        public double   m_threaded_move_dest_X = 0;
        public double   m_threaded_move_dest_Y = 0;
        public double   m_threaded_move_dest_Z = 0;

        bool   m_bHasHomedX = false;
        bool   m_bHasHomedY = false;
        bool   m_bHasHomedZ = false;

        public object m_lock = new object();

        public MotionOps(MainUI parent)
        {
            this.form_parent = parent;

            for (int n = 0; n < 4; n++)
                m_axes[n].init();
        }

        // 恢复出厂设置
        public void save_default()
        {
            for (int n = 0; n < 4; n++)
            {
                m_axes[n].acc_for_short_range = 0.15;
                m_axes[n].dec_for_short_range = 0.15;
                m_axes[n].vel_for_short_range = 50;

                m_axes[n].acc_for_long_range = 2;
                m_axes[n].dec_for_long_range = 2;
                m_axes[n].vel_for_long_range = 800;

                m_axes[n].home_speed = 40;
                m_axes[n].home_speed_second = 30;
            }

            m_axes[AXIS_Z - 1].acc_for_short_range = 0.15;
            m_axes[AXIS_Z - 1].dec_for_short_range = 0.15;
            m_axes[AXIS_Z - 1].vel_for_short_range = 10;

            m_axes[AXIS_Z - 1].acc_for_long_range = 1;
            m_axes[AXIS_Z - 1].dec_for_long_range = 1;
            m_axes[AXIS_Z - 1].vel_for_long_range = 50;

            m_axes[AXIS_Z - 1].home_speed = 12;
            m_axes[AXIS_Z - 1].home_speed_second = 6;

            save_params();
        }

        // 初始化运动系统参数
        public void load_params(string strConfigFilePath)
        {
            m_config_path = strConfigFilePath;

            if (File.Exists(m_config_path))
            {
                string content = File.ReadAllText(m_config_path);
                string value = "";

                for (int n = 0; n < 4; n++)
                {
                    string strAxis = "";
                    if ((AXIS_X - 1) == n)
                        strAxis = "X";
                    else if ((AXIS_Y - 1) == n)
                        strAxis = "Y";
                    else if ((AXIS_Z - 1) == n)
                        strAxis = "Z";
                    else
                        continue;

                    string str = string.Format("{0}轴方向", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].nDir = Convert.ToInt32(value);

                    str = string.Format("{0}轴运动当量", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].motion_unit_scale = Convert.ToDouble(value);

                    str = string.Format("{0}轴短程加速度", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].acc_for_short_range = Convert.ToDouble(value);

                    str = string.Format("{0}轴短程减速度", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].dec_for_short_range = Convert.ToDouble(value);

                    str = string.Format("{0}轴短程工作速度", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].vel_for_short_range = Convert.ToDouble(value);

                    str = string.Format("{0}轴长程加速度", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].acc_for_long_range = Convert.ToDouble(value);

                    str = string.Format("{0}轴长程减速度", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].dec_for_long_range = Convert.ToDouble(value);

                    str = string.Format("{0}轴长程工作速度", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].vel_for_long_range = Convert.ToDouble(value);

                    str = string.Format("{0}轴负限位", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].negative_limit = Convert.ToDouble(value);

                    str = string.Format("{0}轴正限位", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].positive_limit = Convert.ToDouble(value);
                    
                    str = string.Format("{0}轴回零点速度", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].home_speed = Convert.ToDouble(value);

                    str = string.Format("{0}轴搜索原点速度", strAxis);
                    if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                        m_axes[n].home_speed_second = Convert.ToDouble(value);
                }

                string str2 = string.Format("小地图左下角坐标X");
                if (true == GeneralUtils.GetKeyValue(content, str2, ref value))
                    m_pad_leftbottom_crd.x = Convert.ToDouble(value);
                str2 = string.Format("小地图左下角坐标Y");
                if (true == GeneralUtils.GetKeyValue(content, str2, ref value))
                    m_pad_leftbottom_crd.y = Convert.ToDouble(value);
                str2 = string.Format("小地图右上角坐标X");
                if (true == GeneralUtils.GetKeyValue(content, str2, ref value))
                    m_pad_righttop_crd.x = Convert.ToDouble(value);
                str2 = string.Format("小地图右上角坐标Y");
                if (true == GeneralUtils.GetKeyValue(content, str2, ref value))
                    m_pad_righttop_crd.y = Convert.ToDouble(value);

                str2 = string.Format("PCB左下角快捷到达坐标X");
                if (true == GeneralUtils.GetKeyValue(content, str2, ref value))
                    m_PCB_leftbottom_crd.x = Convert.ToDouble(value);
                str2 = string.Format("PCB左下角快捷到达坐标Y");
                if (true == GeneralUtils.GetKeyValue(content, str2, ref value))
                    m_PCB_leftbottom_crd.y = Convert.ToDouble(value);
                str2 = string.Format("PCB左下角快捷到达坐标Z");
                if (true == GeneralUtils.GetKeyValue(content, str2, ref value))
                    m_PCB_leftbottom_crd.z = Convert.ToDouble(value);

                str2 = string.Format("Z轴自动对焦上限位置");
                if (true == GeneralUtils.GetKeyValue(content, str2, ref value))
                    m_autofocus_upper_pos = Convert.ToDouble(value);
                str2 = string.Format("Z轴自动对焦下限位置");
                if (true == GeneralUtils.GetKeyValue(content, str2, ref value))
                    m_autofocus_lower_pos = Convert.ToDouble(value);
            }
            else
            {
                File.Create(m_config_path).Close();
            }
        }

        // 保存运动控制相关参数
        public bool save_params()
        {
            if (m_config_path.Length <= 0)
                return false;
            
            // 检查参数范围
            bool bIsValid = true;
            for (int n = 0; n < 4; n++)
            {
                if ((m_axes[n].acc_for_short_range <= 0) || (m_axes[n].acc_for_short_range >= 5))
                    bIsValid = false;
                if ((m_axes[n].dec_for_short_range <= 0) || (m_axes[n].dec_for_short_range >= 5))
                    bIsValid = false;
                if ((m_axes[n].vel_for_short_range <= 0) || (m_axes[n].vel_for_short_range > 1000))
                    bIsValid = false;
                //CWCCW
                if ((m_axes[n].acc_for_long_range <= 0) || (m_axes[n].acc_for_long_range >= 5))
                    bIsValid = false;
                if ((m_axes[n].dec_for_long_range <= 0) || (m_axes[n].dec_for_long_range >= 5))
                    bIsValid = false;
                if ((m_axes[n].vel_for_long_range <= 0) || (m_axes[n].vel_for_long_range > 1000))
                    bIsValid = false;

                if ((m_axes[n].home_speed <= 0) || (m_axes[n].home_speed >= 50))
                    bIsValid = false;
                if ((m_axes[n].home_speed_second <= 0) || (m_axes[n].home_speed_second >= 50))
                    bIsValid = false;
            }
            if (false == bIsValid)
                return false;
            
            StreamWriter writer = new StreamWriter(m_config_path, false);

            for (int n = 0; n < 4; n++)
            {
                string strAxis = "";
                if ((AXIS_X - 1) == n)
                    strAxis = "X";
                else if ((AXIS_Y - 1) == n)
                    strAxis = "Y";
                else if ((AXIS_Z - 1) == n)
                    strAxis = "Z";
                else
                    continue;

                string field = string.Format("{0}轴方向", strAxis);
                String str = String.Format("{0}={1}", field, m_axes[n].nDir);
                writer.WriteLine(str);

                field = string.Format("{0}轴运动当量", strAxis);
                str = String.Format("{0}={1}", field, m_axes[n].motion_unit_scale);
                writer.WriteLine(str);

                field = string.Format("{0}轴短程加速度", strAxis);
                str = String.Format("{0}={1}", field, m_axes[n].acc_for_short_range);
                writer.WriteLine(str);

                field = string.Format("{0}轴短程减速度", strAxis);
                str = String.Format("{0}={1}", field, m_axes[n].dec_for_short_range);
                writer.WriteLine(str);

                field = string.Format("{0}轴短程工作速度", strAxis);
                str = String.Format("{0}={1}", field, m_axes[n].vel_for_short_range);
                writer.WriteLine(str);

                field = string.Format("{0}轴长程加速度", strAxis);
                str = String.Format("{0}={1}", field, m_axes[n].acc_for_long_range);
                writer.WriteLine(str);

                field = string.Format("{0}轴长程减速度", strAxis);
                str = String.Format("{0}={1}", field, m_axes[n].dec_for_long_range);
                writer.WriteLine(str);

                field = string.Format("{0}轴长程工作速度", strAxis);
                str = String.Format("{0}={1}", field, m_axes[n].vel_for_long_range);
                writer.WriteLine(str);

                field = string.Format("{0}轴负限位", strAxis);
                str = String.Format("{0}={1}", field, m_axes[n].negative_limit);
                writer.WriteLine(str);

                field = string.Format("{0}轴正限位", strAxis);
                str = String.Format("{0}={1}", field, m_axes[n].positive_limit);
                writer.WriteLine(str);

                field = string.Format("{0}轴回零点速度", strAxis);
                str = String.Format("{0}={1}", field, m_axes[n].home_speed);
                writer.WriteLine(str);

                field = string.Format("{0}轴搜索原点速度", strAxis);
                str = String.Format("{0}={1}", field, m_axes[n].home_speed_second);
                writer.WriteLine(str);
                
                writer.WriteLine("");
            }

            string field2 = string.Format("小地图左下角坐标X");
            String str2 = String.Format("{0}={1}", field2, m_pad_leftbottom_crd.x);
            writer.WriteLine(str2);

            field2 = string.Format("小地图左下角坐标Y");
            str2 = String.Format("{0}={1}", field2, m_pad_leftbottom_crd.y);
            writer.WriteLine(str2);

            field2 = string.Format("小地图右上角坐标X");
            str2 = String.Format("{0}={1}", field2, m_pad_righttop_crd.x);
            writer.WriteLine(str2);

            field2 = string.Format("小地图右上角坐标Y");
            str2 = String.Format("{0}={1}", field2, m_pad_righttop_crd.y);
            writer.WriteLine(str2);

            field2 = string.Format("PCB左下角快捷到达坐标X");
            str2 = String.Format("{0}={1}", field2, m_PCB_leftbottom_crd.x);
            writer.WriteLine(str2);

            field2 = string.Format("PCB左下角快捷到达坐标Y");
            str2 = String.Format("{0}={1}", field2, m_PCB_leftbottom_crd.y);
            writer.WriteLine(str2);

            field2 = string.Format("PCB左下角快捷到达坐标Z");
            str2 = String.Format("{0}={1}", field2, m_PCB_leftbottom_crd.z);
            writer.WriteLine(str2);

            field2 = string.Format("Z轴自动对焦上限位置");
            str2 = String.Format("{0}={1}", field2, m_autofocus_upper_pos);
            writer.WriteLine(str2);

            field2 = string.Format("Z轴自动对焦下限位置");
            str2 = String.Format("{0}={1}", field2, m_autofocus_lower_pos);
            writer.WriteLine(str2);

            writer.Close();

            return true;
        }

        public bool check_axis_validness(Axis axis)
        {
            bool bIsValid = true;

            if ((axis.acc_for_short_range <= 0) || (axis.acc_for_short_range >= 5))
                bIsValid = false;
            if ((axis.dec_for_short_range <= 0) || (axis.dec_for_short_range >= 5))
                bIsValid = false;
            if ((axis.vel_for_short_range <= 0) || (axis.vel_for_short_range > 900))
                bIsValid = false;
            
            if ((axis.acc_for_long_range <= 0) || (axis.acc_for_long_range >= 5))
                bIsValid = false;
            if ((axis.dec_for_long_range <= 0) || (axis.dec_for_long_range >= 5))
                bIsValid = false;
            if ((axis.vel_for_long_range <= 0) || (axis.vel_for_long_range > 900))
                bIsValid = false;

            if ((axis.home_speed <= 0) || (axis.home_speed >= 50))
                bIsValid = false;
            if ((axis.home_speed_second <= 0) || (axis.home_speed_second >= 50))
                bIsValid = false;

            return bIsValid;
        }

        // 初始化控制卡
        public bool init()
        {
            short res = mc.GT_Open(0, 1);
            
            if (-6 == res)
            {
                int counter = 0;
                while (true)
                {
                    Thread.Sleep(50);
                    counter++;
                    if (counter >= (20 * 25))
                        break;
                    res = mc.GT_Open(0, 1);
                    if (0 == res)
                        break;
                }
            }
            if (0 != res)
                return false;

            res = mc.GT_Reset();
            if (0 != res)
                return false;
            
            res = mc.GT_LoadConfig("GTS400-ZW.cfg");
            if (0 != res)
                return false;
            
            for (short n = 1; n < 5; n++)
            {
                res = mc.GT_AlarmOff(n);
                if (0 != res)
                    return false;
                
                res = mc.GT_ClrSts(n, 8);
                if (0 != res)
                    return false;

                res = mc.GT_AxisOn(n);
                if (0 != res)
                    return false;
            }
            
            for (int n = 0; n < 100; n++)
            {
                Thread.Sleep(50);
                if (true == m_bStopHomingThread)
                    return false;
            }
            
            m_bInitialized = true;

            return true;
        }

        public void disable_all_axes()
        {
            bool bOK = true;
            for (short n = 1; n < 5; n++)
            {
                short rtn = mc.GT_AxisOff(n);
                if (0 != rtn)
                    bOK = false;
            }
        }

        public void release()
        {
            if (m_bInitialized)
            {
                stop_all_axes();
                disable_all_axes();
                mc.GT_Close();
        }
        }

        // 将指定轴置零
        bool zero_axis(short axis)
        {
            bool result = false;
            short rtn = 0;

            rtn = mc.GT_ZeroPos(axis, 1);
            if (rtn == 0)
                result = true;
            
            return result;
        }
        
        // 将指定轴回原
        bool home_axis(short axis, int nDir)
        {
            mc.TTrapPrm tHomePrm = new mc.TTrapPrm();
            
            short res = mc.GT_PrfTrap(axis);
            
            res = mc.GT_GetTrapPrm(axis, out tHomePrm);
            
            if (0 == res)
            {
                tHomePrm.acc = 0.15;
                tHomePrm.dec = 0.15;
                
                res = mc.GT_SetTrapPrm(axis, ref tHomePrm);
                
                res = mc.GT_SetVel(axis, m_axes[axis - 1].home_speed);
                
                res = mc.GT_SetPos(axis, nDir * -5000000);
                
                res = mc.GT_Update(1 << (axis - 1));

                //Debugger.Log(0, null, string.Format("222222 nDir * -5000000 {0}", nDir * -5000000));

                int status = 0;
                uint clock = 0;
                while (true)
                {
                    Thread.Sleep(10);
                    res = mc.GT_GetSts(axis, out status, 1, out clock);
                    //msg2 = string.Format("222222 轴n {0}: GT_GetSts() result = {1}, status = {2}", n, res, status & 0x400);
                    //Debugger.Log(0, null, msg2);

                    if (0 == (status & 0x400))
                        break;

                    if (true == m_bStopHomingThread)
                        break;

                    if (true == form_parent.m_bEmergencyExit)
                        return false;
                }
                //Debugger.Log(0, null, "222222 222");
                stop_axis(axis);
                
                while (true)
                {
                    Thread.Sleep(10);
                    res = mc.GT_GetSts(axis, out status, 1, out clock);
                    if (0 == (status & 0x400))
                        break;
                    if (true == m_bStopHomingThread)
                        break;
                    if (true == form_parent.m_bEmergencyExit)
                        return false;
                }
                
                for (int n = 0; n < 10; n++)
                {
                    Thread.Sleep(20);
                    if (true == m_bStopHomingThread)
                        break;
                }
                
                res = mc.GT_ClrSts(axis, 1);
                res = mc.GT_SetCaptureMode(axis, mc.CAPTURE_HOME);
                res = mc.GT_SetVel(axis, m_axes[axis - 1].home_speed_second);
                
                if (AXIS_Z == axis)
                    res = mc.GT_SetPos(axis, nDir * 30000);
                else
                    res = mc.GT_SetPos(axis, nDir * 5000000);

                res = mc.GT_Update(1 << (axis - 1));
                //Debugger.Log(0, null, "222222 333");
                Thread.Sleep(100);
                
                // 等待捕获触发
                int pos = 0;
                short capture = 0;
                double prfPos = 0;
                double encPos = 0;
                while (true)
                {
                    res = mc.GT_GetSts(axis, out status, 1, out clock);
                    if (0 == (status & 0x400))
                        break;

                    res = mc.GT_GetCaptureStatus(axis, out capture, out pos, 1, out clock);

                    // 读取规划位置
                    res = mc.GT_GetPrfPos(axis, out prfPos, 1, out clock);

                    // 读取编码器位置
                    res = mc.GT_GetEncPos(axis, out encPos, 1, out clock);

                    //msg2 = string.Format("222222 轴n {0}: capture = {1}, prfPos = {2:0.000}, encPos = {3:0.000}, pos = {4}", axis, capture, prfPos, encPos, pos);
                    //Debugger.Log(0, null, msg2);
                    
                    if (0 != capture)
                        break;

                    if (true == m_bStopHomingThread)
                        break;
                    if (true == form_parent.m_bEmergencyExit)
                        return false;
                }
                
                // 显示捕获位置
                //msg2 = string.Format("222222 轴n {0}: 捕获位置 = {1}", axis, pos);
                //Debugger.Log(0, null, msg2);

                stop_axis(axis);
                for (int n = 0; n < 10; n++)
                {
                    Thread.Sleep(10);
                    if (true == m_bStopHomingThread)
                        break;
                }
                
                //res = mc.GT_ClrSts(axis, 1);
                //res = mc.GT_SetPos(axis, pos);
                //res = mc.GT_Update(1 << (axis - 1));

                //while (true)
                //{
                //    Thread.Sleep(10);
                //    res = mc.GT_GetSts(axis, out status, 1, out clock);
                //    if (0 == (status & 0x400))
                //        break;
                //}
                //Debugger.Log(0, null, "222222 555");
                Thread.Sleep(10);
                zero_axis(axis);
                mc.GT_ClrSts(axis, 1);
                
                // 读取编码器位置
                res = mc.GT_GetEncPos(axis, out encPos, 1, out clock);

                res = mc.GT_GetPrfPos(axis, out prfPos, 1, out clock);
                //msg2 = string.Format("222222 轴n {0}: prfPos = {1}, encPos = {2}", axis, prfPos, encPos);
                //Debugger.Log(0, null, msg2);
            }

            return true;
        }
        
        // 停止指定轴
        public bool stop_axis(short axis)
        {
            bool result = false;
            short rtn = 0;

            rtn = mc.GT_Stop(1 << (axis - 1), 0);              // 平滑停止
            if (rtn == 0)
                result = true;
            
            return result;
        }

        // 停止所有轴
        public bool stop_all_axes()
        {
            bool bOK = true;
            for (short n = 1; n < 5; n++)
            {
                if (false == stop_axis(n))
                    bOK = false;
            }
            
            return bOK;
        }

        void thread_home_X(object obj)
        {
            home_axis(AXIS_X, m_axes[AXIS_X - 1].nDir * -1);
            m_bHasHomedX = true;
        }

        void thread_home_Y(object obj)
        {
            home_axis(AXIS_Y, m_axes[AXIS_Y - 1].nDir);
            m_bHasHomedY = true;
        }

        void thread_home_Z(object obj)
        {
            home_axis(AXIS_Z, m_axes[AXIS_Z - 1].nDir);
            m_bHasHomedZ = true;
        }

        // 回原点
        public bool do_home()
        {
            //Thread.Sleep(1000);
            //return true;
            Debugger.Log(0, null, string.Format("222222 do_home()"));
            if (false == m_bInitialized)
                return false;

            if (true == m_bIsMoving)
                return false;
            lock (m_lock)
            {
                m_bIsMoving = true;
            }

            string msg2;
            short res;
            
            m_bHasHomedX = false;
            m_bHasHomedY = false;
            m_bHasHomedZ = false;
            //Debugger.Log(0, null, string.Format("222222 thread_home_Z()"));
            (new Thread(thread_home_Z)).Start();

            for (int n = 0; n < 10; n++)
            {
                if (true == m_bStopHomingThread)
                    break;
                Thread.Sleep(50);
            }

            if (false == m_bStopHomingThread)
                (new Thread(thread_home_X)).Start();
            if (false == m_bStopHomingThread)
                (new Thread(thread_home_Y)).Start();
            
            // 限时一分钟
            for (int n = 0; n < 6000; n++)
            {
                Thread.Sleep(10);
                if (m_bHasHomedX && m_bHasHomedY && m_bHasHomedZ)
                    break;
                if (true == m_bStopHomingThread)
                    break;
            }
            
            double prfPos = 0;
            uint clock = 0;
            
            for (short nAxis = 1; nAxis < 5; nAxis++)
                mc.GT_ClrSts(nAxis, 1);

            m_bHomed = true;
            
            lock (m_lock)
            {
                m_bIsMoving = false;
            }

            if (true == m_bStopHomingThread)
                m_bHomed = false;

            return true;
        }

        // 获取XYZ当前坐标
        public bool get_xyz_crds(ref Point3d crd)
        {
            if (false == m_bHomed)
                return false;

            double prfPos = 0;
            uint clock = 0;

            short res = mc.GT_GetPrfPos(AXIS_X, out prfPos, 1, out clock);
            crd.x = (prfPos / m_axes[AXIS_X - 1].motion_unit_scale) / 1000;

            res = mc.GT_GetPrfPos(AXIS_Y, out prfPos, 1, out clock);
            crd.y = (prfPos / m_axes[AXIS_Y - 1].motion_unit_scale) / 1000;

            res = mc.GT_GetPrfPos(AXIS_Z, out prfPos, 1, out clock);
            crd.z = (prfPos / m_axes[AXIS_Z - 1].motion_unit_scale) / 1000;

            return true;
        }

        // 获取轴位置
        public bool get_axis_pos(short axis, ref double pos)
        {
            double prfPos = 0;
            uint clock = 0;

            short res = mc.GT_GetPrfPos(axis, out prfPos, 1, out clock);
            pos = (prfPos / m_axes[axis - 1].motion_unit_scale) / 1000;

            return true;
        }

        // 判断轴是否停止移动
        public bool is_axis_stop(short axis)
        {
            int status = 0;
            uint clock = 0;

            short res = mc.GT_GetSts(axis, out status, 1, out clock);
            if (0 == (status & 0x400))
                return true;
            if (0 != (status & 0x800))
                return true;

            return false;
        }

        // 点位运动，等待完成才返回，单位mm，axis数值范围 1~3
        public bool pt_move_axis_wait_until_stop(short axis, double pos, double vel)
        {
            if (false == GeneralUtils.between_two(pos, m_axes[axis - 1].negative_limit, m_axes[axis - 1].positive_limit))
                return false;

            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return false;
            }

            if (true == m_bIsMoving)
                return false;
            
            lock (m_lock)
            {
                m_bIsMoving = true;
            }
            
            short res = mc.GT_ClrSts(axis, 1);

            mc.TTrapPrm trap;

            res = mc.GT_PrfTrap(axis);
            res = mc.GT_GetTrapPrm(axis, out trap);

            trap.acc = 0.25;
            trap.dec = 0.25;
            trap.smoothTime = 25;

            res = mc.GT_SetTrapPrm(axis, ref trap);
            res = mc.GT_SetVel(axis, vel);
            res = mc.GT_SetPos(axis, (int)(pos * 1000 * m_axes[axis - 1].motion_unit_scale));
            res = mc.GT_Update(1 << (axis - 1));

            int status = 0;
            uint clock = 0;
            while (true)
            {
                Thread.Sleep(10);
                res = mc.GT_GetSts(axis, out status, 1, out clock);
                if (0 == (status & 0x400))
                    break;

                if (true == form_parent.m_bEmergencyExit)
                {
                    stop_all_axes();
                    return false;
                }
            }

            lock (m_lock)
            {
                m_bIsMoving = false;
            }

            return true;
        }

        // 点位运动，等待完成或距离目的地指定范围之内（距离阈值proximity，单位mm）才返回，单位mm，axis数值范围 1~3
        public bool pt_move_axis_wait_until_reach_proximity(short axis, double pos, double vel, double proximity)
        {
            if (false == GeneralUtils.between_two(pos, m_axes[axis - 1].negative_limit, m_axes[axis - 1].positive_limit))
                return false;

            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return false;
            }
            
            if (true == m_bIsMoving)
                return false;

            lock (m_lock)
            {
                m_bIsMoving = true;
            }

            short res = mc.GT_ClrSts(axis, 1);

            mc.TTrapPrm trap;

            res = mc.GT_PrfTrap(axis);
            res = mc.GT_GetTrapPrm(axis, out trap);

            trap.acc = 0.25;
            trap.dec = 0.25;
            trap.smoothTime = 25;

            double dest = pos * 1000 * m_axes[axis - 1].motion_unit_scale;

            res = mc.GT_SetTrapPrm(axis, ref trap);
            res = mc.GT_SetVel(axis, vel);
            res = mc.GT_SetPos(axis, (int)dest);
            res = mc.GT_Update(1 << (axis - 1));

            int status = 0;
            uint clock = 0;
            while (true)
            {
                Thread.Sleep(10);
                res = mc.GT_GetSts(axis, out status, 1, out clock);
                if (0 == (status & 0x400))
                    break;

                double dbProfilePos = 0;
                get_axis_pos(axis, ref dbProfilePos);
                if (Math.Abs(dbProfilePos - dest) <= proximity)
                    break;

                if (true == form_parent.m_bEmergencyExit)
                {
                    stop_all_axes();
                    return false;
                }
            }

            lock (m_lock)
            {
                m_bIsMoving = false;
            }

            return true;
        }

        // 点位运动，不等待，立即返回，单位mm，axis数值范围 1~3
        public bool pt_move_axis_no_wait(short axis, double pos, double vel)
        {
            if (false == GeneralUtils.between_two(pos, m_axes[axis - 1].negative_limit, m_axes[axis - 1].positive_limit))
                return false;

            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return false;
            }

            if (true == m_bIsMoving)
                return false;
            lock (m_lock)
            {
                m_bIsMoving = true;
            }

            short res = mc.GT_ClrSts(axis, 1);

            mc.TTrapPrm trap;

            res = mc.GT_PrfTrap(axis);
            res = mc.GT_GetTrapPrm(axis, out trap);

            trap.acc = 0.25;
            trap.dec = 0.25;
            trap.smoothTime = 25;

            res = mc.GT_SetTrapPrm(axis, ref trap);
            res = mc.GT_SetVel(axis, vel);
            res = mc.GT_SetPos(axis, (int)(pos * 1000 * m_axes[axis - 1].motion_unit_scale));
            res = mc.GT_Update(1 << (axis - 1));

            lock (m_lock)
            {
                m_bIsMoving = false;
            }
            return true;
        }
        
        // 点位运动，不等待，立即返回，单位mm，axis数值范围 1~3
        public bool pt_move_axis_no_wait(short axis, double pos, double vel, double acc, double dec)
        {
            if (false == GeneralUtils.between_two(pos, m_axes[axis - 1].negative_limit, m_axes[axis - 1].positive_limit))
                return false;

            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return false;
            }

            if (true == m_bIsMoving)
                return false;
            lock (m_lock)
            {
                m_bIsMoving = true;
            }

            short res = mc.GT_ClrSts(axis, 1);

            mc.TTrapPrm trap;

            res = mc.GT_PrfTrap(axis);
            res = mc.GT_GetTrapPrm(axis, out trap);

            trap.acc = acc;
            trap.dec = dec;
            trap.smoothTime = 25;

            res = mc.GT_SetTrapPrm(axis, ref trap);
            res = mc.GT_SetVel(axis, vel);
            res = mc.GT_SetPos(axis, (int)(pos * 1000 * m_axes[axis - 1].motion_unit_scale));
            res = mc.GT_Update(1 << (axis - 1));

            lock (m_lock)
            {
                m_bIsMoving = false;
            }
            return true;
        }

        // 二轴插补（等待完成才返回)，单位mm
        public bool linear_XY_wait_until_stop(double posX, double posY, double vel, double acc, bool bIgnoreXYLimit)
        {
            if (false == GeneralUtils.between_two(posX, m_axes[AXIS_X - 1].negative_limit, m_axes[AXIS_X - 1].positive_limit))
                return false;
            if (false == GeneralUtils.between_two(posY, m_axes[AXIS_Y - 1].negative_limit, m_axes[AXIS_Y - 1].positive_limit))
                return false;

            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return false;
            }

            if (false == m_bHomed)
                return false;

            if (true == m_bIsMoving)
                return false;
            lock (m_lock)
            {
                m_bIsMoving = true;
            }

            // 建立坐标系
            mc.TCrdPrm crdPrm = new mc.TCrdPrm();
            crdPrm.dimension = 2;                // 坐标系为二维坐标系
            crdPrm.synVelMax = vel;          // 最大合成速度： 500pulse/ms
            crdPrm.synAccMax = acc;          // 最大加速度： 1pulse/ms^2
            crdPrm.evenTime = 50;                // 最小匀速时间： 50ms
            crdPrm.profile1 = 1;                   // 规划器1对应到X轴
            crdPrm.profile2 = 2;                   // 规划器2对应到Y轴
            crdPrm.setOriginFlag = 1;            // 表示需要指定坐标系的原点坐标的规划位置
            crdPrm.originPos1 = 0;             // 坐标系的原点坐标的规划位置为 [0, 0]
            crdPrm.originPos2 = 0;
            //Debugger.Log(0, null, "222222 111");

            // 设置和清空坐标系
            short result = mc.GT_SetCrdPrm(1, ref crdPrm);
            //string msg2 = string.Format("222222 result = {0}", result);
            //Debugger.Log(0, null, msg2);

            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 222");

            result = mc.GT_CrdClear(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 333");

            // 启动运动
            int x = (int)(posX * 1000 * m_axes[AXIS_X - 1].motion_unit_scale);
            int y = (int)(posY * 1000 * m_axes[AXIS_Y - 1].motion_unit_scale);
            result = mc.GT_LnXY(1, x, y, vel, acc, 0, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 bbb");

            result = mc.GT_CrdStart(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }

            // 等待完成
            bool bX_done = false;
            bool bY_done = false;
            int state = 0;
            int counter = 0;
            int interval = 20;
            uint clock = 0;
            while (true)
            {
                if (true == form_parent.m_bEmergencyExit)
                {
                    stop_all_axes();
                    return false;
                }

                double dbCurrentX = 0;
                double dbCurrentY = 0;
                for (short nAxis = 1; nAxis < 5; nAxis++)
                {
                    // 检查状态位
                    if (true)
                    {
                        result = mc.GT_GetSts(nAxis, out state, 1, out clock);
                        if (0 != result) break;
                        if (0 == (state & 0x400))
                        {
                            if (AXIS_X == nAxis)
                                bX_done = true;
                            else if (AXIS_Y == nAxis)
                                bY_done = true;
                        }
                    }
                    
                    // 检查是否已到快结束的位置
                    double dbPrfPos = 0;
                    if (true)
                    {
                        result = mc.GT_GetPrfPos(nAxis, out dbPrfPos, 1, out clock);
                        if (0 != result) break;
                        if (AXIS_X == nAxis)
                            dbCurrentX = dbPrfPos;
                        else if (AXIS_Y == nAxis)
                            dbCurrentY = dbPrfPos;
                    }
                }


                if (bX_done && bY_done)
                    break;

                Thread.Sleep(interval);

                counter++;
                if (counter > (8 * (1000 / interval)))
                    break;
            }
            //Debugger.Log(0, null, "222222 ccc");

            for (short nAxis = 1; nAxis < 5; nAxis++)
                mc.GT_ClrSts(nAxis, 1);

            lock (m_lock)
            {
                m_bIsMoving = false;
            }
            return true;
        }

        // 二轴插补（等待完成才返回)，单位mm
        public bool linear_XY_no_wait(double posX, double posY, double vel, double acc, bool bIgnoreXYLimit)
        {
            if (false == GeneralUtils.between_two(posX, m_axes[AXIS_X - 1].negative_limit, m_axes[AXIS_X - 1].positive_limit))
                return false;
            if (false == GeneralUtils.between_two(posY, m_axes[AXIS_Y - 1].negative_limit, m_axes[AXIS_Y - 1].positive_limit))
                return false;

            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return false;
            }

            if (false == m_bHomed)
                return false;

            if (true == m_bIsMoving)
                return false;
            lock (m_lock)
            {
                m_bIsMoving = true;
            }

            // 建立坐标系
            mc.TCrdPrm crdPrm = new mc.TCrdPrm();
            crdPrm.dimension = 2;                // 坐标系为二维坐标系
            crdPrm.synVelMax = vel;          // 最大合成速度： 500pulse/ms
            crdPrm.synAccMax = acc;          // 最大加速度： 1pulse/ms^2
            crdPrm.evenTime = 50;                // 最小匀速时间： 50ms
            crdPrm.profile1 = 1;                   // 规划器1对应到X轴
            crdPrm.profile2 = 2;                   // 规划器2对应到Y轴
            crdPrm.setOriginFlag = 1;            // 表示需要指定坐标系的原点坐标的规划位置
            crdPrm.originPos1 = 0;             // 坐标系的原点坐标的规划位置为 [0, 0]
            crdPrm.originPos2 = 0;
            //Debugger.Log(0, null, "222222 111");

            // 设置和清空坐标系
            short result = mc.GT_SetCrdPrm(1, ref crdPrm);
            //string msg2 = string.Format("222222 result = {0}", result);
            //Debugger.Log(0, null, msg2);

            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 222");

            result = mc.GT_CrdClear(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 333");

            // 启动运动
            int x = (int)(posX * 1000 * m_axes[AXIS_X - 1].motion_unit_scale);
            int y = (int)(posY * 1000 * m_axes[AXIS_Y - 1].motion_unit_scale);
            result = mc.GT_LnXY(1, x, y, vel, acc, 0, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 bbb");

            result = mc.GT_CrdStart(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }

            
            lock (m_lock)
            {
                m_bIsMoving = false;
            }
            return true;
        }
        
        // 三轴插补（等待完成才返回)，单位mm
        public bool linear_XYZ_wait_until_stop(double posX, double posY, double posZ, double vel, double acc, bool bIgnoreXYLimit)
        {
            if (false == GeneralUtils.between_two(posX, m_axes[AXIS_X - 1].negative_limit, m_axes[AXIS_X - 1].positive_limit))
                return false;
            if (false == GeneralUtils.between_two(posY, m_axes[AXIS_Y - 1].negative_limit, m_axes[AXIS_Y - 1].positive_limit))
                return false;
            if (false == GeneralUtils.between_two(posZ, m_axes[AXIS_Z - 1].negative_limit, m_axes[AXIS_Z - 1].positive_limit))
                return false;

            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return false;
            }

            if (false == m_bHomed)
                return false;

            if (true == m_bIsMoving)
                return false;
            lock (m_lock)
            {
                m_bIsMoving = true;
            }

            // 建立坐标系
            mc.TCrdPrm crdPrm = new mc.TCrdPrm();
            crdPrm.dimension = 3;                // 坐标系为二维坐标系
            crdPrm.synVelMax = vel;          // 最大合成速度： 500pulse/ms
            crdPrm.synAccMax = acc;          // 最大加速度： 1pulse/ms^2
            crdPrm.evenTime = 50;                // 最小匀速时间： 50ms
            crdPrm.profile1 = AXIS_X;                   // 规划器1对应到X轴
            crdPrm.profile2 = AXIS_Y;                   // 规划器2对应到Y轴
            if (4 == AXIS_Z)
                crdPrm.profile4 = 3;                   // 规划器3对应到Z轴
            else
                crdPrm.profile3 = 3;
            crdPrm.setOriginFlag = 1;            // 表示需要指定坐标系的原点坐标的规划位置
            crdPrm.originPos1 = 0;             // 坐标系的原点坐标的规划位置为 [0, 0]
            crdPrm.originPos2 = 0;
            crdPrm.originPos3 = 0;
            //Debugger.Log(0, null, "222222 111");

            // 设置和清空坐标系
            short result = mc.GT_SetCrdPrm(1, ref crdPrm);
            //string msg2 = string.Format("222222 result = {0}", result);
            //Debugger.Log(0, null, msg2);

            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 222");

            result = mc.GT_CrdClear(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 333");

            // 启动运动
            int x = (int)(posX * 1000 * m_axes[AXIS_X - 1].motion_unit_scale);
            int y = (int)(posY * 1000 * m_axes[AXIS_Y - 1].motion_unit_scale);
            int z = (int)(posZ * 1000 * m_axes[AXIS_Z - 1].motion_unit_scale);
            result = mc.GT_LnXYZ(1, x, y, z, vel, acc, 0, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 bbb");

            result = mc.GT_CrdStart(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }

            // 等待完成
            bool bX_done = false;
            bool bY_done = false;
            bool bZ_done = false;
            int state = 0;
            int counter = 0;
            int interval = 20;
            uint clock = 0;
            while (true)
            {
                if (true == form_parent.m_bEmergencyExit)
                {
                    stop_all_axes();
                    return false;
                }

                double dbCurrentX = 0;
                double dbCurrentY = 0;
                double dbCurrentZ = 0;
                for (short nAxis = 1; nAxis < 5; nAxis++)
                {
                    // 检查状态位
                    if (true)
                    {
                        result = mc.GT_GetSts(nAxis, out state, 1, out clock);
                        if (0 != result) break;
                        if (0 == (state & 0x400))
                        {
                            if (AXIS_X == nAxis)
                                bX_done = true;
                            else if (AXIS_Y == nAxis)
                                bY_done = true;
                            else if (AXIS_Z == nAxis)
                                bZ_done = true;
                        }
                    }

                    // 检查是否已到快结束的位置
                    double dbPrfPos = 0;
                    if (true)
                    {
                        result = mc.GT_GetPrfPos(nAxis, out dbPrfPos, 1, out clock);
                        if (0 != result) break;
                        if (AXIS_X == nAxis)
                            dbCurrentX = dbPrfPos;
                        else if (AXIS_Y == nAxis)
                            dbCurrentY = dbPrfPos;
                        else if (AXIS_Z == nAxis)
                            dbCurrentZ = dbPrfPos;
                    }
                }


                if (bX_done && bY_done && bZ_done)
                    break;

                Thread.Sleep(interval);

                counter++;
                if (counter > (8 * (1000 / interval)))
                    break;
            }
            //Debugger.Log(0, null, "222222 ccc");

            for (short nAxis = 1; nAxis < 5; nAxis++)
                mc.GT_ClrSts(nAxis, 1);

            lock (m_lock)
            {
                m_bIsMoving = false;
            }
            return true;
        }

        // 三轴插补（等待完成才返回)，单位mm
        public bool linear_XYZ_wait_until_stop(double posX, double posY, double posZ, bool bIgnoreXYLimit)
        {
            //Debugger.Log(0, null, string.Format("222222 linear_XYZ_wait_until_stop = [{0:0.000},{1:0.000},{2:0.000}]", posX, posY, posZ));

            if (false == GeneralUtils.between_two(posX, m_axes[AXIS_X - 1].negative_limit, m_axes[AXIS_X - 1].positive_limit))
                return false;
            if (false == GeneralUtils.between_two(posY, m_axes[AXIS_Y - 1].negative_limit, m_axes[AXIS_Y - 1].positive_limit))
                return false;
            if (false == GeneralUtils.between_two(posZ, m_axes[AXIS_Z - 1].negative_limit, m_axes[AXIS_Z - 1].positive_limit))
                return false;

            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return false;
            }

            if (false == m_bHomed)
                return false;

            if (true == m_bIsMoving)
                return false;
            lock (m_lock)
            {
                m_bIsMoving = true;
            }

            bool bIsLongRange = false;
            double vel = m_axes[AXIS_X - 1].vel_for_short_range;
            double acc = m_axes[AXIS_X - 1].acc_for_short_range;
            double distance = Math.Sqrt((posX - form_parent.m_current_xyz.x) * (posX - form_parent.m_current_xyz.x)
                + (posY - form_parent.m_current_xyz.y) * (posY - form_parent.m_current_xyz.y));
            if (distance >= LONG_SHORT_RANGE_THRESHOLD)
            {
                bIsLongRange = true;
                vel = m_axes[AXIS_X - 1].vel_for_long_range;
                acc = m_axes[AXIS_X - 1].acc_for_long_range;
            }
            else
            {
                vel = distance * 3.8;
                acc = m_axes[AXIS_X - 1].acc_for_short_range * vel / 10;
                if (acc < m_axes[AXIS_X - 1].acc_for_short_range)
                    acc = m_axes[AXIS_X - 1].acc_for_short_range;
                if (acc > m_axes[AXIS_X - 1].acc_for_long_range)
                    acc = m_axes[AXIS_X - 1].acc_for_long_range;
            }

            //Debugger.Log(0, null, string.Format("222222 vel = {0}, acc = {1}", vel, acc));

            // 建立坐标系
            mc.TCrdPrm crdPrm = new mc.TCrdPrm();
            crdPrm.dimension = 3;                // 坐标系为二维坐标系
            crdPrm.synVelMax = vel;          // 最大合成速度： 500pulse/ms
            crdPrm.synAccMax = acc;          // 最大加速度： 1pulse/ms^2
            crdPrm.evenTime = 50;                // 最小匀速时间： 50ms
            crdPrm.profile1 = AXIS_X;                   // 规划器1对应到X轴
            crdPrm.profile2 = AXIS_Y;                   // 规划器2对应到Y轴
            if (4 == AXIS_Z)
                crdPrm.profile4 = 3;                   // 规划器3对应到Z轴
            else
                crdPrm.profile3 = 3;
            crdPrm.setOriginFlag = 1;            // 表示需要指定坐标系的原点坐标的规划位置
            crdPrm.originPos1 = 0;             // 坐标系的原点坐标的规划位置为 [0, 0]
            crdPrm.originPos2 = 0;
            crdPrm.originPos3 = 0;
            //Debugger.Log(0, null, "222222 111");

            // 设置和清空坐标系
            short result = mc.GT_SetCrdPrm(1, ref crdPrm);
            //string msg2 = string.Format("222222 result = {0}", result);
            //Debugger.Log(0, null, msg2);

            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 222");

            result = mc.GT_CrdClear(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 333");

            // 启动运动
            int x = (int)(posX * 1000 * m_axes[AXIS_X - 1].motion_unit_scale);
            int y = (int)(posY * 1000 * m_axes[AXIS_Y - 1].motion_unit_scale);
            int z = (int)(posZ * 1000 * m_axes[AXIS_Z - 1].motion_unit_scale);
            result = mc.GT_LnXYZ(1, x, y, z, vel, acc, 0, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 bbb");

            result = mc.GT_CrdStart(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }

            // 等待完成
            bool bX_done = false;
            bool bY_done = false;
            bool bZ_done = false;
            int state = 0;
            int counter = 0;
            int interval = 20;
            uint clock = 0;
            while (true)
            {
                if (true == form_parent.m_bEmergencyExit)
                {
                    stop_all_axes();
                    return false;
                }

                double dbCurrentX = 0;
                double dbCurrentY = 0;
                double dbCurrentZ = 0;
                for (short nAxis = 1; nAxis < 5; nAxis++)
                {
                    // 检查状态位
                    if (true)
                    {
                        result = mc.GT_GetSts(nAxis, out state, 1, out clock);
                        if (0 != result) break;
                        if (0 == (state & 0x400))
                        {
                            if (AXIS_X == nAxis)
                                bX_done = true;
                            else if (AXIS_Y == nAxis)
                                bY_done = true;
                            else if (AXIS_Z == nAxis)
                                bZ_done = true;
                        }
                    }

                    // 检查是否已到快结束的位置
                    double dbPrfPos = 0;
                    if (true)
                    {
                        result = mc.GT_GetPrfPos(nAxis, out dbPrfPos, 1, out clock);
                        if (0 != result) break;
                        if (AXIS_X == nAxis)
                            dbCurrentX = dbPrfPos;
                        else if (AXIS_Y == nAxis)
                            dbCurrentY = dbPrfPos;
                        else if (AXIS_Z == nAxis)
                            dbCurrentZ = dbPrfPos;
                    }
                }


                if (bX_done && bY_done && bZ_done)
                    break;

                Thread.Sleep(interval);

                counter++;
                if (counter > (8 * (1000 / interval)))
                    break;
            }
            //Debugger.Log(0, null, "222222 ccc");

            for (short nAxis = 1; nAxis < 5; nAxis++)
                mc.GT_ClrSts(nAxis, 1);

            lock (m_lock)
            {
                m_bIsMoving = false;
            }
            return true;
        }

        // 三轴插补（等待完成才返回)，单位mm
        public void threaded_linear_XYZ_wait_until_stop(object obj)
        {
            double posX = m_threaded_move_dest_X;
            double posY = m_threaded_move_dest_Y;
            double posZ = m_threaded_move_dest_Z;
            bool bIgnoreXYLimit = false;
            
            if (false == GeneralUtils.between_two(posX, m_axes[AXIS_X - 1].negative_limit, m_axes[AXIS_X - 1].positive_limit))
                return;
            if (false == GeneralUtils.between_two(posY, m_axes[AXIS_Y - 1].negative_limit, m_axes[AXIS_Y - 1].positive_limit))
                return;
            if (false == GeneralUtils.between_two(posZ, m_axes[AXIS_Z - 1].negative_limit, m_axes[AXIS_Z - 1].positive_limit))
                return;
            
            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return;
            }
            
            if (false == m_bHomed)
                return;
            
            if (true == m_bIsMoving)
                return;
            lock (m_lock)
            {
                m_bIsMoving = true;
            }
            
            dl_message_sender send_message = obj as dl_message_sender;

            bool bIsLongRange = false;
            double vel = m_axes[AXIS_X - 1].vel_for_short_range;
            double acc = m_axes[AXIS_X - 1].acc_for_short_range;
            double distance = Math.Sqrt((posX - form_parent.m_current_xyz.x) * (posX - form_parent.m_current_xyz.x)
                + (posY - form_parent.m_current_xyz.y) * (posY - form_parent.m_current_xyz.y));
            if (distance >= LONG_SHORT_RANGE_THRESHOLD)
            {
                bIsLongRange = true;
                vel = m_axes[AXIS_X - 1].vel_for_long_range;
                acc = m_axes[AXIS_X - 1].acc_for_long_range;
            }

            // 建立坐标系
            mc.TCrdPrm crdPrm = new mc.TCrdPrm();
            crdPrm.dimension = 3;                 // 坐标系为二维坐标系
            crdPrm.synVelMax = vel;            // 最大合成速度： 500pulse/ms
            crdPrm.synAccMax = acc;           // 最大加速度： 1pulse/ms^2
            crdPrm.evenTime = 50;                // 最小匀速时间： 50ms
            crdPrm.profile1 = 1;                   // 规划器1对应到X轴
            crdPrm.profile2 = 2;                   // 规划器2对应到Y轴
            if (4 == AXIS_Z)
                crdPrm.profile4 = 3;                   // 规划器3对应到Z轴
            else
                crdPrm.profile3 = 3;
            crdPrm.setOriginFlag = 1;            // 表示需要指定坐标系的原点坐标的规划位置
            crdPrm.originPos1 = 0;                 // 坐标系的原点坐标的规划位置为 [0, 0]
            crdPrm.originPos2 = 0;
            crdPrm.originPos3 = 0;
            crdPrm.originPos4 = 0;
            //Debugger.Log(0, null, "222222 111");

            // 设置和清空坐标系
            short result = mc.GT_SetCrdPrm(1, ref crdPrm);
            //string msg2 = string.Format("222222 result = {0}", result);
            //Debugger.Log(0, null, msg2);
            //Debugger.Log(0, null, string.Format("222222 到PCB左下角 777, result = {0}", result));

            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return;
            }
            //Debugger.Log(0, null, "222222 222");
            
            result = mc.GT_CrdClear(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return;
            }
            //Debugger.Log(0, null, "222222 333");
            
            // 启动运动
            int x = (int)(posX * 1000 * m_axes[AXIS_X - 1].motion_unit_scale);
            int y = (int)(posY * 1000 * m_axes[AXIS_Y - 1].motion_unit_scale);
            int z = (int)(posZ * 1000 * m_axes[AXIS_Z - 1].motion_unit_scale);
            //Debugger.Log(0, null, string.Format("222222 到PCB左下角 999, z = {0}", z));

            result = mc.GT_LnXYZ(1, x, y, z, vel, acc, 0, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return;
            }
            //Debugger.Log(0, null, "222222 bbb");
            //Debugger.Log(0, null, "222222 到PCB左下角 aaa");

            result = mc.GT_CrdStart(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return;
            }
            
            // 等待完成
            bool bX_done = false;
            bool bY_done = false;
            bool bZ_done = false;
            int state = 0;
            int counter = 0;
            int interval = 20;
            uint clock = 0;
            while (true)
            {
                if (true == form_parent.m_bEmergencyExit)
                {
                    stop_all_axes();
                    lock (m_lock)
                    {
                        m_bIsMoving = false;
                    }
                    return;
                }
                //Debugger.Log(0, null, string.Format("222222 bX_done {0}, {1}, {2}", bX_done, bY_done, bZ_done));

                double dbCurrentX = 0;
                double dbCurrentY = 0;
                double dbCurrentZ = 0;
                for (short nAxis = 1; nAxis < 5; nAxis++)
                {
                    // 检查状态位
                    if (true)
                    {
                        result = mc.GT_GetSts(nAxis, out state, 1, out clock);
                        if (0 != result) break;
                        
                        if (0 == (state & 0x400))
                        {
                            if (AXIS_X == nAxis)
                                bX_done = true;
                            else if (AXIS_Y == nAxis)
                                bY_done = true;
                            else if (AXIS_Z == nAxis)
                                bZ_done = true;
                        }
                    }

                    // 检查是否已到快结束的位置
                    double dbPrfPos = 0;
                    if (true)
                    {
                        result = mc.GT_GetPrfPos(nAxis, out dbPrfPos, 1, out clock);
                        if (0 != result) break;
                        if (AXIS_X == nAxis)
                            dbCurrentX = dbPrfPos;
                        else if (AXIS_Y == nAxis)
                            dbCurrentY = dbPrfPos;
                        else if (AXIS_Z == nAxis)
                            dbCurrentZ = dbPrfPos;
                    }
                }
                //Debugger.Log(0, null, string.Format("222222 bX_done {0}, {1}, {2}", bX_done, bY_done, bZ_done));

                if (bX_done && bY_done && bZ_done)
                    break;

                Thread.Sleep(interval);

                counter++;
                if (counter > (8 * (1000 / interval)))
                    break;
            }
            //Debugger.Log(0, null, "222222 ccc");

            for (short nAxis = 1; nAxis < 5; nAxis++)
                mc.GT_ClrSts(nAxis, 1);
            
            lock (m_lock)
            {
                m_bIsMoving = false;
            }
            
            return;
        }

        // 三轴插补（不等待，立刻返回)，单位mm
        public bool linear_XYZ_no_stop(double posX, double posY, double posZ, double vel, double acc, bool bIgnoreXYLimit)
        {
            if (false == GeneralUtils.between_two(posX, m_axes[AXIS_X - 1].negative_limit, m_axes[AXIS_X - 1].positive_limit))
                return false;
            if (false == GeneralUtils.between_two(posY, m_axes[AXIS_Y - 1].negative_limit, m_axes[AXIS_Y - 1].positive_limit))
                return false;
            if (false == GeneralUtils.between_two(posZ, m_axes[AXIS_Z - 1].negative_limit, m_axes[AXIS_Z - 1].positive_limit))
                return false;

            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return false;
            }

            if (false == m_bHomed)
                return false;

            if (true == m_bIsMoving)
                return false;
            lock (m_lock)
            {
                m_bIsMoving = true;
            }

            // 建立坐标系
            mc.TCrdPrm crdPrm = new mc.TCrdPrm();
            crdPrm.dimension = 3;                // 坐标系为二维坐标系
            crdPrm.synVelMax = vel;          // 最大合成速度： 500pulse/ms
            crdPrm.synAccMax = acc;          // 最大加速度： 1pulse/ms^2
            crdPrm.evenTime = 50;                // 最小匀速时间： 50ms
            crdPrm.profile1 = AXIS_X;                   // 规划器1对应到X轴
            crdPrm.profile2 = AXIS_Y;                   // 规划器2对应到Y轴
            if (4 == AXIS_Z)
                crdPrm.profile4 = 3;                   // 规划器3对应到Z轴
            else
                crdPrm.profile3 = 3;
            crdPrm.setOriginFlag = 1;            // 表示需要指定坐标系的原点坐标的规划位置
            crdPrm.originPos1 = 0;             // 坐标系的原点坐标的规划位置为 [0, 0]
            crdPrm.originPos2 = 0;
            crdPrm.originPos3 = 0;
            //Debugger.Log(0, null, "222222 111");

            // 设置和清空坐标系
            short result = mc.GT_SetCrdPrm(1, ref crdPrm);
            //string msg2 = string.Format("222222 result = {0}", result);
            //Debugger.Log(0, null, msg2);

            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 222");

            result = mc.GT_CrdClear(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 333");

            // 启动运动
            int x = (int)(posX * 1000 * m_axes[AXIS_X - 1].motion_unit_scale);
            int y = (int)(posY * 1000 * m_axes[AXIS_Y - 1].motion_unit_scale);
            int z = (int)(posZ * 1000 * m_axes[AXIS_Z - 1].motion_unit_scale);
            result = mc.GT_LnXYZ(1, x, y, z, vel, acc, 0, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 bbb");

            result = mc.GT_CrdStart(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }

            //for (short nAxis = 1; nAxis < 4; nAxis++)
            //mc.GT_ClrSts(nAxis, 1);

            lock (m_lock)
            {
                m_bIsMoving = false;
            }
            return true;
        }

        // 三轴插补（不等待，立刻返回)，单位mm
        public bool linear_XYZ_no_stop(double posX, double posY, double posZ, bool bIgnoreXYLimit)
        {
            if (false == GeneralUtils.between_two(posX, m_axes[AXIS_X - 1].negative_limit, m_axes[AXIS_X - 1].positive_limit))
                return false;
            if (false == GeneralUtils.between_two(posY, m_axes[AXIS_Y - 1].negative_limit, m_axes[AXIS_Y - 1].positive_limit))
                return false;
            if (false == GeneralUtils.between_two(posZ, m_axes[AXIS_Z - 1].negative_limit, m_axes[AXIS_Z - 1].positive_limit))
                return false;

            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return false;
            }

            if (false == m_bHomed)
                return false;

            if (true == m_bIsMoving)
                return false;
            lock (m_lock)
            {
                m_bIsMoving = true;
            }

            bool bIsLongRange = false;
            double vel = m_axes[AXIS_X - 1].vel_for_short_range;
            double acc = m_axes[AXIS_X - 1].acc_for_short_range;
            double distance = Math.Sqrt((posX - form_parent.m_current_xyz.x) * (posX - form_parent.m_current_xyz.x)
                + (posY - form_parent.m_current_xyz.y) * (posY - form_parent.m_current_xyz.y));
            if (distance >= LONG_SHORT_RANGE_THRESHOLD)
            {
                bIsLongRange = true;
                vel = m_axes[AXIS_X - 1].vel_for_long_range;
                acc = m_axes[AXIS_X - 1].acc_for_long_range;
            }

            // 建立坐标系
            mc.TCrdPrm crdPrm = new mc.TCrdPrm();
            crdPrm.dimension = 3;                // 坐标系为二维坐标系
            crdPrm.synVelMax = vel;          // 最大合成速度： 500pulse/ms
            crdPrm.synAccMax = acc;          // 最大加速度： 1pulse/ms^2
            crdPrm.evenTime = 50;                // 最小匀速时间： 50ms
            crdPrm.profile1 = AXIS_X;                   // 规划器1对应到X轴
            crdPrm.profile2 = AXIS_Y;                   // 规划器2对应到Y轴
            if (4 == AXIS_Z)
                crdPrm.profile4 = 3;                   // 规划器3对应到Z轴
            else
                crdPrm.profile3 = 3;
            crdPrm.setOriginFlag = 1;            // 表示需要指定坐标系的原点坐标的规划位置
            crdPrm.originPos1 = 0;             // 坐标系的原点坐标的规划位置为 [0, 0]
            crdPrm.originPos2 = 0;
            crdPrm.originPos3 = 0;
            //Debugger.Log(0, null, "222222 111");

            // 设置和清空坐标系
            short result = mc.GT_SetCrdPrm(1, ref crdPrm);
            //string msg2 = string.Format("222222 result = {0}", result);
            //Debugger.Log(0, null, msg2);

            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 222");

            result = mc.GT_CrdClear(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 333");

            // 启动运动
            int x = (int)(posX * 1000 * m_axes[AXIS_X - 1].motion_unit_scale);
            int y = (int)(posY * 1000 * m_axes[AXIS_Y - 1].motion_unit_scale);
            int z = (int)(posZ * 1000 * m_axes[AXIS_Z - 1].motion_unit_scale);
            result = mc.GT_LnXYZ(1, x, y, z, vel, acc, 0, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }
            //Debugger.Log(0, null, "222222 bbb");

            result = mc.GT_CrdStart(1, 0);
            if (0 != result)
            {
                lock (m_lock)
                {
                    m_bIsMoving = false;
                }
                return false;
            }

            //for (short nAxis = 1; nAxis < 4; nAxis++)
            //mc.GT_ClrSts(nAxis, 1);

            lock (m_lock)
            {
                m_bIsMoving = false;
            }
            return true;
        }

        // JOG模式
        public void jog(short axis, double speed, double acc, short dir)
        {
            if (true == form_parent.m_bEmergencyExit)
            {
                stop_all_axes();
                return;
            }

            if (true == m_bIsMoving)
                return;
            lock (m_lock)
            {
                m_bIsMoving = true;
            }

            mc.GT_ClrSts(axis, 1);
            mc.GT_PrfJog(axis);                     // 设置为jog模式

            mc.TJogPrm MyTJogPrm;
            mc.GT_GetJogPrm(axis, out MyTJogPrm);

            MyTJogPrm.acc = acc;
            MyTJogPrm.dec = acc;

            mc.GT_SetJogPrm(axis, ref MyTJogPrm);

            double vel = dir * speed;
            mc.GT_SetVel(axis, vel);
            mc.GT_Update(1 << (axis - 1));

            lock (m_lock)
            {
                m_bIsMoving = false;
            }
        }
        
    }
}
