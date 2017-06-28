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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private bool _isloaded = false;

        private UIElement _sourceimage = null;
        private UIElement _sourcebrushimage = null;

        public UIElement SourceImage
        {
            get{return _sourceimage ;}
            set { _sourceimage = value; }
        }

        public UIElement SourceBrushImage
        {
            get{return _sourcebrushimage;}
            set{_sourcebrushimage =value;}
        }
      
        public void UpdateDisplay()
        {
          
            if (_sourcebrushimage != null && _sourcebrushimage is Grid)
            {
                TopGrid.Background = ((Grid)_sourcebrushimage).Background;
            }
            else
            {
                TopGrid.Background = Brushes.AliceBlue;
            }
            if (_sourceimage != null)
                Image1.Source = ((Image)_sourceimage).Source;
        }

        public Window1()
        {
            InitializeComponent();
        }

        public bool IsWinLoaded
        {
            get { return _isloaded; }
           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isloaded = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _isloaded = false;
        }

        private void ButtonSave_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "snap\\";

                saveFileDialog.Filter = "Jpg files (*.jpg)|*.jpg";
            
                if (saveFileDialog.ShowDialog() != true) return;
                string filename = saveFileDialog.FileName;

                int imagewidth = (int)Image1.Source.Width;
                int imageheight = (int)Image1.Source.Height ;
                System.Drawing.Bitmap bm=null;

                ImageBrush ib = null;
                if (SourceBrushImage == null)
                    bm = new System.Drawing.Bitmap(imagewidth, imageheight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                else
                {
                  
                  ib=(ImageBrush)(TopGrid.Background) ;
                  if (ib != null)
                  {
                      BitmapSource ibimgsrc = ib.ImageSource as BitmapSource;
                      bm = CCommon.BitmapImage2Bitmap(ibimgsrc);
                  }
                  else
                      bm = new System.Drawing.Bitmap(imagewidth, imageheight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                }
                System.Drawing.Graphics gbm = System.Drawing.Graphics.FromImage(bm);

                if (SourceBrushImage == null || ib == null)
                    gbm.Clear(System.Drawing.Color.FromArgb(255,255,255,255));

              

                //Image1 store the image
                System.Drawing.Bitmap bm2 = CCommon.BitmapImage2Bitmap(Image1.Source as BitmapSource );
               
                gbm.DrawImage(bm2, 0, 0);
                gbm.Dispose();

                bm.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg); //default to 24 rgb
               // bm.Save(filename, System.Drawing.Imaging.ImageFormat.Png); //default to 32 argb
                bm.Dispose();
                bm2.Dispose();
                bm = null;
                bm2 = null;

                MessageBox.Show("File saved successfully.");
            }
            catch
            {
                MessageBox.Show("Failed to save file.");
            }
        }

        private void ButtonSave_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ButtonSave_MouseLeftButtonDown(null, null);
        }


    }
}
