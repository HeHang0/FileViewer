using FileViewer.Base;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Text
{
    public class PluginText : IPlugin
    {
        private TextControl? instance;
        private readonly object lockObject = new();

        public UserControl GetUserControl(IManager manager)
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance ??= new TextControl(manager);
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
            ".txt", ".cs", ".go", ".js", ".json", ".vue", ".sql", ".html", ".plist",
            ".bat", ".css", ".md", ".bash", ".sh", ".gitignore", ".swift", ".xaml",
            ".gitattributes", ".rc", ".xml", ".log", ".py", ".java", ".c", ".aml",
            ".cpp", ".cc", ".less", ".kt", ".php", ".ts", ".ps", ".ps1", ".yaml"
        };

        public string Description => "Preview text with avalon editor";

        public string PluginName => "Text";

        public ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.text);
    }
}
