using FileViewer.Base;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Fonts
{
    public class FontsPlugin : IPlugin
    {
        FontsControl? _instance;
        Fonts? _model;
        readonly object lockObject = new();

        public UserControl GetUserControl(IManager manager)
        {
            if (_instance == null)
            {
                lock (lockObject)
                {
                    _model = new Fonts(manager);
                    _instance ??= new FontsControl(_model);
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
            _model?.ChangeFile(filePath);
        }

        public void ChangeTheme(bool dark)
        {
            _model?.ChangeTheme(dark);
        }
    }
}
