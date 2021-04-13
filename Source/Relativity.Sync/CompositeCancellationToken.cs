using System.Threading;

namespace Relativity.Sync
{
	/// <summary>
	/// Contains multi-purpose cancellation tokens.
	/// </summary>
	public class CompositeCancellationToken
	{
		/// <summary>
		/// An empty composite cancellation token.
		/// </summary>
		public static CompositeCancellationToken None { get; } = new CompositeCancellationToken(CancellationToken.None, CancellationToken.None);

		/// <summary>
		/// Regular cancellation token, that should be used to stop a job (e.g. requested by user)
		/// </summary>
		public CancellationToken StopCancellationToken { get; set; }

		/// <summary>
		/// Cancellation token that signals a job to suspend (e.g. drain-stop)
		/// </summary>
		public CancellationToken DrainStopCancellationToken { get; set; }

		/// <summary>
		/// Gets the value that indicates if regular stop has been triggered.
		/// </summary>
		public bool IsStopRequested => StopCancellationToken.IsCancellationRequested;

		/// <summary>
		/// Gets the value that indicates if drain stop has been triggered.
		/// </summary>
		public bool IsDrainStopRequested => DrainStopCancellationToken.IsCancellationRequested;

		/// <summary>
		/// Creates new instance of this class.
		/// </summary>
		public CompositeCancellationToken(CancellationToken stopCancellationToken, CancellationToken drainStopCancellationToken)
		{
			StopCancellationToken = stopCancellationToken;
			DrainStopCancellationToken = drainStopCancellationToken;
		}
	}
}