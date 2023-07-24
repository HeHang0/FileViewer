using FileViewer.Base;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileViewer.Plugins.Media
{
    public class PluginMedia : IPlugin
    {
        private MediaControl? instance;
        private readonly object lockObject = new();

        public UserControl GetUserControl(IManager manager)
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    instance ??= new MediaControl(manager);
                }
            }
            return instance;
        }

        public void ChangeFile(string filePath)
        {
            instance?.ChangeFile(filePath);
        }

        public void ChangeTheme(bool dark)
        {
            instance?.ChangeTheme(dark);
        }

        public void Cancel()
        {
        }

        public bool Available => true;

        public bool SupportedDirectory => false;

        public IEnumerable<string> SupportedExtensions => new string[]
        {
            ".mp3", ".wav", ".wma", ".aac", ".ogg", ".flac", ".m4a",
            ".mp4", ".avi", ".wmv", ".mkv", ".mov", ".flv"
        };

        public string Description => "Preview media file, support audio,video";

        public string PluginName => "Media";

        public ImageSource? Icon => Utils.GetFileThumbnail(GetMediaPlayerPath());

        public static string? GetMediaPlayerPath()
        {
            string? mediaPath = null;
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\wmplayer.exe";

            try
            {
                using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryKey))
                {
                    mediaPath = key?.GetValue(string.Empty) as string;
                }
            }
            catch (Exception)
            {
            }

            return mediaPath;
        }
    }
}
