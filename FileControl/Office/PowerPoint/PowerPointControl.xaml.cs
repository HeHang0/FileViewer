namespace FileViewer.FileControl.PowerPoint
{
    /// <summary>
    /// PowerPointControl.xaml 的交互逻辑
    /// </summary>
    public partial class PowerPointControl : FileControl
    {
        public PowerPointControl() : base(new PowerPointModel())
        {
            InitializeComponent();
        }
    }
}
