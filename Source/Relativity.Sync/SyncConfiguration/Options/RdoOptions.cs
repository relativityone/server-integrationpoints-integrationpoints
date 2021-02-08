namespace Relativity.Sync.SyncConfiguration.Options
{
    public class RdoOptions
    {
        public JobHistoryOptions JobHistory { get; }
        public JobHistoryErrorOptions JobHistoryError { get; }
        public JobHistoryErrorStatusOptions JobHistoryErrorStatus { get; }

        public RdoOptions(JobHistoryOptions jobHistory, JobHistoryErrorOptions jobHistoryError, JobHistoryErrorStatusOptions jobHistoryErrorStatus)
        {
            JobHistory = jobHistory;
            JobHistoryError = jobHistoryError;
            JobHistoryErrorStatus = jobHistoryErrorStatus;
        }
    }
}