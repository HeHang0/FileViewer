using FileViewer.FileHelper;
using FileViewer.Globle;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public IHighlightingDefinition SyntaxHighlighting { get; private set; }



        private double height = SystemParameters.WorkArea.Height / 2;
        private double width = SystemParameters.WorkArea.Width / 2;
        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
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
                using (StreamReader st = new StreamReader((string)e.Argument, Encoding.GetEncoding("GB2312")))
                {
                    StringBuilder sb = new StringBuilder();
                    string str = "";
                    int i = 0;
                    for (; i < 1000; i++)
                    {
                        str = st.ReadLine();
                        if (str == null) break;
                        sb.AppendLine(str);
                    }
                    if(i == 1000)
                    {
                        sb.AppendLine("...");
                    }
                    e.Result = sb.ToString();
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
