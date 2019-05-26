using DocumentFormat.OpenXml.Packaging;
using FileViewer.FileHelper;
using FileViewer.Globle;
using OpenXmlPowerTools;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace FileViewer.FileControl.Word
{
    class WordModel: BaseBackgroundWork, INotifyPropertyChanged, IFileChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            ShowBrowser = false;
            InitBackGroundWork();
            bgWorker.RunWorkerAsync(file.FilePath);
        }

        public string WordContent { get; private set; }
        public bool ShowBrowser { get; private set; }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filePath = e.Argument as string;
            using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, true))
            {
                HtmlConverterSettings settings = new HtmlConverterSettings()
                {
                    PageTitle = Path.GetFileName(filePath)
                };
                XElement html = HtmlConverter.ConvertToHtml(doc, settings);
                var htmlStr = html.ToStringNewLineOnAttributes();
                e.Result = htmlStr;
            }
        }

        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WordContent = (string)e.Result;
            GlobalNotify.OnLoadingChange(false);
            GlobalNotify.OnSizeChange(720, 1280);
            ShowBrowser = true;
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }
    }
}
