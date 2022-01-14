using Relativity.Sync.Storage;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class NonDocumentSynchronizationExecutionConstrains : BaseSynchronizationExecutionConstrains
    {
        public NonDocumentSynchronizationExecutionConstrains(IBatchRepository batchRepository,ISyncLog syncLog) : base(batchRepository, syncLog)
        {
        }
    }
}
