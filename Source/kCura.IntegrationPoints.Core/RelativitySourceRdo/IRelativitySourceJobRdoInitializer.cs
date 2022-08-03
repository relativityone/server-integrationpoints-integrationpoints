namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
    public interface IRelativitySourceJobRdoInitializer
    {
        int InitializeWorkspaceWithSourceJobRdo(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactTypeId);
    }
}