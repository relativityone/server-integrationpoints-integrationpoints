using System.Threading.Tasks;

namespace Relativity.Sync.Configuration
{
    internal interface ISourceWorkspaceTagsCreationConfiguration : IConfiguration
    {
        int DestinationWorkspaceArtifactId { get; }

        int SourceWorkspaceArtifactId { get; }

        int JobHistoryArtifactId { get; }

        bool IsDestinationWorkspaceTagArtifactIdSet { get; }

        Task SetDestinationWorkspaceTagArtifactIdAsync(int artifactId);
    }
}