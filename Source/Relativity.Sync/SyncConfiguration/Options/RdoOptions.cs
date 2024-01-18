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
        /// Configures JobHistory status RDO
        /// </summary>
        public JobHistoryStatusOptions JobHistoryStatus { get; }

        /// <summary>
        /// Configures JobHistoryError RDO
        /// </summary>
        public JobHistoryErrorOptions JobHistoryError { get; }

        /// <summary>
        /// Configures DestinationWorkspace RDO which is used for tagging
        /// </summary>
        public DestinationWorkspaceOptions DestinationWorkspace { get; }

        /// <summary>
        /// Constructor. All parameters are mandatory
        /// </summary>
        public RdoOptions(JobHistoryOptions jobHistory, JobHistoryStatusOptions jobHistoryStatus, JobHistoryErrorOptions jobHistoryError, DestinationWorkspaceOptions destinationWorkspace)
        {
            JobHistory = jobHistory;
            JobHistoryStatus = jobHistoryStatus;
            JobHistoryError = jobHistoryError;
            DestinationWorkspace = destinationWorkspace;
        }
    }
}
