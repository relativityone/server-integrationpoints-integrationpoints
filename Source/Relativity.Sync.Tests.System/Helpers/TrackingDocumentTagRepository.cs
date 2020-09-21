using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Relativity.Sync.Storage;
using Relativity.Sync.Executors;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.System.Helpers
{
	internal class TrackingDocumentTagRepository : IDocumentTagRepository
	{
		private readonly DocumentTagRepository _documentTagRepository;

		public static IList<int> TaggedDocumentsInDestinationWorkspaceWithSourceInfoCounts { get; } = new List<int>();

		public static IList<int> TaggedDocumentsInSourceWorkspaceWithDestinationInfoCounts { get; } = new List<int>();

		public TrackingDocumentTagRepository(IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository,
			ISourceWorkspaceTagRepository sourceWorkspaceTagRepository,
			IJobHistoryErrorRepository jobHistoryErrorRepository)
		{
			_documentTagRepository = new DocumentTagRepository(destinationWorkspaceTagRepository, sourceWorkspaceTagRepository, jobHistoryErrorRepository);
		}

		public Task<TaggingExecutionResult> TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<string> documentIdentifiers, CancellationToken token)
		{
			TaggedDocumentsInDestinationWorkspaceWithSourceInfoCounts.Add(documentIdentifiers.Count());

			return _documentTagRepository.TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(configuration, documentIdentifiers, token);
		}

		public Task<TaggingExecutionResult> TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<int> artifactIds, CancellationToken token)
		{
			TaggedDocumentsInSourceWorkspaceWithDestinationInfoCounts.Add(artifactIds.Count());

			return _documentTagRepository.TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(configuration, artifactIds, token);
		}
	}
}
