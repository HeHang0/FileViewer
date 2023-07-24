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
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetGUIThreadInfo(int hTreadID, ref GUITHREADINFO lpgui);
        [DllImport("shell32.dll")]
        private static extern Int32 SHGetDesktopFolder(out IntPtr ppshf);

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

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public InputUnion inputUnion;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mouseInput;
            [FieldOffset(0)]
            public KEYBDINPUT keyboardInput;
            [FieldOffset(0)]
            public HARDWAREINPUT hardwareInput;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        // 定义常量
        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
        public const int INPUT_HARDWARE = 2;
        public const uint KEYEVENTF_KEYDOWN = 0x0000;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const ushort VK_CONTROL = 0x11;
        public const ushort VK_C = 0x43;

        static void SendCopy()
        {
            // 模拟按下 Ctrl 键
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].inputUnion.keyboardInput.wVk = VK_CONTROL;
            inputs[0].inputUnion.keyboardInput.dwFlags = KEYEVENTF_KEYDOWN;
            _ = SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            // 模拟按下 C 键
            inputs[0].inputUnion.keyboardInput.wVk = VK_C;
            inputs[0].inputUnion.keyboardInput.dwFlags = KEYEVENTF_KEYDOWN;
            _ = SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            // 模拟释放 C 键
            inputs[0].inputUnion.keyboardInput.dwFlags = KEYEVENTF_KEYUP;
            _ = SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            // 模拟释放 Ctrl 键
            inputs[0].inputUnion.keyboardInput.wVk = VK_CONTROL;
            inputs[0].inputUnion.keyboardInput.dwFlags = KEYEVENTF_KEYUP;
            _ = SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static (bool Ok, string FilePath) GetCurrentFilePath()
        {
            try
            {
                (IntPtr activeWindowHandle, string className) = GetCurrentProcessInfo();
                if (activeWindowHandle == IntPtr.Zero) return (false, string.Empty);

                IntPtr shellWindowHandle = GetShellWindow();
                _ = GetWindowThreadProcessId(shellWindowHandle, out int shellProcessId);
                Type? t = Type.GetTypeFromProgID("Shell.Application");
                if (t == null) return (false, string.Empty);

                dynamic? shell = Activator.CreateInstance(t);
                if (cabinetWClass == className && shell != null)
                {
                    for (int i = 0; i < shell!.Windows().Count; i++)
                    {
                        var window = shell.Windows().Item(i);
                        if (window != null && (IntPtr)window!.HWND == activeWindowHandle && window!.Document != null)
                        {
                            if (window!.Document.SelectedItems().Count > 0)
                            {
                                foreach (var item in window.Document.SelectedItems())
                                {
                                    return (true, item.Path);
                                }
                            }
                        }
                    }
                }
                else
                {
                    string filePath = string.Empty;
                    RunAsSTA(() =>
                    {
                        var originalData = Clipboard.GetDataObject();
                        SendCopy();
                        Thread.Sleep(100);
                        var files = Clipboard.GetFileDropList();
                        if (files != null && files.Count > 0)
                        {
                            filePath = files[0]!;
                        }
                        if (originalData != null) Clipboard.SetDataObject(originalData);
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

        static (bool Ok, GUITHREADINFO GUIInfo) GetGUICursorStatus(int processId)
        {
            GUITHREADINFO lpgui = new();
            lpgui.cbSize = Marshal.SizeOf(lpgui);
            bool ok = GetGUIThreadInfo(processId, ref lpgui);
            return (ok, lpgui);
        }
    }
}