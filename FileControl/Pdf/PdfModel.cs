using FileViewer.FileHelper;
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace FileViewer.FileControl.Pdf
{
    public class PdfModel : IFileModel
    {
        public delegate void FileChangedEventHandler();
        public event PropertyChangedEventHandler PropertyChanged;
        public event FileChangedEventHandler TextFileChanged;

        public string PdfFilePath { get; set; }

        public string RealFilePath;

        private FileExtension extension = FileExtension.None;

        public void ChangeFile((string FilePath, FileExtension Ext) file)
        {
            if (RealFilePath != file.FilePath)
            {
                RealFilePath = file.FilePath;
                extension = file.Ext;
                if (file.Ext == FileExtension.PDF)
                {
                    PdfFilePath = file.FilePath;
                }
                else
                {
                    TextFileChanged?.Invoke();
                }
            }
            GlobalNotify.OnLoadingChange(false);
        }

        public void ChangeTheme(bool dark)
        {
            if (dark)
            {
                if (extension == FileExtension.PDF)
                {
                    GlobalNotify.OnColorChange(Color.FromRgb(0x33, 0x33, 0x33));
                }
                else
                {
                    GlobalNotify.OnColorChange(Color.FromRgb(0x1E, 0x1E, 0x1E));
                }
            }
            else
            {
                if (extension == FileExtension.PDF)
                {
                    GlobalNotify.OnColorChange(Color.FromRgb(0xF7, 0xF7, 0xF7));
                }
                else
                {
                    GlobalNotify.OnColorChange(Color.FromRgb(0xFF, 0xFF, 0xFE));
                }
            }
        }
    }
}