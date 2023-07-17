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

        public void ChangeFile((string FilePath, FileExtension Ext) file)
        {
            model?.ChangeFile(file);
        }

        public void ChangeTheme(bool dark)
        {
            model?.ChangeTheme(dark);
        }
    }
}
