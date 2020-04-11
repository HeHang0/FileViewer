using FileViewer.FileHelper;
using FileViewer.Globle;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FileViewer.FileControl.Text
{
    public class TextModel : BaseBackgroundWork, IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private TextEditor textEditor;
        public ICommand Loaded => new DelegateCommand<TextEditor>((editor) => {
            textEditor = editor;
            setText(textTmp);
        });

        private string textTmp = "";
        private void setText(string text)
        {
            if (text == "") return;
            if(textEditor != null)
            {
                textEditor.Text = text;
                textTmp = "";
            }
            else
            {
                textTmp = text;
            }
        }

        public IHighlightingDefinition SyntaxHighlighting { get; set; } = HighlightingManager.Instance.GetDefinitionByExtension(".txt");

        public ReadOnlyCollection<IHighlightingDefinition> HighlightingDefinitions => HighlightingManager.Instance.HighlightingDefinitions;



        private double height = SystemParameters.WorkArea.Height / 2;
        private double width = SystemParameters.WorkArea.Width / 2;

        private Encoding encodingSelected = Encoding.UTF8;
        public Encoding EncodeSelected
        {
            get
            {
                return encodingSelected;
            }
            set
            {
                encodingSelected = value;
                OnFileChanged(currentFilePath);
            }
        }

        public Dictionary<string, Encoding> EncodeList => new Dictionary<string, Encoding>()
        {
            {"ASCII", Encoding.ASCII},
            {"Default", Encoding.Default},
            {"Unicode", Encoding.Unicode},
            {"UTF8", Encoding.UTF8},
            {"GB2312", Encoding.GetEncoding("GB2312")},
            {"UTF32", Encoding.UTF32},
            {"BigEndianUnicode", Encoding.BigEndianUnicode},
            {"UTF7", Encoding.ASCII}
        };

        private (string FilePath, FileExtension Ext) currentFilePath;
        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            currentFilePath = file;
            if (file.Ext == FileExtension.JSON || file.Ext == FileExtension.VUE)
            {
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(".JS");
            }
            else
            {
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension("." + file.Ext.ToString().ToLower());
            }

            InitBackGroundWork();
            bgWorker.RunWorkerAsync(file.FilePath);
            GlobalNotify.OnSizeChange(height, width);
            OnColorChanged(Colors.White);
        }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (StreamReader st = new StreamReader((string)e.Argument, encodingSelected))
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
                    string result = sb.ToString().Substring(0, 10000);
                    if(i == 1000 || sb.Length > 10000)
                    {
                        result += "\r\n...";
                    }
                    e.Result = result;
                }
            }
            catch (Exception)
            {
                e.Result = "哈哈哈 没有读取到东西！！！";
            }
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }
        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            setText((string)e.Result);
            GlobalNotify.OnLoadingChange(false);
        }

        public void OnColorChanged(Color color)
        {
            GlobalNotify.OnColorChange(color);
        }
    }
}
