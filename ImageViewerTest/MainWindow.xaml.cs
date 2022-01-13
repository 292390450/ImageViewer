using ImageViewer;
using System;
using System.Collections.Generic;
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
    }
}
