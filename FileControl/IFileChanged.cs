namespace FileViewer.FileControl
{
    public interface IFileChanged
    {
        void OnFileChanged((string FilePath, FileHelper.FileExtension Ext) file);
    }
}
