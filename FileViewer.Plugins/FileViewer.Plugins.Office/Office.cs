using FileViewer.Base;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;

namespace FileViewer.Plugins.Office
{
    class Office : BackgroundWorkBase, INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067

        readonly IManager manager;
        public Office(IManager manager)
        {
            this.manager = manager;
        }

        public IDocumentPaginatorSource? OfficeContent { get; set; }

        public bool ShowDocument { get; private set; }

        public void ChangeFile(string filePath)
        {
            InitBackGroundWork();
            ShowDocument = false;
            bgWorker?.RunWorkerAsync(filePath);
            ChangeTheme(manager.IsDarkMode());
        }

        public void ChangeTheme(bool dark)
        {
            manager.SetColor(dark ? Utils.BackgroundDark : Utils.BackgroundLight);
            //SetDocumentColor();
        }

        //private void SetDocumentColor()
        //{
        //    if (OfficeContent != null)
        //    {
        //        var dark = manager.IsDarkMode();
        //        var brush = dark ? new SolidColorBrush(Utils.BackgroundDark) : new SolidColorBrush(Colors.White);
        //        foreach (PageContent pageContent in OfficeContent.Pages)
        //        {
        //            FixedPage fixedPage = pageContent.GetPageRoot(true);
        //            if (fixedPage != null)
        //            {
        //                fixedPage.Background = brush;
        //                SetPageColor(fixedPage.Children, dark ? Colors.Black : Colors.White, dark ? Colors.White : Colors.Black);
        //            }
        //        }
        //    }
        //}

        //private void SetPageColor(UIElementCollection elements, Color oldColor, Color newColor)
        //{
        //    if (elements == null || elements.Count == 0) return;
        //    foreach (var child in elements)
        //    {
        //        if (child is System.Windows.Shapes.Path)
        //        {
        //            var shape = (child as System.Windows.Shapes.Path)!;
        //            var brush = shape.Fill as SolidColorBrush;
        //            if (brush?.Color.Equals(oldColor) ?? false)
        //            {
        //                shape.Fill = new SolidColorBrush(newColor);
        //            }

        //        }
        //        else if (child is System.Windows.Documents.Glyphs)
        //        {
        //            var glyphs = (child as System.Windows.Documents.Glyphs)!;
        //            var brush = glyphs.Fill as SolidColorBrush;
        //            if (brush?.Color.Equals(oldColor) ?? false)
        //            {
        //                glyphs.Fill = new SolidColorBrush(newColor);
        //            }
        //        }
        //        else if (child is Canvas)
        //        {
        //            SetPageColor((child as Canvas)!.Children, oldColor, newColor);
        //        }
        //    }
        //}

        protected override void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var filePath = e.Argument as string;
            if (Path.GetExtension(filePath!).ToLower().StartsWith(".xps"))
            {
                e.Result = filePath;
                return;
            }
            string xpsFilePath = Path.Combine(Path.GetTempPath(), Tools.File.CalculateFileMD5(filePath!) + ".xps");

            if (File.Exists(xpsFilePath) || ConvertOfficeToXps(filePath!, xpsFilePath))
            {
                e.Result = xpsFilePath;
            }
            else
            {
                e.Result = "LoadFailed: " + filePath;
            }
        }

        private bool ConvertOfficeToXps(string officeFilePath, string xpsFilePath)
        {
            try
            {
                var ext = Path.GetExtension(officeFilePath).ToLower();
                if (ext.StartsWith(".ppt"))
                {
                    return ConvertPowerPointToXps(officeFilePath, xpsFilePath);
                }
                else if (ext.StartsWith(".xls"))
                {
                    return ConvertExcelToXps(officeFilePath, xpsFilePath);
                }
                return ConvertWordToXps(officeFilePath, xpsFilePath);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool ConvertWordToXps(string wordFilePath, string xpsFilePath)
        {
            var wordApp = new Microsoft.Office.Interop.Word.Application();
            bool success = true;
            try
            {
                var wordDoc = wordApp.Documents.Open(wordFilePath);
                wordDoc.ExportAsFixedFormat(xpsFilePath, Microsoft.Office.Interop.Word.WdExportFormat.wdExportFormatXPS);
                wordDoc.Close(false);
            }
            catch (Exception) { success = false; }
            finally
            {
                wordApp.Quit(false);
            }
            return success;
        }

        private bool ConvertExcelToXps(string excelFilePath, string xpsFilePath)
        {
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook? workbook = null;
            bool success = true;
            try
            {
                excelApp.DisplayAlerts = false;
                workbook = excelApp.Workbooks.Open(excelFilePath);
                var formatType = Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypeXPS;
                var formatQuality = Microsoft.Office.Interop.Excel.XlFixedFormatQuality.xlQualityStandard;
                workbook.ExportAsFixedFormat(formatType, xpsFilePath, formatQuality);
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                // 关闭工作簿和 Excel 应用程序
                if (workbook != null)
                {
                    workbook.Close(false);
                    Marshal.ReleaseComObject(workbook);
                }
                excelApp.Quit();
                Marshal.ReleaseComObject(excelApp);
            }
            return success;
        }

        static bool ConvertPowerPointToXps(string powerPointFilePath, string xpsFilePath)
        {
            var powerPointApp = new Microsoft.Office.Interop.PowerPoint.Application();
            Microsoft.Office.Interop.PowerPoint.Presentation? presentation = null;
            bool success = true;
            try
            {
                presentation = powerPointApp.Presentations.Open(powerPointFilePath, Microsoft.Office.Core.MsoTriState.msoTrue, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse);
                var formatType = Microsoft.Office.Interop.PowerPoint.PpFixedFormatType.ppFixedFormatTypeXPS;
                var formatIntent = Microsoft.Office.Interop.PowerPoint.PpFixedFormatIntent.ppFixedFormatIntentScreen;
                presentation.ExportAsFixedFormat(xpsFilePath, formatType, formatIntent);
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                // 关闭演示文稿和 PowerPoint 应用程序
                if (presentation != null)
                {
                    presentation.Close();
                    Marshal.ReleaseComObject(presentation);
                }
                powerPointApp.Quit();
                Marshal.ReleaseComObject(powerPointApp);
            }
            return success;
        }

        protected override void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as string;
            if (result!.StartsWith("LoadFailed: "))
            {
                manager.LoadFileFailed(result.Substring(12));
                return;
            }
            using (XpsDocument xpsDocument = new XpsDocument(result, FileAccess.Read))
            {
                OfficeContent = xpsDocument.GetFixedDocumentSequence();
                //var xpsDocRef = xpsDocument.GetFixedDocumentSequence().References?.FirstOrDefault();
                //FixedDocument? xpsFixedDoc = xpsDocRef?.GetDocument(false);
                //OfficeContent = xpsFixedDoc;
            }
            manager.SetSize(720, 1280);
            manager.SetResizeMode(true);
            manager.SetLoading(false);
            ShowDocument = true;
        }

        protected override void BgWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
        }
    }
}
