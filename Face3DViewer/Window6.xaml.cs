
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
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;


namespace ThreeDFaces
{
    /// <summary>
    /// Interaction logic for Window6.xaml
    /// </summary>

    public partial class Window6 : Window
    {
        private List<FacePointMapping> _Mappings;
        private Int32Collection _TriangleIndices;
        private Point3DCollection _Positions;
        private PointCollection _TextureCoordinates;
        private Point3DCollection _orgmeshpos;
        private System.Drawing.Color  _MeshColor;


        public ImageBrush ImageBrush
        {
            get;
            set;
        }

        public FaceModelTransformType FaceModelTransform
        {
            get;
            set;
        }

        public int FaceIndexAssigned
        {
            get;
            private set;
        }

        public int FaceModelCreatedIndex
        {
            get;
            private set;
        }

        public int BaseColorARGB
        {
            get;
            set;
        }

        public string BaseModelID
        {
            get;
            set;
        }

        public string BaseXFile
        {
            get;
            set;
        }

        public Window6()
        {
            InitializeComponent();
        }


        private void Load_PositionsFromOrgMeshPos()
        {
            _Positions = _orgmeshpos.Clone();
        }

        private void GenerateTextureCordinatesFromPos()
        {
            var templist = _Positions.OrderBy(x => x.X).ToList();
            double minx = templist[0].X, maxx = templist[templist.Count - 1].X;
            templist = _Positions.OrderBy(y => y.Y).ToList();
            double miny = templist[0].Y, maxy = templist[templist.Count - 1].Y;
            templist=_Positions.OrderBy(z=>z.Z).ToList();
            double _minz = templist[0].Z, _maxz = templist[templist.Count - 1].Z;

            double zrange=_maxz-_minz;
            double zmean = (_minz + _minz) / 2;
            double xrange = maxx - minx;
            double yrange = maxy - miny;

            double xyratio = (double)1920 / 1080;

            //keep the face width to 18% of the texture image
            double xratio = 0.18 / xrange;
            double yratio = xratio * xyratio;

            //offset to center of texture image
            double offsetx = 0.5 - ((minx + maxx) / 2) * xratio;
            double offsety = 0.5 + ((miny + maxy) / 2) * yratio;

            _TextureCoordinates = new PointCollection();

            //double MAX_MULTIPLIER_X = 0.055;
            //double MAX_MULTIPLIER_Y = 0.055;
            double CAM_Z_POS = 0.5;
      

            foreach (var newpoint in _Positions)
            {

                //the nearer to the camera, the greater the multiplier

                ////X multiplier
                //double zdeltaX = MAX_MULTIPLIER_X * (((newpoint.Z - _minz)) / (zrange));
                //double zratioX = 1 + zdeltaX;//1 + zdeltaX;

                ////Y multiplier
                //double zdeltaY = MAX_MULTIPLIER_Y * (((newpoint.Z - _minz)) / (zrange));
                //double zratioY = 1 + zdeltaY;//1 + zdeltaY;

                //Another way of looking at this..
               // double magnifier = (CAM_Z_POS - _minz) /  (CAM_Z_POS - newpoint.Z);

               // double magnifier = (0.5-MEAN_X_FROM_CAM) / (CAM_Z_POS - newpoint.Z -MEAN_X_FROM_CAM);

                double magnifierX = (CAM_Z_POS +0.1) / (CAM_Z_POS - newpoint.Z);
                double magnifierY = (CAM_Z_POS +0.1) / (CAM_Z_POS - newpoint.Z);
                double zratioX = magnifierX;
                double zratioY = magnifierY;
 
                double x = zratioX*newpoint.X * xratio + offsetx;

                //Y direction in World Coordinates is reverse of Texture Coordinate
                double y =  - newpoint.Y * yratio *zratioY + offsety;

                _TextureCoordinates.Add(new Point(x, y));

            }
        }




