using FileViewer.Base;
using FileViewer.Tools;
using Microsoft.Win32;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FileViewer
{
    public class MainModel : IManager, INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

        public event EventHandler<string>? LoadFailed;

        private string filePath = string.Empty;
        public MainModel()
        {
            var dark = IsDarkMode();
            Background = dark ? Brushes.Black : Brushes.White;
            byte colorValue = (byte)(dark ? 255 : 0);
            LoadingBackground = new SolidColorBrush(Color.FromArgb(33, colorValue, colorValue, colorValue));
        }

        public double Height { get; set; } = 480;

        public double Width { get; set; } = 800;

        public bool OpenTextShow => !string.IsNullOrWhiteSpace(OpenText);

        public string OpenText { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public bool Loading { get; set; } = false;

        public double TitleBarOpacity { get; set; } = 1;

        public bool TopMost { get; set; } = false;

        public bool TopMostShow { get; set; } = false;

        public static bool UseModernWindowStyle => OSVersionHelper.IsWindows8OrGreater;

        public double OpenTextMaxWidth => OSVersionHelper.IsWindows7OrLess ? double.MaxValue : (Width / 2);

        public FontStyle TopMostFontStyle => TopMost ? FontStyles.Italic : FontStyles.Normal;

        public Brush Background { get; set; }

        public Brush LoadingBackground { get; set; }

        public WindowState WindowState { get; set; } = WindowState.Normal;

        public ResizeMode ResizeMode { get; set; } = ResizeMode.CanResize;

        public ImageSource? FileIcon { get; set; }

        public ICommand OpenFile => new DelegateCommand(() =>
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            try
            {
                Tools.File.ProcessStart(filePath);
            }
            catch (Exception)
            {
                //MessageBox.Show(Application.Current.MainWindow, e.Message, "提示");
            }
        });

        public ICommand Activated => new DelegateCommand(() =>
        {
            TitleBarOpacity = 1;
        });

        public ICommand DeActivated => new DelegateCommand(() =>
        {
            TitleBarOpacity = 0.6;
        });

        public void SetFile(string filePath)
        {
            this.filePath = filePath;
            Title = Path.GetFileName(filePath);
            System.Drawing.Icon? icon;
            if (Directory.Exists(filePath))
            {
                icon = Icon.GetFolderIcon();
            }
            else
            {
                icon = Icon.GetDefaultFileIcon(Path.GetExtension(LinkFile(filePath)));
            }
            if (icon != null)
            {
                FileIcon = Utils.GetBitmapSource(icon);
            }
            else
            {
                FileIcon = Utils.GetBitmapSource(Properties.Resources.logo);
            }
            var (name, extension) = GetDefaultAppForPath(filePath, Directory.Exists(filePath));
            if (extension == ".dll")
            {
                OpenText = string.Empty;
                return;
            }
            else if (string.IsNullOrEmpty(name))
            {
                OpenText = " 打开 ";
            }
            else if (extension == ".lnk" || extension == ".exe")
            {
                OpenText = $" 打开 {name} ";
            }
            else
            {
                OpenText = $" 使用 {name} 打开 ";
            }
        }

        private static (string? appName, string extension) GetDefaultAppForPath(string filePath, bool? isDirectory)
        {
            string? appName = null;
            string? appPath = null;
            string? progId = null;
            string extension = Path.GetExtension(filePath).ToLower();
            if (extension == ".dll") return (appName, extension);
            isDirectory ??= Directory.Exists(filePath);
            if (extension == ".lnk")
            {
                appPath = LinkFile(filePath);
            }
            else if (extension == ".exe")
            {
                appPath = filePath;
            }
            else if (isDirectory == false)
            {
                var subKey = Registry.ClassesRoot.OpenSubKey(extension);
                progId = (string?)(subKey?.GetValue(null));
                if (progId == null)
                {
                    var openWithProgids = subKey?.OpenSubKey("OpenWithProgids");
                    var names = openWithProgids?.GetValueNames() ?? Array.Empty<string>();
                    foreach (var name in names)
                    {
                        if (openWithProgids?.GetValueKind(name) == RegistryValueKind.String)
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
                RegistryKey? openWithKey = Registry.ClassesRoot.OpenSubKey(progId + "\\shell\\open\\command");
                openWithKey ??= Registry.ClassesRoot.OpenSubKey(progId + "\\shell\\printto\\command");
                if (openWithKey != null)
                {
                    appPath = (string?)openWithKey.GetValue(null);
                    if (appPath != null)
                    {
                        int index = appPath.IndexOf(appPath.StartsWith("\"") ? "\" " : " ");
                        string originAppPath = appPath;
                        appPath = appPath[..index].Replace("\"", "").Trim();
                        if (!System.IO.File.Exists(appPath) && originAppPath.Contains(".exe"))
                        {
                            index = originAppPath.IndexOf(".exe");
                            appPath = originAppPath[..(index + 4)].Replace("\"", "").Trim();
                        }
                        if (appPath.EndsWith("rundll32.exe"))
                        {
                            int dllIndex = originAppPath.IndexOf(".dll");
                            if (dllIndex > 0)
                            {
                                string dllName = originAppPath.Substring(index, dllIndex - index + 4).Replace("\"", "").Trim();
                                string dllPath = dllName;
                                if (!System.IO.File.Exists(dllPath)) dllPath = Path.Combine(Path.GetDirectoryName(originAppPath)!, dllName);
                                appName = GetDisplayName(dllPath);
                                appPath = null;
                            }
                        }
                    }
                    if (!System.IO.File.Exists(appPath)) appPath = null;
                }
                if (appName == null)
                {
                    RegistryKey? openWithFriendlyAppName = Registry.ClassesRoot.OpenSubKey(progId + "\\shell\\open");
                    if (openWithFriendlyAppName != null)
                    {
                        appName = (string?)openWithFriendlyAppName?.GetValue("FriendlyAppName");
                    }
                }
            }
            if (appName == null && appPath != null)
            {
                appName = GetDisplayName(appPath);
            }
            else appName ??= "选取应用";
            if (appName == "Internet Explorer")
            {
                appName = "Microsoft Edge";
            }

            return (appName, extension);
        }

        private static string GetDisplayName(string filePath)
        {
            string defaultName = Path.GetFileNameWithoutExtension(filePath);
            try
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
                return fileVersionInfo.FileDescription ?? defaultName;
            }
            catch (Exception)
            {
                return defaultName;
            }
        }

        private static string LinkFile(string filePath)
        {
            if (System.IO.File.Exists(filePath) && Path.GetExtension(filePath).ToLower() == ".lnk")
            {
                try
                {
                    ShellLink.Shortcut shortcut = ShellLink.Shortcut.ReadFromFile(filePath);
                    return shortcut.LinkInfo.LocalBasePath;
                }
                catch (Exception)
                {
                }
            }
            return filePath;
        }

        public void SetIcon(ImageSource icon)
        {
            FileIcon = icon;
        }

        public void SetColor(Color color)
        {
            Background = new SolidColorBrush(color);
            //color.A = 33;
            LoadingBackground = Background;// new SolidColorBrush(color);
        }

        public void SetSize(double height, double width)
        {
            Height = height;
            Width = width;
        }

        public void SetLoading(bool loading)
        {
            Loading = loading;
        }

        public void SetFullScreen(bool fullScreen)
        {
            WindowState = fullScreen ? WindowState.Maximized : WindowState.Normal;
        }

        public void SetResizeMode(bool resize)
        {
            ResizeMode = resize ? ResizeMode.CanResize : ResizeMode.NoResize;
        }

        public void LoadFileFailed(string filePath)
        {
            LoadFailed?.Invoke(null, filePath);
        }

        public void LoadFileSuccess(double? height, double? width, bool? resize, bool? loading, Color? color)
        {
            if (height != null && width != null)
            {
                SetSize((double)height, (double)width!);
            }
            if (resize != null) SetResizeMode((bool)resize);
            if (loading != null) SetLoading((bool)loading);
            if (color != null) SetColor((Color)color);
        }

        public void CloseWindow()
        {
        }

        public void ChangedWindowVisable()
        {
        }

        public bool IsLoading()
        {
            return Loading;
        }

        private bool _isDark = false;
        public bool IsDarkMode()
        {
            return _isDark;
        }

        public void SetDarkMode(bool isDark)
        {
            _isDark = isDark;
        }
    }
}
