using System.Reflection;
using System.Threading;
using System.Windows;

namespace FileViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        Mutex? mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string? appName = Assembly.GetExecutingAssembly().GetName().Name;
            mutex = new Mutex(true, appName, out bool createdNew);
            if (!createdNew)
            {
                MessageBox.Show("应用程序已经在运行中");
                Current.Shutdown();
                return;
            }
        }
    }
}
