using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

namespace FileViewer.Base
{
    public static class Utils
    {
        public readonly static Color BackgroundDark = Color.FromRgb(0x20, 0x20, 0x20);
        public readonly static Color BackgroundLight = Color.FromRgb(0xF3, 0xF3, 0xF3);// Color.FromRgb(0xEE, 0xF5, 0xFD);
        public readonly static Color BackgroundHello = Color.FromRgb(0xA1, 0xD5, 0xD3);
        public readonly static Bitmap? ImageHelloBack = null;// Properties.Resources.HelloBack;
        public readonly static Bitmap ImagePreview = Properties.Resources.preview;

        public static BitmapSource? GetBitmapSource(Bitmap? bmp)
        {
            if (bmp == null) return null;
            BitmapFrame bf;

            using (MemoryStream ms = new())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                bf = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
            return bf;
        }

        public static BitmapSource? GetBitmapSource(byte[]? data)
        {
            if (data == null) return null;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(data);
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public static BitmapSource? GetBitmapSource(Icon? icon)
        {
            if (icon == null) return null;
            BitmapSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }



        public static BitmapSource? GetFileThumbnail(string? filePath, bool large = true)
        {
            if (filePath == null) return null;
            try
            {
                ShellObject shellFile = ShellObject.FromParsingName(filePath);
                if (large)
                {
                    return shellFile.Thumbnail.ExtraLargeBitmapSource;
                }
                else
                {
                    return shellFile.Thumbnail.SmallBitmapSource;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void RebootApplication()
        {
            try
            {
                var psText = @"
param(
    [Parameter(Mandatory=$true)]
    [string]$currentPath
)
Start-Sleep -Seconds 1
Start-Process -FilePath $currentPath";

                var psFilePath = Path.Combine(Path.GetTempPath(), "app.reboot.ps1");
                File.WriteAllText(psFilePath, psText);
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{psFilePath}\" -currentPath \"{Environment.ProcessPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                };

                new Process { StartInfo = startInfo }.Start();
            }
            catch (Exception)
            {
                MessageBox.Show("重启出错，请手动启动应用！");
            }

            Environment.Exit(0);
        }
    }
}
