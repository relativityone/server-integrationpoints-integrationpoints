using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
	public interface ICancellationAdapter
	{
		CompositeCancellationToken GetCancellationToken();
	}
}
