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
		///     Retries last run of the job
		/// </summary>
		/// <param name="token">Cancellation token</param>
		Task RetryAsync(CancellationToken token);
	}
}