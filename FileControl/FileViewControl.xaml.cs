using FileViewer.FileControl.Excel;
using FileViewer.FileControl.Image;
using FileViewer.FileControl.Music;
using FileViewer.FileControl.Pdf;
using FileViewer.FileControl.PowerPoint;
using FileViewer.FileControl.Text;
using FileViewer.FileControl.Video;
using FileViewer.FileControl.Word;
using FileViewer.FileHelper;
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
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            GlobalNotify.OnLoadingChange(true);
            SetResource(DataContext as string);
        }

        private void SetResource(string filePath)
        {
            var typeInfo = FileType.GetFileViewType(filePath);
            if(lastViewType != typeInfo.Type)
            {
                lastViewType = typeInfo.Type;
                MyGrid.Children.Clear();
            }
            else if(MyGrid.Children.Count == 1)
            {
                (MyGrid.Children[0] as FileControl).OnFileChanged((filePath, typeInfo.Ext));
                return;
            }
            FileControl fc = null;
            switch (typeInfo.Type)
            {
                case FileViewType.Image:
                    fc = new ImageControl();
                    break;
                case FileViewType.Code:
                    fc = new TextControl();
                    break;
                case FileViewType.Music:
                    fc = new MusicControl();
                    break;
                case FileViewType.Video:
                    fc = new VideoControl();
                    break;
                case FileViewType.Pdf:
                    fc = new PdfControl();
                    break;
                case FileViewType.Excel:
                    fc = new ExcelControl();
                    break;
                case FileViewType.Word:
                    fc = new WordControl();
                    break;
                case FileViewType.PowerPoint:
                    fc = new PowerPointControl();
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
