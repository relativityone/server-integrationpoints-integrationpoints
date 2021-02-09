using System.Threading;

namespace kCura.IntegrationPoints.RelativitySync
{
	public class CompositeCancellationToken
	{
		public CancellationToken StopCancellationToken { get; set; }
		public CancellationToken DrainStopCancellationToken { get; set; }

		public CompositeCancellationToken(CancellationToken stopCancellationToken, CancellationToken drainStopCancellationToken)
		{
			StopCancellationToken = stopCancellationToken;
			DrainStopCancellationToken = drainStopCancellationToken;
		}
	}
}