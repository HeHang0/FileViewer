using FileViewer.FileHelper;
using System.ComponentModel;
using System.Windows.Media;

namespace FileViewer.FileControl.Pdf
{
    public class PdfModel : IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string PdfFilePath { get; set; }

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            OnColorChanged(Color.FromRgb(0x47, 0x47, 0x47));
            if (PdfFilePath != file.FilePath)
            {
                PdfFilePath = file.FilePath;
            }
            GlobalNotify.OnLoadingChange(false);
        }

        public void OnColorChanged(Color color)
        {
            GlobalNotify.OnColorChange(color);
        }
    }
}