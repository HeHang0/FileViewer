using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace FileViewer.Style
{
    public static class MicaHelper
    {
        public static bool IsSupported
        {
            get
            {
                if (!Tools.OSVersionHelper.IsWindowsNT) { return false; }

                return Tools.OSVersionHelper.OSVersion >= new Version(10, 0, 21996);
            }
        }

        public static void Apply(IntPtr handle, bool dark, bool force = false)
        {
            if (!force && !IsSupported) return;
            int trueValue = 0x01;
            int falseValue = 0x00;

            if (dark)
            {
                _ = DwmSetWindowAttribute(handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
            }
            else
            {
                _ = DwmSetWindowAttribute(handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref falseValue, Marshal.SizeOf(typeof(int)));
            }

            _ = DwmSetWindowAttribute(handle, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue, Marshal.SizeOf(typeof(int)));
        }

        public static void Apply(Window window, bool dark, bool force = false)
        {
            IntPtr windowHandle = new WindowInteropHelper(window).Handle;
            Apply(windowHandle, dark, force);
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);

        [Flags]
        private enum DwmWindowAttribute : uint
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_MICA_EFFECT = 1029
        }
    }
}
