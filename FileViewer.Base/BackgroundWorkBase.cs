using System.ComponentModel;

namespace FileViewer.Base
{
    public abstract class BackgroundWorkBase
    {
        protected BackgroundWorker? bgWorker;

        public BackgroundWorkBase()
        {
            InitBackGroundWork();
        }

        protected void InitBackGroundWork()
        {
            if (bgWorker != null && !bgWorker.IsBusy)
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
            bgWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;
            bgWorker.ProgressChanged += BgWorker_ProgressChanged;
            bgWorker.DoWork += BgWorker_DoWork;
        }

        public virtual void Cancel()
        {
            try
            {
                bgWorker?.CancelAsync();
            }
            catch (System.Exception)
            {
            }
        }

        protected abstract void BgWorker_DoWork(object? sender, DoWorkEventArgs e);

        protected abstract void BgWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e);

        protected abstract void BgWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e);
    }
}
