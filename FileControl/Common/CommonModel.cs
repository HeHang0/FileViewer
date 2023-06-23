using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using FileViewer.FileHelper;
using FileViewer.Globle;
using Microsoft.WindowsAPICodePack.Shell;

namespace FileViewer.FileControl.Common
{
    class CommonModel : BaseBackgroundWork, IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource ThumbnailImage { get; private set; }

        public ImageSource HelloBack => Utils.GetBitmapSource(Properties.Resources.HelloBack);

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                var nameBytes = Encoding.Default.GetBytes(value);
                if (nameBytes.Length > 33)
                {
                    name = value;
                    NameEllipsis = Encoding.Default.GetString(nameBytes.Take(30).ToArray()) + "...";
                }
                else
                {
                    name = string.Empty;
                    NameEllipsis = value;
                }
            }
        }

        public string NameEllipsis { get; set; }

        public string Size { get; set; }

        public string Quota { get; set; }

        public string CreateTime { get; set; }

        public string ModifyTime { get; set; }

        public void OnColorChanged(Color color)
        {
            GlobalNotify.OnColorChange(color);
        }

        public void ChangeFile((string FilePath, FileExtension Ext) file)
        {
            OnColorChanged(Color.FromRgb(0xA1, 0xD5, 0xD3));
            GlobalNotify.OnSizeChange(450, 800);
            ShellObject shellFile = ShellObject.FromParsingName(Utils.LinkPath(file.FilePath));
            ThumbnailImage =  shellFile.Thumbnail.ExtraLargeBitmapSource;
            GlobalNotify.OnLoadingChange(false);
            Size = "正在计算大小...";
            fileSize = 0;
            directoryCount = 0;
            InitBackGroundWork();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.RunWorkerAsync(file.FilePath);
        }

        public void ChangeTheme(bool dark)
        {
        }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var bgw = sender as BackgroundWorker;
            FileInfo fileInfo = new FileInfo(e.Argument as string);
            bgw?.ReportProgress(Convert.ToInt32(FileAttr.Name), fileInfo.Name == string.Empty ? fileInfo.FullName : fileInfo.Name);
            bgw?.ReportProgress(Convert.ToInt32(FileAttr.CreateTime), fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));
            bgw?.ReportProgress(Convert.ToInt32(FileAttr.ModifyTime), fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
            if(Directory.Exists(e.Argument as string))
            {
                long dirLength = GetDirectorySize(bgw, e.Argument as string);
                bgw?.ReportProgress(Convert.ToInt32(FileAttr.Size), $"{dirLength.ToSizeString()} ({dirLength}字节)");
            }
            else
            {
                bgw?.ReportProgress(Convert.ToInt32(FileAttr.Size), $"{fileInfo.Length.ToSizeString()} ({fileInfo.Length}字节)");
            }
        }

        private long GetDirectorySize(BackgroundWorker bgw, string dirPath = "")
        {
            if (bgw.CancellationPending) return 0;
            if(string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath))
            {
                return 0;
            }
            Console.WriteLine("开始等待获取文件夹");
            DateTime startTime = DateTime.Now;
            List<string> directoryList = GetDirectories(bgw, dirPath);
            Console.WriteLine($"已经获取文件夹，耗时{(DateTime.Now - startTime).TotalMilliseconds}毫秒");
            Console.WriteLine("开始等待获取文件大小");
            startTime = DateTime.Now;
            List<ManualResetEvent> manualEvents = new List<ManualResetEvent>();
            ConcurrentBag<long> cbList = new ConcurrentBag<long>();
            for (int i = 0; i < directoryList.Count; i++)
            {
                WaitGroup wg = new WaitGroup()
                {
                    Mre = new ManualResetEvent(false),
                    DirPath = directoryList[i]
                };
                manualEvents.Add(wg.Mre);
                ThreadPool.QueueUserWorkItem(waitGroup =>
                {
                    try
                    {
                        foreach (var fileInfo in new DirectoryInfo((waitGroup as WaitGroup).DirPath).GetFiles())
                        {
                            cbList.Add(fileInfo.Length);
                            bgw?.ReportProgress(Convert.ToInt32(FileAttr.SegSize), fileInfo.Length);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    (waitGroup as WaitGroup).Mre.Set();
                }, wg);
                if(manualEvents.Count == 64)
                {
                    WaitHandle.WaitAll(manualEvents.ToArray());
                    manualEvents.Clear();
                }
            }
            if(manualEvents.Count > 0) WaitHandle.WaitAll(manualEvents.ToArray());
            Console.WriteLine($"已经获取文件夹，耗时{(DateTime.Now - startTime).TotalMilliseconds}毫秒");
            long dirLength = cbList.Sum();            
            return dirLength;
        }

        class WaitGroup
        {
            public ManualResetEvent Mre { get; set; }
            public string DirPath { get; set; }
        }

        //private long GetDirectorySizeSingle(object directoryInfo)
        //{
        //    long dirLength = 0;
        //    foreach (var fileInfo in directoryInfo.GetFiles())
        //    {
        //        dirLength += fileInfo.Length;
        //    }
        //    return dirLength;
        //}

        private List<string> GetDirectories(BackgroundWorker bgw, string dirPath)
        {
            List<string> directoryList = new List<string>();
            if (string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath)) return directoryList;
            if (!dirPath.EndsWith("\\")) dirPath += "\\";
            directoryList.Add(dirPath);
            try
            {
                foreach (var dirInfo in new DirectoryInfo(dirPath).GetDirectories())
                {
                    directoryList.AddRange(GetDirectories(bgw, dirInfo.FullName));
                }
            }
            catch (Exception)
            {
            }
            bgw?.ReportProgress(Convert.ToInt32(FileAttr.DirectoryCount), directoryList.Count);
            return directoryList;
        }

        //private long GetDirectorySize(BackgroundWorker bgw, string dirPath = "", DirectoryInfo directoryInfo = null)
        //{
        //    if (bgWorker.CancellationPending) return 0;
        //    if (directoryInfo == null)
        //    {
        //        if (Directory.Exists(dirPath)) directoryInfo = new DirectoryInfo(dirPath);
        //        else return 0;
        //    }
        //    long dirLength = 0;
        //    try
        //    {
        //        foreach (var fileInfo in directoryInfo.GetFiles())
        //        {
        //            dirLength += fileInfo.Length;
        //        }
        //        foreach (var dirInfo in directoryInfo.GetDirectories())
        //        {
        //            dirLength += GetDirectorySize(bgw, "", dirInfo);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    return dirLength;
        //}
        private long fileSize = 0;
        private long directoryCount = 0;
        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch ((FileAttr)e.ProgressPercentage)
            {
                case FileAttr.Name:
                    Name = e.UserState as string;
                    break;
                case FileAttr.Size:
                    Size = e.UserState as string;
                    break;
                case FileAttr.SegSize:
                    fileSize += (long)e.UserState;
                    Size = $"{fileSize.ToSizeString()} ({fileSize}字节)";
                    break;
                case FileAttr.DirectoryCount:
                    directoryCount += (int)e.UserState;
                    Size = $"已读取文件夹 ({directoryCount})";
                    break;
                case FileAttr.Quota:
                    Quota = e.UserState as string;
                    break;
                case FileAttr.CreateTime:
                    CreateTime = e.UserState as string;
                    break;
                case FileAttr.ModifyTime:
                    ModifyTime = e.UserState as string;
                    break;
            }
        }

        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
        enum FileAttr
        {
            Name = 0,
            Size = 1,
            Quota = 2,
            CreateTime = 3,
            ModifyTime = 4,
            SegSize = 5,
            DirectoryCount = 6,
        }
    }
}
