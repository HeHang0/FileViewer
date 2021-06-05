using FileViewer.FileHelper;
using Prism.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileViewer.FileControl.Pdf
{
    public class PdfModel: IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string filePath;
        private FileExtension fileExt;
        private MoonPdfLib.MoonPdfPanel moonPdfPanel;

        public void OnFileChanged((string FilePath, FileExtension Ext) file)
        {
            OnColorChanged(Color.FromRgb(0x47,0x47, 0x47));
            if (filePath != file.FilePath)
            {
                filePath = file.FilePath;
                moonPdfPanel?.OpenFile(filePath);
                fileExt = file.Ext;
            }
            GlobalNotify.OnLoadingChange(false);
            openPdf(filePath);
        }

        public void OnColorChanged(Color color)
        {
            GlobalNotify.OnColorChange(color);
        }

        public ICommand GridLoaded => new DelegateCommand<MoonPdfLib.MoonPdfPanel>((panel) => {
            moonPdfPanel = panel;
            openPdf(filePath);
        });

        private System.Windows.Size pageSize = new System.Windows.Size(1280,720);
        private void openPdf(string filePath)
        {
            string outPath = string.Empty;
            if (fileExt != FileExtension.PDF)
            {
                try
                {
                    //outPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(filePath) + ".pdf");
                    //PdfvertService.ConvertFileToPdf(filePath, outPath);
                }
                catch (System.Exception e)
                {
                    outPath = string.Empty;
                }
            }
            if (moonPdfPanel != null && File.Exists(filePath))
            {
                if (File.Exists(outPath)) moonPdfPanel?.OpenFile(outPath);
                else moonPdfPanel?.OpenFile(filePath);
                var sizes = moonPdfPanel.PageBounds;
                if(sizes != null && sizes.Length > 0)
                {
                    pageSize = sizes[0];
                }
                TotalPages = moonPdfPanel.TotalPages;
                currentPage = moonPdfPanel.GetCurrentPageNumber();
                OnPropertyChanged("CurrentPage");
                moonPdfPanel.ViewType = MoonPdfLib.ViewType.SinglePage;
                GlobalNotify.OnSizeChange(pageSize.Height+47, pageSize.Width+34);
            }
        }

        private bool isFullScreen = false;
        public ICommand FullScreen => new DelegateCommand(() => {
            isFullScreen = !isFullScreen;
            GlobalNotify.OnFullScreen(isFullScreen);
        });

        public ICommand PageUp => new DelegateCommand(() => {
            moonPdfPanel.GotoPreviousPage();
        });

        public ICommand PageDown => new DelegateCommand(() => {
            moonPdfPanel.GotoNextPage();
        });

        public ICommand FirstPage => new DelegateCommand(() => {
            moonPdfPanel.GotoFirstPage();
        });

        public ICommand LastPage => new DelegateCommand(() => {
            moonPdfPanel.GotoLastPage();
        });

        public ICommand ZoomIn => new DelegateCommand(() => {
            moonPdfPanel.ZoomIn();
            ScaleList = new List<PageScale>() { new PageScale((int)(moonPdfPanel.CurrentZoom * 100) + "%", 0) };
            scaleSelected = ScaleList[0];
            OnPropertyChanged("ScaleSelected");
        });

        public ICommand ZoomOut => new DelegateCommand(() => {
            moonPdfPanel.ZoomOut();
            ScaleList = new List<PageScale>() { new PageScale((int)(moonPdfPanel.CurrentZoom * 100) + "%", 0) };
            scaleSelected = ScaleList[0];
            OnPropertyChanged("ScaleSelected");
        });

        public ICommand NormalView => new DelegateCommand(() => {
            moonPdfPanel.Zoom(1.0);
        });

        public ICommand FitToHeight => new DelegateCommand(() => {
            moonPdfPanel.ZoomToHeight();
        });

        public ICommand Single => new DelegateCommand(() => {
            moonPdfPanel.ViewType = MoonPdfLib.ViewType.SinglePage;
            GlobalNotify.OnSizeChange(pageSize.Height + 47, pageSize.Width + 34);
        });

        public ICommand Facing => new DelegateCommand(() => {
            moonPdfPanel.ViewType = MoonPdfLib.ViewType.Facing;
            GlobalNotify.OnSizeChange(pageSize.Height + 47, pageSize.Width*2 + 38);
        });

        public ICommand PageChanged => new DelegateCommand(() => {
            currentPage = moonPdfPanel.GetCurrentPageNumber();
            OnPropertyChanged("CurrentPage");
        });

        public ICommand ScalemouseDown => new DelegateCommand(() => {
            ScaleList = scaleList;
        });

        private int currentPage = 1;
        public int CurrentPage
        {
            get
            {
                return currentPage;
            }
            set
            {
                if(value > 0 && value <= TotalPages)
                {
                    moonPdfPanel.GotoPage(value);
                    currentPage = value;
                }
            }
        }

        public int TotalPages { get; private set; }

        private PageScale scaleSelected = scaleList[0];
        public PageScale ScaleSelected
        {
            get
            {
                return scaleSelected;
            }
            set
            {
                if (value == null) return;
                scaleSelected = value;
                switch (scaleSelected.Scale)
                {
                    case -1:
                        moonPdfPanel.ZoomToWidth();
                        break;
                    case -2:
                        moonPdfPanel.ZoomToHeight();
                        break;
                    case 100:
                        moonPdfPanel.Zoom(1.0);
                        break;
                    default:
                        moonPdfPanel.Zoom(scaleSelected.Scale*1.0/100);
                        break;
                }
            }
        }

        public List<PageScale> ScaleList { get; private set; } = scaleList;
        static readonly List<PageScale> scaleList = new List<PageScale>()
        {
            new PageScale("实际大小", 100),
            new PageScale("适合页宽", -1),
            new PageScale("适合页高", -2),
            new PageScale("50%", 50),
            new PageScale("75%", 75),
            new PageScale("100%", 100),
            new PageScale("125%", 125),
            new PageScale("150%", 150),
            new PageScale("200%", 200),
            new PageScale("300%", 300),
            new PageScale("400%", 400),
        };

        public BitmapSource IconPageUp => Utils.GetBitmapSource(Properties.Resources.toolbarButton_pageUp);
        public BitmapSource IconPageDown => Utils.GetBitmapSource(Properties.Resources.toolbarButton_pageDown);
        public BitmapSource IconZoomOut => Utils.GetBitmapSource(Properties.Resources.toolbarButton_zoomOut);
        public BitmapSource IconZoomIn => Utils.GetBitmapSource(Properties.Resources.toolbarButton_zoomIn);
        public BitmapSource IconpZesentationMode => Utils.GetBitmapSource(Properties.Resources.toolbarButton_presentationMode);
        public BitmapSource IconFirstPage => Utils.GetBitmapSource(Properties.Resources.toolbarButton_firstPage);
        public BitmapSource IconLastPage => Utils.GetBitmapSource(Properties.Resources.toolbarButton_lastPage);
        public BitmapSource IconSpreadNone => Utils.GetBitmapSource(Properties.Resources.toolbarButton_spreadNone);
        public BitmapSource IconSpreadOdd => Utils.GetBitmapSource(Properties.Resources.toolbarButton_spreadOdd);
    }

    public class PageScale
    {
        public PageScale(string name, int scale)
        {
            Name = name;
            Scale = scale;
        }
        public string Name { get; set; }
        public int Scale { get; set; }
    }
}
