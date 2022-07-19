using System.ComponentModel;

namespace Relativity.Sync
{
    /// <summary>
    /// Describes the current progress of the sync job step.
    /// </summary>
    public enum SyncJobStatus
    {
        /// <summary>
        /// The sync job is in a newly created state with no work executed.
        /// </summary>
        [Description("New")]
        New = 0,

        /// <summary>
        /// The sync job is started and preparing to be executed.
        /// </summary>
        [Description("Started")]
        Started,

        /// <summary>
        /// The sync job is in progress and executing.
        /// </summary>
        [Description("In Progress")]
        InProgress,

        /// <summary>
        /// The sync job completed successfully without errors.
        /// </summary>
        [Description("Completed")]
        Completed,

        /// <summary>
        /// The sync job completed successfully with errors.
        /// </summary>
        [Description("Completed With Errors")]
        CompletedWithErrors,

        /// <summary>
        /// The sync job failed to execute.
        /// </summary>
        [Description("Failed")]
        Failed,

        /// <summary>
        /// The sync job was cancelled by an external entity.
        /// </summary>
        [Description("Cancelled")]
        Cancelled
    }
}