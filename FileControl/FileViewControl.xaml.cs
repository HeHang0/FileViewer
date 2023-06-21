using FileViewer.FileControl.App;
using FileViewer.FileControl.Common;
using FileViewer.FileControl.Hello;
using FileViewer.FileControl.Image;
using FileViewer.FileControl.MobileProvision;
using FileViewer.FileControl.Music;
using FileViewer.FileControl.Office;
using FileViewer.FileControl.Pdf;
using FileViewer.FileControl.Text;
using FileViewer.FileControl.Video;
using FileViewer.FileHelper;
using System;
using System.Collections.Generic;
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

        private Dictionary<FileViewType, (Type Type, bool ResizeMode)> controlMapping = new Dictionary<FileViewType, (Type, bool)> 
        {
            [FileViewType.Image] = (typeof(ImageControl), true),
            [FileViewType.Code] = (typeof(TextControl), true),
            [FileViewType.Txt] = (typeof(TextControl), true),
            [FileViewType.Music] = (typeof(MusicControl), false),
            [FileViewType.Video] = (typeof(VideoControl), true),
            [FileViewType.Pdf] = (typeof(PdfControl), true),
            [FileViewType.Excel] = (typeof(OfficeControl), true),
            [FileViewType.Word] = (typeof(OfficeControl), true),
            [FileViewType.PowerPoint] = (typeof(OfficeControl), true),
            [FileViewType.MobileProvision] = (typeof(MobileProvisionControl), true),
            [FileViewType.App] = (typeof(AppControl), true)
        };

        private void SetResource(string filePath, bool loadWithTypeNone = false)
        {
            if(filePath == null || filePath.Trim() == string.Empty) return;
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
            bool resizeMode;
            if (controlMapping.TryGetValue(typeInfo.Type, out var controlInfo))
            {
                fc = (FileControl)Activator.CreateInstance(controlInfo.Type);
                resizeMode = controlInfo.ResizeMode;
            }
            else
            {
                fc = new CommonControl();
                resizeMode = false;
            }

            GlobalNotify.OnResizeMode(resizeMode);
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
