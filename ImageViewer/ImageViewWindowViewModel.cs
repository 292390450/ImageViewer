using Microsoft.Xaml.Behaviors.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageViewer
{
 
    internal class ImageViewWindowViewModel : INotifyPropertyChanged
    {
        Image _maxImage;
        bool _isImageLoading;
        public ActionCommand _selectAcion;
        public ActionCommand SelectAcion => _selectAcion ?? (_selectAcion = new ActionCommand((func) =>
        {
            if (_isImageLoading)
            {
                return;
            }
            var oldSelect = SelectImage;
            if (oldSelect != null)
            {
                oldSelect.IsSelected = false;
                oldSelect.MaxBitmap = null;
            }
            if (_maxImage != null)
            {
                var group = _maxImage.RenderTransform as TransformGroup;
                if (group != null)
                {
                    var ts = group.Children[0] as ScaleTransform;
                    var tt = group.Children[1] as TranslateTransform;
                    ts.ScaleX = 1;
                    ts.ScaleY = 1;
                    //还原偏移
                    tt.X = 0;
                    tt.Y = 0;
                }
            }
            (func as ImageWrap).IsSelected = true;
            //大图没有加载去加载
            SelectImage = (func as ImageWrap);
        }));
        private ImageWrap _selectImage;
        public ImageWrap SelectImage
        {
            get { return _selectImage; }
            set
            {
                _selectImage = value;
                //大图没有加载去加载
                LoadMaxImage(value);
                OnPropertyChanged();
            }
        }

        private async void LoadMaxImage(ImageWrap value)
        {
            if (value != null && value.MaxBitmap == null)
            {
                _isImageLoading = true;
                var res = await Task.Run(() =>
                {
                    try
                    {

                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.CreateOptions = BitmapCreateOptions.DelayCreation;
                        // bitmapImage.DecodePixelWidth = 50;
                        bitmapImage.UriSource = new Uri(value.Path);
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        return bitmapImage;

                    }
                    catch
                    {
                        return null;
                    }
                });
                _isImageLoading = false;
                value.MaxBitmap = res;
            }
        }

        private ObservableCollection<ImageWrap> _images;
        public ObservableCollection<ImageWrap> Images
        {
            get { return _images; }
            set
            {
                _images = value;
                OnPropertyChanged();
            }
        }

        public ImageViewWindowViewModel(string[] images, Image maxImage = null, int defaultIndex = 0)
        {
            _maxImage = maxImage;
            //先生成缩略图列表
            InitImage(images, defaultIndex);
        }

        public ImageViewWindowViewModel(Image maxImage = null)
        {
            _maxImage = maxImage;
        }
        public async void SetSource<T>(IEnumerable<T> sources, Func<T, string> pathFactory, int defaultIndex)
        {
            await Task.Delay(100);
            Images = new ObservableCollection<ImageWrap>();
            foreach (var source in sources)
            {
                var res = await Task.Run(() =>
                {
                    try
                    {
                        var path = pathFactory?.Invoke(source);
                        if (!string.IsNullOrEmpty(path))
                        {
                            var img = new ImageWrap() { Path = path };
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.CreateOptions = BitmapCreateOptions.DelayCreation;
                            bitmapImage.DecodePixelWidth = 50;
                            bitmapImage.UriSource = new Uri(path);
                            bitmapImage.EndInit();
                            bitmapImage.Freeze();
                            img.MiniBitmap = bitmapImage;
                            return img;
                        }
                        return null;

                    }
                    catch
                    {
                        return null;
                    }
                });
                if (res != null)
                {
                    Images.Add(res);
                }
            }
            if (Images.Any())
            {
                if (defaultIndex >= Images.Count)
                {
                    defaultIndex = Images.Count - 1;
                }
                if (defaultIndex < 0)
                {
                    defaultIndex = 0;
                }
                SelectAcion.Execute(Images[defaultIndex]);
            }
        }
        private async void InitImage(string[] images, int defaultIndex)
        {
            await Task.Delay(100);
            Images = new ObservableCollection<ImageWrap>();
            foreach (var image in images)
            {
                var res = await Task.Run(() => {
                    try
                    {
                        var img = new ImageWrap() { Path = image };
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.DecodePixelWidth = 50;
                        bitmapImage.UriSource = new Uri(image);
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        img.MiniBitmap = bitmapImage;
                        return img;

                    }
                    catch
                    {
                        return null;
                    }

                });
                if (res != null)
                {
                    Images.Add(res);
                }

            }
            if (Images.Any())
            {
                if (defaultIndex >= Images.Count)
                {
                    defaultIndex = Images.Count - 1;
                }
                if (defaultIndex < 0)
                {
                    defaultIndex = 0;
                }
                SelectAcion.Execute(Images[defaultIndex]);
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
   
}
