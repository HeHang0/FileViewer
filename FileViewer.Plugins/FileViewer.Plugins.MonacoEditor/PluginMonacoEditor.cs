using FileViewer.Base;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.MonacoEditor
{
    public class PluginMonacoEditor : BackgroundWorkBase, IPlugin
    {
        public const string VirtualHostName = "FileViewerLocalMonaco";
        IManager? _manager;
        private WebView2.WebView2? _instance;
        private readonly object lockObject = new();

        public PluginMonacoEditor()
        {
            WebView2.WebView2.LoadWebView2();
        }

        public UserControl GetUserControl(IManager manager)
        {
            if (_instance == null)
            {
                lock (lockObject)
                {
                    if (_instance == null)
                    {
                        _manager = manager;
                        _instance = new WebView2.WebView2();
                        _instance.WebMessageReceived += WebMessageReceived;
                    }
                }
            }
            return _instance;
        }

        private string? _filePath;
        public void ChangeFile(string filePath)
        {
            _filePath = filePath;
            InitBackGroundWork();
            bgWorker?.RunWorkerAsync(filePath);
            _manager?.SetResizeMode(true);
            ChangeTheme(_manager?.IsDarkMode() ?? false);
        }

        private void CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (sender is Microsoft.Web.WebView2.Wpf.WebView2 webview2)
            {
                webview2.CoreWebView2InitializationCompleted -= CoreWebView2InitializationCompleted;
            }
            InitCoreWebView2();
        }

        private void InitCoreWebView2()
        {
            _instance?.WebView2Instance?.EnsureCoreWebView2Async().ContinueWith(task =>
            {
                _instance.Dispatcher.Invoke(() =>
                {
                    _instance?.WebView2Instance.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        VirtualHostName, MonacoAssetsDirectory,
                        CoreWebView2HostResourceAccessKind.Allow);
                    _instance?.WebView2Instance.CoreWebView2.PostWebMessageAsJson(
                        $"{{\"message\": \"load-file\"}}");
                    _manager?.SetLoading(false);
                });
            });
        }

        private static void ExtractJS()
        {
            if (Directory.Exists(MonacoAssetsDirectory)) return;
            using var stream = new MemoryStream(Properties.Resources.monacojs);
            Tools.File.UnZip(stream, MonacoAssetsDirectory);
        }

        private static string MonacoAssetsDirectory => Path.Combine(Path.GetTempPath(), "WebView2", "monacojs");

        public void ChangeTheme(bool dark)
        {
            _manager?.SetColor(dark ? Color.FromRgb(0x1E, 0x1E, 0x1E) : Color.FromRgb(0xFF, 0xFF, 0xFE));
            _instance?.WebView2Instance.CoreWebView2?.PostWebMessageAsJson(
                $"{{\"message\": \"theme\",\"value\": \"{(dark ? "dark" : "light")}\"}}");
        }

        private static readonly long MAX_TEXT_LENGTH = 10485760;
        private void ShowTextWithWebView2(string filePath)
        {
            if (_instance == null) return;
            _instance.WebView2Instance.CoreWebView2.PostWebMessageAsJson(
                $"{{\"message\": \"extension\",\"value\": \"{Path.GetExtension(filePath)}\"}}");
            string content;
            var fileInfo = new FileInfo(filePath);
            var buffer = new char[Math.Min(MAX_TEXT_LENGTH, fileInfo.Length)];

            using (StreamReader st = new(filePath, true))
            {
                int length = st.ReadBlock(buffer, 0, buffer.Length);
                content = new string(buffer.Take(length).ToArray());
            }
            Dictionary<string, string> data = new()
            {
                { "message", "text" },
                { "value", content }
            };
            _instance.WebView2Instance.CoreWebView2.PostWebMessageAsJson(Encoding.UTF8.GetString(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data)));
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            switch (message)
            {
                case "loaded":
                    ShowTextWithWebView2(_filePath ?? string.Empty);
                    break;
            }
        }

        protected override void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (_instance != null)
            {
                var htmlPath = Path.Combine(Path.GetTempPath(), "WebView2", "monaco_editor.html");
                //if (!File.Exists(htmlPath))
                //{
                //    File.WriteAllText(htmlPath, Properties.Resources.monaco_editor);
                //}
                File.WriteAllText(htmlPath, Properties.Resources.monaco_editor);
                ExtractJS();
                e.Result = htmlPath;
            }
        }

        protected override void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (_instance == null) return;
            string targetUrl = (string)e.Result!;
            UriBuilder uriBuilder = new(targetUrl);
            if (_manager?.IsDarkMode() ?? false)
            {
                uriBuilder.Query = "dark=1";
            }
            _instance.WebView2Instance.Source = uriBuilder.Uri;
            if (_instance.WebView2Instance.CoreWebView2 != null)
            {
                InitCoreWebView2();
            }
            else
            {
                _instance.WebView2Instance.CoreWebView2InitializationCompleted += CoreWebView2InitializationCompleted;
            }
        }

        protected override void BgWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
        }

        public bool Available => WebView2.WebView2.Available;

        public bool SupportedDirectory => false;

        public IEnumerable<string> SupportedExtensions => new string[]
        {
            ".txt", ".cs", ".go", ".js", ".json", ".vue", ".sql", ".html", ".plist",
            ".bat", ".css", ".md", ".bash", ".sh", ".gitignore", ".swift", ".xaml",
            ".gitattribute", ".rc", ".xml", ".log", ".py", ".java", ".c", ".aml",
            ".cpp", ".cc", ".less", ".kt", ".php", ".ts", ".ps", ".ps1", ".yaml", ".gitattributes"
        };

        public string Description => "Preview text with monaco editor";

        public string PluginName => "MonacoEditor";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.vscode);
    }
}
