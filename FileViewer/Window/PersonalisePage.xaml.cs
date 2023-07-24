using FileViewer.Base;
using ModernWpf;
using System.Windows;
using System.Windows.Controls;

namespace FileViewer
{
    /// <summary>
    /// PersonalisePage.xaml 的交互逻辑
    /// </summary>
    public partial class PersonalisePage : Page
    {
        public PersonalisePage()
        {
            InitializeComponent();
            InitTheme();
        }

        private void InitTheme()
        {
            switch (Settings.Theme)
            {
                case ApplicationTheme.Dark:
                    ThemeDarkMode.IsChecked = true; break;
                case ApplicationTheme.Light:
                    ThemeLightMode.IsChecked = true; break;
                default:
                    ThemeSystemMode.IsChecked = true; break;
            }
        }

        private void OnThemeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            RadioButton? eb = e.AddedItems[0] as RadioButton;
            ApplicationTheme? theme = (eb?.Tag) switch
            {
                "Dark" => (ApplicationTheme?)ApplicationTheme.Dark,
                "Light" => (ApplicationTheme?)ApplicationTheme.Light,
                _ => null,
            };
            Settings.Theme = theme;
            if (theme != ThemeManager.Current.ApplicationTheme)
            {
                ThemeManager.Current.ApplicationTheme = theme;
            }
            Settings.SaveTheme();
        }

        private void OpenSystemTheme(object sender, RoutedEventArgs e)
        {
            if (OSVersionHelper.IsWindows7OrLess)
            {
                Tools.File.ProcessStart("control.exe", "/name Microsoft.Personalization");
            }
            else
            {
                Tools.File.ProcessStart("ms-settings:personalization-colors");
            }
        }
    }
}
