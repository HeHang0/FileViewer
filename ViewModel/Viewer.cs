namespace FileViewer.ViewModel
{
    public class Viewer
    {
        public Viewer()
        {
            ViewerEventer = new Eventer();
            FileInfo = new FileAccess();
            ViewerEventer.ReceiveFile += FileInfo.InitFile;
        }

        public FileAccess FileInfo { get; } 

        public Eventer ViewerEventer { get; }
    }
}
