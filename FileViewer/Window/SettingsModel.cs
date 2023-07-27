using FileViewer.Base;
using FileViewer.Tools;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FileViewer
{
    public class SettingsModel : INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
        public delegate void NavigatingEventHandler(object? sender, object content, object? extraData);
        public event NavigatingEventHandler? Navigating;

        public class SettingsItem : INotifyPropertyChanged
        {
#pragma warning disable CS0067
            public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
            public SettingsItem(string title, ModernWpf.Controls.Symbol icon, Page page, Thickness? margin = null, bool backgroundVisible = true)
            {
                Title = title;
                Icon = icon;
                Page = page;
                if (margin != null)
                {
                    TitleMargin = margin!;
                }
                else
                {
                    TitleMargin = new(19, 12, 0, 10);
                }
                BackgroundVisible = backgroundVisible;
            }
            public string Title { get; set; } = string.Empty;
            public bool BackgroundVisible { get; private set; } = true;
            public Thickness? TitleMargin { get; set; }
            public double Opacity { get; set; } = 0;
            public ModernWpf.Controls.Symbol? Icon { get; set; }
            internal Page Page { get; set; }
        }

        public static string Title => "设置";

        public static bool UseModernWindowStyle => OSVersionHelper.IsWindows8OrGreater;

        public static ImageSource? Icon => Utils.GetBitmapSource(Properties.Resources.logo);

        public bool IsPaneOpen { get; set; } = false;

        public double Width => IsPaneOpen ? 240 : 50;

        public SettingsItem? _selectedItem;
        public SettingsItem? SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem != null)
                {
                    _selectedItem.Opacity = 0;
                }
                _selectedItem = value;
                if (_selectedItem != null)
                {
                    Navigating?.Invoke(this, _selectedItem.Page, null);
                    _selectedItem.Opacity = 1;
                }
            }
        }

        public ObservableCollection<SettingsItem>? SettingsItems { get; set; }

        public void InitItems(PluginManager model)
        {
            SettingsItems = new ObservableCollection<SettingsItem>()
            {
                new SettingsItem("常规", ModernWpf.Controls.Symbol.Setting, new SettingsPage()),
                new SettingsItem("插件管理", ModernWpf.Controls.Symbol.ViewAll, new PluginManagerPage(model), new(19, 12, 0, 0)),
                new SettingsItem("个性化", ModernWpf.Controls.Symbol.Edit, new PersonalisePage()),
                new SettingsItem("关于", ModernWpf.Controls.Symbol.ReportHacked, new AboutPage())
            };
            SelectedItem = SettingsItems[0];
        }

        public ICommand SwitchPaneOpenCommand => new DelegateCommand(SwitchPaneOpen);

        private void SwitchPaneOpen()
        {
            IsPaneOpen = !IsPaneOpen;
        }
    }
}
