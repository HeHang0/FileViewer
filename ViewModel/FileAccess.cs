using FileViewer.Globle;
using FileViewer.Monitor;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell;
using Syncfusion.Windows.Shared;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using File = System.IO.File;

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

        public bool Topmost { get; set; }

        public string OpenText { get; set; } = "打开";

        private string execPath;

        public bool Loading { get; private set; }

        public Brush BackgroundColor { get; private set; } = Brushes.White;

        public Brush TitleBarForeground { get; private set; } = Brushes.Black;

        public void InitFile(string filePath)
        {
            if (FilePath == filePath) return;
            bool isDirectory = Directory.Exists(filePath);
            bool isFile = File.Exists(filePath);
            if (!isDirectory && !isFile) return;
            Title = Path.GetFileName(filePath);
            FilePath = filePath;
            var (name, execPath) = GetDefaultAppForPath(filePath, isDirectory);
            this.execPath = execPath;
            if (name == null || name.IsNullOrWhiteSpace())
            {
                OpenText = " 打开 ";
            }
            else if(Path.GetExtension(filePath).ToLower() == ".lnk")
            {
                OpenText = $" 打开 {name} ";
            }
            else
            {
                OpenText = $" 使用 {name} 打开 ";
            }
        }

        private (string Name, string Path) GetDefaultAppForPath(string filePath, bool isDirectory)
        {
            string appName = null;
            string appPath = null;
            string progId = null;
            string extension = Path.GetExtension(filePath).ToLower();
            if(extension == ".pdf") Topmost= false;
            if (extension == ".lnk")
            {
                appPath = Utils.LinkPath(filePath);
            }
            else if (!isDirectory)
            {
                // 获取文件扩展名的默认程序的ProgID
                var subKey = Registry.ClassesRoot.OpenSubKey(extension);
                progId = (string)subKey?.GetValue(null);
                if (progId == null)
                {
                    progId = subKey?.OpenSubKey("OpenWithProgids")?.GetValueNames()?.FirstOrDefault();
                }
            }
            else
            {
                appPath = "C:\\Windows\\explorer.exe";
            }

            if (progId != null)
            {
                // 使用ProgID获取应用程序的路径
                RegistryKey openWithKey = Registry.ClassesRoot.OpenSubKey(progId + "\\shell\\open\\command");
                if (openWithKey == null) openWithKey = Registry.ClassesRoot.OpenSubKey(progId + "\\shell\\printto\\command");
                if (openWithKey != null)
                {
                    // 获取默认打开方式的路径
                    appPath = (string)openWithKey.GetValue(null);
                    if (appPath != null)
                    {
                        int index = appPath.IndexOf(appPath.StartsWith("\"") ? "\" " : " ");
                        string originAppPath = appPath;
                        appPath = appPath.Substring(0, index).Replace("\"", "").Trim();
                        if (appPath.EndsWith("rundll32.exe"))
                        {
                            int dllIndex = originAppPath.IndexOf(".dll");
                            if (dllIndex > 0)
                            {
                                string dllName = originAppPath.Substring(index, dllIndex - index + 4).Trim();
                                string dllPath = Path.Combine(Path.GetDirectoryName(originAppPath), dllName);
                                appName = GetDisplayName(dllPath);
                                appPath = null;
                            }
                        }
                    }
                    if (!File.Exists(appPath)) appPath = null;
                }
                if (appName == null)
                {
                    // 使用ProgID获取应用程序的名称
                    RegistryKey openWithFriendlyAppName = Registry.ClassesRoot.OpenSubKey(progId + "\\shell\\open");
                    if (openWithFriendlyAppName != null)
                    {
                        // 获取默认应用程序的名称
                        appName = (string)openWithFriendlyAppName.GetValue("FriendlyAppName");
                    }
                }
            }
            if(appName == null && appPath != null)
            {
                appName = GetDisplayName(appPath);
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
                if(execPath != null && File.Exists(execPath) && Path.GetExtension(FilePath).ToLower() != ".lnk")
                {
                    Process.Start(execPath, FilePath);
                }
                else
                {
                    Process.Start(FilePath);
                }
            }
        });

        public ICommand SwitchTopMost => new Prism.Commands.DelegateCommand(() => {
            Topmost = !Topmost;
            GlobalNotify.OnWindowVisableChanged(true, Topmost);
        });
    }
}
