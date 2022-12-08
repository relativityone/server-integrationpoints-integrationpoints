using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
    public class IntegrationPointTypeService : IIntegrationPointTypeService
    {
        private readonly ICaseServiceContext _context;
        private readonly IAPILog _apiLog;

        public IntegrationPointTypeService(IHelper helper, ICaseServiceContext context)
        {
            _context = context;
            _apiLog = helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointTypeService>();
        }

        public IList<IntegrationPointType> GetAllIntegrationPointTypes()
        {
            QueryRequest query = GetQueryToRetrieveAllIntegrationPointTypes();
            return _context.RelativityObjectManagerService.RelativityObjectManager.Query<IntegrationPointType>(query);
        }

        public IntegrationPointType GetIntegrationPointType(Guid guid)
        {
            QueryRequest query = GetQueryToRetrieveAllIntegrationPointTypes();
            query.Condition = $"'{IntegrationPointTypeFields.Identifier}' == '{guid}'";
            List<IntegrationPointType> integrationPointTypes = _context.RelativityObjectManagerService.RelativityObjectManager.Query<IntegrationPointType>(query);

            if (integrationPointTypes.Count > 1)
            {
                LogMoreThanOneIntegrationPointType(guid);
            }
            return integrationPointTypes.SingleOrDefault();
        }

        private QueryRequest GetQueryToRetrieveAllIntegrationPointTypes()
        {
            return new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = Guid.Parse(ObjectTypeGuids.IntegrationPointType)
                },
                Fields = GetFields()
            };
        }

        private List<FieldRef> GetFields()
        {
            return RDOConverter.GetFieldList<IntegrationPointType>().ToList();
        }

        #region Logging

        private void LogMoreThanOneIntegrationPointType(Guid guid)
        {
            _apiLog.LogWarning("More than one IntegrationPointType found for GUID {GUID}.", guid);
        }

        #endregion
    }
}