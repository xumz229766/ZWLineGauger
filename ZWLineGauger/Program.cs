using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZWLineGauger
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool createNew;
            using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, Application.ProductName, out createNew))
            {
                if (createNew)
                    Application.Run(new MainUI());
                else
                {
                    MessageBox.Show("线宽程序 PixelLiner 已经有一个运行实例，请先关闭 (或在任务管理器中终止实例)，再重新打开。", "程序冲突提示");
                    System.Environment.Exit(1);
                }
            }
        }
    }
}
