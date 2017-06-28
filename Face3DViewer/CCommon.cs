using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;
using System.Globalization;

namespace ThreeDFaces
{

    public struct FacePointMapping
    {
        public string side ;
        public int index;
        public int mappedindex;

    }

    public struct FaceModelTransformType
    {
        public string basemodelid;
        public int index;
        public double rotateX;
        public double rotateY;
        public double rotateZ;
        public double stretchX;
    }

    public struct FeaturePointType
    {
        public string desp;
        public Point pt;
    }

    public static class MyExtension
    {

        private static Action EmptyDelegate = delegate() { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        public static System.Drawing.Bitmap cropAtRect(this System.Drawing.Bitmap b, System.Drawing.Rectangle r)
        {
            System.Drawing.Bitmap nb = new System.Drawing.Bitmap(r.Width, r.Height, b.PixelFormat);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(nb);
            g.DrawImage(b, -r.X, -r.Y);
            return nb;
        }



    }

    public static class CCommon
    {

        public static List<System.Drawing.Rectangle> cacheRects = new List<System.Drawing.Rectangle>();
        public static System.Drawing.Bitmap cacheBm = null;


        public static FaceModelTransformType readFaceModelTransformFromFile(string filename)
        {
            using (var tr = File.OpenText(filename))
            {
                string s = tr.ReadToEnd();
                tr.Close();
                string basemodelid="";
                int index = 0;
                double rotateX = 0, rotateY = 0, rotateZ = 0, stretchX = 0;
                var parts = s.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                  for(int i=0;i<parts.Length ;i++)
                  {
                      var item = parts[i].Split('=');
                      switch (item[0])
                      {
                          case "basemodelid": basemodelid = item[1]; break;
                          case "index":  index =int.Parse(item[1]); break;
                          case "rotateX": rotateX = double.Parse(item[1], CultureInfo.InvariantCulture); break;
                          case "rotateY": rotateY = double.Parse(item[1], CultureInfo.InvariantCulture); break;
                          case "rotateZ": rotateZ = double.Parse(item[1], CultureInfo.InvariantCulture); break;
                          case "stretchX": stretchX = double.Parse(item[1], CultureInfo.InvariantCulture); break;
                      }
                  }
                FaceModelTransformType ft = new FaceModelTransformType()
                {
                    index=index,
                     basemodelid =basemodelid,
                     rotateX =rotateX ,
                     rotateY =rotateY ,
                     rotateZ =rotateZ,
                     stretchX =stretchX
                   
                };
                return ft;
            }
        }


        public static System.Windows.Media.Imaging.BitmapImage Bitmap2BitmapImage(System.Drawing.Bitmap bitmap)
        {
                System.Drawing.Image img = new System.Drawing.Bitmap(bitmap);
                ((System.Drawing.Bitmap)img).SetResolution(96, 96);

                MemoryStream ms = new MemoryStream();
                
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    img.Dispose();

                    ms.Seek(0, SeekOrigin.Begin);

                    BitmapImage bi = new BitmapImage();

                    bi.BeginInit();
                   
                    bi.StreamSource = ms;

                    bi.EndInit();


            
                    return bi;
           


      
        }

        public static System.Drawing.Bitmap BitmapImage2Bitmap(BitmapSource bitmapImage,BitmapEncoder enc)
        {

            using (MemoryStream outStream = new MemoryStream())
            {
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);
                bitmap.SetResolution(96, 96);
                System.Drawing.Bitmap bm = new System.Drawing.Bitmap(bitmap);
                bitmap.Dispose();
                return bm;
            }
        }

        public static System.Drawing.Bitmap BitmapImage2Bitmap(BitmapSource bitmapImage)
        {

            using (MemoryStream outStream = new MemoryStream())
            {
                //BitmapEncoder enc = new BmpBitmapEncoder();
                BitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);
                bitmap.SetResolution(96, 96);

               System.Drawing.Bitmap bm= new System.Drawing.Bitmap(bitmap);
               bitmap.Dispose();
                return bm;
            }
        }



