namespace Relativity.Sync.SyncConfiguration.Options
{
    /// <summary>
    /// Represents retry options.
    /// </summary>
    public class RetryOptions
    {
        /// <summary>
        /// Gets job to retry Artifact ID.
        /// </summary>
        public int JobToRetry { get; }

        /// <summary>
        /// Creates new instance of <see cref="RetryOptions"/> class.
        /// </summary>
        /// <param name="jobToRetry">Job to retry Artifact ID.</param>
        public RetryOptions(int jobToRetry)
        {
            JobToRetry = jobToRetry;
        }
    }
}
