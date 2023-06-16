using System.Windows;
using System.Threading;
using System.Reflection;
using System.IO;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Xml;
using Microsoft.Web.WebView2.Core;

namespace FileViewer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        Mutex mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            mutex = new Mutex(true, appName, out bool createdNew);
            if (!createdNew)
            {
                // 应用程序的实例已经运行
                MessageBox.Show("应用程序已经在运行中");
                Current.Shutdown();
                return;
            }
            InitWebView2();
            InitAvalon();
        }

        private void InitAvalon()
        {
            using (MemoryStream stream = new MemoryStream(FileViewer.Properties.Resources.GolangSyntaxHighlighting))
            {
                using (XmlReader reader = new XmlTextReader(stream))
                {
                    var highlightingDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);

                    // 注册自定义的语法高亮定义
                    HighlightingManager.Instance.RegisterHighlighting("Golang", new string[] { ".go" }, highlightingDefinition);
                }
            }
        }

        private void InitWebView2()
        {
            var webView2Path = Path.Combine(Path.GetTempPath(), "WebView2");
            CoreWebView2Environment.SetLoaderDllFolderPath(webView2Path);
        }
    }
}
