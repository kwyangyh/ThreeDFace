/////////////////////////////////
// Yang Kok Wah, 13 May 2017
////////////////////////////////

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

namespace ThreeDFaces
{
    /// <summary>
    /// Interaction logic for Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        private bool _bMouseDown = false;
        int _mag_size = 50;
        private WriteableBitmap _wbitmap = null;


        Window4 winSelectFace = null;
        Window3 winMagnifier = null;

        //rectangle inside facial point marked for fine positioning
        System.Windows.Shapes.Rectangle _rt;
      
        //num face points
        int _numfacepoints = 5;


        //for implementation of facial points markers
        private double _mouseX0 = 0;
        private double _mouseY0 = 0;
        private int _currentEllipseIndex = 0;

        //for detecting if window has been closed
        //note that a closed window is still in memory
        //thus we need a way to know if the window has been closed
        //simply checking for window=null does not work.
        private bool _isloaded = false;

        //current face file used
        private string _currentfile="";

        //the error of fitting the face file to the mesh
        //the format is:
        //<eye-nose>:<nose-mouth>:<mouth-chin>:<total_err>
        private string _fittingErr = "";

        public Window2()
        {
            InitializeComponent();
        }

        //Load face points from cache
        public bool LoadFacePointsFromCache
        {
            get;
            set;
        }


        public Window3 Magnifier
        {
            get;
            set;
        }

        public string FaceIndexString
        {
            get;
            private set;
        }

        public string CurrentFile
        {
            get { return _currentfile; }
            set
            {
                _currentfile = (string)value;
                if (_rt != null)
                {
                    Canvas1.Children.Remove(_rt);
                    _rt = null;
                }
                if(winSelectFace !=null)
                {
                    winSelectFace.Close();
                    winSelectFace = null;
                }
            }
        }

        public string FittingError
        {
            get { return _fittingErr; }
            set { _fittingErr = value;
                LabelErr.Content = "Fitting Error=" + _fittingErr ;
            }
        }

        public bool IsWinLoaded
        {
            get { return _isloaded; }

        }


        //Face Point processing//////////////////////////////////////
        //(x,y) -> Point
        private Point ExtractPoint(string xycords)
        {
            string s = xycords.Replace(")", "");
            s = s.Replace("(", "");
            var v = s.Split(',');
            if (v.Length == 2)
            {
                return new Point(double.Parse(v[0]), double.Parse(v[1]));
            }

            return new Point();

        }


        //Loading of facial points markers
        private void Button1_Click(object sender, RoutedEventArgs e)
        {

            for (int i = Canvas1.Children.Count - 1; i >= 0; i--)
            {
                if (Canvas1.Children[i].GetType() == typeof(System.Windows.Shapes.Ellipse))
                    Canvas1.Children.Remove(Canvas1.Children[i]);

            }

            var fps = ((MainWindow)this.Owner).ImageFacePoints;

            double imagewidth = ((BitmapImage)(Image1.Source)).PixelWidth;
            double imageheight = ((BitmapImage)(Image1.Source)).PixelHeight;

            this.UpdateLayout();
            double wpfimagewidth = Image1.ActualWidth;
            double wpfimageheight = Image1.ActualHeight;
            double ratio = imageheight / wpfimageheight;

            for (int i = 0; i < _numfacepoints; i++)
            {
                System.Windows.Shapes.Ellipse ep = new System.Windows.Shapes.Ellipse();
                ep.Name = "EP"+i;
                ep.Width =30;
                ep.Height=30;
                ep.StrokeThickness = 2; 
                ep.Stroke = System.Windows.Media.Brushes.Red;

                ep.Fill = new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));
                ep.HorizontalAlignment = HorizontalAlignment.Left  ;
                ep.VerticalAlignment = VerticalAlignment.Top  ;
                ep.Uid = ep.Name;
                Canvas1.Children.Add(ep);

                Canvas.SetTop(ep, Canvas.GetTop(Grid1) + i * ep.Height + Image1.ActualHeight / 2);
                Canvas.SetLeft(ep, Canvas.GetLeft(Grid1) + 50);//(Image1.ActualWidth + ep.Width) / 2);
        
