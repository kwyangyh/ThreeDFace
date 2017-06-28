/////////////////////////////////
// Yang Kok Wah, 13 May 2017
////////////////////////////////

namespace ThreeDFaces
{
    using System;
    using System.ComponentModel;
    using System.Windows.Media.Media3D;
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

    /// <summary>
    /// Main Window
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDisposable
    {


        //Flag to indicate that the program is currently locked in 
        // a process and should not be interrupted
        public bool bSyncLock = false;


        //for mesh left-right mapping
        private List<FacePointMapping> _Mappings;
        private int _iMapping = 0; //0=None,1=Right,2=Left


        //number of slots in the VGrid
        private int _numVGridImageSlot=6;

        //start index for the snapimage to display on VGrid
        private int _startindex = 0;
        private List<ImageSource> _snapimages = new List<ImageSource>();

        //number of slot in the GridFModel
        private int _numFGridImageSlot = 6;

        //start index for fmodel to display on GridFModel
        private int _startindexfm = 0;
        private List<Image> _fmimages = new List<Image>();


        //use to assist on reloading of face points
        private string _lastfacefileloaded = "";
        private bool _bUselastfacefile = false;
       
        //for eye alignment
        private float _degPreRotate=0;

        //start mesh position
        private string _startmeshTranslationString = "0 0 0";

        //stored face points marked by user
        private List<FeaturePointType> _imagefacepoints;

        //Working bitmapimages
        private BitmapImage _orgbitmap = null;
        private BitmapImage _colorbitmap = null;
        private System.Drawing.Bitmap _bmcolor = null;
        private BitmapImage _refbitmap = null;
        
        //To assist in the toggling of startup cube and face-cube
        private MeshGeometry3D _startupgeometry = null;
        private Brush _startupbrush = null;

        //Flag to indicate that the CubeMesh is loaded
        private bool _bCubeMeshLoaded = false;
 

        //Original TriangleIndices
        private Int32Collection _orgtriindices = null;
        //Original Mesh Positions
        private Point3DCollection _orgmeshpos=null;

        //Original Texture
        private PointCollection _orgtexture = null;

        //Original Brush to Render Mesh
        private Brush _orgbrush=null; 

        //working string to display on text boxes
        private string _text1="";
        private string _text2 = "";

        //Flag to indicate that face mesh File is loaded
        private bool bIsXLoaded = false;

        //small change to direction light in X direction
        private float _deltaXdir = 0.5f;
        //small change to direction light in Y direction
        private float _deltaYdir = 0.5f;

        //for animating the directional lights
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        System.Windows.Threading.DispatcherTimer dispatcherTimer2 = new System.Windows.Threading.DispatcherTimer(); 

        ////Location in texture file where the face image is located
        private System.Drawing.Rectangle _meshRect = System.Drawing.Rectangle.Empty;
        //Anchor point for the face image
        private System.Drawing.Point _meshRighteye = System.Drawing.Point.Empty;



        //Facial Point Marker window
        Window2 winFitting = null;


        //Viewer
        Window1 winViewer = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            this.InitializeComponent();
            this.DataContext = this;


        }

        public Window1 Viewer
        {
            get { return winViewer; }
        }

        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public System.Drawing.Rectangle MeshRect
        {
            get { return _meshRect; }

        }

        //This property return the right eye point
        //This point is important as it the point 
        //connnecting the face image and the mesh texture
        public System.Drawing.Point MeshRighteye
        {
            get { return _meshRighteye; }
        }

        public float DegPreRotate
        {
            get { return _degPreRotate; }
            set { _degPreRotate = (float)value; }
        }

        
        public List<FeaturePointType>  ImageFacePoints
        {
            get
            {
                return _imagefacepoints;
            }

            set
            {
                _imagefacepoints = value;
            }

        }

        public string LastFaceFileLoaded
        {
            get { return _lastfacefileloaded; }
            set { _lastfacefileloaded = (string)value; }
        }

        public string TranslationString
        {
            get { return _text1; }
            set
            {
                if (value != _text1)
                {
                    _text1 = value;
                    OnPropertyChanged("TranslationString");
                   
                }
            }
        }

        public string RotationString
        {
            get { return _text2; }
            set
            {
                if (value != _text2)
                {
                    _text2 = value;
                    OnPropertyChanged("RotationString");

                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Called when disposed of
        /// </summary>
        public void Dispose()
        {
          
            GC.SuppressFinalize(this);
        }




        /// <summary>
        /// Fires when Window is Loaded
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        
            this._orgbrush = theMaterial.Brush;
            TranslationString = "0 0 0";
            RotationString = "0 0 0";
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.IsEnabled = false;

            dispatcherTimer2.Interval = new TimeSpan(0, 0, 0,0, 200);
            dispatcherTimer2.Tick += dispatcherTimer2_Tick;
            dispatcherTimer2.IsEnabled = false;

            //Default built in meshes' billboard images
            FModel0.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\mesh\\meshGenericFace.jpg"));
            FModel1.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\mesh\\meshRoundFace.jpg"));
            FModel2.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\mesh\\meshBroadFace.jpg"));
            FModel3.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\mesh\\meshLongFace.jpg"));
            FModel4.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\mesh\\meshOvalBroadFace.jpg"));
            FModel5.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\mesh\\meshOvalFace.jpg"));

            List<UIElement> list = GridFModel.Children.Cast<UIElement>().ToList();
            //store the meshes' billboard images
            for(int i=0;i<_numFGridImageSlot;i++)
            {
                Image fimg=(Image)list.Single(s => s.Uid == ("FModel" + i));
                Image img = new Image();
                img.Source = fimg.Source.Clone();
                img.Tag = fimg.Tag;
                _fmimages.Add(img);

            }
           

            //include meshes in newmesh directory for customized meshes
            var newmeshfiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "newmesh", "*.x", SearchOption.TopDirectoryOnly);
            foreach( var newmeshfile in newmeshfiles )
            {
                var fileparts=newmeshfile.Split('\\');
                var filename=fileparts[fileparts.Length -1];
                var meshname=filename.Substring(0,filename.Length -2);
                var meshid=meshname.Substring(4);
                Image img = new Image();
                //process in a using block so that bitmaps are disposed and bitmap files not locked
                using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(AppDomain.CurrentDomain.BaseDirectory + "\\newmesh\\" + meshname + ".jpg"))
                {
                    BitmapImage bmi = CCommon.Bitmap2BitmapImage(bm);
                    img.Source = bmi;
                    img.Tag = "Image_" + meshid;
                    _fmimages.Add(img);
                }
            }
             

            _orgmeshpos = theGeometry.Positions.Clone();
            SaveCurrentBrush(theMaterial.Brush);
            SaveCurrentGeometry();   
        }


        void dispatcherTimer2_Tick(object sender, EventArgs e)
        {

            var dir = dirlight.Direction;

            if (dir.Y > 5 || dir.Y < -5) _deltaYdir = -1 * _deltaYdir;
            dir.Y += _deltaYdir;
            dirlight.Direction = new Vector3D(dir.X, dir.Y, dir.Z);
        }


        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var dir = dirlight.Direction;

