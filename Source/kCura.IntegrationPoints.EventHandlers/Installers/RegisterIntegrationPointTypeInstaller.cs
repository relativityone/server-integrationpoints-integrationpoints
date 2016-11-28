using System;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Add Integration Points Types into RIP application.")]
	[RunOnce(false)]
	[Guid("28D0FB0A-3CE9-44A3-9774-3CCC9DD57870")]
	public class RegisterIntegrationPointTypeInstaller : PostInstallEventHandler
	{
		private ICaseServiceContext _caseContext;
		private IIntegrationPointTypeService _integrationPointTypeService;
		private IAPILog _logger;

		internal ICaseServiceContext CaseServiceContext
		{
			get { return _caseContext ?? (_caseContext = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID())); }
			set { _caseContext = value; }
		}

		internal IIntegrationPointTypeService IntegrationPointTypeService
		{
			get { return _integrationPointTypeService ?? (_integrationPointTypeService = new IntegrationPointTypeService(Helper, CaseServiceContext)); }
			set { _integrationPointTypeService = value; }
		}

		internal IAPILog Logger
		{
			get { return _logger ?? (_logger = Helper.GetLoggerFactory().GetLogger().ForContext<RegisterIntegrationPointTypeInstaller>()); }
			set { _logger = value; }
		}

		public override Response Execute()
		{
			try
			{
				CreateOrUpdateIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ImportName,
					Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid);
				CreateOrUpdateIntegrationPointType(Constants.IntegrationPoints.IntegrationPointTypes.ExportName,
					Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid);
			}
			catch (Exception e)
			{
				LogCreatingIntegrationPointTypesError(e);
				return new Response
				{
					Exception = e,
					Message = e.Message,
					Success = false
				};
			}
			return new Response
			{
				Message = "Created or updated successfully.",
				Success = true
			};
		}

		private void CreateOrUpdateIntegrationPointType(string name, Guid guid)
		{
			var integrationPointType = IntegrationPointTypeService.GetIntegrationPointType(guid);
			if (integrationPointType == null)
			{
				LogCreatingIntegrationPointType(name);
				integrationPointType = new IntegrationPointType
				{
					Name = name,
					Identifier = guid.ToString(),
					ApplicationIdentifier = Constants.IntegrationPoints.APPLICATION_GUID_STRING
				};
				CaseServiceContext.RsapiService.IntegrationPointTypeLibrary.Create(integrationPointType);
			}
			else
			{
				integrationPointType.Name = name;
				CaseServiceContext.RsapiService.IntegrationPointTypeLibrary.Update(integrationPointType);
			}
		}

		#region Logging

		private void LogCreatingIntegrationPointTypesError(Exception e)
		{
			Logger.LogError(e, "Failed to create or update Integration Point Types.");
		}

		private void LogCreatingIntegrationPointType(string name)
		{
			Logger.LogDebug("Attempting to create Integration Point Type {name}.", name);
		}

		#endregion
	}
}