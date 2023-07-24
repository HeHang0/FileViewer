using ApkReader;
using FileViewer.FileHelper;
using FileViewer.Globle;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using Prism.Commands;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace FileViewer.FileControl.Text
{
    public class TextModel : BaseBackgroundWork, IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IHighlightingDefinition SyntaxHighlighting { get; set; } = HighlightingManager.Instance.GetDefinitionByExtension(".txt");

        public ReadOnlyCollection<IHighlightingDefinition> HighlightingDefinitions => HighlightingManager.Instance.HighlightingDefinitions;

        public TextDocument Document { get; } = new TextDocument();
        public Brush Background { get; set; }

        private readonly double height = SystemParameters.WorkArea.Height / 2;
        private readonly double width = SystemParameters.WorkArea.Width / 2;

        public void ChangeFile((string FilePath, FileExtension Ext) file)
        {
            if (file.Ext == FileExtension.TS || file.Ext == FileExtension.VUE)
            {
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".JS");
            }
            else
            {
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(file.FilePath));
            }

            InitBackGroundWork();
            bgWorker.RunWorkerAsync(file.FilePath);
            GlobalNotify.OnSizeChange(height, width);
        }

        public void ChangeTheme(bool dark)
        {
            var color = dark ? Color.FromRgb(0x33, 0x33, 0x33) : Color.FromRgb(0xEE, 0xF5, 0xFD);
            GlobalNotify.OnColorChange(color);
            Background = new SolidColorBrush(color);
        }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filePath = (string)e.Argument;
            try
            {
                using (StreamReader st = new StreamReader(filePath, true))
                {
                    StringBuilder sb = new StringBuilder();
                    string str = "";
                    int i = 0;
                    for (; i < 1000; i++)
                    {
                        str = st.ReadLine();
                          if (str == null) break;
                        sb.AppendLine(str);
                        if (sb.Length > 10000) break;
                    }
                    string result = sb.ToString().Substring(0, Math.Min(10000, sb.Length));
                    if(i == 1000 || sb.Length > 10000)
                    {
                        result += "\r\n...";
                    }
                    e.Result = result;
                }
                if(Path.GetFileName(filePath).ToLower() == "androidmanifest.xml" && !((string)e.Result).TrimStart().StartsWith("<"))
                {
                    using(FileStream stream = File.OpenRead(filePath))
                    {
                        XmlDocument xmlDocument = BinaryXmlConvert.ToXmlDocument(stream);
                        e.Result = xmlDocument.OuterXmlFormat();
                    }
                }
            }
            catch (Exception)
            {
                if(e.Result == null) e.Result = "哈哈哈 没有读取到东西！！！";
            }
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Document.Text = (string)e.Result;
            if (SyntaxHighlighting == null && Document.Text.StartsWith("<"))
            {
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".xml");
            }

            GlobalNotify.OnLoadingChange(false);
        }
    }
}
