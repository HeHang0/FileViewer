using FileViewer.FileHelper;
using FileViewer.Globle;
using OfficeOpenXml;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

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
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    using (ExcelPackage excelPackage = new ExcelPackage(stream))
                    {
                        ExcelWorkbook workbook = excelPackage.Workbook;
                        if (workbook != null)
                        {
                            for (int c = 1; c <= workbook.Worksheets.Count; c++)
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.Append(@"<!Doctype html>
                                                       <html>
                                                           <head>
                                                               <meta charset=""UTF-8"">
                                                               <title>oo__H__oo</title>
                                                               <style>
                                                                   table{border-right:1px solid #ffc0cb;border-bottom:1px solid #ffc0cb} 
                                                                   table td{border-left:1px solid #ffc0cb;border-top:1px solid #ffc0cb} 
                                                               </style> 
                                                           </head><body><table>");
                                ExcelWorksheet worksheet = workbook.Worksheets[c-1];
                                for (int i = 0; i < worksheet.Dimension.End.Row; i++)
                                {
                                    string row = "<tr>";
                                    for (int j = 0; j < worksheet.Dimension.End.Column; j++)
                                    {
                                        string col = "<td>";
                                        try
                                        {
                                            col += worksheet.Cells[i, j].Value;
                                        }
                                        catch (Exception)
                                        {
                                        }
                                        col += "</td>";
                                        row += col;
                                    }
                                    row += "</tr>";
                                    stringBuilder.Append(row);
                                }
                                stringBuilder.Append("</table></body></html>");
                                bgw?.ReportProgress(c*100/workbook.Worksheets.Count, new SheetDisplay(worksheet.Name, stringBuilder.ToString()));
                            }
                        }
                    }
                }
                


            }
            catch (Exception ex)
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
                    //SheetWidth = item.offsetWidth;
                    //GlobalNotify.OnSizeChange(item.offsetHeight, item.offsetWidth + 20);
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
