using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;
using PListNet;
using System.Xml;
using FileViewer.FileHelper.IcnsParser;
using System.Net.Http;

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

        public static string OuterXmlFormat(this XmlDocument xmlDocument)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true; // Enable indenting
            settings.IndentChars = "  "; // Set the indent characters
            settings.NewLineChars = "\n"; // Set the newline characters
            settings.NewLineHandling = NewLineHandling.Replace; // Replace newlines with the NewLineChars string

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                xmlDocument.Save(writer);
            }

            return sb.ToString();
        }

        public static string LinkPath(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            if(extension != ".lnk") return filePath;
            var shellFile = ShellFile.FromFilePath(filePath);
            string targetPath = shellFile?.Properties.System.Link.TargetParsingPath.Value ?? filePath;
            if (!File.Exists(targetPath) && targetPath.Contains("Program Files (x86)"))
            {
                targetPath = targetPath.Replace("Program Files (x86)", "Program Files");
            }
            if (!File.Exists(targetPath)) targetPath = filePath;
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

        public static BitmapSource GetBitmapSource(Icon icon)
        {
            BitmapSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
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

        [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        private const int SW_NORMAL = 1;

        public static void ShowOpenWithDialog(string filePath)
        {
            SHELLEXECUTEINFO info = new SHELLEXECUTEINFO();
            info.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(info);
            info.lpVerb = "openas";
            info.lpFile = filePath;
            info.nShow = SW_NORMAL;
            ShellExecuteEx(ref info);
        }

        public static string CalculateMD5(string input)
        {
            // 创建MD5实例
            using (MD5 md5 = MD5.Create())
            {
                // 计算字符串的字节表示的哈希值
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

                // 将字节转换为十六进制字符串
                StringBuilder result = new StringBuilder(hash.Length * 2);

                for (int i = 0; i < hash.Length; i++)
                {
                    result.Append(hash[i].ToString("X2"));
                }

                return result.ToString();
            }
        }

        public static string CalculateFileMD5(string filePath)
        {
            // 创建一个MD5实例
            using (MD5 md5 = MD5.Create())
            {
                // 读取文件的字节并计算哈希值
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);

                    // 将字节转换为十六进制字符串
                    StringBuilder result = new StringBuilder(hash.Length * 2);
                    for (int i = 0; i < hash.Length; i++)
                    {
                        result.Append(hash[i].ToString("X2"));
                    }
                    return result.ToString();
                }
            }
        }

        public static string ParsePNodeString(PNode node, bool arrayFirst = false, bool arrayLast = false)
        {
            if (node == null) return string.Empty;
            var nodeType = node.GetType();
            if (nodeType == typeof(PListNet.Nodes.DictionaryNode))
            {
                PListNet.Nodes.DictionaryNode value = (PListNet.Nodes.DictionaryNode)node;
                return string.Join(", ", value.Values.Select(m => ParsePNodeString(m)));
            }
            else if (nodeType == typeof(PListNet.Nodes.BooleanNode))
            {
                PListNet.Nodes.BooleanNode value = (PListNet.Nodes.BooleanNode)node;
                return value.Value.ToString();
            }
            else if (nodeType == typeof(PListNet.Nodes.ArrayNode))
            {
                PListNet.Nodes.ArrayNode value = (PListNet.Nodes.ArrayNode)node;
                if (arrayFirst && value.Count > 0)
                {
                    return ParsePNodeString(value[0]);
                }
                else if(arrayLast && value.Count > 0)
                {
                    return ParsePNodeString(value[value.Count - 1]);
                }
                return string.Join(", ", value.Select(m => ParsePNodeString(m)));
            }
            else if (nodeType == typeof(PListNet.Nodes.DataNode))
            {
                PListNet.Nodes.DataNode value = (PListNet.Nodes.DataNode)node;
                return Convert.ToBase64String(value.Value);
            }
            else if (nodeType == typeof(PListNet.Nodes.DateNode))
            {
                PListNet.Nodes.DateNode value = (PListNet.Nodes.DateNode)node;
                return value.Value.ToString();
            }
            else if (nodeType == typeof(PListNet.Nodes.FillNode))
            {
                PListNet.Nodes.FillNode value = (PListNet.Nodes.FillNode)node;
                return value.ToString();
            }
            else if (nodeType == typeof(PListNet.Nodes.IntegerNode))
            {
                PListNet.Nodes.IntegerNode value = (PListNet.Nodes.IntegerNode)node;
                return value.Value.ToString();
            }
            else if (nodeType == typeof(PListNet.Nodes.RealNode))
            {
                PListNet.Nodes.RealNode value = (PListNet.Nodes.RealNode)node;
                return value.Value.ToString();
            }
            else if (nodeType == typeof(PListNet.Nodes.StringNode))
            {
                PListNet.Nodes.StringNode value = (PListNet.Nodes.StringNode)node;
                return value.Value.ToString();
            }
            else if (nodeType == typeof(PListNet.Nodes.UidNode))
            {
                PListNet.Nodes.UidNode value = (PListNet.Nodes.UidNode)node;
                return value.Value.ToString();
            }
            else
            {
                return node.ToString();
            }
        }

        public static Bitmap GetIcnsMax(string icnsPath)
        {
            return IcnsImageParser.GetImages(icnsPath).OrderByDescending(m => m.Bitmap.Height).FirstOrDefault().Bitmap;
        }

        public static bool CheckUrlOK(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)).Result;
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
