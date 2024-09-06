using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Clipboard = System.Windows.Forms.Clipboard;
using MessageBox = System.Windows.MessageBox;
using MouseButton = System.Windows.Input.MouseButton;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

using P = System.Windows.Point;

namespace GenshinJPTextSpeaker
{
    public partial class BoundsWindow : Window
    {
        public IntPtr WindowHandle => new System.Windows.Interop.WindowInteropHelper(this).Handle;

        private P _startPosition;
        private P _startPositionOnWindow;
        private P _lastPosition;
        private MouseButton _startButton;

        double _left, _top, _width, _height;
        public bool Completed　{ get; private set; }

        public BoundsWindow(Bitmap bitmap)
        {
            InitializeComponent();

            Title = $"範囲を選択してください - マウス左：範囲選択｜スクロール：ズーム｜マウス右・中ボタン｜画面移動";

            if (DataContext is BoundsViewModel viewModel)
            {
                viewModel.ResetPositionAndZoomCommand = new Command(() => ResetPositionAndZoom(null, null));
                viewModel.ImageSourceChanged += (sender, e) =>
                {
                    ResetPositionAndZoom(null, null);
                };
            }

            if (bitmap != null)
            {
                SetImage(bitmap);
            }
        }

        public (double left, double top, double width, double height) Bounds => (_left, _top, _width, _height);

        public void SetImage(Bitmap bitmap)
        {
            if (DataContext is BoundsViewModel viewModel)
            {
                viewModel.SetImage(bitmap);
            }
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (image.IsMouseCaptured)
            {
                return;
            }

            var element = (UIElement)sender;
            var position = e.GetPosition(element);
            var matrix = transform.Matrix;
            var scale = e.Delta >= 0 ? 1.1 : (1.0 / 1.1);

            matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
            transform.Matrix = matrix;
        }

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (top.IsMouseCaptured)
            {
                return;
            }

