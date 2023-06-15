using Shell32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FileViewer.Monitor
{
    public class ExplorerFile
    {
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214E3-0000-0000-C000-000000000046")]
        interface IShellView
        {
        }
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214E6-0000-0000-C000-000000000046")]
        interface IShellFolder
        {
            void ParseDisplayName(IntPtr hwndOwner, IntPtr pbcReserved, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, out uint pchEaten, out IntPtr ppidl, ref uint pdwAttributes);
            [PreserveSig] int EnumObjects(IntPtr hwndOwner, int grfFlags, out IntPtr ppenumIDList);
            void BindToObject(IntPtr pidl, IntPtr pbcReserved, [In] ref Guid riid, out IShellView ppvOut);
            void BindToStorage(IntPtr pidl, IntPtr pbcReserved, [In] ref Guid riid, out IntPtr ppvObj);
            void CompareIDs(IntPtr lParam, IntPtr pidl1, IntPtr pidl2);
            void CreateViewObject(IntPtr hwndOwner, [In] ref Guid riid, out IShellView ppvOut);
            void GetAttributesOf(uint cidl, IntPtr apidl, ref uint rgfInOut);
            void GetUIObjectOf(IntPtr hwndOwner, uint cidl, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr[] apidl, [In] ref Guid riid, IntPtr rgfReserved, out IntPtr ppvOut);
            void GetDisplayNameOf(IntPtr pidl, uint uFlags, out IntPtr pName);
            void SetNameOf(IntPtr hwndOwner, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, uint uFlags, out IntPtr ppidlOut);
        }
        [DllImport("shell32.dll")]
        static extern IntPtr SHGetDesktopFolder(out IShellFolder ppshf);
        [DllImport("user32.dll")]
        static extern IntPtr GetShellWindow();
        [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetGUIThreadInfo(int hTreadID, ref GUITHREADINFO lpgui);
        [DllImport("shell32.dll")]
        private static extern Int32 SHGetDesktopFolder(out IntPtr ppshf);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        [DllImport("ole32.dll", PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.IUnknown)]
        static extern object OleGetClipboard();
        [DllImport("ole32.dll")]
        public static extern int OleSetClipboard(IDataObject pDataObj);

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int iLeft;
            public int iTop;
            public int iRight;
            public int iBottom;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct GUITHREADINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rectCaret;
        }
        static string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static readonly string EXPLORER = "explorer";
        public static (bool Ok, string FilePath) GetCurrentFilePath()
        {
            try
            {
                var cpr = GetCurrentProcessInfo();
                if (cpr.ProcessName != EXPLORER) return (false, string.Empty);
                var status = GetGUICursorStatus(cpr.ProcessId);
                if (status.Ok)
                {
                    if ((int)status.GUIInfo.hwndCaret != 0)
                    {
                        return (false, string.Empty);
                    }
                }
                var windows = new SHDocVw.ShellWindowsClass();
                FolderItems fi = null;
                foreach (SHDocVw.InternetExplorer window in windows)
                {
                    if (window.HWND != cpr.Hwnd.ToInt32()) continue;
                    var filename = Path.GetFileNameWithoutExtension(window.FullName).ToLowerInvariant();
                    if (filename == EXPLORER)
                    {
                        fi = ((IShellFolderViewDual2)window.Document).SelectedItems();
                        if (fi != null && fi.Count == 1)
                        {
                            return (true, fi.Item(0).Path);
                        }
                    }
                }

                SendKeys.SendWait("^c");
                DataObject data = new DataObject(OleGetClipboard());
                var files = data.GetFileDropList();
                if (files.Count == 1)
                {
                    return (true, files[0].ToString());
                }
            }
            catch (Exception)
            {
            }
            return (false, string.Empty);
        }

        static readonly string[] ExplorerClassNames = new string[] { "cabinetwclass", "workerw", "progman" };
        static (IntPtr Hwnd, string ProcessName, int ProcessId) GetCurrentProcessInfo()
        {
            IntPtr myPtr = GetForegroundWindow();
            StringBuilder classNameSB = new StringBuilder(256);
            GetClassName(myPtr, classNameSB, classNameSB.Capacity);
            string className = classNameSB.ToString().ToLower();
            if(!ExplorerClassNames.Contains(className))
            {
                return (myPtr, string.Empty, 0);
            }

            int a = GetWindowThreadProcessId(myPtr, out int calcID);

            Process myProcess = Process.GetProcessById(calcID);

            return (myPtr, myProcess.ProcessName, a);
        }

        static (bool Ok, GUITHREADINFO GUIInfo) GetGUICursorStatus(int processId)
        {
            GUITHREADINFO lpgui = new GUITHREADINFO();
            lpgui.cbSize = Marshal.SizeOf(lpgui);
            bool ok = GetGUIThreadInfo(processId, ref lpgui);
            return (ok, lpgui);
        }
    }
}
