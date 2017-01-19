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
			ICaseServiceContext context,
			IContextContainerFactory contextContainerFactory, 
			ISerializer serializer, 
			IChoiceQuery choiceQuery,
			IJobManager jobService, 
			IManagerFactory managerFactory,
			IIntegrationPointProviderValidator ipValidator,
			IIntegrationPointPermissionValidator permissionValidator,
			IToggleProvider toggleProvider)
		{
			IContextContainer targetContextContainer = contextContainerFactory.CreateContextContainer(targetHelper);
			IJobHistoryService jobHistoryService = managerFactory.CreateJobHistoryService(context, targetContextContainer, serializer);

			return new IntegrationPointService(
				helper, 
				targetHelper, 
				context, 
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
	}
}