using FileViewer.FileHelper;
using FileViewer.Globle;
using NPOI.SS.Converter;
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
using System.Windows.Media;
using System.Xml;

namespace FileViewer.FileControl.Excel
{
    class ExcelModel : BaseBackgroundWork, IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnFileChanged((string FilePath, FileExtension Ext)file)
        {
            InitBackGroundWork();
            SheetList.Clear();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.RunWorkerAsync(file.FilePath);
        }

        public ObservableCollection<SheetDisplay> SheetList { get; } =  new ObservableCollection<SheetDisplay>();

        public SheetDisplay SheetItem { get; set; }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var bgw = sender as BackgroundWorker;
            var filePath = e.Argument as string;
            try
            {
                var workbook = ExcelToHtmlUtils.LoadXls(filePath);
                for (int i = 1; i <= workbook.NumberOfSheets; i++)
                {
                    var sheet = workbook.GetSheetAt(i-1);
                    var wb = new NPOI.HSSF.UserModel.HSSFWorkbook();
                    wb.Add(sheet);
                    ExcelToHtmlConverter excelToHtmlConverter = new ExcelToHtmlConverter();
                    excelToHtmlConverter.OutputColumnHeaders = false;
                    excelToHtmlConverter.OutputHiddenColumns = false;
                    excelToHtmlConverter.OutputHiddenRows = false;
                    excelToHtmlConverter.OutputLeadingSpacesAsNonBreaking = false;
                    excelToHtmlConverter.OutputRowNumbers = false;
                    excelToHtmlConverter.UseDivsToSpan = false;
                    excelToHtmlConverter.ProcessWorkbook(wb);
                    var body = excelToHtmlConverter.Document.GetElementsByTagName("body")[0] as XmlElement;
                    body.RemoveChild(body.FirstChild);
                    body.SetAttribute("style", "overflow:auto");
                    bgw?.ReportProgress(i == workbook.NumberOfSheets ? 100 : i, new SheetDisplay(sheet.SheetName, excelToHtmlConverter.Document.InnerXml));
                }
            }
            catch (Exception)
            {
                //bgw?.ReportProgress(0, new SheetDisplay("oo__H__oo", Properties.Resources.Html404));
                bgw?.ReportProgress(0, new SheetDisplay("oo__H__oo", filePath));
            }
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.ProgressPercentage == 0)
            {
                GlobalNotify.OnFileLoadFailed((e.UserState as SheetDisplay).SheetContent);
                return;
                //GlobalNotify.OnSizeChange(720, 1075);
                //OnColorChanged(Color.FromRgb(0xCB, 0xE8, 0xE6));
            }
            else if(e.ProgressPercentage == 100)
            {
                OnColorChanged(Colors.White);
            }
            SheetList.Add(e.UserState as SheetDisplay);
        }

        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GlobalNotify.OnLoadingChange(false);
            SheetItem = SheetList.FirstOrDefault();
        }

        public void OnColorChanged(System.Windows.Media.Color color)
        {
            GlobalNotify.OnColorChange(color);
        }

        public class SheetDisplay: INotifyPropertyChanged
        {
            public SheetDisplay(string name, string content)
            {
                SheetName = name;
                SheetContent = content;
            }
            public string SheetName { get; set; }
            public string SheetContent { get; set; }
            public double SheetWidth { get; set; } = 1080;

            public ICommand LoadCompleted => new DelegateCommand<System.Windows.Controls.WebBrowser>((wb) =>
            {
                dynamic document = wb.Document;
                var tables = document.getElementsByTagName("table");
                foreach (dynamic item in tables)
                {
                    SheetWidth = item.offsetWidth;
                    GlobalNotify.OnSizeChange(item.offsetHeight, item.offsetWidth + 20);
                    break;
                }
            });

            public ICommand Navigating => new DelegateCommand<System.Windows.Controls.WebBrowser>((wb) =>
            {
                SheetWidth = 1080;
                GlobalNotify.OnSizeChange(1920, 1080);
            });

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}
