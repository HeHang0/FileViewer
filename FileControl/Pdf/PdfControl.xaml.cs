using Microsoft.Web.WebView2.Core;
using System.IO;

namespace FileViewer.FileControl.Pdf
{
    /// <summary>
    /// PdfControl.xaml 的交互逻辑
    /// </summary>
    public partial class PdfControl : FileControl
    {
        public PdfControl():base(new PdfModel())
        {
            InitializeComponent();
            InitializeWebView2Async(Path.Combine(Path.GetTempPath(), "WebView2"));
        }

        private async void InitializeWebView2Async(string userDataFolderPath)
        {
            try
            {
                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolderPath);
                await webView2.EnsureCoreWebView2Async(environment);
            }
            catch (System.Exception)
            {
            }
        }
    }
}
