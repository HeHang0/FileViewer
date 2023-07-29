using System.Windows.Controls;

namespace FileViewer.Plugins.Fonts
{
    /// <summary>
    /// Interaction logic for FontsControl.xaml
    /// </summary>
    public partial class FontsControl : UserControl
    {
        public FontsControl(Fonts model)
        {
            InitializeComponent();
            DataContext = model;
        }
    }
}