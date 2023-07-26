using FileViewer.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Browser
{
    public class PluginBrowser : IPlugin
    {
        IManager? _manager;
        private WebView2.WebView2? instance;
        private readonly object lockObject = new();

        public PluginBrowser()
        {
            WebView2.WebView2.LoadWebView2();
        }

        public UserControl GetUserControl(IManager manager)
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        _manager = manager;
                        instance = new WebView2.WebView2();
                    }
                }
            }
            return instance;
        }

        public void ChangeFile(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            Uri? uri = null;
            if (extension == ".url")
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line.ToLower().StartsWith("url="))
                    {
                        uri = new Uri(line[4..]);
                        break;
                    }
                }
            }
            else
            {
                uri = new Uri(filePath);
            }
            if (uri == null)
            {
                _manager?.LoadFileFailed(filePath);
                return;
            }
            if (instance != null)
            {
                instance.WebView2Instance.Source = uri;
            }
            _manager?.SetResizeMode(true);
            ChangeTheme(_manager?.IsDarkMode() ?? false);
            _manager?.SetLoading(false);
        }

        public void ChangeTheme(bool dark)
        {
            _manager?.SetColor(dark ? Utils.BackgroundDark : Utils.BackgroundLight);
        }

        public void Cancel()
        {
        }

        public bool Available => WebView2.WebView2.Available;

        public bool SupportedDirectory => false;

        public IEnumerable<string> SupportedExtensions => new string[] { ".url", ".html" };

        public string Description => "Preview html, rely on microsoft edge";

        public string PluginName => "Browser";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.webview2);
    }
}
