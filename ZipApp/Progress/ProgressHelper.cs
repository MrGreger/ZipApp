using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZipApp.Progress
{
    public class ProgressHelper
    {
        public long TotalProgress { get; private set; }
        public long CurrentProgress { get; private set; }

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        private object locker = new object();

        public void SetTotalProgress(long amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("Amount can not be negative.");
            }

            lock (locker)
            {
                TotalProgress = amount;

                OnProgressChanged();
            }
        }

        public void AddProgress(long amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("Amount can not be negative.");
            }

            lock (locker)
            {
                CurrentProgress += amount;

                OnProgressChanged();
            }
        }

        private void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(TotalProgress, CurrentProgress));
        }
    }
}
