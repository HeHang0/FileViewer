using FileViewer.Base;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Hello
{
    class PluginHello : IPlugin
    {
        private HelloControl? instance;
        private static readonly object lockObject = new();

        public UserControl GetUserControl(IManager manager)
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance ??= new HelloControl(manager);
                }
            }
            return instance;
        }

        public IEnumerable<string> SupportedExtensions => Array.Empty<string>();

        public bool Available => true;

        public bool SupportedDirectory => false;

        public string Description => string.Empty;

        public string PluginName => "Hello";

        public ImageSource? Icon => Utils.GetBitmapSource(Utils.ImagePreview);

        public void ChangeFile(string filePath) { }
        public void ChangeTheme(bool dark)
        {
            instance?.ChangeTheme(dark);
        }

        public void Cancel()
        {
        }
    }
}
