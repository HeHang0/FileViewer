using FileViewer.FileHelper;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
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

        public string PdfFilePath { get; set; }

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            GlobalNotify.OnColorChange(Color.FromRgb(0x3b, 0x3b, 0x3b));
            GlobalNotify.OnSizeChange(600, 800);
            PdfFilePath = file.FilePath;
            GlobalNotify.OnLoadingChange(false);
        }

        public void OnColorChanged(Color color)
        {
        }
    }
}
