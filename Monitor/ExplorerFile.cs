using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;

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
                (IntPtr activeWindowHandle, string className) = GetCurrentProcessInfo();
                if(activeWindowHandle == IntPtr.Zero)
                {
                    return (false, string.Empty);
                }
                IntPtr shellWindowHandle = GetShellWindow();
                GetWindowThreadProcessId(shellWindowHandle, out int shellProcessId);
                dynamic shell = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
                if(cabinetWClass == className)
                {
                    for (int i = 0; i < shell.Windows().Count; i++)
                    {
                        var window = shell.Windows().Item(i);
                        if (window != null && (IntPtr)window.HWND == activeWindowHandle && window.Document != null)
                        {
                            if (window.Document.SelectedItems().Count > 0)
                            {
                                foreach (var item in window.Document.SelectedItems())
                                {
                                    return (true, item.Path);
                                }
                            }
                        }
                    }
                }else
                {
                    string filePath = string.Empty;
                    RunAsSTA(() =>
                    {
                        var originalData = Clipboard.GetDataObject();
                        System.Windows.Forms.SendKeys.SendWait("^c");
                        Thread.Sleep(100);
                        var files = Clipboard.GetFileDropList();
                        Clipboard.SetDataObject(originalData, true);
                        if (files.Count > 0)
                        {
                            filePath = files[0];
                        }
                    });
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        return (true, filePath);
                    }
                }
            }
            catch (Exception)
            {
            }
            return (false, string.Empty);
        }

        private static void RunAsSTA(Action threadStart)
        {
            try
            {
                Thread t = new Thread(new ThreadStart(threadStart));
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
            }
            catch (Exception)
            {
            }
        }

        static readonly string cabinetWClass = "cabinetwclass";

        static readonly string[] ExplorerClassNames = new string[] { cabinetWClass, "workerw", "progman" };
        static (IntPtr, string) GetCurrentProcessInfo()
        {
            IntPtr myPtr = GetForegroundWindow();
            StringBuilder classNameSB = new StringBuilder(256);
            GetClassName(myPtr, classNameSB, classNameSB.Capacity);
            string className = classNameSB.ToString().ToLower();
            if(!ExplorerClassNames.Contains(className))
            {
                return (IntPtr.Zero, string.Empty);
            }

            return (myPtr, className);
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
