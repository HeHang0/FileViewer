using FileViewer.Base;
using FileViewer.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media;

namespace FileViewer.Plugins.Universal
{
    public class Universal : BackgroundWorkBase, INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
        readonly IManager _manager;
        public Universal(IManager manager)
        {
            this._manager = manager;
        }

        public ImageSource? ThumbnailImage { get; private set; }

        private readonly ImageSource? _helloBack = Utils.GetBitmapSource(Utils.ImageHelloBack);
        public ImageSource? HelloBack { get; private set; }

        public string Name { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public string Quota { get; set; } = string.Empty;

        public string CreateTime { get; set; } = string.Empty;

        public string ModifyTime { get; set; } = string.Empty;

        public void ChangeFile(string filePath)
        {
            var dark = _manager.IsDarkMode();
            HelloBack = dark ? null : _helloBack;
            _manager.LoadFileSuccess(480, 800, false, false, dark ? Utils.BackgroundDark : Utils.BackgroundHello);
            ThumbnailImage = Utils.GetFileThumbnail(LinkFile(filePath)) ?? Utils.GetBitmapSource(Utils.ImagePreview);
            Size = "正在计算大小...";
            fileSize = 0;
            directoryCount = 0;
            InitBackGroundWork();
            if (bgWorker == null) return;
            bgWorker.WorkerReportsProgress = true;
            bgWorker.RunWorkerAsync(filePath);
        }

        public void ChangeTheme(bool dark)
        {
            _manager.SetColor(dark ? Utils.BackgroundDark : Utils.BackgroundHello);
            HelloBack = dark ? null : _helloBack;
        }

        private long fileSize = 0;
        private long directoryCount = 0;
        protected override void BgWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            switch ((FileAttr)e.ProgressPercentage)
            {
                case FileAttr.Name:
                    Name = e.UserState as string ?? string.Empty;
                    break;
                case FileAttr.Size:
                    Size = e.UserState as string ?? string.Empty;
                    break;
                case FileAttr.SegSize:
                    fileSize += (long)(e.UserState ?? 0);
                    Size = $"{fileSize.ToSizeString()} ({fileSize}字节)";
                    break;
                case FileAttr.DirectoryCount:
                    directoryCount += (int)(e.UserState ?? 0);
                    Size = $"已读取文件夹 ({directoryCount})";
                    break;
                case FileAttr.Quota:
                    Quota = e.UserState as string ?? string.Empty;
                    break;
                case FileAttr.CreateTime:
                    CreateTime = e.UserState as string ?? string.Empty;
                    break;
                case FileAttr.ModifyTime:
                    ModifyTime = e.UserState as string ?? string.Empty;
                    break;
            }
        }

        private static string LinkFile(string filePath)
        {
            if (System.IO.File.Exists(filePath) && Path.GetExtension(filePath).ToLower() == ".lnk")
            {
                try
                {
                    ShellLink.Shortcut shortcut = ShellLink.Shortcut.ReadFromFile(filePath);
                    return shortcut.LinkInfo.LocalBasePath;
                }
                catch (Exception)
                {
                }
            }
            return filePath;
        }

        protected override void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var bgw = sender as BackgroundWorker;
            FileInfo fileInfo = new FileInfo(e.Argument as string ?? string.Empty);
            bgw?.ReportProgress(Convert.ToInt32(FileAttr.Name), fileInfo.Name == string.Empty ? fileInfo.FullName : fileInfo.Name);
            bgw?.ReportProgress(Convert.ToInt32(FileAttr.CreateTime), fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));
            bgw?.ReportProgress(Convert.ToInt32(FileAttr.ModifyTime), fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
            if (Directory.Exists(e.Argument as string))
            {
                long dirLength = GetDirectorySize(bgw, (string)e.Argument);
                bgw?.ReportProgress(Convert.ToInt32(FileAttr.Size), $"{dirLength.ToSizeString()} ({dirLength}字节)");
            }
            else
            {
                bgw?.ReportProgress(Convert.ToInt32(FileAttr.Size), $"{fileInfo.Length.ToSizeString()} ({fileInfo.Length}字节)");
            }
        }

        private long GetDirectorySize(BackgroundWorker? bgw, string dirPath = "")
        {
            if (bgw?.CancellationPending ?? false) return 0;
            if (string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath))
            {
                return 0;
            }
            DateTime startTime = DateTime.Now;
            List<string> directoryList = GetDirectories(bgw, dirPath);
            startTime = DateTime.Now;
            List<ManualResetEvent> manualEvents = new List<ManualResetEvent>();
            ConcurrentBag<long> cbList = new ConcurrentBag<long>();
            for (int i = 0; i < directoryList.Count; i++)
            {
                if (bgw?.CancellationPending ?? false) return 0;
                WaitGroup wg = new WaitGroup(new ManualResetEvent(false), directoryList[i]);
                manualEvents.Add(wg.Mre);
                ThreadPool.QueueUserWorkItem(waitGroup =>
                {
                    if (waitGroup == null) return;
                    try
                    {
                        foreach (var fileInfo in new DirectoryInfo(((WaitGroup)waitGroup).DirPath).GetFiles())
                        {
                            if (bgw?.CancellationPending ?? false) return;
                            cbList.Add(fileInfo.Length);
                            bgw?.ReportProgress(Convert.ToInt32(FileAttr.SegSize), fileInfo.Length);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    ((WaitGroup)waitGroup).Mre.Set();
                }, wg);
                if (manualEvents.Count == 64)
                {
                    WaitHandle.WaitAll(manualEvents.ToArray());
                    manualEvents.Clear();
                }
            }
            if (manualEvents.Count > 0) WaitHandle.WaitAll(manualEvents.ToArray());
            Console.WriteLine($"已经获取文件夹，耗时{(DateTime.Now - startTime).TotalMilliseconds}毫秒");
            long dirLength = cbList.Sum();
            return dirLength;
        }

        private List<string> GetDirectories(BackgroundWorker? bgw, string dirPath)
        {
            List<string> directoryList = new List<string>();
            if (string.IsNullOrWhiteSpace(dirPath) || !Directory.Exists(dirPath)) return directoryList;
            if (!dirPath.EndsWith("\\")) dirPath += "\\";
            directoryList.Add(dirPath);
            try
            {
                foreach (var dirInfo in new DirectoryInfo(dirPath).GetDirectories())
                {
                    if (bgw?.CancellationPending ?? false) return directoryList;
                    directoryList.AddRange(GetDirectories(bgw, dirInfo.FullName));
                }
            }
            catch (Exception)
            {
            }
            bgw?.ReportProgress(Convert.ToInt32(FileAttr.DirectoryCount), directoryList.Count);
            return directoryList;
        }

        class WaitGroup
        {
            public WaitGroup(ManualResetEvent mre, string dir)
            {
                Mre = mre;
                DirPath = dir;
            }
            public ManualResetEvent Mre { get; set; }
            public string DirPath { get; set; }
        }

        protected override void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
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
