using FileViewer.Base;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Office
{
    public class PluginOffice : IPlugin
    {
        Office? _model;
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
                        instance = new OfficeControl();
                        _model = new Office(manager);
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

        private bool _installed = false;

        private static readonly string WORD_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Winword.exe";

        public bool Available
        {
            get
            {
                if (_installed) return true;
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(WORD_KEY))
                {
                    _installed = key != null;
                }
                return _installed;
            }
        }

        public bool SupportedDirectory => false;

        public IEnumerable<string> SupportedExtensions => new string[]
        {
            ".doc", ".docx",
            ".xls", ".xlsx",
            ".ppt", ".pptx",
            ".xps"
        };

        public string Description => "Preview office files, support word, excel, ppt";

        public string PluginName => "Office";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.office);
    }
}
