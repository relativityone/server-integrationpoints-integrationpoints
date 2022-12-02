using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal interface IDocumentTagger
    {
        Task<TaggingExecutionResult> TagObjectsAsync(
            IImportJob importJob,
            ISynchronizationConfiguration configuration,
            CompositeCancellationToken token);
    }
}
