using FileViewer.Base;
using FileViewer.Tools;
using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace FileViewer.Plugins.Compressed
{
    public class Compressed : BackgroundWorkBase, INotifyPropertyChanged
    {
#pragma warning disable CS0067
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
        readonly IManager manager;
        public Compressed(IManager manager)
        {
            this.manager = manager;
        }

        private readonly ObservableCollection<FileItem> _fileList = new ObservableCollection<FileItem>();
        public ObservableCollection<FileItem> FileList => _fileList;

        private string currentFilePath = string.Empty;
        public void ChangeFile(string filePath)
        {
            InitBackGroundWork();
            currentFilePath = filePath;
            bgWorker?.RunWorkerAsync(filePath);
            manager.SetResizeMode(true);
            ChangeTheme(manager.IsDarkMode());
        }

        public void ChangeTheme(bool dark)
        {
            manager.SetColor(dark ? Utils.BackgroundDark : Utils.BackgroundLight);
        }

        protected override void BgWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var filePath = (string)e.Argument!;
            List<FileItem> result = new();
            Dictionary<string, FileItem> fileItemMap = new();
            try
            {
                IEnumerable<IArchiveEntry>? entries = null;
                using var stream = System.IO.File.OpenRead(filePath);

                switch (Path.GetExtension(filePath).ToLower())
                {
                    case ".rar":
                        entries = SharpCompress.Archives.Rar.RarArchive.Open(stream).Entries;
                        break;
                    case ".tar":
                        entries = SharpCompress.Archives.Tar.TarArchive.Open(stream).Entries;
                        break;
                    case ".gz":
                        entries = SharpCompress.Archives.GZip.GZipArchive.Open(stream).Entries;
                        break;
                    case ".zip":
                        entries = SharpCompress.Archives.Zip.ZipArchive.Open(stream).Entries;
                        break;
                    case ".7z":
                        entries = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(stream).Entries;
                        break;
                }
                if (entries != null)
                {
                    entries = entries.OrderBy(m => m.Key).OrderByDescending(m => m.IsDirectory);
                    foreach (var entry in entries)
                    {
                        ParseEntry(result, fileItemMap, entry.IsDirectory, entry.Key.TrimEnd('/'), entry.Size, entry.LastModifiedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                    }
                }
            }
            catch (Exception)
            {
            }
            e.Result = result.Count > 0 ? result : null;
        }

        static void ConfirmFolderPath(List<FileItem> result, Dictionary<string, FileItem> fileItemMap, string folderPath, string lastWriteTime)
        {
            if (fileItemMap.ContainsKey(folderPath))
            {
                return;
            }
            if(string.IsNullOrWhiteSpace(folderPath) || folderPath == "/")
            {
                return;
            }
            string parentPath = Path.GetDirectoryName(folderPath)?.Replace("\\", "/") ?? string.Empty;
            if (!fileItemMap.ContainsKey(parentPath))
            {
                ConfirmFolderPath(result, fileItemMap, parentPath, lastWriteTime);
            }
            if (!string.IsNullOrWhiteSpace(parentPath) && !fileItemMap.ContainsKey(parentPath))
            {
                return;
            }
            FileItem folderItem = new(folderPath, true);
            fileItemMap[folderPath] = folderItem;
            if(string.IsNullOrWhiteSpace(parentPath) || parentPath == "/")
            {
                folderItem.IsExpanded = true;
                result.Add(folderItem);
            }
            else
            {
                fileItemMap[parentPath].Children.Add(folderItem);
            }
        }

        static void ParseEntry(List<FileItem> result, Dictionary<string, FileItem> fileItemMap, bool isFolder, string fullName, long length, string lastWriteTime)
        {
            var folderPath = Path.GetDirectoryName(fullName)?.Replace("\\", "/") ?? string.Empty;
            ConfirmFolderPath(result, fileItemMap, folderPath, lastWriteTime);
            FileItem fileItem = new(fullName, isFolder, length, lastWriteTime);
            if (isFolder)
            {
                if (fileItemMap.ContainsKey(folderPath))
                {
                    fileItemMap[folderPath].Children.Add(fileItem);
                }
                else
                {
                    fileItem.IsExpanded = true;
                    result.Add(fileItem);
                }
                fileItemMap[fullName] = fileItem;
            }
            else
            {
                if (fileItemMap.ContainsKey(folderPath))
                {
                    fileItemMap[folderPath].Children.Add(fileItem);
                }
                else
                {
                    result.Add(fileItem);
                }
            }
        }

        protected override void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                manager.LoadFileFailed(currentFilePath);
                return;
            }
            _fileList.Clear();
            foreach (var item in (List<FileItem>)e.Result)
            {
                _fileList.Add(item);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileList)));
            manager.SetLoading(false);
        }

        protected override void BgWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {

        }

        public class FileItem
        {
            private readonly string fullName;

            private readonly bool isFolder;

            private static readonly Dictionary<string, BitmapSource?> iconCache = new();

            public FileItem(string fullName, bool isFolder, long length, string lastWriteTime)
            {
                FileSize = isFolder ? string.Empty : length.ToSizeString();
                LastModified = lastWriteTime;
                this.isFolder = isFolder;
                this.fullName = fullName;
            }

            public FileItem(string fullName, bool isFolder)
            {
                FileSize = string.Empty;
                LastModified = string.Empty;
                this.isFolder = isFolder;
                this.fullName = fullName;
            }

            public string FileName
            {
                get
                {
                    return Path.GetFileName(fullName);
                }
            }

            public string FilePath
            {
                get
                {
                    return fullName;
                }
            }
            public string FileSize { get; set; }
            public string LastModified { get; set; }
            public bool IsExpanded { get; set; }

            private static readonly object lockObject = new();
            public BitmapSource? Icon
            {
                get
                {
                    BitmapSource? fileIcon;
                    lock (lockObject)
                    {
                        string extension = isFolder ? "folder" : Path.GetExtension(fullName);
                        if (iconCache.ContainsKey(extension))
                        {
                            fileIcon = iconCache[extension];
                        }
                        else
                        {
                            try
                            {
                                var _icon = isFolder ? Tools.Icon.GetFolderIcon() : Tools.Icon.GetDefaultFileIcon(extension);
                                if (_icon != null)
                                {
                                    fileIcon = Utils.GetBitmapSource(_icon);
                                }
                                else
                                {
                                    fileIcon = Utils.GetBitmapSource(Utils.ImagePreview);
                                }
                            }
                            catch (Exception)
                            {
                                fileIcon = Utils.GetBitmapSource(Utils.ImagePreview);
                            }
                            iconCache[isFolder ? "folder" : extension] = fileIcon;
                        }
                    }
                    return fileIcon;
                }
            }
            public ObservableCollection<FileItem> Children { get; set; } = new ObservableCollection<FileItem> { };
        }
    }
}
