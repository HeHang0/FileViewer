using FileViewer.FileHelper;
using FileViewer.Globle;
using Prism.Commands;
using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileViewer.FileControl.Compressed
{
    public class CompressedModel : BaseBackgroundWork, IFileModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string currentFilePath;
        public void ChangeFile((string FilePath, FileExtension Ext) file)
        {
            InitBackGroundWork();
            currentFilePath = file.FilePath;
            bgWorker.RunWorkerAsync(file.FilePath);
        }

        public void ChangeTheme(bool dark)
        {
            if (dark)
            {
                GlobalNotify.OnColorChange(Color.FromRgb(0x33, 0x33, 0x33));
                Application.Current.Resources["TreeTextColor"] = new SolidColorBrush(Colors.White);
            }
            else
            {
                GlobalNotify.OnColorChange(Color.FromRgb(0xEE, 0xF5, 0xFD));
                Application.Current.Resources["TreeTextColor"] = new SolidColorBrush(Colors.Black);
            }
        }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filePath = e.Argument as string;
            List<FileItem> result = new List<FileItem>();
            Dictionary<string, FileItem> fileItemMap = new Dictionary<string, FileItem>();
            try
            {
                IEnumerable<IArchiveEntry> entries = null;
                using (var stream = File.OpenRead(filePath))
                {

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
                    if(entries != null)
                    {
                        entries = entries.OrderBy(m => m.Key).OrderByDescending(m => m.IsDirectory);
                        foreach (var entry in entries)
                        {
                            ParseEntry(result, fileItemMap, entry.IsDirectory, entry.Key.TrimEnd('/'), entry.Size, entry.LastModifiedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            e.Result = result.Count > 0 ? result : null;
        }

        void ParseEntry(List<FileItem> result, Dictionary<string, FileItem> fileItemMap, bool isFolder, string fullName, long length, string lastWriteTime)
        {
            var folderPath = Path.GetDirectoryName(fullName).Replace("\\", "/");
            FileItem fileItem = new FileItem(fullName, isFolder, length, lastWriteTime); ;
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

        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(e.Result == null)
            {
                GlobalNotify.OnFileLoadFailed(currentFilePath);
                return;
            }
            FileList.Clear();
            FileList.AddRange(e.Result as List<FileItem>);
            GlobalNotify.OnLoadingChange(false);
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        public ObservableCollection<FileItem> FileList { get; set; } = new ObservableCollection<FileItem>();

        public class FileItem
        {
            private readonly string fullName;

            private readonly bool isFolder;

            private static Dictionary<string, BitmapSource> iconCache = new Dictionary<string, BitmapSource>();

            public FileItem(string fullName, bool isFolder, long length, string lastWriteTime)
            {
                FileSize = isFolder ? string.Empty : length.ToSizeString();
                LastModified = lastWriteTime;
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
            public string FileSize { get; set; }
            public string LastModified { get; set; }
            public bool IsExpanded { get; set; }

            private static object lockObject = new object();
            public BitmapSource Icon
            {
                get
                {
                    BitmapSource fileIcon;
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
                                fileIcon = isFolder ? Utils.GetBitmapSource(IconHelper.GetFolderIcon()) : Utils.GetBitmapSource(IconHelper.GetDefaultFileIcon(extension));
                            }
                            catch (Exception ex)
                            {
                                fileIcon = Utils.GetBitmapSource(Properties.Resources.logo);
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