                ep.Focusable = true;
                ep.MouseDown += ep_MouseDown;
                ep.MouseMove += ep_MouseMove;
                ep.KeyDown += ep_KeyDown;
  
            }



                try
                {
                    System.Drawing.Rectangle facerect=System.Drawing.Rectangle.Empty;
                    System.Drawing.Rectangle[] eyesrect=null;

                    string strfaceindex = "";
                    bool bFromCacheRects = false;
                    if (sender != null && sender.GetType() == winSelectFace.GetType()) //Load face from cache
                    {
                        bFromCacheRects = true;

                    }
                    else
                    {
                        strfaceindex=CCommon.FindFaceAndEyes(Image1.Source.Clone() as BitmapImage, out facerect, out eyesrect);
                    }

                    //for implementation of multiple faces image
                    //c is for cache
                    if (strfaceindex == "c" || bFromCacheRects)
                    {

                        if (strfaceindex == "c")
                        {
                            if (winSelectFace != null)
                            {

                                winSelectFace = null;
                            }

                           // if (winSelectFace == null)
                            winSelectFace = new Window4();

                            winSelectFace.Title = "Double click to select a face";
                            winSelectFace.Image1.Source = Image1.Source.Clone();
                            winSelectFace.FaceRects = CCommon.cacheRects.ToArray();
                            winSelectFace.Owner = this;
                            winSelectFace.Show();
                        }

                        try
                        {

                            //wait for face to be selected
                            while (!winSelectFace.IsFaceSelected)
                            {
                                System.Windows.Forms.Application.DoEvents();

                            }
                           
                        }
                        catch //fires if winSelectFace is closed without making a selection
                        {
                            return;
                        }
                        
                        //get the index of the face from winSelectFace
                        strfaceindex = "" + winSelectFace.FaceSelectedIndex;
                        facerect = CCommon.cacheRects.ElementAt(winSelectFace.FaceSelectedIndex);
                        CCommon.FindEyesFromCache(winSelectFace.FaceSelectedIndex, out eyesrect);

                    }
                    else //for single face image, we close winSelectFace
                    {
                        if(winSelectFace !=null)
                        {
                            winSelectFace.Close();
                            winSelectFace = null;
                        }
                    }

                    //store the index of face selected
                    //note that for single face image, this is ""
                    FaceIndexString = strfaceindex;
                  /////////////////////////
                  //Crop Image1.Source based on facerect
                    if (facerect != System.Drawing.Rectangle.Empty )
                    {
                        //Rescaling

                        if (bFromCacheRects)
                        {
                            //load default unscaled 
                            Image1.Source = CCommon.Bitmap2BitmapImage(CCommon.cacheBm);
                        }
                       
                        //scale and reload if image is too large
                        if(facerect.Height  >500)
                        {
                            double rescale = 500.0 / facerect.Height ;
                            int newsize = ((BitmapImage)Image1.Source).PixelWidth > ((BitmapImage)Image1.Source).PixelHeight ?
                                (int)(rescale * ((BitmapImage)Image1.Source).PixelWidth) :
                               (int)(rescale * ((BitmapImage)Image1.Source).PixelHeight);

                            CCommon.LoadImageSource(Image1, CurrentFile, true, newsize);
                            //rescale face
                            facerect  = new System.Drawing.Rectangle((int)(facerect.X * rescale),
                                                                   (int)(facerect.Y * rescale),
                                                                   (int)(facerect.Width * rescale),
                                                                   (int)(facerect.Height * rescale));
                            //rescale all the eyepoints
                            for (int i = 0; i < eyesrect.Length; i++)
                            {
                                eyesrect[i] = new System.Drawing.Rectangle((int)(eyesrect[i].X * rescale),
                                                                          (int)(eyesrect[i].Y * rescale),
                                                                          (int)(eyesrect[i].Width * rescale),
                                                                          (int)(eyesrect[i].Height * rescale));
                            }        
                        }



                        int newtop = (int)(facerect.Top)  - (int)(0.3 * facerect.Height );
                        if (newtop < 0) newtop = 0;
                        int deltatop = facerect.Top - newtop;

                        int newleft=(int)(facerect.Left)  -(int)(0.3*facerect.Width );
                        if (newleft < 0) newleft = 0;
                        int deltaleft = facerect.Left - newleft;

                        int maxheight = ((BitmapImage)Image1.Source).PixelHeight - newtop-1;
                        facerect.Height =(int) (facerect.Height * 1.6 );
                        if (facerect.Height > maxheight) facerect.Height = maxheight;

                        int maxwidth = ((BitmapImage)Image1.Source).PixelWidth  - newleft - 1;
                        facerect.Width  = (int)(facerect.Width  * 1.6 );
                        if (facerect.Width  > maxwidth) facerect.Width  = maxwidth;

                        facerect = new System.Drawing.Rectangle(newleft , newtop, facerect.Width, facerect.Height);

                        CroppedBitmap crop = new CroppedBitmap(
                                                          Image1.Source.Clone() as BitmapImage,
                                                          new Int32Rect(facerect.Left, facerect.Top, facerect.Width, facerect.Height)
                                                          );
                        System.Drawing.Bitmap bm = CCommon.BitmapImage2Bitmap(crop);
                        bm.Save(AppDomain.CurrentDomain.BaseDirectory + "temp\\~cropface.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                        bm.Dispose();
                        bm = null;
                        CCommon.LoadImageSource(Image1, AppDomain.CurrentDomain.BaseDirectory + "temp\\~cropface.jpg", false);

                        imagewidth = ((BitmapImage)(Image1.Source)).PixelWidth;
                        imageheight =((BitmapImage)(Image1.Source)).PixelHeight;
                        this.UpdateLayout();
                        wpfimagewidth = Image1.ActualWidth;
                        wpfimageheight =Image1.ActualHeight;
                        //find the new ratio
                        ratio = imageheight / wpfimageheight;
                        
                        facerect = new System.Drawing.Rectangle(deltaleft , deltatop  , facerect.Width, facerect.Height);



                    }

                    //////////////////
                    //strfaceindex!="" implies face is from a multiple faces image
                    if(strfaceindex !="")
                    {
                       var fileparts = this.CurrentFile.Split('\\');
                       string facepointsfile=AppDomain.CurrentDomain.BaseDirectory +"temp\\" + fileparts[fileparts.Length -1] +strfaceindex+".info.txt";
                       //check if we have the cached face points in file
                       if (File.Exists(facepointsfile))
                       {

                           //read these face points and update the face points markers

                           using (var file = File.OpenText(facepointsfile))
                           {
                               List<FeaturePointType> facepoints = new List<FeaturePointType>();
                               string s = file.ReadToEnd();
                               var lines = s.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                               for (int i = 0; i < lines.Length; i++)
                               {
                                   var parts = lines[i].Split('=');
                                   FeaturePointType fp = new FeaturePointType();
                                   fp.desp = parts[0];
                                   fp.pt = ExtractPoint(parts[1]);
                                   facepoints.Add(fp);

                               }
                               ((MainWindow)this.Owner).ImageFacePoints = facepoints;
                           }
                           LoadFacePointsFromCache = true;
                           fps = ((MainWindow)this.Owner).ImageFacePoints;
                       }
                       else
                           LoadFacePointsFromCache = false;


                    }

                    ////////////////
                     if (facerect!=System.Drawing.Rectangle.Empty && LoadFacePointsFromCache)
                     {
                         List<UIElement> list = Canvas1.Children.Cast<UIElement>().ToList();
                         for (int i = 0; i < _numfacepoints; i++)
                         {
                             System.Windows.Shapes.Ellipse ep = (System.Windows.Shapes.Ellipse)list.Single(s => s.Uid == "EP" + i);
                             Canvas.SetTop(ep, fps[i].pt.Y / ratio + Canvas.GetTop(Grid1) - ep.Height / 2);
                             Canvas.SetLeft(ep, fps[i].pt.X / ratio + Canvas.GetLeft(Grid1) - ep.Width / 2);
                         }
                     }


                    if (facerect!=System.Drawing.Rectangle.Empty && eyesrect != null && !LoadFacePointsFromCache )
                    {
                        List<UIElement> list = Canvas1.Children.Cast<UIElement>().ToList();

                        //For elimination of overlapping eyes detected 
                        System.Drawing.Bitmap bmtemp = new System.Drawing.Bitmap((int)imagewidth, (int)imageheight /*,System.Drawing.Imaging.PixelFormat.Format24bppRgb*/);
                        System.Drawing.Graphics gbmptemp = System.Drawing.Graphics.FromImage(bmtemp);
                        gbmptemp.Clear(System.Drawing.Color.Black);

                        int nmaxeyes = (eyesrect.Length >_numfacepoints ) ? _numfacepoints  : eyesrect.Length;
                        int n = 0;
                        for (int i = 0; i < nmaxeyes; i++)
                        {
                            //valid eye pos relative to facerect top
                            //var normalized_eye_y = ((double)eyesrect[i].Y +((double)eyesrect[i].Height/2)) / facerect.Height;
                            //var normalized_eye_x_to_mid_face =
                            //     Math.Abs((eyesrect[i].X + (double)eyesrect[i].Width / 2) - ((double)facerect.Width / 2)) / ((double)facerect.Width / 2);
                            
                            //System.Diagnostics.Debug.Print("{0} {1}", normalized_eye_y,normalized_eye_x_to_mid_face);

                            System.Windows.Shapes.Ellipse ep = (System.Windows.Shapes.Ellipse)list.Single(s => s.Uid == "EP" + i);

                            double top=(eyesrect[i].Y + facerect.Y + (double)eyesrect[i].Height / 2) / ratio + Canvas.GetTop(Grid1) - ep.Height / 2;
                            double left = (eyesrect[i].X + facerect.X + (double)eyesrect[i].Width / 2) / ratio + Canvas.GetLeft(Grid1) - ep.Width / 2;

                            int argb = (bmtemp.GetPixel(facerect.X + eyesrect[i].X, facerect.Y + eyesrect[i].Y)).ToArgb();
                            if (argb == System.Drawing.Color.Black.ToArgb())
                            {
                             if (n >= 2) break; //we already got 2 eyes, dont need any more
                              Canvas.SetTop(ep,top );                       
                              Canvas.SetLeft(ep, left);
                              _currentEllipseIndex= (++n)%_numfacepoints;
                           }

                            gbmptemp.FillEllipse(System.Drawing.Brushes.White, facerect.X + eyesrect[i].X, facerect.Y + eyesrect[i].Y, eyesrect[i].Width, eyesrect[i].Height);

                            System.Diagnostics.Debug.Print("Get {0},{1}", Canvas.GetLeft(ep), Canvas.GetTop(ep));
                        }

                        bmtemp.Save(AppDomain.CurrentDomain.BaseDirectory + "temp\\~eploc.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                        gbmptemp.Dispose();
                        bmtemp.Dispose();
                        bmtemp = null;


                    }
                }
                catch (Exception ex)
                {
                     MessageBox.Show(ex.ToString());
                }

            



        }


        void ep_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.Print(e.Key.ToString());
            e.Handled = true;
            if (e.IsDown)
            {
                double deltax=0;
                double deltay=0;

                switch (e.Key)
                {
                    case Key.Down: deltay++; break;
                    case Key.Up: deltay--; break;
                    case Key.Left: deltax--; break;
                    case Key.Right: deltax++; break;
                    default  : return ;

                }


                double newX = Canvas.GetLeft((UIElement)sender)+deltax ;
                double newY = Canvas.GetTop((UIElement)sender)+deltay ; 
                Canvas.SetLeft((UIElement)sender, newX);
                Canvas.SetTop((UIElement)sender, newY);


                if (_rt != null)
                {
                    Canvas.SetLeft(_rt, newX + ((System.Windows.Shapes.Ellipse)sender).Width / 2 - _rt.Width / 2);
                    Canvas.SetTop(_rt, newY + ((System.Windows.Shapes.Ellipse)sender).Height / 2 - _rt.Width / 2);

                    double imagewidth = ((BitmapImage)(Image1.Source)).PixelWidth;
                    double imageheight = ((BitmapImage)(Image1.Source)).PixelHeight;
                    this.UpdateLayout();

                    double wpfimagewidth = Image1.ActualWidth;

                    double ratio = imagewidth / wpfimagewidth;

                    double gridTop = Canvas.GetTop(Grid1);
                    double gridLeft = Canvas.GetLeft(Grid1);

                    UpdateMagnifier(
                        (int)((newX + ((System.Windows.Shapes.Ellipse)sender).Width / 2 - gridLeft) * ratio - 25),
                        (int)((newY + ((System.Windows.Shapes.Ellipse)sender).Height / 2 - gridTop) * ratio - 25)
                        );

                }

           }
           
        }




        void ep_MouseMove(object sender, MouseEventArgs e)
        {
          
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Point p = e.GetPosition((UIElement)sender);
            System.Diagnostics.Debug.Print("Mouse move=" + p.X + "," + p.Y);
            double deltaX = p.X - _mouseX0;
            double deltaY = p.Y - _mouseY0;

            double newX = Canvas.GetLeft((UIElement)sender) + deltaX;
            double newY = Canvas.GetTop((UIElement)sender)  + deltaY;

            Canvas.SetLeft((UIElement)sender,newX);
            Canvas.SetTop((UIElement)sender, newY);


          

            if (_rt != null)
            {
                Canvas.SetLeft(_rt, newX + ((System.Windows.Shapes.Ellipse)sender).Width /2 - _rt.Width / 2);
                Canvas.SetTop(_rt, newY + ((System.Windows.Shapes.Ellipse)sender).Height / 2 - _rt.Width / 2);


                double imagewidth = ((BitmapImage)(Image1.Source)).PixelWidth;
                double imageheight = ((BitmapImage)(Image1.Source)).PixelHeight;
                this.UpdateLayout();

                double wpfimagewidth = Image1.ActualWidth;

                double ratio = imagewidth / wpfimagewidth;

                 double gridTop=Canvas.GetTop(Grid1);
                 double gridLeft = Canvas.GetLeft(Grid1);
                UpdateMagnifier(
                    (int)((newX + ((System.Windows.Shapes.Ellipse)sender).Width / 2 - gridLeft) * ratio - 25),
                    (int)((newY + ((System.Windows.Shapes.Ellipse)sender).Height / 2 - gridTop) * ratio - 25)
                    );
            }


        }

        void ep_MouseDown(object sender, MouseButtonEventArgs e)
        {
   

            if (e.LeftButton != MouseButtonState.Pressed) return;

            Keyboard.Focus((UIElement)sender);

            if(_rt!=null)
            {
                Canvas1.Children.Remove(_rt);
            }

            Point p = e.GetPosition((UIElement)sender);
            System.Diagnostics.Debug.Print("Mouse down=" +p.X + "," + p.Y);
            _mouseX0 = p.X;
            _mouseY0 = p.Y;

            System.Windows.Shapes.Rectangle  rt = new System.Windows.Shapes.Rectangle ();
            rt.Name = "RT";
            rt.Width = 5;
            rt.Height = 5;

            double x0 = Canvas.GetLeft((UIElement)sender) + ((System.Windows.Shapes.Ellipse)sender).Width / 2 - rt.Width / 2;
            double y0 = Canvas.GetTop((UIElement)sender) + ((System.Windows.Shapes.Ellipse)sender).Height / 2 - rt.Width / 2;

            rt.Stroke = System.Windows.Media.Brushes.Red  ;
            rt.Fill = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
            rt.HorizontalAlignment = HorizontalAlignment.Left  ;
            rt.VerticalAlignment = VerticalAlignment.Top  ;
            rt.Uid = rt.Name;
            Canvas1.Children.Add(rt);
            Canvas.SetLeft(rt, x0);
            Canvas.SetTop(rt,y0);
            Canvas.SetZIndex(rt,Canvas1.Children.Count -1);

            double imagewidth = ((BitmapImage)(Image1.Source)).PixelWidth;
            double imageheight = ((BitmapImage)(Image1.Source)).PixelHeight;
            this.UpdateLayout();

            double wpfimagewidth = Image1.ActualWidth;

            double ratio = imagewidth / wpfimagewidth;

            double gridTop = Canvas.GetTop(Grid1);
            double gridLeft = Canvas.GetLeft(Grid1);

            UpdateMagnifier(
                (int)((x0 + rt.Width / 2 - gridLeft) * ratio - 25),
                (int)((y0 + rt.Height/2 - gridTop) * ratio - 25)
                );

            _rt = rt;

        }




        public void LoadFacialPointsFromCacheFaceRects()
        {
             System.Diagnostics.Debug.Print("LoadFacialPointsFromCacheFaceRects");
             Button1_Click(winSelectFace, null);
        }

        public void LoadFacialPoints()
        {
            System.Diagnostics.Debug.Print("LoadFacialPoints");
            Button1_Click(null, null);
        }

        private void UpdateMagnifier(int x, int y)
        {
            try
            {
                BitmapImage bmi = Image1.Source as BitmapImage;
                int byteperpixel=(bmi.Format.BitsPerPixel + 7) / 8;
                int stride = bmi.PixelWidth * byteperpixel;
                byte[] _buffer = new byte[_mag_size * stride];

                bmi.CopyPixels(new Int32Rect(x, y, _mag_size, _mag_size), _buffer, stride, 0);
                //Draw the cross bars
                for (int i = 0; i < _mag_size; i++)
                    for (int k = 0; k < 2;k++ )
                        _buffer[(_mag_size/2 -1) * stride + i * byteperpixel + k] = 255;

                for (int j = 0; j < _mag_size; j++)
                    for (int k = 0; k < 2; k++)
                    {
                        _buffer[j * stride + (_mag_size/2 -1) * byteperpixel + k] = 255;
                    }

                _wbitmap.WritePixels(new Int32Rect(0, 0, _mag_size, _mag_size), _buffer, stride, 0);
            }
            catch
            {

            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (((MainWindow)this.Owner).DegPreRotate != 0) chkRotate.IsChecked = true;
            _isloaded = true;
            System.Diagnostics.Debug.Print("WinLoad");

             Vector offset = VisualTreeHelper.GetOffset(((MainWindow)this.Owner).MainGrid );           

            this.Top = ((MainWindow)this.Owner).Top + 
                         offset.Y+ 
                         SystemParameters.WindowCaptionHeight ;
            this.Left = ((MainWindow)this.Owner).Left +
                        offset.X;



            _wbitmap = new WriteableBitmap(_mag_size, _mag_size, 96, 96, PixelFormats.Bgra32, null);

            winMagnifier = new Window3();
            winMagnifier.Image1.Source = _wbitmap;
            UpdateMagnifier(0, 0);
            winMagnifier.Owner = this;
            winMagnifier.Show();
            winMagnifier.Top = this.Top + Canvas.GetTop(Grid1) + SystemParameters.WindowCaptionHeight +10;
            winMagnifier.Left = this.Left + Canvas.GetLeft(Grid1) +10;
           if(winSelectFace!=null)
           {
               winSelectFace.Close();
               winSelectFace = null;
           }
           Button1_Click(null, null);
           Magnifier = winMagnifier;
         

        }

        private string UpdateFacialPoints()
        {
            List<FeaturePointType> facepoints = new List<FeaturePointType>();
            for (int i = Canvas1.Children.Count - 1; i >= 0; i--)
            {
                if (Canvas1.Children[i].GetType() == typeof(System.Windows.Shapes.Ellipse))
                {
                    FeaturePointType fp = new FeaturePointType();
                    fp.desp = ((System.Windows.Shapes.Ellipse)(Canvas1.Children[i])).Name;
                    double x = Canvas.GetLeft(Canvas1.Children[i]) - Canvas.GetLeft(Grid1) + ((System.Windows.Shapes.Ellipse)Canvas1.Children[i]).Width / 2;
                    double y = Canvas.GetTop(Canvas1.Children[i]) - Canvas.GetTop(Grid1) + ((System.Windows.Shapes.Ellipse)Canvas1.Children[i]).Height / 2;
                    fp.pt = new Point(x, y);
                    facepoints.Add(fp);
                }

            }
            var sortedlist = facepoints.OrderBy(o => o.pt.Y).ToList();
            //top 2 points for eyes
            FeaturePointType fpEyeRight = (sortedlist[0].pt.X < sortedlist[1].pt.X) ? sortedlist[0] : sortedlist[1];
            FeaturePointType fpEyeLeft = (sortedlist[1].Equals(fpEyeRight)) ? sortedlist[0] : sortedlist[1];
            //next point is nose
            FeaturePointType fpNose = sortedlist[2];
            //next point is mouth
            FeaturePointType fpMouth = sortedlist[3];
            //last pt is chin
            FeaturePointType fpChin = sortedlist[4];

            //Validation
          
            double reyechindist = Math.Abs(fpChin.pt.Y - fpEyeRight.pt.Y);
            if (reyechindist < 1) return "Invalid Eye to Chin dist";
            
            double Normalized_eyedist = Math.Abs(fpEyeLeft.pt.X - fpEyeRight.pt.X)/reyechindist;
            double Normalized_reye2nosedist = Math.Abs(fpNose.pt.Y - fpEyeRight.pt.Y) / reyechindist;
            double Normalized_nosemouthdist = Math.Abs(fpMouth.pt.Y - fpNose.pt.Y) / reyechindist;
            double Normalized_mouthchindist = Math.Abs(fpChin.pt.Y - fpMouth.pt.Y) / reyechindist;

            //1) Check min distance between eyes
            if (Normalized_eyedist < 0.2) return "Invalid Distance between Eyes";
            if(Normalized_reye2nosedist<0.2) return "Invalid Nose to Eye Distance";
            if (Normalized_nosemouthdist < 0.01) return "Invalid Nose to Mouth Distance";
            if (Normalized_mouthchindist < 0.05) return "Invalid Mouth to Chin Distance";

            double imagewidth = ((BitmapImage)(Image1.Source)).PixelWidth;
            double imageheight = ((BitmapImage)(Image1.Source)).PixelHeight;
             this.UpdateLayout();

            double wpfimagewidth = Image1.ActualWidth;

            double ratio = imagewidth / wpfimagewidth;

            List<FeaturePointType> imagefacepoints = new List<FeaturePointType>();
            for (int i = 0; i < sortedlist.Count; i++)
            {
                FeaturePointType fp = new FeaturePointType();
                fp.pt = new Point(sortedlist[i].pt.X * ratio, sortedlist[i].pt.Y * ratio);
                string desp = "";
                if (sortedlist[i].Equals(fpEyeRight)) desp = "RightEye1";
                if (sortedlist[i].Equals(fpEyeLeft)) desp = "LeftEye1";
                if (sortedlist[i].Equals(fpNose)) desp = "Nose1";
                if (sortedlist[i].Equals(fpMouth)) desp = "Mouth3";
                if (sortedlist[i].Equals(fpChin)) desp = "Chin1";
                fp.desp = desp;
                imagefacepoints.Add(fp);

            }




            ((MainWindow)this.Owner).ImageFacePoints = imagefacepoints;

            //calculate degPreRotate
            if (chkRotate.IsChecked == true)
            {
                double tan = (fpEyeLeft.pt.Y - fpEyeRight.pt.Y) / (fpEyeLeft.pt.X - fpEyeRight.pt.X);
                float angle = (float)(Math.Atan(tan) * 180 / Math.PI);
                ((MainWindow)this.Owner).DegPreRotate = -1f * angle;
            }
            else
            {
                ((MainWindow)this.Owner).DegPreRotate = 0;
            }

            ((MainWindow)this.Owner).LastFaceFileLoaded = _currentfile;
            return "";
        }

        //Update the face marker position, do the face-mesh fitting, update all the images and display fitting error
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
       

           string errmsg=UpdateFacialPoints();
           if (errmsg !="")
           {
               MessageBox.Show(errmsg);
               return;
           }
           string fittingErr=((MainWindow)this.Owner).processTextureFitting();//_currentfile);
           FittingError = fittingErr;
           var winViewer = ((MainWindow)this.Owner).Viewer;
            if(winViewer !=null && winViewer.IsWinLoaded)
            {
                winViewer.UpdateDisplay();
            }
        }



        private void Image1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //make sure that there was a corresponding mousedown and not a passed through
            //from open dialog box
            if(!_bMouseDown )
            {
                return;
            }

            _bMouseDown = false;

            System.Diagnostics.Debug.Print(sender.ToString());

            List<UIElement> list = Canvas1.Children.Cast<UIElement>().ToList();
            System.Windows.Shapes.Ellipse ep = (System.Windows.Shapes.Ellipse)list.Single(s => s.Uid == "EP" + _currentEllipseIndex);

            _currentEllipseIndex = (_currentEllipseIndex + 1) % 5;
            Point pt = e.GetPosition(Canvas1);

            Canvas.SetLeft(ep, pt.X - ep.Width/2) ;
            Canvas.SetTop(ep, pt.Y - ep.Height/2 );

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _isloaded = false;
            winMagnifier.Close();
            winMagnifier = null;
            if (winSelectFace!=null)
            {
                winSelectFace.Close();
                winSelectFace = null;
            }

        }


        //for implemenentation of Alt-U and Alt-B
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {

                Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
                if (key == Key.U)
                {
                    Button2_Click(null, null);
                }

                if(key ==Key.B)
                {

                    Button3_Click(null, null);


                }
            }
        }


        //Get and Load Best Fitting Mesh
        private void Button3_Click(object sender, RoutedEventArgs e)
        {
                    string errmsg = UpdateFacialPoints();
                    if (errmsg != "")
                    {
                        MessageBox.Show(errmsg);
                        return;
                    }
                    string meshname=((MainWindow)this.Owner).getBestFittingMesh(_currentfile,(bool)chkNewModel.IsChecked);

                    ((MainWindow)this.Owner).LoadFaceMesh(meshname,true );
        }

        private void Image1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _bMouseDown = true;
        }


    }
}
