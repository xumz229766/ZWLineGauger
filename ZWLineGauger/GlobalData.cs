using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using ZWLineGauger.Gaugers;

using ZWLineGauger.Forms;
using System.Data.SqlClient;
using ZWLineGauger.Hardwares;
using HalconDotNet;

namespace ZWLineGauger
{


    public class globaldata
    {
        //xumz
        public static string m_fontname = "Arial Rounded MT";
        public static double m_fontsize = 22;
        public static string m_fontcolor = "FFFF00FF";
        public static int m_fontstyle = 1;
        public static bool isRun = false;
        public static bool isStringOffset = false;

        public static bool isDrawFinish = false;
        public static bool isHandleFinish = false;
    }

}
