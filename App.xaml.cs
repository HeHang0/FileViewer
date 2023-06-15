using System.Windows;
using System.Threading;
using System.Reflection;

namespace FileViewer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        Mutex mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            mutex = new Mutex(true, appName, out bool createdNew);
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
