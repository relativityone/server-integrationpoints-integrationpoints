using System.Threading;

namespace Relativity.Sync
{
	/// <summary>
	/// 
	/// </summary>
	public class CompositeCancellationToken
	{
		/// <summary>
		/// An empty composite cancellation token.
		/// </summary>
		public static CompositeCancellationToken None { get; } = new CompositeCancellationToken(CancellationToken.None, CancellationToken.None);

		/// <summary>
		/// 
		/// </summary>
		public CancellationToken StopCancellationToken { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public CancellationToken DrainStopCancellationToken { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stopCancellationToken"></param>
		/// <param name="drainStopCancellationToken"></param>
		public CompositeCancellationToken(CancellationToken stopCancellationToken, CancellationToken drainStopCancellationToken)
		{
			StopCancellationToken = stopCancellationToken;
			DrainStopCancellationToken = drainStopCancellationToken;
		}
	}
}