using Microsoft.WindowsAPICodePack.Shell;
using Prism.Commands;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;

namespace FileViewer.ViewModel
{
    public class FileAccess : INotifyPropertyChanged
    {
        
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title { get; set; }

        public string FilePath { get; private set; }

        public ImageSource IconImage { get; private set; }

        public Brush BackgroundColor { get; private set; } = Brushes.White;

        public Brush TitleBarForeground { get; private set; } = Brushes.Black;

        public void InitFile(string filePath)
        {
            if (FilePath == filePath) return;
            if (!Directory.Exists(filePath) && !File.Exists(filePath)) return;
            Title = Path.GetFileName(filePath);
            FilePath = filePath;
            ShellObject shellFile = ShellObject.FromParsingName(filePath);
            IconImage = shellFile.Thumbnail.SmallBitmapSource;
            //SizeChange?.Invoke(ThumbnailImage.Height, ThumbnailImage.Width);
        }
        public bool Loading { get; private set; }

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

        public ICommand OpenFile => new DelegateCommand(() => 
        {
            if(File.Exists(FilePath) || Directory.Exists(FilePath))
            System.Diagnostics.Process.Start(FilePath);
        });
    }
}
