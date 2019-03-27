using System;
using System.Threading;

namespace Relativity.Sync
{
	internal sealed class SyncExecutionContext
	{
		public CancellationToken CancellationToken { get; }

		public IProgress<SyncJobState> Progress { get; }

		public SyncExecutionContext(IProgress<SyncJobState> progress, CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
			Progress = progress;
		}
	}
}