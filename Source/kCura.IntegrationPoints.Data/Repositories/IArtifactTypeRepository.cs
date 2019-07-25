namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IArtifactTypeRepository
	{
		int GetArtifactTypeIDFromArtifactTypeName(string artifactTypeName);
	}
}