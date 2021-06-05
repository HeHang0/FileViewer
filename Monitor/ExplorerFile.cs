using Microsoft.WindowsAPICodePack.Shell;
using Shell32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileViewer.Monitor
{
    public class ExplorerFile
    {
        [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        static extern IntPtr GetForegroundWindow();
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
        public static (bool Ok, string FilePath) GetCurrentFilePath()
        {
            var cpr = GetCurrentProcessInfo();
            if (cpr.ProcessName != "explorer") return (false, string.Empty);
            var status = GetGUICursorStatus(cpr.ProcessId);
            if (status.Ok)
            {
                if ((int)status.GUIInfo.hwndCaret != 0)
                {
                    return (false, string.Empty);
                }
            }
            try
             {
                var windows = new SHDocVw.ShellWindowsClass();
                FolderItems fi = null;
                foreach (SHDocVw.InternetExplorer window in windows)
                {
                    var filename = Path.GetFileNameWithoutExtension(window.FullName).ToLowerInvariant();
                    if (filename == "explorer" && window.HWND == cpr.Hwnd.ToInt32())
                    {
                        fi = ((IShellFolderViewDual2)window.Document).SelectedItems();
                        if(fi != null && fi.Count == 1)
                        {
                            return (true, fi.Item(0).Path);
                        }
                    }
                }
                SendKeys.SendWait("^c");
                DataObject data = new DataObject(OleGetClipboard());
                var files = data.GetFileDropList();
                if(files.Count == 1)
                {
                    return (true, files[0].ToString());
                }
            }
            catch (Exception)
            {
            }
            return (false, string.Empty);
        }

        static (IntPtr Hwnd, string ProcessName, int ProcessId) GetCurrentProcessInfo()
        {
            IntPtr myPtr = GetForegroundWindow();

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
