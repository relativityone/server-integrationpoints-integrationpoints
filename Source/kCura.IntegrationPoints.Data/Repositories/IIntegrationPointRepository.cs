namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IIntegrationPointRepository
	{
		IntegrationPoint Read(int integrationPointArtifactID);
		string GetFieldMapJson(int integrationPointArtifactID);
		string GetSecuredConfiguration(int integrationPointArtifactID);
		string GetName(int integrationPointArtifactID);
	}
}