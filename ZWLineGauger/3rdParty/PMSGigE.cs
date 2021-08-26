using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace PMSAPI
{

    #region"enum"

    //brief Bayer颜色模式
    public enum PMS_BAYER_MODE
    {
        BayerRG,	//< RG
	    BayerBG,	//< BG
	    BayerGR,	//< GR
	    BayerGB,	//< GB
	    BayerInvalid        
    }
    public enum PMS_PixelFormatEnums
    {
        PixelFormat_Mono8 = 0x01080001,	//!<8Bit灰度
	    PixelFormat_BayerBG8=0x0108000B,	//!<8Bit Bayer图,颜色模式为BGGR
	    PixelFormat_BayerRG8=0x01080009,	//!<8Bit Bayer图,颜色模式为RGGB
	    PixelFormat_BayerGB8=0x0108000A,	//!<8Bit Bayer图,颜色模式为GBRG
	    PixelFormat_BayerGR8=0x01080008,	//!<8Bit Bayer图,颜色模式为GRBG
	    PixelFormat_BayerGRW8=0x0108000C,	//!<8Bit Bayer图,颜色模式为GRW8
	    PixelFormat_Mono16=0x01100007,		//!<16Bit灰度
	    PixelFormat_BayerGR16=0x0110002E,	//!<16Bit Bayer图,颜色模式为GR
	    PixelFormat_BayerRG16=0x0110002F,	//!<16Bit Bayer图,颜色模式为RG
	    PixelFormat_BayerGB16=0x01100030,	//!<16Bit Bayer图,颜色模式为GB
	    PixelFormat_BayerBG16=0x01100031	//!<16Bit Bayer图,颜色模式为BG  
    };

    public  enum TriggerSourceEnums
    {
        TriggerSource_Software = 0,
        TriggerSource_Line1 = 2
    };

    public  enum TriggerModeEnums
    {
        TriggerMode_Off,
        TriggerMode_On
    };

    public  enum TriggerActivationEnums
    {
        TriggerActivation_RisingEdge,
        TriggerActivation_FallingEdge
    };

    public  enum LineSourceEnums
    {
        LineSource_Off = 0,
        LineSource_ExposureActive = 5,
        LineSource_Timer1Active = 6,
        LineSource_UserOutput0 = 12
    };

    public  enum UserSetSelectorEnums
    {
        UserSetSelector_Default,  //!<
        UserSetSelector_UserSet1,  //!<
        UserSetSelector_UserSet2   //!<
    };

    public  enum SensorTapsEnums
    {
        SensorTaps_One,  //!<
        SensorTaps_Two,  //!<
        SensorTaps_Three,  //!<
        SensorTaps_Four,  //!<
    };

    public  enum ImageFlipType
    {
        FlipHorizontal = 0,  //!< Flip Horizontally (Mirror)
        FlipVertical = 1, //!< Flip Vertically
        FlipBoth = 2 //!< Flip Vertically
    } ;

    enum ImageRotateType
    {
	    Rotate90DegCw = 0,       //!< 顺时针旋转90度
	    Rotate90DegCcw = 1       //!< 逆时针旋转90度
    };
 /// \brief Error return code enumeration. This is returned by all \c Jai_Factory.dll functions
    public enum PMSSTATUS_CODES
    {
        PMSST_SUCCESS = 0,      ///< OK      
        PMSST_ERROR = -1001,  ///< Generic errorcode
        PMSST_ERR_NOT_INITIALIZED = -1002,
        PMSST_ERR_NOT_IMPLEMENTED = -1003,
        PMSST_ERR_RESOURCE_IN_USE = -1004,
        PMSST_ACCESS_DENIED = -1005,  ///< Access denied
        PMSST_INVALID_HANDLE = -1006,  ///< Invalid handle
        PMSST_INVALID_ID = -1007,  ///< Invalid ID
        PMSST_NO_DATA = -1008,  ///< No data
        PMSST_INVALID_PARAMETER = -1009,  ///< Invalid parameter
        PMSST_FILE_IO = -1010,  ///< File IO error
        PMSST_TIMEOUT = -1011,  ///< Timeout
        PMSST_ERR_ABORT = -1012,  /* GenTL v1.1 */
        PMSST_INVALID_BUFFER_SIZE = -1013,  ///< Invalid buffer size
        PMSST_ERR_NOT_AVAILABLE = -1014,  /* GenTL v1.2 */
        PMSST_INVALID_ADDRESS = -1015,  /* GenTL v1.3 */
    };

    public enum PMSCameraRunEnums
    {
	    PMSCameraRun_ON,
	    PMSCameraRun_OFF 
    };

    public enum PMSShowWindowEnums
    {
        SW_SHOW = 5,
        SW_HIDE = 0
    };

    #endregion

     #region"struct"

    [StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_INFO
    {
        public UInt64 nTimeStamp;   ///< 时间戳，采集到图像的时刻，精度为0.01us ;
        public ushort nBlockId;		///< 帧号，从开始采集开始计数
        public IntPtr pImageBuffer;	///< 图像指针，即指向(0,0)像素所在内存位置的指针，通过该指针可以访问整个图像;
        public ulong nImageSizeAcq;	///< 采集到的图像大小[字节];
        public byte nMissingPackets;///< 传输过程中丢掉的包数量
        public UInt64 nPixelType;		///< Pixel Format Type
        public UInt32 nSizeX;			///< Image width
        public UInt32 nSizeY;         ///< Image height
        public UInt32 nOffsetX;		///< Image offset x
        public UInt32 nOffsetY;       ///< Image offset y                            
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PMSCamInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string mIpAddr;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public string mEthernetAddr;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string mMfgName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string mModelName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string mSerialNumber;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string mUserDefinedName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string m_IfIp;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public string m_IfMAC;
    }

    [StructLayout(LayoutKind.Sequential)]
    public  struct  PMSStreamStatistic
    {
	    public ulong  m_nTotalBuf;
	    public ulong m_nFailedBuf;
	    public ulong m_nTotalPacket;
	    public ulong m_nFailedPacket;
	    public ulong m_nResendPacketReq;
        public ulong m_nResendPacket;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RGBQUAD
    {
        public Byte R;
        public Byte G;
        public Byte B;
        public Byte res;
    };
     #endregion

     //回调函数声明
//    [System.Runtime.InteropServices.UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    internal delegate int PMS_SNAPPROC(ref IMAGE_INFO pInfo, IntPtr UserVal);

    class PMSGigE
    {

        public const  Int64   INT64_MAX =0x7fffffffffffffff;  /*maximum signed __int64 value */
        public const  UInt64  INT64_MIN  =   0x8000000000000000;  /*minimum signed __int64 value */
        public const  UInt64 UINT64_MAX  =  0xffffffffffffffff;  /*maximum unsigned __int64 value */

        public const  Int32 INT32_MAX =    0x7fffffff;  /*maximum signed __int32 value */
        public const  UInt32 INT32_MIN =    0x80000000;  /*minimum signed __int32 value */
        public const  UInt32 UINT32_MAX =   0xffffffff;  /*maximum unsigned __int32 value */

        public const UInt16 UINT16_MAX = 0xffff;  /*maximum unsigned __int32 value */

        public const  byte  INT8_MAX =    0x7f ; /*maximum signed __int8 value */
        public const  byte INT8_MIN     =0x80;  /*minimum signed __int8 value */
        public const  byte UINT8_MAX   = 0xff;  /*maximum unsigned __int8 value */

     
        #region"function"

        [DllImport("PMSGigE.dll")]         
        public static extern   PMSSTATUS_CODES PMSInitLib();

        [DllImport("PMSGigE.dll")]  
        public static extern  PMSSTATUS_CODES  PMSTerminateLib();
        
        
        /// <summary>
        /// 查找连接到计算机上的相机
        /// </summary>
        /// <returns></returns>
        [DllImport("PMSGigE.dll")]  
        public static extern  PMSSTATUS_CODES  PMSUpdateCameraList();
        
 
       /// <summary>
         /// 获取连接到计算机上的相机的数量
       /// </summary>
         /// <param name="pNumCams">param [out]	pNumCams	相机数量</param>
         /// <returns>PMSST_SUCCESS	: 成功</returns>

        [DllImport("PMSGigE.dll")]  
        public static extern  PMSSTATUS_CODES  PMSGetNumOfCameras(out int pNumCams);


        /// <summary>
        /// 得到第idx个相机的信息
        /// </summary>
         /// <param name="idx">[in]	idx idx从0开始，按照相机的IP地址排序，地址小的排在前面。</param>
         /// <param name="pCamInfo">pCamInfo [out]  相机的信息 (IP,MAC,SN,型号...)</param>
         /// <returns>	PMSST_SUCCESS	: 成功</returns>
        [DllImport("PMSGigE.dll")] 
        public static extern  PMSSTATUS_CODES  PMSGetCameraInfo(byte  idx,out  PMSCamInfo pCamInfo);


        /// <summary>
        /// 打开第idx个相机
        /// </summary>
        /// <param name="idx">[in]	idx	idx从0开始，按照相机的IP地址排序，地址小的排在前面</param>
        /// <param name="hCam">[out]	hCam 如果成功,返回的相机句柄</param>
        /// <returns> PMSST_INVALID_PARAMETER : idx取值不对 
        ///  PMSST_ACCESS_DENIED: 相机无法访问，可能正被别的软件控制 
        ///  PMSST_ERROR: 其他错误 
        ///  PMSST_SUCCESS	: 成功</returns>
        [DllImport("PMSGigE.dll")] 
        public static extern  PMSSTATUS_CODES  PMSOpenCamByIndex(byte  idx,out IntPtr  hCam);

        /*! 
         *  \brief	打开指定UserDefinedName的相机
         *  \param [in]	name UserDefinedName。
         *  \param [out]	hCam 如果成功,返回的相机句柄
         *  \retval 
         *			PMSST_ACCESS_DENIED		: 相机无法访问，可能正被别的软件控制
         *			PMSST_ERROR				: 其他错误
         *			PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern  PMSSTATUS_CODES  PMSOpenCamByUserDefinedName( string name, out IntPtr  hCam );

		/*! 
		 *  \brief	打开指定IP的相机
		 *  \param [in]	ip 相机的IP地址。
		 *  \param [out] hCam 如果成功,返回的相机句柄。如果失败，为NULL。
		 *  \retval 
		 *			PMSST_ACCESS_DENIED		: 相机无法访问，可能正被别的软件控制
		 *			PMSST_ERROR				: 其他错误
		 *			PMSST_SUCCESS			: 成功
		 */
        [DllImport("PMSGigE.dll")]
        public static extern PMSSTATUS_CODES PMSOpenCamByIP(string ip, out IntPtr hCam);
              /*! 
         *  \brief	关闭相机。断开和相机的连接。
         *  \param [in]	hCam 相机的句柄
         *  \retval 
         */
        [DllImport("PMSGigE.dll")] 
        public static extern  PMSSTATUS_CODES  PMSCloseCam(IntPtr  hCam);

//////////////////////////////////////////////////////////////////////////

        /*!
         * \brief	读取图像宽度
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	int * pWidth	图像宽度[像素]
         * \retval  	PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern  PMSSTATUS_CODES   PMSGetWidth(IntPtr  hCam,out  int pWidth);


        /*!
         * \brief	读取图像高度
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	int * pHeight	图像高度[像素]
         * \retval  	PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern  PMSSTATUS_CODES  PMSGetHeight(IntPtr  hCam,out int pHeight);


        /*!
         * \brief    读取图像的像素格式
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	PMS_PixelFormatEnums * pPixelFormat
         * \retval  	PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern  PMSSTATUS_CODES  PMSGetPixelFormat(IntPtr  hCam,out PMS_PixelFormatEnums pPixelFormat);
    

        /*!
         *  \brief	读取传感器的通道数
         *  \param [in]	HANDLE hCam		相机句柄
         *  \param [out] SensorTapsEnums* pSensorTaps
         *  \retval  	PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetSensorTaps(IntPtr  hCam,out SensorTapsEnums pSensorTaps );

        /*!
         * \brief    读取当前增益值
         * \param [in]	HANDLE hCam	相机句柄
         * \param [out]	double * pGain
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetGain(IntPtr  hCam,out double pGain);

        /*!
         * \brief    读取增益可以设置的范围
         * \param [in]	HANDLE hCam	相机句柄
         * \param [out]	double * pGainMin	最小值
         * \param [out]	double * pGainMax	最大值
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetGainRange(IntPtr  hCam,out  double pGainMin,out double pGainMax);	

        /*!
         * \brief    设置增益
         * \param [in]	HANDLE hCam	相机句柄
         * \param [in]	double fGain	增益
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetGain(IntPtr  hCam,double fGain);

        /*!
         * \brief    当相机传感器为多通道时，设置某个通道的增益
         * \param [in]	HANDLE hCam		相机句柄
         * \param [in]	double fGain	增益
         * \param [in]	int nTap		通道。双通道[0,1],四通道[0,1,2,3]
         *  \retval PMSC_ST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetGainTaps(IntPtr  hCam, double fGain, int nTap );

        /*!
         * \brief    当相机传感器为多通道时，读取某个通道的增益
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	double* pGain
         * \param [in]	int nTap		通道。双通道[0,1],四通道[0,1,2,3]
         *  \retval PMSC_ST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetGainTaps(int hCam,out double pGain,int nTap );

        /*!
         * \brief    当相机传感器为多通道时，读取某个通道的增益可设置的范围
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	double * pGainMin	增益最小值
         * \param [out]	double * pGainMax	增益最大值
         * \param [in]	int nTap		通道。双通道[0,1],四通道[0,1,2,3]
         *  \retval PMSC_ST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetGainRangeTaps(IntPtr  hCam,out double pGainMin,out double pGainMax, int nTap );

        /*!
         * \brief    读取当前白平衡系数
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	double * pRed	红色平衡系数
         * \param [out]	double * pGreen 绿色平衡系数
         * \param [out]	double * pBlue	蓝色平衡系数
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetWhiteBalance( IntPtr  hCam,out double pRed,out double pGreen,out double pBlue );

        /*!
         * \brief	读取白平衡设置的范围
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	double * pMin	系数最小值
         * \param [out]	double * pMax	系数最大值
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetWhiteBalanceRange(IntPtr   hCam,out double pMin,out  double pMax );

        /*!
         * \brief    设置白平衡系数
         * \param [in]	HANDLE hCam		相机句柄
         * \param [in]	double fRed		红色平衡系数
         * \param [in]	double fGreen	绿色平衡系数
         * \param [in]	double fBlue	蓝色平衡系数
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetWhiteBalance(IntPtr  hCam,double fRed,double fGreen, double fBlue );

        /*!
         * \brief    读取是否通道自动平衡
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	int* pBalance	是否自动平衡
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetGainBalance(IntPtr  hCam,out int pBalance );

        /*!
         * \brief    设置是否自动通道平衡
         * \param [in]	HANDLE hCam		相机句柄
         * \param [in]	int nBalance	是否自动通道平衡
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetGainBalance(IntPtr  hCam,int nBalance );

        /*! 
         *  \brief	读取当前曝光时间
         *  \param [in]	hCam
         *  \param [in]	pExposuretime	单位us
         *  \retval 
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetExposureTime(IntPtr  hCam,out double pExposuretime);

        /*!
         * \brief  读取曝光时间的设置范围  
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	double * pExpMin	最短曝光时间 单位为us
         * \param [out]	double * pExpMax	最长曝光时间 单位为us
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetExposureTimeRange(IntPtr  hCam,out  double pExpMin,out double pExpMax);

        /*!
         * \brief	设置曝光时间
         * \param [in]	HANDLE hCam		相机句柄
         * \param [in]	unsigned long nExp_us 曝光时间 单位为us
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetExposureTime(IntPtr  hCam, double nExp_us);

        /*!
         * \brief	读取帧率可设置的范围
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	double* pFpsMin 最低帧率
         * \param [out]	double* pFpsMax 最高帧率	
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetFrameRateRange(IntPtr  hCam,out double pFpsMin,out double pFpsMax);

        /*!
         * \brief	读取当前帧率   
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	double * fFPS	帧率 帧/秒
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetFrameRate(IntPtr  hCam,out double fFPS);

        /*!
         * \brief    设置帧率
         * \param [in]	HANDLE hCam		相机句柄
         * \param [in]	double fps	帧率 帧/秒
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetFrameRate(IntPtr  hCam,double fps);

        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSStartGrab(IntPtr  hCam,PMS_SNAPPROC callbackFunc,IntPtr nUserVal);
                /*!
	     *  \brief 采集一帧图像。
	     *  \param [in]	hCam	相机句柄
	     *  \param [out]	hImage	图像句柄。保存采集到的图像。
	     *  \param [in]	 nWaitMs	等待多长时间，单位ms
	     *  \retval  	PMSST_SUCCESS			: 成功
	     */
        [DllImport("PMSGigE.dll")] 
	    public static extern PMSSTATUS_CODES PMSSingleGrab(IntPtr hCam, IntPtr hImage, UInt64 nWaitMs);
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSStopGrab(IntPtr  hCam);


        /*!
         * \brief    读取触发模式
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	TriggerModeEnums * pMode	触发模式  TriggerMode_Off,TriggerMode_On
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetTriggerMode(IntPtr  hCam,out TriggerModeEnums pMode);

        /*! 
         *  \brief	设置触发模式
         * \param [in]	HANDLE hCam		相机句柄
         *  \param [in]	mode	触发模式
	        TriggerMode_Off：相机工作在连续采集模式，
	        TriggerMode_On:相机工作在触发模式，需要有外触发信号或软触发指令才拍摄
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetTriggerMode(IntPtr  hCam,TriggerModeEnums mode);

        /*!
         * \brief    读取触发源
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	TriggerSourceEnums * pSource	触发源，软触发或外触发
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetTriggerSource(IntPtr  hCam,out TriggerSourceEnums pSource);
        /*! 
         *  \brief	设置触发源
         * \param [in]	HANDLE hCam		相机句柄
         *  \param [in]	TriggerSourceEnums	source 触发源
				        TriggerSource_Software：通过\c PMSTriggerSoftware()函数触发。
				        TriggerSource_Line1：通过连接的触发线触发。
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetTriggerSource(IntPtr  hCam,TriggerSourceEnums source);

        /*!
         * \brief    读取触发极性
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	TriggerActivationEnums * pAct
				        TriggerActivation_RisingEdge: 上升沿触发
				        TriggerActivation_FallingEdge: 下降沿触发
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetTriggerActivation(IntPtr  hCam,out TriggerActivationEnums pAct);
        /*! 
         *  \brief	当使用触发线触发时,设置是上升沿触发还是下降沿触发
         *  \param [in]	hCam
         *  \param [in]	act 上升沿或下降沿
				        TriggerActivation_RisingEdge: 上升沿触发
				        TriggerActivation_FallingEdge: 下降沿触发
         *  \retval 
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetTriggerActivation(IntPtr  hCam,TriggerActivationEnums act);

        /*!
         * \brief	读取触发延时
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	uint32_t * pDelay_us	触发延时,单位us
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetTriggerDelay(IntPtr  hCam,out UInt32  pDelay_us);

        /*!
         * \brief    读取触发延时范围
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	uint32_t * pMin	触发延时最小值,单位us	
         * \param [out]	uint32_t * pMax 触发延时最大值,单位us	
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetTriggerDelayRange(IntPtr  hCam,out UInt32  pMin,out UInt32  pMax);
        /*! 
         *  \brief	设置相机接到触发信号后延迟多少微秒后再开始曝光。
         *  \param [in]	HANDLE hCam		相机句柄
         *  \param [in]	nDelay_us
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetTriggerDelay(IntPtr  hCam,UInt32  nDelay_us);

        /*! 
         *  \brief	发出软件触发指令
         *  \param [in]	HANDLE hCam		相机句柄
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSTriggerSoftware(IntPtr  hCam);

        /*!
         * \brief	读取闪光同步信号源
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	LineSourceEnums * pSource	闪光同步信号源
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetStrobeSource(IntPtr  hCam,out LineSourceEnums pSource);
        /*! 
         *  \brief	闪光同步信号源
         *  \param [in]	hCam
         *  \param [in]	source
				        LineSource_Off：关闭闪光同步
				        LineSource_ExposureActive：曝光的同时闪光
				        LineSource_Timer1Active：由定时器控制
				        LineSource_UserOutput0：由用户通过指令控制
         *  \retval 
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetStrobeSource(IntPtr  hCam,LineSourceEnums source);

        /*!
         * \brief	读取闪光同步是否反转
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	BOOL * pInvert
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetStrobeInvert(IntPtr  hCam,out  bool pInvert);

        /*! 
         *  \brief	闪光同步是否反转，即闪光同步有效时输出高电平还是低电平。
         *  \param [in]	HANDLE hCam		相机句柄
         *  \param [in]	bInvert
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetStrobeInvert(IntPtr  hCam,bool bInvert);

        /*!
         * \brief	读取用户设置的闪光同步
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	BOOL * pSet
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetUserOutputValue0(IntPtr  hCam,out bool pSet);

        /*! 
         *  \brief	当闪光同步源选为UserOutput时
			        主机可以通过PMSSetUserOutputValue0来控制闪光同步输出高电平或低电平。
         *  \param [in]	HANDLE hCam		相机句柄
         *  \param [in]	bSet 设置电平
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetUserOutputValue0(IntPtr hCam,bool bSet);

        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetHeartbeatTimeout(IntPtr hCam,ulong nTimeOut);//unit ms

        /*!
         * \brief	读取网络数据包大小
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	int *pPacketSize 数据包大小
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetPacketSize(IntPtr hCam, out uint pPacketSize);
        /*!
         * \brief    读取网络数据包范围
         * \param [in]	HANDLE hCam			相机句柄
         * \param [out]	unsigned int * pMin	网络数据包最小值	
         * \param [out]	unsigned int * pMax 网络数据包最大值	
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetPacketSizeRange(IntPtr hCam,out  uint pMin,out uint pMax);

        /*! 
         *  \brief	设置网络数据包的大小。
         *  \param [in]	hCam
         *  \param [in]	nPacketSize 网络数据包大小(单位:字节)。该大小应该小于网卡能够支持的最大巨型帧。
         *  \retval 
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetPacketSize(IntPtr hCam,uint nPacketSize);

        /*!
         * \brief	读取网络数据包间隔
         * \param [in]	HANDLE hCam				相机句柄
         * \param [out]	unsigned int *pDelay_us 数据包间隔时间，单位us
         *  \retval PMSST_SUCCESS				: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetPacketDelay(IntPtr hCam,out uint pDelay_us);
        /*!
         * \brief    读取网络数据包范围
         * \param [in]	HANDLE hCam			相机句柄
         * \param [out]	unsigned int * pMin	数据包间隔时间最小值，单位us	
         * \param [out]	unsigned int * pMax 数据包间隔时间最大值，单位us	
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetPacketDelayRange(IntPtr hCam,out  uint pMin,out uint pMax);


        /*! 
         *  \brief	设置网络数据包之间的时间间隔。如果网卡或电脑的性能欠佳，无法处理高速到达的数据包，
			        可以增加数据包之间的时间间隔以保证图像传输。但是增加该值将增加图像的时间延迟，并有可能影像到帧率。
         *  \param [in]	hCam
         *  \param [in]	nDelay_us 时间间隔(单位:微秒)
         *  \retval 
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetPacketDelay(IntPtr hCam,uint nDelay_us);

        /*!
         * \brief    读取定时器延时
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	uint32_t * pDelay	定时器延时
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetTimerDelay(IntPtr hCam,out UInt32  pDelay);

        /*!
         * \brief	读取定时器延时的范围
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	uint32_t * pMin	定时器延时的最小值
         * \param [out]	uint32_t * pMax 定时器延时的最大值
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")]
        public static extern PMSSTATUS_CODES PMSGetTimerDelayRange(IntPtr hCam, out UInt32 pMin, out UInt32 pMax);

        /*! 
         *  \brief	当闪光同步源选为Timer1时,设置Timer1在接到触发信号后延迟多少us开始计时
         *  \param [in]	HANDLE hCam		相机句柄
         *  \param [in]	uint32_t nDelay 接到触发信号后延迟多少us开始计时
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetTimerDelay(IntPtr hCam,UInt32  nDelay);

        /*!
         * \brief    读取定时器计时时长
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	uint32_t * pDuration	定时器计时时长
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetTimerDuration(IntPtr hCam,out UInt32  pDuration);
        /*!
         * \brief    读取定时器计时时长取值范围
         * \param [in]	HANDLE hCam		相机句柄
         * \param [out]	uint32_t * pMin	定时器计时时长最小值
         * \param [out]	uint32_t * pMax	定时器计时时长最大值
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetTimerDurationRange(IntPtr hCam,out UInt32  pMin,out UInt32  pMax);

        /*! 
        *  \brief	当闪光同步源选为Timer1时,设置Timer1在开始计时后，计时多长时间。
         *  \param [in]	HANDLE hCam
         *  \param [in]	uint32_t nDuration 设置Timer1在开始计时后，计时多长时间(us)。即输出高/低电平的脉冲宽度。
         *  \retval PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetTimerDuration(IntPtr hCam,UInt32  nDuration);

        //[DllImport("PMSGigE.dll")] 
        // public static extern PMSSTATUS_CODES  PMSTRACE(byte format);
        /// <summary>
        /// Bayer格式图像数据到RGB格式图像数据转换 ( 转成RGB24位)
        /// </summary>
        /// <param name="hCam">该相机句柄</param>
        /// <param name="psrc">原始图像数据指针</param>
        /// <param name="pdst">转换后图像存储内存指针</param>
        /// <param name="dststep">转换步长</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <param name="pixelformat">像素格式</param>
        /// <returns>成功返回Success,否则返回错误信息</returns>
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSBayerToBGR(IntPtr hCam, IntPtr  psrc,IntPtr  pdst,uint dststep,uint width,uint height,PMS_PixelFormatEnums pixelformat);
        [DllImport("PMSGigE.dll")]
        public static extern PMSSTATUS_CODES PMSInfo2Image(IntPtr hCam, ref PMSAPI.IMAGE_INFO pInfo , IntPtr pImage);
        /// <summary>
        /// <summary>
        /// 图像缩放
        /// </summary>
        /// <param name="hCam">该相机句柄</param>
        /// <param name="pSrc">原始图像指针</param>
        /// <param name="srcWidth">原始图像宽度</param>
        /// <param name="srcHeight">原始图像高度</param>
        /// <param name="pDst">缩放后存放图像内存指针</param>
        /// <param name="fFactorX">水平方向缩放因子</param>
        /// <param name="fFactorY">垂直方向缩放因子</param>
        /// <returns>成功返回Success,否则返回错误信息</returns>
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSZoomImageBGR(IntPtr hCam,IntPtr  pSrc,int srcWidth,int srcHeight, out IntPtr  pDst,double fFactorX,double fFactorY);
        [DllImport("PMSGigE.dll")] 

        public static extern PMSSTATUS_CODES  PMSGetStreamStatistic(IntPtr hCam, PMSStreamStatistic pStatistic );



        /*!
         *  \brief	读取并应用某组用户预设的参数
         *  \param [in]	HANDLE hCam		相机句柄
         *  \param [in]	UserSetSelectorEnums userset
         *  \retval  	PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSLoadUserSet(IntPtr hCam,UserSetSelectorEnums userset);

        /*!
         *  \brief	将当前相机的参数保存到用户设置中
         *  \param [in]	HANDLE hCam		相机句柄
         *  \param [in]	UserSetSelectorEnums userset
         *  \retval  	PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSaveUserSet(IntPtr hCam,UserSetSelectorEnums userset);

        /*!
         *  \brief	设置相机上电开机时默认读取并应用哪一组用户设置
         *  \param [in]	HANDLE hCam		相机句柄
         *  \param [in]	UserSetSelectorEnums userset
         *  \retval  	PMSST_SUCCESS			: 成功
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSSetDefaultUserSet(IntPtr hCam,UserSetSelectorEnums userset);

        /*!
         *  \brief	读取相机上电开机时默认读取并应用哪一组用户设置
         *  \param [in]	HANDLE hCam		相机句柄
         *  \param [out]	UserSetSelectorEnums* pUserset	用户设置
         *  \retval  	PMSGIGE_API PMSSTATUS_CODES __stdcall
         */
        [DllImport("PMSGigE.dll")] 
        public static extern PMSSTATUS_CODES  PMSGetDefaultUserSet(IntPtr hCam,out UserSetSelectorEnums pUserset);
        
        /*!
	     *  \brief 图像翻转
	     *  \param [in]	hCam	相机句柄
	     *  \param [in]	 pSrcImage	源图像指针
	     *  \param [out] pDstImage	结果图像指针。如果为NULL，则翻转的结果还在源图像内。
	     *  \param [in]	flipType	翻转类型。FlipHorizontal:左右翻转,FlipVertical:上下翻转,FlipBoth:旋转180度
	     *  \retval  	PMSST_SUCCESS			: 成功
	     */
        [DllImport("PMSGigE.dll")] 
	    public static extern PMSSTATUS_CODES PMSImageFlip(IntPtr hCam, IntPtr pSrcImage, IntPtr pDstImage, ImageFlipType flipType);
	    /*!
	     *  \brief 图像旋转
	     *  \param [in]	hCam	相机句柄
	     *  \param [in]	 pSrcImage	源图像指针
	     *  \param [out] pDstImage	结果图像指针,不能为NULL。结果图像的宽度和高度应该和源图像的宽度和高度互换。
	     *  \param [in]	roateType	旋转类型：Rotate90DegCw, Rotate90DegCcw
	     *  \retval  	PMSST_SUCCESS			: 成功
	     */
        [DllImport("PMSGigE.dll")] 
	    public static extern PMSSTATUS_CODES PMSImageRotate(IntPtr hCam, IntPtr pSrcImage, IntPtr pDstImage, ImageRotateType roateType);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        #endregion
    }
    class PMSImage
    {
        #region"function"
        [DllImport("PMSGigE.dll")]
        public static extern IntPtr PMSImageCreate(int nWidth, int nHeight, int nBPP);
        [DllImport("PMSGigE.dll")]
        public static extern bool PMSImageIsNull(IntPtr hImage);
        [DllImport("PMSGigE.dll")]
        public static extern int PMSImageGetWidth(IntPtr hImage);
        [DllImport("PMSGigE.dll")]
        public static extern int PMSImageGetHeight(IntPtr hImage);
        [DllImport("PMSGigE.dll")]
        public static extern IntPtr PMSImageGetBits(IntPtr hImage);
        [DllImport("PMSGigE.dll")]
        public static extern int PMSImageGetPitch(IntPtr hImage);
        [DllImport("PMSGigE.dll")]
        public static extern int PMSImageGetBPP(IntPtr hImage);
        [DllImport("PMSGigE.dll")]
        public static extern bool PMSImageDraw(IntPtr hImage, IntPtr hDestDC, int xDest, int yDest);
        [DllImport("PMSGigE.dll")]
        public static extern bool PMSImageDrawEx(IntPtr hImage, IntPtr hDestDC, int xDest, int yDest, int nDestWidth, int nDestHeight, int xSrc, int ySrc, int nSrcWidth, int nSrcHeight);
        [DllImport("PMSGigE.dll")]
        public static extern bool PMSImageDrawHwnd(IntPtr hImage, IntPtr hWnd, int xDest, int yDest);
        [DllImport("PMSGigE.dll")]
        public static extern bool PMSImageDrawHwndEx(IntPtr hImage, IntPtr hWnd, int xDest, int yDest, int nDestWidth, int nDestHeight, int xSrc, int ySrc, int nSrcWidth, int nSrcHeight);
        [DllImport("PMSGigE.dll")]
        public static extern IntPtr PMSImageGetDC(IntPtr hImage);
        [DllImport("PMSGigE.dll")]
        public static extern IntPtr PMSImageReleaseDC(IntPtr hImage);
        [DllImport("PMSGigE.dll")]
        public static extern void PMSImageSave(IntPtr hImage, string strFileName);
        [DllImport("PMSGigE.dll")]
        public static extern void PMSImageDestroy(IntPtr hImage);
        [DllImport("PMSGigE.dll")]
        public static extern void PMSImageRelease(IntPtr hImage);
        #endregion
    }
    
}
