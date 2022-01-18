using Microsoft.Xaml.Behaviors.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageViewer
{
    /// <summary>
    /// ImageViewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImageViewWindow : Window, INotifyPropertyChanged
    {
        private bool _isImageLoading;
        private ObservableCollection<ImageWrap> _currentImages;
        private ImageWrap _oldSelect;
        private bool _isNoPraraContrs;
        private readonly double SingleScaleRatio = 0.5;
        private double _screenDpi;
        private Guid tipGuid = Guid.Empty;
     
    

        public ImageViewWindow()
        {
            InitializeComponent();
            
            _isNoPraraContrs = true;
            //_dataContext = new ImageViewWindowViewModel(MaxImage);
            //this.DataContext = _dataContext;
            this.DataContext = this;
        }
   
        /// <summary>
        /// 数据及数据处理工厂，工厂方法同步处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sources"></param>
        /// <param name="pathFactory"></param>
        /// <exception cref="Exception"></exception>
        public void SetSource<T>(IEnumerable<T> sources, Func<T, string> pathFactory, int defaultIndex = 0)
        {
            if (!_isNoPraraContrs)
            {
                throw new Exception("无参构造创建此类才能调用此方法");
            }
            GnneratImageList(sources, pathFactory, defaultIndex);
         //   _dataContext.SetSource(sources, pathFactory, defaultIndex);
        }
        private async void GnneratImageList<T>(IEnumerable<T> sources, Func<T, string> pathFactory, int defaultIndex = 0)
        {
            await Task.Delay(100);
            _screenDpi = 96 * PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            _currentImages = new ObservableCollection<ImageWrap>();
            MiniItemControl.ItemsSource= _currentImages;
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
                          //  bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
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
                    _currentImages.Add(res);
                }
            }
            if (_currentImages.Any())
            {
                if (defaultIndex >= _currentImages.Count)
                {
                    defaultIndex = _currentImages.Count - 1;
                }
                if (defaultIndex < 0)
                {
                    defaultIndex = 0;
                }
                SelectItem(_currentImages[defaultIndex]);
            }
        }
        public ActionCommand _selectAcion;
        public ActionCommand SelectAcion => _selectAcion ?? (_selectAcion = new ActionCommand((ob) =>
        {
            SelectItem(ob as ImageWrap);
        }));
        public void SelectItem(ImageWrap imageWrap)
        {
            if (_isImageLoading)
            {
                return;
            }
           
            if (_oldSelect != null)
            {
                _oldSelect.IsSelected = false;
                _oldSelect.MaxBitmap = null;
            }
            if (MaxImage != null)
            {
                var group = MaxImage.RenderTransform as TransformGroup;
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
            imageWrap.IsSelected = true;
            //加载大图
            _oldSelect=imageWrap;
            LoadMaxImage(imageWrap);
        }

        Rect _orginRec, _showRec;
        private double _scaleRatio;
        private async void LoadMaxImage(ImageWrap value)
        {
            if (value != null && value.MaxBitmap == null)
            {
                _isImageLoading = true;
                try
                {
                    //需要先获取图像的基本信息再进行创建渲染
                    BitmapImage infoImage = new BitmapImage(new Uri(value.Path));
                    //存下当前加载图像的实际宽高，计算当前展示image的宽高，进行缩放
                    _orginRec = new Rect(0, 0, infoImage.PixelWidth, infoImage.PixelHeight);
                    infoImage = null;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    //加载宽高
                    var widthRatio = BaseGrid.ActualWidth / _orginRec.Width;
                    var heightRatio =  BaseGrid.ActualHeight / _orginRec.Height;
                   
                    if (widthRatio < heightRatio)  //以宽为边界
                    {
                        _showRec = Rect.Transform(_orginRec, new Matrix(widthRatio, 0, 0, widthRatio, 0, 0));
                        bitmapImage.DecodePixelWidth = (int)_showRec.Width;
                        _scaleRatio = widthRatio;
                    }
                    else
                    {
                        _showRec = Rect.Transform(_orginRec, new Matrix(heightRatio, 0, 0, heightRatio, 0, 0));
                        bitmapImage.DecodePixelHeight = (int)_showRec.Height;
                        _scaleRatio = heightRatio;
                    }
                    //   bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    //bitmapImage.CreateOptions = BitmapCreateOptions.DelayCreation;

                  
                    bitmapImage.UriSource = new Uri(value.Path);
                    bitmapImage.EndInit();
                   //  bitmapImage.Freeze();
                
                    value.MaxBitmap = bitmapImage;
                }
                catch
                {

                }
                finally
                {
                    _isImageLoading = false;
                }

          
            }
            MaxImage.Source = value.MaxBitmap;
        }
        public ImageViewWindow(string[] images, int defaultIndex = 0)
        {
            InitializeComponent();
         //   _dataContext = new ImageViewWindowViewModel(images, MaxImage, defaultIndex);
            this.DataContext = this;
            GnneratImageList(images, (path) => path, defaultIndex);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_currentImages != null)
            {
                foreach (var image in _currentImages)
                {
                    image.MiniBitmap = null;
                    image.MaxBitmap = null;
                }
            }
            base.OnClosing(e);
        }
        private void HorizontalSmoothScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (e.Delta < 0)
            {
                MiniScroller.AnimateScroll(MiniScroller.HorizontalOffset + 80);
            }
            else
            {
                MiniScroller.AnimateScroll(MiniScroller.HorizontalOffset - 80);
            }

        }
        private void ImageZoom(object sender, MouseWheelEventArgs e)
        {
            var position = Mouse.GetPosition(e.Device.Target);
           
            if (e.Delta > 0)
            {
                //将当前比例放大
                var moreX =(int) (position.X /_scaleRatio*1.2);
                var moreY = (int)(position.Y /_scaleRatio*1.2);
                
                var bit=   MaxImage.Source as BitmapImage;
                if (bit.SourceRect == null || bit.SourceRect.IsEmpty)
                {
                    
                }
            }
            else
            {

            }
        }
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if (!_isCtrlCap)
            //{
            //    var index = _dataContext.Images.IndexOf(_dataContext.SelectImage);
            //    //上一张下一张滚动
            //    if (e.Delta > 0)
            //    {

            //        if (index >= 1)
            //        {
            //            _dataContext.SelectAcion.Execute(_dataContext.Images[index - 1]);
            //            index--;
            //        }
            //    }
            //    else
            //    {
            //        if (index <= _dataContext.Images.Count - 2)
            //        {
            //            _dataContext.SelectAcion.Execute(_dataContext.Images[index + 1]);
            //            index++;
            //        }

            //    }
            //    var contar = MiniItemControl.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
            //    MiniScroller.AnimateScroll(index * contar.ActualWidth);
            //    return;
            //}
            var position = Mouse.GetPosition(e.Device.Target);

            var group = MaxImage.RenderTransform as TransformGroup;
            var ts = group.Children[0] as ScaleTransform;
            var tt = group.Children[1] as TranslateTransform;
            if (e.Delta > 0)
            {
                if (ts.ScaleX >= 10)
                {
                    return;
                }
                //var myDoubleAnimation = new DoubleAnimation();
                //myDoubleAnimation.From = ts.ScaleX;
                //myDoubleAnimation.To = ts.ScaleX+ SingleScaleRatio;
                //myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
                //var myYDoubleAnimation = new DoubleAnimation();
                //myYDoubleAnimation.From = ts.ScaleY;
                //myYDoubleAnimation.To = ts.ScaleY + SingleScaleRatio;
                //myYDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
                //ts.BeginAnimation(ScaleTransform.ScaleXProperty, myDoubleAnimation);
                //ts.BeginAnimation(ScaleTransform.ScaleYProperty, myYDoubleAnimation);

                ts.ScaleX += SingleScaleRatio;
                ts.ScaleY += SingleScaleRatio;
                ts.CenterX = position.X;
                ts.CenterY = position.Y;
            }
            else
            {
                if (ts.ScaleX - SingleScaleRatio < 1)
                {
                    return;
                }

                if (ts.ScaleX - SingleScaleRatio == 1)
                {

                    ts.ScaleX = 1;
                    ts.ScaleY = 1;
                    ts.CenterX = 0;
                    ts.CenterY = 0;
                 
                    Show(ts.ScaleX * 100 + "%");
                    //还原偏移
                    tt.X = 0;
                    tt.Y = 0;
                    return;
                }
                //var myDoubleAnimation = new DoubleAnimation();
                //myDoubleAnimation.From = ts.ScaleX;
                //myDoubleAnimation.To = ts.ScaleX - SingleScaleRatio;
                //myDoubleAnimation.FillBehavior = FillBehavior.Stop;
                //myDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
                //var myYDoubleAnimation = new DoubleAnimation();
                //myYDoubleAnimation.From = ts.ScaleY;
                //myYDoubleAnimation.To = ts.ScaleY - SingleScaleRatio;
                //myYDoubleAnimation.FillBehavior = FillBehavior.Stop;
                //myYDoubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
                //ts.BeginAnimation(ScaleTransform.ScaleXProperty, myDoubleAnimation);
                //ts.BeginAnimation(ScaleTransform.ScaleYProperty, myYDoubleAnimation);
                ts.ScaleX -= SingleScaleRatio;
                ts.ScaleY -= SingleScaleRatio;
             
            }
            Show(ts.ScaleX * 100 + "%");
      
        }
       


        private void MaxImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MaxImage.ReleaseMouseCapture();
        }

        private void MaxImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.StartDrag(e.GetPosition(MaxImage));
        }

        private void MaxImage_MouseMove(object sender, MouseEventArgs e)
        {
            this.HandleDrag(e.GetPosition(MaxImage));
        }
        internal void HandleDrag(Point newPositionInElementCoordinates)
        {
            double relativeXDiff = newPositionInElementCoordinates.X - this.relativePosition.X;
            double relativeYDiff = newPositionInElementCoordinates.Y - this.relativePosition.Y;
            var sourceImg = MaxImage.Source as BitmapImage;
            
            if (sourceImg.SourceRect.IsEmpty==true)
            {

            }
            GeneralTransform elementToRoot = MaxImage.TransformToVisual(this);
            Point relativeDifferenceInRootCoordinates = TransformAsVector(elementToRoot, relativeXDiff, relativeYDiff);

            this.settingPosition = true;
            var group = MaxImage.RenderTransform as TransformGroup;
            var ts = group.Children[1] as TranslateTransform;
            ts.X = ts.X + relativeDifferenceInRootCoordinates.X;
            ts.Y = ts.Y + relativeDifferenceInRootCoordinates.Y;
            //  this.ApplyTranslation(relativeDifferenceInRootCoordinates.X, relativeDifferenceInRootCoordinates.Y);
            //  this.UpdatePosition();
            this.settingPosition = false;
        }
        private Point relativePosition;
        bool settingPosition;
        internal void StartDrag(Point positionInElementCoordinates)
        {
            this.relativePosition = positionInElementCoordinates;

            MaxImage.CaptureMouse();

            MaxImage.MouseMove += this.MaxImage_MouseMove;
            MaxImage.LostMouseCapture += this.OnLostMouseCapture;
            MaxImage.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.MaxImage_MouseLeftButtonUp), false /* handledEventsToo */);
        }
        private void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            MaxImage.MouseMove -= this.MaxImage_MouseMove;
            MaxImage.LostMouseCapture -= this.OnLostMouseCapture;
            MaxImage.RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(this.MaxImage_MouseLeftButtonUp));
        }
        private static Point TransformAsVector(GeneralTransform transform, double x, double y)
        {
            Point origin = transform.Transform(new Point(0, 0));
            Point transformedPoint = transform.Transform(new Point(x, y));
            return new Point(transformedPoint.X - origin.X, transformedPoint.Y - origin.Y);
        }
      

        private bool _isCtrlCap;
      

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                _isCtrlCap = true;
            }

        }

        private void MainWindow_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _isCtrlCap = false;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                _isCtrlCap = false;
            }

        }

    

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            var index = _currentImages.IndexOf(_oldSelect);

            if (index >= 1)
            {
                SelectItem(_currentImages[index - 1]);
                index--;
            }
            var contar = MiniItemControl.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
            MiniScroller.AnimateScroll(index * contar.ActualWidth);
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            var index = _currentImages.IndexOf(_oldSelect);
            if (index <= _currentImages.Count - 2)
            {
                SelectItem(_currentImages[index+ 1]);
                index++;
            }
            var contar = MiniItemControl.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
            MiniScroller.AnimateScroll(index * contar.ActualWidth);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var bit = MaxImage.Source as BitmapImage;
            
            bit.SourceRect = new Int32Rect(0, 0, 50, 50);
            
            //bit.DecodePixelWidth = 8000;
     
        }
        #region tip显示
        public void Show(string content, int IsType = 0)
        {
            TipAtom atom = new TipAtom();
            switch (IsType)
            {
                default:
                    atom.Content = content;
                    atom.UniqueGuid = Guid.NewGuid();
                    break;
            }
            Handle(atom);
        }
        public async void Handle(TipAtom message)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(async () =>
                {
                    //显示tip
                    TipView.Visibility = Visibility.Visible;
                    TipContent.Text = message.Content;
                    tipGuid = message.UniqueGuid;
                    await Task.Delay(2000);
                    if (tipGuid == message.UniqueGuid) //其他提示串入
                    {
                        TipView.Visibility = Visibility.Collapsed;
                    }
                }));
            }
            else
            {
                //显示tip
                TipView.Visibility = Visibility.Visible;
                TipContent.Text = message.Content;
                tipGuid = message.UniqueGuid;
                await Task.Delay(2000);
                if (tipGuid == message.UniqueGuid) //其他提示串入
                {
                    TipView.Visibility = Visibility.Collapsed;
                }
            }
        }
        #endregion

        #region notify
        public event PropertyChangedEventHandler PropertyChanged;

   

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

    }
    public class TipAtom
    {
        public string Content { get; set; }
        public Guid UniqueGuid { get; set; }
    }
    public class ImageWrap : INotifyPropertyChanged
    {

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }
        public string Path { get; set; }
        private BitmapSource _miniBitmap;
        public BitmapSource MiniBitmap
        {
            get { return _miniBitmap; }
            set
            {
                _miniBitmap = value;
                OnPropertyChanged(nameof(MiniBitmap));
            }
        }
        private BitmapSource _maxBitmap;
        public BitmapSource MaxBitmap
        {
            get { return _maxBitmap; }
            set
            {
                _maxBitmap = value;
                OnPropertyChanged(nameof(MaxBitmap));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
