using System;
using System.Windows;
using System.Windows.Controls;
using FileViewer.FileHelper;

namespace FileViewer.FileControl
{
    /// <summary>
    /// FileControl.xaml 的交互逻辑
    /// </summary>
    public class FileControl : UserControl, IFileChanged
    {
        public readonly IFileChanged model;
        public FileControl(IFileChanged iModel)
        {
            model = iModel;
            DataContext = model;
        }
        public FileControl()
        {
        }

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            model.OnFileChanged(file);
        }
    }
}
