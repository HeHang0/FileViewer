namespace FileViewer.FileControl.Office
{
    /// <summary>
    /// OfficeControl.xaml 的交互逻辑
    /// </summary>
    public partial class OfficeControl : FileControl
    {
        public OfficeControl() : base(new OfficeModel())
        {
            InitializeComponent();
        }
    }
}
