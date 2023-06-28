using AutoUpdate.Core;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace FileViewer
{
    /// <summary>
    /// AboutWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AboutWindow : Window
    {
        private AutoUpdate.Core.AutoUpdate autoUpdate;
        private AssemblyName assemblyName = Application.ResourceAssembly.GetName();
        public AboutWindow()
        {
            InitializeComponent();
            VersionText.Content = $"版本 {assemblyName.Version.ToString()}";
            CopyrightText.Content = $"Copyright © 2019 - {DateTime.Now.Year} picapico";
            InitUpdate();
        }

        private void InitUpdate()
        {
            var githubChecker = new GithubChecker("hehang0", assemblyName.Name, assemblyName.Name+".exe", assemblyName.Version.ToString());
            autoUpdate = new AutoUpdate.Core.AutoUpdate(new Options(githubChecker));
            autoUpdate.NewPackageChecked += NewPackageChecked;

            autoUpdate.Start();
        }

        private void NewPackageChecked(AutoUpdate.Core.AutoUpdate sender, PackageCheckedEventArgs e)
        {
            VersionUpdate.Visibility = Visibility.Visible;
            VersionUpdate.Content = $"有新版本(v{e.Version})，点击更新";
            Height += 10;
        }

        private void UpdateNewVersion(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            VersionUpdate.Visibility = Visibility.Collapsed;
            UpdateProgress.Visibility = Visibility.Visible;
            Height -= 10;
            CancellationTokenSource cts = new CancellationTokenSource();
            autoUpdate.Update(new SingleInstaller(), cts.Token, new Progress<int>(p =>
            {
                UpdateProgress.Value = p;
                if (p == 100)
                {
                    UpdateProgress.Visibility = Visibility.Collapsed;
                }
            }));
        }

        private void AboutWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            autoUpdate.Stop();
        }
    }
}
