using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Services.Interfaces.Private.Extensions;

namespace kCura.IntegrationPoints.Services.Repositories.Implementations
{
	public class IntegrationPointTypeRepository : IIntegrationPointTypeRepository
	{
		private readonly IRSAPIService _rsapiService;

		public IntegrationPointTypeRepository(IRSAPIService rsapiService)
		{
			_rsapiService = rsapiService;
		}

		public IList<IntegrationPointTypeModel> GetIntegrationPointTypes()
		{
			var query = new AllIntegrationPointTypesQueryBuilder().Create();
			var types = _rsapiService.IntegrationPointTypeLibrary.Query(query);
			return types.Select(x => x.ToModel()).ToList();
		}
	}
}