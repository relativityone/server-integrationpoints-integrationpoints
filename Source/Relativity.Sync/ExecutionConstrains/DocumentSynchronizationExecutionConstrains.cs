using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.ExecutionConstrains
{
	internal sealed class DocumentSynchronizationExecutionConstrains : BaseSynchronizationExecutionConstrains<IDocumentSynchronizationConfiguration>
	{
		public DocumentSynchronizationExecutionConstrains(IBatchRepository batchRepository, ISyncLog syncLog) : base(batchRepository, syncLog)
		{
		}
	}
}