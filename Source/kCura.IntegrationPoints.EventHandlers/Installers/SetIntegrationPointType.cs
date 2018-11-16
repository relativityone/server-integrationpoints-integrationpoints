using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.SourceProviderInstaller;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Description("Set Integration Point Type field on old Integration Points (before introducing Type).")]
	[RunOnce(true)]
	[Guid("D767FC26-7369-4845-8B94-973F60D0CAAD")]
	public class SetIntegrationPointType : PostInstallEventHandlerBase
	{
		private ICaseServiceContext _caseContext;
		private IIntegrationPointTypeService _integrationPointTypeService;

		protected override IAPILog CreateLogger()
		{
			return Helper.GetLoggerFactory().GetLogger().ForContext<SetIntegrationPointType>();
		}

		protected override string SuccessMessage => "Updating 'Type' fieldin the Integration Point object completed successfully during schema upgrade.";

		protected override string GetFailureMessage(Exception ex)
		{
			return "Updating 'Type' fieldin the Integration Point object failed during schema upgrade.";
		}

		protected override void Run()
		{
			IList<IntegrationPoint> integrationPoints = GetIntegrationPoints();
			IList<IntegrationPointType> integrationPointTypes = IntegrationPointTypeService.GetAllIntegrationPointTypes();

			foreach (IntegrationPoint integrationPoint in integrationPoints)
			{
				UpdateIntegrationPointType(integrationPoint, integrationPointTypes);
			}
		}

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

		internal void UpdateIntegrationPointType(IntegrationPoint integrationPoint, IList<IntegrationPointType> integrationPointTypes)
		{
			if ((integrationPoint.Type != null) && (integrationPoint.Type != 0))
			{
				return;
			}
			var sourceProvider = ObjectManager.Read<Data.SourceProvider>(integrationPoint.SourceProvider.Value);
			if (sourceProvider.Identifier == Constants.RELATIVITY_PROVIDER_GUID)
			{
				integrationPoint.Type = integrationPointTypes.First(x => x.Identifier == Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid.ToString()).ArtifactId;
			}
			else
			{
				integrationPoint.Type = integrationPointTypes.First(x => x.Identifier == Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid.ToString()).ArtifactId;
			}
			ObjectManager.Update(integrationPoint);
		}

		internal IList<IntegrationPoint> GetIntegrationPoints()
		{
			IntegrationPointQuery integrationPointQuery = new IntegrationPointQuery(ObjectManager);
			IList<Data.IntegrationPoint> integrationPoints = integrationPointQuery.GetAllIntegrationPoints();
			return integrationPoints;
		}
	}
}