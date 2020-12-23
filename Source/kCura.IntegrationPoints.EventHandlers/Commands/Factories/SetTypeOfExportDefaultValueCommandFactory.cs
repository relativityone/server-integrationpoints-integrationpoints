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
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Repositories.Implementations;

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

			IChoiceQuery choiceQuery = new ChoiceQuery(helper.GetServicesManager());

			IAgentService agentService = new AgentService(helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
			IJobServiceDataProvider jobServiceDataProvider = new JobServiceDataProvider(agentService, helper);
			IJobService jobService = new JobService(agentService, jobServiceDataProvider, helper);
			IEddsServiceContext eddsServiceContext = new EddsServiceContext(serviceContextHelper);
			IRepositoryFactory repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
			IDBContext dbContext = helper.GetDBContext(helper.GetActiveCaseID());
			IWorkspaceDBContext workspaceDbContext = new WorkspaceDBContext(dbContext);
			IJobResourceTracker jobResourceTracker = new JobResourceTracker(repositoryFactory, workspaceDbContext);
			IJobTracker jobTracker = new JobTracker(jobResourceTracker);
			IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager();
			IWorkspaceManager workspaceManager = new WorkspaceManager(repositoryFactory);
			IJobManager jobManager = new AgentJobManager(eddsServiceContext, jobService, helper, integrationPointSerializer, jobTracker);
			IRelativityObjectManager objectManager =
				new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(workspaceArtifactId);
			IProviderTypeService providerTypeService = new ProviderTypeService(objectManager);
			IMessageService messageService = new MessageService();

			IJobHistoryService jobHistoryService = new JobHistoryService(
				caseServiceContext.RsapiService.RelativityObjectManager, 
				federatedInstanceManager, 
				workspaceManager, 
				logger, 
				integrationPointSerializer);

			IManagerFactory managerFactory = new ManagerFactory(helper);

			IIntegrationPointProviderValidator ipValidator = new IntegrationPointProviderValidator(Enumerable.Empty<IValidator>(), integrationPointSerializer);

			IIntegrationPointPermissionValidator permissionValidator = new IntegrationPointPermissionValidator(Enumerable.Empty<IPermissionValidator>(), integrationPointSerializer);

			IValidationExecutor validationExecutor = new ValidationExecutor(ipValidator, permissionValidator, helper);

			ISecretsRepository secretsRepository = new SecretsRepository(
				SecretStoreFacadeFactory_Deprecated.Create(helper.GetSecretStore, logger), 
				logger
			);
			IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(
				caseServiceContext.RsapiService.RelativityObjectManager,
				integrationPointSerializer,
				secretsRepository,
				logger);
			IJobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(caseServiceContext, helper, integrationPointRepository);
			IIntegrationPointService integrationPointService = new IntegrationPointService(helper, caseServiceContext,
				integrationPointSerializer, choiceQuery, jobManager, jobHistoryService,
				jobHistoryErrorService, managerFactory, validationExecutor, providerTypeService, messageService, integrationPointRepository,
				caseServiceContext.RsapiService.RelativityObjectManager);

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
				integrationPointService, 
				integrationPointProfileService,
				objectManager, 
				sourceConfigurationTypeOfExpertUpdater
			);
		}
	}
}
