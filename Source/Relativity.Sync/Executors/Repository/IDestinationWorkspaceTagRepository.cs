using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SourceWorkspaceTagsCreation;

namespace Relativity.Sync.Executors.Repository
{
	internal interface IDestinationWorkspaceTagRepository
	{
		Task<DestinationWorkspaceTag> QueryAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId);
		Task<DestinationWorkspaceTag> CreateAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName);
		Task UpdateAsync(int sourceWorkspaceArtifactId, DestinationWorkspaceTag destinationWorkspaceTag);
	}
}