using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace FileViewer.FileHelper
{
    public class IconHelper
    {
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_SHELL_ICON_SIZE = 0x4;
        private const uint SHGFI_EXE_TYPE = 0x2000;
        private const uint SHGFI_USE_FILE_ATTRS = 0x10;

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("User32.dll", EntryPoint = "DestroyIcon")]
        public static extern int DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        public static Icon GetFolderIcon()
        {
            SHGetFileInfo("", 0x80, out SHFILEINFO shinfo, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), SHGFI_ICON | SHGFI_SMALLICON);
            Icon icon = Icon.FromHandle(shinfo.hIcon).Clone() as Icon;
            DestroyIcon(shinfo.hIcon);
            return icon;
        }

        public static Icon GetDefaultFileIcon(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) extension = ".txt";
            uint uFlags = SHGFI_ICON | SHGFI_SHELL_ICON_SIZE | SHGFI_SMALLICON | SHGFI_USE_FILE_ATTRS;
            SHGetFileInfo(extension, 0x80, out SHFILEINFO shinfo, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), uFlags);
            Icon icon = Icon.FromHandle(shinfo.hIcon).Clone() as Icon;
            DestroyIcon(shinfo.hIcon);
            return icon;
        }
    }
}
