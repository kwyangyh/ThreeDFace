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
using Gif.Components;

namespace ThreeDFaces
{
    /// <summary>
    /// Interaction logic for Window5.xaml
    /// </summary>
    public partial class Window5 : Window
    {
        private bool _bIsBusy = false;

        public Window5()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            BitmapSource bms=((MainWindow)this.Owner).CreateSnap();
            Image1.Source = bms;
            string navstr = "<html><body style=\"margin: 0px; background: #0e0e0e;\"></body></html>";

            WebBrowser1.NavigateToString(navstr);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {

                _bIsBusy = true;
                Button1.IsEnabled = false;
              
                try
                {

                    bool bCompleteRound = false;
                    Slider slider = ((MainWindow)this.Owner).hscroll;
                    float deltaXDeg = 5f;
                    int nDirChanged = 0;

                    AnimatedGifEncoder enc = new AnimatedGifEncoder();
                    String outputFilePath = AppDomain.CurrentDomain.BaseDirectory + "temp\\~gif.gif";
                    enc.Start(outputFilePath);
                    enc.SetDelay(100);
                    //-1:no repeat,0:always repeat
                    enc.SetRepeat(0);
                    int maxwidth = 0;
                    int maxheight = 0;

                    int frameno = 0;
                    string savedTitle = this.Title;
                    while (!bCompleteRound)
                    {
                        var deg = slider.Value;
                        System.Diagnostics.Debug.Print("Deg=" + deg);
                        BitmapSource bms = ((MainWindow)this.Owner).CreateSnap();


                        Image1.Source = bms;
                        Image1.Refresh();
                  
                      
                        System.Drawing.Bitmap bm = CCommon.BitmapImage2Bitmap(bms);
                        if (bm.Width > maxwidth) maxwidth = bm.Width;
                        if (bm.Height > maxheight) maxheight = bm.Height;


                        enc.AddFrame(bm);
                        frameno++;
                        this.Title = "Processing Frame no:" + frameno;

                        if (deg > 50 || deg < -50)
                        {
                            deltaXDeg = -1 * deltaXDeg;
                            nDirChanged++;
                        }
                        slider.Value += deltaXDeg;

                        System.Windows.Forms.Application.DoEvents();

                        if (nDirChanged >= 2 && slider.Value > 0 && slider.Value < 10)
                            bCompleteRound = true;

                        bm.Dispose();
                        bm = null;
                        
                    }
                    enc.Finish();

                    this.Title = savedTitle;
                    string path2gif = "file:///" + AppDomain.CurrentDomain.BaseDirectory + "temp/~gif.gif";

                    int height = 0, width = 0;
                    double ratio = ((double)maxwidth) / maxheight;
                    if (ratio > 1.0) //long
                    {
                        width = (int)WebBrowser1.Width;
                        height = (int)(width / ratio);
                    }
                    else //tall;
                    {
                        height = (int)WebBrowser1.Height;
                        width = (int)(height * ratio);
                    }



                    string imgstr = "<img width=\"" + width + "\" height=\"" + height + "\" src=\"" + path2gif + "\">";


                   // string imgstr = "<img src=\"" + path2gif + "\">";
                    string strdiv = "<div style=\"width:100%; text-align:center\">" + imgstr + "</div>";

                    string navstr = "<html><body style=\"margin: 0px; background: #0e0e0e;\">"+strdiv+"</body></html>";

                    WebBrowser1.NavigateToString(navstr);

                    
                }
                catch
                {

                }

                Button2.IsEnabled = true;
                _bIsBusy = false;
            

        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + "animatedgif\\";

                saveFileDialog.Filter = "Gif files (*.gif)|*.gif";

                if (saveFileDialog.ShowDialog() != true) return;
                string destfilename = saveFileDialog.FileName;
                string srcfilename = AppDomain.CurrentDomain.BaseDirectory + "temp\\~gif.gif";
                if(System.IO.File.Exists(destfilename) )
                        System.IO.File.Delete(destfilename);
                System.IO.File.Copy(srcfilename, destfilename);
                MessageBox.Show("File saved successfully");
            }
            catch
            {
                MessageBox.Show("Failed to save file");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            e.Cancel =_bIsBusy ;
        }
    }
}
