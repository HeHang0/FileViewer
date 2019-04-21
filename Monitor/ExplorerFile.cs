using Shell32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileViewer.Monitor
{
    public class ExplorerFile
    {
        [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        static extern IntPtr GetForegroundWindow();
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        public static (bool Ok, string FilePath) GetCurrentFilePath()
        {
            var cpr = GetCurrentProcessInfo();
            if (cpr.ProcessName != "explorer") return (false, string.Empty);
            var windows = new SHDocVw.ShellWindowsClass();
            foreach (SHDocVw.InternetExplorer window in windows)
            {
                var filename = Path.GetFileNameWithoutExtension(window.FullName).ToLowerInvariant();
                if (filename == "explorer" && window.HWND == cpr.Hwnd.ToInt32())
                {
                    FolderItems items = ((IShellFolderViewDual2)window.Document).SelectedItems();
                    if(items.Count == 1)
                    {
                        return (true, items.Item(0).Path);
                    }
                }
            }
            return (false, string.Empty);
        }

        private static (IntPtr Hwnd, string ProcessName) GetCurrentProcessInfo()
        {
            IntPtr myPtr = GetForegroundWindow();

            GetWindowThreadProcessId(myPtr, out int calcID);

            Process myProcess = Process.GetProcessById(calcID);

            return (myPtr, myProcess.ProcessName);
        }
    }
}
