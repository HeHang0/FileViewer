using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileViewer.Globle
{
    public class MutexBool
    {
        private bool value;
        private object lockObject = new object();

        public void setVal(bool v)
        {
            lock(lockObject)
            {
                value = v;
            }
        }

        public bool getValue()
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
