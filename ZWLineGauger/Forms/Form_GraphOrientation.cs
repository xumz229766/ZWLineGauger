using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;

namespace ZWLineGauger
{
    public enum GRAPH_READING_STATE
    {
        NONE = 0,
        READING,
        PARSING_1,
        PARSING_2,
        RENDERING,
        RENDERING_ENLARGED
    }
    
    public partial class Form_GraphOrientation : Form
    {
        // ret_ints[] 0是成功标志，1和2是图像宽高，3是图纸每行数据所占字节数，4是图纸每行数据所占字节数2
        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool read_gerber_thumbnail(char[] path, int nZoomRatio, int[] pRetInts);

        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool get_gerber_graph_reading_state_info(int[] pRetInts);

        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool copy_thumbnail_view(byte[] buf, int nOrientation, int nBytesPerLine, int nWidth, int nHeight);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_set_picture_box_wh_for_ODB(int[] in_values);

        [DllImport("graph_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_set_picture_box_wh_for_gerber(int[] in_values);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool odb_read_steps(char[] path, int[] pRetInts);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool odb_copy_step_names(byte[] buf, int[] string_lens, int[] pRetInts);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool odb_get_layers_info_for_step(char[] strStep, int[] pRetInts);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool odb_copy_layer_names(char[] strStep, byte[] buf, int[] string_lens, int[] pRetInts);
        
        // ret_ints[] 0是成功标志，1和2是图像宽高，3是图纸每行数据所占字节数，4是图纸每行数据所占字节数2，
        //5和6是offset xy，ret_doubles[] 0是pixels_per_mm
        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool draw_layer(char[] strOdbFile, char[] strStep, char[] strLayer, int[] pRetInts, double[] pRetDoubles);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool copy_ODB_thumbnail(byte[] buf, int nOrientation, int nBytesPerLine, int nWidth, int nHeight);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_delete_odb_files(int[] pRetInts);
        
        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_ODB_graph_reading_state_info(int[] pRetInts);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_ODB_array_unit_types_num(int[] pRetInts, int[] pRetInts2);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_ODB_array_unit_name(int nTypeIdx, byte[] name);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_ODB_array_rect_info(int nTypeIdx, int[] pRetInts, double[] pRetDoubles);

        [DllImport("odb_engine.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool dllapi_get_ODB_measure_items(int[] pRetInts, double[] pRetDoubles);

        MainUI   parent;

        const int   UNIT_NAME_LEN = 64;

        static public ThumbnailView[] m_thumbnail_views = new ThumbnailView[9];
        
        static public int   m_thumbnail_width = 300;
        static public int   m_thumbnail_height = 300;

        static public bool   m_bIsFormActivated = false;
        static public bool   m_bIsReadingGraphForThumbnail = false;
        static public int   m_nThumbnailProgress = 0;
        static public GRAPH_READING_STATE m_reading_state = GRAPH_READING_STATE.NONE;

        static public bool m_bIsMonitorStarted = false;
        static public bool m_bIsDLLCommunicationStarted = false;
        
        public int   m_nSelectedStepIndex = -1;
        public int   m_nSelectedLayerIndex = -1;

        static private int   m_nStepsCount = 0;

        static private List<string>   m_list_step_names = new List<string>();
        static private char[]   m_strSelectedStep;
        static public char[]    m_strSelectedLayer;
        
        static private byte[]   m_odb_img_buf;

        static private int   m_nOdbImageW = 0;
        static private int   m_nOdbImageH = 0;
        static private int   m_nOdbBytesPerLine = 0;
        static public int   m_nOdbOffsetX = 0;
        static public int   m_nOdbOffsetY = 0;

        static public double   m_pixels_per_mm = 0;

        public bool m_bLoadGraph = false;

        public string        m_strLayerName;

        static public int   m_nGraphType = 0;                     // 0 为odb，1 为gerber

        Form_ProgressInfo   m_form_progress;

        static string   m_strFilePath = "";
        
        public delegate void dl_key_message_notifier(string info);

        public void CBD_PostKeyMessage(string info)
        {
            if (this.InvokeRequired)
            {
                dl_key_message_notifier callback = new dl_key_message_notifier(CBD_PostKeyMessage);
                this.Invoke(callback, info);
                return;
            }
            else
            {
                if (info == "从dll得到缩略图")
                {
                    for (int n = 0; n < 8; n++)
                    {
                        m_thumbnail_views[n].refresh_image();
                    }
                }
                else if (info == "得到ODB缩略图")
                {
                    for (int n = 0; n < 9; n++)
                        m_thumbnail_views[n].refresh_image();

                    m_form_progress.Close();
                    m_form_progress.Dispose();
                }
                else if (info == "得到ODB缩略图失败")
                {
                    m_form_progress.Close();
                    m_form_progress.Dispose();
                }
                else if (info == "文件缓存区清理完毕")
                {
                    m_form_progress.Close();
                    m_form_progress.Dispose();
                    MessageBox.Show(this, "文件缓存区已清理完毕", "提示", MessageBoxButtons.OK);
                }
                else if (info == "得到steps名字")
                {
                    listBox_Steps.Items.Clear();
                    for (int n = 0; n < m_nStepsCount; n++)
                    {
                        //if (m_list_step_names[n].StartsWith("pcb") ||
                        //    m_list_step_names[n].StartsWith("cad") ||
                        //    m_list_step_names[n].StartsWith("panel") ||
                        //    m_list_step_names[n].StartsWith("pcb") ||
                        //    m_list_step_names[n].StartsWith("pnl") ||
                        //    m_list_step_names[n].StartsWith("cam") ||
                        //    m_list_step_names[n].StartsWith("unit") ||
                        //    m_list_step_names[n].StartsWith("hc") ||
                        //    m_list_step_names[n].StartsWith("set") ||
                        //    m_list_step_names[n].StartsWith("mi"))
                        {
                            listBox_Steps.Items.Add(m_list_step_names[n]);
                        }
                        
                        //Debugger.Log(0, null, string.Format("222222 n {0}: string = {1}", n, m_list_step_names[n]));
                    }

                    m_form_progress.Close();
                    m_form_progress.Dispose();
                }
                else if (info == "缩略图读取进度")
                {
                    //string msg2 = string.Format("222222 缩略图读取进度 = {0}, {1}", (int)m_reading_state, m_nThumbnailProgress);
                    //Debugger.Log(0, null, msg2);

                    if (null != m_form_progress)
                    {
                        m_form_progress.progressBar1.Value = m_nThumbnailProgress;

                        string text;
                        switch (m_reading_state)
                        {
                            case GRAPH_READING_STATE.READING:
                                text = String.Format("正在读取文件: {0}%", m_nThumbnailProgress);
                                m_form_progress.label_ProgressInfo.Text = text;
                                break;
                            case GRAPH_READING_STATE.PARSING_1:
                                text = String.Format("解析第一阶段: {0}%", m_nThumbnailProgress);
                                m_form_progress.label_ProgressInfo.Text = text;
                                break;
                            case GRAPH_READING_STATE.PARSING_2:
                                text = String.Format("解析第二阶段: {0}%", m_nThumbnailProgress);
                                m_form_progress.label_ProgressInfo.Text = text;
                                break;
                            case GRAPH_READING_STATE.RENDERING:
                                text = String.Format("正在渲染: {0}%", m_nThumbnailProgress);
                                m_form_progress.label_ProgressInfo.Text = text;
                                break;
                        }
                        
                        if (false == Form_GraphOrientation.m_bIsReadingGraphForThumbnail)
                            m_form_progress.Close();
                    }
                }
            }
        }

        public Form_GraphOrientation(MainUI parent)
        {
            this.parent = parent;
            InitializeComponent();
            
            // 动态创建图片控件
            for (int n = 0; n < 4; n++)
            {
                m_thumbnail_views[n] = new ThumbnailView(n);
                m_thumbnail_views[n].Width = m_thumbnail_width;
                m_thumbnail_views[n].Height = m_thumbnail_height;
                m_thumbnail_views[n].Top = 50;
                m_thumbnail_views[n].Left = 20 + n * (m_thumbnail_width + 10);
                m_thumbnail_views[n].BorderStyle = BorderStyle.FixedSingle;
                m_thumbnail_views[n].SizeMode = PictureBoxSizeMode.Zoom;
                m_thumbnail_views[n].BackColor = Color.Black;
                this.Controls.Add(m_thumbnail_views[n]);
            }
            
            for (int n = 4; n < 8; n++)
            {
                m_thumbnail_views[n] = new ThumbnailView(n);
                m_thumbnail_views[n].Width = m_thumbnail_width;
                m_thumbnail_views[n].Height = m_thumbnail_height;
                m_thumbnail_views[n].Top = 50 + 400;
                m_thumbnail_views[n].Left = 20 + (n - 4) * (m_thumbnail_width + 10);
                m_thumbnail_views[n].BorderStyle = BorderStyle.FixedSingle;
                m_thumbnail_views[n].SizeMode = PictureBoxSizeMode.Zoom;
                m_thumbnail_views[n].BackColor = Color.Black;
                this.Controls.Add(m_thumbnail_views[n]);
            }
            
            m_thumbnail_views[8] = new ThumbnailView(0);
            m_thumbnail_views[8].Width = pictureBox1.Width;
            m_thumbnail_views[8].Height = pictureBox1.Height;
            m_thumbnail_views[8].Top = pictureBox1.Top;
            m_thumbnail_views[8].Left = pictureBox1.Left;
            m_thumbnail_views[8].BorderStyle = BorderStyle.FixedSingle;
            m_thumbnail_views[8].SizeMode = PictureBoxSizeMode.Zoom;
            m_thumbnail_views[8].BackColor = Color.Black;
            this.groupBox1.Controls.Add(m_thumbnail_views[8]);
            this.pictureBox1.Hide();

            textBox_GraphFileName.Text = Path.GetFileName(MainUI.m_strGraphFilePath);
            
            refresh_orientation_icons();
        }

        public static void thread_DLL_communication(object obj)
        {
            int nState = 0;
            int nProgress = 0;
            int nPrevState = 0;
            int nPrevProgress = 0;

            while (true)
            {
                Thread.Sleep(100);

                int[] pRetInts = new int[5];

                if (0 == Form_GraphOrientation.m_nGraphType)
                {
                    dllapi_get_ODB_graph_reading_state_info(pRetInts);
                    nState = pRetInts[0];
                    nProgress = pRetInts[1];

                    //string msg = string.Format("222222 nState = {0}, nProgress = {1}", nState, nProgress);
                    //Debugger.Log(0, null, msg);
                }
                else if (1 == Form_GraphOrientation.m_nGraphType)
                {
                    get_gerber_graph_reading_state_info(pRetInts);
                    nState = pRetInts[0];
                    nProgress = pRetInts[1];
                }
                
                if (nState != nPrevState)
                {
                    m_reading_state = (GRAPH_READING_STATE)nState;
                    m_nThumbnailProgress = nProgress;
                    MainUI.m_reset_event_for_updating_thumbnail_progress.Set();
                    MainUI.m_reset_event_for_updating_graphview_progress.Set();
                }
                if (nProgress != nPrevProgress)
                {
                    m_reading_state = (GRAPH_READING_STATE)nState;
                    m_nThumbnailProgress = nProgress;
                    MainUI.m_reset_event_for_updating_thumbnail_progress.Set();
                    MainUI.m_reset_event_for_updating_graphview_progress.Set();
                }
                nPrevState = nState;
                nPrevProgress = nProgress;

                if (MainUI.m_bExitProgram)
                    break;
            }
        }

        public static void thread_monitor_graph_thumbnail_reading_progress(object obj)
        {
            while (true)
            {
                MainUI.m_reset_event_for_updating_thumbnail_progress.WaitOne();

                if (MainUI.m_bExitProgram)
                    break;
                
                dl_key_message_notifier notifier = obj as dl_key_message_notifier;
                notifier("缩略图读取进度");
            }
        }

        public static void thread_read_gerber_thumbnail(object obj)
        {
            Form_GraphOrientation.m_bIsReadingGraphForThumbnail = true;

            string msg = string.Format("222222 开始读取图纸");
            Debugger.Log(0, null, msg);
            
            char[] path = Form_GraphOrientation.m_strFilePath.ToCharArray();
            int[] pRetInts = new int[10];

            pRetInts[0] = 0;
            read_gerber_thumbnail(path, MainUI.m_nGraphZoomRatio, pRetInts);

            if (1 == pRetInts[0])
            {
                int nWidth = pRetInts[1];
                int nHeight = pRetInts[2];
                int nBytesPerLine = pRetInts[3];
                int nBytesPerLine2 = pRetInts[4];
                
                Debugger.Log(0, null, string.Format("222222 图纸宽高 = [{0},{1}], 每行字节数 = {2}", nWidth, nHeight, nBytesPerLine));

                // 拷贝缩略图
                if (true)
                {
                    byte[] buf = new byte[nBytesPerLine * nHeight];
                    for (int n = 0; n < 7; n += 2)
                    {
                        copy_thumbnail_view(buf, n, nBytesPerLine, nWidth, nHeight);

                        MemoryStream stream = new MemoryStream(buf);

                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit = new Bitmap(nWidth, nHeight, PixelFormat.Format1bppIndexed);

                        Rectangle rect = new Rectangle(new Point(0, 0), Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Size);
                        BitmapData bmp_data = Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.LockBits(rect, ImageLockMode.WriteOnly, 
                            Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.PixelFormat);
                        System.Runtime.InteropServices.Marshal.Copy(buf, 0, bmp_data.Scan0, buf.Length);
                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.UnlockBits(bmp_data);
                        
                        //Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.RotateFlip(RotateFlipType.Rotate180FlipX);

                        ColorPalette palette = Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Palette;
                        Color[] colors = palette.Entries;
                        colors[0] = Color.FromArgb(255, 0, 0, 0);
                        colors[1] = Color.FromArgb(255, 255, 0, 0);

                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Palette = palette;

                        stream.Close();
                    }
                }
                
                if (true)
                {
                    byte[] buf = new byte[nBytesPerLine2 * nWidth];
                    for (int n = 1; n < 8; n += 2)
                    {
                        copy_thumbnail_view(buf, n, nBytesPerLine2, nWidth, nHeight);

                        MemoryStream stream = new MemoryStream(buf);

                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit = new Bitmap(nHeight, nWidth, PixelFormat.Format1bppIndexed);

                        Rectangle rect = new Rectangle(new Point(0, 0), Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Size);
                        BitmapData bmp_data = Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.LockBits(rect, ImageLockMode.WriteOnly, 
                            Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.PixelFormat);
                        System.Runtime.InteropServices.Marshal.Copy(buf, 0, bmp_data.Scan0, buf.Length);
                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.UnlockBits(bmp_data);

                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.RotateFlip(RotateFlipType.Rotate180FlipX);

                        ColorPalette palette = Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Palette;
                        Color[] colors = palette.Entries;
                        colors[0] = Color.FromArgb(255, 0, 0, 0);
                        colors[1] = Color.FromArgb(255, 255, 0, 0);

                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Palette = palette;

                        stream.Close();
                    }
                }

                for (int n = 0; n < 8; n++)
                    Form_GraphOrientation.m_thumbnail_views[n].m_bRequireRedraw32Bitmap = true;

                dl_key_message_notifier notifier = obj as dl_key_message_notifier;
                notifier("从dll得到缩略图");
            }
            
            msg = string.Format("222222 图纸读取结束");
            Debugger.Log(0, null, msg);
            
            Form_GraphOrientation.m_bIsReadingGraphForThumbnail = false;
            MainUI.m_reset_event_for_updating_thumbnail_progress.Set();
        }

        public static void thread_read_steps(object obj)
        {
            char[] path = m_strFilePath.ToCharArray();
            int[] pRetInts = new int[10];
            odb_read_steps(path, pRetInts);
            if (1 == pRetInts[0])
            {
                byte[] recv_buf = new byte[pRetInts[2]];
                int[] string_lens = new int[pRetInts[1]];
                int[] pRetInts2 = new int[10];
                odb_copy_step_names(recv_buf, string_lens, pRetInts2);

                //string msg = string.Format("222222 共{0}个step, step名称字节总数{1}", out_values[1], out_values[2]);
                string msg = string.Format("222222 共{0}个steps", pRetInts[1]);
                Debugger.Log(0, null, msg);

                int counter = 0;
                StringBuilder sb = new StringBuilder();
                string[] strings = new string[pRetInts[1]];
                for (int n = 0; n < pRetInts[1]; n++)
                {
                    for (int m = counter; m < counter + string_lens[n]; m++)
                    {
                        sb.Append((char)(recv_buf[m]));
                    }
                    strings[n] = sb.ToString();
                    m_list_step_names.Add(strings[n]);

                    sb.Clear();
                    
                    counter += (string_lens[n] + 1);
                }
                
                m_nStepsCount = pRetInts[1];

                dl_key_message_notifier notifier = obj as dl_key_message_notifier;
                notifier("得到steps名字");
            }
        }

        public static void thread_parse_and_draw_layers(object obj)
        {
            Form_GraphOrientation.m_bIsReadingGraphForThumbnail = true;
            
            char[] path = m_strFilePath.ToCharArray();
            int[] pRetInts = new int[10];
            double[] pRetDoubles = new double[10];

            pRetInts[0] = 0;
            
            draw_layer(path, m_strSelectedStep, m_strSelectedLayer, pRetInts, pRetDoubles);

            //string msg = string.Format("222222 step = {0}, layer = {1}, path = {2}", m_strSelectedStep, m_strSelectedLayer, path);
            //Debugger.Log(0, null, msg);

            if (1 == pRetInts[0])
            {
                m_nOdbImageW = pRetInts[1];
                m_nOdbImageH = pRetInts[2];
                m_nOdbBytesPerLine = pRetInts[3];
                m_nOdbOffsetX = pRetInts[5];
                m_nOdbOffsetY = pRetInts[6];

                m_pixels_per_mm = pRetDoubles[0];
                
                string msg = string.Format("222222 ODB图像宽高 = [{0},{1}], nBytesPerLine = {2}, offset [{3},{4}], pixels_per_mm = {5:0.000}",
                    m_nOdbImageW, m_nOdbImageH, m_nOdbBytesPerLine, m_nOdbOffsetX, m_nOdbOffsetY, m_pixels_per_mm);
                Debugger.Log(0, null, msg);

                // 获取阵列信息
                rotated_array_rect.m_nSelectCount = 0;
                //get_ODB_array_info(m_ODB_thumbnail_array_rects);
                
                // 拷贝缩略图
                if (true)
                {
                    Debugger.Log(0, null, string.Format("222222 ODB图像 new 111"));
                    m_odb_img_buf = new byte[m_nOdbBytesPerLine * m_nOdbImageH];
                    Debugger.Log(0, null, string.Format("222222 ODB图像 new 222"));
                    //copy_ODB_thumbnail(m_odb_img_buf, 0, m_nOdbBytesPerLine, m_nOdbImageW, m_nOdbImageH);

                    for (int n = 0; n < 9; n += 2)
                    {
                        if (8 == n)
                        {
                            if (GraphView.m_ODB_thumbnail_array_rects.Count > 0)
                                //Form_GraphOrientation.m_thumbnail_views[n].m_bDrawArrayRects = true;
                                Form_GraphOrientation.m_thumbnail_views[n].m_bDrawArrayRects = false;
                            else
                                Form_GraphOrientation.m_thumbnail_views[n].m_bDrawArrayRects = false;
                            copy_ODB_thumbnail(m_odb_img_buf, 0, m_nOdbBytesPerLine, m_nOdbImageW, m_nOdbImageH);
                        }
                        else
                            copy_ODB_thumbnail(m_odb_img_buf, n, m_nOdbBytesPerLine, m_nOdbImageW, m_nOdbImageH);

                        MemoryStream stream = new MemoryStream(m_odb_img_buf);

                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit = new Bitmap(m_nOdbImageW, m_nOdbImageH, PixelFormat.Format1bppIndexed);

                        Rectangle rect = new Rectangle(new Point(0, 0), Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Size);
                        BitmapData bmp_data = Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.LockBits(rect, ImageLockMode.WriteOnly,
                            Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.PixelFormat);
                        System.Runtime.InteropServices.Marshal.Copy(m_odb_img_buf, 0, bmp_data.Scan0, m_odb_img_buf.Length);
                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.UnlockBits(bmp_data);

                        ColorPalette palette = Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Palette;
                        Color[] colors = palette.Entries;
                        colors[0] = Color.FromArgb(255, 0, 0, 0);
                        colors[1] = Color.FromArgb(255, 0, 255, 0);

                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Palette = palette;

                        stream.Close();
                    }

                    int nBytesPerLine2 = ((m_nOdbImageH + 31) / 32 * 4);
                    byte[] buf = new byte[nBytesPerLine2 * m_nOdbImageW];
                    for (int n = 1; n < 8; n += 2)
                    {
                        copy_ODB_thumbnail(buf, n, nBytesPerLine2, m_nOdbImageW, m_nOdbImageH);

                        MemoryStream stream = new MemoryStream(buf);

                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit = new Bitmap(m_nOdbImageH, m_nOdbImageW, PixelFormat.Format1bppIndexed);

                        Rectangle rect = new Rectangle(new Point(0, 0), Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Size);
                        BitmapData bmp_data = Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.LockBits(rect, ImageLockMode.WriteOnly,
                            Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.PixelFormat);
                        System.Runtime.InteropServices.Marshal.Copy(buf, 0, bmp_data.Scan0, buf.Length);
                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.UnlockBits(bmp_data);

                        ColorPalette palette = Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Palette;
                        Color[] colors = palette.Entries;
                        colors[0] = Color.FromArgb(255, 0, 0, 0);
                        colors[1] = Color.FromArgb(255, 0, 255, 0);
                        
                        Form_GraphOrientation.m_thumbnail_views[n].m_bitmap_1bit.Palette = palette;

                        stream.Close();
                    }

                    for (int n = 0; n < 9; n++)
                        Form_GraphOrientation.m_thumbnail_views[n].m_bRequireRedraw32Bitmap = true;

                    dl_key_message_notifier notifier = obj as dl_key_message_notifier;
                    notifier("得到ODB缩略图");
                }
            }
            else
            {
                dl_key_message_notifier notifier = obj as dl_key_message_notifier;
                notifier("得到ODB缩略图失败");
            }

            Form_GraphOrientation.m_bIsReadingGraphForThumbnail = false;
        }

        public static void thread_delete_ODB_files(object obj)
        {
            int[] pRetInts = new int[10];
            pRetInts[0] = 0;
            dllapi_delete_odb_files(pRetInts);

            dl_key_message_notifier notifier = obj as dl_key_message_notifier;
            notifier("文件缓存区清理完毕");
        }

        public static void get_ODB_array_info(List<List<rotated_array_rect>> list_list_rects)
        {
            for (int n = 0; n < list_list_rects.Count; n++)
                list_list_rects[n].Clear();
            list_list_rects.Clear();

            int[] values = new int[10];
            int[] values2 = new int[10];
            dllapi_get_ODB_array_unit_types_num(values, values2);

            int nTypesNum = values[1];
            if (nTypesNum > 0)
            {
                for (int n = 0; n < nTypesNum; n++)
                {
                    int nLen = values2[n + 2];
                    int nUnitsNum = values[n + 2];
                    if ((nLen > 0) && (nLen < UNIT_NAME_LEN))
                    {
                        // 获取单元名称
                        byte[] name = new byte[nLen];
                        dllapi_get_ODB_array_unit_name(n, name);

                        StringBuilder sb = new StringBuilder();
                        for (int m = 0; m < nLen; m++)
                            sb.Append((char)(name[m]));

                        //msg = string.Format("222222 阵列子单元类型{0} 共包含 {1} 个子单元, name = {2}",
                        //    n + 1, nUnitsNum, sb.ToString());
                        //Debugger.Log(0, null, msg);

                        // 获取阵列信息
                        int[] pRetInts = new int[nUnitsNum * 7];
                        double[] pRetDoubles = new double[nUnitsNum * 7];
                        dllapi_get_ODB_array_rect_info(n, pRetInts, pRetDoubles);

                        List<rotated_array_rect> list_rects = new List<rotated_array_rect>();
                        for (int k = 0; k < nUnitsNum; k++)
                        {
                            rotated_array_rect rect = new rotated_array_rect();
                            rect.strUnitName = sb.ToString();
                            rect.angle = pRetDoubles[k * 7];
                            rect.odb_angle = pRetDoubles[k * 7 + 1];
                            rect.left = pRetDoubles[k * 7 + 2];
                            rect.top = pRetDoubles[k * 7 + 3];
                            rect.width = pRetDoubles[k * 7 + 4];
                            rect.height = pRetDoubles[k * 7 + 5];
                            
                            list_rects.Add(rect);

                            //string msg = string.Format("222222 阵列子单元 {0}: lefttop = [{1:0.000},{2:0.000}], width [{3:0.000},{4:0.000}], angle = {5:0}",
                            //    k + 1, rect.left, rect.top, rect.width, rect.height, rect.odb_angle);
                            //Debugger.Log(0, null, msg);
                        }

                        if (list_rects.Count > 0)
                            list_list_rects.Add(list_rects);
                    }
                }
            }

            // 取左上角第一个作为默认选择的unit
            if (list_list_rects.Count > 0)
            {
                int idx_m = -1;
                int idx_n = -1;
                double min_dist = 10000000;
                for (int n = 0; n < list_list_rects.Count; n++)
                {
                    //Debugger.Log(0, null, string.Format("222222 阵列子单元类型{0} 共包含 {1} 个子单元, name = {2}",
                    //    n + 1, list_list_rects[n].Count, list_list_rects[n][0].strUnitName));
                    
                    for (int m = 0; m < list_list_rects[n].Count; m++)
                    {
                        //Debugger.Log(0, null, string.Format("222222 阵列子单元 {0}: lefttop = [{1:0.000},{2:0.000}], width [{3:0.000},{4:0.000}], angle = {5:0}",
                        //        m + 1, list_list_rects[n][m].left, list_list_rects[n][m].top,
                        //        list_list_rects[n][m].width, list_list_rects[n][m].height, list_list_rects[n][m].odb_angle));

                        if ((list_list_rects[n][m].left + list_list_rects[n][m].top) < min_dist)
                        {
                            idx_m = m;
                            idx_n = n;
                            min_dist = list_list_rects[n][m].left + list_list_rects[n][m].top;
                        }
                    }
                }

                if (idx_m >= 0)
                {
                    list_list_rects[idx_n][idx_m].bSelected = true;
                    list_list_rects[idx_n][idx_m].m_nSelectOrder = 1;
                    rotated_array_rect.m_nSelectCount = 1;
                }
            }
        }

        public void set_path(string path)
        {
            m_strFilePath = path;
        }

        private void OnShown(object sender, EventArgs e)
        {
            m_bLoadGraph = false;
            m_nSelectedStepIndex = -1;
            m_nSelectedLayerIndex = -1;
            
            m_list_step_names.Clear();
            listBox_Steps.Items.Clear();
            listBox_Layers.Items.Clear();

            textBox_GraphFileName.Text = Path.GetFileName(MainUI.m_strGraphFilePath);

            for (int n = 0; n < 9; n++)
            {
                m_thumbnail_views[n].m_bHasValidImage = false;
                m_thumbnail_views[n].render_black_image();
            }

            // 启动DLL通讯线程
            if (false == m_bIsDLLCommunicationStarted)
            {
                m_bIsDLLCommunicationStarted = true;
                Thread thrd3 = new Thread(thread_DLL_communication);
                thrd3.Start();
            }
            
            // 启动进度监控线程
            if (false == m_bIsMonitorStarted)
            {
                m_bIsMonitorStarted = true;
                dl_key_message_notifier messenger = CBD_PostKeyMessage;
                Thread thrd = new Thread(thread_monitor_graph_thumbnail_reading_progress);
                thrd.Start(messenger);
            }

            m_reading_state = GRAPH_READING_STATE.NONE;
            
            // 启动图纸读取线程
            dl_key_message_notifier messenger2 = CBD_PostKeyMessage;
            if (m_strFilePath.EndsWith(".tgz") || m_strFilePath.EndsWith(".rar"))
            {
                m_nGraphType = 0;
                dl_key_message_notifier messenger = CBD_PostKeyMessage;
                Thread thrd = new Thread(thread_read_steps);
                thrd.Start(messenger);
            }
            else
            {
                m_nGraphType = 1;
                Thread thrd2 = new Thread(thread_read_gerber_thumbnail);
                thrd2.Start(messenger2);
            }
            
            // 显示进度条
            m_form_progress = new Form_ProgressInfo(parent);
            if (0 == m_nGraphType)
                m_form_progress.set_infinite_wait_mode(PROGRESS_WAIT_MODE.WAIT_FOR_LOADING_ODB);
            m_form_progress.ShowDialog();
        }
        
        void refresh_orientation_icons()
        {
            pictureBox_F1.Image = Image.FromFile("icons\\方向F_1.bmp");
            pictureBox_F2.Image = Image.FromFile("icons\\方向F_2.bmp");
            pictureBox_F3.Image = Image.FromFile("icons\\方向F_3.bmp");
            pictureBox_F4.Image = Image.FromFile("icons\\方向F_4.bmp");
            pictureBox_F5.Image = Image.FromFile("icons\\方向F_5.bmp");
            pictureBox_F6.Image = Image.FromFile("icons\\方向F_8.bmp");
            pictureBox_F7.Image = Image.FromFile("icons\\方向F_7.bmp");
            pictureBox_F8.Image = Image.FromFile("icons\\方向F_6.bmp");

            if (0 == MainUI.m_nGraphOrientation)
                pictureBox_F1.Image = Image.FromFile("icons\\方向F_1_chosen.bmp");
            else if (1 == MainUI.m_nGraphOrientation)
                pictureBox_F2.Image = Image.FromFile("icons\\方向F_2_chosen.bmp");
            else if (2 == MainUI.m_nGraphOrientation)
                pictureBox_F3.Image = Image.FromFile("icons\\方向F_3_chosen.bmp");
            else if (3 == MainUI.m_nGraphOrientation)
                pictureBox_F4.Image = Image.FromFile("icons\\方向F_4_chosen.bmp");
            else if (4 == MainUI.m_nGraphOrientation)
                pictureBox_F5.Image = Image.FromFile("icons\\方向F_5_chosen.bmp");
            else if (5 == MainUI.m_nGraphOrientation)
                pictureBox_F6.Image = Image.FromFile("icons\\方向F_8_chosen.bmp");
            else if (6 == MainUI.m_nGraphOrientation)
                pictureBox_F7.Image = Image.FromFile("icons\\方向F_7_chosen.bmp");
            else if (7 == MainUI.m_nGraphOrientation)
                pictureBox_F8.Image = Image.FromFile("icons\\方向F_6_chosen.bmp");
        }

        // 清理ODB缓存文件
        private void btn_Clear_ODB_Files_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show(this, "请确认是否清理ODB文件缓存区?", "提示", MessageBoxButtons.YesNo);
            if (DialogResult.Yes == res)
            {
                dl_key_message_notifier messenger = CBD_PostKeyMessage;
                Thread thrd = new Thread(thread_delete_ODB_files);
                thrd.Start(messenger);

                m_form_progress = new Form_ProgressInfo(parent);
                m_form_progress.set_infinite_wait_mode(PROGRESS_WAIT_MODE.NORMAL);
                m_form_progress.label_ProgressInfo.Text = "正在清理ODB缓存文件";
                m_form_progress.ShowDialog();
            }
        }

        private void pictureBox_F1_Click(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 0;
            refresh_orientation_icons();
        }

        private void pictureBox_F2_Click(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 1;
            refresh_orientation_icons();
        }

        private void pictureBox_F3_Click(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 2;
            refresh_orientation_icons();
        }

        private void pictureBox_F4_Click(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 3;
            refresh_orientation_icons();
        }

        private void pictureBox_F5_Click(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 4;
            refresh_orientation_icons();
        }

        private void pictureBox_F6_Click(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 5;
            refresh_orientation_icons();
        }

        private void pictureBox_F7_Click(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 6;
            refresh_orientation_icons();
        }

        private void pictureBox_F8_Click(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 7;
            refresh_orientation_icons();
        }

        private void pictureBox_F1_DoubleClick(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 0;
            m_bLoadGraph = true;
            this.Close();
        }

        private void pictureBox_F2_DoubleClick(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 1;
            m_bLoadGraph = true;
            this.Close();
        }

        private void pictureBox_F3_DoubleClick(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 2;
            m_bLoadGraph = true;
            this.Close();
        }

        private void pictureBox_F4_DoubleClick(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 3;
            m_bLoadGraph = true;
            this.Close();
        }

        private void pictureBox_F5_DoubleClick(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 4;
            m_bLoadGraph = true;
            this.Close();
        }

        private void pictureBox_F6_DoubleClick(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 5;
            m_bLoadGraph = true;
            this.Close();
        }

        private void pictureBox_F7_DoubleClick(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 6;
            m_bLoadGraph = true;
            this.Close();
        }

        private void pictureBox_F8_DoubleClick(object sender, EventArgs e)
        {
            MainUI.m_nGraphOrientation = 7;
            m_bLoadGraph = true;
            this.Close();
        }

        private void OnActivated(object sender, EventArgs e)
        {
            m_bIsFormActivated = true;
        }

        private void OnDeactivated(object sender, EventArgs e)
        {
            m_bIsFormActivated = false;
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            m_bLoadGraph = false;
            this.Close();
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            m_bLoadGraph = true;
            this.Close();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (Form_GraphOrientation.m_bIsReadingGraphForThumbnail)
            {
                e.Cancel = true;
            }
        }

        private void pictureBox_Test_MouseEnter(object sender, EventArgs e)
        {

        }

        private void pictureBox_Test_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox_Test_MouseLeave(object sender, EventArgs e)
        {

        }

        private void pictureBox_Test_Validated(object sender, EventArgs e)
        {

        }

        private void pictureBox_Test_DoubleClick(object sender, EventArgs e)
        {

        }

        private void pictureBox_Test_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void pictureBox_Test_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void listBox_Steps_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
            if (e.Index >= 0)
            {
                StringFormat sStringFormat = new StringFormat();
                sStringFormat.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds, sStringFormat);
            }
            e.DrawFocusRectangle();
        }

        private void listBox_Steps_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 20;
        }
        
