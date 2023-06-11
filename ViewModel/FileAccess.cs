using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
using Syncfusion.Windows.Shared;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace FileViewer.ViewModel
{
    public class FileAccess : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Title { get; set; }

        public string FilePath { get; set; }

        public string OpenText { get; set; } = "打开";

        private string execPath;

        public bool Loading { get; private set; }

        public Brush BackgroundColor { get; private set; } = Brushes.White;

        public Brush TitleBarForeground { get; private set; } = Brushes.Black;

        public void InitFile(string filePath)
        {
            if (FilePath == filePath) return;
            if (!Directory.Exists(filePath) && !File.Exists(filePath)) return;
            Title = Path.GetFileName(filePath);
            FilePath = filePath;
            var (name, execPath) = GetDefaultAppForExtension(Path.GetExtension(filePath));
            this.execPath = execPath;
            if (name == null || name.IsNullOrWhiteSpace())
            {
                OpenText = "打开";
            }else
            {
                OpenText = $" 使用 {name} 打开 ";
            }
        }

        private (string Name, string Path) GetDefaultAppForExtension(string extension)
        {
            string appName = null;
            string appPath = null;

            // 获取文件扩展名的默认程序的ProgID
            var subKey = Registry.ClassesRoot.OpenSubKey(extension);
            string progId = (string)subKey?.GetValue(null);
            if (progId == null)
            {
                progId = subKey?.OpenSubKey("OpenWithProgids")?.GetValueNames()?.FirstOrDefault();
            }
            if (progId != null)
            {
                // 使用ProgID获取应用程序的路径
                RegistryKey openWithKey = Registry.ClassesRoot.OpenSubKey(progId + "\\shell\\open\\command");
                if(openWithKey == null) openWithKey = Registry.ClassesRoot.OpenSubKey(progId + "\\shell\\printto\\command");
                if (openWithKey != null)
                {
                    // 获取默认打开方式的路径
                    appPath = (string)openWithKey.GetValue(null);
                    if(appPath != null)
                    {
                        int index = appPath.IndexOf(appPath.StartsWith("\"") ? "\" " : " ");
                        if(index > 0) appPath = appPath.Substring(0, index);
                        appPath = appPath?.Replace("\"", "").Trim();
                    }
                    if (!File.Exists(appPath)) appPath = null;
                }


                // 使用ProgID获取应用程序的名称
                RegistryKey openWithFriendlyAppName = Registry.ClassesRoot.OpenSubKey(progId + "\\shell\\open");
                if (openWithFriendlyAppName != null)
                {
                    // 获取默认应用程序的名称
                    appName = (string)openWithFriendlyAppName.GetValue("FriendlyAppName");
                }
                if(appName == null && appPath != null)
                {
                    appName = GetDisplayName(appPath);
                }
            }

            return (appName, appPath);
        }

        private string GetDisplayName(string filePath)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
            return fileVersionInfo.FileDescription ?? Path.GetFileNameWithoutExtension(filePath);
        }

        private void LoadingChange(bool loading)
        {
            Loading = loading;
        }

        private void ColorChange(Color color, bool white)
        {
            BackgroundColor = new SolidColorBrush(color);
            TitleBarForeground = white ? Brushes.Black : Brushes.White;
        }

        public FileAccess()
        {
            GlobalNotify.LoadingChange += LoadingChange;
            GlobalNotify.ColorChange += ColorChange;
        }

        public ICommand OpenFile => new Prism.Commands.DelegateCommand(() => 
        {
            if(File.Exists(FilePath) || Directory.Exists(FilePath))
            {
                if(execPath != null && File.Exists(execPath))
                {
                    System.Diagnostics.Process.Start(execPath, FilePath);
                }
                else
                {
                    System.Diagnostics.Process.Start(FilePath);
                }
            }
        });
    }
}
