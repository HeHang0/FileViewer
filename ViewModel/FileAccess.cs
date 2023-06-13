using FileViewer.Globle;
using Microsoft.Win32;
using Syncfusion.Windows.Shared;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
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

        public bool Topmost { get; set; }

        public string OpenText { get; set; } = "";

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
            var (name, extension) = GetDefaultAppForPath(filePath, isDirectory);
            if (extension == ".dll")
            {
                OpenText = "";
                return;
            }
            else if (name == null || name.IsNullOrWhiteSpace())
            {
                OpenText = " 打开 ";
            }
            else if(extension == ".lnk" || extension == ".exe")
            {
                OpenText = $" 打开 {name} ";
            }
            else
            {
                OpenText = $" 使用 {name} 打开 ";
            }
        }

        private (string appName, string extension) GetDefaultAppForPath(string filePath, bool? isDirectory)
        {
            string appName = null;
            string appPath = null;
            string progId = null;
            string extension = Path.GetExtension(filePath).ToLower();
            if(extension == ".dll") return (appName, extension);
            if(isDirectory == null) isDirectory = Directory.Exists(filePath);
            if (extension == ".pdf") Topmost= false;
            if (extension == ".lnk")
            {
                appPath = Utils.LinkPath(filePath);
            }
            else if(extension == ".exe")
            {
                appPath = filePath;
            }
            else if (isDirectory == false)
            {
                // 获取文件扩展名的默认程序的ProgID
                var subKey = Registry.ClassesRoot.OpenSubKey(extension);
                progId = (string)subKey?.GetValue(null);
                if (progId == null)
                {
                    var openWithProgids = subKey?.OpenSubKey("OpenWithProgids");
                    var names = openWithProgids?.GetValueNames() ?? new string[] { };
                    foreach (var name in names)
                    {
                        if(openWithProgids.GetValueKind(name) == RegistryValueKind.String)
                        {
                            progId = name;
                            break;
                        }
                    }
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
                        if (!File.Exists(appPath) && originAppPath.Contains(".exe"))
                        {
                            index = originAppPath.IndexOf(".exe");
                            appPath = originAppPath.Substring(0, index+4).Replace("\"", "").Trim();
                        }
                        if (appPath.EndsWith("rundll32.exe"))
                        {
                            int dllIndex = originAppPath.IndexOf(".dll");
                            if (dllIndex > 0)
                            {
                                string dllName = originAppPath.Substring(index, dllIndex - index + 4).Replace("\"", "").Trim();
                                string dllPath = dllName;
                                if(!File.Exists(dllPath)) dllPath = Path.Combine(Path.GetDirectoryName(originAppPath), dllName);
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
            }else if(appName == null)
            {
                appName = "选取应用";
            }

            return (appName, extension);
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
                try
                {
                    Process.Start(FilePath);
                }
                catch (System.Exception e)
                {
                    MessageBox.Show(Application.Current.MainWindow, e.Message, "提示");
                }
            }
        });

        public ICommand SwitchTopMost => new Prism.Commands.DelegateCommand(() => {
            Topmost = !Topmost;
            GlobalNotify.OnWindowVisableChanged(true, Topmost);
        });
    }
}