            _startPosition = e.GetPosition(image);
            _startPositionOnWindow = e.GetPosition(this);
            image.CaptureMouse();
        }

        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (image.IsMouseCaptured)
            {
                var endPosition = e.GetPosition(image);
                image.ReleaseMouseCapture();
                border.Width = 0;
                border.Height = 0;

                if (DataContext is not BoundsViewModel viewmodel)
                {
                    return;
                }

                if (viewmodel.ImageSource == null)
                {
                    return;
                }

                var (left, top, _, _, normalizedWidth, normalizedHeight) = CalcSize(_startPosition, endPosition);
                var success = viewmodel.CutImage(ref left, ref top, ref normalizedWidth, ref normalizedHeight);
                if (success)
                {
                    viewmodel.PreviewViewModel.IsOpen = true;
                    var result = MessageBox.Show("この範囲でよろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        _left = left;
                        _top = top;
                        _width = Math.Clamp(normalizedWidth, 0, 1.0);
                        _height = Math.Clamp(normalizedHeight, 0, 1.0);
                        Console.WriteLine($"{_left}, {_top}, {_width}, {_height}");
                        Completed = true;
                        Close();
                    }
                    else
                    {
                        viewmodel.PreviewViewModel.IsOpen = false;
                    }
                }
            }
        }

        private void top_MouseMove(object sender, MouseEventArgs e)
        {
            var newPosition = e.GetPosition(this);

            if (top.IsMouseCaptured)
            {
                var currentPosition = e.GetPosition(top);
                var delta = currentPosition - _lastPosition;
                var matrix = transform.Matrix;
                matrix.Translate(delta.X, delta.Y);
                transform.Matrix = matrix;
                _lastPosition = currentPosition;
            }
            else if (image.IsMouseCaptured)
            {
                var (_, _, width, height, _, _) = CalcSize(_startPositionOnWindow, newPosition);
                var delta = newPosition - _startPositionOnWindow;
                var mx = delta.X < 0 ? newPosition.X : _startPositionOnWindow.X;
                var my = delta.Y < 0 ? newPosition.Y : _startPositionOnWindow.Y;
                border.Margin = new Thickness(mx, my, 0, 0);
                border.Width = width;
                border.Height = height;
            }

            verticalLine.Margin = new Thickness(newPosition.X, 0, 0, 0);
            horizontalLine.Margin = new Thickness(0, newPosition.Y, 0, 0);

        }

        private (double left, double top, double width, double height, double normalizedWidth, double normalizedHeight) CalcSize(P oldPoint, P newPosition)
        {
            var delta = newPosition - oldPoint;
            var width = Math.Abs(delta.X);
            var height = Math.Abs(delta.Y);

            var normalizedWidth = width / image.ActualWidth;
            var normalizedHeight = height / image.ActualHeight;

            var x = oldPoint.X < newPosition.X ? oldPoint.X : newPosition.X;
            var y = oldPoint.Y < newPosition.Y ? oldPoint.Y : newPosition.Y;

            var left = x / image.ActualWidth;
            var top = y / image.ActualHeight;

            return (left, top, width, height, normalizedWidth, normalizedHeight);
        }

        private void top_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton is not MouseButton.Right and not MouseButton.Middle)
            {
                return;
            }

            if (image.IsMouseCaptured)
            {
                return;
            }

            _startButton = e.ChangedButton;
            _lastPosition = e.GetPosition(top);
            top.CaptureMouse();
        }

        private void top_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton is not MouseButton.Right and not MouseButton.Middle)
            {
                return;
            }

            if (top.IsMouseCaptured && e.ChangedButton == _startButton)
            {
                top.ReleaseMouseCapture();
            }
        }

        private void ResetPositionAndZoom(object sender, RoutedEventArgs e)
        {
            transform.Matrix = new Matrix();
        }

        private void image_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            ResetPositionAndZoom(null, null);
        }
    }

    public class BoundsViewModel : Bindable
    {
        public event EventHandler ImageSourceChanged;

        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get => _imageSource;
            set
            {
                SetProperty(ref _imageSource, value);
                ImageSourceChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public PreviewViewModel PreviewViewModel { get; } = new PreviewViewModel();

        private Bitmap _sourceImage;

        public Command LoadImageCommand { get; }
        public Command LoadImageFromClipboardCommand { get; }
        public Command ResetPositionAndZoomCommand { get => _resetPositionAndZoomCommand; set => SetProperty(ref _resetPositionAndZoomCommand, value); }
        private Command _resetPositionAndZoomCommand;


        public BoundsViewModel()
        {
            LoadImageFromClipboardCommand = new Command(SetImageFromClipboard);
            LoadImageCommand = new Command(() =>
            {
                using var ofd = new OpenFileDialog
                {
                    Filter = "Image File(*.png,*.jpg,*.bmp)|*.png;*.jpg;*.bmp;",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };
                var result = ofd.ShowDialog();
                if (result is DialogResult.OK && File.Exists(ofd.FileName))
                {
                    SetImage(ofd.FileName);
                }
            });
        }

        public void SetImage(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _ = MessageBox.Show("画像ファイルが存在しませんでした", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                SetImage(new Bitmap(filePath));
            }
            catch (Exception e)
            {
                _ = MessageBox.Show("画像の読み込みに失敗しました", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetImage(Bitmap bitmap)
        {
            try
            {
                _sourceImage = bitmap;
                ImageSource = _sourceImage.ToImageSource();
            }
            catch (Exception e)
            {
                _ = MessageBox.Show("画像の読み込みに失敗しました", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetImageFromClipboard()
        {
            try
            {
                var bitmap = Clipboard.GetImage() as Bitmap;
                if (bitmap == null)
                {
                    _ = MessageBox.Show("クリップボードから画像を読み込めませんでした", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _sourceImage = bitmap;
                ImageSource = _sourceImage.ToImageSource();
            }
            catch (Exception e)
            {
                _ = MessageBox.Show("画像の読み込みに失敗しました", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool CutImage(ref double left, ref double top, ref double normalizedWidth, ref double normalizedHeight)
        {
            if (normalizedWidth <= 0 || normalizedHeight <= 0)
            {
                return false;
            }

            left = Math.Max(0, left);
            top = Math.Max(0, top);


            var imageWidth = _sourceImage.Width;
            var imageHeight = _sourceImage.Height;

            var x = Math.Clamp((int)Math.Round(left * imageWidth), 0, imageWidth);
            var y = Math.Clamp((int)Math.Round(top * imageHeight), 0, imageHeight);

            var rect = new Rectangle
            {
                X = x,
                Y = y,
                Width = Math.Clamp((int)Math.Round(normalizedWidth * imageWidth), 1, imageWidth - x),
                Height = Math.Clamp((int)Math.Round(normalizedHeight * imageHeight), 1, imageHeight - y)
            };

            using var clippedBitmap = _sourceImage.Clone(rect, _sourceImage.PixelFormat);

            PreviewViewModel.SetImage(rect, clippedBitmap);

            return true;
        }
    }

    public class PreviewViewModel : Bindable
    {
        public bool IsOpen { get => _isOpen; set => SetProperty(ref _isOpen, value); }
        private bool _isOpen;

        public ImageSource ImageSource { get => _imageSource; set => SetProperty(ref _imageSource, value); }
        private ImageSource _imageSource;

        public Rectangle Rectangle { get; private set; }

        public void SetImage(Rectangle rect, Bitmap bitmap)
        {
            try
            {
                Rectangle = rect;
                ImageSource = bitmap.ToImageSource();
            }
            catch (Exception e)
            {
            }
        }
    }
}