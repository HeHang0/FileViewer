using ApkReader;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using FileViewer.Globle;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace FileViewer.FileControl.Pdf
{
    /// <summary>
    /// PdfControl.xaml 的交互逻辑
    /// </summary>
    public partial class PdfControl : FileControl
    {
        private PdfModel _model => (PdfModel)model;
        public PdfControl():base(new PdfModel())
        {
            var webView2Path = Path.Combine(Path.GetTempPath(), "WebView2");
            SetLoaderDllFolderPath(webView2Path);
            _model.TextFileChanged += TextFileChanged;
            InitializeComponent();
            InitWebView2(webView2Path);
        }

        private async void InitWebView2(string userDataFolder)
        {
            try
            {
                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
                await webView2.EnsureCoreWebView2Async(environment);
                webView2.WebMessageReceived += WebMessageReceived;
            }
            catch (Exception)
            {
            }
        }

        private void TextFileChanged()
        {
            var htmlPath = Path.Combine(Path.GetTempPath(), "WebView2", "monaco_editor.html");
            if (!File.Exists(htmlPath))
            {
                File.WriteAllText(htmlPath, Properties.Resources.monaco_editor);
            }
            if(_model.PdfFilePath == htmlPath)
            {
                ShowTextWithWebView2(_model.RealFilePath);
            }
            else
            {
                _model.PdfFilePath = htmlPath;
            }
        }

        private static readonly long MAX_Text_LENGTH = 10485760;
        private void ShowTextWithWebView2(string filePath)
        {
            webView2.CoreWebView2.PostWebMessageAsJson($"{{\"message\": \"extension\",\"value\": \"{Path.GetExtension(filePath)}\"}}");
            string content;
            var fileInfo = new FileInfo(filePath);
            var buffer = new char[Math.Min(MAX_Text_LENGTH, fileInfo.Length)];

            using (StreamReader st = new StreamReader(filePath, true))
            {
                int length = st.ReadBlock(buffer, 0, buffer.Length);
                content = new string(buffer.Take(length).ToArray());
            }
            if (Path.GetFileName(filePath).ToLower() == "androidmanifest.xml" && !content.TrimStart().StartsWith("<"))
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    XmlDocument xmlDocument = BinaryXmlConvert.ToXmlDocument(stream);
                    content = xmlDocument.OuterXmlFormat();
                }
            }
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "message", "text" },
                { "value", content }
            };
            webView2.CoreWebView2.PostWebMessageAsJson(Encoding.UTF8.GetString(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data)));
        }

        private void WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            switch (message)
            {
                case "loaded":
                    ShowTextWithWebView2(_model.RealFilePath);
                    break;
            }
        }

        private void SetLoaderDllFolderPath(string webView2Path)
        {
            var loaderPath = Path.Combine(webView2Path, "WebView2Loader.dll");
            if (!Directory.Exists(webView2Path))
            {
                Directory.CreateDirectory(webView2Path);
            }
            var architecture = RuntimeInformation.ProcessArchitecture;
            var architectureName = architecture.ToString().ToLower();
            var architecturePath = loaderPath + ".architecture";
            if (File.Exists(loaderPath) && 
                File.Exists(architecturePath) && 
                File.ReadAllText(architecturePath) == architectureName)
            {
                return;
            }
            try
            {
                File.WriteAllText(architecturePath, architectureName);
                switch (architecture)
                {
                    case Architecture.X86:
                        File.WriteAllBytes(loaderPath, Properties.Resources.WebView2Loader_x86);
                        break;
                    case Architecture.X64:
                        File.WriteAllBytes(loaderPath, Properties.Resources.WebView2Loader_x64);
                        break;
                    case Architecture.Arm64:
                        File.WriteAllBytes(loaderPath, Properties.Resources.WebView2Loader_arm64);
                        break;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
