using FileViewer.Base;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Hello
{
    /// <summary>
    /// HelloControl.xaml 的交互逻辑
    /// </summary>
    public partial class HelloControl : UserControl
    {
        readonly IManager manager;
        public HelloControl(IManager manager)
        {
            InitializeComponent();
            this.manager = manager;
            ChangeTheme(manager.IsDarkMode());
            manager.SetSize(480, 800);
            manager.SetResizeMode(false);
            HelloText.Content = $"Copyright © {DateTime.Now.Year}";
            HelloLogo.Source = Utils.GetBitmapSource(Utils.ImagePreview);
        }

        private readonly ImageSource? _helloBack = Utils.GetBitmapSource(Utils.ImageHelloBack);

        public void ChangeTheme(bool dark)
        {
            manager.SetColor(dark ? Utils.BackgroundDark : Utils.BackgroundHello);
            HelloBack.Source = dark ? null : _helloBack;
        }
    }
}
