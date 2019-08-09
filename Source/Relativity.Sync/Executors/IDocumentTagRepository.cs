using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal interface IDocumentTagRepository
	{
		Task<IEnumerable<string>> TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(
			ISynchronizationConfiguration configuration, IEnumerable<string> documentIdentifiers, CancellationToken token);

		Task<IEnumerable<int>> TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(
			ISynchronizationConfiguration configuration, IEnumerable<int> artifactIds, CancellationToken token);

		Task<ExecutionResult> GetTaggingResultsAsync<TIdentifier>(IList<Task<IEnumerable<TIdentifier>>> taggingTasks,
			int jobHistoryArtifactId);

		Task GenerateDocumentTaggingJobHistoryErrorAsync(ExecutionResult taggingResult,
			ISynchronizationConfiguration configuration);
	}
}