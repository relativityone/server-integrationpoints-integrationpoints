namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Aggregate class for configuring RDO GUIDs
    /// </summary>
    public class RdoOptions
    {
        /// <summary>
        /// Configures JobHistory RDO
        /// </summary>
        public JobHistoryOptions JobHistory { get; }
        
        /// <summary>
        /// Configures JobHistoryError RDO
        /// </summary>
        public JobHistoryErrorOptions JobHistoryError { get; }
        
        /// <summary>
        /// Constructor. All parameters are mandatory
        /// </summary>
        public RdoOptions(JobHistoryOptions jobHistory, JobHistoryErrorOptions jobHistoryError)
        {
            JobHistory = jobHistory;
            JobHistoryError = jobHistoryError;
        }
    }
}