using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Drawing.Imaging;
using System.Windows.Forms;
using HalconDotNet;

namespace ZWLineGauger.Gaugers
{
    public class ImgOperators
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();

        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool get_image_sharpness(byte[] in_bytes, int[] in_data, int[] out_ints, double[] out_doubles);

        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool get_image_brightness(byte[] in_bytes, int[] in_data, int[] out_ints, double[] out_doubles);

        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool find_circle_in_image(byte[] in_bytes, int[] in_data, double[] in_doubles, int[] out_ints, double[] out_doubles);

        [DllImport("pxflow.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        unsafe extern static bool find_circle_in_image_hv(byte[] in_bytes, int[] in_data, double[] in_doubles, int[] out_ints, double[] out_doubles);

        [DllImport("pxflow.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool get_max_line_contrast_with_angle_in_ROI(byte[] in_bytes, int[] in_data, double[] in_doubles, int[] out_ints, double[] out_doubles);

        MainUI parent;

        static MotionOps   m_motion;

        int   m_nAutoFocusSpeed = 1;
        bool   m_bEnableAutofocusOffset = true;
        double   m_bottomline_autofocus_offset = 1;

        public bool   m_bFastFocus = false;
        public MeasurePointData   m_mes_data;

        public ImgOperators(MainUI form, MotionOps motion)
        {
            this.parent = form;
            m_motion = motion;
        }

        public static HImage convert_bitmap_to_HImage_24bpp(Bitmap bitmap)
        {
            if (PixelFormat.Format24bppRgb != bitmap.PixelFormat)
                return null;
            
            HObject hObj;
            HOperatorSet.GenEmptyObj(out hObj);
            
            unsafe
            {
                BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);
                unsafe
                {
                    HOperatorSet.GenImageInterleaved(out hObj, bmpData.Scan0, "bgr", bitmap.Width, bitmap.Height, -1, "byte",
                        bitmap.Width, bitmap.Height, 0, 0, -1, 0);
                }
            }

            HImage hImg = new HImage();
            HTuple type, width, height, pointerRed, pointerGreen, pointerBlue;
            HOperatorSet.GetImagePointer3(hObj, out pointerRed, out pointerGreen, out pointerBlue,
                           out type, out width, out height);
            hImg.GenImage3(type, width, height, pointerRed, pointerGreen, pointerBlue);
            
            return hImg;
        }

        public static HImage convert_HObject_to_HImage_24bpp(HObject hObj)
        {
            HImage hImg = new HImage();
            HTuple type, width, height, pointerRed, pointerGreen, pointerBlue;
            HOperatorSet.GetImagePointer3(hObj, out pointerRed, out pointerGreen, out pointerBlue,
                           out type, out width, out height);
            hImg.GenImage3(type, width, height, pointerRed, pointerGreen, pointerBlue);

            return hImg;
        }

        public bool locate_corner_under_ratio(CameraOps cam, int nRatioIdx, Point2d left_top, Point2d right_bottom, ref Point2d corner_crd)
        {
            Bitmap bmp;
            lock (parent.m_main_cam_lock)
            {
                if (false == parent.m_bOfflineMode)
                {
                    bmp = new Bitmap(parent.m_main_camera.m_nCamWidth, parent.m_main_camera.m_nCamHeight, PixelFormat.Format24bppRgb);
                    Rectangle rect = new Rectangle(new Point(0, 0), bmp.Size);
                    BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                    System.Runtime.InteropServices.Marshal.Copy(parent.m_main_camera.m_pImageBuf, 0, bmp_data.Scan0, bmp_data.Stride * bmp_data.Height);
                    bmp.UnlockBits(bmp_data);
                }
                else
                    bmp = (Bitmap)parent.ui_MainImage.Image.Clone();
            }
            
            // halcon 运算开始
            HImage hSourceImg = Gaugers.ImgOperators.convert_bitmap_to_HImage_24bpp(bmp);
            if (null == hSourceImg)
                return false;
            
            double contrast_thres = 50;
            double contour_length_thres = 60;
            HImage hImgReduced = new HImage();
            HImage hRoi = new HImage();
            HObject hoEdges = new HObject();
            HObject hoSelectedEdges = new HObject();
            HObject hoUnitedContours = new HObject();
            HObject hoSelectedContours = new HObject();
            HObject hoPolygons = new HObject();
            HObject hoSplitContours = new HObject();
            HRegion hRegion = new HRegion();
            HTuple htNum = new HTuple();
            HTuple htWindowHandle = new HTuple();

            hRegion.GenRectangle1(left_top.y, left_top.x, right_bottom.y, right_bottom.x);
            hImgReduced = hSourceImg.ReduceDomain(hRegion);
            hRoi = hImgReduced.CropDomain();
            
            HOperatorSet.EdgesSubPix(hRoi, out hoEdges, "canny", 3, contrast_thres - 5, contrast_thres);
            HOperatorSet.SelectContoursXld(hoEdges, out hoSelectedEdges, "contour_length", contour_length_thres, 100000000, 0, 0);
            
            HOperatorSet.UnionCollinearContoursExtXld(hoSelectedEdges, out hoUnitedContours, 10, 1, 5, 0.1, 0, -1, 1, 1, 1, 1, 1, 0, "attr_keep");
            HOperatorSet.SelectContoursXld(hoUnitedContours, out hoSelectedContours, "contour_length", contour_length_thres * 2, 100000000, 0, 0);

            HOperatorSet.GenPolygonsXld(hoSelectedContours, out hoPolygons, "ramer", 2);
            HOperatorSet.SplitContoursXld(hoPolygons, out hoSplitContours, "dominant", 2, 20);
            HOperatorSet.SelectContoursXld(hoSplitContours, out hoSelectedContours, "contour_length", contour_length_thres * 2, 100000000, 0, 0);
            
            HOperatorSet.CountObj(hoSelectedContours, out htNum);
            //Debugger.Log(0, null, string.Format("222222 hoSelectedContours htNum = {0}", htNum));

            // 输出到窗口
            #region
            if (false)
            {
                //HOperatorSet.OpenWindow(0, 0, parent.ui_MainImage.Width, parent.ui_MainImage.Height, parent.ui_MainImage.Handle, "visible", "", out htWindowHandle);
                HOperatorSet.OpenWindow(0, 0, parent.ui_MainImage.Width, parent.ui_MainImage.Height, 0, "visible", "", out htWindowHandle);
                HOperatorSet.SetColor(htWindowHandle, "green");
                HOperatorSet.DispObj(hSourceImg, htWindowHandle);

                HTuple htHomMat2D;
                HOperatorSet.VectorAngleToRigid(0, 0, 0, left_top.y, left_top.x, 0, out htHomMat2D);

                HOperatorSet.AffineTransContourXld(hoEdges, out hoEdges, htHomMat2D);
                HOperatorSet.DispObj(hoEdges, htWindowHandle);
                Thread.Sleep(3000);
                
            }
            #endregion
            
            if (2 == htNum)
            {
                //hRoi.WriteImage("bmp", 0, "hRoi.bmp");

                HTuple htWidth, htHeight;
                hRoi.GetImageSize(out htWidth, out htHeight);

                HTuple htRowBegin, htColumnBegin, htRowEnd, htColumnEnd, nr, nc, dist;
                HOperatorSet.FitLineContourXld(hoSelectedContours, "tukey", -1, 0, 5, 2,
                    out htRowBegin, out htColumnBegin, out htRowEnd, out htColumnEnd, out nr, out nc, out dist);
                
                HTuple htHomMat2D;
                HTuple htLine1Rows = new HTuple();
                HTuple htLine1Columns = new HTuple();
                HTuple htLine2Rows = new HTuple();
                HTuple htLine2Columns = new HTuple();
                #region
                if (2 == htRowBegin.Length)
                {
                    for (int n = 0; n < 2; n++)
                    {
                        HTuple rad;
                        HTuple width = 20.0;
                        HTuple Qx, Qy;

                        //HOperatorSet.AngleLx(htRowEnd[n].D, htColumnEnd[n].D, htRowBegin[n].D, htColumnBegin[n].D, out rad);

                        //HOperatorSet.VectorAngleToRigid(0, 0, 0, htRowBegin[n], htColumnBegin[n], rad, out htHomMat2D);
                        //HOperatorSet.AffineTransPoint2d(htHomMat2D, -width, 0, out Qx, out Qy);

                        //Debugger.Log(0, null, string.Format("222222 {0}: {1}", n, htRowBegin[n].D));
                        //Debugger.Log(0, null, string.Format("222222 {0}: Qx [{1},{2}]", n, Qx, Qy));

                        HTuple htMetrologyHandle;
                        HTuple htIndex;
                        HTuple htShapeParam = new HTuple();
                        htShapeParam = htShapeParam.TupleConcat(htRowBegin[n]);
                        htShapeParam = htShapeParam.TupleConcat(htColumnBegin[n]);
                        htShapeParam = htShapeParam.TupleConcat(htRowEnd[n]);
                        htShapeParam = htShapeParam.TupleConcat(htColumnEnd[n]);
                        
                        HOperatorSet.CreateMetrologyModel(out htMetrologyHandle);
                        HOperatorSet.SetMetrologyModelImageSize(htMetrologyHandle, htWidth, htHeight);
                        
                        HOperatorSet.AddMetrologyObjectGeneric(htMetrologyHandle, "line", htShapeParam, width, 5, 3, 50, 
                            new HTuple("measure_select", "measure_transition", "min_score"), new HTuple("all", "all", 0.3), out htIndex);

                        HOperatorSet.ApplyMetrologyModel(hRoi, htMetrologyHandle);

                        HObject hoMeasureContour, hoCrossContour, hoLineContour;
                        HTuple htEdgesX, htEdgesY;
                        HOperatorSet.GetMetrologyObjectMeasures(out hoMeasureContour, htMetrologyHandle, htIndex, "all", out htEdgesY, out htEdgesX);
                        HOperatorSet.GenCrossContourXld(out hoCrossContour, htEdgesY, htEdgesX, 10, 0.78);
                        HOperatorSet.GenContourPolygonXld(out hoLineContour, htEdgesY, htEdgesX);
                        
                        HTuple htLineBeginY, htLineBeginX, htLineEndY, htLineEndX, nr2, nc2, dist2;
                        HOperatorSet.FitLineContourXld(hoLineContour, "tukey", -1, 0, 5, 2,
                            out htLineBeginY, out htLineBeginX, out htLineEndY, out htLineEndX, out nr2, out nc2, out dist2);

                        if (1 == htLineBeginY.Length)
                        {
                            if (0 == n)
                            {
                                HOperatorSet.TupleConcat(htLine1Rows, htLineBeginY, out htLine1Rows);
                                HOperatorSet.TupleConcat(htLine1Columns, htLineBeginX, out htLine1Columns);
                                HOperatorSet.TupleConcat(htLine1Rows, htLineEndY, out htLine1Rows);
                                HOperatorSet.TupleConcat(htLine1Columns, htLineEndX, out htLine1Columns);
                            }
                            else if (1 == n)
                            {
                                HOperatorSet.TupleConcat(htLine2Rows, htLineBeginY, out htLine2Rows);
                                HOperatorSet.TupleConcat(htLine2Columns, htLineBeginX, out htLine2Columns);
                                HOperatorSet.TupleConcat(htLine2Rows, htLineEndY, out htLine2Rows);
                                HOperatorSet.TupleConcat(htLine2Columns, htLineEndX, out htLine2Columns);
                            }
                        }

                        HOperatorSet.ClearMetrologyModel(htMetrologyHandle);
                    }
                }
                #endregion
                
                if (4 == (htLine1Rows.Length + htLine2Rows.Length))
                {
                    HTuple htIntersectX, htIntersectY, htOverlap;
                    HOperatorSet.IntersectionLines(htLine1Rows[0], htLine1Columns[0], htLine1Rows[1], htLine1Columns[1],
                        htLine2Rows[0], htLine2Columns[0], htLine2Rows[1], htLine2Columns[1], out htIntersectY, out htIntersectX, out htOverlap);

                    corner_crd.x = htIntersectX + left_top.x;
                    corner_crd.y = htIntersectY + left_top.y;

                    // 输出到图片
                    #region
                    if (false)
                    {
                        try
                        {
                            HObject hoDumpedImage;
                            HObject hoCircle;
                            HObject hoShowContours;
                            HImage hiDumpedImg;

                            HOperatorSet.OpenWindow(0, 0, bmp.Width, bmp.Height, 0, "buffer", "", out htWindowHandle);
                            HOperatorSet.SetColor(htWindowHandle, "green");
                            HOperatorSet.DispObj(hSourceImg, htWindowHandle);

                            HOperatorSet.GenCircleContourXld(out hoCircle, htIntersectY, htIntersectX, 5, 0, 6.28318, "positive", 1.0);

                            HOperatorSet.VectorAngleToRigid(0, 0, 0, left_top.y, left_top.x, 0, out htHomMat2D);
                            HOperatorSet.AffineTransContourXld(hoCircle, out hoShowContours, htHomMat2D);
                            //HOperatorSet.AffineTransPolygonXld(hoPolygons, out hoShowContours, htHomMat2D);
                            HOperatorSet.DispObj(hoShowContours, htWindowHandle);

                            HOperatorSet.DumpWindowImage(out hoDumpedImage, htWindowHandle);
                            hiDumpedImg = convert_HObject_to_HImage_24bpp(hoDumpedImage);
                            //hiDumpedImg.WriteImage("bmp", 0, "hiDumpedImg.bmp");
                        }
                        catch (Exception ex)
                        {
                            Debugger.Log(0, null, string.Format("222222 locate_corner_under_ratio(): exception \"{0}\"", ex.Message));
                        }
                    }
                    #endregion

                    return true;
                }
            }
            
            return false;
        }

        // 计算图像清晰度
        public static bool get_image_sharpness(Image img, ref double sharpness)
        {
            int nStride = 0;
            byte[] pBuf = GeneralUtils.convert_image_to_bytes(img, img.RawFormat, ref nStride);

            int[] in_data = new int[10];
            int[] out_ints = new int[10];
            double[] out_doubles = new double[10];

            in_data[0] = img.Width;
            in_data[1] = img.Height;
            in_data[2] = pBuf.Length;
            in_data[3] = nStride;

            out_ints[0] = 0;
            get_image_sharpness(pBuf, in_data, out_ints, out_doubles);
            if (1 == out_ints[0])
            {
                sharpness = out_doubles[0];
                return true;
            }
            else
                return false;
        }
        
        // 计算图像清晰度
        public static bool get_image_sharpness(byte[] pBuf, int nWidth, int nHeight, ref double sharpness)
        {
            int[] in_data = new int[10];
            int[] out_ints = new int[10];
            double[] out_doubles = new double[10];

            in_data[0] = nWidth;
            in_data[1] = nHeight;
            in_data[2] = pBuf.Length;

            out_ints[0] = 0;
            get_image_sharpness(pBuf, in_data, out_ints, out_doubles);
            if (1 == out_ints[0])
            {
                sharpness = out_doubles[0];
                return true;
            }
            else
                return false;
        }

        // 计算图像亮度
        public static bool get_image_brightness(byte[] pBuf, int nWidth, int nHeight, ref double brightness)
        {
            int[] in_data = new int[10];
            int[] out_ints = new int[10];
            double[] out_doubles = new double[10];

            in_data[0] = nWidth;
            in_data[1] = nHeight;
            in_data[2] = pBuf.Length;

            out_ints[0] = 0;
            get_image_brightness(pBuf, in_data, out_ints, out_doubles);
            if (1 == out_ints[0])
            {
                brightness = out_doubles[0];
                return true;
            }
            else
                return false;
        }

        // 在导航图像中寻找指定半径的圆，如找到，返回圆心坐标
        public bool find_circle_in_image(Image img, double radius, ref Point2d center)
        {
            int[] in_data = new int[10];
            int[] out_ints = new int[10];
            double[] in_doubles = new double[10];
            double[] out_doubles = new double[10];
            
            int nStride = 0;
            byte[] pBuf = GeneralUtils.convert_image_to_bytes(img, img.RawFormat, ref nStride);
            
            in_data[0] = img.Width;
            in_data[1] = img.Height;
            in_data[2] = pBuf.Length;
            in_data[3] = nStride;
            in_doubles[0] = radius / 2;

            out_ints[0] = 0;
            //find_circle_in_image(pBuf, in_data, in_doubles, out_ints, out_doubles);
            find_circle_in_image_hv(pBuf, in_data, in_doubles, out_ints, out_doubles);
            if (1 == out_ints[0])
            {
                center.x = out_doubles[0];
                center.y = out_doubles[1];
                return true;
            }
            else
                return false;
        }

        // 在相机中寻找指定半径的圆，如找到，返回圆心坐标
        public bool find_circle_in_cam(CameraOps cam, object cam_lock, double m_pixels_per_um, double radius, ref Point2d center)
        {
            Image guide_img;
            lock (cam_lock)
            {
                Bitmap bmp = new Bitmap(cam.m_nCamWidth, cam.m_nCamHeight, PixelFormat.Format24bppRgb);
                Rectangle rect = new Rectangle(new Point(0, 0), bmp.Size);
                BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(cam.m_pImageBuf, 0, bmp_data.Scan0, bmp_data.Stride * bmp_data.Height);
                bmp.UnlockBits(bmp_data);

                guide_img = bmp;
            }

            Point2d center2 = new Point2d(0, 0);
            Point2d offset = new Point2d(0, 0);
            if (true == find_circle_in_image(guide_img, radius * m_pixels_per_um, ref center2))
            {
                center = center2;
                return true;
            }
            else
                return false;
        }

        // nUpOrDown 1为向上，2为向下
        bool autofocus_monitor_go_up_or_down(int nUpOrDown, int nPhase, short nAxis, int time_limit, double dbUpperLimit, double dbLowerLimit,
            double clarity_thres1, double clarity_thres2, double dest, int counter_limit, double dist_limit, ref double pMaxClarity, ref double pMaxClarityPos)
        {
            long nAxisStatus = 0;
            double dbProfilePos = 0;
            double dbPreviousClarity = 0;
            double dbClarity = 0;

            Form form = null;
            bool bIsCalibrating = false;
            if (true == GeneralUtils.check_if_form_is_open("Form_Calibration", ref form))
                bIsCalibrating = true;

            int counter = 0;
            int start_time = GetTickCount();
            while (true)
            {
                // 判断是否满足退出条件
                if (true)
                {
                    if (true == m_motion.is_axis_stop(nAxis))
                        break;

                    m_motion.get_axis_pos(nAxis, ref dbProfilePos);
                    if (Math.Abs(dbProfilePos - dest) <= dist_limit)
                        break;

                    if ((GetTickCount() - start_time) > time_limit)
                        break;
                }

                bool bNeedHandle = false;
                if (1 == nUpOrDown)
                {
                    if (dbProfilePos >= dbLowerLimit)
                        bNeedHandle = true;
                }
                else if(2 == nUpOrDown)
                {
                    if (dbProfilePos <= dbUpperLimit)
                        bNeedHandle = true;
                }
                
                // 获取图像清晰度
                if (true == bNeedHandle)
                {
                    dbClarity = 0;
                    lock (parent.m_main_cam_lock)
                    {
                        double sharpness = 0;
                        if (true == Gaugers.ImgOperators.get_image_sharpness(parent.m_main_camera.m_pImageBuf,
                            parent.m_main_camera.m_nCamWidth, parent.m_main_camera.m_nCamHeight, ref sharpness))
                        {
                            dbClarity = sharpness;
                        }
                    }

                    if (dbClarity > 0)
                    {
                        if (0 == dbPreviousClarity)
                            dbPreviousClarity = dbClarity;
                        else
                        {
                            if (Math.Abs(dbClarity - dbPreviousClarity) > clarity_thres1)
                            {
                                double thres = (true == bIsCalibrating) ? 30 : 30;
                                if ((dbClarity > pMaxClarity) && (dbClarity > thres))
                                {
                                    counter = 0;
                                    pMaxClarity = dbClarity;
                                    pMaxClarityPos = dbProfilePos;
                                }
                                else
                                {
                                    bNeedHandle = false;
                                    if (1 == nUpOrDown)
                                    {
                                        if (pMaxClarityPos >= dbLowerLimit)
                                            bNeedHandle = true;
                                    }
                                    else if (2 == nUpOrDown)
                                    {
                                        if (pMaxClarityPos <= dbUpperLimit)
                                            bNeedHandle = true;
                                    }

                                    if (true == bNeedHandle)
                                    {
                                        if ((pMaxClarity - dbClarity) >= clarity_thres2)
                                        {
                                            counter++;
                                        }
                                        else
                                            counter = 0;
                                    }
                                    else
                                        counter = 0;
                                }

                                Debugger.Log(0, null, string.Format("111111 自动对焦 第 {0} 段运动：当前轴规划位置 = {1:0.000}, 清晰度 = {2:0.000}, 最大清晰度 = {3:0.000}, 最大清晰度位置 = {4:0.000}, counter = {5}",
                                    nPhase, dbProfilePos, dbClarity, pMaxClarity, pMaxClarityPos, counter));
                                
                                if (counter >= counter_limit)
                                {
                                    Debugger.Log(0, null, string.Format("111111 自动对焦 第 {0} 段运动达到退出条件，现在退出", nPhase));
                                    break;
                                }
                            }

                            dbPreviousClarity = dbClarity;
                        }
                    }
                }
                
                Thread.Sleep(10);
            }

            return true;
        }

        // nUpOrDown 1为向上，2为向下
        bool autofocus_monitor_go_up_or_down3(int nUpOrDown, int nPhase, short nAxis,
            Image img, Point2d[] ROI_rect, double extension,
            int time_limit, double dbUpperLimit, double dbLowerLimit, double clarity_thres1, double clarity_thres2,
            double dest, int counter_limit, double dist_limit, ref double pMaxClarity, ref double pMaxClarityPos, ref int pLoopCounter)
        {
            clarity_thres1 = 0.2;

            long nAxisStatus = 0;
            double dbProfilePos = 0;
            double dbPreviousClarity = 0;
            double dbClarity = 0;
            double dbStartClarity = pMaxClarity;

            double final_max_contrast = 0;
            double final_max_contrast_pos = 0;
            double final_max_contrast_y = 0;
            double prev_max_contrast = -1;
            
            int counter = 0;
            int counter2 = 0;
            int start_time = GetTickCount();
            while (true)
            {
                // 判断是否满足退出条件
                if (true)
                {
                    if (true == m_motion.is_axis_stop(nAxis))
                        break;

                    m_motion.get_axis_pos(nAxis, ref dbProfilePos);
                    if (Math.Abs(dbProfilePos - dest) <= dist_limit)
                        break;

                    if ((GetTickCount() - start_time) > time_limit)
                        break;
                }
                
                bool bNeedHandle = false;
                if (1 == nUpOrDown)
                {
                    if (dbProfilePos <= dbUpperLimit)
                        bNeedHandle = true;
                }
                else if (2 == nUpOrDown)
                {
                    if (dbProfilePos >= dbLowerLimit)
                        bNeedHandle = true;
                }

                // 获取图像清晰度
                if (true == bNeedHandle)
                {
                    dbClarity = 0;
                    //Debugger.Log(0, null, string.Format("111111 自动对焦 333"));
                    lock (parent.m_main_cam_lock)
                    {
                        double sharpness = 0;
                        if (true == Gaugers.ImgOperators.get_image_sharpness(parent.m_main_camera.m_pImageBuf,
                            parent.m_main_camera.m_nCamWidth, parent.m_main_camera.m_nCamHeight, ref sharpness))
                        {
                            dbClarity = sharpness;
                        }
                    }
                    //Debugger.Log(0, null, string.Format("111111 自动对焦 444"));
                    if (dbClarity > 0)
                    {
                        if (0 == dbPreviousClarity)
                            dbPreviousClarity = dbClarity;
                        else
                        {
                            if (Math.Abs(dbClarity - dbPreviousClarity) > clarity_thres1)
                            {
                                if ((dbClarity > pMaxClarity) && (dbClarity > 20))
                                {
                                    counter = 0;
                                    pMaxClarity = dbClarity;
                                    pMaxClarityPos = dbProfilePos;
                                }
                                else
                                {
                                    bNeedHandle = false;
                                    if (1 == nUpOrDown)
                                    {
                                        if (pMaxClarityPos >= dbLowerLimit)
                                            bNeedHandle = true;
                                    }
                                    else if (2 == nUpOrDown)
                                    {
                                        if (pMaxClarityPos <= dbUpperLimit)
                                            bNeedHandle = true;
                                    }

                                    if (nPhase >= 4)
                                    {
                                        if (pMaxClarity <= dbStartClarity)
                                            bNeedHandle = false;
                                    }

                                    if (true == bNeedHandle)
                                    {
                                        if ((pMaxClarity - dbClarity) >= (clarity_thres2 / 3))
                                        {
                                            counter++;
                                        }
                                        else
                                            counter = 0;
                                    }
                                    else
                                        counter = 0;
                                }

                                counter2++;

                                Debugger.Log(0, null, string.Format("111111 自动对焦 第 {0} 段运动：当前轴规划位置 = {1:0.000}, 清晰度 = {2:0.000}, 最大清晰度 = {3:0.000}, 最大清晰度位置 = {4:0.000}, counter = {5}",
                                    nPhase, dbProfilePos, dbClarity, pMaxClarity, pMaxClarityPos, counter));

                                if (counter >= counter_limit)
                                {
                                    Debugger.Log(0, null, string.Format("111111 自动对焦 第 {0} 段运动达到退出条件，现在退出", nPhase));
                                    break;
                                }
                            }

                            dbPreviousClarity = dbClarity;
                        }
                    }
                }

                Thread.Sleep(10);
            }

            pLoopCounter = counter2;

            return true;
        }

        // nUpOrDown 1为向上，2为向下
        bool autofocus_monitor_go_up_or_down2(int nUpOrDown, int nPhase, short nAxis,
            Image img, Point2d[] ROI_rect, double line_angle, double extension,
            int time_limit, double dbUpperLimit, double dbLowerLimit, double clarity_thres1, double clarity_thres2, 
            double dest, int counter_limit, double dist_limit, ref double pMaxClarity, ref double pMaxClarityPos, ref int pLoopCounter)
        {
            clarity_thres1 = 0.2;

            long nAxisStatus = 0;
            double dbProfilePos = 0;
            double dbPreviousClarity = 0;
            double dbClarity = 0;

            double final_max_contrast = 0;
            double final_max_contrast_pos = 0;
            double final_max_contrast_y = 0;
            double prev_max_contrast = -1;
            
            int counter = 0;
            int counter2 = 0;
            int start_time = GetTickCount();
            while (true)
            {
                // 判断是否满足退出条件
                if (true)
                {
                    if (true == m_motion.is_axis_stop(nAxis))
                        break;

                    m_motion.get_axis_pos(nAxis, ref dbProfilePos);
                    if (Math.Abs(dbProfilePos - dest) <= dist_limit)
                        break;

                    if ((GetTickCount() - start_time) > time_limit)
                        break;
                }

                bool bNeedHandle = false;
                if (1 == nUpOrDown)
                {
                    if (dbProfilePos <= dbUpperLimit)
                        bNeedHandle = true;
                }
                else if (2 == nUpOrDown)
                {
                    if (dbProfilePos >= dbLowerLimit)
                        bNeedHandle = true;
                }

                // 获取图像清晰度
                if (true == bNeedHandle)
                {
                    dbClarity = 0;
                    
                    lock (parent.m_main_cam_lock)
                    {
                        //dbClarity = get_max_line_contrast_with_angle_in_ROI(parent.m_main_camera.m_pImageBuf, 
                        //    parent.m_main_camera.m_nCamWidth, parent.m_main_camera.m_nCamHeight, ROI_rect, line_angle, extension);

                        double sharpness = 0;
                        Gaugers.ImgOperators.get_image_sharpness(parent.m_main_camera.m_pImageBuf,
                            parent.m_main_camera.m_nCamWidth, parent.m_main_camera.m_nCamHeight, ref sharpness);
                        dbClarity = sharpness;
                    }

                    if (dbClarity > 0)
                    {
                        if (0 == dbPreviousClarity)
                            dbPreviousClarity = dbClarity;
                        else
                        {
                            if (Math.Abs(dbClarity - dbPreviousClarity) > clarity_thres1)
                            {
                                if ((dbClarity > pMaxClarity) && (dbClarity > 20))
                                {
                                    counter = 0;
                                    pMaxClarity = dbClarity;
                                    pMaxClarityPos = dbProfilePos;
                                }
                                else
                                {
                                    bNeedHandle = false;
                                    if (1 == nUpOrDown)
                                    {
                                        if (pMaxClarityPos >= dbLowerLimit)
                                            bNeedHandle = true;
                                    }
                                    else if (2 == nUpOrDown)
                                    {
                                        if (pMaxClarityPos <= dbUpperLimit)
                                            bNeedHandle = true;
                                    }

                                    if (true == bNeedHandle)
                                    {
                                        if ((pMaxClarity - dbClarity) >= (clarity_thres2 / 3))
                                        {
                                            counter++;
                                        }
                                        else
                                            counter = 0;
                                    }
                                    else
                                        counter = 0;
                                }

                                counter2++;

                                Debugger.Log(0, null, string.Format("111111 自动对焦 第 {0} 段运动：当前轴规划位置 = {1:0.000}, 清晰度 = {2:0.000}, 最大清晰度 = {3:0.000}, 最大清晰度位置 = {4:0.000}, counter = {5}",
                                    nPhase, dbProfilePos, dbClarity, pMaxClarity, pMaxClarityPos, counter));

                                if (counter >= counter_limit)
                                {
                                    Debugger.Log(0, null, string.Format("111111 自动对焦 第 {0} 段运动达到退出条件，现在退出", nPhase));
                                    break;
                                }
                            }

                            dbPreviousClarity = dbClarity;
                        }
                    }
                }

                Thread.Sleep(10);
            }

            pLoopCounter = counter2;

            return true;
        }

        // 获取指定图像区域内指定角度线段的最大对比度
        public double get_max_line_contrast_with_angle_in_ROI(byte[] pBuf, int width, int height, Point2d[] rect, double line_angle, double extension)
        {
            int nStride = (((width * 24 + 31) / 32) * 4);
            int[] out_ints = new int[10];
            int[] in_data = new int[10];
            in_data[0] = width;
            in_data[1] = height;
            in_data[2] = pBuf.Length;
            in_data[3] = nStride;

            double[] in_doubles = new double[20];
            double[] out_doubles = new double[20];

            in_doubles[0] = rect[0].x;
            in_doubles[1] = rect[0].y;
            in_doubles[2] = rect[3].x;
            in_doubles[3] = rect[3].y;
            in_doubles[4] = rect[2].x;
            in_doubles[5] = rect[2].y;
            in_doubles[6] = rect[1].x;
            in_doubles[7] = rect[1].y;
            in_doubles[8] = line_angle;
            in_doubles[9] = extension;

            out_ints[0] = 0;
            out_doubles[0] = 0;

            get_max_line_contrast_with_angle_in_ROI(pBuf, in_data, in_doubles, out_ints, out_doubles);
            if (1 == out_ints[0])
            {
                return out_doubles[0];
            }

            return 0;
        }

        // 线程：自动对焦
        public void thread_auto_focus(object obj)
        {
            const double CLARITY_THRES = 15.1;
            double dbAcc = 0.20;
            double dbSpeed = 15;
            double dbSpeed1 = 6.0;
            double dbSpeed2 = 2.0;
            double dbSpeed3 = 0.7;
            double dbSpeed4 = 0.22;
            double dbSpeed5 = 0.08;
            double ratio23 = 1.0;
            double ratio34 = 1.0;
            double ratio45 = 1.0;
            double offset = 0;
            bool bUseFastFocus = false;
            
            int nCount = 0;
            int nRepeats = 3;
            int sleep_time = 80;
            int nEarliestTime = GetTickCount();
            const int TIME_LIMIT = 4500;
            
            double dbLowerLimit = m_motion.m_autofocus_lower_pos - 0.3;
            double dbUpperLimit = m_motion.m_autofocus_upper_pos + 0.3;
            double dbMaxClarity = 0;
            double dbLastMaxClarity = 0;
            double dbMaxClarityPos = 0;
            double dbClarity = 0;
            double dbProfilePos = 0;
            double dbGotoPos = 0;
            double dbPreviousClarity = 0;
            
            //if (true == GeneralUtils.check_if_form_is_open("Form_Calibration"))
            //{
            //    dbLowerLimit += 4;
            //    dbUpperLimit += 4;
            //}
            
            double neg_limit = m_motion.m_axes[MotionOps.AXIS_Z - 1].negative_limit;
            if (true == GeneralUtils.check_if_form_is_open("Form_Calibration"))
            {
                dbLowerLimit = neg_limit + 2;
                dbUpperLimit = dbLowerLimit + 4;
            }
            if (dbLowerLimit < neg_limit)
                dbLowerLimit = neg_limit;

            if (parent.comboBox_Len.SelectedIndex >= 3)
                m_nAutoFocusSpeed = 1;
            else
                m_nAutoFocusSpeed = 2;

            switch (m_nAutoFocusSpeed)
            {
                case 1:
                    ratio34 = 1.10;
                    ratio45 = 1.10;
                    break;
                case 2:
                    ratio34 = 1.25;
                    ratio45 = 1.25;
                    break;
                case 3:
                    ratio34 = 1.5;
                    ratio45 = 1.25;
                    //sleep_time = 10;
                    break;
            }
            
            if (true == m_bEnableAutofocusOffset)
            {
                switch (parent.m_current_measure_type)
                {
                    case MEASURE_TYPE.LINE_WIDTH_14:
                    case MEASURE_TYPE.LINE_WIDTH_13:
                    case MEASURE_TYPE.LINE_WIDTH_1234:
                    case MEASURE_TYPE.LINE_WIDTH_AVR:
                    case MEASURE_TYPE.LINE_SPACE:
                    case MEASURE_TYPE.LINE_SPACE_13:
                    case MEASURE_TYPE.ARC_LINE_SPACE:
                    case MEASURE_TYPE.ARC_LINE_WIDTH:
                    case MEASURE_TYPE.CIRCLE_OUTER_TO_INNER:
                    case MEASURE_TYPE.CIRCLE_INNER_TO_OUTER:
                    case MEASURE_TYPE.LINE:
                        //offset = -0.005 * m_bottomline_autofocus_offset;
                        offset = -0.005 * 4;
                        break;
                }
            }

            if (0 == parent.comboBox_Len.SelectedIndex)
            {
                m_nAutoFocusSpeed = 10;
                ratio23 = 1.05;
                ratio34 = 1.25;
                ratio45 = 1.25;
                nRepeats = 2;
                sleep_time = 30;
            }

            bool bNoNeedToGoUp = false;

            #region
            //if (parent.m_bUseHeightSensor && (parent.m_bDetectHeightOnce || parent.m_bIsCreatingTask))
            if (parent.m_bUseHeightSensor && parent.m_bDetectHeightOnce)
            {
                double vel = MainUI.m_motion.m_axes[MotionOps.AXIS_Z - 1].vel_for_long_range;

                if (true == parent.m_bDetectHeightOnce)
                    MainUI.m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, parent.m_dbStageTriggerHeight + 15, vel);
                
                // 手动学习模式第一个定位孔
                if ((1 == parent.m_nCreateTaskMode) && (0 == parent.get_fiducial_mark_count(parent.m_current_task_data)))
                    MainUI.m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, parent.m_dbStageTriggerHeight + 15, vel);

                if (false == parent.m_IO.is_height_sensor_activated())
                {
                    MainUI.m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, parent.m_dbStageTriggerHeight - 3, 7);

                    for (int m = 0; m < 1000; m++)
                    {
                        if (true == parent.m_IO.is_height_sensor_activated())
                        {
                            Point3d crd = new Point3d(0, 0, 0);
                            MainUI.m_motion.get_xyz_crds(ref crd);

                            parent.m_dbClearPlanePosZ = crd.z - parent.m_dbStageHeightGap;

                            MainUI.m_motion.stop_axis(MotionOps.AXIS_Z);
                            Thread.Sleep(150);
                            
                            break;
                        }

                        Thread.Sleep(5);
                    }
                }
                else
                {
                    MessageBox.Show(parent, "传感器触发高度设置有问题，可能触发高度过大或过小，请检查。", "提示");

                    parent.m_bDetectHeightOnce = false;
                    return;
                }

                if (true == parent.m_IO.is_height_sensor_activated())
                {
                    double lower = parent.m_dbClearPlanePosZ - 2;
                    double upper = lower + 4;

                    if (lower <= (neg_limit + 0.3))
                    {
                        lower = neg_limit + 0.3;
                        upper = lower + 2.5;
                    }

                    dbUpperLimit = upper;
                    dbLowerLimit = lower;

                    bNoNeedToGoUp = true;

                    //m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, parent.m_dbClearPlanePosZ, 10);
                    //MainUI.m_motion.stop_axis(MotionOps.AXIS_Z);
                    //Debugger.Log(0, null, string.Format("111111 m_dbClearPlanePosZ = {0:0.000}", parent.m_dbClearPlanePosZ));
                    //return;
                }
                else
                {
                    MessageBox.Show(parent, "无法触发高度传感器信号，请检查原因。", "提示");

                    parent.m_bDetectHeightOnce = false;
                    return;
                }

                parent.m_bDetectHeightOnce = false;
            }
            #endregion

            m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

            Debugger.Log(0, null, string.Format("111111 自动对焦 start = {0:0.000}, end = {1:0.000}, 当前位置 {2:0.000}", dbUpperLimit, dbLowerLimit, dbProfilePos));

            // 如果太高或太低，先移动到预设高度范围
            if ((dbProfilePos > dbUpperLimit) && (false == bNoNeedToGoUp))
            {
                dbGotoPos = dbUpperLimit + 3;
                m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, 30);
            }
            else if ((dbProfilePos < dbLowerLimit) && (false == bNoNeedToGoUp))
            {
                dbGotoPos = dbLowerLimit + 0.3;
                m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, 30);
            }

            int nStartTime = GetTickCount();
            
            // 判断当前图像质量是否符合高速对焦的要求
            Point2d[] ROI_rect = new Point2d[4];
            double first_contrast = 0;
            if (true == m_bFastFocus)
            {
                double thres = 20;
                
                for (int n = 0; n < 4; n++)
                {
                    double offset_x = m_mes_data.m_graphmade_ROI_rect[n].x - m_mes_data.m_center_x_in_metric;
                    double offset_y = m_mes_data.m_graphmade_ROI_rect[n].y - m_mes_data.m_center_y_in_metric;
                    ROI_rect[n].x = offset_x * 1000 * parent.m_calib_data[parent.comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_X - 1].nDir);
                    ROI_rect[n].y = offset_y * 1000 * parent.m_calib_data[parent.comboBox_Len.SelectedIndex] * (double)(m_motion.m_axes[MotionOps.AXIS_Y - 1].nDir);
                    ROI_rect[n].x = ROI_rect[n].x + parent.ui_MainImage.Image.Width / 2;
                    ROI_rect[n].y = ROI_rect[n].y + parent.ui_MainImage.Image.Height / 2;
                }

                lock (parent.m_main_cam_lock)
                {
                    first_contrast = get_max_line_contrast_with_angle_in_ROI(parent.m_main_camera.m_pImageBuf,
                            parent.m_main_camera.m_nCamWidth, parent.m_main_camera.m_nCamHeight, ROI_rect,
                        m_mes_data.m_line_angle_on_graph, parent.m_nSmallSelectionFrameExtension);
                }

                if (first_contrast < thres)
                    m_bFastFocus = false;
            }
            else
            {
                double thres = 58;
                
                lock (parent.m_main_cam_lock)
                {
                    double sharpness = 0;
                    if (true == Gaugers.ImgOperators.get_image_sharpness(parent.m_main_camera.m_pImageBuf,
                        parent.m_main_camera.m_nCamWidth, parent.m_main_camera.m_nCamHeight, ref sharpness))
                    {
                        if (sharpness >= thres)
                            bUseFastFocus = true;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 sharpness = {0:0.000}", sharpness));
                    }
                }
            }

            // 执行自动对焦
            if (true == m_bFastFocus)
            {
                double speed1 = 0.5;
                double speed2 = 0.08;
                speed2 = 0.08 + (5 - parent.comboBox_Len.SelectedIndex) * 0.02;

                if (dbProfilePos < (dbLowerLimit + (Math.Abs(dbLowerLimit - dbUpperLimit) / 2)))
                {
                    Debugger.Log(0, null, string.Format("111111 自动对焦 从下到上"));

                    #region
                    double first_pos = 0;
                    m_motion.get_axis_pos(MotionOps.AXIS_Z, ref first_pos);

                    // 从下到上，第一段
                    int start_time = GetTickCount();
                    int counter = 0;
                    int exit_counter = 0;
                    int nMaxClarityPosIndex = 0;
                    if (true)
                    {
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, speed1, dbAcc, dbAcc);

                        exit_counter = 0;
                        bool res = autofocus_monitor_go_up_or_down2(1, 2, MotionOps.AXIS_Z, parent.ui_MainImage.Image, ROI_rect,
                                    m_mes_data.m_line_angle_on_graph, parent.m_nSmallSelectionFrameExtension,
                                    TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, CLARITY_THRES,
                                    dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos, ref exit_counter);
                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        //if ((0 == dbMaxClarityPos) || (false == res))
                        //{
                        //    return;
                        //}
                    }

                    // 第 2 段 向下返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 2 段 向下返回"));

                        ratio23 = 1.0;
                        dbLowerLimit = dbLowerLimit;
                        if (dbMaxClarity > 0.001)
                        {
                            dbUpperLimit = dbProfilePos - Math.Abs(dbProfilePos - dbMaxClarityPos) * ratio23;
                            if (dbUpperLimit < dbLowerLimit)
                                dbUpperLimit = dbLowerLimit;
                            if (dbMaxClarity < first_contrast)
                                dbUpperLimit = first_pos + 0.025;
                            else if (exit_counter > 1)
                                dbUpperLimit -= 0.025;
                        }

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 15;
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed, 0.015);
                    }

                    // 第三段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity -= 20;
                    dbPreviousClarity = 0;
                    exit_counter = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, speed2, dbAcc, dbAcc);

                        bool res = autofocus_monitor_go_up_or_down2(2, 3, MotionOps.AXIS_Z, parent.ui_MainImage.Image, ROI_rect,
                                    m_mes_data.m_line_angle_on_graph, parent.m_nSmallSelectionFrameExtension,
                                    TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, CLARITY_THRES,
                                    dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos, ref exit_counter);
                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 3 段 向上返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段 向上返回"));

                        dbLowerLimit = dbMaxClarityPos + 0.010 + offset;
                        dbUpperLimit = dbLowerLimit + 0.060;
                        dbMaxClarityPos = dbLowerLimit;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 4 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 5;
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed, 0);

                        Thread.Sleep(sleep_time);
                    }
                    #endregion
                }
                else
                {
                    Debugger.Log(0, null, string.Format("111111 自动对焦 从上到下"));

                    #region
                    double first_pos = 0;
                    m_motion.get_axis_pos(MotionOps.AXIS_Z, ref first_pos);

                    // 从上到下，第一段
                    int start_time = GetTickCount();
                    int counter = 0;
                    int exit_counter = 0;
                    int nMaxClarityPosIndex = 0;
                    if (true)
                    {
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, speed1, dbAcc, dbAcc);

                        exit_counter = 0;
                        bool res = autofocus_monitor_go_up_or_down2(2, 2, MotionOps.AXIS_Z, parent.ui_MainImage.Image, ROI_rect,
                                    m_mes_data.m_line_angle_on_graph, parent.m_nSmallSelectionFrameExtension,
                                    TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, CLARITY_THRES,
                                    dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos, ref exit_counter);
                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        //if ((0 == dbMaxClarityPos) || (false == res))
                        //{
                        //    return;
                        //}
                    }

                    // 第 2 段 向上返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 2 段 向上返回"));

                        ratio23 = 1.0;
                        dbUpperLimit = dbUpperLimit;
                        if (dbMaxClarity > 0.001)
                        {
                            dbLowerLimit = dbProfilePos + Math.Abs(dbProfilePos - dbMaxClarityPos) * ratio23;
                            if (dbLowerLimit > dbUpperLimit)
                                dbLowerLimit = dbUpperLimit;

                            if (dbMaxClarity < first_contrast)
                                dbLowerLimit = first_pos - 0.025;
                            else if (exit_counter > 1)
                                dbLowerLimit += 0.025;
                        }

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 15;
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed, 0.015);
                    }

                    // 第三段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity -= 20;
                    dbPreviousClarity = 0;
                    exit_counter = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, speed2, dbAcc, dbAcc);

                        bool res = autofocus_monitor_go_up_or_down2(1, 3, MotionOps.AXIS_Z, parent.ui_MainImage.Image, ROI_rect,
                                    m_mes_data.m_line_angle_on_graph, parent.m_nSmallSelectionFrameExtension,
                                    TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, CLARITY_THRES,
                                    dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos, ref exit_counter);
                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 3 段 向下返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段 向下返回"));

                        dbUpperLimit = dbMaxClarityPos - 0.010 + offset;
                        dbLowerLimit = dbUpperLimit - 0.060;
                        dbMaxClarityPos = dbUpperLimit;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 4 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 5;
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed, 0);

                        Thread.Sleep(sleep_time);
                    }
                    #endregion
                }
            }
            else if (true == bUseFastFocus)
            {
                double speed1 = 0.5;
                double speed2 = 0.08;
                double speed3 = 0.05;
                speed2 = 0.08 + (5 - parent.comboBox_Len.SelectedIndex) * 0.02;

                if (dbProfilePos < (dbLowerLimit + (Math.Abs(dbLowerLimit - dbUpperLimit) / 2)))
                //if (false)
                {
                    Debugger.Log(0, null, string.Format("111111 自动对焦 从下到上"));

                    #region
                    double first_pos = 0;
                    m_motion.get_axis_pos(MotionOps.AXIS_Z, ref first_pos);

                    // 从下到上，第一段
                    bool res = false;
                    int start_time = GetTickCount();
                    int counter = 0;
                    int exit_counter = 0;
                    int nMaxClarityPosIndex = 0;
                    if (true)
                    {
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, speed1, dbAcc, dbAcc);

                        exit_counter = 0;
                        res = autofocus_monitor_go_up_or_down3(1, 2, MotionOps.AXIS_Z, parent.ui_MainImage.Image, ROI_rect,
                                    parent.m_nSmallSelectionFrameExtension,
                                    TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, CLARITY_THRES,
                                    dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos, ref exit_counter);
                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);
                    }

                    // 第 2 段 向下返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 2 段 向下返回"));

                        ratio23 = 1.0;
                        dbLowerLimit = dbLowerLimit;
                        if (dbMaxClarity > 0.001)
                        {
                            dbUpperLimit = dbProfilePos - Math.Abs(dbProfilePos - dbMaxClarityPos) * ratio23;
                            if (dbUpperLimit < dbLowerLimit)
                                dbUpperLimit = dbLowerLimit;
                            if (dbMaxClarity < first_contrast)
                                dbUpperLimit = first_pos + 0.025;
                            else if (exit_counter > 1)
                                dbUpperLimit -= 0.025;
                        }

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 15;
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed, 0.015);

                        Thread.Sleep(sleep_time);
                    }

                    // 第三段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity -= 20;
                    dbPreviousClarity = 0;
                    exit_counter = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, speed2, dbAcc, dbAcc);

                        res = autofocus_monitor_go_up_or_down3(2, 3, MotionOps.AXIS_Z, parent.ui_MainImage.Image, ROI_rect,
                                    parent.m_nSmallSelectionFrameExtension,
                                    TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, CLARITY_THRES,
                                    dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos, ref exit_counter);
                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 3 段 向上返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段 向上返回"));

                        dbLowerLimit = dbMaxClarityPos + 0.01;
                        dbUpperLimit = dbLowerLimit + 0.080;
                        dbMaxClarityPos = dbLowerLimit;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 4 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 5;
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed, 0);

                        Thread.Sleep(sleep_time);
                    }

                    // 第四段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity -= 20;
                    dbPreviousClarity = 0;
                    exit_counter = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, speed3, dbAcc, dbAcc);

                        res = autofocus_monitor_go_up_or_down3(1, 4, MotionOps.AXIS_Z, parent.ui_MainImage.Image, ROI_rect,
                                    parent.m_nSmallSelectionFrameExtension,
                                    TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, 1,
                                    dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos, ref exit_counter);
                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 4 段 向下返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 4 段 向下返回"));

                        if (parent.comboBox_Len.SelectedIndex >= 4)
                            dbUpperLimit = dbMaxClarityPos - 0.002 + offset;
                        else
                            dbUpperLimit = dbMaxClarityPos - 0;
                        dbLowerLimit = dbUpperLimit - 0.030;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 5 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 15;
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed, 0.015);

                        Thread.Sleep(sleep_time);
                    }
                    #endregion
                }
                else
                {
                    Debugger.Log(0, null, string.Format("111111 自动对焦 从上到下"));

                    #region
                    double first_pos = 0;
                    m_motion.get_axis_pos(MotionOps.AXIS_Z, ref first_pos);

                    // 从上到下，第一段
                    int start_time = GetTickCount();
                    int counter = 0;
                    int exit_counter = 0;
                    int nMaxClarityPosIndex = 0;
                    if (true)
                    {
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, speed1, dbAcc, dbAcc);

                        exit_counter = 0;
                        bool res = autofocus_monitor_go_up_or_down3(2, 2, MotionOps.AXIS_Z, parent.ui_MainImage.Image, ROI_rect,
                                    parent.m_nSmallSelectionFrameExtension,
                                    TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, CLARITY_THRES,
                                    dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos, ref exit_counter);
                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        //if ((0 == dbMaxClarityPos) || (false == res))
                        //{
                        //    return;
                        //}
                    }

                    // 第 2 段 向上返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 2 段 向上返回"));

                        ratio23 = 1.0;
                        dbUpperLimit = dbUpperLimit;
                        if (dbMaxClarity > 0.001)
                        {
                            dbLowerLimit = dbProfilePos + Math.Abs(dbProfilePos - dbMaxClarityPos) * ratio23;
                            if (dbLowerLimit > dbUpperLimit)
                                dbLowerLimit = dbUpperLimit;

                            if (dbMaxClarity < first_contrast)
                                dbLowerLimit = first_pos - 0.025;
                            else if (exit_counter > 1)
                                dbLowerLimit += 0.025;
                        }

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 15;
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed, 0.015);

                        Thread.Sleep(sleep_time);
                    }

                    // 第三段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity -= 20;
                    dbPreviousClarity = 0;
                    exit_counter = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, speed2, dbAcc, dbAcc);

                        bool res = autofocus_monitor_go_up_or_down3(1, 3, MotionOps.AXIS_Z, parent.ui_MainImage.Image, ROI_rect,
                                    parent.m_nSmallSelectionFrameExtension,
                                    TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, CLARITY_THRES,
                                    dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos, ref exit_counter);
                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 3 段 向下返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段 向下返回"));

                        dbUpperLimit = dbMaxClarityPos - 0.010;
                        dbLowerLimit = dbUpperLimit - 0.060;
                        dbMaxClarityPos = dbUpperLimit;
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 4 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 5;
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed, 0);

                        Thread.Sleep(sleep_time);
                    }

                    // 第四段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity -= 20;
                    dbPreviousClarity = 0;
                    exit_counter = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, speed3, dbAcc, dbAcc);

                        bool res = autofocus_monitor_go_up_or_down3(2, 4, MotionOps.AXIS_Z, parent.ui_MainImage.Image, ROI_rect,
                                    parent.m_nSmallSelectionFrameExtension,
                                    TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, 1,
                                    dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos, ref exit_counter);
                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 4 段 向上返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 4 段 向上返回"));

                        if (parent.comboBox_Len.SelectedIndex >= 4)
                            dbLowerLimit = dbMaxClarityPos + 0.03 + offset;
                        else
                            dbLowerLimit = dbMaxClarityPos + 0;
                        dbUpperLimit = dbLowerLimit + 0.030;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 5 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 15;
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed, 0.015);

                        Thread.Sleep(sleep_time);
                    }
                    #endregion
                }
            }
            else
            {
                if (dbProfilePos < (dbLowerLimit + (Math.Abs(dbLowerLimit - dbUpperLimit) / 2)))
                {
                    Debugger.Log(0, null, string.Format("111111 自动对焦 从下到上"));

                    #region
                    // 从下到上，第一段
                    int start_time = GetTickCount();
                    int counter = 0;
                    int nMaxClarityPosIndex = 0;
                    bool res = false;
                    if (true)
                    {
                        dbAcc = 0.80;
                        dbGotoPos = dbUpperLimit;
                        //m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed1, dbAcc, dbAcc, 0.1);
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, dbSpeed1, dbAcc, dbAcc);
                        res = autofocus_monitor_go_up_or_down(1, 2, MotionOps.AXIS_Z, TIME_LIMIT, dbUpperLimit, dbLowerLimit,
                            0.002, CLARITY_THRES, dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos);

                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        //if ((0 == dbMaxClarityPos) || (false == res))
                        //{
                        //    return;
                        //}
                    }

                    // 第 2 段 向下返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 2 段 向下返回"));

                        dbLowerLimit = dbLowerLimit;
                        if (dbMaxClarityPos > 0.001)
                        {
                            dbUpperLimit = dbProfilePos - Math.Abs(dbProfilePos - dbMaxClarityPos) * ratio23;
                            if (dbLowerLimit > dbUpperLimit)
                                dbLowerLimit = dbUpperLimit;
                        }

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 15;
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, dbSpeed);

                        Thread.Sleep(sleep_time + 80);
                    }

                    // 第三段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity = 0;
                    dbPreviousClarity = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbLowerLimit;

                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, dbSpeed2, dbAcc, dbAcc);
                        res = autofocus_monitor_go_up_or_down(2, 3, MotionOps.AXIS_Z, TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, CLARITY_THRES,
                                dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos);

                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 3 段 向上返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段 向上返回"));

                        dbUpperLimit = dbUpperLimit;
                        dbLowerLimit = dbProfilePos + Math.Abs(dbProfilePos - dbMaxClarityPos) * ratio34;
                        if (dbLowerLimit > dbUpperLimit)
                            dbLowerLimit = dbUpperLimit;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 4 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 10;
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, dbSpeed);

                        Thread.Sleep(sleep_time);
                    }

                    // 第四段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity = 0;
                    dbPreviousClarity = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbUpperLimit;
                        dbAcc = 0.25;

                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, dbSpeed3, dbAcc, dbAcc);
                        res = autofocus_monitor_go_up_or_down(1, 4, MotionOps.AXIS_Z, TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, dbLastMaxClarity / 100,
                                dbGotoPos, nRepeats, 0.003, ref dbMaxClarity, ref dbMaxClarityPos);

                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 4 段 向下返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 4 段 向下返回"));

                        dbLowerLimit = dbLowerLimit;
                        dbUpperLimit = dbProfilePos - Math.Abs(dbProfilePos - dbMaxClarityPos) * ratio45;
                        if (dbLowerLimit > dbUpperLimit)
                            dbLowerLimit = dbUpperLimit;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 5 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 3;
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, dbSpeed);

                        Thread.Sleep(sleep_time);
                    }

                    // 第五段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity = 0;
                    dbPreviousClarity = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbLowerLimit;
                        //dbAcc = 0.25;

                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, dbSpeed4, dbAcc, dbAcc);
                        res = autofocus_monitor_go_up_or_down(2, 5, MotionOps.AXIS_Z, TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, dbLastMaxClarity / 100,
                                dbGotoPos, nRepeats, 0.003, ref dbMaxClarity, ref dbMaxClarityPos);

                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 5 段 向上返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 5 段 向上返回"));

                        dbUpperLimit = dbUpperLimit;
                        dbLowerLimit = dbProfilePos + Math.Abs(dbProfilePos - dbMaxClarityPos) * 1.5;
                        if (dbLowerLimit > dbUpperLimit)
                            dbLowerLimit = dbUpperLimit;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 6 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 3;
                        dbGotoPos = dbLowerLimit;
                        if (10 == m_nAutoFocusSpeed)
                            dbGotoPos = dbLowerLimit + 0.018 + offset;
                        m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, dbSpeed);

                        Thread.Sleep(sleep_time);
                    }

                    // 第六段运动
                    if (10 != m_nAutoFocusSpeed)
                    {
                        dbLastMaxClarity = dbMaxClarity;
                        counter = 0;
                        dbMaxClarity = 0;
                        dbPreviousClarity = 0;

                        start_time = GetTickCount();
                        dbGotoPos = dbUpperLimit;

                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, dbSpeed5, dbAcc, dbAcc);
                        res = autofocus_monitor_go_up_or_down(1, 6, MotionOps.AXIS_Z, TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, dbLastMaxClarity / 100,
                                dbGotoPos, nRepeats, 0.003, ref dbMaxClarity, ref dbMaxClarityPos);

                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }

                        dbUpperLimit = dbMaxClarityPos - 0.006 + offset;
                        dbLowerLimit = dbUpperLimit - 0.060;
                        dbMaxClarityPos = dbUpperLimit;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 7 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 3;
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, dbSpeed);

                        Thread.Sleep(sleep_time);
                    }
                    #endregion
                }
                else
                {
                    Debugger.Log(0, null, string.Format("111111 自动对焦 从上到下"));

                    #region
                    // 从上到下，第一段
                    int start_time = GetTickCount();
                    int counter = 0;
                    int nMaxClarityPosIndex = 0;
                    bool res = false;
                    if (true)
                    {
                        dbAcc = 0.80;
                        dbGotoPos = dbLowerLimit;
                        //m_motion.pt_move_axis_wait_until_reach_proximity(MotionOps.AXIS_Z, dbGotoPos, dbSpeed1, dbAcc, dbAcc, 0.1);
                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, dbSpeed1, dbAcc, dbAcc);
                        res = autofocus_monitor_go_up_or_down(2, 2, MotionOps.AXIS_Z, TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, CLARITY_THRES,
                                dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos);

                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        //if ((0 == dbMaxClarityPos) || (false == res))
                        //{
                        //    return;
                        //}
                    }

                    // 第 2 段 向上返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 2 段 向上返回"));

                        dbUpperLimit = dbUpperLimit;
                        if (dbMaxClarityPos > 0.001)
                        {
                            dbLowerLimit = dbProfilePos + Math.Abs(dbProfilePos - dbMaxClarityPos) * ratio23;
                            if (dbLowerLimit > dbUpperLimit)
                                dbLowerLimit = dbUpperLimit;
                        }

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));
                        
                        dbSpeed = 15;
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, dbSpeed);
                        
                        Thread.Sleep(sleep_time + 80);
                    }

                    // 第三段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity = 0;
                    dbPreviousClarity = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbUpperLimit;

                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, dbSpeed2, dbAcc, dbAcc);
                        res = autofocus_monitor_go_up_or_down(1, 3, MotionOps.AXIS_Z, TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, CLARITY_THRES,
                                dbGotoPos, nRepeats, 0.005, ref dbMaxClarity, ref dbMaxClarityPos);

                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 3 段 向下返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 3 段 向下返回"));

                        dbUpperLimit = dbProfilePos - Math.Abs(dbProfilePos - dbMaxClarityPos) * ratio34;
                        if (dbUpperLimit < dbLowerLimit)
                            dbUpperLimit = dbLowerLimit;
                        dbLowerLimit = dbLowerLimit;
                        if (dbLowerLimit > dbUpperLimit)
                            dbLowerLimit = dbUpperLimit;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 4 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 10;
                        dbGotoPos = dbUpperLimit;
                        m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, dbSpeed);

                        Thread.Sleep(sleep_time);
                    }

                    // 第四段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity = 0;
                    dbPreviousClarity = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbLowerLimit;
                        dbAcc = 0.25;

                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, dbSpeed3, dbAcc, dbAcc);
                        res = autofocus_monitor_go_up_or_down(2, 4, MotionOps.AXIS_Z, TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, dbLastMaxClarity / 100,
                                dbGotoPos, nRepeats, 0.003, ref dbMaxClarity, ref dbMaxClarityPos);

                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 4 段 向上返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 4 段 向上返回"));

                        dbUpperLimit = dbUpperLimit;
                        dbLowerLimit = dbProfilePos + Math.Abs(dbProfilePos - dbMaxClarityPos) * ratio45;
                        if (dbLowerLimit > dbUpperLimit)
                            dbLowerLimit = dbUpperLimit;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 5 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 3;
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, dbSpeed);

                        Thread.Sleep(sleep_time);
                    }

                    // 第五段运动
                    dbLastMaxClarity = dbMaxClarity;
                    counter = 0;
                    dbMaxClarity = 0;
                    dbPreviousClarity = 0;
                    if (true)
                    {
                        start_time = GetTickCount();
                        dbGotoPos = dbUpperLimit;
                        //dbAcc = 0.25;

                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, dbSpeed4, dbAcc, dbAcc);
                        res = autofocus_monitor_go_up_or_down(1, 5, MotionOps.AXIS_Z, TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, dbLastMaxClarity / 100,
                                dbGotoPos, nRepeats, 0.003, ref dbMaxClarity, ref dbMaxClarityPos);

                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }
                    }

                    // 第 5 段 向下返回
                    if (true)
                    {
                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 5 段 向下返回"));

                        dbUpperLimit = dbProfilePos - Math.Abs(dbProfilePos - dbMaxClarityPos) * 1.5;
                        if (dbUpperLimit < dbLowerLimit)
                            dbUpperLimit = dbLowerLimit;
                        dbLowerLimit = dbLowerLimit;
                        if (dbLowerLimit > dbUpperLimit)
                            dbLowerLimit = dbUpperLimit;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 6 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 3;
                        dbGotoPos = dbUpperLimit;
                        if (10 == m_nAutoFocusSpeed)
                            dbGotoPos = dbUpperLimit - 0.018 + offset;
                        m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, dbSpeed);

                        Thread.Sleep(sleep_time);
                    }

                    // 第六段运动
                    if (10 != m_nAutoFocusSpeed)
                    {
                        dbLastMaxClarity = dbMaxClarity;
                        counter = 0;
                        dbMaxClarity = 0;
                        dbPreviousClarity = 0;

                        start_time = GetTickCount();
                        dbGotoPos = dbLowerLimit;

                        m_motion.pt_move_axis_no_wait(MotionOps.AXIS_Z, dbGotoPos, dbSpeed5, dbAcc, dbAcc);
                        res = autofocus_monitor_go_up_or_down(2, 6, MotionOps.AXIS_Z, TIME_LIMIT, dbUpperLimit, dbLowerLimit, 0.002, dbLastMaxClarity / 100,
                                dbGotoPos, nRepeats, 0.003, ref dbMaxClarity, ref dbMaxClarityPos);

                        m_motion.get_axis_pos(MotionOps.AXIS_Z, ref dbProfilePos);

                        if ((0 == dbMaxClarityPos) || (false == res))
                        {
                            return;
                        }

                        dbLowerLimit = dbMaxClarityPos + 0.006 + offset;
                        dbUpperLimit = dbLowerLimit + 0.060;
                        dbMaxClarityPos = dbLowerLimit;

                        Debugger.Log(0, null, string.Format("111111 自动对焦 第 7 段运动：dbUpperLimit = {0:0.000}, dbLowerLimit = {1:0.000}", dbUpperLimit, dbLowerLimit));

                        dbSpeed = 3;
                        dbGotoPos = dbLowerLimit;
                        m_motion.pt_move_axis_wait_until_stop(MotionOps.AXIS_Z, dbGotoPos, dbSpeed);

                        Thread.Sleep(sleep_time);
                    }
                    #endregion
                }
            }

            int nEndTime = GetTickCount();
            Debugger.Log(0, null, string.Format("111111 自动对焦共耗时 {0} 毫秒, {1}", nEndTime - nStartTime, nEndTime - nEarliestTime));
            Debugger.Log(0, null, string.Format("222222 自动对焦共耗时 {0} 毫秒, {1}", nEndTime - nStartTime, nEndTime - nEarliestTime));
        }

        // 自动对焦
        public bool do_auto_focus()
        {
            Thread thrd = new Thread(thread_auto_focus);
            thrd.Start();
            
            return true;
        }
    }
}