        private void RenderMesh()
        {
            if (_TextureCoordinates == null) return;

            double minx = 1.0, maxx = 0, miny = 1, maxy = 0;

            for (int i = 0; i < _TextureCoordinates.Count; i++)
            {
                if (_TextureCoordinates[i].X < minx) minx = _TextureCoordinates[i].X;
                if (_TextureCoordinates[i].Y < miny) miny = _TextureCoordinates[i].Y;
                if (_TextureCoordinates[i].X > maxx) maxx = _TextureCoordinates[i].X;
                if (_TextureCoordinates[i].Y > maxy) maxy = _TextureCoordinates[i].Y;
            }

            //2. normalize
            int width = (int)((maxx - minx) * 1920) + 1;
            int height = (int)((maxy - miny) * 1080) + 1;
            List<System.Drawing.PointF> meshpoints = new List<System.Drawing.PointF>();
            for (int i = 0; i < _TextureCoordinates.Count; i++)
            {
                meshpoints.Add(new System.Drawing.PointF((float)(_TextureCoordinates[i].X - minx) * 1920,
                                                         (float)(_TextureCoordinates[i].Y - miny) * 1080)
                               );
            }

            ////The bitmap must have ARGB Pixel format to support transparency
            System.Drawing.Bitmap bm = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ///////////////////////////////////////////////////////

            System.Drawing.Graphics gbm = System.Drawing.Graphics.FromImage(bm);
            gbm.Clear(System.Drawing.Color.Transparent);


            //Draw the points
            System.Drawing.Brush  brush;
            for (int i = 0; i < _TextureCoordinates.Count; i++)
            {
                FacePointMapping mapping= _Mappings.Find(p => p.index==i);
                brush = (mapping.side == "R") ? System.Drawing.Brushes.Red : System.Drawing.Brushes.Blue;
                if (mapping.side =="C")
                    brush =System.Drawing.Brushes.Cyan;

                gbm.FillRectangle(brush, new System.Drawing.RectangleF(
                                                       meshpoints[i].X,
                                                       meshpoints[i].Y,
                                                       3, 3));
             
            }


         
           if((bool)chkMesh.IsChecked )
            for (int i = 0; i < _TriangleIndices.Count; i = i + 3)
            {

                System.Drawing.Pen pen = new System.Drawing.Pen(_MeshColor );
                System.Drawing.Pen pen2 = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, System.Drawing.Color.YellowGreen ));
               
                //Use different pen for these triangles connected to these points
                //10 (center of base of upper lip)
                if (_TriangleIndices[i] == 10 || _TriangleIndices[i + 1] == 10 || _TriangleIndices[i + 2] == 10)
                    pen = pen2;

                //14 (nose tip)
                if (_TriangleIndices[i] == 14 || _TriangleIndices[i + 1] == 14 || _TriangleIndices[i + 2] == 14)
                    pen = pen2;
                //0 (chin)
                if (_TriangleIndices[i] == 0 || _TriangleIndices[i + 1] == 0 || _TriangleIndices[i + 2] == 0)
                    pen = pen2;
                //328-1105 (right eye is between this 2 points)
                if (_TriangleIndices[i] == 1105 || _TriangleIndices[i + 1] == 1105 || _TriangleIndices[i + 2] == 1105)
                    pen = pen2;
                //883-1092 (left eye is between these 2 points)
                if (_TriangleIndices[i] == 1092 || _TriangleIndices[i + 1] == 1092 || _TriangleIndices[i + 2] == 1092)
                    pen = pen2;

                if (CCommon.ArePointsClockwise(
                       meshpoints[_TriangleIndices[i]],
                       meshpoints[_TriangleIndices[i + 1]],
                       meshpoints[_TriangleIndices[i + 2]])
                    )
                {

                    gbm.DrawPolygon(pen, new System.Drawing.PointF[]
                             {
                               meshpoints[_TriangleIndices[i]],
                               meshpoints[_TriangleIndices[i+1]],
                               meshpoints[_TriangleIndices[i+2]]
                             }
                      );
                }


            }
            gbm.Dispose();

