using FileViewer.Globle;
using FileViewer.Monitor;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace FileViewer.ViewModel
{
    public class Eventer : BaseBackgroundWork
    {
        private readonly MutexBool isLoop = new MutexBool();
        System.Windows.Threading.Dispatcher dispatcher;
        public Eventer(System.Windows.Threading.Dispatcher dp)
        {
            dispatcher = dp;
            GlobalNotify.WindowVisableChanged += WindowVisableChanged; 
        }

        private void WindowVisableChanged(bool show)
        {
            bool loop = isLoop.getValue();
            if (loop != show)
            {
                if (loop) isLoop.setVal(false);
                else
                {
                    Thread thread = new Thread(new ThreadStart(LoopShowFile));
                    thread.IsBackground = true;
                    isLoop.setVal(true);
                    thread.Start();
                }
            }
        }

        private void LoopShowFile()
        {
            while (isLoop.getValue())
            {
                var (ok, filePath) = ExplorerFile.GetCurrentFilePath();
                if (ok)
                {
                    dispatcher.Invoke(() =>
                    {
                        if(isLoop.getValue()) OnReceiveFile(filePath);
                    });
                }
                Thread.Sleep(400);
            }
        }

        public ICommand OnLoaded => new DelegateCommand<MainWindow>((win) => {
            InitHotKey(win);
        });
        public ICommand OpenFile => new DelegateCommand(() => {
            
        });

        public ICommand IsVisibleChanged => new DelegateCommand<MainWindow>((win) => {
            GlobalNotify.OnWindowVisableChanged(win.IsVisible);
        });

        public ICommand StateChanged => new DelegateCommand<MainWindow>((win) => {
            GlobalNotify.OnWindowVisableChanged(win.WindowState != System.Windows.WindowState.Minimized);
        });

        public delegate void ReceiveFileEventHandler(string msg);
        public event ReceiveFileEventHandler ReceiveFile;
        void OnReceiveFile(string filePath)
        {
            ReceiveFile?.Invoke(filePath);
        }

        KeyboardHook _keyboardHook;
        private void InitHotKey(System.Windows.Window win)
        {
            _keyboardHook = new KeyboardHook();
            _keyboardHook.InstallHook(OnKeyPress);
        }

        private void OnKeyPress(KeyboardHook.HookStruct hookStruct, out bool handle)
        {
            handle = false; //预设不拦截任何键
            if (GlobalNotify.IsLoading()) return;
            Keys key = (Keys)hookStruct.vkCode;
            if (key == Keys.Space)
            {
                InitBackGroundWork();
                bgWorker.RunWorkerAsync();
            }
        }

        protected override void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Result != null)
            {
                string filePath = (string)e.Result;
                OnReceiveFile(filePath);
            }
        }

        protected override void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var (ok, filePath) = ExplorerFile.GetCurrentFilePath();
            if (ok && !(sender as BackgroundWorker).CancellationPending)
            {
                e.Result = filePath;
            }
            else
            {
                e.Cancel = true;
            }
        }

        protected override void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }
    }
}
