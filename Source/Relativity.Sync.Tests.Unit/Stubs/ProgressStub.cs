using System;

namespace Relativity.Sync.Tests.Unit.Stubs
{
	internal sealed class ProgressStub : IProgress<SyncProgress>
	{
		public SyncProgress SyncProgress { get; private set; }

		public void Report(SyncProgress value)
		{
			SyncProgress = value;
		}
	}
}