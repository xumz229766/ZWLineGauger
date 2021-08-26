using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PMSGigE = PMSAPI.PMSGigE;
using PMSSTATUS = PMSAPI.PMSSTATUS_CODES;
using PMSImage = PMSAPI.PMSImage;
using CameraHandle = System.Int32;
using MvApi = MVSDK.MvApi;
using MVSDK;

namespace ZWLineGauger
{
    public enum CAMERA_TYPE
    {
        NONE = 0,
        MAIN_CAMERA,
        GUIDE_CAMERA
    }

    public class CameraOps
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();

        MainUI form_parent;
        PictureBox   m_pic_box_container;             // 用于显示相机图像的图像控件容器

        public delegate int   InvokeDraw();

        public bool m_bInitialized = false;

        public string m_config_path = "";

        public double m_saturation = 1;
        public double m_gamma = 1;

        public double m_red = 2;
        public double m_blue = 1;
        public double m_green = 2;
        public double m_gain = 13;
        public double m_exposure = 35;
        public double m_pixels_per_um = 0.1356;                // 像素当量，只对导航相机有效果

        public double MIN_SATURATION = 0.1;
        public double MAX_SATURATION = 5;
        public double MIN_GAMMA = 0.1;
        public double MAX_GAMMA = 5;
        public double MIN_RED = 1;
        public double MAX_RED = 5;
        public double MIN_BLUE = 1;
        public double MAX_BLUE = 5;
        public double MIN_GREEN = 1;
        public double MAX_GREEN = 5;
        public double MIN_GAIN = 1;
        public double MAX_GAIN = 30;
        public double MIN_EXPOSURE = 1;
        public double MAX_EXPOSURE = 255;

        //MindVision
        public int MDVS_MIN_RED = 0;
        public int MDVS_MAX_RED = 400;
        public int MDVS_MIN_GREEN = 0;
        public int MDVS_MAX_GREEN = 400;
        public int MDVS_MIN_BLUE = 0;
        public int MDVS_MAX_BLUE = 400;
        public int MDVS_MIN_GAIN = 0;
        public int MDVS_MAX_GAIN = 400;
        //MindVision 初始化RGB
        /*
        public double MDVS_m_red = 100;
        public double MDVS_m_green = 100;
        public double MDVS_m_blue = 2;
        public double MDVS_m_gain = 13;
        */
        public double m_current_sharpness = 0;           // 当前清晰度
        public double m_current_brightness = 0;          // 当前亮度
        public double m_fps = 0;                                   // 当前帧率
        int m_nPrevTimeStamp = 0;
        int m_nCurrentTimeStamp = 0;

        public byte[]   m_pImageBuf;

        public CAMERA_TYPE m_camera_type = CAMERA_TYPE.NONE;

        int   m_nCamIndex = 0;
        public int m_nCamWidth = 0;
        public int m_nCamHeight = 0;
        IntPtr   m_hCam = IntPtr.Zero;
        IntPtr   m_hImage = IntPtr.Zero;
        PMSAPI.PMS_PixelFormatEnums   m_pixel_format;
        PMSAPI.PMS_SNAPPROC             m_callback_delegate = null;

        IAsyncResult   m_async_res = null;
        InvokeDraw     m_invoke_draw = null;

        CameraHandle              m_hMindVisionCamera = 0;              // 句柄
        tSdkCameraCapbility   m_tCameraCapability;                        // 相机特性描述
        IntPtr                             m_ImageBuffer;                                 // 预览通道RGB图像缓存
        IntPtr                             m_ImageBufferSnapshot;                   // 抓拍通道RGB图像缓存
        Thread                           m_tCaptureThread;                            // 图像抓取线程
        tSdkFrameHead            m_FrameHead;

        public CameraHandle m_Camera_Handle//给外部提供CameraHandle
        {
            get { return m_hMindVisionCamera; }
        }

        public CameraOps(MainUI parent, PictureBox container)
        {
            this.form_parent = parent;
            this.m_pic_box_container = container;
        }

