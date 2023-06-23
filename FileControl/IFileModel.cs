using System.ComponentModel;
using System.Windows.Media;

namespace FileViewer.FileControl
{
    interface IFileModel: IFileChanged, INotifyPropertyChanged
    {
    }


    public interface IFileChanged
    {
        void ChangeFile((string FilePath, FileHelper.FileExtension Ext) file);
        void ChangeTheme(bool dark);
    }
}
