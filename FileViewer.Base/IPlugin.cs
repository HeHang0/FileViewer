using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Base
{
    public interface IPlugin
    {
        IEnumerable<string> SupportedExtensions { get; }
        bool Available { get; }
        bool SupportedDirectory { get; }
        string PluginName { get; }
        string Description { get; }
        ImageSource? Icon { get; }
        void ChangeFile(string filePath);
        void ChangeTheme(bool dark);
        void Cancel();
        UserControl GetUserControl(IManager manager);
    }
}
