namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IArtifactTypeRepository
	{
		int GetArtifactTypeIdFromArtifactTypeName(string artifactTypeName);
	}
}