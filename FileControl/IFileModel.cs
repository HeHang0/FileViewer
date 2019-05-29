using System.ComponentModel;
using System.Windows.Media;

namespace FileViewer.FileControl
{
    interface IFileModel: IFileChanged, INotifyPropertyChanged, IColorChanged
    {
    }


    public interface IFileChanged
    {
        void OnFileChanged((string FilePath, FileHelper.FileExtension Ext) file);
    }

    public interface IColorChanged
    {
        void OnColorChanged(Color color);
    }
}
