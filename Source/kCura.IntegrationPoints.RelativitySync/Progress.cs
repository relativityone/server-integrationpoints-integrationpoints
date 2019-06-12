using System;
using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal sealed class Progress : IProgress<SyncJobState>
	{
		public event EventHandler<SyncJobState> SyncProgress;

		public void Report(SyncJobState value)
		{
			SyncProgress?.Invoke(this, value);
		}
	}
}