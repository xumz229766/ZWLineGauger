using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Excel = Microsoft.Office.Interop.Excel;
//using Microsoft.Office.Core;
using System.Reflection;
using System.Collections;
using System.Drawing;

namespace ZWLineGauger.Misc
{
    public class ExcelUtil
    {
        Excel.Application   m_excel_exe;
        Excel.Workbook    m_work_book;
        Excel.Worksheet   m_work_sheet;

        bool m_bInited = false;
        bool m_bIsFileOpened = false;

        public ExcelUtil()
        {

        }

        public bool init()
        {
            if (true == m_bInited)
                return true;

            try
            {
                m_excel_exe = new Excel.Application();
                m_excel_exe.Visible = false;
                
                m_bInited = true;
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 Excel电子表格功能初始化失败！异常信息：{0}", ex.Message));
                m_bInited = false;
            }

            return m_bInited;
        }

        public bool release()
        {
            if (false == m_bInited)
                return false;

            try
            {
                if (true == m_bIsFileOpened)
                    m_work_book.Close();

                m_excel_exe.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(m_excel_exe);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool create_excel_file(string strFileName, string[] listFieldNames, int nRows, string m_strTaskRunningStartingHMSTime)
        {
            if (false == m_bInited)
                return false;

            

            m_work_book = m_excel_exe.Workbooks.Add(System.Reflection.Missing.Value);
            m_work_sheet = (Excel.Worksheet)m_work_book.Sheets[1];
            m_work_sheet.Name = m_strTaskRunningStartingHMSTime;

            Excel.Worksheet delete_sheet;
            delete_sheet = (Excel.Worksheet)m_work_book.Sheets[2];
            delete_sheet.Delete();
            delete_sheet = (Excel.Worksheet)m_work_book.Sheets[2];
            delete_sheet.Delete();

            //m_work_sheet.Name = "测量结果";
            
            //m_work_sheet = (Excel.Worksheet)m_work_book.Sheets[2];
            //m_work_sheet.Name = "测量";
            m_work_sheet.Activate();

            for (int n = 0; n < listFieldNames.Length; n++)
            {
                m_work_sheet.Cells[1, n + 1] = listFieldNames[n];
            }

            m_work_sheet.SaveAs(strFileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing);

            //worksheet.PageSetup.CenterHorizontally = true;
            //worksheet.PageSetup.CenterVertically = true;

            for (int n = 1; n < nRows; n++)
            {
                ((Excel.Range)m_work_sheet.Rows[n, Missing.Value]).RowHeight = 30;
                ((Excel.Range)m_work_sheet.Rows[n, Missing.Value]).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            }

            ((Excel.Range)m_work_sheet.Columns[1, Missing.Value]).ColumnWidth = 7;
            ((Excel.Range)m_work_sheet.Columns[2, Missing.Value]).ColumnWidth = 16;
            ((Excel.Range)m_work_sheet.Columns[3, Missing.Value]).ColumnWidth = 16;
            ((Excel.Range)m_work_sheet.Columns[4, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[5, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[6, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[7, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[8, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[9, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[10, Missing.Value]).ColumnWidth = 13;
            //((Excel.Range)m_work_sheet.Columns[7, Missing.Value]).Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(250, 0, 0));

            Debugger.Log(0, null, string.Format("222222 worksheet.Rows.Count = {0}, {1}", m_work_sheet.Rows.Count, Missing.Value));

            m_work_book.Save();

            m_work_book.Close(false, Type.Missing, Type.Missing);

            return true;
        }

        //在已有的excel上新增sheet
        public bool creat_old_excel_file(string strFileName, string[] listFieldNames, int nRows, string m_strTaskRunningStartingHMSTime)
        {
            Excel.Application exe = new Excel.Application();
            exe.Visible = false;

            m_work_book = exe.Workbooks.Open(strFileName, System.Type.Missing, System.Type.Missing, System.Type.Missing,
                System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing,
                System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);


            //m_work_sheet = (Excel.Worksheet)m_work_book.Sheets[2];

            //m_work_sheet.Name = "测量";
            //m_work_sheet.Activate();
            m_work_sheet = (Excel.Worksheet)m_work_book.Worksheets.Add();
            m_work_sheet.Name = m_strTaskRunningStartingHMSTime;
            m_work_book.Save();

            m_work_sheet = (Excel.Worksheet)m_work_book.Sheets[1];
            Debugger.Log(0, null, string.Format("222222 报表保存 333 "));
            m_work_sheet.Activate();
            Debugger.Log(0, null, string.Format("222222 报表保存 555 "));
            for (int n = 0; n < listFieldNames.Length; n++)
            {
                m_work_sheet.Cells[1, n + 1] = listFieldNames[n];
            }
            exe.DisplayAlerts = false;
            m_work_sheet.SaveAs(strFileName, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Excel.XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing);
            exe.DisplayAlerts = true;
            //worksheet.PageSetup.CenterHorizontally = true;
            //worksheet.PageSetup.CenterVertically = true;

            for (int n = 1; n < nRows; n++)
            {
                ((Excel.Range)m_work_sheet.Rows[n, Missing.Value]).RowHeight = 30;
                ((Excel.Range)m_work_sheet.Rows[n, Missing.Value]).HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            }

            ((Excel.Range)m_work_sheet.Columns[1, Missing.Value]).ColumnWidth = 7;
            ((Excel.Range)m_work_sheet.Columns[2, Missing.Value]).ColumnWidth = 16;
            ((Excel.Range)m_work_sheet.Columns[3, Missing.Value]).ColumnWidth = 16;
            ((Excel.Range)m_work_sheet.Columns[4, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[5, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[6, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[7, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[8, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[9, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[10, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[11, Missing.Value]).ColumnWidth = 15;
            ((Excel.Range)m_work_sheet.Columns[12, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[13, Missing.Value]).ColumnWidth = 13;
            ((Excel.Range)m_work_sheet.Columns[14, Missing.Value]).ColumnWidth = 20;
            //((Excel.Range)m_work_sheet.Columns[7, Missing.Value]).Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(250, 0, 0));

            Debugger.Log(0, null, string.Format("222222 worksheet.Rows.Count = {0}, {1}", m_work_sheet.Rows.Count, Missing.Value));

            m_work_book.Save();

            m_work_book.Close(false, Type.Missing, Type.Missing);

            return true;
        }

        public bool update_row(int nRowIndex, ArrayList data)
        {
            //Debugger.Log(0, null, string.Format("222222 data = {0}", data.Count));

            for (int n = 0; n < data.Count; n++)
            {
                m_work_sheet.Cells[nRowIndex + 1, n + 1] = data[n];
                
                if ("定位孔" != data[data.Count - 1])
                {
                    if ("OK" == data[n])
                    {
                        ((Excel.Range)m_work_sheet.Cells[nRowIndex + 1, n + 1]).Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(0, 250, 0));
                    }
                    else if ("NG" == data[n])
                    {
                        ((Excel.Range)m_work_sheet.Cells[nRowIndex + 1, n + 1]).Interior.Color = System.Drawing.ColorTranslator.ToOle(Color.FromArgb(250, 0, 0));
                    }
                }
            }

            return true;
        }

        //打开已有的excel
        public bool open_old_file(string strFileName)
        {
            if (false == m_bInited)
                return false;

            // 设置 excel表格 打开后不可见
            Excel.Application exe = new Excel.Application();
            exe.Visible = false;

            m_work_book = exe.Workbooks.Open(strFileName, System.Type.Missing, System.Type.Missing, System.Type.Missing,
                System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing,
                System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);

            m_work_sheet = (Excel.Worksheet)m_work_book.Sheets[1];
            //m_work_sheet.Name = "测量结果";
            //m_work_sheet = (Excel.Worksheet)m_work_book.Sheets[2];
            //m_work_sheet.Name = "测量";
            m_work_sheet.Activate();

            m_bIsFileOpened = true;

            return true;
        }

        public bool open(string strFileName)
        {
            if (false == m_bInited)
                return false;

            // 设置 excel表格 打开后不可见
            Excel.Application exe = new Excel.Application();
            exe.Visible = false;

            m_work_book = exe.Workbooks.Open(strFileName, System.Type.Missing, System.Type.Missing, System.Type.Missing,
                System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing,
                System.Type.Missing, System.Type.Missing, System.Type.Missing, System.Type.Missing);

            m_work_sheet = (Excel.Worksheet)m_work_book.Sheets[1];
            //m_work_sheet.Name = "测量结果";
            //m_work_sheet = (Excel.Worksheet)m_work_book.Sheets[2];
            //m_work_sheet.Name = "测量";
            m_work_sheet.Activate();

            m_bIsFileOpened = true;

            return true;
        }

        public bool save_current_file()
        {
            if (false == m_bInited)
                return false;
            if (false == m_bIsFileOpened)
                return false;

            m_work_book.Save();

            return true;
        }

        public bool close_current_file()
        {
            if (false == m_bInited)
                return false;

            if (true == m_bIsFileOpened)
                m_work_book.Close();

            m_bIsFileOpened = false;

            return true;
        }
    }
}
