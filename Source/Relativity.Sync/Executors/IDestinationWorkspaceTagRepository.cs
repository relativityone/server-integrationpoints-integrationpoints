using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal interface IDestinationWorkspaceTagRepository
	{
		Task<DestinationWorkspaceTag> ReadAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, CancellationToken token);

		Task<DestinationWorkspaceTag> CreateAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName);

		Task UpdateAsync(int sourceWorkspaceArtifactId, DestinationWorkspaceTag destinationWorkspaceTag);

		Task TagDocumentsAsync(ISynchronizationConfiguration synchronizationConfiguration, IList<int> documentArtifactIds, CancellationToken token);
	}
}