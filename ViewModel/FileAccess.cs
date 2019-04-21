using FileViewer.FileHelper;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileViewer.ViewModel
{
    public class FileAccess : INotifyPropertyChanged
    {
        public delegate void SizeChangeEventHandler(double height, double width);
        public event SizeChangeEventHandler SizeChange;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Title { get; private set; }

        public FileExtension FType { get; private set; }

        public BitmapSource ThumbnailImage { get; private set; }

        public void InitFile(object sender, string filePath)
        {
            Title = Path.GetFileName(filePath);
            FType = FileType.GetFileType(filePath);
            ShellObject shellFile = ShellObject.FromParsingName(filePath);
            ThumbnailImage = shellFile.Thumbnail.ExtraLargeBitmapSource;
            SizeChange?.Invoke(ThumbnailImage.Height, ThumbnailImage.Width);
        }
    }
}
