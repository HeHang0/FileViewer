namespace FileViewer.ViewModel
{
    public class Viewer
    {
        public Viewer(System.Windows.Threading.Dispatcher dispatcher)
        {
            ViewerEventer = new Eventer(dispatcher);
            FileInfo = new FileAccess();
        }

        public FileAccess FileInfo { get; } 

        public Eventer ViewerEventer { get; }
    }
}