        // 初始化相机参数
        public void load_params(string strConfigFilePath)
        {
            //MVSDK.MvApi.CameraGetCapability(m_hMindVisionCamera, out m_tCameraCapability);

            //Debugger.Log(0, null, string.Format("222222 [{0},{1}]", m_tCameraCapability.sRgbGainRange.));
            
            m_config_path = strConfigFilePath;

            if (File.Exists(m_config_path))
            {
                string content = File.ReadAllText(m_config_path);
                string value = "";

                string str = string.Format("饱和度");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_saturation = Convert.ToDouble(value);

                str = string.Format("伽马值");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_gamma = Convert.ToDouble(value);

                str = string.Format("红");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_red = Convert.ToDouble(value);

                str = string.Format("蓝");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_blue = Convert.ToDouble(value);

                str = string.Format("绿");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_green = Convert.ToDouble(value);

                str = string.Format("增益");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_gain = Convert.ToDouble(value);

                str = string.Format("曝光时间");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_exposure = Convert.ToDouble(value);

                str = string.Format("像素当量");
                if (true == GeneralUtils.GetKeyValue(content, str, ref value))
                    m_pixels_per_um = Convert.ToDouble(value);
            }
            else
            {
                File.Create(m_config_path).Close();
            }

            //Debugger.Log(0, null, string.Format("222222 我要的相机数据{0},{1},{2},{3}", m_red.ToString(), m_green.ToString(), m_blue.ToString(), m_gain.ToString()));


        }

        // 恢复出厂设置
        public void save_default()
        {
            m_saturation = 1;
            m_gamma = 1;
            if (MainUI.m_nCameraType == 0)//Pomeas
            {
            m_red = 2;
            m_blue = 1;
            m_green = 2;
            m_gain = 13;
            }
            else if (MainUI.m_nCameraType == 1)//MindVision
            {
                m_red = 300;
                m_blue = 300;
                m_green = 300;
                m_gain = 300;
            }

            m_exposure = 35;
            m_pixels_per_um = 0.1356;

            save_params();
        }

        // 保存相机参数
        public bool save_params()
        {
            if (m_config_path.Length <= 0)
                return false;

            if (m_exposure < 1)
                m_exposure = 1;
            if (m_exposure > 255)
                m_exposure = 255;

            StreamWriter writer = new StreamWriter(m_config_path, false);
            
            string field = string.Format("饱和度");
            String str = String.Format("{0}={1}", field, m_saturation);
            writer.WriteLine(str);

            field = string.Format("伽马值");
            str = String.Format("{0}={1}", field, m_gamma);
            writer.WriteLine(str);

            field = string.Format("红");
            str = String.Format("{0}={1}", field, m_red);
            writer.WriteLine(str);

            field = string.Format("蓝");
            str = String.Format("{0}={1}", field, m_blue);
            writer.WriteLine(str);

            field = string.Format("绿");
            str = String.Format("{0}={1}", field, m_green);
            writer.WriteLine(str);

            field = string.Format("增益");
            str = String.Format("{0}={1}", field, m_gain);
            writer.WriteLine(str);

            field = string.Format("曝光时间");
            str = String.Format("{0}={1}", field, m_exposure);
            writer.WriteLine(str);
            
            field = string.Format("像素当量");
            str = String.Format("{0}={1}", field, m_pixels_per_um);
            writer.WriteLine(str);
            
            writer.Close();

            return true;
        }

