namespace kCura.IntegrationPoint.Tests.Core.Models
{
	public class ProductionCreateResultDto
	{
		public ProductionCreateResultDto(int productionArtifactId, int productionDataSourceArtifactID)
		{
			ProductionArtifactID = productionArtifactId;
			ProductionDataSourceArtifactID = productionDataSourceArtifactID;
		}

		public int ProductionArtifactID { get; }
		public int ProductionDataSourceArtifactID { get; }
	}
}
