using FileViewer.ViewModel;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Application = System.Windows.Application;

namespace FileViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Viewer myViewer;
        public MainWindow()
        {
            myViewer = new Viewer();
            myViewer.ViewerEventer.ReceiveFile += BalloonTips;
            myViewer.ViewerEventer.ReceiveFile += myViewer.FileInfo.InitFile;
            GlobalNotify.SizeChange += SizeChange;
            GlobalNotify.FullScreen += FullScreen;
            InitializeComponent();
            DataContext = myViewer;
        }

        private void FullScreen(bool isFullScreen)
        {
            if (isFullScreen)
            {
                WindowStyle = System.Windows.WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
        }

        private void SizeChange(double height, double width)
        {
            height += SystemParameters.CaptionHeight+10;
            var workArea = SystemParameters.WorkArea;
            if(height > workArea.Height)
            {
                Height = workArea.Height;
            }
            else
            {
                Height = height;
            }
            if (width > workArea.Width)
            {
                Width = workArea.Width;
            }
            else
            {
                Width = width;
            }
            //Left = workArea.Left + (workArea.Width / 2 - Width / 2);
            //Top = workArea.Top + (workArea.Height / 2 - Height / 2);
        }

        private void MyMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitNotyfy();
            //Hide();
            myViewer.ViewerEventer.OnLoaded.Execute(null);
        }

        NotifyIcon notifyIcon;
        private void InitNotyfy()
        {
            notifyIcon = new NotifyIcon();
            notifyIcon.Text = Title;//最小化到托盘时，鼠标点击时显示的文本
            notifyIcon.Icon = Properties.Resources.logo ;//程序图标
            notifyIcon.Visible = true;
            MenuItem closeItem = new MenuItem("退出");
            closeItem.Click += ExitApp;
            MenuItem openItem = new MenuItem("显示");
            openItem.Click += Show;
            MenuItem[] menu = new MenuItem[] { openItem, closeItem };
            notifyIcon.ContextMenu = new ContextMenu(menu);
        }

        private void ExitApp(object sender, EventArgs e)
        {
            notifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        private void Show(object sender, EventArgs e)
        {
            Show();
        }

        private void BalloonTips(string msg)
        {
            msg = Path.GetFileName(msg);
            if (msg.Length > 16)
            {
                msg = msg.Substring(0, 13) + "...";
            }
            notifyIcon.Text = msg;
            Show();
            Activate();
            if(WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }

        private void MyMainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
            GlobalNotify.OnWindowClose();
        }
    }
}
