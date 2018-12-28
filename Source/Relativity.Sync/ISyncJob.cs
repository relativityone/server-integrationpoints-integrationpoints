using System;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync
{
	/// <summary>
	///     Represents Sync job
	/// </summary>
	public interface ISyncJob
	{
		/// <summary>
		///     Executes job
		/// </summary>
		/// <param name="token">Cancellation token</param>
		Task ExecuteAsync(CancellationToken token);

		/// <summary>
		///     Executes job
		/// </summary>
		/// <param name="progress">The progress object</param>
		/// <param name="token">Cancellation token</param>
		Task ExecuteAsync(IProgress<SyncProgress> progress, CancellationToken token);

		/// <summary>
		///     Retries last run of the job
		/// </summary>
		/// <param name="token">Cancellation token</param>
		Task RetryAsync(CancellationToken token);

		/// <summary>
		///     Retries last run of the job
		/// </summary>
		/// <param name="progress">The progress object</param>
		/// <param name="token">Cancellation token</param>
		Task RetryAsync(IProgress<SyncProgress> progress, CancellationToken token);
	}
}