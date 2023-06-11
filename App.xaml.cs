using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Threading;
using System.Reflection;

namespace FileViewer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            new Mutex(true, appName, out bool createdNew);
            if (!createdNew)
            {
                // 应用程序的实例已经运行
                MessageBox.Show("应用程序已经在运行中");
                Current.Shutdown();
                return;
            }
        }
    }
}
