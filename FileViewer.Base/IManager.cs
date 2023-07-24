using System.Windows.Media;

namespace FileViewer.Base
{
    public interface IManager
    {
        public void SetColor(Color color);
        public void SetFile(string filePath);
        public void SetIcon(ImageSource icon);
        public void SetSize(double height, double width);
        public void SetLoading(bool loading);
        public void SetFullScreen(bool fullScreen);
        public void SetResizeMode(bool resize);
        public void LoadFileFailed(string filePath);
        public void LoadFileSuccess(double? height, double? width, bool? resize, bool? loading, Color? color);
        public void CloseWindow();
        public void ChangedWindowVisable();
        public bool IsLoading();
        public bool IsDarkMode();
    }
}