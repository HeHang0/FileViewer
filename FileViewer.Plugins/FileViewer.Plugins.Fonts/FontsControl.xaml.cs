using FileViewer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

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