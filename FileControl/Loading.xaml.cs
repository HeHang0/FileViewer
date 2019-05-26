using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace FileViewer.FileControl
{
    /// <summary>
    /// Loading.xaml 的交互逻辑
    /// </summary>
    public partial class Loading : UserControl
    {
        public Loading()
        {
            InitializeComponent();
        }


        #region 加载圆圈的margin
        [DescriptionAttribute("加载圆圈的margin"), CategoryAttribute("扩展"), DefaultValueAttribute(0)]
        public string LoadCirclesMargin
        {
            get { return (string)GetValue(LoadCirclesMarginProperty); }
            set { SetValue(LoadCirclesMarginProperty, value); }
        }


        public static readonly DependencyProperty LoadCirclesMarginProperty =
            DependencyProperty.Register("LoadCirclesMargin", typeof(string), typeof(Loading),
            new FrameworkPropertyMetadata("50"));
        #endregion

        #region 加载中的提示
        [DescriptionAttribute("加载中的提示"), CategoryAttribute("扩展"), DefaultValueAttribute(0)]
        public string LoadingText
        {
            get { return (string)GetValue(LoadingTextProperty); }
            set { SetValue(LoadingTextProperty, value); }
        }


        public static readonly DependencyProperty LoadingTextProperty =
            DependencyProperty.Register("LoadingText", typeof(string), typeof(Loading),
            new FrameworkPropertyMetadata("加载中"));
        #endregion

        #region 加载中的提示的字体大小
        [DescriptionAttribute("加载中的提示的字体大小"), CategoryAttribute("扩展"), DefaultValueAttribute(0)]
        public int LoadingTextFontSize
        {
            get { return (int)GetValue(LoadingTextFontSizeProperty); }
            set { SetValue(LoadingTextFontSizeProperty, value); }
        }


        public static readonly DependencyProperty LoadingTextFontSizeProperty =
            DependencyProperty.Register("LoadingTextFontSize", typeof(int), typeof(Loading),
            new FrameworkPropertyMetadata(12));
        #endregion

        #region 圆圈的颜色
        [DescriptionAttribute("圆圈的颜色"), CategoryAttribute("扩展"), DefaultValueAttribute(0)]
        public Brush CirclesBrush
        {
            get { return (Brush)GetValue(CirclesBrushProperty); }
            set { SetValue(CirclesBrushProperty, value); }
        }


        public static readonly DependencyProperty CirclesBrushProperty =
            DependencyProperty.Register("CirclesBrush", typeof(Brush), typeof(Loading),
            new FrameworkPropertyMetadata(Brushes.Black));
        #endregion

        #region 加载中的提示的字体颜色
        [DescriptionAttribute("加载中的提示的字体颜色"), CategoryAttribute("扩展"), DefaultValueAttribute(0)]
        public Brush LoadingTextForeground
        {
            get { return (Brush)GetValue(LoadingTextForegroundProperty); }
            set { SetValue(LoadingTextForegroundProperty, value); }
        }


        public static readonly DependencyProperty LoadingTextForegroundProperty =
            DependencyProperty.Register("LoadingTextForeground", typeof(Brush), typeof(Loading),
            new FrameworkPropertyMetadata(Brushes.DarkSlateGray));
        #endregion
    }
}
