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

namespace ThreeDFaces
{
    /// <summary>
    /// Interaction logic for Window4.xaml
    /// </summary>
    public partial class Window4 : Window
    {

        System.Windows.Threading.DispatcherTimer dispatchTimerCheckClose = new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500), IsEnabled = false };

        private bool _bToClose = false;


        public bool IsWinLoaded
        {
            get;
            set;
        }

        public bool IsFaceSelected
        {
            get;
            private set;
        }

        public int FaceSelectedIndex
        {
            get;
            private set;
        }

        public System.Drawing.Rectangle[] FaceRects
        {
            get;
            set;
        }

        public Window4()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateLayout();
            this.Width = this.Image1.ActualWidth;

            double imagewidth = ((BitmapImage)(Image1.Source)).PixelWidth;
            double imageheight = ((BitmapImage)(Image1.Source)).PixelHeight;

            double wpfimagewidth = Image1.ActualWidth;
            double wpfimageheight = Image1.ActualHeight;
            double ratio = imageheight / wpfimageheight;

            var facerects = FaceRects;
            for(int i=0;i<facerects.Length;i++)
            {
                var facert = facerects[i];
                Rectangle rt=new Rectangle();
                rt.Name = "RT" + i;
                rt.Width = facert.Width /ratio ;
                rt.Height = facert.Height /ratio;
                rt.StrokeThickness = 2;
                rt.Stroke = System.Windows.Media.Brushes.Red;

                rt.Fill = new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));
                rt.HorizontalAlignment = HorizontalAlignment.Left;
                rt.VerticalAlignment = VerticalAlignment.Top;
                rt.Uid = rt.Name;
                Canvas.SetTop(rt, Canvas.GetTop(Grid1) + facert.Top/ratio );
                Canvas.SetLeft(rt, Canvas.GetLeft(Grid1) + facert.Left/ratio );

                Canvas1.Children.Add(rt);
                rt.MouseDown += rt_MouseDown;


            }

            FaceSelectedIndex = 0;

            dispatchTimerCheckClose.Tick += dispatchTimerCheckClose_Tick;
      
            IsWinLoaded = true;
        }

        void dispatchTimerCheckClose_Tick(object sender, EventArgs e)
        {

            dispatchTimerCheckClose.IsEnabled = false;
          
            if (_bToClose) //this.Close();
            {
                this.WindowState = WindowState.Minimized;
                IsFaceSelected = true;
            }

        }

        void rt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton==MouseButtonState.Pressed)
            {

                if(e.ClickCount>=2)
                {
                    int index = int.Parse(((Rectangle)sender).Uid.Substring(2));
                    FaceSelectedIndex = index;
                    _bToClose = true;
                    dispatchTimerCheckClose.IsEnabled = true;
                                             
                }
              
            }
         
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //make sure that the MainWindow is not in locked in a crtical process
            if (((MainWindow)((Window2)this.Owner).Owner).bSyncLock)
            {
                            
                WindowState = WindowState.Minimized;

                return;

            }    
     
            if (WindowState == WindowState.Normal)
            {
           
                this.IsFaceSelected = false;
                ((Window2)this.Owner).LoadFacialPointsFromCacheFaceRects();
            }

            if (WindowState == WindowState.Minimized)
            {
            
                IsFaceSelected = true;
            }
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            IsWinLoaded = false;
        }

    }
}
