using System;
using Banzai;

namespace Relativity.Sync
{
    internal interface ISyncExecutionContextFactory
    {
        IExecutionContext<SyncExecutionContext> Create(IProgress<SyncJobState> progress, CompositeCancellationToken token);
    }
}
