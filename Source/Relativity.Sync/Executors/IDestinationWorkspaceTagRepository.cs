using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
    internal interface IDestinationWorkspaceTagRepository : IWorkspaceTagRepository<int>
    {
        Task<DestinationWorkspaceTag> ReadAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, CancellationToken token);

        Task<DestinationWorkspaceTag> CreateAsync(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName);

        Task UpdateAsync(int sourceWorkspaceArtifactId, DestinationWorkspaceTag destinationWorkspaceTag);
    }
}
