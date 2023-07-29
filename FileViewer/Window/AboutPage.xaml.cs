using AutoUpdate.Core;
using FileViewer.Base;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace FileViewer
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : BadgePage
    {
        private static readonly AssemblyName assemblyName = Application.ResourceAssembly.GetName();

        public AboutPage()
        {
            InitializeComponent();
            AppName.Content = assemblyName.Name;
            AppVersion.Content = $" {assemblyName.Version?.ToString().TrimEnd('0').TrimEnd('.') ?? string.Empty}";
            CurrentYear.Content = $"{DateTime.Now.Year}";
            PageIcon.Source = Utils.GetBitmapSource(Properties.Resources.logo);
            SetBadgeShow(Update.GithubChecker.CanUpdate());
            VersionUpdate.Visibility = IsBadgeShow ? Visibility.Visible : Visibility.Collapsed;
            Update.NewPackageChecked += NewPackageChecked;
        }

        private void NewPackageChecked(AutoUpdate.Core.AutoUpdate sender, PackageCheckedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                VersionUpdate.Visibility = Visibility.Visible;
                SetBadgeShow(true);
            });
        }

        private void UpdateNewVersion(object sender, RoutedEventArgs e)
        {
            VersionUpdate.Visibility = Visibility.Collapsed;
            UpdateProgress.Visibility = Visibility.Visible;
            SetBadgeShow(false);
            Height -= 10;
            CancellationTokenSource cts = new();
            Update.AutoUpdate?.Update(new SingleInstaller(), cts.Token, new Progress<int>(p =>
            {
                UpdateProgress.Value = p;
                if (p == 100)
                {
                    Dispatcher.Invoke(() =>
                    {
                        UpdateProgress.Visibility = Visibility.Collapsed;
                    });
                }
            }));
        }
    }
}
