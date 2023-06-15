using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
