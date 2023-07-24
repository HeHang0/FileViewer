using FileViewer.Base;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Image
{
    public class PluginImage : IPlugin
    {
        private ImageControl? instance;
        private readonly object lockObject = new();

        public UserControl GetUserControl(IManager manager)
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance ??= new ImageControl(manager);
                }
            }
            return instance;
        }

        public void ChangeFile(string filePath)
        {
            instance?.ChangeFile(filePath);
        }

        public void ChangeTheme(bool dark)
        {
            instance?.ChangeTheme(dark);
        }

        public void Cancel()
        {
        }

        public bool Available => true;

        public bool SupportedDirectory => false;

        public IEnumerable<string> SupportedExtensions => new string[]
        {
            ".jpg", ".png", ".jpeg", ".bmp", ".ico",
            ".wdp", ".jxr", ".jif", ".tiff"
        };

        public string Description => "Preview image file, support img,gif,svg...";

        public string PluginName => "Image";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.img);
    }
}
