using FileViewer.Base;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Pdf
{
    public class PluginPdf : IPlugin
    {
        IManager? _manager;
        private WebView2.WebView2? instance;
        private readonly object lockObject = new();

        public PluginPdf()
        {
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
            if (instance != null)
            {
                instance.WebView2Instance.Source = new Uri(filePath);
            }
            _manager?.SetResizeMode(true);
            ChangeTheme(_manager?.IsDarkMode() ?? false);
            _manager?.SetLoading(false);
        }

        public void ChangeTheme(bool dark)
        {
            _manager?.SetColor(dark ? Color.FromRgb(0x33, 0x33, 0x33) : Color.FromRgb(0xF7, 0xF7, 0xF7));
        }

        public void Cancel()
        {
        }

        public bool Available => WebView2.WebView2.Available;

        public bool SupportedDirectory => false;

        public IEnumerable<string> SupportedExtensions => new string[] { ".pdf", ".svg", ".gif" };

        public string Description => "Preview pdf, rely on microsoft edge";

        public string PluginName => "Pdf";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.pdf);
    }
}
