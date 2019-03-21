using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
	public interface IIntegrationPointRepository
	{
		Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID);
		Task<IEnumerable<FieldMap>> GetFieldMappingAsync(int integrationPointArtifactID);
		string GetSecuredConfiguration(int integrationPointArtifactID);
		string GetName(int integrationPointArtifactID);
	}
}