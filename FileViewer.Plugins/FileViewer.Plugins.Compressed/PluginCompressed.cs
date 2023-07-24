using FileViewer.Base;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Compressed
{
    public class PluginCompressed : IPlugin
    {
        Compressed? _model;
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
                        instance = new CompressedControl();
                        _model = new Compressed(manager);
                        instance.DataContext = _model;
                    }
                }
            }
            return instance;
        }

        public void Cancel()
        {
            _model?.Cancel();
        }

        public void ChangeFile(string filePath)
        {
            _model?.ChangeFile(filePath);
        }

        public void ChangeTheme(bool dark)
        {
            _model?.ChangeTheme(dark);
        }

        public bool Available => true;

        public bool SupportedDirectory => false;

        public IEnumerable<string> SupportedExtensions => new string[]
        {
            ".zip", ".rar", ".tar", ".gz", ".7z"
        };

        public string Description => "Preview compressed files, support zip,rar,7z,gz,tar";

        public string PluginName => "Compressed";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.zip);
    }
}
