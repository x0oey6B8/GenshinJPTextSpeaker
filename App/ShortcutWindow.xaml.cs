using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using System.Xml.Linq;

namespace GenshinJPTextSpeaker
{
    /// <summary>
    /// ShortcutWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ShortcutWindow : Window
    {
        public ShortcutWindow()
        {
            InitializeComponent();

            DataContext = new ShortcutViewModel();
        }
    }

    public class ShortcutViewModel : Bindable
    {
        public ObservableCollection<SettingRow> SystemSettings { get; set; } = new ObservableCollection<SettingRow>();

        public ObservableCollection<SettingRow> ManualSettings { get; set; } = new ObservableCollection<SettingRow>();

        public ObservableCollection<SettingRow> PresetSettings { get; set; } = new ObservableCollection<SettingRow>();

        public ShortcutViewModel()
        {
            SystemSettings.Add(new SettingRow((int)AppFeatures.ToggleEnabled, AppFeatures.ToggleEnabled, "有効無効の切り替え"));
            ManualSettings.Add(new SettingRow((int)AppFeatures.Run, AppFeatures.Run, "実行"));
            ManualSettings.Add(new SettingRow((int)AppFeatures.RunWithClipboard, AppFeatures.RunWithClipboard, "クリップボードの画像から実行"));
            AppSettings.Presets.ForEach(preset => PresetSettings.Add(new SettingRow(preset.Id, AppFeatures.RunWithPreset, preset.Name)));
        }
    }


    public class SettingRow : Bindable
    {
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        private string _name;

        public string Key { get => _key; set => SetProperty(ref _key, value); }
        private string _key;

        public bool Control 
        {
            get => _setting.ModifierKey.Control; 
            set
            {
                _setting.ModifierKey.Control = value;
                AppSettings.Instance.Save();
                OnNotifyPropertyChanged();
            } 
        }

        public bool Shift
        {
            get => _setting.ModifierKey.Shift;
            set
            {
                _setting.ModifierKey.Shift = value;
                AppSettings.Instance.Save();
                OnNotifyPropertyChanged();
            }
        }

        public bool Alt
        {
            get => _setting.ModifierKey.Alt;
            set
            {
                _setting.ModifierKey.Alt = value;
                AppSettings.Instance.Save();
                OnNotifyPropertyChanged();
            }
        }

        public bool KeyCancel
        {
            get => _setting.KeyCancel;
            set
            {
                _setting.KeyCancel = value;
                AppSettings.Instance.Save();
                OnNotifyPropertyChanged();
            }
        }

        public Command ChangeKeyCommand { get; }

        public Command ClearKeyCommand { get; }

        Shortcut _setting;
        KeyboardHooker _keyboard;
        KeyboardHooker _mouse;

        public SettingRow(int id, AppFeatures kind, string name)
        {
            _setting = AppSettings.Instance.Shortucts.Find(shortcut => shortcut.Id == id);
            if (_setting == null)
            {
                AppSettings.Instance.Shortucts.Add(_setting = new Shortcut
                {
                    Id = id,
                    Kind = kind
                });
            }

            Name = name;
            Key = _setting.Key.ToString();

            ClearKeyCommand = new Command(() => ChangeKey(Keys.None));
            ChangeKeyCommand = new Command(() => 
            {
                Key = "入力待ち";

                _keyboard = new KeyboardHooker();
                //_mouse = new KeyboardHooker();

                var callback = new EventHandler<HookEventArgs>((sender, e) =>
                {
                    if (!e.State) return;
                    if (e.Key == Keys.LButton) return;

                    _keyboard.Stop();
                    //_mouse.Stop();
                    _keyboard = null;
                   //_mouse = null;

                    ChangeKey(e.Key);
                    e.WillCancel = true;
                });

                _keyboard.Hooked += callback;
                //_mouse.Hooked += callback;

                _keyboard.Start();
                //_mouse.Start();
            });
        }

        private void ChangeKey(Keys key)
        {
            _setting.Key = key;
            Key = key.ToString();
            AppSettings.Instance.Save();
        }
    }
}
