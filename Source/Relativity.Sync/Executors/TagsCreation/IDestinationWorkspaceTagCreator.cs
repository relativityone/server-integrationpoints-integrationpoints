using System.Threading.Tasks;
using Relativity.Sync.Executors.SourceWorkspaceTagsCreation;

namespace Relativity.Sync.Executors.TagsCreation
{
	internal interface IDestinationWorkspaceTagCreator
	{
		Task<DestinationWorkspaceTag> CreateAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName);
	}
}