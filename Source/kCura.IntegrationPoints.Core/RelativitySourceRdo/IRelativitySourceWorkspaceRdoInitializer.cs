namespace kCura.IntegrationPoints.Core.RelativitySourceRdo
{
	public interface IRelativitySourceWorkspaceRdoInitializer
	{
		int InitializeWorkspaceWithSourceWorkspaceRdo(int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId);

		int InitializeWorkspaceWithSourceWorkspaceRdo(int destinationWorkspaceArtifactId);
	}
}