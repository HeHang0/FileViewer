using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
