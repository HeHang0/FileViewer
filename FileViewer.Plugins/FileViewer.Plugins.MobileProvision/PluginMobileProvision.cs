using FileViewer.Base;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.MobileProvision
{
    public class PluginMobileProvision : IPlugin
    {
        MobileProvision? _model;
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
                        instance = new MobileProvisionControl();
                        _model = new MobileProvision(manager);
                        instance.DataContext = _model;
                    }
                }
            }
            return instance;
        }

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

        public bool Available => true;

        public bool SupportedDirectory => false;

        public IEnumerable<string> SupportedExtensions => new string[]
        {
            ".mobileprovision"
        };

        public string Description => "Preview apple mobileprovision";

        public string PluginName => "MobileProvision";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.cert);
    }
}
