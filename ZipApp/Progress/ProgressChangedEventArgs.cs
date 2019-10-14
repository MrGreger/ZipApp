namespace ZipApp.Progress
{
    public class ProgressChangedEventArgs
    {
        public long TotalProgress { get; }
        public long CurrentProgress { get; }

        public ProgressChangedEventArgs(long totalProgress, long currentProgress)
        {
            TotalProgress = totalProgress;
            CurrentProgress = currentProgress;
        }
    }
}