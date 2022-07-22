namespace Relativity.Sync
{
    /// <summary>
    ///     Represents Sync job execution configuration
    /// </summary>
    public sealed class SyncJobExecutionConfiguration
    {
        /// <summary>
        ///     Defines how many batches can be run in parallel
        /// </summary>
        public int NumberOfBatchRunInParallel { get; set; } = 4;

        /// <summary>
        ///     Defines how many job steps can be run in parallel
        /// </summary>
        public int NumberOfStepsRunInParallel { get; set; } = 3;
    }
}