using Relativity.API;
using Relativity.Sync.Logging;
using System.Threading;

namespace Relativity.Sync
{
	/// <summary>
	/// Contains multi-purpose cancellation tokens.
	/// </summary>
	public class CompositeCancellationToken
	{
		private readonly IAPILog _log;

		/// <summary>
		/// An empty composite cancellation token.
		/// </summary>
		public static CompositeCancellationToken None { get; } = new CompositeCancellationToken(CancellationToken.None, CancellationToken.None, new EmptyLogger());

		/// <summary>
		/// Regular cancellation token, that should be used to stop a job (e.g. requested by user)
		/// </summary>
		public CancellationToken StopCancellationToken { get; }

		/// <summary>
		/// Cancellation token that signals a job to suspend (e.g. drain-stop)
		/// </summary>
		public CancellationToken DrainStopCancellationToken { get;  }


		/// <summary>
		/// Cancellation token that signals either cancel or drain stop
		/// </summary>
		public CancellationToken AnyReasonCancellationToken { get; }

		/// <summary>
		/// Gets the value that indicates if regular stop has been triggered.
		/// </summary>
		public virtual bool IsStopRequested => StopCancellationToken.IsCancellationRequested;

		/// <summary>
		/// Gets the value that indicates if drain stop has been triggered.
		/// </summary>
		public virtual bool IsDrainStopRequested => DrainStopCancellationToken.IsCancellationRequested;

		/// <summary>
		/// Creates new instance of this class.
		/// </summary>
		public CompositeCancellationToken(CancellationToken stopCancellationToken, CancellationToken drainStopCancellationToken, IAPILog log)
		{
			_log = log;

			StopCancellationToken = stopCancellationToken;
			DrainStopCancellationToken = drainStopCancellationToken;

			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

			StopCancellationToken.Register(() =>
			{
				LogTokenWasRequested(nameof(StopCancellationToken));
				cancellationTokenSource.Cancel();
			});
			DrainStopCancellationToken.Register(() =>
			{
				LogTokenWasRequested(nameof(DrainStopCancellationToken));
				cancellationTokenSource.Cancel();
			});

			AnyReasonCancellationToken = cancellationTokenSource.Token;
		}

		private void LogTokenWasRequested(string token)
        {
			_log.LogInformation("{token} was requested.", token);
		}
	}
}