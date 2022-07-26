using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.System.Helpers
{
    internal class TrackingDocumentTagRepository : IDocumentTagRepository
    {
        private readonly DocumentTagRepository _documentTagRepository;

        public static IDictionary<int, IList<int>> TaggedDocumentsInDestinationWorkspaceWithSourceInfoCounts { get; } = new Dictionary<int, IList<int>>();

        public static IDictionary<int, IList<int>> TaggedDocumentsInSourceWorkspaceWithDestinationInfoCounts { get; } = new Dictionary<int, IList<int>>();

        public TrackingDocumentTagRepository(IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository,
            ISourceWorkspaceTagRepository sourceWorkspaceTagRepository,
            IJobHistoryErrorRepository jobHistoryErrorRepository)
        {
            _documentTagRepository = new DocumentTagRepository(destinationWorkspaceTagRepository, sourceWorkspaceTagRepository, jobHistoryErrorRepository);
        }

        public Task<TaggingExecutionResult> TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<string> documentIdentifiers, CancellationToken token)
        {
            lock (TaggedDocumentsInDestinationWorkspaceWithSourceInfoCounts)
            {
                if (!TaggedDocumentsInDestinationWorkspaceWithSourceInfoCounts.ContainsKey(configuration.DestinationWorkspaceArtifactId))
                {
                    TaggedDocumentsInDestinationWorkspaceWithSourceInfoCounts.Add(configuration.DestinationWorkspaceArtifactId, new List<int>());
                }
            }

            TaggedDocumentsInDestinationWorkspaceWithSourceInfoCounts[configuration.DestinationWorkspaceArtifactId].Add(documentIdentifiers.Count());

            return _documentTagRepository.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(configuration, documentIdentifiers, token);
        }

        public Task<TaggingExecutionResult> TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<int> artifactIds, CancellationToken token)
        {
            lock (TaggedDocumentsInSourceWorkspaceWithDestinationInfoCounts)
            {
                if (!TaggedDocumentsInSourceWorkspaceWithDestinationInfoCounts.ContainsKey(configuration.SourceWorkspaceArtifactId))
                {
                    TaggedDocumentsInSourceWorkspaceWithDestinationInfoCounts.Add(configuration.SourceWorkspaceArtifactId, new List<int>());
                }
            }

            TaggedDocumentsInSourceWorkspaceWithDestinationInfoCounts[configuration.SourceWorkspaceArtifactId].Add(artifactIds.Count());

            return _documentTagRepository.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(configuration, artifactIds, token);
        }
    }
}
