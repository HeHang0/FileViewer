using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FileViewer.FileControl.Pdf
{
    /// <summary>
    /// PdfControl.xaml 的交互逻辑
    /// </summary>
    public partial class PdfControl : FileControl
    {
        public PdfControl():base(new PdfModel())
        {
            var webView2Path = Path.Combine(Path.GetTempPath(), "WebView2");
            SetLoaderDllFolderPath(webView2Path);
            InitializeComponent();
            InitWebView2(webView2Path);
        }

        private async void InitWebView2(string userDataFolder)
        {
            try
            {
                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
                await webView2.EnsureCoreWebView2Async(environment);
            }
            catch (Exception)
            {
            }
        }

        private void SetLoaderDllFolderPath(string webView2Path)
        {
            var loaderPath = Path.Combine(webView2Path, "WebView2Loader.dll");
            if (!Directory.Exists(webView2Path))
            {
                Directory.CreateDirectory(webView2Path);
            }
            if (!File.Exists(loaderPath))
            {
                File.WriteAllBytes(loaderPath, Properties.Resources.WebView2Loader);
            }
        }
    }
}
