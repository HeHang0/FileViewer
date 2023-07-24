using FileViewer.Base;
using FileViewer.Icns.IcnsParser;
using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FileViewer.Plugins.Image
{
    /// <summary>
    /// Interaction logic for ImageControl.xaml
    /// </summary>
    public partial class ImageControl : UserControl
    {
        readonly IManager _manager;
        public ImageControl(IManager manager)
        {
            _manager = manager;
            InitializeComponent();
        }

        public void ChangeFile(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            try
            {
                if (extension == ".icns")
                {
                    var bitmap = IcnsImageParser.GetImages(filePath).OrderByDescending(m => m.Bitmap.Height).FirstOrDefault()?.Bitmap;
                    if (bitmap != null)
                    {
                        ThumbnailImage.Source = Utils.GetBitmapSource(bitmap);
                    }
                    else
                    {
                        throw new ArgumentNullException(filePath, "Bitmap");
                    }
                }
                else
                {
                    ThumbnailImage.Source = new BitmapImage(new Uri(filePath, UriKind.Absolute));
                }
                _manager.LoadFileSuccess(
                    ThumbnailImage.ActualHeight + 34,
                    ThumbnailImage.ActualWidth,
                    true, false,
                    _manager.IsDarkMode() ? Utils.BackgroundDark : Utils.BackgroundLight
                    );
            }
            catch (Exception)
            {
                _manager.LoadFileFailed(filePath);
            }
        }

        public void ChangeTheme(bool dark)
        {
            _manager?.SetColor(dark ? Utils.BackgroundDark : Utils.BackgroundLight);
        }
    }
}