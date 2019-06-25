using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IIntegrationPointRepository
	{
		Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID);
		Task<IntegrationPoint> ReadWithFieldMappingAsync(int integrationPointArtifactID);
		Task<IEnumerable<FieldMap>> GetFieldMappingAsync(int integrationPointArtifactID);
		string GetSecuredConfiguration(int integrationPointArtifactID);
		string GetName(int integrationPointArtifactID);
		int CreateOrUpdate(IntegrationPoint integrationPoint);
		void Update(IntegrationPoint integrationPoint);
		void Delete(int integrationPointID);

		IList<IntegrationPoint> GetAll(List<int> integrationPointIDs);
		IList<IntegrationPoint> GetAllBySourceAndDestinationProviderIDs(
			int sourceProviderArtifactID,
			int destinationProviderArtifactID);
		IList<IntegrationPoint> GetIntegrationPoints(List<int> sourceProviderIds);
		IList<IntegrationPoint> GetIntegrationPointsWithAllFields(List<int> sourceProviderIds);
		IList<IntegrationPoint> GetAllIntegrationPoints();
		IList<IntegrationPoint> GetIntegrationPointsWithAllFields();
	}
}