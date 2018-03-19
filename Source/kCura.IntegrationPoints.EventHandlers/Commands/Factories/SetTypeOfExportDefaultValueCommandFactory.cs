﻿using System;
using System.Linq;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Authentication;
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
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Commands.Factories
{
	public class SetTypeOfExportDefaultValueCommandFactory
	{
		public static SetTypeOfExportDefaultValueCommand Create(IEHHelper helper, int workspaceArtifactId)
		{
			IRsapiClientWithWorkspaceFactory rsapiClientFactory = new RsapiClientWithWorkspaceFactory(helper);
			IServiceContextHelper serviceContextHelper = new ServiceContextHelperForEventHandlers(helper, helper.GetActiveCaseID(), rsapiClientFactory);
			ICaseServiceContext caseServiceContext = new CaseServiceContext(serviceContextHelper);

			IContextContainerFactory contextContainerFactory = new ContextContainerFactory();

			IAPILog logger = helper.GetLoggerFactory().GetLogger();
			IIntegrationPointSerializer integrationPointSerializer = new IntegrationPointSerializer(logger);

			IRSAPIClient rsapiClient = rsapiClientFactory.CreateAdminClient(helper.GetActiveCaseID());
			IChoiceQuery choiceQuery = new ChoiceQuery(rsapiClient);

			IAgentService agentService = new AgentService(helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
			IJobServiceDataProvider jobServiceDataProvider = new JobServiceDataProvider(agentService, helper);
			IJobService jobService = new JobService(agentService, jobServiceDataProvider, helper);
			IEddsServiceContext eddsServiceContext = new EddsServiceContext(serviceContextHelper);
			IRepositoryFactory repositoryFactory = new RepositoryFactory(helper, helper.GetServicesManager());
			IDBContext dbContext = helper.GetDBContext(helper.GetActiveCaseID());
			IWorkspaceDBContext workspaceDbContext = new WorkspaceContext(dbContext);
			IJobResourceTracker jobResourceTracker = new JobResourceTracker(repositoryFactory, workspaceDbContext);
			IJobTracker jobTracker = new JobTracker(jobResourceTracker);
			IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager(repositoryFactory);
			IWorkspaceManager workspaceManager = new WorkspaceManager(repositoryFactory);
			IJobManager jobManager = new AgentJobManager(eddsServiceContext, jobService, helper, integrationPointSerializer, jobTracker);

			IJobHistoryService jobHistoryService = new JobHistoryService(caseServiceContext, federatedInstanceManager, workspaceManager, helper, integrationPointSerializer);

			IConfigFactory configFactory = new ConfigFactory();
			IAuthProvider authProvider = new AuthProvider();
			IAuthTokenGenerator tokenGenerator = new ClaimsTokenGenerator();
			ICredentialProvider credentialProvider = new TokenCredentialProvider(authProvider, tokenGenerator, helper);
			ITokenProvider tokenProvider = new RelativityCoreTokenProvider();
			ISerializer serializer = new JSONSerializer();
			ISqlServiceFactory sqlServiceFactory = new HelperConfigSqlServiceFactory(helper);
			IServiceManagerProvider serviceManagerProvider = new ServiceManagerProvider(configFactory, credentialProvider, serializer, tokenProvider, sqlServiceFactory);
			IManagerFactory managerFactory = new ManagerFactory(helper, serviceManagerProvider);

			IIntegrationPointProviderValidator ipValidator = new IntegrationPointProviderValidator(Enumerable.Empty<IValidator>(), integrationPointSerializer);

			IIntegrationPointPermissionValidator permissionValidator = new IntegrationPointPermissionValidator(Enumerable.Empty<IPermissionValidator>(), integrationPointSerializer);

			IIntegrationPointService integrationPointService = new IntegrationPointService(helper, caseServiceContext,
				contextContainerFactory, integrationPointSerializer, choiceQuery, jobManager, jobHistoryService,
				managerFactory, ipValidator, permissionValidator);

			IIntegrationPointProfileService integrationPointProfileService = new IntegrationPointProfileService(helper,
				caseServiceContext, contextContainerFactory, integrationPointSerializer, choiceQuery, managerFactory,
				ipValidator, permissionValidator);

			IRSAPIService rsapiService = new RSAPIService(helper, workspaceArtifactId);
			IProviderTypeService providerTypeService = new ProviderTypeService(rsapiService);
			ISourceConfigurationTypeOfExportUpdater sourceConfigurationTypeOfExpertUpdater = new SourceConfigurationTypeOfExportUpdater(providerTypeService);

			return new SetTypeOfExportDefaultValueCommand(integrationPointService, integrationPointProfileService,
				rsapiService.RelativityObjectManager, sourceConfigurationTypeOfExpertUpdater);
		}
	}
}