        public static bool LoadImageSource(Image img, string filename,bool bResize,int maxsize=700)
        {
            try
            {
                using (FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {

                    using (var bm = System.Drawing.Bitmap.FromStream(file))
                    {

                       // ((System.Drawing.Bitmap)bm).SetResolution(96, 96);
                        if (((bm.Width > maxsize) || (bm.Height > maxsize)) && bResize)
                        {
                            //resize the image
                            double ratio = (double)bm.Width / bm.Height;
                            int newwidth = ratio > 1 ? maxsize : (int)(maxsize * ratio);
                            int newheight = (int)(newwidth / ratio);
                            System.Drawing.Bitmap bm2 = new System.Drawing.Bitmap(newwidth, newheight, System.Drawing.Imaging.PixelFormat.Format32bppArgb /*bm.PixelFormat*/);
                          
                            System.Drawing.Graphics gbm2 = System.Drawing.Graphics.FromImage(bm2);
                            System.Drawing.Rectangle srcRect = new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height);
                            System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(0, 0, bm2.Width, bm2.Height);
                            gbm2.DrawImage(bm, destRect, srcRect, System.Drawing.GraphicsUnit.Pixel);
                            gbm2.Dispose();
                            img.Source = CCommon.Bitmap2BitmapImage((System.Drawing.Bitmap)bm2);
                            bm2.Dispose();
                            bm2 = null;
                        }
                        else
                        {
            
                            img.Source = CCommon.Bitmap2BitmapImage((System.Drawing.Bitmap)bm);
                            bm.Dispose();
                           
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }

            return true;
        }


        //Load from cache CCommon cacheBm and cacheRects
        public static void FindEyesFromCache(int faceindex,out System.Drawing.Rectangle[] eyesrect)
        {
            String eyeFileName = AppDomain.CurrentDomain.BaseDirectory + "haarcascade_eye.xml";

            using (HaarClassifier haareye = new HaarClassifier(eyeFileName))
            {
                var face = cacheRects.ElementAt(faceindex);


                var facerect = new System.Drawing.Rectangle((int)(face.X),
                                                         (int)(face.Y),
                                                         (int)(face.Width),
                                                         (int)(face.Height));

                int x = facerect.X, y = facerect.Y, h0 = facerect.Height, w0 = facerect.Width;

                //to handle oversize face area
                double rescale = 1.0;
                if (h0 > 300) rescale = 300.0 / h0;

                System.Drawing.Rectangle temprect = new System.Drawing.Rectangle(x, y, w0, 10 * h0 / 16);

                System.Drawing.Bitmap bm_eyes = cacheBm.cropAtRect(temprect);

                bm_eyes.Save(AppDomain.CurrentDomain.BaseDirectory + "temp\\~eye.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                IntelImage image_eyes = CDetectFace.CreateIntelImageFromBitmap(bm_eyes);

                //resize eyes area for better detection
                IntelImage image_eyes2X = new IntelImage((int)(image_eyes.IplImageStruc().width * 2 * rescale),
                                                       (int)(image_eyes.IplImageStruc().height * 2 * rescale));
                NativeMethods.CvResize(image_eyes.IplImage(), image_eyes2X.IplImage(), NativeMethods.CV_INTER_CUBIC);

                IntPtr p_eq_img_eyes = CDetectFace.HistEqualize(image_eyes2X);

                var eyes = haareye.DetectObjects(p_eq_img_eyes);

                ////clean up
                NativeMethods.cvReleaseImage(ref  p_eq_img_eyes);
                image_eyes.Dispose();
                image_eyes = null;
                bm_eyes.Dispose();

                image_eyes2X.Dispose();
                image_eyes2X = null;

                if (eyes.Count > 0)
                {
                    eyesrect = new System.Drawing.Rectangle[eyes.Count];

                    for (int i = 0; i < eyesrect.Length; i++)
                    {
                        var eye = eyes.ElementAt(i);

                        //note that we had scale the eyes area by 2, so we scale back
                        eyesrect[i] = new System.Drawing.Rectangle((int)(eye.x / (2 * rescale)),
                                                                   (int)(eye.y / (2 * rescale)),
                                                                   (int)(eye.width / (2 * rescale)),
                                                                   (int)(eye.height / (2 * rescale)));


                    }

                    int mineyesize = (h0 / 12);
                    int maxeyesize = (h0 / 3);

                    //sorting
                    var tempeyeslist = eyesrect.ToList();

                    //dist to center of face
                    //  <-1/2 w -->
                    //  |          |          |
                    //  |<-x ->o<d>|          |
                    //  |          |          |
                    //  |          |          |
                    //  |          |          |
                    // o=center of eye
                    // x= x dist to center of eye 
                    // d= difference of 1/2 w and x 
                    //  = distance of eye center to center of face
                    // the further this distance, the more likely it is an eye
                    int half_facewidth = facerect.Width / 2;
                    tempeyeslist = tempeyeslist.OrderByDescending(eye => Math.Abs(eye.X + eye.Width / 2 - (half_facewidth))).ToList();

                    //size: should be within min and max eye size
                    tempeyeslist = tempeyeslist.OrderByDescending(eye => (eye.Width > mineyesize)).ToList();
                    tempeyeslist = tempeyeslist.OrderByDescending(eye => (eye.Width < maxeyesize)).ToList();


                    eyesrect = tempeyeslist.ToArray();

                }
                else
                    eyesrect = null;
            }

            
        }


        public static bool IsValidPoint(System.Drawing.PointF pt)
        {
            if (pt.X == 0 && pt.Y == 0) return false;
            return true;
        }

        public static bool ArePointsClockwise(System.Drawing.PointF a, System.Drawing.PointF b, System.Drawing.PointF c)
        {


            double edge_ab = (b.X - a.X) * (b.Y + a.Y);
            double edge_bc = (c.X - b.X) * (c.Y + b.Y);
            double edge_ca = (a.X - c.X) * (a.Y + c.Y);

            double sum = edge_ab + edge_bc + edge_ca;

            return (sum >= 0);

        }


        //return "0" for single face and "c"(cache) for multiple face
        //for multiple face, the face rectangles are stored in CCommon.cacheRects
        //CComon.cacheBm store the 24bit Bitmap for source image
        public static string FindFaceAndEyes(BitmapSource srcimage, out System.Drawing.Rectangle facerect, out System.Drawing.Rectangle[] eyesrect)
        {

            String faceFileName = AppDomain.CurrentDomain.BaseDirectory + "haarcascade_frontalface_alt2.xml";
            String eyeFileName = AppDomain.CurrentDomain.BaseDirectory + "haarcascade_eye.xml";
           
            //working with gdi to get 24bit rgb image
            System.Drawing.Bitmap bmtest = CCommon.BitmapImage2Bitmap(srcimage);
            System.Drawing.Bitmap bmsrc24 = new System.Drawing.Bitmap(bmtest.Width, bmtest.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Graphics gbmsrc24 = System.Drawing.Graphics.FromImage(bmsrc24);
            gbmsrc24.DrawImageUnscaled(bmtest, 0, 0);
            gbmsrc24.Dispose();
            bmtest.Dispose();
            bmtest = null;

            if (cacheBm != null) cacheBm.Dispose();
            cacheBm = (System.Drawing.Bitmap)  bmsrc24.Clone();

            //we do scaling if the source is too large
            double scale = 1.0;
            if (bmsrc24.Height > 500)
                scale = (double)500 / bmsrc24.Height;

            System.Drawing.Bitmap bm = new System.Drawing.Bitmap((int)(bmsrc24.Width * scale),
                                                                 (int)(bmsrc24.Height * scale), 
                                                                 System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Graphics gbm = System.Drawing.Graphics.FromImage(bm);
                   
            //scale down the source image for face dection
            gbm.DrawImage(bmsrc24,
                          new System.Drawing.Rectangle(0,0,bm.Width,bm.Height),
                          new System.Drawing.Rectangle(0, 0, bmsrc24.Width, bmsrc24.Height), 
                          System.Drawing.GraphicsUnit.Pixel);
            gbm.Dispose();

           // bm.Save(AppDomain.CurrentDomain.BaseDirectory +"temp\\~bm.jpg",System.Drawing.Imaging.ImageFormat.Jpeg);
            //////////////////////////////////////////


            IntelImage _img = CDetectFace.CreateIntelImageFromBitmap(bm);
            //IntPtr p_face = CDetectFace.HistEqualize(_img);
            bm.Dispose();
            bm = null;
            string  strindex = "";

            using (HaarClassifier haarface = new HaarClassifier(faceFileName))
            using (HaarClassifier haareye = new HaarClassifier(eyeFileName))
            {

                var faces = haarface.DetectObjects(_img.IplImage());
               // var faces = haarface.DetectObjects(p_face );
                if (faces.Count > 0)
                {
                    List<System.Drawing.Rectangle> facerects = new List<System.Drawing.Rectangle>();
                    for (int i = 0; i < faces.Count; i++)
                    {
                        var face = faces.ElementAt(i);
                        System.Drawing.Rectangle rt = new System.Drawing.Rectangle((int)(face.x / scale),
                                                                                   (int)(face.y / scale),
                                                                                   (int)(face.width / scale),
                                                                                   (int)(face.height / scale));
                        facerects.Add(rt);
                    }
                    cacheRects = facerects;

                    if (faces.Count > 1)
                    {
                        //clean up and return
                        eyesrect = null;
                        facerect = facerect = System.Drawing.Rectangle.Empty;

                        _img.Dispose();
                        bmsrc24.Dispose();
                        bmsrc24 = null;
                        
                        return "c"; //cached

                    }

                }
                else
                    cacheRects.Clear();

                //only handle 1 face
                if (faces.Count == 1)
                {

                    var face = faces.ElementAt(0);

                    facerect = new System.Drawing.Rectangle((int)(face.x / scale),
                                                             (int)(face.y / scale),
                                                             (int)(face.width / scale),
                                                             (int)(face.height / scale));

                   int x = facerect.X, y = facerect.Y, h0 = facerect.Height , w0 = facerect.Width ;

                   //to handle oversize face area
                   double rescale = 1.0;
                   if (h0 > 300) rescale = 300.0 / h0;

                    System.Drawing.Rectangle temprect = new System.Drawing.Rectangle(x, y, w0, 10 * h0 / 16);

                    System.Drawing.Bitmap bm_eyes = bmsrc24.cropAtRect(temprect);

                    bm_eyes.Save(AppDomain.CurrentDomain.BaseDirectory + "temp\\~eye.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                    IntelImage image_eyes = CDetectFace.CreateIntelImageFromBitmap(bm_eyes);

                    //resize eyes area for better detection
                    IntelImage image_eyes2X=new IntelImage((int)(image_eyes.IplImageStruc().width *2*rescale) ,
                                                           (int)(image_eyes.IplImageStruc().height  *2*rescale) );
                    NativeMethods.CvResize(image_eyes.IplImage(), image_eyes2X.IplImage(), NativeMethods.CV_INTER_CUBIC);
           
                    IntPtr p_eq_img_eyes = CDetectFace.HistEqualize(image_eyes2X);

                    var eyes = haareye.DetectObjects(p_eq_img_eyes);

                    ////clean up
                    NativeMethods.cvReleaseImage(ref  p_eq_img_eyes);
                    image_eyes.Dispose();
                    image_eyes = null;
                    bm_eyes.Dispose();

                    image_eyes2X.Dispose();
                    image_eyes2X = null;

                    if (eyes.Count > 0)
                    {
                        eyesrect = new System.Drawing.Rectangle[eyes.Count];

                        for (int i = 0; i < eyesrect.Length; i++)
                        {
                            var eye = eyes.ElementAt(i);

                            //note that we had scale the eyes area by 2, so we scale back
                            eyesrect[i] = new System.Drawing.Rectangle((int)(eye.x/(2*rescale)) , 
                                                                       (int)(eye.y/(2*rescale)) , 
                                                                       (int)(eye.width/(2*rescale)) , 
                                                                       (int)(eye.height/(2*rescale)) );


                        }


                        int mineyesize = (h0 / 12);
                        int maxeyesize = (h0 / 3);
                       //sorting
                        var tempeyeslist = eyesrect.ToList();


                        //dist to center of face
                        //  <-1/2 w -->
                        //  |          |          |
                        //  |<-x ->o<d>|          |
                        //  |          |          |
                        //  |          |          |
                        //  |          |          |
                        // o=center of eye
                        // x= x dist to center of eye 
                        // d= difference of 1/2 w and x 
                        //  = distance of eye center to center of face
                        // the further this distance, the more likely it is an eye
                        int half_facewidth = facerect.Width/2;
                        tempeyeslist = tempeyeslist.OrderByDescending(eye => Math.Abs(eye.X + eye.Width / 2 - (half_facewidth))).ToList(); 

                        //size: should be within min and max eye size
                        tempeyeslist= tempeyeslist.OrderByDescending(eye => (eye.Width>mineyesize  )).ToList();
                        tempeyeslist = tempeyeslist.OrderByDescending(eye => (eye.Width < maxeyesize)).ToList();
                       

                        eyesrect = tempeyeslist.ToArray();
                    
                    }
                    else
                        eyesrect = null;

                }
                else
                {
                    facerect = System.Drawing.Rectangle.Empty;
                    eyesrect = null;
                }

            }

            //NativeMethods.cvReleaseImage(ref  p_face );
            
            _img.Dispose();

            bmsrc24.Dispose();
            bmsrc24 = null;

            return strindex;

        }


     }

  }

