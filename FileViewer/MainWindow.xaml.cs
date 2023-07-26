using FileViewer.Base;
using FileViewer.Plugins.Hello;
using FileViewer.Style;
using FileViewer.Tools;
using ModernWpf;
using PicaPico;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;

namespace FileViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Hook.Keyboard _keyboardHook;
        private readonly MutexBool isLoop = new();
        readonly MainModel _model = new();

        NotifyIcon? notifyIcon;
        readonly PluginManager pluginManager;
        IPlugin? currentPlugin;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _model;
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            _keyboardHook = new Hook.Keyboard();
            _keyboardHook.InstallHook(OnKeyPress);
            pluginManager = new PluginManager();
            _model.LoadFailed += FileLoadFailed;
            IsVisibleChanged += OnVisibleChanged;
            Closing += OnClosing;
            InitNotify();
            InitHello();
            DependencyPropertyDescriptor desc = DependencyPropertyDescriptor.FromProperty(ThemeManager.ActualApplicationThemeProperty, ThemeManager.Current.GetType());
            desc.AddValueChanged(ThemeManager.Current, OnThemeChanged);
            _model.SetDarkMode(IsDark);
        }

        private static bool IsDark => ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark;

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            var isDark = IsDark;
            if (_settingsWindow != null) _settingsWindow.Background = new SolidColorBrush(isDark ? Utils.BackgroundDark : Utils.BackgroundLight);
            currentPlugin?.ChangeTheme(isDark);
            _model.SetDarkMode(isDark);
            MicaHelper.Apply(this, isDark);
        }

        private void InitHello()
        {
            currentPlugin = new PluginHello();
            var control = currentPlugin.GetUserControl(_model);
            MainContent.Content = control;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Visibility = Visibility.Collapsed;
            if (MicaHelper.IsSupported)
            {
                MicaHelper.Apply(this, ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark);
            }
            else
            {
                AcrylicHelper.Apply(this, true);
            }
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _model.SetLoading(false);
            MessageBox.Show(Application.Current.MainWindow, e.ExceptionObject.ToString(), "错误");
        }

        string lastFilePath = string.Empty;
        private void ShowFile(string filePath)
        {
            if (filePath == lastFilePath) return;
            lastFilePath = filePath;
            _model.SetFile(filePath);
            _model.SetLoading(true);
            if (!_model.TopMostShow) _model.TopMostShow = true;
            currentPlugin = pluginManager.GetPlugin(filePath);
            MainContent.Content = currentPlugin.GetUserControl(_model);

            currentPlugin.ChangeFile(filePath);
            Show();
            if (_model.WindowState == WindowState.Minimized)
            {
                _model.WindowState = WindowState.Normal;
            }
            Activate();
        }

        private void FileLoadFailed(object? sender, string filePath)
        {
            currentPlugin = pluginManager.GetPlugin();
            MainContent.Content = currentPlugin.GetUserControl(_model);
            currentPlugin.ChangeFile(filePath);
        }

        #region NotifyIcon

        private void InitNotify()
        {
            notifyIcon = new NotifyIcon()
            {
                Text = Assembly.GetEntryAssembly()?.GetName().Name,
                Icon = Properties.Resources.logo
            };
            ToolStripMenuItem exitItem = new("退  出", null, ExitApp)
            {
                Margin = new System.Windows.Forms.Padding(0, 0, 0, 2),
                Width = 120,
            };
            ToolStripMenuItem openItem = new("显  示", null, ShowWindow)
            {
                Margin = new System.Windows.Forms.Padding(0, 2, 0, 0)
            };
            ToolStripMenuItem pluginItem = new("设  置", null, ShowSettings);
            notifyIcon.AddMenu(new ToolStripMenuItem[] { openItem, pluginItem, exitItem });
#if DEBUG
            ShowSettings(null, new EventArgs());
#endif
        }

        private void ShowWindow(object? sender, EventArgs e)
        {
            Show();
            Activate();
        }

        private SettingsWindow? _settingsWindow;
        private void ShowSettings(object? sender, EventArgs e)
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow(pluginManager);
                var isDark = ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark;
                _settingsWindow.Background = new SolidColorBrush(isDark ? Utils.BackgroundDark : Utils.BackgroundLight);
                _settingsWindow.Closed += (a, b) => { _settingsWindow = null; };
            }
            _settingsWindow.Show();
            _settingsWindow.Activate();
            if (_settingsWindow.WindowState == WindowState.Minimized)
            {
                _settingsWindow.WindowState = WindowState.Normal;
            }
        }

        private void ExitApp(object? sender, EventArgs e)
        {
            notifyIcon?.Dispose();
            Application.Current.Shutdown();
        }
        #endregion

        #region FileHook
        private void OnKeyPress(Hook.Keyboard.HookStruct hookStruct, out bool handle)
        {
            handle = false;
            if (hookStruct.vkCode != 32 || hookStruct.flags != 0 || isLoop.GetValue()) return;
            RunCheckFileThread();
        }

        private void RunCheckFileThread()
        {
            Thread thread = new(new ThreadStart(RunCheckFile))
            {
                IsBackground = true
            };
            thread.Start();
        }

        private void RunCheckFile()
        {
            var (ok, filePath) = Hook.ExplorerFile.GetCurrentFilePath();
            if (ok)
            {
                Dispatcher.Invoke(() =>
                {
                    ShowFile(filePath);
                });
            }
        }

        private void ChangeTopMost(object sender, RoutedEventArgs e)
        {
            _model.TopMost = !_model.TopMost;
            ProcessLoop();
        }

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ProcessLoop();
        }

        private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
            ProcessLoop();
        }

        private void ProcessLoop()
        {
            bool loop = isLoop.GetValue();
            bool needLoop = Visibility == Visibility.Visible && Topmost;
            if (loop != needLoop)
            {
                if (loop) isLoop.SetVal(false);
                else
                {
                    Thread thread = new(new ThreadStart(LoopShowFile))
                    {
                        IsBackground = true
                    };
                    isLoop.SetVal(true);
                    thread.Start();
                }
            }
        }

        private void LoopShowFile()
        {
            while (isLoop.GetValue())
            {
                var (ok, filePath) = Hook.ExplorerFile.GetCurrentFilePath();
                if (ok)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (isLoop.GetValue()) ShowFile(filePath);
                    });
                }
                Thread.Sleep(400);
            }
        }
        #endregion
    }
}
