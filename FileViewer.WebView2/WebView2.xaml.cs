using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace FileViewer.WebView2
{
    /// <summary>
    /// Interaction logic for WebView2.xaml
    /// </summary>
    public partial class WebView2 : UserControl
    {
        private static bool webView2LoaderLoaded = false;
        private static bool _available = false;
        private Microsoft.Web.WebView2.Wpf.WebView2? _webview2;
        private readonly object lockObject = new();
        readonly Tools.Timeout timeout = new();
        public event EventHandler<CoreWebView2WebMessageReceivedEventArgs>? WebMessageReceived;

        public WebView2()
        {
            InitializeComponent();
            Unloaded += WebView2_Unloaded;
        }

        private void WebView2_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            timeout.SetTimeout(DisposeWebView, TimeSpan.FromSeconds(10));
        }

        private void DisposeWebView()
        {
            Dispatcher.Invoke(() =>
            {
                Content = null;
                _webview2?.Dispose();
                _webview2 = null;
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
                            InitWebView2(Path.Combine(Path.GetTempPath(), "WebView2"));
                            Content = _webview2;
                            _webview2.WebMessageReceived += OnWebMessageReceived;
                        }
                    }
                }
                return _webview2;
            }
        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            WebMessageReceived?.Invoke(this, e);
        }

        public static bool Available => _available;

        private async void InitWebView2(string userDataFolder)
        {
            try
            {
                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
                _webview2?.EnsureCoreWebView2Async(environment);
            }
            catch (Exception)
            {
            }
        }

        public static async void LoadWebView2()
        {
            if (webView2LoaderLoaded) return;
            webView2LoaderLoaded = true;
            string webView2Path = Path.Combine(Path.GetTempPath(), "WebView2");
            CoreWebView2Environment.SetLoaderDllFolderPath(webView2Path);
            var loaderPath = Path.Combine(webView2Path, "WebView2Loader.dll");
            if (!Directory.Exists(webView2Path))
            {
                Directory.CreateDirectory(webView2Path);
            }
            var architecture = RuntimeInformation.ProcessArchitecture;
            var architectureName = architecture.ToString().ToLower();
            var architecturePath = loaderPath + ".architecture";
            string architectureText = string.Empty;
            try
            {
                architectureText = File.ReadAllText(architecturePath);
            }
            catch (Exception)
            {
            }
            if (!File.Exists(loaderPath) ||
                !File.Exists(architecturePath) ||
                architectureText != architectureName)
            {
                try
                {
                    File.WriteAllText(architecturePath, architectureName);
                    switch (architecture)
                    {
                        case Architecture.X86:
                            File.WriteAllBytes(loaderPath, Properties.Resources.WebView2Loader_x86);
                            break;
                        case Architecture.X64:
                            File.WriteAllBytes(loaderPath, Properties.Resources.WebView2Loader_x64);
                            break;
                        case Architecture.Arm64:
                            File.WriteAllBytes(loaderPath, Properties.Resources.WebView2Loader_arm64);
                            break;
                    }
                }
                catch (Exception)
                {
                }
            }
            try
            {
                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: webView2Path);
                _available = environment != null;
            }
            catch (Exception)
            {
            }
        }
    }
}