namespace FileViewer.Tools
{
    public class MutexBool
    {
        private bool value;
        private readonly object lockObject = new();

        public void SetVal(bool v)
        {
            lock (lockObject)
            {
                value = v;
            }
        }

        public bool GetValue()
        {
            bool v = false;
            lock (lockObject)
            {
                v = value;
            }
            return v;
        }
    }
}
