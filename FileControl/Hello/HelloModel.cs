using Prism.Commands;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace FileViewer.FileControl.Hello
{
    class HelloModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource HelloBack => Utils.GetBitmapSource(Properties.Resources.HelloBack);

        public ImageSource HelloLogo => Utils.GetBitmapSource(Properties.Resources.preview);

        public ICommand Loaded => new DelegateCommand(() =>
        {
            GlobalNotify.OnColorChange(Color.FromRgb(0xA1, 0xD5, 0xD3));
            GlobalNotify.OnResizeMode(false);
        });

        public ICommand ToGitHub => new DelegateCommand(() =>
        {
            System.Diagnostics.Process.Start("https://github.com/HeHang0/FileViewer");
        });
    }
}
