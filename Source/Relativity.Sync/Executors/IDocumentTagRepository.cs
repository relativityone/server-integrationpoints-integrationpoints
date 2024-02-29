using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal interface IDocumentTagRepository
    {
        Task<TaggingExecutionResult> TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<string> documentIdentifiers, CancellationToken token);

        Task<TaggingExecutionResult> TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<int> artifactIds, CancellationToken token);
    }
}
