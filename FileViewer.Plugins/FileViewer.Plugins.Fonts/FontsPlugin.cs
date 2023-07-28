using FileViewer.Base;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Fonts
{
    public class FontsPlugin : IPlugin
    {
        FontsControl? _instance;
        readonly object lockObject = new();

        public UserControl GetUserControl(IManager manager)
        {
            if (_instance == null)
            {
                lock (lockObject)
                {
                    _instance ??= new FontsControl(manager);
                }
            }
            return _instance;
        }

        public IEnumerable<string> SupportedExtensions => new string[] { ".ttf" };

        public bool Available => true;

        public bool SupportedDirectory => false;

        public string PluginName => "FontFamily";

        public string Description => "Preview font file, support ttf";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.font);

        public void Cancel()
        {
        }

        public void ChangeFile(string filePath)
        {
            _instance?.ChangeFile(filePath);
        }

        public void ChangeTheme(bool dark)
        {
            _instance?.ChangeTheme(dark);
        }
    }
}
