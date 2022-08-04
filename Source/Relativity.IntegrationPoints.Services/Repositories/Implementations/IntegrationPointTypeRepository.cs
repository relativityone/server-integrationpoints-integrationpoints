using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Services.Repositories.Implementations
{
    public class IntegrationPointTypeRepository : IIntegrationPointTypeRepository
    {
        private readonly IRelativityObjectManagerService _relativityObjectManagerService;

        public IntegrationPointTypeRepository(IRelativityObjectManagerService relativityObjectManagerService)
        {
            _relativityObjectManagerService = relativityObjectManagerService;
        }

        public IList<IntegrationPointTypeModel> GetIntegrationPointTypes()
        {
            QueryRequest query = new AllIntegrationPointTypesQueryBuilder().Create();
            List<IntegrationPointType> types = _relativityObjectManagerService.RelativityObjectManager.Query<IntegrationPointType>(query);
            return types.Select(Mapper.Map<IntegrationPointTypeModel>).ToList();
        }
    }
}