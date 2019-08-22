using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal interface IDocumentTagRepository
	{
		Task<ExecutionResult> TagDocumentsInDestinationWorkspaceWithSourceInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<string> documentIdentifiers, CancellationToken token);

		Task<ExecutionResult> TagDocumentsInSourceWorkspaceWithDestinationInfoAsync(ISynchronizationConfiguration configuration, IEnumerable<int> artifactIds, CancellationToken token);
	}
}