using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace FileViewer.FileControl
{
    public class Utils
    {
        public static BitmapSource GetBitmapSource(Bitmap bmp)
        {
            BitmapFrame bf = null;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bf = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
            return bf;
        }

        public static double FileSize(string filePath)
        {
            var f = new FileInfo(filePath);
            if (f.Exists)
            {
                return f.Length * 1.0 / 1048576;
            }
            return 0;
        }
    }
}
