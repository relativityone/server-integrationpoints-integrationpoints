using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
	public class IntegrationPointTypeInstaller : IIntegrationPointTypeInstaller
	{
		private readonly ICaseServiceContext _caseContext;
		private readonly IIntegrationPointTypeService _integrationPointTypeService;
		private readonly IAPILog _logger;

		public IntegrationPointTypeInstaller(ICaseServiceContext caseContext, IIntegrationPointTypeService integrationPointTypeService, IAPILog logger)
		{
			_caseContext = caseContext;
			_integrationPointTypeService = integrationPointTypeService;
			_logger = logger;
		}

		public void Install(Dictionary<Guid, string> types)
		{
			foreach (var typeGuid in types.Keys)
			{
				CreateOrUpdateIntegrationPointType(types[typeGuid], typeGuid);
			}
		}

		private void CreateOrUpdateIntegrationPointType(string name, Guid guid)
		{
			var integrationPointType = _integrationPointTypeService.GetIntegrationPointType(guid);
			if (integrationPointType == null)
			{
				LogCreatingIntegrationPointType(name);
				integrationPointType = new IntegrationPointType
				{
					Name = name,
					Identifier = guid.ToString(),
					ApplicationIdentifier = Constants.IntegrationPoints.APPLICATION_GUID_STRING
				};
				_caseContext.RelativityObjectManagerService.RelativityObjectManager.Create(integrationPointType);
			}
			else
			{
				integrationPointType.Name = name;
				_caseContext.RelativityObjectManagerService.RelativityObjectManager.Update(integrationPointType);
			}
		}

		#region Logging

		private void LogCreatingIntegrationPointType(string name)
		{
			_logger.LogInformation("Attempting to create Integration Point Type {name}.", name);
		}

		#endregion
	}
}