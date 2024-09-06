using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GenshinJPTextSpeaker
{
    public class AppSettings
    {
        private static readonly Lazy<AppSettings> _instance = new Lazy<AppSettings>(() => LoadSettings());

        public static List<Preset> Presets { get; set; } = new List<Preset>();

        public bool AlwaysOnTop { get; set; } = false;
        public double Speed { get; set; } = 1.0;
        public bool SplitByPeriod { get; set; } = false;
        public int Threshold { get; set; } = 191;
        public int SpeakerId { get; set; } = 2;
        public int MaxLengthOfLine { get; set; } = 80;

        public CaptureArea CaptureArea { get; set; } = new CaptureArea();

        public Notification Notification { get; set; } = new Notification();

        public List<Shortcut> Shortucts { get; set; } = new List<Shortcut>();


        public WindowSettings WindowSettings { get; set; } = new WindowSettings();

        private static string _settingsFilePath = "appsettings.json";

        private AppSettings() { }

        public static AppSettings Instance => _instance.Value;

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(_settingsFilePath, json);
        }

        private static AppSettings LoadSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            return new AppSettings();
        }
    }

    public class WindowSettings
    {
        public double Top { get; set; } = 0;
        public double Left { get; set; } = 0;
        public double Width { get; set; } = 800;
        public double Height { get; set; } = 600;
        public WindowState WindowState { get; set; } = WindowState.Normal;
    }

    public enum TargetedWindow
    {
        Foreground,
        Genshin
    }

    public class CaptureArea
    {
        public double Left { get; set; } = 0;
        public double Top { get; set; } = 0;
        public double Width { get; set; } = 100;
        public double Height { get; set; } = 100;

        public override string ToString()
        {
            return $"left: {Left:F2}, top: {Top:F2}, width: {Width:F2}, height: {Height:F2}";
        }
    }

    public class Preset
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int Threshold { get; set; }

        public CaptureArea CaptureArea { get; set; } = new CaptureArea();

        public override string ToString()
        {
            return Name;
        }

    }

    public class Shortcut
    {
        public int Id { get; set; }

        public AppFeatures Kind { get; set; } = AppFeatures.RunWithPreset;

        public bool KeyCancel { get; set; } = true;

        public ModifierKey ModifierKey { get; set; } = new ModifierKey();

        public Keys Key { get; set; }
    }

    public class ModifierKey
    {
        public bool Alt { get; set; } // Keys.LMenu or Keys.RMenu
        public bool Control { get; set; } // Keys.LControlKey or Keys.RControlKey
        public bool Shift { get; set; } // Keys.LShiftKey or Keys.RShiftKey

        public int Count()
        {
            var i = 0;
            if (Alt) i++;
            if (Control) i++;
            if (Shift) i++;
            return i;
        }

        public bool IsPressed()
        {
            var alt = !Alt || InputStateManager.Status[Keys.LMenu] || InputStateManager.Status[Keys.RMenu];
            var control = !Control || InputStateManager.Status[Keys.LControlKey] || InputStateManager.Status[Keys.RControlKey];
            var shift = !Shift || InputStateManager.Status[Keys.LShiftKey] || InputStateManager.Status[Keys.RShiftKey];
            return alt && control && shift;
        }
    }

    public enum AppFeatures
    {
        Run = 0,
        RunWithClipboard = 1,
        RunWithPreset = 2,
        ToggleEnabled = 3,
    }

    public class Notification
    {
        public bool NotifyToggleEnabled { get; set; } = true;
    }
}
