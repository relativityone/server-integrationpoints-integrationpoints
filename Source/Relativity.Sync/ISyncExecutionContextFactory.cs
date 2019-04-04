using System;
using System.Threading;
using Banzai;

namespace Relativity.Sync
{
	internal interface ISyncExecutionContextFactory
	{
		IExecutionContext<SyncExecutionContext> Create(IProgress<SyncJobState> progress, CancellationToken token);
	}
}