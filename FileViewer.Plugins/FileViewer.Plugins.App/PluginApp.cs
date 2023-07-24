using FileViewer.Base;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.App
{
    public class PluginApp : IPlugin
    {
        App? _model;
        private UserControl? instance;
        private readonly object lockObject = new();

        public UserControl GetUserControl(IManager manager)
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new AppControl();
                        _model = new App(manager);
                        instance.DataContext = _model;
                    }
                }
            }
            return instance;
        }

        public IEnumerable<string> SupportedExtensions => new string[] { ".apk", ".ipa", ".app" };

        public bool Available => true;

        public bool SupportedDirectory => true;

        public string Description => "Preview appliction files, support apk, ipa, macos app";

        public string PluginName => "Application";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.app);

        public void ChangeFile(string filePath)
        {
            _model?.ChangeFile(filePath);
        }

        public void ChangeTheme(bool dark)
        {
            _model?.ChangeTheme(dark);
        }

        public void Cancel()
        {
            _model?.Cancel();
        }
    }
}
