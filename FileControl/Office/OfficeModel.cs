using FileViewer.FileHelper;
using FileViewer.Globle;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps.Packaging;

namespace FileViewer.FileControl.Office
{
    internal class OfficeModel: BaseBackgroundWork, IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            ShowBrowser = false;
            InitBackGroundWork();
            bgWorker.RunWorkerAsync(file.FilePath);
            GlobalNotify.OnColorChange(Color.FromRgb(0xEE, 0xF5, 0xFD));
        }

        public IDocumentPaginatorSource OfficeContent { get; set; }
        public bool ShowBrowser { get; private set; }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filePath = e.Argument as string;
            string xpsFilePath = Path.Combine(Path.GetTempPath(), Utils.CalculateFileMD5(filePath) + ".xps");
            
            if (File.Exists(xpsFilePath) || ConvertOfficeToXps(filePath, xpsFilePath))
            {
                e.Result = xpsFilePath;
            }
            else
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
            }
            else
            {
                XpsDocument xpsDocument = new XpsDocument(result, FileAccess.Read);
                OfficeContent = xpsDocument.GetFixedDocumentSequence();
                GlobalNotify.OnSizeChange(720, 1280);
            }
            ShowBrowser = true;
            GlobalNotify.OnLoadingChange(false);
        }

        public void OnColorChanged(Color color)
        {
            GlobalNotify.OnColorChange(color);
        }

        private bool ConvertOfficeToXps(string officeFilePath, string xpsFilePath)
        {
            if (!IsOfficeInstalled()) return false;
            var ext = Path.GetExtension(officeFilePath).ToLower();
            if(ext.StartsWith(".ppt"))
            {
                return ConvertPowerPointToXps(officeFilePath, xpsFilePath);
            }
            else if(ext.StartsWith(".xls"))
            {
                return ConvertExcelToXps(officeFilePath, xpsFilePath);
            }
            return ConvertWordToXps(officeFilePath, xpsFilePath);
        }

        private static bool IsOfficeInstalled()
        {
            // 检查注册表以确定是否安装了Office
            RegistryKey officeKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Office");
            return officeKey != null;
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
            Microsoft.Office.Interop.Excel.Workbook workbook = null;
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
            Microsoft.Office.Interop.PowerPoint.Presentation presentation = null;
            bool success = true;
            try
            {
                presentation = powerPointApp.Presentations.Open(powerPointFilePath, Microsoft.Office.Core.MsoTriState.msoTrue, Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoFalse);
                var formatType = Microsoft.Office.Interop.PowerPoint.PpFixedFormatType.ppFixedFormatTypeXPS;
                var formatIntent = Microsoft.Office.Interop.PowerPoint.PpFixedFormatIntent.ppFixedFormatIntentScreen;
                presentation.ExportAsFixedFormat(xpsFilePath, formatType, formatIntent);
            }
            catch (Exception ex)
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
    }
}
