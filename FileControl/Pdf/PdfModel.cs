using FileViewer.FileHelper;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileViewer.FileControl.Pdf
{
    public class PdfModel: IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string filePath;
        private FileExtension fileExt;

        public string PdfFilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
            }
        }

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            OnColorChanged(Color.FromRgb(0x47,0x47, 0x47));
            if (filePath != file.FilePath)
            {
                PdfFilePath = file.FilePath;
                fileExt = file.Ext;
            }
            GlobalNotify.OnLoadingChange(false);
        }

        public void OnColorChanged(Color color)
        {
            GlobalNotify.OnColorChange(color);
        }
    }
}
