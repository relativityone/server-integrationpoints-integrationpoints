using System.Threading.Tasks;

namespace Relativity.Sync.Executors.SourceWorkspaceTagsCreation
{
	internal interface IDestinationWorkspaceTagsLinker
	{
		Task LinkDestinationWorkspaceTagToJobHistoryAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceTagArtifactId, int jobArtifactId);
	}
}