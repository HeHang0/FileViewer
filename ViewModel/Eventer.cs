using FileViewer.Monitor;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace FileViewer.ViewModel
{
    public class Eventer
    {
        public ICommand OnLoaded => new DelegateCommand<System.Windows.Window>((win) => {
            Loaded?.Invoke(null, null);
            InitHotKey(win);
        });

        public delegate void ReceiveFileEventHandler(object sender, string msg);
        public event ReceiveFileEventHandler ReceiveFile;
        void OnReceiveFile(string msg)
        {
            ReceiveFile?.Invoke(this, msg);
        }

        public event EventHandler Loaded;


        private void InitHotKey(System.Windows.Window win)
        {
            HwndSource hWndSource;
            WindowInteropHelper wih = new WindowInteropHelper(win);
            hWndSource = HwndSource.FromHwnd(wih.Handle);
            //添加处理程序 
            hWndSource.AddHook(MainWindowProc);
            HotKey.RegisterHotKey(wih.Handle, HotKey.GlobalAddAtom("Space"), HotKey.KeyModifiers.None, (int)System.Windows.Forms.Keys.Space);
        }

        private IntPtr MainWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case HotKey.WM_HOTKEY:
                    {
                        handled = true;
                        var (ok, filePath) = ExplorerFile.GetCurrentFilePath();
                        if (ok)
                        {
                            OnReceiveFile(filePath);
                        }
                        break;
                    }
            }
            return IntPtr.Zero;
        }
    }
}
