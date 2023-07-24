using System.Timers;
using Timer = System.Timers.Timer;

namespace FileViewer.Tools
{
    public class Timeout
    {
        private Timer? timer;
        private Action? callback;
        private bool isRunning = false;

        public void SetTimeout(Action callback, double timeout)
        {
            ClearTimeout();
            this.callback = callback;
            timer = new Timer(timeout);
            timer.Elapsed += OnTimerElapsed;
            isRunning = true;
            timer.Start();
        }

        public void SetTimeout(Action callback, TimeSpan timeout)
        {
            SetTimeout(callback, timeout.TotalMilliseconds);
        }

        public void ClearTimeout()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Elapsed -= OnTimerElapsed;
                timer.Dispose();
                timer = null;
                isRunning = false;
            }
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (isRunning)
            {
                ClearTimeout(); // Clear the timeout after executing the callback to make it stoppable
                callback?.Invoke();
            }
        }
    }
}
