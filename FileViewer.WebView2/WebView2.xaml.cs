using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows.Controls;

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
            timeout.SetTimeout(DisposeWebView, TimeSpan.FromSeconds(10));
        }

        private void DisposeWebView()
        {
            Dispatcher.Invoke(() =>
            {
                _webview2?.CoreWebView2.Profile.ClearBrowsingDataAsync().ContinueWith(x =>
                {
                    Content = null;
                    _webview2?.Dispose();
                    _webview2 = null;
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
                            Content = _webview2;
                            _webview2.WebMessageReceived += OnWebMessageReceived;
                        }
                    }
                }
                return _webview2;
            }
        }

        private static async void InitWebView2(Microsoft.Web.WebView2.Wpf.WebView2? _webview2 = null)
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
            WebMessageReceived?.Invoke(this, e);
        }

        public static bool Available => _coreEnvironment != null;
    }
}