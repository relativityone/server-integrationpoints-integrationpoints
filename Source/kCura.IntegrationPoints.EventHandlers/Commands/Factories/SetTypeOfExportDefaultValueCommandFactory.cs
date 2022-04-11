using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using System;
using System.Linq;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Factories
{
	public static class SetTypeOfExportDefaultValueCommandFactory
	{
		public static SetTypeOfExportDefaultValueCommand Create(IEHHelper helper, int workspaceArtifactId)
		{
			IServiceContextHelper serviceContextHelper = new ServiceContextHelperForEventHandlers(helper, helper.GetActiveCaseID());
			ICaseServiceContext caseServiceContext = new CaseServiceContext(serviceContextHelper);
			
			IAPILog logger = helper.GetLoggerFactory().GetLogger();
			IIntegrationPointSerializer integrationPointSerializer = new IntegrationPointSerializer(logger);

			IServicesMgr servicesManager = helper.GetServicesManager();
			IChoiceQuery choiceQuery = new ChoiceQuery(servicesManager);

			Guid agentGuid = new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

			IQueueQueryManager queryManager = new QueueQueryManager(helper, agentGuid);
			IAgentService agentService = new AgentService(helper, queryManager, agentGuid);
			IJobServiceDataProvider jobServiceDataProvider = new JobServiceDataProvider(queryManager);
            IJobService jobService = new JobService(agentService, jobServiceDataProvider, new KubernetesMode(logger), helper);
			IEddsServiceContext eddsServiceContext = new EddsServiceContext(serviceContextHelper);
			IRepositoryFactory repositoryFactory = new RepositoryFactory(helper, servicesManager);
			IDBContext dbContext = helper.GetDBContext(helper.GetActiveCaseID());
			IWorkspaceDBContext workspaceDbContext = new WorkspaceDBContext(dbContext);
			IJobTrackerQueryManager jobTrackerQueryManager = new JobTrackerQueryManager(repositoryFactory, workspaceDbContext);
			IJobResourceTracker jobResourceTracker = new JobResourceTracker(jobTrackerQueryManager, queryManager);
			IJobTracker jobTracker = new JobTracker(jobResourceTracker, logger);
			IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager();
			IWorkspaceManager workspaceManager = new WorkspaceManager(repositoryFactory);
			IJobManager jobManager = new AgentJobManager(eddsServiceContext, jobService, helper, integrationPointSerializer, jobTracker);
			RelativityObjectManagerFactory relativityObjectManagerFactory = new RelativityObjectManagerFactory(helper);
			IRelativityObjectManager objectManager = relativityObjectManagerFactory.CreateRelativityObjectManager(workspaceArtifactId);
			IProviderTypeService providerTypeService = new ProviderTypeService(objectManager);
			IMessageService messageService = new MessageService();

			IJobHistoryService jobHistoryService = new JobHistoryService(
				caseServiceContext.RelativityObjectManagerService.RelativityObjectManager, 
				federatedInstanceManager, 
				workspaceManager, 
				logger, 
				integrationPointSerializer);

			IManagerFactory managerFactory = new ManagerFactory(helper, new FakeNonRemovableAgent());

			IIntegrationPointProviderValidator ipValidator = new IntegrationPointProviderValidator(Enumerable.Empty<IValidator>(), integrationPointSerializer, relativityObjectManagerFactory);

			IIntegrationPointPermissionValidator permissionValidator = new IntegrationPointPermissionValidator(Enumerable.Empty<IPermissionValidator>(), integrationPointSerializer);

			IValidationExecutor validationExecutor = new ValidationExecutor(ipValidator, permissionValidator, helper);

			ISecretsRepository secretsRepository = new SecretsRepository(
				SecretStoreFacadeFactory_Deprecated.Create(helper.GetSecretStore, logger), 
				logger
			);
			IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(
				caseServiceContext.RelativityObjectManagerService.RelativityObjectManager,
				integrationPointSerializer,
				secretsRepository,
				logger);

			IIntegrationPointProfileService integrationPointProfileService = new IntegrationPointProfileService(
				helper,
				caseServiceContext, 
				integrationPointSerializer, 
				choiceQuery,
				managerFactory, 
				validationExecutor, 
				objectManager);

			ISourceConfigurationTypeOfExportUpdater sourceConfigurationTypeOfExpertUpdater = new SourceConfigurationTypeOfExportUpdater(providerTypeService);

			return new SetTypeOfExportDefaultValueCommand(
				integrationPointRepository, 
				integrationPointProfileService,
				objectManager, 
				sourceConfigurationTypeOfExpertUpdater
			);
		}
	}
}
