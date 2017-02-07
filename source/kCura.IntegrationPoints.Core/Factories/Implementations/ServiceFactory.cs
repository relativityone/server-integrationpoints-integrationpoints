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

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
	public class ServiceFactory : IServiceFactory
	{
		public IIntegrationPointService CreateIntegrationPointService(
			IHelper helper, 
			IHelper targetHelper, 
			ICaseServiceContext caseServiceContext,
			IContextContainerFactory contextContainerFactory, 
			ISerializer serializer, 
			IChoiceQuery choiceQuery,
			IJobManager jobService, 
			IManagerFactory managerFactory,
			IIntegrationPointProviderValidator ipValidator,
			IIntegrationPointPermissionValidator permissionValidator,
			IToggleProvider toggleProvider)
		{
			IJobHistoryService jobHistoryService = CreateJobHistoryService(helper, targetHelper, caseServiceContext, contextContainerFactory, managerFactory, serializer);

			return new IntegrationPointService(
				helper,
				caseServiceContext, 
				contextContainerFactory, 
				serializer, 
				choiceQuery, 
				jobService, 
				jobHistoryService, 
				managerFactory,
				ipValidator,
				permissionValidator,
				toggleProvider);
		}

		public IArtifactService CreateArtifactService(IHelper helper, IHelper targetHelper)
		{
			var rsapiClient = targetHelper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser);
			return new ArtifactService(rsapiClient, helper);
		}

		public IFieldCatalogService CreateFieldCatalogService(IHelper targetHelper)
		{
			return new FieldCatalogService(targetHelper);
		}

		public IJobHistoryService CreateJobHistoryService(IHelper helper, IHelper targetHelper, ICaseServiceContext caseServiceContext, 
			IContextContainerFactory contextContainerFactory, IManagerFactory managerFactory, ISerializer serializer)
		{
			IContextContainer sourceContextContainer = contextContainerFactory.CreateContextContainer(helper);
			IContextContainer targetContextContainer = contextContainerFactory.CreateContextContainer(helper, targetHelper.GetServicesManager());

			IJobHistoryService jobHistoryService = new JobHistoryService(caseServiceContext, managerFactory.CreateFederatedInstanceManager(sourceContextContainer),
				managerFactory.CreateWorkspaceManager(targetContextContainer), helper, serializer);

			return jobHistoryService;
		}
	}
}