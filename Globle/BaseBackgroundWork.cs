using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileViewer.Globle
{
    public abstract class BaseBackgroundWork
    {
        protected BackgroundWorker bgWorker;

        public BaseBackgroundWork()
        {
            InitBackGroundWork();
        }

        protected void InitBackGroundWork()
        {
            if(bgWorker != null && !bgWorker.IsBusy)
            {
                return;
            }
            if (bgWorker != null)
            {
                bgWorker.CancelAsync();
                bgWorker.DoWork -= BgWorker_DoWork;
                bgWorker.ProgressChanged -= BgWorker_ProgressChanged;
                bgWorker.RunWorkerCompleted -= BgWorker_RunWorkerCompleted;
                bgWorker.Dispose();
            }
            bgWorker = new BackgroundWorker();
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.DoWork += BgWorker_DoWork;
        }

        protected abstract void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e);

        protected abstract void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e);

        protected abstract void BgWorker_DoWork(object sender, DoWorkEventArgs e);
    }
}
