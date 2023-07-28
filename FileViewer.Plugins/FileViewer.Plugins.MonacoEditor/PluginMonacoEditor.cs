using FileViewer.Base;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.MonacoEditor
{
    public class PluginMonacoEditor : BackgroundWorkBase, IPlugin
    {
        public const string LocalMonacoVirtualHostName = "FileViewerLocalMonaco";
        IManager? _manager;
        private WebView2.WebView2? _instance;
        private readonly object lockObject = new();

        public PluginMonacoEditor()
        {
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
                        _instance.CoreWebView2InitializationCompleted += OnCoreWebView2InitializationCompleted;
                        _instance.WebMessageReceived += OnWebMessageReceived;
                    }
                }
            }
            return _instance;
        }

        private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            switch (message)
            {
                case "failed":
                    _instance?.Dispatcher.Invoke(() =>
                    {
                        _manager?.LoadFileFailed(_filePath!);
                    });
                    break;
            }
        }

        private void OnCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            InitCoreWebView2();
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
            InitCoreWebView2();
            _manager?.SetLoading(false);
        }

        private void InitCoreWebView2()
        {
            if (_instance == null || _instance?.WebView2Instance.CoreWebView2 == null) return;
            _instance.WebView2Instance.CoreWebView2.SetVirtualHostNameToFolderMapping(
                LocalMonacoVirtualHostName, MonacoAssetsDirectory,
                CoreWebView2HostResourceAccessKind.Allow);
            var data = new Dictionary<string, string>()
            {
                {"message", "load-file" },
                {"value", _filePath ?? string.Empty },
            };
            _instance.WebView2Instance.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(data));
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

        protected override void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            if (_instance == null) return;
            var htmlPath = Path.Combine(Path.GetTempPath(), "WebView2", "monaco_editor.html");
            if (!File.Exists(htmlPath))
            {
            }
                File.WriteAllText(htmlPath, Properties.Resources.monaco_editor);
            ExtractJS();
            e.Result = htmlPath;
        }

        protected override void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (_instance == null) return;
            string targetUrl = (string)e.Result!;
            UriBuilder uriBuilder = new(targetUrl)
            {
                Query = "file=" + System.Net.WebUtility.UrlEncode(_filePath)
            };
            if (_manager?.IsDarkMode() ?? false)
            {
                uriBuilder.Query += "&dark=1";
            }
            _instance.WebView2Instance.Source = uriBuilder.Uri;
            if (_instance.WebView2Instance.CoreWebView2 != null)
            {
                InitCoreWebView2();
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
            ".gitattributes", ".rc", ".xml", ".log", ".py", ".java", ".c", ".aml",
            ".cpp", ".cc", ".less", ".kt", ".php", ".ts", ".ps", ".ps1", ".yaml"
        };

        public string Description => "Preview text with monaco editor";

        public string PluginName => "MonacoEditor";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.vscode);
    }
}
