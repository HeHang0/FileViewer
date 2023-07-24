using FileViewer.Base;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.Windows;
using System.Windows.Media;

namespace FileViewer
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        readonly PluginManagerPage _pluginManagerPage;
        readonly AboutPage _aboutPage;
        readonly SettingsPage _settingsPage;
        readonly PersonalisePage _personalisePage;

        public SettingsWindow(PluginManager model)
        {
            InitializeComponent();
            if (OSVersionHelper.IsWindows8OrGreater) WindowHelper.SetUseModernWindowStyle(this, true);
            _pluginManagerPage = new PluginManagerPage(model);
            _aboutPage = new AboutPage();
            _settingsPage = new SettingsPage();
            _personalisePage = new PersonalisePage();
            ContentFrame.Navigate(_settingsPage);
        }

        private void OnNavigationChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            NavigationView.Header = args.SelectedItemContainer.Content.ToString();
            switch (args.SelectedItemContainer.Tag)
            {
                case "Plugin":
                    ContentFrame.Navigate(_pluginManagerPage); break;
                case "About":
                    ContentFrame.Navigate(_aboutPage); break;
                case "Personalise":
                    ContentFrame.Navigate(_personalisePage); break;
                case "Settings":
                    ContentFrame.Navigate(_settingsPage); break;
            }
        }

        public FrameworkElement? FindElementByName(DependencyObject parent, string name)
        {
            if (parent == null)
                return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is FrameworkElement element && element.Name == name)
                    return element;

                FrameworkElement? foundElement = FindElementByName(child, name);
                if (foundElement != null)
                    return foundElement;
            }

            return null;
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            var element = FindElementByName(NavigationView, "SettingsItem");
            if (element is NavigationViewItem settingsItem) settingsItem.Content = "设置";
        }
    }
}