            Image1.Source = CCommon.Bitmap2BitmapImage(bm);
            bm.Dispose();
            bm = null;

        }

        private void LoadMesh()
        {

            using (TextReader tr = File.OpenText(System.AppDomain.CurrentDomain.BaseDirectory + "//meshmapping.txt"))
            {
                string s = tr.ReadToEnd();
                var lines = s.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                _Mappings = new List<FacePointMapping>();
                var n = lines.Length;
                for(int i=0;i<n ;i++)
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


            using (TextReader tr = File.OpenText(System.AppDomain.CurrentDomain.BaseDirectory + "//tri_index.txt"))
            {
                string s = tr.ReadToEnd();
                var lines = s.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                _TriangleIndices = new Int32Collection();
                var n = lines.Length;
                for (int i = 0; i < n; i++)
                {
                    _TriangleIndices.Add(int.Parse(lines[i]));
                }
            }


            using (TextReader tr = File.OpenText(BaseXFile))
            {
                string s = tr.ReadToEnd();
                _Positions = new Point3DCollection();
                _TextureCoordinates = new PointCollection();
                _orgmeshpos = new Point3DCollection();
                var lines = s.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);


                int n = lines.Length;

                if (lines.Length > 1347)
                    n = lines.Length / 2;

                for (int i = 0; i < n; i++)
                {

                    var c = lines[i].Split(':');

                    var vertice = new Point3D(double.Parse(c[0]), double.Parse(c[1]), double.Parse(c[2]));

                    _Positions.Add(vertice);
                    _orgmeshpos.Add(vertice);

                }

               //one time test code 
               //create meshmapping
               // CreateMeshMapping();

                NormalizePos();

                GenerateTextureCordinatesFromPos();

            }
        }

        private void CreateMeshMapping()
        {
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory +"\\meshmapping.txt"))
                File.Delete(AppDomain.CurrentDomain.BaseDirectory +"\\meshmapping.txt");

          using (var tw=File.CreateText(AppDomain.CurrentDomain.BaseDirectory +"\\meshmapping.txt"))
          {
              double episilonY = 0.00005;
              double episilonX = 0.00005;
              for (int i = 0; i < _orgmeshpos.Count; i++)
              {
                  double ypos = _orgmeshpos[i].Y;
                  double xpos = _orgmeshpos[i].X;
                  double cxpos = -xpos;
          
                  int ifound = i;
                  for (int j = 0; j < _orgmeshpos.Count; j++)
                  {
                      if (i != j)
                      {
                          if(Math.Abs(_orgmeshpos[j].Y -ypos )<=episilonY)
                              if (Math.Abs(_orgmeshpos[j].X - cxpos) <= episilonX)
                              {
                              
                                  ifound = j;
                                  break;
                              }
                      }
                  }

                  string side = xpos > episilonX ? "L" : "R";

                  tw.WriteLine("{0}:{1}={2}",side, i, ifound);

              }
          }

        }


        private void NormalizePos()
        {


            var templist = _Positions.OrderBy(z => z.Z).ToList();
            double _minz = templist[0].Z, _maxz = templist[templist.Count - 1].Z;
            double zmean=(_minz + _maxz )/2;


            for(int i=0;i<_Positions.Count;i++)
            {
                _Positions[i] = new Point3D(_Positions[i].X , _Positions[i].Y, _Positions[i].Z-zmean );
                _orgmeshpos[i] = new Point3D(_Positions[i].X, _Positions[i].Y, _Positions[i].Z);
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

         
            this.Title = "Create New " + BaseModelID + " Model";
            BaseXFile = AppDomain.CurrentDomain.BaseDirectory + "\\mesh\\mesh" + BaseModelID + ".x";
            LoadMesh();
            if (FaceModelTransform.basemodelid != null)
            {

                TransformMesh();
                this.Title = "Edit " + FaceModelTransform.basemodelid + FaceModelTransform.index;
  
                    
            }
           // else if (BaseModelID!="GenericFace")
           // {
           //    //sliderx.Value = 2.7;
           //    //slidery.Value = 3.2;

           ////     sliderz.Value = 0.40;
           //    // sliderStretchX.Value = -2;
           //     //sliderx.Value = 5.3;
           //     //slidery.Value = 3.7;
           //   //  sliderz.Value = 0.13;
           //   //  sliderStretchX.Value = -2;
           // }
           // else

                if (ImageBrush != null && (chkImage.IsChecked==true))
                    theGrid.Background = ImageBrush;

                _MeshColor = 
                     System.Drawing.Color.FromArgb(50, System.Drawing.Color.Purple );

                RenderMesh();




        }

        private void TransformMesh()
        {
            if (FaceModelTransform.basemodelid!=null)
            {
                sliderx.Value = FaceModelTransform.rotateX;
                slidery.Value = FaceModelTransform.rotateY;
                sliderz.Value = FaceModelTransform.rotateZ;
                sliderStretchX.Value = FaceModelTransform.stretchX;
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblStatus.Content = string.Format("X={0},Y={1},Z={2},StretchX={3}", 
                sliderx.Value.ToString("N"),
                slidery.Value.ToString("N"),
                sliderz.Value.ToString("N"),
                sliderStretchX.Value.ToString("N"));

            Matrix3D m3dx = new Matrix3D();
            double angle = sliderx.Value;
            double ax = 1.0, ay = 0, az = 0;
            double qw = Math.Cos(angle * Math.PI / 180);
            double qx = ax * Math.Sin(angle * Math.PI / 180);
            double qy = ay * Math.Sin(angle * Math.PI / 180);
            double qz = az * Math.Sin(angle * Math.PI / 180);
            Quaternion q = new Quaternion(qx, qy, qz, qw);
            m3dx.Rotate(q);


            Matrix3D m3dy = new Matrix3D();
            angle = slidery.Value;
            ax = 0; ay = 1.0; az = 0;
            qw = Math.Cos(angle * Math.PI / 180);
            qx = ax * Math.Sin(angle * Math.PI / 180);
            qy = ay * Math.Sin(angle * Math.PI / 180);
            qz = az * Math.Sin(angle * Math.PI / 180);
            q = new Quaternion(qx, qy, qz, qw);
            m3dy.Rotate(q);

            Matrix3D m3dz = new Matrix3D();
            angle = sliderz.Value;
            ax = 0; ay = 0; az = 1.0;
            qw = Math.Cos(angle * Math.PI / 180);
            qx = ax * Math.Sin(angle * Math.PI / 180);
            qy = ay * Math.Sin(angle * Math.PI / 180);
            qz = az * Math.Sin(angle * Math.PI / 180);
            q = new Quaternion(qx, qy, qz, qw);
            m3dz.Rotate(q);

            Matrix3D m3d = new Matrix3D();
            m3d.Append(m3dx);
            m3d.Append(m3dy);
            m3d.Append(m3dz);

            m3d.Scale(new Vector3D(1 + sliderStretchX.Value / 100, 1, 1));
        //    m3d.Scale(new Vector3D(1,1 + sliderStretchY.Value / 100, 1));



            ////////////////////////////////
            Load_PositionsFromOrgMeshPos();
            for (int i = 0; i < _Positions.Count; i++)
            {

                var vertice = _Positions[i];
                vertice = m3d.Transform(vertice);
                _Positions[i] = vertice;

            }

            GenerateTextureCordinatesFromPos();
            RenderMesh();

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //newmesh directory
            string newmeshdir = AppDomain.CurrentDomain.BaseDirectory + "newmesh\\";
            string meshfilename = "";
            string texturefilename = "";
            string infofilename = "";
            string billboardfilename = "";
            string configfilename = "";

            //find the index and next name for the meshid
            int index = 0;
            bool bfound = false;

            //bool to load new face model

            try
            {
                if (FaceModelTransform.basemodelid != null)
                {
                    if (MessageBox.Show("Save Edited face model as New face model?", "",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.None,
                        MessageBoxResult.No) != MessageBoxResult.Yes)
                    {
                        bfound = true;
                        index = FaceModelTransform.index;
                    }
                    else
                        FaceModelTransform = new FaceModelTransformType();

                }

                while (!bfound)
                {
                    index++;
                    string testfile = newmeshdir + "mesh" + BaseModelID + index;
                   // meshfilename = testfile + ".x";
                    //texturefilename = testfile + ".png";
                    //billboardfilename = testfile + ".jpg";
                    //infofilename = testfile + ".info.txt";
                    //configfilename = testfile + ".config.txt";
                    if (!File.Exists(testfile + ".x")) bfound = true;

                }

                FaceModelCreatedIndex = index;
                string basefilename = newmeshdir + "mesh" + BaseModelID + index;
                meshfilename = basefilename + ".x";
                texturefilename = basefilename + ".png";
                billboardfilename = basefilename + ".jpg";
                infofilename = basefilename + ".info.txt";
                configfilename = basefilename + ".config.txt";

                //create the texture file
                var bm = new System.Drawing.Bitmap(1920, 1080, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var gbm = System.Drawing.Graphics.FromImage(bm);
                gbm.Clear(System.Drawing.Color.FromArgb(BaseColorARGB));
                for (int i = 0; i < _TriangleIndices.Count; i = i + 3)
                {

                    System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(255, System.Drawing.Color.Red));

                    //Use different pen for these triangles connected to these points
                    //10 (center of base of upper lip)
                    if (_TriangleIndices[i] == 10 || _TriangleIndices[i + 1] == 10 || _TriangleIndices[i + 2] == 10)
                        pen = System.Drawing.Pens.Green;

                    //14 (nose tip)
                    if (_TriangleIndices[i] == 14 || _TriangleIndices[i + 1] == 14 || _TriangleIndices[i + 2] == 14)
                        pen = System.Drawing.Pens.Green;
                    //0 (chin)
                    if (_TriangleIndices[i] == 0 || _TriangleIndices[i + 1] == 0 || _TriangleIndices[i + 2] == 0)
                        pen = System.Drawing.Pens.Green;
                    //328-1105 (right eye is between this 2 points)
                    if (_TriangleIndices[i] == 1105 || _TriangleIndices[i + 1] == 1105 || _TriangleIndices[i + 2] == 1105)
                        pen = System.Drawing.Pens.Green;
                    //883-1092 (left eye is between these 2 points)
                    if (_TriangleIndices[i] == 1092 || _TriangleIndices[i + 1] == 1092 || _TriangleIndices[i + 2] == 1092)
                        pen = System.Drawing.Pens.Green;

                    var meshpoint0 = new System.Drawing.PointF((float)_TextureCoordinates[_TriangleIndices[i]].X * (1920 - 1),
                                                             (float)_TextureCoordinates[_TriangleIndices[i]].Y * (1080 - 1));
                    var meshpoint1 = new System.Drawing.PointF((float)_TextureCoordinates[_TriangleIndices[i + 1]].X * (1920 - 1),
                                                            (float)_TextureCoordinates[_TriangleIndices[i + 1]].Y * (1080 - 1));
                    var meshpoint2 = new System.Drawing.PointF((float)_TextureCoordinates[_TriangleIndices[i + 2]].X * (1920 - 1),
                                                            (float)_TextureCoordinates[_TriangleIndices[i + 2]].Y * (1080 - 1));
                    gbm.DrawPolygon(pen, new System.Drawing.PointF[]
                     {
                       meshpoint0,meshpoint1,meshpoint2 
                     }
                    );

                }
                gbm.Dispose();
                bm.Save(texturefilename, System.Drawing.Imaging.ImageFormat.Png);
                bm.Dispose();
                bm = null;



                //create the billboard jpg from the display


                chkMesh.IsChecked = true;
                BitmapSource imgsrc = Image1.Source as BitmapSource;
                System.Drawing.Bitmap bm1 = CCommon.BitmapImage2Bitmap(imgsrc);
                System.Drawing.Bitmap bm2 = new System.Drawing.Bitmap(bm1.Width + 80, bm1.Height + 150, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                System.Drawing.Graphics gbm2 = System.Drawing.Graphics.FromImage(bm2);
                gbm2.Clear(System.Drawing.Color.FromArgb(BaseColorARGB));
                gbm2.DrawImage(bm1, 40, 150);

                var sf = new System.Drawing.StringFormat();
                sf.Alignment = System.Drawing.StringAlignment.Center;

                string str = BaseModelID.Substring(0,BaseModelID.Length-4) + "\n" + index;

                gbm2.DrawString(str,
                                new System.Drawing.Font("Arial", 50, System.Drawing.FontStyle.Bold), System.Drawing.Brushes.Black,
                                new System.Drawing.RectangleF(20, 10, bm1.Width + 40, 140), sf
                                );
                gbm2.Dispose();

                bm2.Save(billboardfilename, System.Drawing.Imaging.ImageFormat.Jpeg);

                bm1.Dispose();
                bm1 = null;

                bm2.Dispose();
                bm2 = null;

                //the xfile
                using (TextWriter tw = File.CreateText(meshfilename))
                {
                    foreach (var vertice in _Positions)

                        tw.WriteLine("{0}:{1}:{2}", vertice.X, vertice.Y, vertice.Z);

                    for( int i=0;i<_TextureCoordinates.Count;i++)
                    {
                        //if (hidden_indices.IndexOf(i) < 0)
                        //{
                        //    tw.WriteLine("{0}:{1}", _TextureCoordinates[i].X, _TextureCoordinates[i].Y);
                        //}
                        //else
                        //{
                        //    tw.WriteLine("{0}:{1}", 0, 0);
                        //}
                        tw.WriteLine("{0}:{1}", _TextureCoordinates[i].X, _TextureCoordinates[i].Y);   
                    }

                }

                //the config file
                if (File.Exists(configfilename)) File.Delete(configfilename);
                using (TextWriter tw = File.CreateText(configfilename))
                 {
                     tw.WriteLine("basemodelid={0}",BaseModelID);
                     tw.WriteLine("index={0}",index);
                     tw.WriteLine("rotateX={0}",sliderx.Value);
                     tw.WriteLine("rotateY={0}",slidery.Value);
                     tw.WriteLine("rotateZ={0}",sliderz.Value);
                     tw.WriteLine("stretchX={0}", sliderStretchX.Value);
                 }

                //the info.txt file
                if (File.Exists(infofilename)) File.Delete(infofilename);
                using (TextWriter tw = File.CreateText(infofilename))
                {
                    tw.WriteLine("RightEye1={0},{1}",
                                                   (int)(((_TextureCoordinates[328].X + _TextureCoordinates[1105].X) / 2) * 1920),
                                                   (int)(((_TextureCoordinates[328].Y + _TextureCoordinates[1105].Y) / 2) * 1080)
                                                   );

                    tw.WriteLine("LeftEye1={0},{1}",
                                                   (int)(((_TextureCoordinates[883].X + _TextureCoordinates[1092].X) / 2) * 1920),
                                                   (int)(((_TextureCoordinates[883].Y + _TextureCoordinates[1092].Y) / 2) * 1080)
                                                    );

                    tw.WriteLine("Nose1={0},{1}",
                                                   (int)(_TextureCoordinates[14].X * 1920),
                                                   (int)(_TextureCoordinates[14].Y * 1080)
                                                    );

                    tw.WriteLine("Mouth3={0},{1}",
                                                   (int)(_TextureCoordinates[10].X * 1920),
                                                   (int)(_TextureCoordinates[10].Y * 1080)
                                                    );

                    tw.WriteLine("Chin1={0},{1}",
                                                   (int)(_TextureCoordinates[0].X * 1920),
                                                   (int)(_TextureCoordinates[0].Y * 1080)
                                                    );
                }
                MessageBox.Show(BaseModelID + index + " successfully " +
                                   ((FaceModelTransform.basemodelid==null)? "created":"modified")
                                     
                        );
                FaceIndexAssigned = index;
                this.DialogResult = true;

                this.Close();

            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void chkImage_Checked(object sender, RoutedEventArgs e)
        {
            if (ImageBrush != null && (chkImage.IsChecked == true))
                theGrid.Background = ImageBrush;
            else
                theGrid.Background = null;

        }

        private void chkMesh_Checked(object sender, RoutedEventArgs e)
        {
            RenderMesh();
        }



        //private void chkImage_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    if (ImageBrush != null && (chkImage.IsChecked == true))
        //        theGrid.Background = ImageBrush;
        //    else
        //        theGrid.Background = null;
        //}

        //private void rbColor_Checked(object sender, RoutedEventArgs e)
        //{
        //    _MeshColor = ((bool)rbBlue.IsChecked) ?
        //         System.Drawing.Color.FromArgb(100, System.Drawing.Color.Blue) :
        //         System.Drawing.Color.FromArgb(100, System.Drawing.Color.Red);
        //    RenderMesh();
        //}




    }
}
