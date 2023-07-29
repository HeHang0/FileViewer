﻿using AutoUpdate.Core;
using FileViewer.Base;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace FileViewer
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : Page
    {
        private static readonly AssemblyName assemblyName = Application.ResourceAssembly.GetName();

        public AboutPage()
        {
            InitializeComponent();
            AppName.Content = assemblyName.Name;
            AppVersion.Content = $" {assemblyName.Version?.ToString().TrimEnd('0').TrimEnd('.') ?? string.Empty}";
            CurrentYear.Content = $"{DateTime.Now.Year}";
            PageIcon.Source = Utils.GetBitmapSource(Properties.Resources.logo);
            VersionUpdate.Visibility = Update.GithubChecker.CanUpdate() ? Visibility.Visible : Visibility.Collapsed;
            Update.NewPackageChecked += NewPackageChecked;
        }

        private void NewPackageChecked(AutoUpdate.Core.AutoUpdate sender, PackageCheckedEventArgs e)
        {
            VersionUpdate.Visibility = Visibility.Visible;
        }

        private void UpdateNewVersion(object sender, RoutedEventArgs e)
        {
            VersionUpdate.Visibility = Visibility.Collapsed;
            UpdateProgress.Visibility = Visibility.Visible;
            Height -= 10;
            CancellationTokenSource cts = new();
            Update.AutoUpdate?.Update(new SingleInstaller(), cts.Token, new Progress<int>(p =>
            {
                UpdateProgress.Value = p;
                if (p == 100)
                {
                    UpdateProgress.Visibility = Visibility.Collapsed;
                }
            }));
        }
    }
}
