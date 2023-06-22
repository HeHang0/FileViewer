﻿using FileViewer.FileHelper;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;

namespace FileViewer.FileControl.Text
{
    /// <summary>
    /// TextControl.xaml 的交互逻辑
    /// </summary>
    public partial class TextControl : FileControl
    {
        public TextControl(): base(new TextModel())
        {
            InitializeComponent();
        }
    }
}
