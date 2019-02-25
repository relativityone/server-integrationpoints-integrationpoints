using System;
using Relativity.Sync;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal sealed class Progress : IProgress<SyncProgress>
	{
		public event EventHandler<SyncProgress> SyncProgress;

		public void Report(SyncProgress value)
		{
			SyncProgress?.Invoke(this, value);
		}
	}
}