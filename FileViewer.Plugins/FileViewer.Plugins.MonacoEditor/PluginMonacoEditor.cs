using FileViewer.Base;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.MonacoEditor
{
    public class PluginMonacoEditor : IPlugin
    {
        IManager? manager;
        private WebView2.WebView2? instance;
        private readonly object lockObject = new();
        private bool cloudflareOK = false;

        public PluginMonacoEditor()
        {
            WebView2.WebView2.LoadWebView2();
            CheckNetOK();
        }

        private async void CheckNetOK()
        {
            cloudflareOK = await Tools.Http.CheckUrlOK("https://cdnjs.cloudflare.com");
        }

        public UserControl GetUserControl(IManager manager)
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        this.manager = manager;
                        instance = new WebView2.WebView2();
                        instance.WebMessageReceived += WebMessageReceived;
                    }
                }
            }
            return instance;
        }

        private string? filePath;
        public void ChangeFile(string filePath)
        {
            this.filePath = filePath;
            if (instance != null)
            {
                var htmlPath = Path.Combine(Path.GetTempPath(), "WebView2", "monaco_editor.html");
                if (!File.Exists(htmlPath))
                {
                    File.WriteAllText(htmlPath, Properties.Resources.monaco_editor);
                }
                instance.WebView2Instance.Source = new Uri(htmlPath);
            }
            manager?.SetResizeMode(true);
            ChangeTheme(manager?.IsDarkMode() ?? false);
        }

        public void ChangeTheme(bool dark)
        {
            manager?.SetColor(dark ? Color.FromRgb(0x1E, 0x1E, 0x1E) : Color.FromRgb(0xFF, 0xFF, 0xFE));
        }

        public void Cancel()
        {
        }

        private static readonly long MAX_TEXT_LENGTH = 10485760;
        private void ShowTextWithWebView2(string filePath)
        {
            if (instance == null) return;
            instance.WebView2Instance.CoreWebView2.PostWebMessageAsJson($"{{\"message\": \"extension\",\"value\": \"{Path.GetExtension(filePath)}\"}}");
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
            instance.WebView2Instance.CoreWebView2.PostWebMessageAsJson(Encoding.UTF8.GetString(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data)));
        }

        private void WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            switch (message)
            {
                case "loaded":
                    ShowTextWithWebView2(filePath ?? string.Empty);
                    manager?.SetLoading(false);
                    break;
            }
        }

        public bool Available => WebView2.WebView2.Available && cloudflareOK;

        public bool SupportedDirectory => false;

        public IEnumerable<string> SupportedExtensions => new string[]
        {
            ".txt", ".cs", ".go", ".js", ".json", ".vue", ".sql", ".html", ".plist",
            ".bat", ".css", ".md", ".bash", ".sh", ".gitignore", ".swift", ".xaml",
            ".gitattribute", ".rc", ".xml", ".log", ".py", ".java", ".c", ".aml",
            ".cpp", ".cc", ".less", ".kt", ".php", ".ts", ".ps", ".ps1", ".yaml"
        };

        public string Description => "Preview text with monaco editor";

        public string PluginName => "MonacoEditor";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.vscode);
    }
}
