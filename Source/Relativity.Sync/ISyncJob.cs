using System;
using System.Threading.Tasks;

namespace Relativity.Sync
{
    /// <summary>
    ///     Represents Sync job
    /// </summary>
    public interface ISyncJob
    {
        /// <summary>
        /// Executes job
        /// </summary>
        /// <param name="token">Cancellation token</param>
        Task ExecuteAsync(CompositeCancellationToken token);

        /// <summary>
        /// Executes job
        /// </summary>
        /// <param name="progress">The progress object</param>
        /// <param name="token">Cancellation token</param>
        Task ExecuteAsync(IProgress<SyncJobState> progress, CompositeCancellationToken token);
    }
}