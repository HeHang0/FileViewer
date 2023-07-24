using AutoUpdate.Core;
using System;
using System.Reflection;
using System.Windows;

namespace FileViewer
{
    public static class Update
    {
        public static event NewPackageCheckedEventHandler? NewPackageChecked;

        private static readonly AutoUpdate.Core.AutoUpdate autoUpdate;
        private static readonly AssemblyName assemblyName = Application.ResourceAssembly.GetName();
        static Update()
        {
            var githubChecker = new GithubChecker("hehang0", assemblyName.Name, assemblyName.Name + ".exe", assemblyName.Version?.ToString());
            autoUpdate = new AutoUpdate.Core.AutoUpdate(new Options(githubChecker, TimeSpan.FromMinutes(60)));
            autoUpdate.NewPackageChecked += NewPackageChecked;
            autoUpdate.Start();
        }
    }
}
