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
    /// Interaction logic for Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        private double _mouseX0 = 0;
        private double _mouseY0 = 0;
   

        bool _bHeightChanged = false;

        System.Windows.Threading.DispatcherTimer dispatchTimerResize = 
            new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 500), IsEnabled = false };

        public Window3()
        {
            InitializeComponent();
        }

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition((UIElement)sender);
            System.Diagnostics.Debug.Print("Mouse down=" + p.X + "," + p.Y);
            _mouseX0 = p.X;
            _mouseY0 = p.Y;
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Point p = e.GetPosition((UIElement)sender);
            System.Diagnostics.Debug.Print("Mouse move=" + p.X + "," + p.Y);
            double deltaX = p.X - _mouseX0;
            double deltaY = p.Y - _mouseY0;

            double newX = Canvas.GetLeft((UIElement)sender) + deltaX;
            double newY = Canvas.GetTop((UIElement)sender) + deltaY;
            System.Diagnostics.Debug.Print("Pos=" + newX + "," + newY );
            this.Left = newX;
            this.Top = newY;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = 150;
            this.Height = 150;
            dispatchTimerResize.Tick += dispatchTimerResize_Tick;
        }

        void dispatchTimerResize_Tick(object sender, EventArgs e)
        {
            dispatchTimerResize.IsEnabled = false;
            if (_bHeightChanged)
                this.Width = this.Height;
            else
                this.Height = this.Width;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {


            Size oldsize = e.PreviousSize;
            Size newsize = e.NewSize;

            _bHeightChanged = ((int)oldsize.Height) == ((int)newsize.Height) ? false : true;

            dispatchTimerResize.IsEnabled = true;
            dispatchTimerResize.Stop();
            dispatchTimerResize.Start();
  
        }


    }
}
