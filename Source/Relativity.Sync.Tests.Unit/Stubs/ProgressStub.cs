using System;

namespace Relativity.Sync.Tests.Unit.Stubs
{
	internal sealed class ProgressStub : IProgress<SyncJobState>
	{
		public SyncJobState SyncJobState { get; private set; }

		public void Report(SyncJobState value)
		{
			SyncJobState = value;
		}
	}
}