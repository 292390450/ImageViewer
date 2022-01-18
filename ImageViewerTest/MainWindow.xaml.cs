using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using ImageViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageViewerTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
         //   this.Loaded += MainWindow_Loaded;
            //ImageLoadTest test=new ImageLoadTest();
            ////return;
            //test.WpfLoadLockFile();
            //return;
            var images = new ImageViewWindow();
            images.Show();
            images.SetSource<string>(new string[] { @"C:\Users\zeng\Pictures\d21.jpg",@"C:\Users\zeng\Pictures\78801ec615ee48fa8399263ef7b3c0a5.jpg",
            @"C:\Users\zeng\Pictures\123.jpg",@"C:\Users\zeng\Pictures\no_pic.jpg",@"C:\Users\zeng\Pictures\u178.jpg",@"C:\Users\zeng\Pictures\21.jpg",@"C:\Users\zeng\Pictures\21.jpg",
           @"C:\Users\zeng\Pictures\123.jpg",@"C:\Users\zeng\Pictures\no_pic.jpg",@"C:\Users\zeng\Pictures\u178.jpg",@"C:\Users\zeng\Pictures\21.jpg",@"C:\Users\zeng\Pictures\21.jpg",
                 @"C:\Users\zeng\Pictures\123.jpg",@"C:\Users\zeng\Pictures\no_pic.jpg",@"C:\Users\zeng\Pictures\u178.jpg",@"C:\Users\zeng\Pictures\21.jpg",@"C:\Users\zeng\Pictures\21.jpg"}, (ob) =>
                 {
                     return ob;
                 }, 99);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Delay(1000).ContinueWith(t => {
                Summary summary = BenchmarkRunner.Run<ImageLoadTest>();
            });
        }
    }
    [MemoryDiagnoser]
    public class ImageLoadTest
    {
        [Benchmark]
        public void GdiLoad()
        {
            using (System.Drawing.Bitmap bi = new System.Drawing.Bitmap(@"C:\Users\zeng\Pictures\78801ec615ee48fa8399263ef7b3c0a5.jpg")){
                var width = bi.Width;
                var height = bi.Height;
            }
        }
        [Benchmark]
        public void WpfLoadLockFile()
        {
            BitmapImage bitmapImage = new BitmapImage(new Uri(@"C:\Users\zeng\Pictures\78801ec615ee48fa8399263ef7b3c0a5.jpg"));
            var width = bitmapImage.PixelWidth;
            var height = bitmapImage.PixelHeight;
           // bitmapImage = null;
        }
        [Benchmark]
        public void WpfLoadedInMemory()
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
           
            // bitmapImage.DecodePixelWidth = 50;
            bitmapImage.UriSource = new Uri(@"C:\Users\zeng\Pictures\78801ec615ee48fa8399263ef7b3c0a5.jpg");
            bitmapImage.EndInit();
            var width = bitmapImage.PixelWidth;
            var height = bitmapImage.PixelHeight;
            bitmapImage = null;
        }
        [Benchmark(Baseline = true)]
        public void WpfFileStreamSource()
        {
            using(var stream = new FileStream(@"C:\Users\zeng\Pictures\78801ec615ee48fa8399263ef7b3c0a5.jpg", FileMode.Open, FileAccess.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
               // bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

                // bitmapImage.DecodePixelWidth = 150;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                var width = bitmapImage.PixelWidth;
                var height = bitmapImage.PixelHeight;
            }
           
        }
    }
}
