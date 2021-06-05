using FileViewer.FileHelper;
using FileViewer.Globle;
using Prism.Commands;
using Spire.Presentation;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
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
                e.Result = "LoadFailed: "+filePath;
            }
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = e.Result as string;
            if(result.StartsWith("LoadFailed: "))
            {
                GlobalNotify.OnFileLoadFailed(result.Substring(12));
                //PowerPointContent = Properties.Resources.Html404;
                //GlobalNotify.OnSizeChange(720, 1075);
                //OnColorChanged(Color.FromRgb(0xCB, 0xE8, 0xE6));
            }
            else
            {
                PowerPointContent = result;
                GlobalNotify.OnSizeChange(720, 1280);
                GlobalNotify.OnLoadingChange(false);
                ShowBrowser = true;
            }
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
            System.Drawing.PointF scaleUI = GetCurrentDIPScale();
            if (100 != (int)(scaleUI.X * 100))
            {
                SetZoom(wb, (int)(scaleUI.X * scaleUI.Y * 100));
            }
        });

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(
      IntPtr hdc,         // handle to DC
            int nIndex          // index of capability
        );

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        static extern IntPtr GetDC(IntPtr Hwnd); //其在MSDN中原型为HDC GetDC(HWND hWnd),HDC和HWND都是驱动器句柄（长指针），在C#中只能用IntPtr代替了
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        const int LOGPIXELSX = 88;
        const int LOGPIXELSY = 90;

        System.Drawing.PointF GetCurrentDIPScale()
        {
            System.Drawing.PointF scaleUI = new System.Drawing.PointF(1.0f, 1.0f);
            try
            {
                SetProcessDPIAware();
                IntPtr screenDC = GetDC(IntPtr.Zero);
                int dpi_x = GetDeviceCaps(screenDC, LOGPIXELSX);
                int dpi_y = GetDeviceCaps(screenDC, LOGPIXELSY);

                scaleUI.X = (float)dpi_x / 96.0f;
                scaleUI.Y = (float)dpi_y / 96.0f;
                ReleaseDC(IntPtr.Zero, screenDC);
                return scaleUI;
            }
            catch (Exception)
            {
            }

            return scaleUI;
        }


        /// <summary>
        /// The flags are used to zoom web browser's content.
        /// </summary>
        readonly int OLECMDEXECOPT_DODEFAULT = 0;
        readonly int OLECMDID_OPTICAL_ZOOM = 63;

        void SetZoom(WebBrowser webbrowser, int zoom)
        {
            try
            {
                if (null == webbrowser)
                {
                    return;
                }

                FieldInfo fiComWebBrowser = webbrowser.GetType().GetField(
                  "_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
                if (null != fiComWebBrowser)
                {
                    Object objComWebBrowser = fiComWebBrowser.GetValue(webbrowser);

                    if (null != objComWebBrowser)
                    {
                        object[] args = new object[]
                        {
              OLECMDID_OPTICAL_ZOOM,
              OLECMDEXECOPT_DODEFAULT,
              zoom,
              IntPtr.Zero
                        };
                        objComWebBrowser.GetType().InvokeMember(
                          "ExecWB",
                          BindingFlags.InvokeMethod,
                          null, objComWebBrowser,
                          args);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
