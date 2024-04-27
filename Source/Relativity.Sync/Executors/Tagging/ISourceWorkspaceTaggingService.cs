using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Kepler.SyncBatch;

namespace Relativity.Sync.Executors.Tagging
{
    internal interface ISourceWorkspaceTaggingService
    {
        Task<TaggingExecutionResult> TagDocumentsInSourceWorkspaceAsync(ISynchronizationConfiguration configuration, SyncBatchDto batch);
    }
}