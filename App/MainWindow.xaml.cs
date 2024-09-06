using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.UI.Composition;

namespace GenshinJPTextSpeaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModel("");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var settings = AppSettings.Instance.WindowSettings;

            settings.Top = this.Top;
            settings.Left = this.Left;
            settings.Width = this.Width;
            settings.Height = this.Height;
            settings.WindowState = this.WindowState;

            AppSettings.Instance.Save();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var settings = AppSettings.Instance.WindowSettings;

            if (!double.IsNaN(settings.Top))
                this.Top = settings.Top;

            if (!double.IsNaN(settings.Left))
                this.Left = settings.Left;

            if (!double.IsNaN(settings.Width))
                this.Width = settings.Width;

            if (!double.IsNaN(settings.Height))
                this.Height = settings.Height;

            this.WindowState = settings.WindowState;
        }
    }

    public class ViewModel : Bindable
    {
        public ObservableCollection<Style> Styles { get; } = new ObservableCollection<Style>();

        public ObservableCollection<Preset> Presets { get; } = new ObservableCollection<Preset>();

        public ObservableCollection<SectionViewModel> Sections { get; } = new ObservableCollection<SectionViewModel>();

        public Style SelectedSpeaker { get => _selectedSpeaker; set => SetProperty(ref _selectedSpeaker, value); }
        private Style? _selectedSpeaker;

        public SectionViewModel SelectedSection { get => _selectedSection; set => SetProperty(ref _selectedSection, value); }
        private SectionViewModel? _selectedSection;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        private string _title;

        public string Text { get => _text; set => SetProperty(ref _text, value); }
        private string _text;

        public bool Enabled { get => _enabled; set => SetProperty(ref _enabled, value); }
        private bool _enabled = true;

        public int Threshold
        {
            get => AppSettings.Instance.Threshold;
            set
            {
                AppSettings.Instance.Threshold = value;
                AppSettings.Instance.Save();
                OnNotifyPropertyChanged();
            }
        }

        public double Speed
        {
            get => AppSettings.Instance.Speed;
            set
            {
                AppSettings.Instance.Speed = value;
                AppSettings.Instance.Save();
                OnNotifyPropertyChanged();
            }
        }

        public bool AlwaysOnTop
        {
            get => AppSettings.Instance.AlwaysOnTop;
            set
            {
                AppSettings.Instance.AlwaysOnTop = value;
                AppSettings.Instance.Save();
                OnNotifyPropertyChanged();
            }
        }

        public bool SplitByPeriod
        {
            get => AppSettings.Instance.SplitByPeriod;
            set
            {
                AppSettings.Instance.SplitByPeriod = value;
                AppSettings.Instance.Save();
                OnNotifyPropertyChanged();
            }
        }

        public ImageSource Image { get => _image; set => SetProperty(ref _image, value); }
        private ImageSource _image = null;

        public bool IsRunning { get => _isRunning; set => SetProperty(ref _isRunning, value); }
        private bool _isRunning;

        public string CaptureArea { get => _captureArea; set => SetProperty(ref _captureArea, value); }
        private string _captureArea;

        public Command ChangeCaptureAreaCommand { get; set; }
        public Command OpenShortcutSettingCommand { get; set; }
        public Command RunCommand { get; }
        public Command RunWithPresetCommand { get; }
        public Command RunWithClipboardImageCommand { get; }
        public Command StopCommand { get; }
        public Command ReplayCommand { get; }
        public Command FetchSpeakersCommand { get; }
        public Command ChangeSpeakerCommand { get; }
        public Command TestCommand { get; }
        public Command TestChangeCaptureAreaCommand { get; }
        public NotificationViewModel Notification { get; } = new NotificationViewModel();

        public PlayOptions PlayOptions => new PlayOptions
        {
            SpeakerId = SelectedSpeaker.id,
            Speed = Speed
        };

        private CancellationTokenSource _cts;
        private KeyboardHooker _keyboard;
        private MouseHooker _mouse;
        private bool _isShortuctWindowShowing;

        private const string TitleTemplate = "原神日本語テキスト読み上げ - @";

        // 消さない
        public ViewModel()
        {
        }

        public ViewModel(string dummy)
        {
            Title = TitleTemplate.Replace("@", "有効");

            Presets = new ObservableCollection<Preset>(AppSettings.Presets);

            TestCommand = new Command(() =>
            {
            });

            OpenShortcutSettingCommand = new Command(() =>
            {
                _isShortuctWindowShowing = true;
                var window = new ShortcutWindow
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = App.Current.MainWindow
                };
                window.ShowDialog();
                _isShortuctWindowShowing = false;
            });

            StopCommand = new Command(Stop);


            FetchSpeakersCommand = new Command(async () =>
            {
                try
                {
                    var styles = await Utility.FetchSpeakers();
                    styles.ForEach(Styles.Add);
                    SelectedSpeaker = styles.First();
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    System.Windows.MessageBox.Show("ボイス一覧の取得に失敗しました。VOICEVOXが起動していない可能性があります。\n先にVOICEVOXを起動してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    Process.GetCurrentProcess().Kill();
                }
            });

            ChangeSpeakerCommand = new Command(parameter =>
            {
                if (parameter is Style speaker)
                {
                    SelectedSpeaker = speaker;
                    AppSettings.Instance.SpeakerId = SelectedSpeaker.id;
                    AppSettings.Instance.Save();
                }
            });

            RunWithClipboardImageCommand = new Command(() =>
            {
                RunWithClipboardImage();
            });

            RunWithPresetCommand = new Command(parameter =>
            {
                if (parameter is Preset preset)
                {
                    Run(preset.CaptureArea, preset.Threshold);
                }
            });

            RunCommand = new Command(() =>
            {
                Run(AppSettings.Instance.CaptureArea, Threshold);
            });

            ReplayCommand = new Command(async () =>
            {
                if (SelectedSection != null)
                {
                    Stop();
                    _cts = new CancellationTokenSource();
                    await SelectedSection.Play(PlayOptions, _cts.Token);
                }
            });

            ChangeCaptureAreaCommand = new Command(async () =>
            {
                Bitmap ss = null;

                var genshinWindow = WindowController.FindWindow("原神");
                if (genshinWindow.Handle != IntPtr.Zero)
                {

                    System.Windows.MessageBox.Show("OKボタンを押した後、原神のウィンドウのスクリーンショットを撮影します。少しのあいだ操作しないでください。", "通知");

                    var genshinWindowState = genshinWindow.State;
                    var mainWindow = WindowController.CreateInstance(new WindowInteropHelper(App.Current.MainWindow).Handle);
                    var mainWindowState = mainWindow.State;

                    mainWindow.Minimize();
                    genshinWindow.Activate();
                    await Task.Delay(1000);
                    var (l, t, w, h) = genshinWindow.Bounds.ToTuple();
                    ss = Utility.CaptureScreen(l, t, w, h);

                    await Console.Out.WriteLineAsync($"{l}, {t}, {w}, {h}");

                    genshinWindow.State = genshinWindowState;
                    mainWindow.State = mainWindowState;
                }
                else
                {
                    System.Windows.MessageBox.Show("原神のウィンドウが見つかりませんでした", "通知");
                }

                var boundsWindow = new BoundsWindow(ss)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = App.Current.MainWindow
                };
                boundsWindow.ShowDialog();
                if (boundsWindow.Completed)
                {
                    var bounds = boundsWindow.Bounds;
                    AppSettings.Instance.CaptureArea.Left = bounds.left;
                    AppSettings.Instance.CaptureArea.Top = bounds.top;
                    AppSettings.Instance.CaptureArea.Width = bounds.width;
                    AppSettings.Instance.CaptureArea.Height = bounds.height;
                    AppSettings.Instance.Save();
                    CaptureArea = AppSettings.Instance.CaptureArea.ToString();
                }
            });

            CaptureArea = AppSettings.Instance.CaptureArea.ToString();

            _keyboard = new KeyboardHooker();
            _keyboard.Hooked += Hooked;
            _keyboard.Start();

            _mouse = new MouseHooker();
            _mouse.Hooked += Hooked;
            _mouse.Start();
        }

        private void Hooked(object? sender, HookEventArgs e)
        {
            if (_isShortuctWindowShowing)
            {
                return;
            }

            if (e.State && e.HasStateChanged)
            {
                var shortcut = AppSettings.Instance.Shortucts
                    .FindAll(s => s.Key == e.Key)
                    .Where(s => s.ModifierKey.IsPressed())
                    .OrderByDescending(s => s.ModifierKey.Count())
                    .FirstOrDefault();

                if (shortcut != null)
                {
                    switch (shortcut.Kind)
                    {
                        case AppFeatures.ToggleEnabled:
                            TogglEnabled();
                            break;
                        case AppFeatures.Run:
                            Run(AppSettings.Instance.CaptureArea, Threshold);
                            break;
                        case AppFeatures.RunWithClipboard:
                            RunWithClipboardImageCommand.Execute(null);
                            break;
                        case AppFeatures.RunWithPreset:
                            var preset = AppSettings.Presets.Find(p => p.Id == shortcut.Id);
                            if (preset != null)
                            {
                                Run(preset.CaptureArea, preset.Threshold);
                            }
                            break;
                    }

                    if (Enabled && shortcut.KeyCancel)
                    {
                        e.WillCancel = true;
                    }
                }
            }
        }

        private void TogglEnabled()
        {
            Enabled = !Enabled;
            Title = TitleTemplate.Replace("@", (Enabled ? "有効" : "無効"));
            if (Notification.NotifyToggleEnabled)
            {
                Utility.QuickSpeak($"読み上げが{(Enabled ? "有効" : "無効")}になりました", speakerId: 2, speed: 1.25);
            }
        }

        private void RunWithClipboardImage()
        {
            var bitmap = System.Windows.Forms.Clipboard.GetImage();
            if (bitmap is not Bitmap bmp)
            {
                Utility.QuickSpeak("クリップボードに画像がありません", speakerId: 2, speed: 1.25);
                return;
            }

            Handle(bmp, Threshold);
        }

        private void Run(CaptureArea area, int threshold)
        {
            try
            {
                if (!Enabled)
                {
                    return;
                }

                // スクショの範囲取得
                var window = WindowController.FindWindow("原神");
                if (window.Handle == IntPtr.Zero)
                {
                    return;
                }

                var dpi = Win32Api.GetDpiFrom(window.Handle);
                var cb = window.ClientBounds.ToTuple();
                var left = cb.left;
                var top = cb.top;
                var width = cb.width * dpi;
                var height = cb.height * dpi;
                var l = (int)(left + width * area.Left);
                var w = (int)(width * area.Width);
                var h = (int)(height * area.Height);
                var t = (int)(top + height * area.Top);
                var marginBottom = (int)(h * 0.03);

                // スクショを撮る
                using var screenshot = Utility.CaptureScreen(l, t, w, h - marginBottom);

                // OCRと再生
                Handle(screenshot, threshold);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void Stop()
        {
            _cts?.Cancel();
            while (IsRunning)
            {
                await Task.Delay(500);
            }
        }

        private async void Handle(Bitmap screenshot, int threshold)
        {
            Stop();

            try
            {
                _cts = new CancellationTokenSource();

                IsRunning = true;

                // フィルターをかけるて文字だけが認識できるようにする
                using var filteredImage = Utility.ColorFilter(screenshot, 255, threshold);

                // Bitmapを表示用のデータに変換
                Image = filteredImage.ToImageSource();

                // BitmapではOCRが出来ないのでSoftwareBitmapに変換すし、OCRを実行する
                using var softwareBitmap = await Utility.GetSoftwareBitmap(filteredImage);
                var result = await Utility.RecognizeText(softwareBitmap);

                // 認識できた文字からスペースを取り除き、UIDが含まれている行は削除する
                var lines = result.Lines.Select(x => x.Text.Replace(" ", "")).ToList();
                lines.RemoveAll(line => line.Contains("UID"));

                // 各行の結合
                var line = string.Concat(lines);
                if (!string.IsNullOrWhiteSpace(line))
                {
                    Text = line;

                    var sections = CreateSectionsFrom(line).ToList();
                    Sections.Clear();
                    sections.ForEach(Sections.Add);

                    for (int i = 0; i < sections.Count && !_cts.IsCancellationRequested; i++)
                    {
                        var section = sections[i];
                        await section.Play(PlayOptions, _cts.Token);
                    }
                }
            }
            finally
            {
                IsRunning = false;
            }
        }

        private IEnumerable<SectionViewModel> CreateSectionsFrom(string line)
        {
            if (SplitByPeriod || line.Length >= AppSettings.Instance.MaxLengthOfLine)
            {
                return line.Split('。').Select(section => new SectionViewModel(section));
            }
            else
            {
                return new List<SectionViewModel> { new SectionViewModel(line) };
            }
        }
    }

    public class SectionViewModel : Bindable
    {
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        private string _text;

        public bool IsPlaying { get => _isPlaying; set => SetProperty(ref _isPlaying, value); }
        private bool _isPlaying;

        public SectionViewModel(string text)
        {
            Text = text;
        }

        public async Task Play(PlayOptions options, CancellationToken token)
        {
            try
            {
                using var stream = await Utility.GenerateAudioStream(new GenerateAudioOptions
                {
                    Text = Text,
                    SpeakerId = options.SpeakerId,
                    Speed = options.Speed
                });

                if (token.IsCancellationRequested)
                {
                    return;
                }

                IsPlaying = true;

                using var audioFileReader = new NAudio.Wave.WaveFileReader(stream);
                using var outputDevice = new NAudio.Wave.WaveOutEvent();
                outputDevice.Init(audioFileReader);
                outputDevice.Play();

                while (outputDevice.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                {
                    if (token.IsCancellationRequested)
                    {
                        outputDevice.Stop();
                        break;
                    }
                    await Task.Delay(500, token); // キャンセル可能な待機
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                IsPlaying = false;
            }
        }
    }

    public class NotificationViewModel : Bindable
    {
        public bool NotifyToggleEnabled
        {
            get => AppSettings.Instance.Notification.NotifyToggleEnabled;
            set
            {
                AppSettings.Instance.Notification.NotifyToggleEnabled = value;
                AppSettings.Instance.Save();
                OnNotifyPropertyChanged();
            }
        }
    }

    public class RoundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double d)
            {
                return value;
            }

            return $"{d:F2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            return value;
        }
    }

    public class ReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool b)
            {
                return value;
            }

            return !b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool b)
            {
                return value;
            }

            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
