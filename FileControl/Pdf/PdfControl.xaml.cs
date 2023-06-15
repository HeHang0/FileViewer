namespace FileViewer.FileControl.Pdf
{
    /// <summary>
    /// PdfControl.xaml 的交互逻辑
    /// </summary>
    public partial class PdfControl : FileControl
    {
        public PdfControl():base(new PdfModel())
        {
            InitializeComponent();
        }
    }
}
