using FileViewer.FileHelper;
using FileViewer.Globle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace FileViewer.FileControl.MobileProvision
{
    public class MobileProvisionModel : BaseBackgroundWork, IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MobileProvisionModel()
        {

        }

        public List<KeyValuePair<string, string>> BaseList { get; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> EntitlementsList { get; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> CertificatesList { get; } = new List<KeyValuePair<string, string>>();

        private double height = SystemParameters.WorkArea.Height / 2;
        private double width = SystemParameters.WorkArea.Width / 2;

        private (string FilePath, FileExtension Ext) currentFilePath;
        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            currentFilePath = file;
            InitBackGroundWork();
            bgWorker.RunWorkerAsync(file.FilePath);
            GlobalNotify.OnSizeChange(height, width);
            OnColorChanged(Colors.White);
        }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string text = File.ReadAllText((string)e.Argument);
                Match match = Regex.Match(text, "<plist[\\S\\s]+</plist>");
                if(match.Success)
                {
                    e.Result = match.Value;
                }
                else
                {
                    e.Result = "";
                }
            }
            catch (Exception)
            {
                e.Result = "";
            }
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }
        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string result = (string)e.Result;
            if (string.IsNullOrWhiteSpace(result))
            {
                GlobalNotify.OnFileLoadFailed(currentFilePath.FilePath);
                return;
            }
            processXML(result);
            GlobalNotify.OnLoadingChange(false);
        }

        private void processXML(string text)
        {

        }

        public void OnColorChanged(Color color)
        {
            GlobalNotify.OnColorChange(color);
        }
    }
}
