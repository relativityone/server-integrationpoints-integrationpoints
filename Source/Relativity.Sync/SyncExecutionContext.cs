using System;
using System.Threading;

namespace Relativity.Sync
{
	internal sealed class SyncExecutionContext
	{
		public CancellationToken CancellationToken { get; }

		public IProgress<SyncProgress> Progress { get; }

		public SyncExecutionContext(IProgress<SyncProgress> progress, CancellationToken cancellationToken)
		{
			CancellationToken = cancellationToken;
			Progress = progress;
		}
	}
}