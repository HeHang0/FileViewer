﻿using FileViewer.FileHelper;
using Microsoft.WindowsAPICodePack.Shell;
using Prism.Commands;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FileViewer.Globle;
using System.Drawing;
using System.IO;
using Svg2Xaml;
using System.Windows.Media;
using WpfAnimatedGif;

namespace FileViewer.FileControl.Image
{
    public class ImageModel : BaseBackgroundWork, IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ImageSource ThumbnailImage { get; private set; }

        public void ChangeFile((string FilePath, FileExtension Ext) file)
        {
            try
            {
                switch (file.Ext)
                {
                    case FileExtension.GIF:
                        if (Utils.FileSize(file.FilePath) > 10) throw new FileLoadException();
                        ShowGif(file.FilePath);
                        break;
                    case FileExtension.SVG:
                        ShowSVG(file.FilePath);
                        break;
                    case FileExtension.ICNS:
                        ShowICNS(file.FilePath);
                        break;
                    default:
                        ShowImg(file);
                        break;
                }
            }
            catch (Exception)
            {
                ShowThumbnail(GetDefaultThumbnail(file.FilePath));
            }
            GlobalNotify.OnColorChange(Colors.White);
        }

        public void ChangeTheme(bool dark)
        {
        }

        public bool IsGif { get; private set; }

        public bool IsImg { get; private set; }

        public string FilePath { get; private set; }

        private void ShowGif(string filePath)
        {
            IsImg = false;
            IsGif = true;
            FilePath = filePath;

            byte[] data = null;
            using (FileStream fReader = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                data = new byte[fReader.Length];
                fReader.Read(data, 0, (int)fReader.Length);
            }
            if (data != null)
            {
                gifStream = new MemoryStream(data);
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = gifStream;
                image.EndInit();
                image.Freeze();
                ImageBehavior.SetAnimatedSource(gifImgCtrl, image);
                var height = image.Height;
                var width = image.Width;
                if (height > width)
                {
                    width += 5;
                }
                GlobalNotify.OnSizeChange(height, width);
            }

            GlobalNotify.OnLoadingChange(false);
            //GlobalNotify.OnSizeChange(400, 600);
        }

        private System.Windows.Controls.Image gifImgCtrl;
        private MemoryStream gifStream;
        public void setGifImageCtrl(System.Windows.Controls.Image imgCtrl)
        {
            gifImgCtrl = imgCtrl;
            imgCtrl.Unloaded += ImgCtrl_Unloaded;
        }

        private void ImgCtrl_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if(gifStream != null)
            {
                gifStream.Close();
                gifStream = null;
            }
        }

        private void ShowImg((string, FileExtension) file)
        {
            IsImg = true;
            IsGif = false;
            InitBackGroundWork();
            bgWorker.RunWorkerAsync(file);
        }

        private void ShowSVG(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IsImg = true;
                IsGif = false;
                ThumbnailImage = SvgReader.Load(stream);
                GlobalNotify.OnLoadingChange(false);
                GlobalNotify.OnSizeChange(ThumbnailImage.Height, ThumbnailImage.Width);
            }
        }

        private void ShowICNS(string filePath)
        {
            IsImg = true;
            IsGif = false;
            ThumbnailImage = Utils.GetBitmapSource(Utils.GetIcnsMax(filePath));
            GlobalNotify.OnLoadingChange(false);
            GlobalNotify.OnSizeChange(ThumbnailImage.Height, ThumbnailImage.Width);
        }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if(!(sender as BackgroundWorker).CancellationPending)
            {
                var result = AnalizeFile(((string, FileExtension))e.Argument);
                if(result == null)
                {
                    e.Result = e.Argument;
                }
                else
                {
                    e.Result = result;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private ImageSource GetDefaultThumbnail(string filePath)
        {
            ShellObject shellFile = ShellObject.FromParsingName(filePath);
            return shellFile.Thumbnail.ExtraLargeBitmapSource;
        }

        private void ShowThumbnail(ImageSource thumbnail)
        {
            IsImg = true;
            IsGif = false;
            ThumbnailImage = thumbnail;
            GlobalNotify.OnLoadingChange(false);
            var height = ThumbnailImage.Height;
            var width = ThumbnailImage.Width;
            //if (height < width)
            //{
            //    width -= 5;
            //}
            //else
            if (height > width)
            {
                width += 5;
            }
            GlobalNotify.OnSizeChange(height, width);
        }

        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Bitmap bm;
            ImageSource result;
            if(e.Result is Bitmap)
            {
                bm = (Bitmap)e.Result;
                result = Utils.GetBitmapSource(bm);
            }
            else
            {
                var file = ((string FilePath, FileExtension Ext))e.Result;
                result = GetDefaultThumbnail(file.FilePath);
            }
            ShowThumbnail(result);
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private Bitmap AnalizeFile((string FilePath, FileExtension Ext) file)
        {
            Bitmap bs = null;
            switch (file.Ext)
            {
                case FileExtension.PSD:
                    bs = ImageFromPSD(file.FilePath);
                    break;

            }
            return bs;
        }

        private Bitmap ImageFromPSD(string filePath)
        {
            Bitmap bf = null;
            try
            {
                //using (PsdImage image = (PsdImage)PsdImage.Load(filePath))
                //{
                //    foreach (var resource in image.ImageResources)
                //    {
                //        if (resource is ThumbnailResource)
                //        {
                //            var thumbnail = (ThumbnailResource)resource;
                //            if (thumbnail.Format == ThumbnailFormat.KJpegRgb)
                //            {
                //                BmpImage thumnailImage = new BmpImage(thumbnail.Width, thumbnail.Height);
                //                thumnailImage.SavePixels(thumnailImage.Bounds, thumbnail.ThumbnailData);
                //                bf = thumnailImage.ToBitmap();
                //            }
                //            break;
                //        }
                //    }
                //}
            }
            catch (Exception)
            {
            }
            return bf;// GetBitmapSource(FreeImageToBitmap.LoadImageFormFreeImage(filePath));
        }
    }
}
