using Newtonsoft.Json;
using OpenCvSharp.Extensions;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Diagnostics;

namespace GenshinJPTextSpeaker
{
    public static class Utility
    {
        public static async Task<OcrResult> RecognizeText(SoftwareBitmap snap)
        {
            var ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
            var ocrResult = await ocrEngine.RecognizeAsync(snap);
            return ocrResult;
        }

        public static async Task<SoftwareBitmap> GetSoftwareBitmap(Bitmap bitmap)
        {
            SoftwareBitmap softwareBitmap = null;

            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }

            return softwareBitmap;
        }

        public static Bitmap CaptureScreen(int left, int top, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            using Graphics g = Graphics.FromImage(bitmap);
            var l = (int)(left * 1.25);
            var t = (int)(top * 1.25);
            g.CopyFromScreen(left, top, 0, 0, bitmap.Size);
            return bitmap;
        }

        // 参考：https://stackoverflow.com/questions/891345/get-a-screenshot-of-a-specific-application
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out Win32Api.Rect lpRect);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        public static Bitmap PrintWindow(IntPtr hwnd)
        {
            var window = WindowController.CreateInstance(hwnd);
            var (_, _, width, height) = window.Bounds.ToTuple();
            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics gfxBmp = Graphics.FromImage(bmp);
            IntPtr hdcBitmap = gfxBmp.GetHdc();

            PrintWindow(hwnd, hdcBitmap, 0);

            gfxBmp.ReleaseHdc(hdcBitmap);
            gfxBmp.Dispose();

            return bmp;
        }

        public static Bitmap ColorFilter(Bitmap bitmap, int max, int thre)
        {
            var img = bitmap.ToMat();
            Cv2.CvtColor(img, img, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(img, img, thre, max, ThresholdTypes.Tozero);
            return img.ToBitmap();
        }

        // 参考：https://www.nuits.jp/entry/2016/10/17/181232
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource ToImageSource(this Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        // 参考：https://qiita.com/oyahun/items/e01e56878dc011cdc094
        public static async Task<Stream> GenerateAudioStream(GenerateAudioOptions options)
        {
            using var httpClient = new HttpClient();
            AudioQuery query;

            // 音声クエリを生成
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), $"http://localhost:50021/audio_query?text={options.Text}&speaker={options.SpeakerId}"))
            {
                request.Headers.TryAddWithoutValidation("accept", "application/json");
                request.Content = new StringContent("");
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                var response = await httpClient.SendAsync(request);
                var queryStr = await response.Content.ReadAsStringAsync();
                query = JsonConvert.DeserializeObject<AudioQuery>(queryStr);
                query.speedScale = options.Speed;
            }

            // 音声クエリから音声合成
            using (var request = new HttpRequestMessage(new HttpMethod("POST"), $"http://localhost:50021/synthesis?speaker={options.SpeakerId}&enable_interrogative_upspeak=true"))
            {
                request.Headers.TryAddWithoutValidation("accept", "audio/wav");
                request.Content = new StringContent(JsonConvert.SerializeObject(query));
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = await httpClient.SendAsync(request);

                // 音声を保存
                //using var fileStream = System.IO.File.Create("audio.wav");
                var stream = await response.Content.ReadAsStreamAsync();
                //httpStream.CopyTo(fileStream);
                //fileStream.Flush();
                return stream;
            }
        }

        public static async Task<List<Style>> FetchSpeakers()
        {
            var wc = new HttpClient();
            var response = await wc.GetStringAsync("http://localhost:50021/speakers");
            var array = JsonConvert.DeserializeObject<Class1[]>(response);
            var styles = new List<Style>();

            foreach (var data in array)
            {
                var name = data.name.ToString();
                foreach (var style in data.styles)
                {
                    style.name = $"{data.name}({style.name})";
                    styles.Add(style);
                }
            }

            return styles;
        }

        public static async void QuickSpeak(string text, int speakerId, double speed)
        {
            var section = new SectionViewModel(text);
            var cts = new CancellationTokenSource();
            await section.Play(new PlayOptions
            {
                SpeakerId = speakerId,
                Speed = speed
            },
            cts.Token);
        }
    }

    public class GenerateAudioOptions : PlayOptions
    {
        public string Text { get; set; }
    }

    public class PlayOptions
    {
        public int SpeakerId { get; set; } = 3;

        public double Speed { get; set; } = 1.0;
    }
}
