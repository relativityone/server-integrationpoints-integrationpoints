using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Set Integration Point Type field on old Integration Points (before introducing Type).")]
	[RunOnce(true)]
	[Guid("D767FC26-7369-4845-8B94-973F60D0CAAD")]
	public class SetIntegrationPointType : PostInstallEventHandler
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
				IList<Data.IntegrationPoint> integrationPoints = GetIntegrationPoints();
				var integrationPointTypes = IntegrationPointTypeService.GetAllIntegrationPointTypes();

				foreach (Data.IntegrationPoint integrationPoint in integrationPoints)
				{
					UpdateIntegrationPointType(integrationPoint, integrationPointTypes);
				}
			}
			catch (Exception e)
			{
				var response = new Response
				{
					Message = $"Updating the Type field on the Integration Point object failed. Exception message: {e.Message}.",
					Exception = e,
					Success = false
				};
				return response;
			}

			return new Response
			{
				Message = "Updated successfully.",
				Success = true
			};
		}

		internal void UpdateIntegrationPointType(Data.IntegrationPoint integrationPoint, IList<IntegrationPointType> integrationPointTypes)
		{
			if ((integrationPoint.Type != null) && (integrationPoint.Type != 0))
			{
				return;
			}

			var sourceProvider = CaseServiceContext.RsapiService.SourceProviderLibrary.Read(integrationPoint.SourceProvider.Value);

			if (sourceProvider.Identifier == Constants.RELATIVITY_PROVIDER_GUID)
			{
				integrationPoint.Type = integrationPointTypes.First(x => x.Identifier == Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString()).ArtifactId;
			}
			else
			{
				integrationPoint.Type = integrationPointTypes.First(x => x.Identifier == Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString()).ArtifactId;
			}

			CaseServiceContext.RsapiService.IntegrationPointLibrary.Update(integrationPoint);
		}

		internal IList<Data.IntegrationPoint> GetIntegrationPoints()
		{
			IntegrationPointQuery integrationPointQuery = new IntegrationPointQuery(CaseServiceContext.RsapiService);
			IList<Data.IntegrationPoint> integrationPoints = integrationPointQuery.GetAllIntegrationPoints();
			return integrationPoints;
		}
	}
}