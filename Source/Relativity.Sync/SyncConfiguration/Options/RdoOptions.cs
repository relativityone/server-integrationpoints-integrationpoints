namespace Relativity.Sync.SyncConfiguration.Options
{
    public class RdoOptions
    {
        public JobHistoryOptions JobHistory { get; }

        public RdoOptions(JobHistoryOptions jobHistory)
        {
            JobHistory = jobHistory;
        }
    }
}