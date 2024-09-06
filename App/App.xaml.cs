using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Windows;

namespace GenshinJPTextSpeaker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        const string _presetsFileName = "presets.json";
        const string _presetsResourecPath = "GenshinJPTextSpeaker.presets.json";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // プリセットファイルが無い場合は生成
            if (!File.Exists(_presetsFileName))
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_presetsResourecPath);
                using var fs = File.Create(_presetsFileName);
                stream?.CopyTo(fs);
            }

            // プリセットのロード
            var json = File.ReadAllText(_presetsFileName);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var list = JsonConvert.DeserializeObject<List<Preset>>(json);
                if (list != null)
                {
                    AppSettings.Presets = list;
                }
            }
        }
    }
}
