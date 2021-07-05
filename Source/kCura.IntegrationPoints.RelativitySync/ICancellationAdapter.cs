using Relativity.Sync;
using System;

namespace kCura.IntegrationPoints.RelativitySync
{
	public interface ICancellationAdapter
	{
		CompositeCancellationToken GetCancellationToken(Action drainStopTokenCallback = null);
	}
}
