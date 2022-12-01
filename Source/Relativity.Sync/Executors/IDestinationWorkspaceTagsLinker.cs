using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
    internal interface IDestinationWorkspaceTagsLinker
    {
        Task LinkDestinationWorkspaceTagToJobHistoryAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceTagArtifactId, int jobArtifactId);
    }
}
