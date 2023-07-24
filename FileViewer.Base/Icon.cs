using System;
using System.Runtime.InteropServices;

namespace FileViewer.Tools
{
    public class Icon
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

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, int nIcons);

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

        public static System.Drawing.Icon? GetFolderIcon()
        {
            SHGetFileInfo("", 0x80, out SHFILEINFO shinfo, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), SHGFI_ICON | SHGFI_SMALLICON);
#pragma warning disable CA1416 // 验证平台兼容性
            System.Drawing.Icon? icon = System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone() as System.Drawing.Icon;
#pragma warning restore CA1416 // 验证平台兼容性
            DestroyIcon(shinfo.hIcon);
            return icon;
        }

        public static System.Drawing.Icon? GetDefaultFileIcon(string extension, bool large = false)
        {
            if (string.IsNullOrWhiteSpace(extension)) extension = ".txt";
            uint uFlags = SHGFI_ICON | SHGFI_SHELL_ICON_SIZE | SHGFI_USE_FILE_ATTRS;
            uFlags |= large ? SHGFI_LARGEICON : SHGFI_SMALLICON;
            SHGetFileInfo(extension, 0x80, out SHFILEINFO shinfo, (uint)Marshal.SizeOf(typeof(SHFILEINFO)), uFlags);
#pragma warning disable CA1416 // 验证平台兼容性
            System.Drawing.Icon? icon = System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone() as System.Drawing.Icon;
#pragma warning restore CA1416 // 验证平台兼容性
            DestroyIcon(shinfo.hIcon);
            return icon;
        }

        public static System.Drawing.Icon? GetIconFromDll(string filePath, int iconIndex, bool isLarge = false)
        {
            IntPtr[] largeIcons = new IntPtr[1];
            IntPtr[] smallIcons = new IntPtr[1];

            int result = ExtractIconEx(filePath, iconIndex, largeIcons, smallIcons, 1);

            if (result > 0)
            {
                var handle = isLarge ? largeIcons[0] : smallIcons[0];
                if (handle == IntPtr.Zero)
                {
                    handle = !isLarge ? largeIcons[0] : smallIcons[0];
                }
                return System.Drawing.Icon.FromHandle(handle);
            }

            return null;
        }
    }
}
