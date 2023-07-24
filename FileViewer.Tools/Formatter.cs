using System.Text;
using System.Xml;

namespace FileViewer.Tools
{
    public static class Formatter
    {
        const long TB = 1099511627776;
        const int GB = 1073741824;
        const int MB = 1048576;
        const int KB = 1024;
        public static string ToSizeString(this long b)
        {
            if (b > TB)
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

        public static string OuterXmlFormat(this XmlDocument xmlDocument)
        {
            StringBuilder sb = new();
            XmlWriterSettings settings = new()
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace
            };

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                xmlDocument.Save(writer);
            }

            return sb.ToString();
        }
    }
}