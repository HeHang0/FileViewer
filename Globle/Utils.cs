using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FileViewer.Globle
{
    public static class Utils
    {
        const long TB = 1099511627776;
        const int GB = 1073741824;
        const int MB = 1048576;
        const int KB = 1024;
        public static string ToSizeString(this long b)
        {
            if(b > TB)
            {
                return Math.Round(b * 1.0 / TB, 2) + " TB";
            }

            if (b > GB)
            {
                return Math.Round(b * 1.0 / GB, 2) + " GB";
            }


            if (b > MB)
            {
                return Math.Round(b * 1.0 / MB, 2) + " MB";
            }


            if (b > KB)
            {
                return Math.Round(b * 1.0 / KB, 2) + " KB";
            }


            return b + " B";
        }

        public static string LinkPath(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            if(extension != ".lnk") return filePath;
            var shellFile = ShellFile.FromFilePath(filePath);
            string targetPath = shellFile?.Properties.System.Link.TargetParsingPath.Value ?? filePath;
            if (!System.IO.File.Exists(targetPath) && targetPath.Contains("Program Files (x86)"))
            {
                targetPath = targetPath.Replace("Program Files (x86)", "Program Files");
            }
            if (!System.IO.File.Exists(targetPath)) targetPath = filePath;
            return targetPath;
        }

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
