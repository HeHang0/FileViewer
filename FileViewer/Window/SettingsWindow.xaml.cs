using System.Windows;

namespace FileViewer
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(PluginManager model)
        {
            InitializeComponent();
            if (Tools.OSVersionHelper.IsWindows8OrGreater)
            {
                ModernWpf.Controls.Primitives.WindowHelper.SetUseModernWindowStyle(this, true);
            }
            SettingsModel _model = new();
            DataContext = _model;
            _model.Navigating += Navigate;
            _model.InitItems(model);
        }

        private void Navigate(object? sender, object content, object? extraData)
        {
            if (extraData != null)
            {
                NavigationFrame.Navigate(content, extraData);
            }
            else
            {
                NavigationFrame.Navigate(content);
            }
        }
    }
}
