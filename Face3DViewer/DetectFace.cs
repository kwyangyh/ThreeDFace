using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;


namespace ThreeDFaces
{
    public enum CvFilter
    {
        CV_GAUSSIAN_5x5 = 7,
    }

    public enum CvInterpolation
    {
        CV_INTER_CUBIC = 2,
    }


    class DllPaths
    {
        public const string KERNEL32_DLL_PATH = "kernel32.dll";
        public const string OPENCV_CORE_DLL_PATH = "opencv_core220.dll";
        public const string OPENCV_OBJDETECT_DLL_PATH = "opencv_objdetect220.dll";
        public const string OPENCV_IMGPROC_DLL_PATH = "opencv_imgproc220.dll";
        public const string OPENCV_HIGHGUI_PATH = "opencv_highgui220.dll";
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct IplImage
    {
        public int nSize;
        public int ID;
        public int nChannels;
        public int alphaChannel;
        public int depth;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string colorModel;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string channelSeq;
        public int dataOrder;
        public int origin;
        public int align;
        public int width;
        public int height;
        public System.IntPtr roi;
        public System.IntPtr maskROI;
        public System.IntPtr imageId;
        public System.IntPtr tileInfo;
        public int imageSize;
        public System.IntPtr imageData;
        public int widthStep;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I4)]
        public int[] BorderMode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I4)]
        public int[] BorderConst;
        public System.IntPtr imageDataOrigin;
        public IntPtr ptr;
        public static explicit operator Bitmap(IplImage img)
        {
            return CDetectFace.ToBitmap(img, false);
        }

    }


    [StructLayout(LayoutKind.Explicit)]
    public struct CvMat
    {
        /// <summary>
        /// CvMat signature (CV_MAT_MAGIC_VAL), element type and flags
        /// </summary>
        [FieldOffset(0)]
        public int type;

        /// <summary>
        /// full row length in bytes
        /// </summary>
        [FieldOffset(4)]
        public int step;

        /// <summary>
        /// for internal use only
        /// </summary>
        [FieldOffset(8)]
        public IntPtr refcount;

        /// <summary>
        /// for internal use only
        /// </summary>
        [FieldOffset(12)]
        public int hdr_refcount;

        /// <summary>
        /// underlaying data pointer
        /// </summary>
        [FieldOffset(16)]
        public IntPtr data;

        /// <summary>
        /// number of rows
        /// </summary>
        [FieldOffset(20)]
        public int rows;

        /// <summary>
        /// number of rows
        /// </summary>
        [FieldOffset(20)]
        public int height;

        /// <summary>
        /// number of columns
        /// </summary>
        [FieldOffset(24)]
        public int cols;

        /// <summary>
        /// number of columns
        /// </summary>
        [FieldOffset(24)]
        public int width;

        /// <summary>
        /// this pointer
        /// </summary>
        [FieldOffset(28)]
        public IntPtr ptr;
    }

    /// <summary>
    /// pairs (number of elements, distance between elements in bytes) for every dimension
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Dim
    {
        /// <summary>
        /// size
        /// </summary>
        public int size;

        /// <summary>
        /// step
        /// </summary>
        public int step;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CvSize
    {
        public int width;
        public int height;

        public CvSize(int width, int height) { this.width = width; this.height = height; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CvRect
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public CvRect(int x, int y, int width, int height)
        {
            this.x = x; this.y = y;
            this.width = width; this.height = height; 
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct CvPoint
    {
        /// <summary>
        /// x-coordinate
        /// </summary>
        public int x;

        /// <summary>
        /// y-coordinate
        /// </summary>
        public int y;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="x">x-coordinate</param>
        /// <param name="y">y-coordinate</param>
        public CvPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CvFont
    {
        public int font_face;
        public IntPtr ascii;
        public IntPtr greek;
        public IntPtr cyrillic;
        public float hscale, vscale;
        public float shear;
        public int thickness;
        public float dx;
        public int line_type;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CvScalar
    {
        /// <summary>
        /// value 1
        /// </summary>
        public double val1;

        /// <summary>
        /// value 2
        /// </summary>
        public double val2;

        /// <summary>
        /// value 3
        /// </summary>
        public double val3;

        /// <summary>
        /// value 4
        /// </summary>
        public double val4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v1">value 1</param>
        /// <param name="v2">value 2</param>
        /// <param name="v3">value 3</param>
        /// <param name="v4">value 4</param>
        public CvScalar(double v1, double v2, double v3, double v4)
        {
            val1 = v1; val2 = v2; val3 = v3; val4 = v4;
        }


    }

    public static class NativeMethods
    {

        public const int CV_CN_SHIFT = 3;
        public const int CV_8U = 0;
        public const int CV_8S = 1;
        public const int CV_16U = 2;
        public const int CV_16S = 3;
        public const int CV_32S = 4;
        public const int CV_32F = 5;
        public const int CV_64F = 6;

        public static int CV_MAKETYPE(int depth, int cn)
        {
            return ((depth) + (((cn) - 1) << CV_CN_SHIFT));
        }



        public static int CV_8UC1 = CV_MAKETYPE(CV_8U, 1);
        public static int CV_8UC2 = CV_MAKETYPE(CV_8U, 2);
        public static int CV_8UC3 = CV_MAKETYPE(CV_8U, 3);
        public static int CV_8UC4 = CV_MAKETYPE(CV_8U, 4);


        /* basic font types */
        public const int CV_FONT_HERSHEY_SIMPLEX = 0;
        public const int CV_FONT_HERSHEY_PLAIN = 1;
        public const int CV_FONT_HERSHEY_DUPLEX = 2;
        public const int CV_FONT_HERSHEY_COMPLEX = 3;
        public const int CV_FONT_HERSHEY_TRIPLEX = 4;
        public const int CV_FONT_HERSHEY_COMPLEX_SMALL = 5;
        public const int CV_FONT_HERSHEY_SCRIPT_SIMPLEX = 6;
        public const int CV_FONT_HERSHEY_SCRIPT_COMPLEX = 7;


        /* font flags */
        public const int CV_FONT_ITALIC = 16;

        public const int CV_FONT_VECTOR0 = CV_FONT_HERSHEY_PLAIN;//CV_FONT_HERSHEY_SIMPLEX;



        public const int IPL_DEPTH_8U = 8;
        public const int CV_INTER_CUBIC = 2;


        public const int CV_THRESH_BINARY = 0			/* value = value threshold max_value 0 */;


        public const int CV_BGR2YCrCb = 36;
        public const int CV_BGR2GRAY = 6;
        public const int CV_YCrCb2BGR = 38;

        public const int CV_LOAD_IMAGE_GRAYSCALE = 0;
        public const int CV_LOAD_IMAGE_ANYCOLOR = 4;

        /// <summary>
        /// 8 bit unless combined with CV_LOAD_IMAGE_ANYDEPTH, color
        /// </summary>
        public const int CV_LOAD_IMAGE_COLOR = 1;

        /// <summary>
        /// any depth, if specified on its own gray
        /// </summary>
        public const int CV_LOAD_IMAGE_ANYDEPTH = 2;

        [DllImport(DllPaths.KERNEL32_DLL_PATH, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport(DllPaths.KERNEL32_DLL_PATH, SetLastError=true)]
        public static extern bool FreeLibrary(IntPtr hModule);


        [DllImport(DllPaths.KERNEL32_DLL_PATH, EntryPoint = "CopyMemory")]
        public static extern void memcpy(IntPtr dest, IntPtr src, int len);


        //public static IplImage CvLoadImage(string path, int flags)
        //{
        //    IntPtr p = cvLoadImage(path, flags);
        //    IplImage i;
        //    i= (IplImage)Marshal.PtrToStructure(p, typeof(IplImage));
        //    i.imageData = p;
        //    return i;
        //}



        public static void CvCopy(ref IplImage src, ref IplImage dst, ref IplImage mask)
        {
            cvCopy(ref src, ref dst, ref mask);
        }
        public static void CvCopy(ref CvMat src, ref CvMat dst, ref CvMat mask)
        {
            cvCopy(ref src, ref dst, ref mask);
        }

        public static void CvCopy(ref IplImage src, ref IplImage dst)
        {
            cvCopy(ref src, ref dst, IntPtr.Zero);
        }
        public static void CvCopy(ref CvMat src, ref CvMat dst)
        {
            cvCopy(ref src, ref dst, IntPtr.Zero);
        }

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern void cvCopy(ref IplImage src, ref IplImage dst, ref IplImage mask);

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern void cvCopy(ref CvMat src, ref CvMat dst, ref CvMat mask);

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern void cvCopy(ref IplImage src, ref IplImage dst, IntPtr mask);

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern void cvCopy(ref CvMat src, ref CvMat dst, IntPtr mask);


        public static CvMat CvGetSubRect(ref IplImage arr, ref CvMat submat, CvRect rect)
        {
            IntPtr p = cvGetSubRect(ref arr, ref submat, rect);
            CvMat i = (CvMat)Marshal.PtrToStructure(p, typeof(CvMat));
            i.ptr = p;
            return i;
        }
        public static CvMat CvGetSubRect(ref CvMat arr, ref CvMat submat, CvRect rect)
        {
            IntPtr p = cvGetSubRect(ref arr, ref submat, rect);
            CvMat i = (CvMat)Marshal.PtrToStructure(p, typeof(CvMat));
            i.ptr = p;
            return i;
        }

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern IntPtr cvGetSubRect(ref IplImage arr, ref CvMat submat, CvRect rect);
        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern IntPtr cvGetSubRect(ref CvMat arr, ref CvMat submat, CvRect rect);




        public static CvMat  CvLoadImageM(string path, int flags)
        {
            IntPtr p = cvLoadImageM(path, flags);
            CvMat i;
            i = (CvMat)Marshal.PtrToStructure(p, typeof(CvMat));
            i.ptr = p;
            return i;
        }
        [DllImport(DllPaths.OPENCV_HIGHGUI_PATH)]
        private static extern IntPtr cvLoadImageM([MarshalAs(UnmanagedType.LPStr)] String filename, int flags);


        public static void CvSetZero(ref CvMat arr)
        {
            cvSetZero(ref arr);
        }
        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern void cvSetZero(ref CvMat arr);

        public static void CvSet2D(ref CvMat arr, int idx0, int idx1, CvScalar value)
        {
            cvSet2D(ref arr, idx0, idx1, value);
        }
        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern void cvSet2D(ref CvMat arr, int idx0, int idx1, CvScalar value);



        public static void CvReleaseMat(ref CvMat mat)
        {
            cvReleaseMat(ref mat.ptr);
        }

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern void cvReleaseMat(ref IntPtr mat);

        public static void CvFilter2D(ref IplImage src, ref IplImage dst, ref CvMat kernel, CvPoint anchor)
        {
            cvFilter2D(ref src, ref dst, ref kernel, anchor);
        }
        public static void CvFilter2D(ref CvMat src, ref CvMat dst, ref CvMat kernel, CvPoint anchor)
        {
            cvFilter2D(ref src, ref dst, ref kernel, anchor);
        }

        [DllImport(DllPaths.OPENCV_IMGPROC_DLL_PATH)]
        private static extern void cvFilter2D(ref IplImage src, ref IplImage dst, ref CvMat kernel, CvPoint anchor);
        [DllImport(DllPaths.OPENCV_HIGHGUI_PATH)]
        private static extern void cvFilter2D(ref CvMat src, ref CvMat dst, ref CvMat kernel, CvPoint anchor);



        public static CvMat CvGetMat(ref IplImage arr, ref CvMat header, IntPtr coi, int allowND)
        {
            IntPtr p = cvGetMat(ref arr, ref header, coi, allowND);
            CvMat i = (CvMat)Marshal.PtrToStructure(p, typeof(CvMat));
            i.ptr = p;
            return i;
        }

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvGetMat", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr cvGetMat(ref IplImage arr, ref CvMat header, IntPtr coi, int allowND);



        public static CvMat CvCreateMat(int rows, int cols, int type)
        {
            IntPtr p = cvCreateMat(rows, cols, type);
            CvMat i = (CvMat)Marshal.PtrToStructure(p, typeof(CvMat));
            i.ptr = p;
            return i;
        }

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvCreateMat", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr cvCreateMat(int rows, int cols, int type);

        public static CvRect CvGetImageROI(ref IplImage image)
        {
            return cvGetImageROI(ref image);
        }
        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvGetImageROI", CallingConvention = CallingConvention.Cdecl)]

        private static extern CvRect cvGetImageROI(ref IplImage image);

        public static void CvThreshold(ref IplImage src, ref IplImage dst, double threshold, double max_value, int threshold_type)
        {
            cvThreshold(ref src, ref dst, threshold, max_value, threshold_type);
        }

        [DllImport(DllPaths.OPENCV_IMGPROC_DLL_PATH, EntryPoint = "cvThreshold", CallingConvention = CallingConvention.Cdecl)]
        private static extern void cvThreshold(ref IplImage src, ref IplImage dst, double threshold, double max_value, int threshold_type);

        public static void CvAbsDiff(ref IplImage src1, ref IplImage src2, ref IplImage dst)
        {
            cvAbsDiff(ref src1, ref src2, ref dst);
        }

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvAbsDiff", CallingConvention = CallingConvention.Cdecl)]
        private static extern void cvAbsDiff(ref IplImage src1, ref IplImage src2, ref IplImage dst);

        public static void CvMerge(ref IplImage src0, ref IplImage src1, ref IplImage src2, ref IplImage dst)
        {
            cvMerge(ref src0, ref src1, ref src2, IntPtr.Zero, ref dst);
        }

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvMerge", CallingConvention = CallingConvention.Cdecl)]
        private static extern void cvMerge(ref IplImage src0, ref IplImage src1, ref IplImage src2, IntPtr src3, ref IplImage dst);


        public static void CvSplit(ref IplImage src, ref IplImage dst0, ref IplImage dst1, ref IplImage dst2)
        {
            cvSplit(ref src, ref dst0, ref dst1, ref dst2, IntPtr.Zero);
        }

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvSplit", CallingConvention = CallingConvention.Cdecl)]
        private static extern void cvSplit(ref IplImage src, ref IplImage dst0, ref IplImage dst1, ref IplImage dst2, IntPtr dst3);

        [DllImport(DllPaths.OPENCV_HIGHGUI_PATH, EntryPoint = "cvLoadImage", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr cvLoadImage([MarshalAs(UnmanagedType.LPStr)] String filename, int flags);


       // We pass IntPtr.Zero to the third parameter as required by cvSaveImage function
        public static void CvSaveImage(string path, IntPtr _img)
        {
            IplImage img=(IplImage)Marshal.PtrToStructure(_img, typeof(IplImage));
            cvSaveImage(path, ref img, IntPtr.Zero );
        }
       
        /// <summary>
        /// Include a third paramter as required by the external function 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="img"></param>
        /// <param name="dummy"></param>
        [DllImport(DllPaths.OPENCV_HIGHGUI_PATH, EntryPoint = "cvSaveImage", CallingConvention = CallingConvention.Cdecl)]
        private static extern void cvSaveImage([MarshalAs(UnmanagedType.LPStr)] String filename, ref IplImage img,IntPtr dummy);


        public static void CvCvtColor(IntPtr _src, IntPtr _dst, int code)
        {
            IplImage src = (IplImage)Marshal.PtrToStructure(_src, typeof(IplImage));
            IplImage dst = (IplImage)Marshal.PtrToStructure(_dst, typeof(IplImage));
            cvCvtColor(ref src, ref dst, code);
        }

        [DllImport(DllPaths.OPENCV_IMGPROC_DLL_PATH, EntryPoint = "cvCvtColor", CallingConvention = CallingConvention.Cdecl)]
        private static extern void cvCvtColor(ref IplImage src, ref IplImage dst, int code);

        public static void CvEqualizeHist(IntPtr _src, IntPtr _dst)
        {
            IplImage src = (IplImage)Marshal.PtrToStructure(_src, typeof(IplImage));
            IplImage dst = (IplImage)Marshal.PtrToStructure(_dst, typeof(IplImage));
            cvEqualizeHist(ref src, ref dst);
        }

        [DllImport(DllPaths.OPENCV_IMGPROC_DLL_PATH, EntryPoint = "cvEqualizeHist", CallingConvention = CallingConvention.Cdecl)]
        private static extern void cvEqualizeHist(ref IplImage src, ref IplImage dst);

        public static void CvInitFont(ref CvFont _font, int font_face, double hscale, double vscale, double shear, int thickness, int line_type)
        {
           // CvFont _font = (CvFont)Marshal.PtrToStructure(font, typeof(CvFont));
            cvInitFont(ref _font, font_face, hscale, vscale, shear, thickness, line_type);
        }

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern void cvInitFont(ref CvFont font, int font_face, double hscale, double vscale, double shear, int thickness, int line_type);


        public static void CvPutText(IntPtr img, string text, CvPoint org, ref CvFont _font, CvScalar color)
        {
       //     CvFont _font = (CvFont )Marshal.PtrToStructure(font, typeof(CvFont));
            IplImage _img = (IplImage)Marshal.PtrToStructure(img, typeof(IplImage));
            cvPutText(ref _img, text, org, ref _font, color);
        }

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern void cvPutText(ref IplImage img, [MarshalAs(UnmanagedType.LPStr)] String text, CvPoint org, ref CvFont font, CvScalar color);


        public static void CvRectangle(IntPtr img, CvPoint pt1, CvPoint pt2, CvScalar color, int thickness, int line_type, int shift)
        {
            IplImage _img = (IplImage)Marshal.PtrToStructure(img, typeof(IplImage));
            cvRectangle(ref _img, pt1, pt2, color, thickness, line_type, shift);
        }

       [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, [MarshalAs(UnmanagedType.U4)] int Length);


        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, CharSet = CharSet.Ansi)]
        private static extern void cvRectangle(ref IplImage img, CvPoint pt1, CvPoint pt2, CvScalar color, int thickness, int line_type, int shift);




        public static void CvPyrDown(IntPtr src, IntPtr dst, int filter)
        {
            IplImage _src = (IplImage)Marshal.PtrToStructure(src, typeof(IplImage));
            IplImage _dst = (IplImage)Marshal.PtrToStructure(dst, typeof(IplImage));
            cvPyrDown(ref _src, ref _dst, filter);
        }

        [DllImport(DllPaths.OPENCV_IMGPROC_DLL_PATH, EntryPoint = "cvPyrDown", CallingConvention = CallingConvention.Cdecl)]
        private static extern void cvPyrDown(ref IplImage src, ref IplImage dst, int filter);

        public static void CvResize(IntPtr src, IntPtr dst, int interpolation)
        {
            IplImage _src = (IplImage)Marshal.PtrToStructure(src, typeof(IplImage));
            IplImage _dst = (IplImage)Marshal.PtrToStructure(dst, typeof(IplImage));
            cvResize( ref _src, ref _dst, interpolation);
        }

        [DllImport(DllPaths.OPENCV_IMGPROC_DLL_PATH, EntryPoint = "cvResize", CallingConvention = CallingConvention.Cdecl)]
        private static extern void cvResize(ref IplImage src, ref IplImage dst, int interpolation);


        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvCreateImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvCreateImage(CvSize size, int depth, int channels);

        //public static IplImage CvCreateImage(CvSize size, int depth, int channels)
        //{
        //    IntPtr p = cvCreateImage(size, depth, channels);
        //    IplImage i = (IplImage)Marshal.PtrToStructure(p, typeof(IplImage));
        //    i.imageData = p;
        //    return i;
        //}


        public static void CvReleaseImage_(ref IplImage image)
        {
            IntPtr p = image.ptr;
            cvReleaseImage(ref p);
        }

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvReleaseImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvReleaseImage(ref IntPtr image);

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvLoad", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvLoad([In][MarshalAs(UnmanagedType.LPStr)]string filename, IntPtr storage, IntPtr name, 
            IntPtr real_name);

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvCreateMemStorage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvCreateMemStorage(int block_size = 0);

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvClearMemStorage", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvClearMemStorage(IntPtr memory_storage);

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvReleaseMemStorage", CallingConvention = CallingConvention.Cdecl)]
        public static extern void cvReleaseMemStorage(ref IntPtr storage);

        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvGetSeqElem", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvGetSeqElem(IntPtr sequence, int index);
        
        [DllImport(DllPaths.OPENCV_CORE_DLL_PATH, EntryPoint = "cvSeqPopFront", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvSeqPopFront(IntPtr sequence, IntPtr dest);

        [DllImport(DllPaths.OPENCV_OBJDETECT_DLL_PATH, EntryPoint = "cvReleaseHaarClassifierCascade", CallingConvention = 
            CallingConvention.Cdecl)]
        public static extern void cvReleaseHaarClassifierCascade(ref IntPtr cascade);

        [DllImport(DllPaths.OPENCV_OBJDETECT_DLL_PATH, EntryPoint = "cvHaarDetectObjects", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr cvHaarDetectObjects(IntPtr image, IntPtr cascade, IntPtr mem_storage,
            double scale_factor, int min_neighbours, int flags,
            CvSize min_size, CvSize max_size);




    }

    public class IntelImage : IDisposable
    {        
        IntPtr iplImagePointer;
        IplImage iplImageStruct;

        public IntelImage(IplImage image)
        {

            iplImagePointer = NativeMethods.cvCreateImage(new CvSize() { width = image.width , height = image.height  }, 8, image.nChannels>1 ? 3 : 1);
            iplImageStruct = (IplImage)Marshal.PtrToStructure(iplImagePointer, typeof(IplImage));
            NativeMethods.CopyMemory(iplImageStruct.imageData, image.imageData, image.height * image.widthStep);
            //CopyPixels((byte[])image.imageData);
        }

        public IntelImage(int width, int height, bool color = true)
        {
            iplImagePointer = NativeMethods.cvCreateImage(new CvSize() { width = width, height = height }, 8, color ? 3 : 1);
            iplImageStruct = (IplImage)Marshal.PtrToStructure(iplImagePointer, typeof(IplImage));
        }


        public void CopyPixels(byte[] sourcePixelBuffer, int startIndex = 0)
        {
            Marshal.Copy(sourcePixelBuffer, startIndex, iplImageStruct.imageData, sourcePixelBuffer.Length);
        }

        public int Stride { get{return iplImageStruct.widthStep;} }

    
        public IplImage IplImageStruc()
        {
            return iplImageStruct;
        }

        public IntPtr IplImage()
        {
            return iplImagePointer;
        }

        public void Dispose()
        {
            NativeMethods.cvReleaseImage(ref iplImagePointer);
            GC.SuppressFinalize(this);
        }

        ~IntelImage()
        {
            Dispose();
        }
    }

    class HaarClassifier : IDisposable
    {
        IntPtr dllHandle, haarCascade, memoryStorage;
        
        void LoadObjDetectDll()
        {
            dllHandle = NativeMethods.LoadLibrary(DllPaths.OPENCV_OBJDETECT_DLL_PATH);
            if (dllHandle == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        public HaarClassifier(string cascadeFilePath)
        {
            LoadObjDetectDll(); // this is a workaround, leave it be
            haarCascade = NativeMethods.cvLoad(cascadeFilePath, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            memoryStorage = NativeMethods.cvCreateMemStorage();
        }

        [Flags]
        public enum DetectionFlags : int 
        {
            None = 0,
            DoCannyPruning = 1,
            ScaleImage = 2,
            FindBiggestObject = 4, 
            DoRoughSearch = 8
        }



        public LinkedList<CvRect> DetectObjects(IntPtr p_sourceImage/*IntelImage  sourceImage*/, double scaleFactor = 1.1, int minNeighbours = 3,
            DetectionFlags flags = DetectionFlags.DoCannyPruning, CvSize minSize = default(CvSize), CvSize maxSize = default(CvSize))
        {
            NativeMethods.cvClearMemStorage(memoryStorage);
            LinkedList<CvRect> result = new LinkedList<CvRect>();
            IntPtr faceSequence = NativeMethods.cvHaarDetectObjects(p_sourceImage, haarCascade, memoryStorage, 
                scaleFactor, minNeighbours, (int)flags, minSize, maxSize);
            for (;;)
            {
                IntPtr faceRectPointer = NativeMethods.cvGetSeqElem(faceSequence, 0);
                if (faceRectPointer == IntPtr.Zero) break;
                NativeMethods.cvSeqPopFront(faceSequence, IntPtr.Zero); // TODO: merge with cvGetSeqElem
                result.AddFirst((CvRect)Marshal.PtrToStructure(faceRectPointer, typeof(CvRect)));
            }
            return result;
        }

        public void Dispose()
        {
            NativeMethods.cvReleaseMemStorage(ref memoryStorage);
            NativeMethods.cvReleaseHaarClassifierCascade(ref haarCascade);
            NativeMethods.FreeLibrary(dllHandle);
            GC.SuppressFinalize(this);
        }

        ~HaarClassifier()
        {
            Dispose();
        }
    }

    static public class CDetectFace
    {

        public static Bitmap ToBitmap(IplImage image, bool dispose)
        {
            if (image.dataOrder != 0)
            {
                throw new Exception("Only interleaved Images are supported for conversion to Bitmap");
            }


            if (image.nChannels != 3 || image.depth != NativeMethods.IPL_DEPTH_8U)
            {
                throw new Exception("Only 3 Channel and IPL_DEPTH_8U images are supported for conversion to Bitmap");
            }


            CvSize size;
            int Width = size.width = image.width;
            int Height = size.height = image.height;

            //create the bitmap and get the pointer to the data
            Bitmap gdiBmp;
            System.Drawing.Imaging.PixelFormat fmt;
            fmt = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            gdiBmp = new Bitmap(Width, Height, fmt);

            System.Drawing.Imaging.BitmapData data = gdiBmp.LockBits(new Rectangle(0, 0, Width, Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly, fmt);
            int dataPtr = data.Scan0.ToInt32();

            int start, elementCount, byteWidth, rows, widthStep;
            RoiParam(image, out start, out rows, out elementCount, out byteWidth, out widthStep);
            for (int row = 0; row < data.Height; row++, start += widthStep, dataPtr += data.Stride)
                NativeMethods.memcpy((IntPtr)dataPtr, (IntPtr)start, data.Stride);

            gdiBmp.UnlockBits(data);

            if (dispose) NativeMethods.CvReleaseImage_(ref image);
            return gdiBmp;
        }

        private static void RoiParam(IplImage img, out int start, out int rows, out int elementCount, out int byteWidth, out int widthStep)
        {
            start = img.imageData.ToInt32();
            widthStep = img.widthStep;

            if (img.roi != IntPtr.Zero)
            {
                CvRect rec = NativeMethods.CvGetImageROI(ref img);
                elementCount = rec.width * img.nChannels;
                byteWidth = (img.depth >> 3) * elementCount;

                start += rec.y * widthStep + (img.depth >> 3) * rec.x;
                rows = rec.height;
            }
            else
            {
                byteWidth = widthStep;
                elementCount = img.width * img.nChannels;
                rows = img.height;
            }
        }

        static public void BitmapCopyPixels(Bitmap bm, byte[] b)
        {
            //direct bit manipulation
            Rectangle rect = new Rectangle(0, 0, bm.Width, bm.Height);

            //lock the bits
            System.Drawing.Imaging.BitmapData bmpData =
                     bm.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly,
                     bm.PixelFormat);
            IntPtr ptr = bmpData.Scan0;

            int bytes = b.Length;

            System.Runtime.InteropServices.Marshal.Copy(b, 0, ptr, bytes);
            // Unlock the bits.
            bm.UnlockBits(bmpData);
            bmpData = null;
        }

        static private void CopyPixelsFromBitmap(System.Drawing.Bitmap bm, byte[] b)
        {
            //direct bit manipulation
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height);

            //lock the bits
            System.Drawing.Imaging.BitmapData bmpData =
                     bm.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                     bm.PixelFormat);
            IntPtr ptr = bmpData.Scan0;

            int bytes = b.Length;
            Marshal.Copy(ptr, b, 0, bytes);

            // Unlock the bits.
            bm.UnlockBits(bmpData);
            bmpData = null;
        }

        static public BitmapSource CreateBitmapSourceFromFile(string filePath, PixelFormat targetPixelFormat)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var decoder = BitmapDecoder.Create(fileStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                return new FormatConvertedBitmap(decoder.Frames[0], targetPixelFormat, null, 0);
            }
        }


        static public IntelImage FlipYIntelImage(IntelImage img)
        {
            Bitmap tempbm = new Bitmap(img.IplImageStruc().width,img.IplImageStruc().height , img.IplImageStruc().widthStep, 
                      System.Drawing.Imaging.PixelFormat.Format24bppRgb, img.IplImageStruc().imageData);
            tempbm.RotateFlip(RotateFlipType.RotateNoneFlipY);
            IntelImage _img = CreateIntelImageFromBitmap(tempbm);
            tempbm.Dispose();
            tempbm = null;
            return _img;
        }

        static public IntelImage CreateIntelImageFromBitmap(System.Drawing.Bitmap bm)
        {
            var intelImage = new IntelImage(bm.Width , bm.Height, bm.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            byte[] pixelBuffer = new byte[bm.Height * intelImage.Stride];
            CopyPixelsFromBitmap(bm, pixelBuffer);
           
            intelImage.CopyPixels(pixelBuffer);
            return intelImage;
        }

        static public IntelImage CreateIntelImageFromBitmapSource(BitmapSource bitmapSource)
        {
            var intelImage = new IntelImage(bitmapSource.PixelWidth, bitmapSource.PixelHeight, bitmapSource.Format != PixelFormats.Gray8);
           // int stride = bitmapSource.PixelWidth * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            byte[] pixelBuffer = new byte[bitmapSource.PixelHeight * intelImage.Stride];
            bitmapSource.CopyPixels(pixelBuffer, intelImage.Stride, 0);
            
            intelImage.CopyPixels(pixelBuffer);
            return intelImage;
        }

        static public IntelImage CreateIntelImageFromFile(string filePath, bool convertToGrayscale = false)
        {
            var bitmapSource = CreateBitmapSourceFromFile(filePath, convertToGrayscale ? PixelFormats.Gray8 : PixelFormats.Bgr24);
            return CreateIntelImageFromBitmapSource(bitmapSource);
        }



        static public IntPtr HistEqualize(IntelImage isrc)
        {
            try
            {
              

                //image for processing
                IntelImage iimg = new IntelImage(isrc.IplImageStruc().width, isrc.IplImageStruc().height);

                //convert src image to ycrcb format
                NativeMethods.CvCvtColor(isrc.IplImage(), iimg.IplImage(), NativeMethods.CV_BGR2YCrCb);

                //converted src image structure
                IplImage iimg_iplimage = iimg.IplImageStruc();

                //get all the channels from the 
                IntPtr y = NativeMethods.cvCreateImage(new CvSize(iimg_iplimage.width, iimg_iplimage.height), 8, 1);
                IplImage yimg = (IplImage)Marshal.PtrToStructure(y, typeof(IplImage));

                IntPtr cr = NativeMethods.cvCreateImage(new CvSize(iimg_iplimage.width, iimg_iplimage.height), 8, 1);
                IplImage crimg = (IplImage)Marshal.PtrToStructure(cr, typeof(IplImage));

                IntPtr cb = NativeMethods.cvCreateImage(new CvSize(iimg_iplimage.width, iimg_iplimage.height), 8, 1);
                IplImage cbimg = (IplImage)Marshal.PtrToStructure(cb, typeof(IplImage));

                NativeMethods.CvSplit(ref iimg_iplimage, ref yimg, ref crimg, ref cbimg);


                //****************************

                //*******Equalized for y plane only

                //the ydest intel image
               // IntelImage iydest = new IntelImage(yimg);
              //  IntPtr ydest = NativeMethods.cvCreateImage(new CvSize(iimg_iplimage.width, iimg_iplimage.height), 8, 1);
              //  NativeMethods.CvEqualizeHist(y, ydest);


                NativeMethods.CvSaveImage(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\~eq.y1.bmp", y);
             //   NativeMethods.CvSaveImage(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\~eq.cr1.bmp", cr);
             //   NativeMethods.CvSaveImage(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\~eq.cb1.bmp", cb);

               // NativeMethods.CvSaveImage(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\~eq.ydest.bmp", ydest);


             //   IplImage ydest_iplimage = iydest.IplImageStruc();

                //merge back into processing image
             //   NativeMethods.CvMerge(ref ydest_iplimage, ref crimg, ref cbimg, ref iimg_iplimage);
              //  IntPtr presult = NativeMethods.cvCreateImage(new CvSize(iimg_iplimage.width, iimg_iplimage.height), 8, 3);
              // IntPtr presult = NativeMethods.cvCreateImage(new CvSize(iimg_iplimage.width, iimg_iplimage.height), 8, 1);
             //   NativeMethods.CvCvtColor(iimg.IplImage(), presult, NativeMethods.CV_YCrCb2BGR);
            //    IplImage result_img = (IplImage)Marshal.PtrToStructure(presult, typeof(IplImage));
             //   NativeMethods.CvCopy(ref ydest_iplimage, ref result_img);

              //  NativeMethods.CvSaveImage(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\~eq.bmp", presult);

               // NativeMethods.cvReleaseImage(ref y);
                NativeMethods.cvReleaseImage(ref  cb);
                NativeMethods.cvReleaseImage(ref  cr);
               //NativeMethods.cvReleaseImage(ref presult);

                iimg.Dispose();
                iimg = null;
             //   iydest.Dispose();
             //   iydest = null;

               

               // return presult;
                return y;
               // return ydest;
            }catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }

            return IntPtr.Zero;
        }


    }
}