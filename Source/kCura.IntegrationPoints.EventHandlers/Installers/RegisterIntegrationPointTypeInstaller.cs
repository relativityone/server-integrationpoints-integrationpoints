using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Add Integration Points Types into RIP application.")]
	[RunOnce(false)]
	[Guid("28D0FB0A-3CE9-44A3-9774-3CCC9DD57870")]
	public class RegisterIntegrationPointTypeInstaller : PostInstallEventHandler
	{
		private IAPILog _logger;
		private IIntegrationPointTypeInstaller _integrationPointTypeInstaller;


		internal IAPILog Logger
		{
			get { return _logger ?? (_logger = Helper.GetLoggerFactory().GetLogger().ForContext<RegisterIntegrationPointTypeInstaller>()); }
			set { _logger = value; }
		}

		internal IIntegrationPointTypeInstaller IntegrationPointTypeInstaller
		{
			get
			{
				var caseServiceContext = ServiceContextFactory.CreateCaseServiceContext(Helper, Helper.GetActiveCaseID());
				var typeService = new IntegrationPointTypeService(Helper, caseServiceContext);
				return _integrationPointTypeInstaller ?? (_integrationPointTypeInstaller = new IntegrationPointTypeInstaller(caseServiceContext, typeService, Logger));
			}
			set { _integrationPointTypeInstaller = value; }
		}

		public override Response Execute()
		{
			try
			{
				var types = new Dictionary<Guid, string>
				{
					{Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid, Constants.IntegrationPoints.IntegrationPointTypes.ImportName},
					{Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid, Constants.IntegrationPoints.IntegrationPointTypes.ExportName}
				};
				IntegrationPointTypeInstaller.Install(types);
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

		#region Logging

		private void LogCreatingIntegrationPointTypesError(Exception e)
		{
			Logger.LogError(e, "Failed to create or update Integration Point Types.");
		}

		#endregion
	}
}