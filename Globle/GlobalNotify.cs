using System.Windows.Media;

namespace FileViewer
{
    class GlobalNotify
    {
        public delegate void SizeChangeEventHandler(double height, double width);
        public static event SizeChangeEventHandler SizeChange;

        public static void OnSizeChange(double height, double width)
        {
            SizeChange?.Invoke(height, width);
        }
        public delegate void ColorChangeEventHandler(Color color, bool white);
        public static event ColorChangeEventHandler ColorChange;

        public static void OnColorChange(Color color)
        {
            var colorMean = (color.R + color.G + color.B) / 3;
            ColorChange?.Invoke(color, colorMean > 128);
        }


        public delegate void LoadingChangeEventHandler(bool loading);
        public static event LoadingChangeEventHandler LoadingChange;

        public static void OnLoadingChange(bool loading)
        {
            lock (obj)
            {
                _loading = loading;
            }
            LoadingChange?.Invoke(loading);
        }
        private static bool _loading;
        private static readonly object obj = new object();
        public static bool IsLoading()
        {
            var result = false;
            lock (obj)
            {
                result = _loading;
            }
            return result;
        }

        public delegate void FullScreenEventHandler(bool fullScreen);
        public static event FullScreenEventHandler FullScreen;

        public static void OnFullScreen(bool fullScreen)
        {
            FullScreen?.Invoke(fullScreen);
        }

        public delegate void ReSizeModeEventHandler(bool resize);
        public static event ReSizeModeEventHandler ResizeMode;

        public static void OnResizeMode(bool resize)
        {
            ResizeMode?.Invoke(resize);
        }

        public delegate void WindowCloseEventHandler();
        public static event WindowCloseEventHandler WindowClose;

        public static void OnWindowClose()
        {
            WindowClose?.Invoke();
        }

        public delegate void FileLoadFailedEventHandler(string filePath);
        public static event FileLoadFailedEventHandler FileLoadFailed;

        public static void OnFileLoadFailed(string filePath)
        {
            FileLoadFailed?.Invoke(filePath);
        }

        public delegate void WindowVisableChangedEventHandler(bool show, bool topmost);
        public static event WindowVisableChangedEventHandler WindowVisableChanged;

        public static void OnWindowVisableChanged(bool show, bool topmost)
        {
            WindowVisableChanged?.Invoke(show, topmost);
        }
    }
}
