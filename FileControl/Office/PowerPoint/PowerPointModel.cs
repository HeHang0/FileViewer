using FileViewer.FileHelper;
using FileViewer.Globle;
using Prism.Commands;
using Spire.Presentation;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FileViewer.FileControl.PowerPoint
{
    public class PowerPointModel : BaseBackgroundWork, IFileModel
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            ShowBrowser = false;
            InitBackGroundWork();
            bgWorker.RunWorkerAsync(file.FilePath);
            GlobalNotify.OnLoadingChange(true);
            OnColorChanged(Colors.White);
        }

        public string PowerPointContent { get; private set; }
        public bool ShowBrowser { get; private set; }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var bgw = sender as BackgroundWorker;
            var filePath = e.Argument as string;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Presentation ppt = new Presentation();
                    ppt.LoadFromFile(filePath);
                    ppt.SaveToFile(ms, FileFormat.Html);
                    byte[] b = ms.ToArray();
                    e.Result = System.Text.Encoding.UTF8.GetString(b, 0, b.Length);
                }
            }
            catch (Exception)
            {
                e.Result = string.Empty;
            }
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as string;
            if(result == string.Empty)
            {
                PowerPointContent = Properties.Resources.Html404;
                GlobalNotify.OnSizeChange(720, 1075);
                OnColorChanged(Color.FromRgb(0xCB, 0xE8, 0xE6));
            }
            else
            {
                PowerPointContent = result;
                GlobalNotify.OnSizeChange(720, 1280);
            }
            GlobalNotify.OnLoadingChange(false);
            ShowBrowser = true;
        }

        public void OnColorChanged(System.Windows.Media.Color color)
        {
            GlobalNotify.OnColorChange(color);
        }

        public ICommand LoadCompleted => new DelegateCommand<WebBrowser>((wb) => 
        {
            dynamic document = wb.Document;
            double width = document.getElementsByTagName("html")[0].scrollWidth;
            GlobalNotify.OnSizeChange(720, width + 25);
            dynamic slides = document.getElementsByClassName("slide");
            for (var i = 0; i < slides.length; i++)
            {
                slides[i].getElementsByTagName("svg")[0].getElementsByTagName("g")[0].lastElementChild.innerHTML = "";
            }
        });
    }
}
