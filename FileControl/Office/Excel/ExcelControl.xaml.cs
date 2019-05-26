namespace FileViewer.FileControl.Excel
{
    /// <summary>
    /// ExcelControl.xaml 的交互逻辑
    /// </summary>
    public partial class ExcelControl : FileControl
    {
        public ExcelControl() : base(new ExcelModel())
        {
            InitializeComponent();
        }
    }
}
