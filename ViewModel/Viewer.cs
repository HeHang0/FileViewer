namespace FileViewer.ViewModel
{
    public class Viewer
    {
        public Viewer()
        {
            ViewerEventer = new Eventer();
            FileInfo = new FileAccess();
        }

        public FileAccess FileInfo { get; } 

        public Eventer ViewerEventer { get; }
    }
}
