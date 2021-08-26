using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ZWLineGauger
{
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    class GeneralUtils
    {
        [DllImport("user32.dll")]
        private static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT pt);

        // 将鼠标光标移动到指定坐标
        static public void set_cursor_pos(int x, int y)
        {
            SetCursorPos(x, y);
        }

        // 获取鼠标光标在屏幕上的位置
        static public void get_cursor_pos(ref int x, ref int y)
        {
            POINT pt;
            GetCursorPos(out pt);
            x = pt.X;
            y = pt.Y;
        }

        // 获取两点之间的欧拉距离
        static public double get_distance(Point2d pt1, Point2d pt2)
        {
            return System.Math.Sqrt((pt1.x - pt2.x) * (pt1.x - pt2.x) + (pt1.y - pt2.y) * (pt1.y - pt2.y));
        }

        static public bool between_two(int value, int lower, int upper)
        {
            if ((value >= lower) && (value <= upper))
                return true;
            else
                return false;
        }

        static public bool between_two(double value, double lower, double upper)
        {
            if ((value >= lower) && (value <= upper))
                return true;
            else
                return false;
        }

        // 检查对应名字的窗口是否处于打开状态
        static public bool check_if_form_is_open(string strFormName, ref Form form)
        {
            bool bResult = false;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.Name == strFormName)
                {
                    form = frm;
                    bResult = true;
                    break;
                }
            }
            return bResult;
        }

        static public bool check_if_form_is_open(string strFormName)
        {
            bool bResult = false;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.Name == strFormName)
                {
                    bResult = true;
                    break;
                }
            }
            return bResult;
        }
        
        static public double convert_um_value_by_unit(double value_in_um, int nUnitType)
        {
            if (0 == nUnitType)
                return value_in_um / 1000;
            else if (1 == nUnitType)
                return value_in_um;
            else if (2 == nUnitType)
                return value_in_um / 25.4;

            return -1;
        }

        static public double convert_mm_value_by_unit(double value_in_mm, int nUnitType)
        {
            if (0 == nUnitType)
                return value_in_mm;
            else if (1 == nUnitType)
                return value_in_mm * 1000;
            else if (2 == nUnitType)
                return value_in_mm * 1000 / 25.4;

            return -1;
        }

        static public double convert_mil_value_by_unit(double value_in_mil, int nUnitType)
        {
            if (0 == nUnitType)
                return value_in_mil * 25.4 / 1000;
            else if (1 == nUnitType)
                return value_in_mil * 25.4;
            else if (2 == nUnitType)
                return value_in_mil;

            return -1;
        }

        // 把 Image 转成 bytes
        static public byte[] convert_image_to_bytes(Image img, ImageFormat format, ref int nStride)
        {
            Bitmap bmp = new Bitmap(img);
            
            Rectangle rect = new Rectangle(new Point(0, 0), bmp.Size);
            BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            byte[] buf = new byte[bmp_data.Stride * bmp_data.Height];

            System.Runtime.InteropServices.Marshal.Copy(bmp_data.Scan0, buf, 0, buf.Length);

            bmp.UnlockBits(bmp_data);

            nStride = bmp_data.Stride;

            return buf;
        }

        static public bool GetKeyValue(string buf, string key, ref string value)
        {
            int index = buf.IndexOf(key);

            if (index < 0)
                return false;

            string sub = buf.Substring(index, key.Length);

            if (sub == key)
            {
                int start_idx = index + key.Length + 1;
                if (buf.Length > start_idx)
                {
                    int idx = buf.IndexOf("\r\n", start_idx);
                    if (idx > 0)
                    {
                        value = buf.Substring(start_idx, idx - start_idx);
                        return true;
                    }
                }
            }

            return false;
        }

        static public bool GetKeyValue2(string buf, string key, ref string value, bool bThrowExOnFailure = true)
        {
            int index = buf.IndexOf(key);

            if (index < 0)
            {
                if (true == bThrowExOnFailure)
                    throw new Exception();
                else
                    return false;
            }
            
            string sub = buf.Substring(index, key.Length);

            if (sub == key)
            {
                int start_idx = index + key.Length + 1;
                if (start_idx < buf.Length)
                    value = buf.Substring(start_idx, buf.Length - start_idx);
                else
                    value = "";
                
                return true;
            }

            if (true == bThrowExOnFailure)
                throw new Exception();
            else
                return false;
        }

        static public string convert_number_to_string_with_digits(float num, int nDigits)
        {
            string str = "";

            switch (nDigits)
            {
                case 1:
                    str = string.Format("{0:0.0}", num);
                    break;
                case 2:
                    str = string.Format("{0:0.00}", num);
                    break;
                case 3:
                    str = string.Format("{0:0.000}", num);
                    break;
                case 4:
                    str = string.Format("{0:0.0000}", num);
                    break;
                case 5:
                    str = string.Format("{0:0.00000}", num);
                    break;
                case 6:
                    str = string.Format("{0:0.000000}", num);
                    break;
            }

            return str;
        }

        static public string convert_number_to_string_with_digits(double num, int nDigits)
        {
            string str = "";

            switch (nDigits)
            {
                case 1:
                    str = string.Format("{0:0.0}", num);
                    break;
                case 2:
                    str = string.Format("{0:0.00}", num);
                    break;
                case 3:
                    str = string.Format("{0:0.000}", num);
                    break;
                case 4:
                    str = string.Format("{0:0.0000}", num);
                    break;
                case 5:
                    str = string.Format("{0:0.00000}", num);
                    break;
                case 6:
                    str = string.Format("{0:0.000000}", num);
                    break;
            }

            return str;
        }
    }
}
