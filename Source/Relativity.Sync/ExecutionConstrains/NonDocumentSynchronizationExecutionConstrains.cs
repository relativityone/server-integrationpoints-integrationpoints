using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.ExecutionConstrains
{
    internal sealed class NonDocumentSynchronizationExecutionConstrains : BaseSynchronizationExecutionConstrains<INonDocumentSynchronizationConfiguration>
    {
        public NonDocumentSynchronizationExecutionConstrains(IBatchRepository batchRepository,ISyncLog syncLog) : base(batchRepository, syncLog)
        {
        }
    }
}