            if (dir.X > 5 || dir.X<-5) _deltaXdir = -1 * _deltaXdir;
            dir.X += _deltaXdir;
            dirlight.Direction = new Vector3D(dir.X, dir.Y, dir.Z);
            
        }

  

        private void UpdateMesh(float offsetx,float offsety,float offsetz)
        {
            
            var vertices = _orgmeshpos;
    
            for (int i = 0; i < vertices.Count; i++)
            {
                var vert = vertices[i];
                vert.Z += offsetz;
                vert.Y += offsety;
                vert.X += offsetx;
                this.theGeometry.Positions[i] = new Point3D(vert.X, vert.Y, vert.Z);
            }

          
        }


        //Button1_Click calls Update Mesh
        private void Button1_Click(object sender, RoutedEventArgs e)
        {

            if (_orgmeshpos == null) return;
           
            var v = Text1.Text.Split(' ');

            UpdateMesh(float.Parse(v[0]), float.Parse(v[1]), float.Parse(v[2]));
        }

        private void SaveCurrentBrush(Brush brush)
        {
            _startupbrush = brush.Clone();
        }

        private void SaveCurrentGeometry()
        {
            _startupgeometry = new MeshGeometry3D();
            _startupgeometry.Positions = theGeometry.Positions.Clone();
            _startupgeometry.TextureCoordinates = theGeometry.TextureCoordinates.Clone();
            _startupgeometry.TriangleIndices = theGeometry.TriangleIndices.Clone();
        }

        private void LoadCubeMesh(string imgfile)
        {

            _bCubeMeshLoaded = true;
            string Positions = "-0.05,-0.1,0   0.05,-0.1,0   -0.05,0,0   0.05,0,0  "+  //Front
                               "-0.05,-0.1,-0.1   0.05,-0.1,-0.1   -0.05,0,-0.1   0.05,0,-0.1 " + //Back
                               "-0.05,-0.1,-0.1 -0.05,-0.1,0 -0.05,0,-0.1 -0.05,0,0 " + //Left
                               "0.05,-0.1,-0.1 0.05,-0.1,0 0.05,0,-0.1 0.05,0,0"; //Right

            string TextureCoordinates = "0,1 1,1 0,0 1,0 "+  //Front
                                        "0,0 1,0 0,1 1,1 " + //Back
                                        "0,1 1,1 0,0 1,0 " + //Left
                                        "0,1 1,1 0,0 1,0"; //Right

//////////////////////////////////////////////////////////////
//
//           Back Face  
//            6 * ----------* 7
//              |           |
//              |           |
//              |           |               
//            4 *-----------* 5
//   
//                  Front Face
//                     2 * ----------* 3
//                       |           |
//                       |           |
//                       |           |               
//                     0 *-----------* 1
//
//           Left Face                    Right Face
//           10 * ----------*11           10* ----------*11
//              |           |               |           |
//              |           |               |           |
//              |           |               |           |   
//            8 *-----------* 9 //        8 *-----------* 9
//   
//////////////////////////////////////////////////////////////// 

            string TriangleIndices = "0,1,2 1,3,2 "+ //Front
                                     "0,5,1 0,4,5 "+ //Bottom
                                     "6,5,4 6,7,5 "+ //Back
                                     "2,3,6 3,7,6 "+ //Top
                                     "8,9,10 9,11,10 "+ //Left
                                     "15,13,12 15,12,14"; //Right


            var positions = Positions.Split(new string[]{" "},StringSplitOptions.RemoveEmptyEntries );
            theGeometry.Positions = new Point3DCollection();
            foreach(var position in positions )
            {
                var xyz = position.Split(',');
                theGeometry.Positions.Add(new Point3D(double.Parse(xyz[0]),
                                                      double.Parse(xyz[1]), 
                                                      double.Parse(xyz[2])));
            }
            theGeometry.TextureCoordinates = new PointCollection();
            var textureCoordinates = TextureCoordinates.Split(new string[]{" "},StringSplitOptions.RemoveEmptyEntries );
            foreach(var textureCoordinate in textureCoordinates  )
            {
                var xy = textureCoordinate.Split(',');
                theGeometry.TextureCoordinates.Add(new Point(double.Parse(xy[0]),
                                                              double.Parse(xy[1])));
            }

            theGeometry.TriangleIndices =new Int32Collection();
            var triangleIndices=TriangleIndices.Split(new string[]{" "},StringSplitOptions.RemoveEmptyEntries );
            foreach(var triangleIndex in triangleIndices)
            {
                var tri=triangleIndex.Split(',');
                theGeometry.TriangleIndices.Add(int.Parse(tri[0]));
                theGeometry.TriangleIndices.Add(int.Parse(tri[1]));
                theGeometry.TriangleIndices.Add(int.Parse(tri[2]));

            }

            BitmapImage bmi = new BitmapImage(new Uri(imgfile));
            theMaterial.Brush = new ImageBrush(bmi)
            {
                ViewportUnits = BrushMappingMode.Absolute
            };

            _orgmeshpos = theGeometry.Positions.Clone();
            //Update the mesh
            Button1_Click(null, null);


        }



        private void MapTexture(int mapping)
        {
            //reset
            for (int i = 0; i < theGeometry.TextureCoordinates.Count; i++)
            {
                theGeometry.TextureCoordinates[i] = new Point(_orgtexture[i].X,_orgtexture[i].Y);
               
            }

            if (mapping == 0) return;
            //alter
            for (int i = 0; i < theGeometry.TextureCoordinates.Count; i++)
            {
                int index = i;
                FacePointMapping fpm = _Mappings.Find(p => p.index == i);
                switch (mapping)
                {
                    case 1: /*Map Left*/
                        if (fpm.side == "L") theGeometry.TextureCoordinates[i] = 
                                     new Point( _orgtexture[fpm.mappedindex].X,_orgtexture[fpm.mappedindex].Y);
                        break;
                    case 2: /*Map right*/
                        if (fpm.side == "R") theGeometry.TextureCoordinates[i] =
                                     new Point(_orgtexture[fpm.mappedindex].X, _orgtexture[fpm.mappedindex].Y);
                        break;

                }

            }
        }

        ////Alter triindices to do mapping
        //private void MapTriIndices(int mapping)
        //{
        //    //reset
        //    for(int i=0;i<theGeometry.TriangleIndices.Count;i++)
        //    {
        //        theGeometry.TriangleIndices[i] = _orgtriindices[i];
        //    }
        //    if(mapping ==0) return;

        //    //alter
        //    for(int i=0;i<theGeometry.TriangleIndices.Count;i=i+3)
        //        for (int j = 0; j < 3; j++)
        //        {
        //            int index = theGeometry.TriangleIndices[i+j];
        //            FacePointMapping fpm = _Mappings.Find(p => p.index == index);
        //            switch (mapping)
        //            {
        //                case 1: /*Map Left*/
        //                    if (fpm.side == "L") theGeometry.TriangleIndices[i+j] = fpm.mappedindex;
                          
        //                    break;
        //                case 2: /*Map right*/
        //                    if (fpm.side == "R") theGeometry.TriangleIndices[i+j] = fpm.mappedindex;
                           
        //                    break;

        //            }


        //        }
            
        //}


        //Load mesh and related files
        private void LoadXFile(string xfile)
        {
            var picfilename = xfile.Substring(0, xfile.Length - 2) + ".png";
            var filename = xfile;
            bool bIsFirstTimeXLoaded = false;
            if (File.Exists(picfilename))
            {
                if (!bIsXLoaded) bIsFirstTimeXLoaded = true;
                bIsXLoaded = true;

  
                CCommon.LoadImageSource(RefImage, picfilename,false );
               
                if(this._colorbitmap !=null) this._colorbitmap =null;
                this._colorbitmap = (BitmapImage)RefImage.Source.Clone();//new BitmapImage(new Uri(picfilename));
                if (_bmcolor != null) _bmcolor = null;
                _bmcolor = CCommon.BitmapImage2Bitmap(this._colorbitmap);

                slider_ValueChanged(sliderBrightness, null);


                if (this._refbitmap != null) this._refbitmap = null;
                this._refbitmap = this._colorbitmap.Clone();

                if (this._orgbitmap != null) this._orgbitmap = null;
                this._orgbitmap = this._colorbitmap.Clone();

             
                //this.theMaterial.Brush = new ImageBrush(this._colorbitmap)
                //{

                //    ViewportUnits = BrushMappingMode.Absolute
                //};
            }
            else
            {
                this.theMaterial.Brush = _orgbrush;
            }


            //meshmapping
            using (TextReader tr = File.OpenText(System.AppDomain.CurrentDomain.BaseDirectory + "//meshmapping.txt"))
            {
                string s = tr.ReadToEnd();
                var lines = s.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                _Mappings = new List<FacePointMapping>();
                var n = lines.Length;
                for (int i = 0; i < n; i++)
                {
                    var v = lines[i].Split(':');
                    FacePointMapping fpm = new FacePointMapping();
                    fpm.side = v[0];
                    var v1 = v[1].Split('=');
                    fpm.index = int.Parse(v1[0]);
                    fpm.mappedindex = int.Parse(v1[1]);
                    if (fpm.index == fpm.mappedindex)
                        fpm.side = "C";
                    _Mappings.Add(fpm);
                }
            }



            //offset to use for initial translation of mesh points for better viewing
            double xoff = 0, yoff = 0, zoff = 0;
            this.theGeometry.Normals = null;

            using (TextReader tr = File.OpenText(System.AppDomain.CurrentDomain.BaseDirectory + "//tri_index.txt"))
            {
                string s = tr.ReadToEnd();
                var lines = s.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                theGeometry.TriangleIndices = new Int32Collection();
                _orgtriindices = new Int32Collection();
                var n = lines.Length;
                for (int i = 0; i < n; i++)
                {
                    theGeometry.TriangleIndices.Add(int.Parse(lines[i]));
                    _orgtriindices.Add(int.Parse(lines[i]));
                }
            }

            // there are 1347 X2 lines in the .x file
            //The first 1347 lines are for position of mesh points
            //The next 1347 are corresponding texture points
            using (TextReader tr = File.OpenText(filename))
            {
                string s = tr.ReadToEnd();
                theGeometry.Positions = new Point3DCollection();
                theGeometry.TextureCoordinates = new PointCollection();
                _orgtexture = new PointCollection();

                _orgmeshpos = new Point3DCollection();
                var lines = s.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);


                int n = lines.Length;

                if (lines.Length > 1347)
                    n = lines.Length / 2;


                double accx = 0, accy = 0, accz = 0;

                for (int i = 0; i < n; i++)
                {

                    var c = lines[i].Split(':');

                    var vertice = new Point3D(double.Parse(c[0]), double.Parse(c[1]), double.Parse(c[2]));

                    this.theGeometry.Positions.Add(vertice);
                    _orgmeshpos.Add(vertice);
                    accx += vertice.X;
                    accy += vertice.Y;
                    accz += vertice.Z;

                }

     
                xoff = accx / 1347;
                yoff = accy / 1347;
                zoff = accz / 1347;



                if (lines.Length > 1347)
                {
                    for (int i = n; i < lines.Length; i++)
                    {
                        var c = lines[i].Split(':');

                        var vertice = new Point(double.Parse(c[0]), double.Parse(c[1]));

                        this.theGeometry.TextureCoordinates.Add(vertice);
                        _orgtexture.Add(new Point(vertice.X, vertice.Y));

                    }

                    
                }

                //Left right mapping..



            }


            //Translate the mesh to a default location for good frontal viewing
            sliderx.Value = -1 * xoff;// +0.06;// 0.04; 
            slidery.Value = -1 * yoff - 0.1;
            sliderz.Value = -1 * zoff + 0.1;

            //reset camera orientation only when the first mesh is loaded
            if (bIsFirstTimeXLoaded)
            {
                vscroll.Value = 0;
                vscrollz.Value = 0;
                hscroll.Value = 0;
                RotationString = "0 0 0";
            }


            TranslationString = string.Format("{0:0.##} {1:0.##} {2:0.##}",
                sliderx.Value,
                slidery.Value,
                sliderz.Value);

            _startmeshTranslationString = TranslationString;


            //Draw the mesh on an image
            //////////////////////////////////////////////
            //1. Find the bounding rect
            double minx=1.0,maxx=0,miny=1,maxy=0;

            for(int i=0;i<theGeometry.TextureCoordinates.Count;i++)
            {
                if (theGeometry.TextureCoordinates[i].X < minx) minx = theGeometry.TextureCoordinates[i].X;
                if (theGeometry.TextureCoordinates[i].Y < miny) miny= theGeometry.TextureCoordinates[i].Y;
                if (theGeometry.TextureCoordinates[i].X > maxx) maxx = theGeometry.TextureCoordinates[i].X;
                if (theGeometry.TextureCoordinates[i].Y > maxy) maxy = theGeometry.TextureCoordinates[i].Y;
            }

            //2. normailze
            int width =(int)( (maxx - minx)*1920) +1;
            int height = (int)((maxy - miny)*1080) +1;
            List<System.Drawing.PointF> meshpoints = new List<System.Drawing.PointF>();
            for (int i = 0; i < theGeometry.TextureCoordinates.Count;i++ )
            {
                meshpoints.Add(new System.Drawing.PointF((float)(theGeometry.TextureCoordinates[i].X - minx)*1920, 
                                                         (float)(theGeometry.TextureCoordinates[i].Y - miny)*1080)
                               );
            }

            ////The bitmap must have ARGB Pixel format to support transparency
            System.Drawing.Bitmap bm=new System.Drawing.Bitmap(width,height,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ///////////////////////////////////////////////////////

            System.Drawing.Graphics gbm=System.Drawing.Graphics.FromImage(bm);
            gbm.Clear(System.Drawing.Color.Transparent);

            Point righteyeTop = theGeometry.TextureCoordinates[328];
            Point righteyeBottom = theGeometry.TextureCoordinates[1105];

            //Right eye point is the anchor point
            //for aligning the face image and the mesh
            _meshRighteye = new System.Drawing.Point((int)((righteyeTop.X + righteyeBottom.X) / 2), 
                                                          (int)((righteyeTop.Y + righteyeBottom.Y) / 2));
            _meshRect = new System.Drawing.Rectangle((int)(minx*1920), (int)(miny*1080), width, height);




            for(int i=0;i<theGeometry.TriangleIndices.Count ;i=i+3)
            {
               
                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(70, System.Drawing.Color.Red  ));

                //Use different pen for these triangles connected to these points
                //10 (center of base of upper lip)
                if (theGeometry.TriangleIndices[i] ==10 || theGeometry.TriangleIndices[i + 1] ==10 || theGeometry.TriangleIndices[i + 2] ==10)
                    pen = System.Drawing.Pens.GreenYellow;
                
                //14 (nose tip)
                if (theGeometry.TriangleIndices[i] == 14 || theGeometry.TriangleIndices[i + 1] == 14 || theGeometry.TriangleIndices[i + 2] == 14)
                    pen = System.Drawing.Pens.GreenYellow;
                //0 (chin)
                if (theGeometry.TriangleIndices[i] == 0 || theGeometry.TriangleIndices[i + 1] == 0 || theGeometry.TriangleIndices[i + 2] == 0)
                    pen = System.Drawing.Pens.GreenYellow;
                //328-1105 (right eye is between this 2 points)
                if (theGeometry.TriangleIndices[i] == 1105 || theGeometry.TriangleIndices[i + 1] == 1105 || theGeometry.TriangleIndices[i + 2] == 1105)
                    pen = System.Drawing.Pens.GreenYellow;
                //883-1092 (left eye is between these 2 points)
                if (theGeometry.TriangleIndices[i] == 1092 || theGeometry.TriangleIndices[i + 1] == 1092|| theGeometry.TriangleIndices[i + 2] == 1092)
                    pen = System.Drawing.Pens.GreenYellow;

                if (CCommon.ArePointsClockwise(meshpoints[theGeometry.TriangleIndices[i]],
                                   meshpoints[theGeometry.TriangleIndices[i + 1]],
                                   meshpoints[theGeometry.TriangleIndices[i + 2]]))
                {
                    if (CCommon.IsValidPoint(meshpoints[theGeometry.TriangleIndices[i]]) &&
                       CCommon.IsValidPoint(meshpoints[theGeometry.TriangleIndices[i+1]]) &&
                      CCommon.IsValidPoint(meshpoints[theGeometry.TriangleIndices[i+2]])  
                        )
                    gbm.DrawPolygon(pen, new System.Drawing.PointF[]
                     {
                       meshpoints[theGeometry.TriangleIndices[i]],
                       meshpoints[theGeometry.TriangleIndices[i+1]],
                       meshpoints[theGeometry.TriangleIndices[i+2]]
                     }
                    );
                }
              
            }
            gbm.Dispose();

            bm.Save(AppDomain.CurrentDomain.BaseDirectory +"\\temp\\~mesh.png",System.Drawing.Imaging.ImageFormat.Png);
            ImageMesh.Source = CCommon.Bitmap2BitmapImage(bm);
            bm.Dispose();
            bm = null;
            if (ImageMesh.Source != null) MeshImage.Source = ImageMesh.Source;
            //////////////////////////////////////////////////////////////////////////////////

            ShowMesh();

            //set the default ambient and directional lights color
            sliderColor_ValueChanged(null, null);
            

            //Apply Mapping
            MapTexture(_iMapping);

            //Update the mesh
            Button1_Click(null, null);
        }


        private void ShowMesh()
        {
            if (MeshImage != null && MeshImage.Source != null)
            {
                if (chkImageMesh.IsChecked == true)
                {
                    MeshGrid.Visibility = System.Windows.Visibility.Visible;
                    MeshImage.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    MeshGrid.Visibility = System.Windows.Visibility.Hidden;
                    MeshImage.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }





        //implementation of all slider (except color sliders) values updates
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
         
            var t = TranslationString.Split(' ');

            var tr = RotationString.Split(' ');
            
            bool bUpdate = false;

            if (((System.Windows.Controls.Slider)sender).Name == "sliderBrightness" ||
                ((System.Windows.Controls.Slider)sender).Name == "sliderContrast")
            {
                    //if (_colorbitmap == null) return;

                    //System.Drawing.Bitmap bm = CCommon.BitmapImage2Bitmap((BitmapImage)_colorbitmap);

                   if (_bmcolor == null) return;
                   System.Drawing.Bitmap bm = (System.Drawing.Bitmap)_bmcolor.Clone();

                    AForge.Imaging.Filters.BrightnessCorrection filterB = new AForge.Imaging.Filters.BrightnessCorrection();
                    AForge.Imaging.Filters.ContrastCorrection filterC = new AForge.Imaging.Filters.ContrastCorrection();

                    filterB.AdjustValue = (int)sliderBrightness.Value;
                    filterC.Factor = (int)sliderContrast.Value;
                  
                    bm = filterB.Apply(bm);
                    bm = filterC.Apply(bm);
               

                    BitmapImage bitmapimage = CCommon.Bitmap2BitmapImage(bm);
                    bm.Dispose();
                    bm = null;

                    if (theMaterial.Brush != null) theMaterial.Brush = null;


                    theMaterial.Brush = new ImageBrush(bitmapimage)
                    {

                        ViewportUnits = BrushMappingMode.Absolute
                    };
                                
            }

            if (((System.Windows.Controls.Slider)sender).Name == "vscrollz")
            {
                tr[2] = (e.NewValue).ToString();

            }

            if (((System.Windows.Controls.Slider)sender).Name == "hscroll")
            {

                tr[1] = (e.NewValue).ToString();
            }
            if (((System.Windows.Controls.Slider)sender).Name == "vscroll")
            {

                tr[0] = (e.NewValue).ToString();
            }


           if ( ((System.Windows.Controls.Slider)sender).Name =="sliderz")
           {
              t[2] = (e.NewValue).ToString();
              bUpdate = true;
           }

           if (((System.Windows.Controls.Slider)sender).Name == "slidery")
           {

               t[1] = (e.NewValue).ToString();
               bUpdate = true;
           }
           if (((System.Windows.Controls.Slider)sender).Name == "sliderx")
           {

               t[0] = (e.NewValue).ToString();
               bUpdate = true;
           }

           if (bUpdate)
           {
               TranslationString = string.Format("{0:0.##} {1:0.##} {2:0.##}", float.Parse(t[0]), float.Parse(t[1]), float.Parse(t[2]));
               Button1_Click(null, null);
           }

           RotationString = string.Format("{0:0.#} {1:0.#} {2:0.#}", float.Parse(tr[0]), float.Parse(tr[1]), float.Parse(tr[2]));

           
        }

        //for scrolling of mesh billbaord images on the Face Model Grid
        private void UpdateFMGrid(int startfrom)
        {
            List<UIElement> list = GridFModel.Children.Cast<UIElement>().ToList();

            int maxindex =  _fmimages.Count - 1;
            int startindex = 0;
            if (maxindex < _numFGridImageSlot)
            {
                startindex = 0;
            }
            else
            {
                if (startfrom < 0)
                    startindex = maxindex - _numFGridImageSlot + 1;
                else
                    startindex = startfrom;
            }

            int maxstartindex = maxindex - _numFGridImageSlot + 1;
            if (maxstartindex < 0) maxstartindex = 0;
            if (startindex > maxstartindex) startindex = maxstartindex;
            if (startindex < 0) startindex = 0;

            _startindexfm = startindex;

            for (int i = 0; i < _numFGridImageSlot; i++)
            {
                Image img = (Image)list.Single(s => s.Uid == ("FModel" + i));
                if (_fmimages.Count > (startindex + i))
                {
                    img.Source = _fmimages[startindex + i].Source;
                    img.Tag = _fmimages[startindex + i].Tag;
                }
                else
                    img.Source = null;
            }
        }

        //for scrolling of snapped pictures in the VGrid
        private void updateVGrid(int startfrom)
        {
            List<UIElement> list = GridVSnap.Children.Cast<UIElement>().ToList();

            int maxindex = _snapimages.Count - 1;
            int startindex = 0;
            if (maxindex <_numVGridImageSlot)
            {
                startindex = 0;
            }
            else
            {
                if (startfrom <0)
                    startindex = maxindex - _numVGridImageSlot +1;
                else
                    startindex = startfrom;
            }

            int maxstartindex = maxindex - _numVGridImageSlot +1;
            if (maxstartindex < 0) maxstartindex = 0;
            if (startindex > maxstartindex) startindex = maxstartindex;
            if (startindex < 0) startindex = 0;

            _startindex = startindex;

            for (int i = 0; i < _numVGridImageSlot; i++)
            {
              Image img = (Image)list.Single(s => s.Uid == ("VSnap" + i));
              if (_snapimages.Count > (startindex + i))
                  img.Source = _snapimages[startindex + i];
              else
                  img.Source = null;
            }
        }

        
        public BitmapSource CreateSnap()
        {
            var viewport = this.viewport3d;
            var renderTargetBitmap = new RenderTargetBitmap((int)(((int)viewport.ActualWidth + 3) / 4 * 4),
                                                            (int)viewport.ActualHeight,
                                                            96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(viewport);

            byte[] b = new byte[(int)renderTargetBitmap.Height * (int)renderTargetBitmap.Width * 4];
            int stride = ((int)renderTargetBitmap.Width) * 4;
            renderTargetBitmap.CopyPixels(b, stride, 0);

            //get bounding box;
            int x = 0, y = 0, minx = 99999, maxx = 0, miny = 99999, maxy = 0;
            //reset all the alpha bits
            for (int i = 0; i < b.Length; i = i + 4)
            {
                y = i / stride;
                x = (i % stride) / 4;

                if (b[i + 3] == 0) //if transparent we set to white
                {
                    b[i] = 255;
                    b[i + 1] = 255;
                    b[i + 2] = 255;
              //      b[i + 3] = 255;

                }
                else
                {
                    if (x > maxx) maxx = x;
                    if (x < minx) minx = x;
                    if (y > maxy) maxy = y;
                    if (y < miny) miny = y;

                }
            }

            BitmapSource image = BitmapSource.Create(
                (int)renderTargetBitmap.Width,
                (int)renderTargetBitmap.Height,
                96,
                96,
                PixelFormats.Bgra32,
                null,
                b,
                stride);

            int cropx = minx - 20;
            if (cropx < 0) cropx = 0;
            int cropy = miny - 20;

            if (cropy < 0) cropy = 0;

            int cropwidth = (((maxx - cropx + 20 + 1) + 3) / 4) * 4;
            int cropheight = maxy - cropy + 20 + 1;

            //check oversized cropping
            int excessx = cropwidth + cropx - image.PixelWidth;
            int excessy = cropheight + cropy - image.PixelHeight;
            if (excessx < 0) excessx = 0;
            if (excessy < 0) excessy = 0;
            excessx = ((excessx + 3) / 4) * 4;

            CroppedBitmap crop = null ;
            try
            {
                crop = new CroppedBitmap(image, new Int32Rect(cropx, cropy, cropwidth - excessx, cropheight - excessy));
            }
            catch
            {
           
            }

            return crop;
        }

        //Snap Button: Make Snapshot of Face 
        private void Button4_Click(object sender, RoutedEventArgs e)
        {
           //   if (!bIsXLoaded) return;

            BitmapSource crop = CreateSnap();
            if (crop == null) return;
            _snapimages.Add(crop/*destbmp*/);
            updateVGrid(-1);            

        }

        private void sliderColor_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
        //    if (!bIsXLoaded) return;
            if (sliderRed!= null && sliderGreen!= null && sliderBlue!= null && sliderAmb!=null)
            {
                Color color = Color.FromArgb(255, (byte)sliderRed.Value, (byte)sliderGreen.Value, (byte)sliderBlue.Value);
                if (labelColor != null)
                {
                    labelColor.Content = color.ToString();
                    labelColor.Background = new SolidColorBrush(color);
                }

                
                if (dirlight != null)
                    dirlight.Color = color;

                Color amcolor = Color.FromArgb(255, (byte)sliderAmb.Value, (byte)sliderAmb.Value, (byte)sliderAmb.Value);
                if (amlight != null)
                    amlight.Color = amcolor;
            }

        }

        //Animate Directional left right Light
        private void Button5_Click(object sender, RoutedEventArgs e)
        {
           dispatcherTimer.IsEnabled = !dispatcherTimer.IsEnabled;
           Button5.Foreground = dispatcherTimer.IsEnabled?new SolidColorBrush(Colors.Red):new SolidColorBrush(Colors.Black );
        }

        //Animate Directional Up down Light
        private void Button6_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer2.IsEnabled = !dispatcherTimer2.IsEnabled;
            Button6.Foreground = dispatcherTimer2.IsEnabled ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Black);
        }


        //reset Y directional light
        private void Button6_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            dispatcherTimer2.IsEnabled = false;
            Button6.Foreground = new SolidColorBrush(Colors.Black);
            var dir = dirlight.Direction;
            dir.Y = 0;
            dirlight.Direction = new Vector3D(dir.X, dir.Y, dir.Z);
        }

        //reset X directional light
        private void Button5_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            dispatcherTimer.IsEnabled = false;
            Button5.Foreground = new SolidColorBrush(Colors.Black);
            var dir = dirlight.Direction;
            dir.X = 0;
            dirlight.Direction = new Vector3D(dir.X, dir.Y, dir.Z);
        }



        //Face Point processing//////////////////////////////////////
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

        private FeaturePointType rotateFeaturePoint(FeaturePointType fp,float angle)
        {
            FeaturePointType _fp = fp;
            System.Drawing.Drawing2D.Matrix m=new System.Drawing.Drawing2D.Matrix();
            m.RotateAt(angle,new System.Drawing.PointF(0,0));
            System.Drawing.PointF[] pts=new System.Drawing.PointF[]{
                          new System.Drawing.PointF((float)_fp.pt.X,(float)_fp.pt.Y)};
            m.TransformPoints(pts);

            _fp.pt.X = pts[0].X; _fp.pt.Y = pts[0].Y;
            return _fp;
        }

        public string getBestFittingMesh(string filename,bool bIncludeNewModels)
        {
          
            FeaturePointType righteyeNew = new FeaturePointType();
            FeaturePointType lefteyeNew = new FeaturePointType();
            FeaturePointType noseNew = new FeaturePointType();
            FeaturePointType mouthNew = new FeaturePointType();
            FeaturePointType chinNew = new FeaturePointType();


            for (int i = 0; i < _imagefacepoints.Count; i++)
            {
                FeaturePointType fp = new FeaturePointType();
                fp.desp = _imagefacepoints[i].desp;
                fp.pt = _imagefacepoints[i].pt;
                switch (fp.desp)
                {
                    case "RightEye1":
                        righteyeNew = fp;
                        break;
                    case "LeftEye1":
                        lefteyeNew = fp;
                        break;
                    case "Nose1":
                        noseNew = fp;
                        break;
                    case "Mouth3":
                        mouthNew = fp;
                        break;
                    case "Chin1":
                        chinNew = fp;
                        break;

                }
            }


            //do prerotation
            if (_degPreRotate != 0)
            {

                //all point are to be alterted
                righteyeNew = rotateFeaturePoint(righteyeNew, _degPreRotate);
                lefteyeNew = rotateFeaturePoint(lefteyeNew, _degPreRotate);
                noseNew = rotateFeaturePoint(noseNew, _degPreRotate);
                mouthNew = rotateFeaturePoint(mouthNew, _degPreRotate);
                chinNew = rotateFeaturePoint(chinNew, _degPreRotate);


            }


            int eyedistNew = (int)(lefteyeNew.pt.X - righteyeNew.pt.X);


            FeaturePointType righteyeRef = new FeaturePointType();
            FeaturePointType lefteyeRef = new FeaturePointType();
            FeaturePointType noseRef = new FeaturePointType();
            FeaturePointType mouthRef = new FeaturePointType();
            FeaturePointType chinRef = new FeaturePointType();

            string[] meshinfofiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "mesh\\","*.info.txt");
            List<string> listinfo = new List<string>();
            listinfo.AddRange(meshinfofiles);
           
            if (bIncludeNewModels)
            {
                string[] addmeshfiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "newmesh\\", "*.info.txt");

                listinfo.AddRange(addmeshfiles);
            }

            meshinfofiles = listinfo.ToArray();

            List<Tuple<string,string, double>> listerr = new List<Tuple<string,string, double>>();

            foreach(var infofilename in meshinfofiles)
            {
                //string infofilename = AppDomain.CurrentDomain.BaseDirectory + "\\mesh\\mesh" + this.Title + ".info.txt";
                using (var file = File.OpenText(infofilename))
                {
                    string s = file.ReadToEnd();
                    var lines = s.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        var parts = lines[i].Split('=');
                        FeaturePointType fp = new FeaturePointType();
                        fp.desp = parts[0];
                        fp.pt = ExtractPoint(parts[1]);
                        switch (fp.desp)
                        {
                            case "RightEye1":
                                righteyeRef = fp;
                                break;
                            case "LeftEye1":
                                lefteyeRef = fp;
                                break;
                            case "Nose1":
                                noseRef = fp;
                                break;
                            case "Mouth3":
                                mouthRef = fp;
                                break;
                            case "Chin1":
                                chinRef = fp;
                                break;

                        }
                    }
                }

                double x0Ref = (lefteyeRef.pt.X + righteyeRef.pt.X) / 2;
                double y0Ref = (lefteyeRef.pt.Y + righteyeRef.pt.Y) / 2;
                double x0New = (lefteyeNew.pt.X + righteyeNew.pt.X) / 2;
                double y0New = (lefteyeNew.pt.Y + righteyeNew.pt.Y) / 2;

               int eyedistRef = (int)(lefteyeRef.pt.X - righteyeRef.pt.X);
               double noselengthNew = Math.Sqrt((noseNew.pt.X - x0New) * (noseNew.pt.X - x0New) + (noseNew.pt.Y - y0New) * (noseNew.pt.Y - y0New));
               double noselengthRef = Math.Sqrt((noseRef.pt.X - x0Ref) * (noseRef.pt.X - x0Ref) + (noseRef.pt.Y - y0Ref) * (noseRef.pt.Y - y0Ref));

               double ratiox = (double)eyedistRef / (double)eyedistNew;
               double ratioy = noselengthRef / noselengthNew;
               double errFitting = /*Math.Abs*/(ratiox - ratioy) / ratiox;

               ////Alight the mouth//////////
               Point newptNose = new Point(noseNew.pt.X * ratiox, noseNew.pt.Y * ratioy);
               Point newptMouth = new Point(mouthNew.pt.X * ratiox, mouthNew.pt.Y * ratioy);

               double mouthDistRef = mouthRef.pt.Y - noseRef.pt.Y;

               double mouthDistNew = newptMouth.Y - newptNose.Y;//noseNew.pt.Y * ratioy;

               double ratioy2 = mouthDistRef / mouthDistNew;

               double errFitting1 = /*Math.Abs*/(1 - ratioy2);

               ///Align the chin
               Point newptChin = new Point(chinNew.pt.X * ratiox, chinNew.pt.Y * ratioy);
               double chinDistRef = chinRef.pt.Y - mouthRef.pt.Y;

               double chinDistNew = newptChin.Y - newptMouth.Y;//noseNew.pt.Y * ratioy;

               double ratioy3 = chinDistRef / chinDistNew;

               double errFitting2 = /*Math.Abs*/(1 - ratioy3);

               double score = Math.Abs(errFitting)*4+ Math.Abs(errFitting1)*2+ Math.Abs(errFitting2);
               string fittingerr = (int)(errFitting*100)+":"+ (int)(errFitting1*100) +":"+ (int)(errFitting2*100);
               Tuple<string,string,double> tp=new Tuple<string,string,double> (infofilename,fittingerr,score);
               listerr.Add(tp);           
            }
            var sortedlist = listerr.OrderBy(o => o.Item3).ToList();
            string selected=sortedlist[0].Item1;
            var v=selected.Split('\\');
            var v2 = v[v.Length - 1].Split('.');
            string meshname = v2[0].Replace("mesh","");
            return meshname ;
        }



        public string processTextureFitting()
        {
            //not ready yet..
            if (_imagefacepoints == null) return "";

            //find the eyes points for _refbitmap
            var destbmp = new FormatConvertedBitmap();
            destbmp.BeginInit();
            destbmp.DestinationFormat = PixelFormats.Rgb24;
            destbmp.Source = _orgbitmap.Clone(); 
            destbmp.EndInit();

            BitmapEncoder encoder = new BmpBitmapEncoder();

            encoder.Frames.Clear();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)destbmp));

            using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\~ref.bmp", FileMode.Create))
            {
                encoder.Save(fs);
            }


            FeaturePointType righteyeRef = new FeaturePointType();
            FeaturePointType lefteyeRef = new FeaturePointType();
            FeaturePointType noseRef = new FeaturePointType();
            FeaturePointType mouthRef = new FeaturePointType();
            FeaturePointType chinRef = new FeaturePointType();
           

            string infofilename = AppDomain.CurrentDomain.BaseDirectory + "\\mesh\\mesh" + this.Title + ".info.txt";
            if (!IsDefaultMesh(this.Title))
                infofilename = AppDomain.CurrentDomain.BaseDirectory + "\\newmesh\\mesh" + this.Title + ".info.txt";

            using (var file = File.OpenText(infofilename))
            {
                string s = file.ReadToEnd();
                var lines = s.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    var parts = lines[i].Split('=');
                    FeaturePointType fp = new FeaturePointType();
                    fp.desp = parts[0];
                    fp.pt = ExtractPoint(parts[1]);
                    switch (fp.desp)
                    {
                        case "RightEye1":
                            righteyeRef = fp;
                            break;
                        case "LeftEye1":
                            lefteyeRef = fp;
                            break;
                        case "Nose1":
                            noseRef = fp;
                            break;
                        case "Mouth3":
                            mouthRef = fp;
                            break;
                        case "Chin1":
                            chinRef = fp;
                            break;

                    }
                }
            }
            int eyedistRef = (int)(lefteyeRef.pt.X - righteyeRef.pt.X);

            using (var file = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\~ref.bmp.info.txt"))
            {
                file.WriteLine(righteyeRef.desp + "=" + righteyeRef.pt.X + "," + righteyeRef.pt.Y);
                file.WriteLine(lefteyeRef.desp + "=" + lefteyeRef.pt.X + "," + lefteyeRef.pt.Y);
                file.WriteLine(noseRef.desp + "=" + righteyeRef.pt.X + "," + noseRef.pt.Y);
                file.WriteLine(mouthRef.desp + "=" + righteyeRef.pt.X + "," + mouthRef.pt.Y);
                file.WriteLine(chinRef.desp + "=" + chinRef.pt.X + "," + chinRef.pt.Y);
            }

            FeaturePointType righteyeNew = new FeaturePointType();
            FeaturePointType lefteyeNew = new FeaturePointType();
            FeaturePointType noseNew = new FeaturePointType();
            FeaturePointType mouthNew = new FeaturePointType();
            FeaturePointType chinNew = new FeaturePointType();


            for (int i = 0; i < _imagefacepoints.Count; i++)
            {
                FeaturePointType fp = new FeaturePointType();
                fp.desp = _imagefacepoints[i].desp;
                fp.pt = _imagefacepoints[i].pt;
                switch (fp.desp)
                {
                    case "RightEye1":
                        righteyeNew = fp;
                        break;
                    case "LeftEye1":
                        lefteyeNew = fp;
                        break;
                    case "Nose1":
                        noseNew = fp;
                        break;
                    case "Mouth3":
                        mouthNew = fp;
                        break;
                    case "Chin1":
                        chinNew = fp;
                        break;

                }
            }

            //save all these points to file
            var fileparts=winFitting.CurrentFile.Split('\\');
            //using (var file = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\~new.bmp.info.txt"))
            using (var file = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "temp\\" + fileparts[fileparts.Length-1]+winFitting.FaceIndexString+".info.txt"))
            {
                file.WriteLine(righteyeNew.desp + "=" + righteyeNew.pt.X + "," + righteyeNew.pt.Y);
                file.WriteLine(lefteyeNew.desp + "=" + lefteyeNew.pt.X + "," + lefteyeNew.pt.Y);
                file.WriteLine(noseNew.desp + "=" + noseNew.pt.X + "," + noseNew.pt.Y);
                file.WriteLine(mouthNew.desp + "=" + mouthNew.pt.X + "," + mouthNew.pt.Y);
                file.WriteLine(chinNew.desp + "=" + chinNew.pt.X + "," + chinNew.pt.Y);
            }
           


            System.Drawing.Bitmap bm = CCommon.BitmapImage2Bitmap((BitmapImage)RefImage.Source);
            System.Drawing.Bitmap bm2 = CCommon.BitmapImage2Bitmap((BitmapImage)winFitting.Image1.Source);
            if(chkGrid.IsChecked ==true )
            {
                int gridsize = bm2.Height / 20;
                System.Drawing.Graphics gbm2temp = System.Drawing.Graphics.FromImage(bm2);
                List<System.Drawing.Rectangle> rects = new List<System.Drawing.Rectangle>();
                for (int x = 0; x < bm2.Width; x += gridsize)
                    for (int y = 0; y < bm2.Height; y += gridsize)
                    {
                        rects.Add(new System.Drawing.Rectangle(x, y, gridsize, gridsize));
                    }

                var rects_arr = rects.ToArray();
                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Brushes.Pink, (bm2.Width / 400)+1);
                gbm2temp.DrawRectangles(pen, rects_arr);
                gbm2temp.Dispose();

            }

            //do prerotation
            if (_degPreRotate != 0)
            {
                System.Drawing.Bitmap bmtemp1 = (System.Drawing.Bitmap)bm2.Clone();
                System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
                m.RotateAt(_degPreRotate, new System.Drawing.PointF(0, 0));
                System.Drawing.Graphics gbmtemp1 = System.Drawing.Graphics.FromImage(bmtemp1);
                gbmtemp1.Transform = m;
                gbmtemp1.DrawImage(bm2, new System.Drawing.PointF(0, 0));
                bm2.Dispose();
                gbmtemp1.Dispose();
                bm2 = bmtemp1;
                //all point are to be alterted
                righteyeNew = rotateFeaturePoint(righteyeNew, _degPreRotate);
                lefteyeNew = rotateFeaturePoint(lefteyeNew, _degPreRotate);
                noseNew = rotateFeaturePoint(noseNew, _degPreRotate);
                mouthNew = rotateFeaturePoint(mouthNew, _degPreRotate);
                chinNew = rotateFeaturePoint(chinNew, _degPreRotate);
            }

           // this.ColorImage.Source = CCommon.Bitmap2BitmapImage(bm2);

            int eyedistNew = (int)(lefteyeNew.pt.X - righteyeNew.pt.X);

            double x0Ref = (lefteyeRef.pt.X + righteyeRef.pt.X) / 2;
            double y0Ref = (lefteyeRef.pt.Y + righteyeRef.pt.Y) / 2;
            double x0New = (lefteyeNew.pt.X + righteyeNew.pt.X) / 2;
            double y0New = (lefteyeNew.pt.Y + righteyeNew.pt.Y) / 2;

            double noselengthNew = Math.Sqrt((noseNew.pt.X - x0New) * (noseNew.pt.X - x0New) + (noseNew.pt.Y - y0New) * (noseNew.pt.Y - y0New));
            double noselengthRef = Math.Sqrt((noseRef.pt.X - x0Ref) * (noseRef.pt.X - x0Ref) + (noseRef.pt.Y - y0Ref) * (noseRef.pt.Y - y0Ref));

            double ratiox = (double)eyedistRef / (double)eyedistNew;
            double ratioy = noselengthRef / noselengthNew;

            double errFitting = /*Math.Abs*/(ratiox - ratioy) / ratiox;


            int widthNew = (int)(bm2.Width * ratiox);
            int heightNew = (int)(bm2.Height * ratioy);


            //Stretch the new bitmap to match the eye and nose dist for ref
            System.Drawing.Rectangle rectSourceMain = new System.Drawing.Rectangle(0, 0, bm2.Width, bm2.Height);
            System.Drawing.Rectangle rectDestMain = new System.Drawing.Rectangle(0, 0, widthNew, heightNew);
            System.Drawing.Bitmap bmtemp = new System.Drawing.Bitmap(widthNew, heightNew, bm2.PixelFormat);
            System.Drawing.Graphics gbmtemp = System.Drawing.Graphics.FromImage(bmtemp);
            gbmtemp.DrawImage(bm2, rectDestMain, rectSourceMain, System.Drawing.GraphicsUnit.Pixel);
            gbmtemp.Dispose();
            bm2.Dispose();
            bm2 = bmtemp;
            bm2.Save(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\eye_nose_fitted.bmp");
            //Align the bitmap
            //New right eye location with reference to new bitmap
            FeaturePointType newpt = new FeaturePointType();
            newpt.desp = "RightEye1";
            newpt.pt = new Point(ratiox * righteyeNew.pt.X, ratioy * lefteyeNew.pt.Y);
            //Anchor pt. The point for inserting new bitmap to the ref bitmap
            FeaturePointType anchorpt = new FeaturePointType();
            anchorpt.desp = "Anchor";
            anchorpt.pt = new Point(righteyeRef.pt.X - newpt.pt.X, righteyeRef.pt.Y - newpt.pt.Y);


            /////////////////////////////////////////////////////////////////
            //stretching sideways to create more face
            if (checkNoStretch.IsChecked == false)
            {
                System.Drawing.Bitmap bm3 = (System.Drawing.Bitmap)bm2.Clone();
                System.Drawing.Graphics gbm2 = System.Drawing.Graphics.FromImage(bm2);


                //eye to nose
                //int xright = (int)(newpt.pt.X - 0.3 * eyedistNew * ratiox);
                //int xleft = (int)(newpt.pt.X + 1.3 * eyedistNew * ratiox);
                int xright = (int)(newpt.pt.X - 0.1 * eyedistNew * ratiox);
                int xleft = (int)(newpt.pt.X + 1.1 * eyedistNew * ratiox);
                int xwidth = (int)(1.0 * eyedistNew * ratiox);

                int ystart = (int)(newpt.pt.Y + 0.15 * eyedistNew * ratiox);
               // int ystart = (int)(newpt.pt.Y + 0.20 * eyedistNew * ratiox);
                int yend = (int)(noseNew.pt.Y * ratioy);//(int)(newpt.pt.Y + 0.3 * eyedistNew * ratiox);
                int yheight = yend - ystart;

                System.Drawing.Rectangle rectRightSource = new System.Drawing.Rectangle(xright - xwidth, ystart, xwidth, yheight/*bm2.Height*/);
                System.Drawing.Rectangle rectRightDest = new System.Drawing.Rectangle(xright - (int)(2.0 * xwidth) /*0*/, ystart, (int)(2.0 * xwidth)  /*xright*/, yheight/*bm2.Height*/);
                System.Drawing.Rectangle rectLeftSource = new System.Drawing.Rectangle(xleft, ystart, xwidth, yheight/*bm2.Height*/);
                System.Drawing.Rectangle rectLeftDest = new System.Drawing.Rectangle(xleft, ystart, (int)(2.0 * xwidth) /*bm2.Width - xleft*/, yheight/*bm2.Height*/);

                gbm2.DrawImage(bm3, rectRightDest, rectRightSource, System.Drawing.GraphicsUnit.Pixel);
                gbm2.DrawImage(bm3, rectLeftDest, rectLeftSource, System.Drawing.GraphicsUnit.Pixel);

               // ////above eye
                //xright = (int)(newpt.pt.X - 0.50 * eyedistNew * ratiox);
                //xleft = (int)(newpt.pt.X + 1.50 * eyedistNew * ratiox);
                xright = (int)(newpt.pt.X - 0.3 * eyedistNew * ratiox);
                xleft = (int)(newpt.pt.X + 1.3 * eyedistNew * ratiox);
                xwidth = (int)(1.0 * eyedistNew * ratiox);

                ystart = 0;// (int)(newpt.pt.Y + 0.3 * eyedistNew * ratiox);

                yend = (int)(newpt.pt.Y + 0.15 * eyedistNew * ratiox);
                yheight = yend - ystart;

                rectRightSource = new System.Drawing.Rectangle(xright - xwidth, ystart, xwidth, yheight/*bm2.Height*/);
                rectRightDest = new System.Drawing.Rectangle(xright - (int)(2.0 * xwidth) /*0*/, ystart, (int)(2.0 * xwidth)  /*xright*/, yheight/*bm2.Height*/);
                rectLeftSource = new System.Drawing.Rectangle(xleft, ystart, xwidth, yheight/*bm2.Height*/);
                rectLeftDest = new System.Drawing.Rectangle(xleft, ystart, (int)(2.0 * xwidth) /*bm2.Width - xleft*/, yheight/*bm2.Height*/);

                gbm2.DrawImage(bm3, rectRightDest, rectRightSource, System.Drawing.GraphicsUnit.Pixel);
                gbm2.DrawImage(bm3, rectLeftDest, rectLeftSource, System.Drawing.GraphicsUnit.Pixel);
               ///////////////////////////////////
 
                //nose downwards
                //xright = (int)(newpt.pt.X - 0.0 * eyedistNew * ratiox);
                //xleft = (int)(newpt.pt.X + 1.0 * eyedistNew * ratiox);
                xright = (int)(newpt.pt.X - 0.0 * eyedistNew * ratiox);
                xleft = (int)(newpt.pt.X + 1.0 * eyedistNew * ratiox);
                //(int)(0.5 * eyedistNew * ratiox);
                //xwidth = (int)(0.5 * eyedistNew * ratiox);
                xwidth = (int)(1.0 * eyedistNew * ratiox);
                ystart = (int)(noseNew.pt.Y * ratioy);

                yend = bm2.Height;// (int)(newpt.pt.Y + 0.3 * eyedistNew * ratiox);
                yheight = yend - ystart;

                rectRightSource = new System.Drawing.Rectangle(xright - xwidth, ystart, xwidth, yheight/*bm2.Height*/);
                rectRightDest = new System.Drawing.Rectangle(xright - (int)(1.5 * xwidth) /*0*/, ystart, (int)(1.5 * xwidth)  /*xright*/, yheight/*bm2.Height*/);
                rectLeftSource = new System.Drawing.Rectangle(xleft, ystart, xwidth, yheight/*bm2.Height*/);
                rectLeftDest = new System.Drawing.Rectangle(xleft, ystart, (int)(1.5 * xwidth)  /*bm2.Width - xleft*/, yheight/*bm2.Height*/);

                gbm2.DrawImage(bm3, rectRightDest, rectRightSource, System.Drawing.GraphicsUnit.Pixel);
                gbm2.DrawImage(bm3, rectLeftDest, rectLeftSource, System.Drawing.GraphicsUnit.Pixel);

                gbm2.Dispose();

                bm2.Save(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\stretched.bmp");
             
              

            }
            ////////////////////////////


            ////Alight the mouth//////////
            Point newptNose = new Point(noseNew.pt.X * ratiox, noseNew.pt.Y * ratioy);
            Point newptMouth = new Point(mouthNew.pt.X * ratiox, mouthNew.pt.Y * ratioy);

            double mouthDistRef = mouthRef.pt.Y - noseRef.pt.Y;

            double mouthDistNew = newptMouth.Y - newptNose.Y;//noseNew.pt.Y * ratioy;

            double ratioy2 = mouthDistRef / mouthDistNew;

            double errFitting1 = /*Math.Abs*/(1-ratioy2 );

            System.Drawing.Bitmap bm3a = (System.Drawing.Bitmap)bm2.Clone();
            System.Drawing.Graphics gbm2a = System.Drawing.Graphics.FromImage(bm2);

            System.Drawing.Rectangle rectSourceMouth = new System.Drawing.Rectangle(0, (int)newptNose.Y, bm2.Width, bm2.Height - (int)newptNose.Y);
            System.Drawing.Rectangle rectDestMouth = new System.Drawing.Rectangle(0, (int)newptNose.Y, bm2.Width, (int)((bm2.Height - newptNose.Y) * ratioy2));

            gbm2a.DrawImage(bm3a, rectDestMouth, rectSourceMouth, System.Drawing.GraphicsUnit.Pixel);
            gbm2a.Dispose();
            bm3a.Dispose();
            bm2.Save(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\mouth_aligned.bmp");
            //////////////////////////////////////

            ///Align the chin
            Point newptChin = new Point(chinNew.pt.X * ratiox, chinNew.pt.Y * ratioy);
            double chinDistRef = chinRef.pt.Y - mouthRef.pt.Y;

            double chinDistNew = newptChin.Y - newptMouth.Y;//noseNew.pt.Y * ratioy;

            double ratioy3 = chinDistRef / chinDistNew;

            double errFitting2 = /*Math.Abs*/(1-ratioy3);

            System.Drawing.Bitmap bm3b = (System.Drawing.Bitmap)bm2.Clone();
            System.Drawing.Graphics gbm2b = System.Drawing.Graphics.FromImage(bm2);

            System.Drawing.Rectangle rectSourceChin = new System.Drawing.Rectangle(0, (int)newptMouth.Y, bm2.Width, bm2.Height - (int)newptMouth.Y);
            System.Drawing.Rectangle rectDestChin = new System.Drawing.Rectangle(0, (int)newptMouth.Y, bm2.Width, (int)((bm2.Height - newptMouth.Y) * ratioy3));

            gbm2b.DrawImage(bm3b, rectDestChin, rectSourceChin, System.Drawing.GraphicsUnit.Pixel);

            gbm2b.Dispose();
            bm3b.Dispose();
            bm2.Save(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\chin_aligned.bmp");
            ////////////////

            this.ColorImage.Source = CCommon.Bitmap2BitmapImage(bm2);

            //Anchor the new bitmap on the ref bitmap
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bm);
            g.DrawImage(bm2, (float)(anchorpt.pt.X), (float)(anchorpt.pt.Y));


            bm2.Dispose();
            bm2 = null;
            g.Dispose();

            bm.Save(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\new.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            if (_bmcolor != null) _bmcolor = null;

            _bmcolor = (System.Drawing.Bitmap)bm.Clone();

            if (this._refbitmap != null) this._refbitmap = null;
            _refbitmap = CCommon.Bitmap2BitmapImage(bm);
            RefImage.Source = _refbitmap;

            if (this._refbitmap != null) this._colorbitmap = null;
            _colorbitmap = _refbitmap.Clone();

            bm.Dispose();
            //this.theMaterial.Brush = new ImageBrush(this._refbitmap)
            //{

            //    ViewportUnits = BrushMappingMode.Absolute
            //};
            slider_ValueChanged(sliderBrightness, null);

            CroppedBitmap crop=new CroppedBitmap(_refbitmap,new Int32Rect(_meshRect.X,_meshRect.Y,_meshRect.Width,_meshRect.Height));
            System.Drawing.Bitmap bmcrop = CCommon.BitmapImage2Bitmap(crop);
            bmcrop.Save(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\~crop.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

           

            ////////////////////////////////////////
            ////Convert to rgb24
            //destbmp = new FormatConvertedBitmap();
            //destbmp.BeginInit();
            //destbmp.DestinationFormat = PixelFormats.Rgb24;
            //destbmp.Source = crop;
            //destbmp.EndInit();
            //encoder = new BmpBitmapEncoder();
            //encoder.Frames.Clear();
            //encoder.Frames.Add(BitmapFrame.Create((BitmapSource)destbmp));

            //using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\temp\\~crop.bmp", FileMode.Create))
            //{
            //    encoder.Save(fs);
            //}
            ////////////////////////////////////////

            ImageBrush ib = new ImageBrush();
            ib.ImageSource =crop as BitmapSource ;
            ib.Stretch = Stretch.Uniform;

            if (MeshGrid.Background != null)
                MeshGrid.Background = null;

            MeshGrid.Background = ib;

            double score = Math.Abs(errFitting) * 4 + Math.Abs(errFitting1) * 2 + Math.Abs(errFitting2);

            return  (int)Math.Round((errFitting*100))+ ":"+
                (int)Math.Round(errFitting1*100)+":" +
                (int)Math.Round(errFitting2*100) +":" +
                (int)Math.Round(score*100);

        }


        //Show the Window to mark facial points
        private void RefImage_MouseLeftButtonDown1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
               string filename = "";

                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
               
                openFileDialog.FileName = _lastfacefileloaded;
                openFileDialog.Filter = "Picture files|*.bmp;*.jpg;*.png";

                if (openFileDialog.ShowDialog() != true) return;
               
                filename = openFileDialog.FileName;
               

                bool bWinFittingAlreadyActive = false;
                if (winFitting != null && winFitting.IsWinLoaded)
                {
                    //we already have a window to work on.
                    bWinFittingAlreadyActive = true;
                   
                }
                else //create a new window
                {
             
                    winFitting = new Window2();
                }


                //****YKW
                if (!CCommon.LoadImageSource(winFitting.Image1, filename,false)) return;
                //****

                winFitting.CurrentFile = filename;

                var fileparts = winFitting.CurrentFile.Split('\\');
                string facepointsfile=AppDomain.CurrentDomain.BaseDirectory +"temp\\" + fileparts[fileparts.Length -1] +".info.txt";
                if (File.Exists(facepointsfile))
                {
                   
                    //read and update the face points

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
                        ImageFacePoints = facepoints;
                    }
                    winFitting.LoadFacePointsFromCache = true;
                }
                else
                    winFitting.LoadFacePointsFromCache = false;


                if (bWinFittingAlreadyActive)
                {
                 
                     winFitting.LoadFacialPoints();
                }
                    
                winFitting.Owner = this;

                //Loading of winFitting
                try
                {
                    winFitting.Show();


                    if (winFitting.WindowState == WindowState.Minimized)
                        winFitting.WindowState = WindowState.Normal;
                }
                catch
                {

                }

        }

        private bool IsDefaultMesh(string meshid)
        {
            string defaultmeshes = "RoundFace|OvalFace|GenericFace|OvalBroadFace|BroadFace|LongFace";
            if (defaultmeshes.IndexOf(meshid) >= 0)
                return true;
            else
                return false;
        }

        public void LoadFaceMesh(string meshname,bool bPrompt)
        {
           
            if (_lastfacefileloaded != "")
            {
                if (bPrompt)
                {
                    MessageBoxResult msg_result = MessageBox.Show("Load " + meshname + " with last face file?", "Load Mesh", MessageBoxButton.YesNoCancel);
                    if (msg_result == MessageBoxResult.Cancel) return;

                    _bUselastfacefile = msg_result == MessageBoxResult.Yes ? true : false;
                }
                else
                    _bUselastfacefile = true;

            }
            else
            {
                if(bPrompt)
                    if (!(MessageBox.Show("Load " + meshname + " ?", "Load Mesh", MessageBoxButton.OKCancel) == MessageBoxResult.OK))
                        return;
            }

            this.Title = meshname;
         
           //Load meshes from default locations
           if(IsDefaultMesh(meshname))  //built in meshes      
              LoadXFile(AppDomain.CurrentDomain.BaseDirectory + "\\mesh\\mesh" + meshname + ".x");   
            else //for customized meshes
              LoadXFile(AppDomain.CurrentDomain.BaseDirectory + "\\newmesh\\mesh" + meshname + ".x");



            if (_bUselastfacefile)
            {
               
                string fittingerr = processTextureFitting();
                if (winFitting != null && winFitting.IsWinLoaded)
                {
                    winFitting.FittingError = fittingerr;
                }
                if (winViewer != null && winViewer.IsWinLoaded)
                    winViewer.UpdateDisplay();
            }
        }

        //Selection of Face mesh to load
        private void Image_Face_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            string uid = ((Image)sender).Uid;
            List<UIElement> list = GridFModel.Children.Cast<UIElement>().ToList();
            Image img = (Image)list.Single(n => n.Uid == uid);

            string s = (string)img.Tag;// img.Name;
         //   string s = ((Image)sender).Name;
            var v = s.Split('_');
            LoadFaceMesh(v[1],true );

        }

        //for implementation of scrolling the images in VGrid
        private void GridVSnap_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Diagnostics.Debug.Print("Delta=" + e.Delta);
            //-ve up
            //+ve down;

            if (e.Delta < 0)
                _startindex++;//startindex increase
            else
                _startindex--;

            if (_startindex < 0) _startindex = 0;

            updateVGrid(_startindex);

        }

        private void VSnap_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Delete Image?", "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {

                string uid = ((Image)sender).Uid;
                int index = int.Parse(uid.Substring(uid.Length - 1));
                if (_snapimages.Count > 0)
                {
                    _snapimages.RemoveAt(index + _startindex);
                    updateVGrid(_startindex);
                }
            }
        }

        private void VSnap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                string uid = ((Image)sender).Uid;
                List<UIElement> list = GridVSnap.Children.Cast<UIElement>().ToList();
                Image img = (Image)list.Single(s => s.Uid == uid);

                ShowViewer(img,null);

            }
            catch
            {
                MessageBox.Show("Failed to save file.");
            }

                
            
        }


        private void chkImageMesh_Checked(object sender, RoutedEventArgs e)
        {
            ShowMesh();
        }

        private void chkImageMesh_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowMesh();
        }

        private void ShowViewer(UIElement  imgsrc,UIElement  imgbrushsrc)
        {
            if (winViewer != null && winViewer.IsWinLoaded)
            {
                //OK do nothing extra, we already have a window to work on.
                ;
            }
            else //create a new window
            {

                winViewer = new Window1();
            }

            winViewer.Owner = this;
            winViewer.Image1.Source = ((Image)imgsrc).Source;
            winViewer.SourceImage = imgsrc;
            winViewer.SourceBrushImage = imgbrushsrc;
            winViewer.UpdateDisplay();
            winViewer.Title = this.Title + " Viewer";
            winViewer.Show();

            winViewer.WindowState = WindowState.Normal;
            //Bring window to the front
            winViewer.Activate();
            winViewer.Topmost = true;  // important
            winViewer.Topmost = false; // important
            winViewer.Focus();  

        }

        private void ImageMesh_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowViewer(MeshImage, null);
        }

        private void MeshImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowViewer(MeshImage, MeshGrid );
        }

        private void ColorImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowViewer(ColorImage,null );
        }

        private void updateDisplay()
        {

            if (winFitting != null)
            {
                string err = processTextureFitting();// (winFitting.CurrentFile);
                winFitting.FittingError = err;
                if(winViewer!=null && winViewer.IsWinLoaded)
                    winViewer.UpdateDisplay();
            }

        }
        
        private void chkGrid_Checked(object sender, RoutedEventArgs e)
        {
            updateDisplay();
        }

        private void chkGrid_Unchecked(object sender, RoutedEventArgs e)
        {
            updateDisplay();
        }

        private void checkNoStretch_Checked(object sender, RoutedEventArgs e)
        {
            updateDisplay();
        }


        private void checkNoStretch_Unchecked(object sender, RoutedEventArgs e)
        {
            updateDisplay();
        }

        private void ResetMeshTransalationAndRotation()
        {
            RotationString  = "0 0 0";
            vscroll.Value = 0;
            hscroll.Value = 0;
            vscrollz.Value = 0;
            TranslationString = _startmeshTranslationString;
            var v=_startmeshTranslationString.Split (' ');
            sliderx.Value =double.Parse(v[0]);
            slidery.Value =double.Parse(v[1]);
            sliderz.Value =double.Parse(v[2]);
        }

        private void viewport3d_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.Print("Viewport Mousedown click=" +e.ClickCount );
            if(e.ClickCount >=2)
            {

                ResetMeshTransalationAndRotation();

            }
        }


        private void LoadOrginalCube()
        {
            _bCubeMeshLoaded = false;
            theMaterial.Brush = _startupbrush;

            theGeometry.TriangleIndices = _startupgeometry.TriangleIndices;
            theGeometry.Positions = _startupgeometry.Positions.Clone();

            _orgmeshpos = _startupgeometry.Positions;
            theGeometry.TextureCoordinates = _startupgeometry.TextureCoordinates;

            //Update the mesh
            Button1_Click(null, null);


        }

       
        
        private void viewport3d_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (bIsXLoaded)
            {

                ContextMenu cm = this.FindResource("cmFView") as ContextMenu;
                foreach(var item in cm.Items)
                {
                   

                    if(item.GetType() ==typeof(MenuItem ))
                    {
                        ((MenuItem)item).IsChecked = false;

                        if (_iMapping == 0 && ((MenuItem)item).Name == "miNoMap")
                            ((MenuItem)item).IsChecked = true;
                        if (_iMapping == 1 && ((MenuItem)item).Name == "miMapLeft")
                            ((MenuItem)item).IsChecked = true;
                        if (_iMapping == 2 && ((MenuItem)item).Name == "miMapRight")
                            ((MenuItem)item).IsChecked = true;
                    }
                }

                cm.PlacementTarget = sender as Image;
                cm.IsOpen = true;
                return;

            }

            if (!_bCubeMeshLoaded)
                LoadCubeMesh(AppDomain.CurrentDomain.BaseDirectory + "cubeimage.png");
            else
                LoadOrginalCube();


        }

        //Reset Button
        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            ResetMeshTransalationAndRotation();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if(WindowState==WindowState.Minimized )
            {
                if (winFitting != null)
                {
                    winFitting.WindowState = WindowState.Minimized;

                    winFitting.Magnifier.WindowState = WindowState.Minimized;
                }

            }
            else
            {
                if (winFitting != null)
                {
                    winFitting.WindowState = WindowState.Normal;

                    winFitting.Magnifier.WindowState = WindowState.Normal;
                }

            }
        }


        //For GIF animation and creation
        private void Button7_Click(object sender, RoutedEventArgs e)
        {

            hscroll.Value = 0;
         
            var v = _startmeshTranslationString.Split(' ');
            sliderx.Value = double.Parse(v[0]);
            slidery.Value = double.Parse(v[1]);
            sliderz.Value = double.Parse(v[2]);

            Window5 winGIF = new Window5();
            winGIF.Owner = this;
            bSyncLock = true;
            winGIF.ShowDialog();
            winGIF.Close();
            winGIF = null;
            bSyncLock = false;
        }

        private void GridFModel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Diagnostics.Debug.Print("Delta=" + e.Delta);
            //-ve up
            //+ve down;

            if (e.Delta < 0)
                _startindexfm++;//startindex increase
            else
                _startindexfm--;

            if (_startindexfm < 0) _startindexfm = 0;

            UpdateFMGrid(_startindexfm);
        }

        private int getARGBOfTextureFile(string basemodelid)
        {
            //texture file name
            var texturefile = AppDomain.CurrentDomain.BaseDirectory + "mesh\\" + "mesh" + basemodelid + ".png";
            using(var bm=new System.Drawing.Bitmap(texturefile))
            {
               System.Drawing.Color basecolor= bm.GetPixel(10, 10);
               return basecolor.ToArgb();
            }

        }


        private bool EditFaceModel(string filename , ref bool bNewCreated)
        {
            FaceModelTransformType ft = CCommon.readFaceModelTransformFromFile(filename);
            Window6 winEditMesh = new Window6();
            winEditMesh.Owner = this;
            bSyncLock = true;
            winEditMesh.BaseModelID = ft.basemodelid;
            winEditMesh.BaseColorARGB = getARGBOfTextureFile(ft.basemodelid);
            winEditMesh.FaceModelTransform = ft;
            if (MeshGrid.Background != null)
                if(MeshGrid.Background.GetType()==typeof(ImageBrush)  )
                      winEditMesh.ImageBrush = (ImageBrush)MeshGrid.Background;

            winEditMesh.ShowDialog();
            bool dr = (bool)winEditMesh.DialogResult;

            if (winEditMesh.FaceModelTransform.basemodelid == null)
            {
                bNewCreated = true;
                using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(AppDomain.CurrentDomain.BaseDirectory +
                "\\newmesh\\mesh" + ft.basemodelid + winEditMesh.FaceModelCreatedIndex + ".jpg"))
                {
                    BitmapImage bmi = CCommon.Bitmap2BitmapImage(bm);
                    Image img = new Image();
                    img.Name = "Image_" + ft.basemodelid + winEditMesh.FaceModelCreatedIndex;
                    img.Tag = "Image_" + ft.basemodelid + winEditMesh.FaceModelCreatedIndex;
                    img.Source = bmi;
                    _fmimages.Add(img);
                }

                UpdateFMGrid(-1);
                string meshid = ft.basemodelid + winEditMesh.FaceModelCreatedIndex;
                //this.Title =meshid ;
                LoadFaceMesh(meshid, true);
            }
            winEditMesh.Close();
            winEditMesh = null;
            bSyncLock = false;

            return dr;

        }

        private int AddNewFaceModel(string basemodelid)
        {
           
            Window6 winAddMesh = new Window6();
            winAddMesh.Owner = this;
            bSyncLock = true;
            winAddMesh.BaseModelID = basemodelid;
            winAddMesh.BaseColorARGB = getARGBOfTextureFile(basemodelid);
            if (MeshGrid.Background!=null)
                if (MeshGrid.Background.GetType() == typeof(ImageBrush))
                    winAddMesh.ImageBrush = (ImageBrush)MeshGrid.Background;

            winAddMesh.ShowDialog();

            if(winAddMesh.DialogResult ==true)
            {
                using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(AppDomain.CurrentDomain.BaseDirectory +
                "\\newmesh\\mesh" + basemodelid + winAddMesh.FaceModelCreatedIndex + ".jpg"))
                {
                    BitmapImage bmi = CCommon.Bitmap2BitmapImage(bm);
                    Image img = new Image();
                    img.Name = "Image_" + basemodelid + winAddMesh.FaceModelCreatedIndex;
                    img.Tag = "Image_" + basemodelid + winAddMesh.FaceModelCreatedIndex;
                    img.Source = bmi;
                    _fmimages.Add(img);
                }

                UpdateFMGrid(-1);  
            }

            winAddMesh.Close();
            int intRet= winAddMesh.FaceIndexAssigned;
            winAddMesh = null;
            bSyncLock = false;
            return intRet;

        }

        private void Image_Face_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            string tag = (string)((Image)sender).Tag;
            var tagparts = tag.Split('_');
            var meshid = tagparts[1];
            if(IsDefaultMesh(meshid))
            {
                if(MessageBox.Show("Add New "+meshid +"Model?","Add New Face Model?",MessageBoxButton.OKCancel )==MessageBoxResult.OK)
                {
                  
                    int faceindex=AddNewFaceModel(meshid);
                    if(faceindex >0) LoadFaceMesh(meshid+faceindex,true);
                }
            }
            else
            {

                ContextMenu cm = this.FindResource("cmFModel") as ContextMenu;
                
                cm.PlacementTarget = sender as Image;
                cm.IsOpen = true;


            }
        }

        private void miEdit_Click(object sender, RoutedEventArgs e)
        {

            Image img = (Image)((ContextMenu)((sender as MenuItem).Parent)).PlacementTarget;
            //  string tag = (string)((Image)sender).Tag;
            string tag = (string)(img.Tag);
            var tagparts = tag.Split('_');
            var meshid = tagparts[1];
            string filename = AppDomain.CurrentDomain.BaseDirectory + "newmesh\\mesh" + meshid + ".config.txt";
            bool bCreateNew = false;
            if (EditFaceModel(filename,ref bCreateNew ))
            {
                if(!bCreateNew )
                    using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(AppDomain.CurrentDomain.BaseDirectory +
                        "\\newmesh\\mesh" + meshid + ".jpg"))
                    {
                        BitmapImage bmi = CCommon.Bitmap2BitmapImage(bm);
                        Image img1 = new Image();
                        img1.Name = "Image_" + meshid;
                        img1.Tag = "Image_" + meshid;
                        img1.Source = bmi;
                        //find and replace the img
                        string uid = img.Uid;//((Image)sender).Uid;
                        int index = int.Parse(uid.Substring(uid.Length - 1));
                        if (_fmimages.Count > 0)
                        {
                           // _fmimages.RemoveAt(index + _startindexfm);
                            _fmimages[index + _startindexfm] = img1;
                            img = null;
                            UpdateFMGrid(_startindexfm);
                        }

                        if (this.Title == meshid) LoadFaceMesh(meshid, false);              
                    }

                   // if (this.Title == meshid) LoadFaceMesh(meshid, false);
            }
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
   
           Image img= (Image)((ContextMenu)((sender as MenuItem).Parent)).PlacementTarget;
          //  string tag = (string)((Image)sender).Tag;
            string tag=(string)(img.Tag );
            var tagparts = tag.Split('_');
            var meshid = tagparts[1];

            MessageBoxResult msgResult;
            if (meshid == this.Title)
            {
                msgResult=MessageBox.Show(meshid + " currently in use.\nIf you delete this mesh, GenericFace would be use instead.\nDelete anyway?",
                      "Delete Face Model?",
                      MessageBoxButton.OKCancel,
                      MessageBoxImage.None,
                      MessageBoxResult.Cancel);
               // return;
            }
            else
               msgResult = MessageBox.Show("Delete Face Model " + meshid + "?", "Delete Face Model?", MessageBoxButton.OKCancel);
            if (msgResult == MessageBoxResult.OK)
            {
                //do the delete
                //delete .x
                string meshfile = AppDomain.CurrentDomain.BaseDirectory + "newmesh\\mesh" + meshid + ".x";
                string infofile = AppDomain.CurrentDomain.BaseDirectory + "newmesh\\mesh" + meshid + ".info.txt";
                string billboardfile = AppDomain.CurrentDomain.BaseDirectory + "newmesh\\mesh" + meshid + ".jpg";
                string texturefile = AppDomain.CurrentDomain.BaseDirectory + "newmesh\\mesh" + meshid + ".png";


                string uid = img.Uid;//((Image)sender).Uid;
                int index = int.Parse(uid.Substring(uid.Length - 1));
                if (_fmimages.Count > 0)
                {
                    _fmimages.RemoveAt(index + _startindexfm);
                    UpdateFMGrid(_startindexfm);
                }

                try
                {
                    File.Delete(meshfile);
                    File.Delete(infofile);
                    File.Delete(texturefile);
                    File.Delete(billboardfile);

                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }

                if (meshid == this.Title) //
                    LoadFaceMesh("GenericFace", false);
            }

        }

        private void MeshImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            string filename = AppDomain.CurrentDomain.BaseDirectory + "newmesh\\mesh" + this.Title  + ".config.txt";

            if (IsDefaultMesh(this.Title))
            {
                if (MessageBox.Show("Add New " + this.Title + "?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    int faceindex=AddNewFaceModel(this.Title);
                    if (faceindex > 0)
                        LoadFaceMesh(this.Title + faceindex,true);

                }
                else
                    return;
            }
            else
            {
                if (MessageBox.Show("Edit " + this.Title + "?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    bool bCreateNew = false;
                    EditFaceModel(filename,ref bCreateNew );
                    if (!bCreateNew)
                        LoadFaceMesh(this.Title, false);

                }
                else
                    return;

              //updateDisplay();
              //  LoadFaceMesh(this.Title,false );
            
            }

        }

        private void m_iMapping_Click(object sender, RoutedEventArgs e)
        {
            string itemname=((MenuItem)sender).Name;
           
            switch (itemname )
            {
                case "miMapLeft": 
                    _iMapping = 1; 
                    break;

                case "miMapRight": 
                    _iMapping = 2; break;

                case "miNoMap": 
                    _iMapping = 0; break;

            }

            MapTexture (_iMapping);
           
        }



    }
}