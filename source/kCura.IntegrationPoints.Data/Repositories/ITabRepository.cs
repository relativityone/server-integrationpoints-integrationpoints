namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface ITabRepository
	{
		int? RetrieveTabArtifactId(int objectTypeArtifactId, string tabName);
		void Delete(int artifactId);
	}
}