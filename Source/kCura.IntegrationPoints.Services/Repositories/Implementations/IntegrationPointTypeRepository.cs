using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using Relativity.Services.Objects.DataContracts;

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
			QueryRequest query = new AllIntegrationPointTypesQueryBuilder().Create();
			List<IntegrationPointType> types = _rsapiService.RelativityObjectManager.Query<IntegrationPointType>(query);
			return types.Select(Mapper.Map<IntegrationPointTypeModel>).ToList();
		}
	}
}