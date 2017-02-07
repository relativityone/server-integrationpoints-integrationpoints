using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Factories
{
	public interface IServiceFactory
	{
		IIntegrationPointService CreateIntegrationPointService(IHelper helper,
			IHelper targetHelper,
			ICaseServiceContext context,
			IContextContainerFactory contextContainerFactory,
			ISerializer serializer, IChoiceQuery choiceQuery,
			IJobManager jobService,
			IManagerFactory managerFactory,
			IIntegrationPointProviderValidator ipValidator,
			IIntegrationPointPermissionValidator permissionValidator,
			IToggleProvider toggleProvider);

		IArtifactService CreateArtifactService(IHelper helper, IHelper targetHelper);

		IFieldCatalogService CreateFieldCatalogService(IHelper targetHelper);

		IJobHistoryService CreateJobHistoryService(IHelper helper, IHelper targetHelper, ICaseServiceContext caseServiceContext, 
			IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory, ISerializer serializer);
	}
}