using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;

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
			return Enumerable.ToList(types.Select(Mapper.Map<IntegrationPointTypeModel>));
		}
	}
}