        // 初始化相机
        public bool init_MindVision(CAMERA_TYPE type, int nCamIndex)
        {
            if (m_hMindVisionCamera > 0)
                return true;
            
            CameraSdkStatus status;
            tSdkCameraDevInfo[] tCameraDevInfoList;
            IntPtr ptr;

            try
            {
                status = MvApi.CameraEnumerateDevice(out tCameraDevInfoList);

                Debugger.Log(0, null, string.Format("222222 初始化相机 init_MindVision() status = {0}, nCamIndex = {1}", status, nCamIndex));
                if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    //return false;
                    if (tCameraDevInfoList != null)        // 此时iCameraCounts返回了实际连接的相机个数。如果大于1，则初始化第一个相机
                    {
                        status = MvApi.CameraInit(ref tCameraDevInfoList[nCamIndex], -1, -1, ref m_hMindVisionCamera);
                        
                        Debugger.Log(0, null, string.Format("222222 初始化相机 init_MindVision() status = {0}", status));
                        if (status == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                        {
                            // 获得相机特性描述
                            MvApi.CameraGetCapability(m_hMindVisionCamera, out m_tCameraCapability);

                            // 枚举相机默认分辨率个数
                            print_resolutions();

                            //Debugger.Log(0, null, string.Format("222222 初始化相机 init_MindVision() iWidthMax = {0}, {1}",
                            //    m_tCameraCapability.sResolutionRange.iWidthMax, m_tCameraCapability.sResolutionRange.iHeightMax));

                            m_ImageBuffer = Marshal.AllocHGlobal(
                                m_tCameraCapability.sResolutionRange.iWidthMax * m_tCameraCapability.sResolutionRange.iHeightMax * 3 + 1024);
                            m_ImageBufferSnapshot = Marshal.AllocHGlobal(
                                m_tCameraCapability.sResolutionRange.iWidthMax * m_tCameraCapability.sResolutionRange.iHeightMax * 3 + 1024);
                            
                            // 初始化显示模块，使用SDK内部封装好的显示接口
                            MvApi.CameraDisplayInit(m_hMindVisionCamera, m_pic_box_container.Handle);
                            MvApi.CameraSetDisplaySize(m_hMindVisionCamera, m_pic_box_container.Width, m_pic_box_container.Height);
                            
                            // 设置抓拍通道的分辨率。
                            tSdkImageResolution tResolution;
                            tResolution.uSkipMode = 0;
                            tResolution.uBinAverageMode = 0;
                            tResolution.uBinSumMode = 0;
                            tResolution.uResampleMask = 0;
                            tResolution.iVOffsetFOV = 0;
                            tResolution.iHOffsetFOV = 0;
                            tResolution.iWidthFOV = m_tCameraCapability.sResolutionRange.iWidthMax;
                            tResolution.iHeightFOV = m_tCameraCapability.sResolutionRange.iHeightMax;
                            tResolution.iWidth = tResolution.iWidthFOV;
                            tResolution.iHeight = tResolution.iHeightFOV;
                            //tResolution.iIndex = 0xff;表示自定义分辨率,如果tResolution.iWidth和tResolution.iHeight
                            //定义为0，则表示跟随预览通道的分辨率进行抓拍。抓拍通道的分辨率可以动态更改。
                            //本例中将抓拍分辨率固定为最大分辨率。
                            tResolution.iIndex = 0xff;
                            tResolution.acDescription = new byte[32];//描述信息可以不设置
                            tResolution.iWidthZoomHd = 0;
                            tResolution.iHeightZoomHd = 0;
                            tResolution.iWidthZoomSw = 0;
                            tResolution.iHeightZoomSw = 0;

                            m_nCamWidth = m_tCameraCapability.sResolutionRange.iWidthMax;
                            m_nCamHeight = m_tCameraCapability.sResolutionRange.iHeightMax;

                            MvApi.CameraSetResolutionForSnap(m_hMindVisionCamera, ref tResolution);

                            //让SDK来根据相机的型号动态创建该相机的配置窗口。
                            //MvApi.CameraCreateSettingPage(m_hCamera, this.Handle, tCameraDevInfoList[0].acFriendlyName,/*SettingPageMsgCalBack*/null,/*m_iSettingPageMsgCallbackCtx*/(IntPtr)null, 0);

                            m_bInitialized = true;
                            m_camera_type = type;
                            m_nCamIndex = nCamIndex;

                            m_tCaptureThread = new Thread(new ThreadStart(thread_capture_camera_image_MindVision));
                            m_tCaptureThread.Start();

                            MvApi.CameraPlay(m_hMindVisionCamera);

                            Debugger.Log(0, null, string.Format("222222 初始化相机 init_MindVision() 捕捉线程启动"));

                            //set_gain(10);
                            //Debugger.Log(0, null, string.Format("222222 初始化相机 set_gain(1)"));

                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Debugger.Log(0, null, string.Format("222222 初始化相机 init_MindVision() 异常信息：{0}", ex.Message));
            }

            return true;
        }

        // 初始化相机
        public bool init_PMS(CAMERA_TYPE type, int nCamIndex)
        {
            bool bProceed = true;
            PMSSTATUS result = PMSGigE.PMSOpenCamByIndex((byte)nCamIndex, out m_hCam);

            if (m_hCam == IntPtr.Zero)
            {
                if (result == PMSSTATUS.PMSST_ACCESS_DENIED)
                {
                    MessageBox.Show(null, "无法打开相机，可能正被其它软件控制，请检查。", "提示");
                    bProceed = false;
                }
            }

            if (true == bProceed)
            {
                if (false == create_cam_image(ref m_hCam, ref m_hImage, nCamIndex))
                {
                    MessageBox.Show(null, "无法创建相机图像，请检查原因。", "提示");
                    bProceed = false;
                }
            }

            if (true == bProceed)
            {
                PMSGigE.PMSSetStrobeSource(m_hCam, PMSAPI.LineSourceEnums.LineSource_ExposureActive);

                // 设置相机参数
                set_gain(m_gain);
                set_exposure(m_exposure);

                m_callback_delegate += new PMSAPI.PMS_SNAPPROC(stream_callback);
                PMSGigE.PMSSetTriggerMode(m_hCam, PMSAPI.TriggerModeEnums.TriggerMode_Off);
                result = PMSGigE.PMSStartGrab(m_hCam, m_callback_delegate, form_parent.Handle);

                if (PMSSTATUS.PMSST_SUCCESS == result)
                {
                    m_bInitialized = true;
                    m_camera_type = type;
                    m_nCamIndex = nCamIndex;

                    Debugger.Log(0, null, "222222 相机初始化成功");
                }
            }
            
            return true;
        }

        // 释放资源
        public void release()
        {
            if (0 == MainUI.m_nCameraType)
            {
                PMSGigE.PMSSetTriggerMode(m_hCam, PMSAPI.TriggerModeEnums.TriggerMode_Off);
                PMSGigE.PMSCloseCam(m_hCam);
                PMSImage.PMSImageRelease(m_hImage);
            }
            else if (1 == MainUI.m_nCameraType)
            {
                MvApi.CameraUnInit(m_hMindVisionCamera);
            }
        }

        // 创建相机图像
        private bool create_cam_image(ref IntPtr hCam, ref IntPtr hImage, int nCamIdx)
        {
            int   w = 0, h = 0;

            PMSSTATUS r = PMSGigE.PMSGetWidth(hCam, out w);
            if (r != PMSSTATUS.PMSST_SUCCESS)
            {
                MessageBox.Show("获取图像宽度失败");
                return false;
            }

            r = PMSGigE.PMSGetHeight(hCam, out h);
            if (r != PMSSTATUS.PMSST_SUCCESS)
            {
                MessageBox.Show("获取取得图像高度失败");
                return false;
            }
            r = PMSGigE.PMSGetPixelFormat(hCam, out m_pixel_format);
            if (r != PMSSTATUS.PMSST_SUCCESS)
            {
                MessageBox.Show("获取取得图像颜色模式失败");
                return false;
            }
            
            if (m_nCamWidth != w || m_nCamHeight != h)
            {
                if (hImage != IntPtr.Zero)
                {
                    PMSAPI.PMSImage.PMSImageRelease(hImage);
                    hImage = IntPtr.Zero;
                }

                if (m_pixel_format == PMSAPI.PMS_PixelFormatEnums.PixelFormat_Mono8)
                    hImage = PMSAPI.PMSImage.PMSImageCreate(w, h, 8);
                else
                    hImage = PMSAPI.PMSImage.PMSImageCreate(w, h, 24);

                m_nCamWidth = w;
                m_nCamHeight = h;
            }

            return true;
        }

        public void thread_capture_camera_image_MindVision()
        {
            CameraSdkStatus eStatus;
            IntPtr uRawBuffer;//rawbuffer由SDK内部申请。应用层不要调用delete之类的释放函数
            
            while (false == MainUI.m_bExitProgram)
            {
                Thread.Sleep(30);
                if (1 == m_nCamIndex)
                    Thread.Sleep(20);
                if (true == form_parent.m_bFreezeCameraImage)
                    continue;
                //if (0 == m_nCamIndex)
                //    Debugger.Log(0, null, string.Format("222222 MindVision() 222"));

                //500毫秒超时,图像没捕获到前，线程会被挂起,释放CPU，所以该线程中无需调用sleep
                eStatus = MvApi.CameraGetImageBuffer(m_hMindVisionCamera, out m_FrameHead, out uRawBuffer, 500);
                //if (0 == m_nCamIndex)
                //    Debugger.Log(0, null, string.Format("222222 333"));

                if (CameraSdkStatus.CAMERA_STATUS_SUCCESS == eStatus)          // 如果是触发模式，则有可能超时
                {
                    // 图像处理，将原始输出转换为RGB格式的位图数据，同时叠加白平衡、饱和度、LUT等ISP处理。
                    MvApi.CameraImageProcess(m_hMindVisionCamera, uRawBuffer, m_ImageBuffer, ref m_FrameHead);
                    
                    // 叠加十字线、自动曝光窗口、白平衡窗口信息(仅叠加设置为可见状态的)。    
                    MvApi.CameraImageOverlay(m_hMindVisionCamera, m_ImageBuffer, ref m_FrameHead);
                    
                    // 调用SDK封装好的接口，显示预览图像
                    MvApi.CameraDisplayRGB24(m_hMindVisionCamera, m_ImageBuffer, ref m_FrameHead);
                    // 成功调用CameraGetImageBuffer后必须释放，下次才能继续调用CameraGetImageBuffer捕获图像。
                    MvApi.CameraReleaseImageBuffer(m_hMindVisionCamera, uRawBuffer);
                    
                    int nBytesPerLine = (m_FrameHead.iWidth * 24) / 8;
                    //Debugger.Log(0, null, string.Format("222222 m_nCamIndex {0} 111", m_nCamIndex));
                    //Debugger.Log(0, null, string.Format("222222 初始化相机 init_MindVision() FrameHead.iWidth = [{0},{1}]",
                    //    FrameHead.iWidth, FrameHead.iHeight));

                    if (null == m_pImageBuf)
                        m_pImageBuf = new byte[nBytesPerLine * m_FrameHead.iHeight];
                    
                    refresh_image_control_for_MindVision();
                    
                    m_nCurrentTimeStamp = GetTickCount();
                    if ((m_nPrevTimeStamp != 0) && ((m_nCurrentTimeStamp - m_nPrevTimeStamp) > 0))
                    {
                        m_fps = 1000 / (double)(m_nCurrentTimeStamp - m_nPrevTimeStamp);
                    }
                    m_nPrevTimeStamp = m_nCurrentTimeStamp;

                    //switch (m_nCamIndex)
                    //{
                    //    case 0:
                    //        lock (form_parent.m_main_cam_lock)
                    //        {
                    //            if (null != m_ImageBuffer)
                    //            {
                    //                //Debugger.Log(0, null, string.Format("222222 m_nCamIndex {0} 222", m_nCamIndex));
                    //                System.Runtime.InteropServices.Marshal.Copy(m_ImageBuffer, m_pImageBuf, 0, nBytesPerLine * FrameHead.iHeight);
                    //                //Debugger.Log(0, null, string.Format("222222 m_nCamIndex {0} 333", m_nCamIndex));
                    //                Bitmap bmp = new Bitmap(FrameHead.iWidth, FrameHead.iHeight, PixelFormat.Format24bppRgb);
                    //                Rectangle rect = new Rectangle(new Point(0, 0), bmp.Size);
                    //                BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                    //                System.Runtime.InteropServices.Marshal.Copy(m_pImageBuf, 0, bmp_data.Scan0, bmp_data.Stride * bmp_data.Height);
                    //                bmp.UnlockBits(bmp_data);
                    //                //Debugger.Log(0, null, string.Format("222222 m_nCamIndex {0} 444", m_nCamIndex));

                    //                m_pic_box_container.Image = bmp;
                    //            }
                    //        }
                    //        break;

                    //    case 1:
                    //        lock (form_parent.m_guide_cam_lock)
                    //        {
                    //            if (null != m_ImageBuffer)
                    //            {
                    //                System.Runtime.InteropServices.Marshal.Copy(m_ImageBuffer, m_pImageBuf, 0, nBytesPerLine * FrameHead.iHeight);

                    //                Bitmap bmp = new Bitmap(FrameHead.iWidth, FrameHead.iHeight, PixelFormat.Format24bppRgb);
                    //                Rectangle rect = new Rectangle(new Point(0, 0), bmp.Size);
                    //                BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                    //                System.Runtime.InteropServices.Marshal.Copy(m_pImageBuf, 0, bmp_data.Scan0, bmp_data.Stride * bmp_data.Height);
                    //                bmp.UnlockBits(bmp_data);

                    //                m_pic_box_container.Image = bmp;
                    //            }
                    //        }
                    //        break;
                    //}
                    //Debugger.Log(0, null, string.Format("222222 m_nCamIndex {0} 555", m_nCamIndex));
                    //if (FrameHead.iWidth != m_tFrameHead.iWidth || FrameHead.iHeight != m_tFrameHead.iHeight)
                    //{
                    //    m_bEraseBk = true;
                    //    m_tFrameHead = FrameHead;
                    //}

                    //MvApi.CameraSaveImage(m_hMindVisionCamera, "0.bmp", m_ImageBuffer, ref FrameHead, emSdkFileType.FILE_BMP, 100);

                    //Bitmap bmp = (Bitmap)(Bitmap.FromFile("0.bmp"));
                    //m_pic_box_container.Image = bmp;
                }
                else
                    Debugger.Log(0, null, string.Format("222222 初始化相机 init_MindVision() 捕捉线程 捕获图像结果 = {0}, m_nCamIndex = {1}", eStatus, m_nCamIndex));
            }
        }

        int   refresh_image_control_for_MindVision()
        {
            //Debugger.Log(0, null, string.Format("222222 form_parent.InvokeRequired = {0}", form_parent.InvokeRequired));
            if (form_parent.InvokeRequired)
            {
                if (m_async_res == null)
                {
                    m_invoke_draw = refresh_image_control_for_MindVision;
                    m_async_res = form_parent.BeginInvoke(m_invoke_draw);
                }
                else if (m_async_res.IsCompleted)
                {
                    m_invoke_draw = refresh_image_control_for_MindVision;
                    form_parent.EndInvoke(m_async_res);
                    m_async_res = form_parent.BeginInvoke(m_invoke_draw);
                }
                return 0;
            }

            int nBytesPerLine = (m_FrameHead.iWidth * 24) / 8;
            
            switch (m_nCamIndex)
            {
                case 0:
                    lock (form_parent.m_main_cam_lock)
                    {
                        if (null != m_ImageBuffer)
                        {
                            System.Runtime.InteropServices.Marshal.Copy(m_ImageBuffer, m_pImageBuf, 0, nBytesPerLine * m_FrameHead.iHeight);

                            Bitmap bmp = new Bitmap(m_FrameHead.iWidth, m_FrameHead.iHeight, PixelFormat.Format24bppRgb);
                            Rectangle rect = new Rectangle(new Point(0, 0), bmp.Size);
                            BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                            System.Runtime.InteropServices.Marshal.Copy(m_pImageBuf, 0, bmp_data.Scan0, bmp_data.Stride * bmp_data.Height);
                            bmp.UnlockBits(bmp_data);

                            bmp.RotateFlip(RotateFlipType.Rotate180FlipX);

                            m_pic_box_container.Image = bmp;
                        }
                    }
                    break;

                case 1:
                    lock (form_parent.m_guide_cam_lock)
                    {
                        if (null != m_ImageBuffer)
                        {
                            System.Runtime.InteropServices.Marshal.Copy(m_ImageBuffer, m_pImageBuf, 0, nBytesPerLine * m_FrameHead.iHeight);

                            Bitmap bmp = new Bitmap(m_FrameHead.iWidth, m_FrameHead.iHeight, PixelFormat.Format24bppRgb);
                            Rectangle rect = new Rectangle(new Point(0, 0), bmp.Size);
                            BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                            System.Runtime.InteropServices.Marshal.Copy(m_pImageBuf, 0, bmp_data.Scan0, bmp_data.Stride * bmp_data.Height);
                            bmp.UnlockBits(bmp_data);

                            m_pic_box_container.Image = bmp;
                        }
                    }
                    break;
            }

            return 0;
        }

        // 通过回调方式，获取相机图像
        int stream_callback(ref PMSAPI.IMAGE_INFO pInfo, IntPtr UserVal)
        {
            if (true == form_parent.m_bFreezeCameraImage)
                return 0;
            if (true == MainUI.m_bExitProgram)
                return 0;

            PMSGigE.PMSInfo2Image(m_hCam, ref pInfo, m_hImage);
            draw_image();

            m_nCurrentTimeStamp = GetTickCount();
            if ((m_nPrevTimeStamp > 0) && ((m_nCurrentTimeStamp - m_nPrevTimeStamp) > 0))
            {
                m_fps = 1000 / (double)(m_nCurrentTimeStamp - m_nPrevTimeStamp);
            }
            m_nPrevTimeStamp = m_nCurrentTimeStamp;

            return 0;
        }

        // 将相机图像绘制到图像控件
        int draw_image()
        {
            if (form_parent.InvokeRequired)
            {
                if (m_async_res == null)
                {
                    m_invoke_draw = draw_image;
                    m_async_res = form_parent.BeginInvoke(m_invoke_draw);
                }
                else if (m_async_res.IsCompleted)
                {
                    m_invoke_draw = draw_image;
                    form_parent.EndInvoke(m_async_res);
                    m_async_res = form_parent.BeginInvoke(m_invoke_draw);
                }
                return 0;
            }

            if (m_hImage != IntPtr.Zero)
            {
                int nBits = PMSImage.PMSImageGetBPP(m_hImage);
                int nBytesPerLine = (m_nCamWidth * nBits) / 8;

                if (null == m_pImageBuf)
                    m_pImageBuf = new byte[nBytesPerLine * m_nCamHeight];

                IntPtr source = PMSImage.PMSImageGetBits(m_hImage);

                if (true == MainUI.m_bExitProgram)
                    return 0;

                switch (m_nCamIndex)
                {
                    case 0:
                        lock (form_parent.m_main_cam_lock)
                        {
                            if (null != source)
                            {
                                System.Runtime.InteropServices.Marshal.Copy(source, m_pImageBuf, 0, nBytesPerLine * m_nCamHeight);

                                Bitmap bmp = new Bitmap(m_nCamWidth, m_nCamHeight, PixelFormat.Format24bppRgb);
                                Rectangle rect = new Rectangle(new Point(0, 0), bmp.Size);
                                BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                                System.Runtime.InteropServices.Marshal.Copy(m_pImageBuf, 0, bmp_data.Scan0, bmp_data.Stride * bmp_data.Height);
                                bmp.UnlockBits(bmp_data);

                                m_pic_box_container.Image = bmp;
                            }
                        }

                        break;

                    case 1:
                        lock (form_parent.m_guide_cam_lock)
                        {
                            if (null != source)
                            {
                                System.Runtime.InteropServices.Marshal.Copy(source, m_pImageBuf, 0, nBytesPerLine * m_nCamHeight);

                                Bitmap bmp = new Bitmap(m_nCamWidth, m_nCamHeight, PixelFormat.Format24bppRgb);
                                Rectangle rect = new Rectangle(new Point(0, 0), bmp.Size);
                                BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                                System.Runtime.InteropServices.Marshal.Copy(m_pImageBuf, 0, bmp_data.Scan0, bmp_data.Stride * bmp_data.Height);
                                bmp.UnlockBits(bmp_data);

                                m_pic_box_container.Image = bmp;
                            }
                        }
                        
                        break;
                }
            }
            return 0;
        }

        // 枚举相机支持的分辨率
        public void print_resolutions()
        {
            if (0 == MainUI.m_nCameraType)
            {
            }
            else if (1 == MainUI.m_nCameraType)
            {
                Debugger.Log(0, null, string.Format("222222 迈德威视相机默认分辨率个数 = {0}", m_tCameraCapability.iImageSizeDesc));
            }
        }

        // 设置红色通道
        public void set_red(double value)
        {
            if (value < MIN_RED)
                value = MIN_RED;
            if (value > MAX_RED)
                value = MAX_RED;

            double red, blue, green;
            PMSGigE.PMSGetWhiteBalance(m_hCam, out red, out blue, out green);

            Debugger.Log(0, null, string.Format("222222 red = {0}", red));

            PMSGigE.PMSSetWhiteBalance(m_hCam, value, blue, green);
        }

        //MDVS设置红色通道
        public void MDVS_set_red(int value)
        {
            if (value < MDVS_MIN_RED)
                value = MDVS_MIN_RED;
            if (value > MDVS_MAX_RED)
                value = MDVS_MAX_RED;

            //Debugger.Log(0, null, string.Format("222222  [{0}]", value));

            int red = 0, blue = 0, green = 0;
            //PMSGigE.PMSGetWhiteBalance(m_hCam, out red, out blue, out green);

            //PMSGigE.PMSSetWhiteBalance(m_hCam, value, blue, green);
            MVSDK.MvApi.CameraGetGain(m_hMindVisionCamera, ref red, ref green, ref blue);
            //Debugger.Log(0, null, string.Format("222222  [{0},{1},{2}]", red, green, blue));
            MVSDK.MvApi.CameraSetGain(m_hMindVisionCamera, value, green, blue);
            //Debugger.Log(0, null, string.Format("222222  [{0},{1},{2}]", value, green, blue));
        }

        // 设置蓝色通道
        public void set_blue(double value)
        {
            if (value < MIN_BLUE)
                value = MIN_BLUE;
            if (value > MAX_BLUE)
                value = MAX_BLUE;

            double red, blue, green;
            PMSGigE.PMSGetWhiteBalance(m_hCam, out red, out green, out blue);

            PMSGigE.PMSSetWhiteBalance(m_hCam, red, green, value);
        }

        //MDVS设置蓝色通道
        public void MDVS_set_blue(int value)
        {
            if (value < MDVS_MIN_RED)
                value = MDVS_MIN_RED;
            if (value > MDVS_MAX_RED)
                value = MDVS_MAX_RED;

            //Debugger.Log(0, null, string.Format("222222  [{0}]", value));

            int red = 0, blue = 0, green = 0;
            //PMSGigE.PMSGetWhiteBalance(m_hCam, out red, out blue, out green);

            //PMSGigE.PMSSetWhiteBalance(m_hCam, value, blue, green);
            MVSDK.MvApi.CameraGetGain(m_hMindVisionCamera, ref red, ref green, ref blue);
            //Debugger.Log(0, null, string.Format("222222  [{0},{1},{2}]", red, green, blue));
            MVSDK.MvApi.CameraSetGain(m_hMindVisionCamera, red, green, value);
            //Debugger.Log(0, null, string.Format("222222  [{0},{1},{2}]", red, green, value));
        }

        // 设置绿色通道
        public void set_green(double value)
        {
            if (value < MIN_GREEN)
                value = MIN_GREEN;
            if (value > MAX_GREEN)
                value = MAX_GREEN;

            double red, blue, green;
            PMSGigE.PMSGetWhiteBalance(m_hCam, out red, out green, out blue);

            PMSGigE.PMSSetWhiteBalance(m_hCam, red, value, blue);
        }

        //MDVS设置绿色通道
        public void MDVS_set_green(int value)
        {

            if (value < MDVS_MIN_RED)
                value = MDVS_MIN_RED;
            if (value > MDVS_MAX_RED)
                value = MDVS_MAX_RED;

            //Debugger.Log(0, null, string.Format("222222  [{0}]", value));

            int red = 0, blue = 0, green = 0;
            //PMSGigE.PMSGetWhiteBalance(m_hCam, out red, out blue, out green);

            //PMSGigE.PMSSetWhiteBalance(m_hCam, value, blue, green);
            MVSDK.MvApi.CameraGetGain(m_hMindVisionCamera, ref red, ref green, ref blue);
            //Debugger.Log(0, null, string.Format("222222  [{0},{1},{2}]", red, green, blue));
            MVSDK.MvApi.CameraSetGain(m_hMindVisionCamera, red, value, blue);
            //Debugger.Log(0, null, string.Format("222222  [{0},{1},{2}]", red, value, blue));
        }

        // 设置增益
        public void set_gain(double gain)
        {
            if (0 == MainUI.m_nCameraType)
            {
            if (gain < 1)
                gain = 1;
            if (gain > 255)
                gain = 255;
                PMSAPI.PMSSTATUS_CODES status = PMSGigE.PMSSetGain(m_hCam, gain);
            }
            else if (1 == MainUI.m_nCameraType)
            {
                if (gain < 0)
                    gain = 0;
                if (gain > 400)
                    gain = 400;
                MvApi.CameraSetGain(m_hMindVisionCamera, (int)gain, (int)gain, (int)gain);
            }
        }
        
        // 设置曝光
        public void set_exposure(double exposure)
        {
            if (exposure < 1)
                exposure = 1;
            if (exposure > 255)
                exposure = 255;

            if (0 == MainUI.m_nCameraType)
            {
                PMSGigE.PMSSetExposureTime(m_hCam, exposure * 1000);
            }
            else if (1 == MainUI.m_nCameraType)
            {
                MvApi.CameraSetExposureTime(m_hMindVisionCamera, exposure * 1000);
            }
        }
    }
}
