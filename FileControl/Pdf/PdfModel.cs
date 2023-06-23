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

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            if (RealFilePath != file.FilePath)
            {
                RealFilePath = file.FilePath;
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

        public void OnColorChanged(Color color)
        {
            GlobalNotify.OnColorChange(color);
        }
    }
}