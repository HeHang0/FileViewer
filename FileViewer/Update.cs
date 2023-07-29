using AutoUpdate.Core;
using System;
using System.Reflection;
using System.Windows;

namespace FileViewer
{
    public static class Update
    {
        public static event NewPackageCheckedEventHandler? NewPackageChecked;

        public static readonly AutoUpdate.Core.AutoUpdate AutoUpdate;
        public static readonly GithubChecker GithubChecker;
        private static readonly AssemblyName assemblyName = Application.ResourceAssembly.GetName();
        static Update()
        {
            GithubChecker = new GithubChecker("hehang0", assemblyName.Name, assemblyName.Name + ".exe", assemblyName.Version?.ToString());
            AutoUpdate = new AutoUpdate.Core.AutoUpdate(new Options(GithubChecker, TimeSpan.FromHours(1)));
            AutoUpdate.NewPackageChecked += OnNewPackageChecked;
            AutoUpdate.Start();
        }

        private static void OnNewPackageChecked(AutoUpdate.Core.AutoUpdate sender, PackageCheckedEventArgs e)
        {
            NewPackageChecked?.Invoke(sender, e);
        }
    }
}
