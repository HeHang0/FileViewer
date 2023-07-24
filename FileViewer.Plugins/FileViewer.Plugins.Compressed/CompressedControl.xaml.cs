using System.Windows.Controls;

namespace FileViewer.Plugins.Compressed
{
    /// <summary>
    /// Interaction logic for CompressedControl.xaml
    /// </summary>
    public partial class CompressedControl : UserControl
    {
        public CompressedControl()
        {
            InitializeComponent();
        }

        private void TreeView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }
    }
}