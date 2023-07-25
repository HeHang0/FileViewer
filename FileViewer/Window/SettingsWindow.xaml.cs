using FileViewer.Base;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static FileViewer.SettingsWindow;

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
            if (OSVersionHelper.IsWindows8OrGreater)
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
            if(extraData != null)
            {
                NavigationFrame.Navigate(content, extraData);
            }
            else
            {
                NavigationFrame.Navigate(content);
            }
        }

        //public FrameworkElement? FindElementByName(DependencyObject parent, string name)
        //{
        //    if (parent == null)
        //        return null;

        //    int childCount = VisualTreeHelper.GetChildrenCount(parent);
        //    for (int i = 0; i < childCount; i++)
        //    {
        //        DependencyObject child = VisualTreeHelper.GetChild(parent, i);
        //        if (child is FrameworkElement element && element.Name == name)
        //            return element;

        //        FrameworkElement? foundElement = FindElementByName(child, name);
        //        if (foundElement != null)
        //            return foundElement;
        //    }

        //    return null;
        //}
    }
}
