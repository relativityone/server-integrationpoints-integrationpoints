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
		///     Aborts job without waiting for current step to complete
		/// </summary>
		/// <remarks>
		///     Preferred way to manage job cancellation is to pass CancellationToken to ExecuteAsync method
		/// </remarks>
		void Abort();
	}
}