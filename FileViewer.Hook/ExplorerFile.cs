using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace FileViewer.Hook
{
    public class ExplorerFile
    {
        [DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string? windowTitle);
        [DllImport("shlwapi.dll")]
        static extern int IUnknown_QueryService(IntPtr pUnk, ref Guid guidService, ref Guid riid,
                [MarshalAs(UnmanagedType.IUnknown)] out object ppv);

        private static readonly Guid ClsidShellWindows = new("9BA05972-F6A8-11CF-A442-00A0C90A8F39");
        private static readonly Guid SID_STopLevelBrowser = new(1284947520u, 37212, 4559, 153, 211, 0, 170, 0, 74, 232, 55);
        private static readonly Guid IID_IShellBrowser = new("000214E2-0000-0000-C000-000000000046");

        public static IntPtr FindChildWindow(IntPtr parentHandle, string className, string? windowTitle = null)
        {
            return FindWindowEx(parentHandle, IntPtr.Zero, className, windowTitle);
        }

        static string GetWindowText(IntPtr handle)
        {
            const int nChars = 256;
            StringBuilder Buff = new(nChars);
            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return string.Empty;
        }

        static void SendCopy()
        {
            Keyboard.INPUT[] inputs = new Keyboard.INPUT[1];
            inputs[0].type = Keyboard.INPUT_KEYBOARD;

            inputs[0].inputUnion.keyboardInput.wVk = Keyboard.VK_CONTROL;
            inputs[0].inputUnion.keyboardInput.dwFlags = Keyboard.KEYEVENTF_KEYDOWN;
            _ = Keyboard.SendInput(1, inputs, Marshal.SizeOf(typeof(Keyboard.INPUT)));

            inputs[0].inputUnion.keyboardInput.wVk = Keyboard.VK_C;
            inputs[0].inputUnion.keyboardInput.dwFlags = Keyboard.KEYEVENTF_KEYDOWN;
            _ = Keyboard.SendInput(1, inputs, Marshal.SizeOf(typeof(Keyboard.INPUT)));

            inputs[0].inputUnion.keyboardInput.dwFlags = Keyboard.KEYEVENTF_KEYUP;
            _ = Keyboard.SendInput(1, inputs, Marshal.SizeOf(typeof(Keyboard.INPUT)));

            inputs[0].inputUnion.keyboardInput.wVk = Keyboard.VK_CONTROL;
            inputs[0].inputUnion.keyboardInput.dwFlags = Keyboard.KEYEVENTF_KEYUP;
            _ = Keyboard.SendInput(1, inputs, Marshal.SizeOf(typeof(Keyboard.INPUT)));
        }

        public static (bool Ok, string FilePath) GetCurrentFilePath()
        {
            try
            {
                (IntPtr activeWindowHandle, string className) = GetCurrentProcessInfo();
                if (activeWindowHandle == IntPtr.Zero) return (false, string.Empty);

                var selectedFils = IsDesktopWindow(className) ?
                    GetSelectedFilesFromDesktop() :
                    GetSelectedFilesFromFileExplorer(activeWindowHandle);
                if (selectedFils.Length > 0) return (true, selectedFils[0]);
            }
            catch (Exception e)
            {
            }
            return (false, string.Empty);
        }

        private static string[] GetSelectedFilesFromFileExplorer(IntPtr foregroundWindowHandle)
        {
            //var activeTab = foregroundWindowHandle;
            //if (Tools.OSVersionHelper.IsWindows11OrGreater)
            //{
            //    activeTab = FindChildWindow(foregroundWindowHandle, "ShellTabWindowClass");
            //    if (activeTab == IntPtr.Zero)
            //    {
            //        activeTab = FindChildWindow(foregroundWindowHandle, "TabWindowClass");
            //    }
            //}
            dynamic shellWindows = Activator.CreateInstance(Type.GetTypeFromCLSID(ClsidShellWindows)!)!;
            foreach (dynamic webBrowserApp in shellWindows)
            {
                dynamic shellFolderView = webBrowserApp.Document;
                var folderTitle = shellFolderView.Folder.Title;
                if ((IntPtr)webBrowserApp.HWND != foregroundWindowHandle) continue;
                if (folderTitle != GetWindowText(foregroundWindowHandle)) continue;
                var selectedItems = shellFolderView.SelectedItems();
                string[] result = new string[selectedItems.Count];
                var i = 0;
                foreach (var item in selectedItems)
                {
                    result[i++] = item.Path;
                }
                return result;
                //IUnknown_QueryService(Marshal.GetIDispatchForObject(webBrowserApp), ref SID_STopLevelBrowser,
                //    ref IID_IShellBrowser, out dynamic shellBrowser);
                //Type type = shellBrowser.GetType();
                //MethodInfo[] methods = type.GetMethods();
                ////var shellBrowser = webBrowserApp.QueryService(SID_STopLevelBrowser, IID_IShellBrowser);
                //shellBrowser.GetWindow(out IntPtr shellBrowserHandle);

                //if (activeTab == shellBrowserHandle)
                //{
                //    return GetSelectedFilesFromShellBrowser(shellBrowser, true);
                //}
            }
            return Array.Empty<string>();
        }

        private static string[] GetSelectedFilesFromDesktop()
        {
            string[] filePaths = Array.Empty<string>();
            RunAsSTA(() =>
            {
                var originalData = Clipboard.GetDataObject();
                SendCopy();
                Thread.Sleep(100);
                var files = Clipboard.GetFileDropList();
                if (files != null && files.Count > 0)
                {
                    filePaths = new string[files.Count];
                    for (int i = 0; i < filePaths.Length; i++)
                    {
                        filePaths[i] = files[i]!;
                    }
                }
                if (originalData != null) Clipboard.SetDataObject(originalData);
            });
            return filePaths;
            //const int SWC_DESKTOP = 8;
            //const int SWFO_NEEDDISPATCH = 1;
            //object pvarloc = 0;
            //object pvarlocRoot = 0;
            //dynamic shellWindows = Activator.CreateInstance(Type.GetTypeFromCLSID(ClsidShellWindows)!)!;
            //dynamic serviceProvider = shellWindows.FindWindowSW(ref pvarloc, ref pvarlocRoot, SWC_DESKTOP, out int pHWND, SWFO_NEEDDISPATCH);
            //long num = shellWindows.Count;
            //IUnknown_QueryService(Marshal.GetIDispatchForObject(serviceProvider), ref SID_STopLevelBrowser,
            //    ref IID_IShellBrowser, out dynamic shellBrowser);
            //return GetSelectedFilesFromShellBrowser(shellBrowser, true);
        }

        private static string[] GetSelectedFilesFromShellBrowser(dynamic shellBrowser, bool onlySelectedFiles)
        {
            var shellView = shellBrowser.QueryActiveShellView();
            if (shellView != null)
            {
                const uint SVGIO_SELECTION = 2;
                const uint SVGIO_ALLVIEW = 0xFFFFFFFF;
                var selectionFlag = onlySelectedFiles ? SVGIO_SELECTION : SVGIO_ALLVIEW;
                shellView.ItemCount(selectionFlag, out int countItems);
                if (countItems > 0)
                {
                    Guid IID_IShellItemArray = new("b63ea76d-1f85-456f-a19c-48159efa858b");
                    shellView.Items(selectionFlag, IID_IShellItemArray, out dynamic items);
                    var result = new string[countItems];
                    for (int i = 0; i < countItems; i++)
                    {
                        result[i] = items.GetItemAt(i).ToIFileSystemItem().Path;
                    }
                    return result;
                }
            }
            return Array.Empty<string>();
        }

        private static void RunAsSTA(Action threadStart)
        {
            try
            {
                Thread t = new(new ThreadStart(threadStart));
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

        static bool IsDesktopWindow(string className)
        {
            return className != cabinetWClass;
        }

        static (IntPtr, string) GetCurrentProcessInfo()
        {
            IntPtr myPtr = GetForegroundWindow();
            StringBuilder classNameSB = new(256);
            _ = GetClassName(myPtr, classNameSB, classNameSB.Capacity);
            string className = classNameSB.ToString().ToLower();
            if (!ExplorerClassNames.Contains(className))
            {
                return (IntPtr.Zero, string.Empty);
            }
            return (myPtr, className);
        }
    }
}