        private void listBox_Layers_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
            if (e.Index >= 0)
            {
                StringFormat sStringFormat = new StringFormat();
                sStringFormat.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds, sStringFormat);
            }
            e.DrawFocusRectangle();
        }

        private void listBox_Layers_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 20;
        }

        private void listBox_Steps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_nSelectedStepIndex != listBox_Steps.SelectedIndex)
            {
                m_nSelectedStepIndex = listBox_Steps.SelectedIndex;
                m_nSelectedLayerIndex = -1;

                int[] pRetInts = new int[10];
                pRetInts[0] = 0;
                odb_get_layers_info_for_step(listBox_Steps.Text.ToCharArray(), pRetInts);

                if (1 == pRetInts[0])
                {
                    //string msg = string.Format("222222 SelectedIndex {0}, {1}, 共{2}个layers, layers名称字节总数{3}",
                    //    listBox_Steps.SelectedIndex, listBox_Steps.Text, out_values[1], out_values[2]);
                    //Debugger.Log(0, null, msg);

                    byte[] recv_buf = new byte[pRetInts[2]];
                    int[] string_lens = new int[pRetInts[1]];
                    int[] pRetInts2 = new int[10];
                    odb_copy_layer_names(listBox_Steps.Text.ToCharArray(), recv_buf, string_lens, pRetInts2);

                    int counter = 0;
                    StringBuilder sb = new StringBuilder();
                    string[] strings = new string[pRetInts[1]];
                    for (int n = 0; n < pRetInts[1]; n++)
                    {
                        //msg = string.Format("222222 {0}: string_lens = {1}", n, string_lens[n]);
                        //Debugger.Log(0, null, msg);

                        for (int m = counter; m < counter + string_lens[n]; m++)
                        {
                            sb.Append((char)(recv_buf[m]));
                        }
                        strings[n] = sb.ToString();
                        sb.Clear();

                        counter += (string_lens[n] + 1);
                    }

                    listBox_Layers.Items.Clear();
                    for (int n = 0; n < pRetInts[1]; n++)
                    {
                        //if (strings[n].StartsWith("l") || strings[n].StartsWith("sig"))
                        {
                            listBox_Layers.Items.Add(strings[n]);
                            //msg = string.Format("222222 n {0}: string = {1}", n, strings[n]);
                            //Debugger.Log(0, null, msg);
                        }
                    }
                }
            }
            
        }

        private void listBox_Layers_DoubleClick(object sender, EventArgs e)
        {
            if (m_nSelectedLayerIndex != listBox_Layers.SelectedIndex)
            {
                m_nSelectedLayerIndex = listBox_Layers.SelectedIndex;

                if (m_nSelectedStepIndex >= 0)
                {
                    m_strSelectedStep = listBox_Steps.Text.ToCharArray();
                    m_strSelectedLayer = listBox_Layers.Text.ToCharArray();
                    m_strLayerName = listBox_Layers.Text;
                    
                    dl_key_message_notifier messenger = CBD_PostKeyMessage;
                    Thread thrd = new Thread(thread_parse_and_draw_layers);
                    thrd.Start(messenger);

                    m_form_progress = new Form_ProgressInfo(parent);
                    m_form_progress.set_infinite_wait_mode(PROGRESS_WAIT_MODE.WAIT_FOR_RENDERING);
                    m_form_progress.ShowDialog();
                }
            }

            parent.CBD_SendMessage("获取当前Steps与Layer", false, listBox_Steps.Text, listBox_Layers.Text);
        }

        private void Form_GraphOrientation_Load(object sender, EventArgs e)
        {

        }

        private void btn_BrowseGraph_Click(object sender, EventArgs e)
        {
            if (false == Form_GraphOrientation.m_bIsReadingGraphForThumbnail)
            {
                bool bHasDefaultDir = false;
                if (null != parent.m_strGraphBrowseDir)
                {
                    if ((parent.m_strGraphBrowseDir.Length > 0) && (Directory.Exists(parent.m_strGraphBrowseDir)))
                        bHasDefaultDir = true;
                }

                OpenFileDialog dlg = new OpenFileDialog();
                if (bHasDefaultDir)
                    dlg.InitialDirectory = parent.m_strGraphBrowseDir;
                else
                    dlg.InitialDirectory = ".";
                dlg.Filter = "图纸文件|*.*";
                dlg.ShowDialog();
                if (dlg.FileName != string.Empty)
                {
                    int[] in_values = new int[10];
                    in_values[0] = Form_GraphOrientation.m_thumbnail_views[8].Width;
                    in_values[1] = Form_GraphOrientation.m_thumbnail_views[8].Height;
                    dllapi_set_picture_box_wh_for_ODB(in_values);

                    parent.m_strGraphBrowseDir = System.IO.Path.GetDirectoryName(dlg.FileName);
                    MainUI.m_strGraphFilePath = dlg.FileName;

                    string info = string.Format("图纸信息：{0}", Path.GetFileName(MainUI.m_strGraphFilePath));
                    parent.toolStripStatusLabel_GraphInfo.Text = info;

                    set_path(MainUI.m_strGraphFilePath);
                    OnShown(new object(), new EventArgs());
                }
            }
        }
    }

    public class rotated_array_rect: IComparable<rotated_array_rect>
    {
        public rotated_array_rect()
        {
            strUnitName = "";
            bSelected = false;
            angle = 0;
            odb_angle = 0;
            left = 0;
            top = 0;
            width = 0;
            height = 0;
            score = 0;
        }

        public int CompareTo(rotated_array_rect other)
        {
            if (null == other)
            {
                return 1;                 // 空值比较大，返回1
            }
            return this.m_nSelectOrder.CompareTo(other.m_nSelectOrder);      // 升序
        }

        public rotated_array_rect cloneClass()
        {
            return (rotated_array_rect)this.MemberwiseClone();
        }

        public string strUnitName;
        public bool bSelected;
        public int   m_nSelectOrder = -1;
        public static int   m_nSelectCount = 0;
        public double angle;
        public double odb_angle;
        public double left;
        public double top;
        public double width;
        public double height;
        public float score;
    };
}
