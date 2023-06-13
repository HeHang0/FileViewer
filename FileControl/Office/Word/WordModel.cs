using DocumentFormat.OpenXml.Packaging;
using FileViewer.FileHelper;
using FileViewer.Globle;
using OpenXmlPowerTools;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Xml.Linq;

namespace FileViewer.FileControl.Word
{
    class WordModel: BaseBackgroundWork, IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            ShowBrowser = false;
            InitBackGroundWork();
            bgWorker.RunWorkerAsync(file.FilePath);
            OnColorChanged(Colors.White);
        }

        public string WordContent { get; private set; }
        public bool ShowBrowser { get; private set; }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filePath = e.Argument as string;
            try
            {
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
            catch (Exception)
            {
                e.Result = "LoadFailed: " + filePath;
            }
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as string;
            if (result.StartsWith("LoadFailed: "))
            {
                GlobalNotify.OnFileLoadFailed(result.Substring(12));
                //PowerPointContent = Properties.Resources.Html404;
                //GlobalNotify.OnSizeChange(720, 1075);
                //OnColorChanged(Color.FromRgb(0xCB, 0xE8, 0xE6));
            }
            else
            {
                WordContent = result;
                GlobalNotify.OnSizeChange(720, 1280);
            }
            GlobalNotify.OnLoadingChange(false);
            ShowBrowser = true;
        }

        public void OnColorChanged(Color color)
        {
            GlobalNotify.OnColorChange(color);
        }
    }
}
