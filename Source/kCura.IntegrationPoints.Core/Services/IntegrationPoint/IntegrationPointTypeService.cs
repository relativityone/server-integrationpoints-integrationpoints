using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.API;

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
			var query = new Query<RDO>
			{
				Fields = GetFields()
			};
			return _context.RsapiService.IntegrationPointTypeLibrary.Query(query);
		}

		public IntegrationPointType GetIntegrationPointType(Guid guid)
		{
			var query = new Query<RDO>
			{
				Condition = new TextCondition(Guid.Parse(IntegrationPointTypeFieldGuids.Identifier), TextConditionEnum.EqualTo, guid.ToString()),
				Fields = GetFields()
			};
			var integrationPointTypes = _context.RsapiService.IntegrationPointTypeLibrary.Query(query);
			if (integrationPointTypes.Count > 1)
			{
				LogMoreThanOneIntegrationPointType(guid);
			}
			return integrationPointTypes.SingleOrDefault();
		}

		private List<FieldValue> GetFields()
		{
			return BaseRdo.GetFieldMetadata(typeof(IntegrationPointType)).Values.ToList().Select(field => new FieldValue(field.FieldGuid)).ToList();
		}

		#region Logging

		private void LogMoreThanOneIntegrationPointType(Guid guid)
		{
			_apiLog.LogWarning("More than one IntegrationPointType found for GUID {GUID}.", guid);
		}

		#endregion
	}
}