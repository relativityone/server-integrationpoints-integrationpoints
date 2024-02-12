using System;
using System.Collections.Generic;

namespace Relativity.Sync
{
    internal sealed class SyncExecutionContext
    {
        public CompositeCancellationToken CompositeCancellationToken { get; }

        public IProgress<SyncJobState> Progress { get; }

        public List<ExecutionResult> Results { get; } = new List<ExecutionResult>();

        public SyncExecutionContext(IProgress<SyncJobState> progress, CompositeCancellationToken compositeCancellationToken)
        {
            CompositeCancellationToken = compositeCancellationToken;
            Progress = progress;
        }
    }
}
