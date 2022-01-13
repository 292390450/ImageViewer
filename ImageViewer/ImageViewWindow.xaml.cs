using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private bool _isNoPraraContrs;
        private readonly double SingleScaleRatio = 0.5;
        ImageViewWindowViewModel _dataContext;
        private Guid tipGuid = Guid.Empty;
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


        public ImageViewWindow()
        {
            InitializeComponent();
            _isNoPraraContrs = true;
            _dataContext = new ImageViewWindowViewModel(MaxImage);
            this.DataContext = _dataContext;
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
            _dataContext.SetSource(sources, pathFactory, defaultIndex);
        }
        public ImageViewWindow(string[] images, int defaultIndex = 0)
        {
            InitializeComponent();
            _dataContext = new ImageViewWindowViewModel(images, MaxImage, defaultIndex);
            this.DataContext = _dataContext;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_dataContext?.Images != null)
            {
                foreach (var image in _dataContext.Images)
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
            var index = _dataContext.Images.IndexOf(_dataContext.SelectImage);

            if (index >= 1)
            {
                _dataContext.SelectAcion.Execute(_dataContext.Images[index - 1]);
                index--;
            }
            var contar = MiniItemControl.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
            MiniScroller.AnimateScroll(index * contar.ActualWidth);
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            var index = _dataContext.Images.IndexOf(_dataContext.SelectImage);
            if (index <= _dataContext.Images.Count - 2)
            {
                _dataContext.SelectAcion.Execute(_dataContext.Images[index + 1]);
                index++;
            }
            var contar = MiniItemControl.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
            MiniScroller.AnimateScroll(index * contar.ActualWidth);
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
