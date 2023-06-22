using System.Drawing;

namespace FileViewer.FileHelper.IcnsParser
{
  public class IcnsImage
  {
    private readonly Bitmap bitmap;
    private readonly IcnsType type;

    public IcnsImage(Bitmap bitmap, IcnsType type)
    {
      this.bitmap = bitmap;
      this.type = type;
    }

    public Bitmap Bitmap
    {
      get { return bitmap; }
    }

    public IcnsType Type
    {
      get { return type; }
    }
  }
}
