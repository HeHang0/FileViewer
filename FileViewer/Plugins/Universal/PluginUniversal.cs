using FileViewer.Base;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Universal
{
    class PluginUniversal : IPlugin
    {
        private UserControl? instance;
        private readonly object lockObject = new();

        Universal? _model;

        public IEnumerable<string> SupportedExtensions => new string[] { "*" };

        public bool Available => true;

        public bool SupportedDirectory => true;

        public string Description => "Show directory or file size";

        public string PluginName => "Universal";

        public ImageSource? Icon => Utils.GetBitmapSource(Utils.ImagePreview);

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

        public UserControl GetUserControl(IManager manager)
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new UniversalControl();
                        _model = new Universal(manager);
                        instance.DataContext = _model;
                    }
                }
            }
            return instance;
        }
    }
}
