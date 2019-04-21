using FileViewer.ViewModel;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;

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
            myViewer.ViewerEventer.Loaded += MyViewer_Loaded;
            myViewer.FileInfo.SizeChange += SizeChange;
            InitializeComponent();
            DataContext = myViewer;
        }

        private void SizeChange(double height, double width)
        {
            Height = height;
            Width = width;
        }

        private void MyViewer_Loaded(object sender, EventArgs e)
        {
            InitNotyfy();
            Hide();
        }

        System.Windows.Forms.NotifyIcon notifyIcon;
        private void InitNotyfy()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = Title;//最小化到托盘时，鼠标点击时显示的文本
            notifyIcon.Icon = FromImageSource(Icon);//程序图标
            notifyIcon.Visible = true;
            BalloonTips(null, "Viewer");
        }

        private static Icon FromImageSource(ImageSource icon)
        {
            if (icon == null)
            {
                return null;
            }
            Uri iconUri = new Uri(icon.ToString());
            return new Icon(Application.GetResourceStream(iconUri).Stream);
        }

        private void BalloonTips(object sender, string msg)
        {
            //notifyIcon.BalloonTipText = msg; //设置托盘提示显示的文本
            //notifyIcon.ShowBalloonTip(500);
            Show();
            Activate();
        }
    }
}
