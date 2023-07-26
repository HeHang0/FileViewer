using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FileViewer.Hook
{
    public class Keyboard
    {
        private const int WH_KEYBOARD_LL = 13; //键盘

        //键盘处理事件委托 ,当捕获键盘输入时调用定义该委托的方法.
        private delegate int HookHandle(int nCode, int wParam, IntPtr lParam);

        //客户端键盘处理事件
        public delegate void ProcessKeyHandle(HookStruct param, out bool handle);

        //接收SetWindowsHookEx返回值
        private static int _hHookValue = 0;

        //勾子程序处理事件
        private HookHandle? _KeyBoardHookProcedure;

        //Hook结构
        [StructLayout(LayoutKind.Sequential)]
        public class HookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        //设置钩子
        [DllImport("user32.dll")]
        private static extern int SetWindowsHookEx(int idHook, HookHandle lpfn, IntPtr hInstance, int threadId);

        //取消钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(int idHook);

        //调用下一个钩子
        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        //获取当前线程ID
        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        //Gets the main module for the associated process.
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string name);

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

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

        private IntPtr _hookWindowPtr = IntPtr.Zero;

        //构造器
        public Keyboard() { }

        //外部调用的键盘处理事件
        private static ProcessKeyHandle? _clientMethod;

        /// <summary>
        /// 安装勾子
        /// </summary>
        /// <param name="hookProcess">外部调用的键盘处理事件</param>
        public void InstallHook(ProcessKeyHandle clientMethod)
        {
            _clientMethod = clientMethod;

            // 安装键盘钩子
            if (_hHookValue == 0)
            {
                _KeyBoardHookProcedure = new HookHandle(OnHookProc);

                _hookWindowPtr = GetModuleHandle(Process.GetCurrentProcess()?.MainModule?.ModuleName ?? string.Empty);

                _hHookValue = SetWindowsHookEx(
                WH_KEYBOARD_LL,
                _KeyBoardHookProcedure,
                _hookWindowPtr,
                0);

                //如果设置钩子失败.
                if (_hHookValue == 0) UninstallHook();
            }
        }

        //取消钩子事件
        public void UninstallHook()
        {
            if (_hHookValue != 0)
            {
                bool ret = UnhookWindowsHookEx(_hHookValue);
                if (ret) _hHookValue = 0;
            }
        }

        //钩子事件内部调用,调用_clientMethod方法转发到客户端应用。
        private static int OnHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                ////转换结构
                HookStruct? hookStruct = (HookStruct?)Marshal.PtrToStructure(lParam, typeof(HookStruct));

                if (_clientMethod != null && hookStruct != null)
                {
                    //调用客户提供的事件处理程序。
                    _clientMethod(hookStruct, out bool handle);
                    if (handle) return 1; //1:表示拦截键盘,return 退出
                }
            }
            return CallNextHookEx(_hHookValue, nCode, wParam, lParam);
        }
    }
}
