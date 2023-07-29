namespace FileViewer
{
    /// <summary>
    /// PluginManagerPage.xaml 的交互逻辑
    /// </summary>
    public partial class PluginManagerPage : BadgePage
    {
        public PluginManagerPage(PluginManager model)
        {
            InitializeComponent();
            DataContext = model;
        }
    }
}
