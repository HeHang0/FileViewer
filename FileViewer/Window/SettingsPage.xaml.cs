using FileViewer.Base;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileViewer
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {
        private const string SegoeMDL2Assets = "Segoe MDL2 Assets";
        private const string SegoeUIEmoji = "Segoe UI Emoji";
        private const string SegoeMDL2AssetsFile = "segmdl2.ttf";
        private const string SegoeUIEmojiFile = "seguiemj.ttf";
        private const string SystemFontsPath = @"C:\Windows\Fonts\";
        public SettingsPage()
        {
            InitializeComponent();
            StartupSwitch.IsOn = IsStartupEnabled();
            CheckFontInstalled();
        }

        private void CheckFontInstalled()
        {
            List<string> needInstalls = new();
            if (!IsFontInstalled(SegoeMDL2Assets))
            {
                needInstalls.Add(SegoeMDL2Assets);
            }
            if (!IsFontInstalled(SegoeUIEmoji))
            {
                needInstalls.Add(SegoeUIEmoji);
            }
            NeedInstalledFonts.Content = string.Join(", ", needInstalls);
            FontInstallControl.Visibility = needInstalls.Count <= 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private static bool IsFontInstalled(string fontName)
        {
            using InstalledFontCollection fonts = new();
            return fonts.Families.Any(m => string.Equals(m.Name, fontName, StringComparison.OrdinalIgnoreCase));
        }

        private void InstallFont(object sender, RoutedEventArgs e)
        {
            try
            {
                var tempPath = Path.GetTempPath();
                string mdlPath = Path.Combine(tempPath, SegoeMDL2AssetsFile);
                string emjPath = Path.Combine(tempPath, SegoeUIEmojiFile);
                var installPath = new List<string>();
                if (!File.Exists(Path.Combine(SystemFontsPath, SegoeMDL2AssetsFile)))
                {
                    File.WriteAllBytes(mdlPath, Properties.Resources.segmdl2);
                    installPath.Add(mdlPath);
                }
                if (!File.Exists(Path.Combine(SystemFontsPath, SegoeUIEmojiFile)))
                {
                    File.WriteAllBytes(emjPath, Properties.Resources.seguiemj);
                    installPath.Add(emjPath);
                }
                if (installPath.Count <= 0) return;
                Task.Run(() => InstallFontInBackground(installPath));
                InstallFontButton.IsEnabled = false;
                InstallFontProgressRing.IsActive = true;
                checkTimes = 20;
                CheckFontInstalResult();
            }
            catch (Exception)
            {
            }
        }



        private static void InstallFontInBackground(IEnumerable<string> installPaths)
        {
            foreach (string item in installPaths)
            {
                try
                {
                    var type = Type.GetTypeFromProgID("Shell.Application");
                    if (type == null) return;
                    dynamic? objShell = Activator.CreateInstance(type);
                    var folderPath = Path.GetDirectoryName(item);
                    var fileName = Path.GetFileName(item);
                    dynamic? objFolder = objShell?.Namespace(folderPath);
                    dynamic? objFolderItem = objFolder?.ParseName(fileName);
                    objFolderItem?.InvokeVerb("Install");
                }
                catch (Exception)
                {
                }
            }
        }

        readonly Tools.Timeout timeout = new();
        private int checkTimes = 0;
        private void CheckFontInstalResult()
        {
            checkTimes--;
            var result = false;
            try
            {
                result = File.Exists(Path.Combine(SystemFontsPath, SegoeMDL2AssetsFile)) &&
                    File.Exists(Path.Combine(SystemFontsPath, SegoeUIEmojiFile));
            }
            catch (Exception)
            {
            }
            if (result)
            {
                var tempPath = Path.GetTempPath();
                Tools.File.Delete(Path.Combine(tempPath, SegoeMDL2AssetsFile));
                Tools.File.Delete(Path.Combine(tempPath, SegoeUIEmojiFile));
                Dispatcher.Invoke(() =>
                {
                    var rebootResult = MessageBox.Show(Window.GetWindow(this),
                        "字体安装成功，点击【确定】重启应用", "提示",
                        MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (rebootResult == MessageBoxResult.OK)
                    {
                        Utils.RebootApplication();
                    }
                    InstallFontProgressRing.IsActive = false;
                    InstallFontButton.IsEnabled = true;
                });
            }
            else if (checkTimes > 0)
            {
                timeout.SetTimeout(CheckFontInstalResult, TimeSpan.FromSeconds(1));
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    InstallFontProgressRing.IsActive = false;
                    InstallFontButton.IsEnabled = true;
                });
            }
        }

        private void OpenFontFolder(object sender, RoutedEventArgs e)
        {
            if (OSVersionHelper.IsWindows10OrGreater)
            {
                Tools.File.ProcessStart("ms-settings:fonts");
            }
            else
            {
                Tools.File.ProcessStart("control.exe", "/name Microsoft.Fonts");
            }
        }

        private void OpenSystemStartup(object sender, RoutedEventArgs e)
        {
            if (OSVersionHelper.IsWindows11OrGreater)
            {
                Tools.File.ProcessStart("ms-settings:startupapps");
            }
            else if (OSVersionHelper.IsWindows8OrGreater)
            {
                Tools.File.ProcessStart("taskmgr", "/Startup");
            }
            else
            {
                Tools.File.ProcessStart("msconfig.exe", "/4");
            }
        }

        private void SetStartup(object? sender, EventArgs e)
        {
            if (sender is not ModernWpf.Controls.ToggleSwitch startupMenu) return;
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string appPath = Environment.ProcessPath ?? string.Empty;
            string lnkPath = Path.Combine(startupFolderPath, Path.GetFileNameWithoutExtension(appPath) + ".lnk");
            var exists = File.Exists(lnkPath);
            var isChecked = startupMenu.IsOn;
            if (exists && isChecked) return;
            if (!exists && !isChecked) return;
            if (isChecked)
            {
                ShellLink.Shortcut.CreateShortcut(appPath).WriteToFile(lnkPath);
            }
            else
            {
                File.Delete(lnkPath);
            }
        }

        private static bool IsStartupEnabled()
        {
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string appPath = Environment.ProcessPath ?? string.Empty;
            string lnkPath = Path.Combine(startupFolderPath, Path.GetFileNameWithoutExtension(appPath) + ".lnk");
            return File.Exists(lnkPath);
        }
    }
}
