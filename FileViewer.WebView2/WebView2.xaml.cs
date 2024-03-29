using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Ude;

namespace FileViewer.WebView2
{
    /// <summary>
    /// Interaction logic for WebView2.xaml
    /// </summary>
    public partial class WebView2 : UserControl
    {
        private Microsoft.Web.WebView2.Wpf.WebView2? _webview2;
        private readonly object lockObject = new();
        private static CoreWebView2Environment? _coreEnvironment;
        readonly Tools.Timeout timeout = new();
        public event EventHandler<CoreWebView2WebMessageReceivedEventArgs>? WebMessageReceived;
        public event EventHandler<CoreWebView2InitializationCompletedEventArgs>? CoreWebView2InitializationCompleted;

        public WebView2()
        {
            InitializeComponent();
            Unloaded += WebView2_Unloaded;
        }
        static WebView2()
        {
            InitWebView2();
        }

        private void WebView2_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            timeout.SetTimeout(DisposeWebView, TimeSpan.FromMinutes(1));
        }

        private void DisposeWebView()
        {
            Dispatcher.Invoke(() =>
            {
                _webview2?.CoreWebView2.Profile.ClearBrowsingDataAsync().ContinueWith(x =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Content = null;
                        _webview2?.Dispose();
                        _webview2 = null;
                    });
                });
            });
        }

        public Microsoft.Web.WebView2.Wpf.WebView2 WebView2Instance
        {
            get
            {
                timeout.ClearTimeout();
                if (_webview2 == null)
                {
                    lock (lockObject)
                    {
                        if (_webview2 == null)
                        {
                            _webview2 = new Microsoft.Web.WebView2.Wpf.WebView2();
                            _webview2.EnsureCoreWebView2Async(_coreEnvironment);
                            _webview2.CoreWebView2InitializationCompleted += OnCoreWebView2InitializationCompleted;
                            _webview2.WebMessageReceived += OnWebMessageReceived;
                            Content = _webview2;
                        }
                    }
                }
                return _webview2;
            }
        }

        public class FileAccessor
        {
#pragma warning disable CA1822 // 将成员标记为 static
            static FileAccessor()
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }

            public async Task<object> ReadFile(string filePath)
            {
                try
                {
                    Encoding encoding = Encoding.UTF8;

                    using (var fs = File.OpenRead(filePath))
                    {
                        var detector = new CharsetDetector();
                        detector.Feed(fs);
                        detector.DataEnd();

                        if (detector.Charset != null)
                        {
                            try
                            {
                                encoding = Encoding.GetEncoding(detector.Charset);
                            }
                            catch (ArgumentException)
                            {
                            }
                        }
                    }
                    return await File.ReadAllTextAsync(filePath, encoding);
                }
                catch (Exception)
                {
                    return -1;
                }
            }

            public async Task<object> ReadFileLines(string filePath, int startLine = 1, int length = 0)
            {
                try
                {
                    var lines = new StringBuilder();
                    int lineCount = 0;
                    using (var reader = new StreamReader(filePath))
                    {
                        string? line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            lineCount++;
                            if (lineCount >= startLine && (length <= 0 || lineCount < length))
                            {
                                lines.AppendLine(line);
                            }
                            if (length > 0 && lineCount > length)
                            {
                                break;
                            }
                        }
                    }

                    string result = lines.ToString();

                    return lines.ToString();
                }
                catch (Exception)
                {
                    return -1;
                }
            }

            public async Task<object> ReadFileLineCount(string filePath)
            {
                try
                {

                    int lineCount = 0;

                    using (var reader = new StreamReader(filePath))
                    {
                        while (await reader.ReadLineAsync() != null)
                        {
                            lineCount++;
                        }
                    }

                    return lineCount;
                }
                catch (Exception)
                {
                    return -1;
                }
            }

            public long ReadFileSize(string filePath)
            {
                try
                {
                    FileInfo fileInfo = new(filePath);
                    return fileInfo.Length;
                }
                catch (Exception)
                {
                    return -1;
                }
            }
#pragma warning restore CA1822 // 将成员标记为 static
        }

        private void OnCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (_webview2 != null)
            {
                _webview2.CoreWebView2.AddHostObjectToScript("fileAccessor", new FileAccessor());
            }
            CoreWebView2InitializationCompleted?.Invoke(sender, e);
        }

        private static async void InitWebView2()
        {
            try
            {
                string userDataFolder = Path.Combine(Path.GetTempPath(), "WebView2");
                _coreEnvironment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
            }
            catch (Exception)
            {
            }
        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            WebMessageReceived?.Invoke(sender, e);
        }

        public static bool Available => _coreEnvironment != null;
    }
}