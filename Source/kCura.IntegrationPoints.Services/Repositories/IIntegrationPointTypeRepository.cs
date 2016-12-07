using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services.Repositories
{
	public interface IIntegrationPointTypeRepository
	{
		IList<IntegrationPointTypeModel> GetIntegrationPointTypes();
	}
}