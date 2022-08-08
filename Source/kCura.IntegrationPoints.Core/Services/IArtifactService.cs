using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services
{
    public interface IArtifactService
    {
        RelativityObject GetArtifact(int workspaceArtifactId, string artifactTypeName, int artifactId);
    }
}