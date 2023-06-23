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
using FileViewer.Globle;
using FileViewer.Monitor;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace FileViewer.FileControl
{
    /// <summary>
    /// FileViewControl.xaml 的交互逻辑
    /// </summary>
    public partial class FileViewControl : UserControl
    {
        private Type lastViewType = null;

        private bool cloudflareOK = false;

        private bool isWebView2Installed = false;

        public FileViewControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            MyGrid.Children.Add(new HelloControl());
            GlobalNotify.FileLoadFailed += OnFileLoadFailed;
            cloudflareOK = Utils.CheckUrlOK("https://cdnjs.cloudflare.com");
            new ThemeListener().ThemeChanged += OnThemeChanged;
            CheckWebView2();
        }

        private async void CheckWebView2()
        {
            try
            {
                var webView2Environment = await CoreWebView2Environment.CreateAsync(userDataFolder: Path.Combine(Path.GetTempPath(), "WebView2"));
                isWebView2Installed = webView2Environment != null;
            }
            catch (Exception)
            {
            }
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
            [FileViewType.Text] = (typeof(TextControl), true),
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
            if(!controlMapping.TryGetValue(typeInfo.Type, out var controlInfo))
            {
                controlInfo = (typeof(CommonControl), false);
            }

            if(cloudflareOK && isWebView2Installed && typeInfo.Type == FileViewType.Text)
            {
                controlInfo.Type = typeof(PdfControl);
            }

            if(lastViewType == controlInfo.Type)
            {
                (MyGrid.Children[0] as FileControl).ChangeFile((filePath, typeInfo.Ext));
                return;
            }
            lastViewType = controlInfo.Type;
            MyGrid.Children.Clear();
            FileControl fc = (FileControl)Activator.CreateInstance(controlInfo.Type);

            GlobalNotify.OnResizeMode(controlInfo.ResizeMode);
            if (fc != null) LoadFile(fc, filePath, typeInfo.Ext);
        }

        private void LoadFile(FileControl fc, string filePath, FileExtension ext)
        {
            MyGrid.Children.Add(fc);
            fc.Margin = new Thickness(0);
            fc.ChangeFile((filePath, ext));
            fc.ChangeTheme(ThemeListener.IsDarkMode());
        }

        private void OnThemeChanged(bool isDark)
        {
            (MyGrid.Children[0] as FileControl).ChangeTheme(isDark);
        }
    }
}
