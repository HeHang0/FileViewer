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
        public delegate void ColorChangeEventHandler(Color color);
        public static event ColorChangeEventHandler ColorChange;

        public static void OnColorChange(Color color)
        {
            ColorChange?.Invoke(color);
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
        public delegate void FullScreenEventHandler(bool isFullScreen);
        public static event FullScreenEventHandler FullScreen;

        public static void OnFullScreen(bool isFullScreen)
        {
            FullScreen?.Invoke(isFullScreen);
        }

        public delegate void WindowCloseEventHandler();
        public static event WindowCloseEventHandler WindowClose;

        public static void OnWindowClose()
        {
            WindowClose?.Invoke();
        }
    }
}
