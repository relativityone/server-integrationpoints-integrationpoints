using System.Threading.Tasks;

namespace Relativity.Sync.Configuration
{
    internal interface IDestinationWorkspaceSavedSearchCreationConfiguration : IConfiguration
    {
        int DestinationWorkspaceArtifactId { get; }

        int SourceJobTagArtifactId { get; }

        bool CreateSavedSearchForTags { get; }

        bool IsSavedSearchArtifactIdSet { get; }

        string GetSourceJobTagName();

        Task SetSavedSearchInDestinationArtifactIdAsync(int artifactId);
    }
}
