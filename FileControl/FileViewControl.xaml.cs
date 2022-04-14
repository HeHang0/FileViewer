using FileViewer.FileControl.Common;
using FileViewer.FileControl.Excel;
using FileViewer.FileControl.Hello;
using FileViewer.FileControl.Image;
using FileViewer.FileControl.Music;
using FileViewer.FileControl.Pdf;
using FileViewer.FileControl.PowerPoint;
using FileViewer.FileControl.Text;
using FileViewer.FileControl.Video;
using FileViewer.FileControl.Word;
using FileViewer.FileHelper;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FileViewer.FileControl
{
    /// <summary>
    /// FileViewControl.xaml 的交互逻辑
    /// </summary>
    public partial class FileViewControl : UserControl
    {
        private FileViewType lastViewType = FileViewType.None;

        public FileViewControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            MyGrid.Children.Add(new HelloControl());
            GlobalNotify.FileLoadFailed += OnFileLoadFailed;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            GlobalNotify.OnLoadingChange(true);
            SetResource(DataContext as string);
        }

        private void OnFileLoadFailed(string filePath)
        {
            SetResource(filePath, true);
        }

        private void SetResource(string filePath, bool loadWithTypeNone = false)
        {
            var typeInfo = FileType.GetFileViewType(filePath);
            if (loadWithTypeNone)
            {
                typeInfo.Type = FileViewType.None;
            }
            if(lastViewType != typeInfo.Type || MyGrid.Children[0] is HelloControl)
            {
                lastViewType = typeInfo.Type;
                MyGrid.Children.Clear();
            }
            else if(MyGrid.Children.Count == 1)
            {
                (MyGrid.Children[0] as FileControl).OnFileChanged((filePath, typeInfo.Ext));
                return;
            }
            FileControl fc;
            switch (typeInfo.Type)
            {
                case FileViewType.Image:
                    fc = new ImageControl();
                    GlobalNotify.OnResizeMode(true);
                    break;
                case FileViewType.Code:
                case FileViewType.Txt:
                    fc = new TextControl();
                    GlobalNotify.OnResizeMode(true);
                    break;
                case FileViewType.Music:
                    fc = new MusicControl();
                    GlobalNotify.OnResizeMode(false);
                    break;
                case FileViewType.Video:
                    fc = new VideoControl();
                    GlobalNotify.OnResizeMode(true);
                    break;
                case FileViewType.Pdf:
                    fc = new PdfControl();
                    GlobalNotify.OnResizeMode(true);
                    break;
                case FileViewType.Excel:
                    fc = new ExcelControl();
                    GlobalNotify.OnResizeMode(true);
                    break;
                case FileViewType.Word:
                    fc = new WordControl();
                    GlobalNotify.OnResizeMode(true);
                    break;
                case FileViewType.PowerPoint:
                    fc = new PowerPointControl();
                    GlobalNotify.OnResizeMode(true);
                    break;
                default:
                    fc = new CommonControl();
                    GlobalNotify.OnResizeMode(false);
                    break;

            }
            if (fc != null) LoadFile(fc, filePath, typeInfo.Ext);
        }

        private void LoadFile(FileControl fc, string filePath, FileExtension ext)
        {
            fc.Margin = new Thickness(0);
            fc.OnFileChanged((filePath, ext));
            MyGrid.Children.Add(fc);
        }
    }
}
