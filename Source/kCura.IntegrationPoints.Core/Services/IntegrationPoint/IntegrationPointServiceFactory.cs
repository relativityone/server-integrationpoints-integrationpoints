using System;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Abstract;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.IntegrationPoint
{
	public class IntegrationPointServiceFactory : IIntegrationPointServiceFactory
	{
		private readonly IHelper _helper;
		private readonly IServiceContextHelper _serviceContextHelper;
		private readonly ISerializer _serializer;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly int _workspaceArtifactId;
		private readonly RsapiClientFactory _rsapiClientFactory;

		public IntegrationPointServiceFactory(int workspaceArtifactId, IHelper helper, IServiceContextHelper serviceContextHelper, ISerializer serializer, IRepositoryFactory repositoryFactory, RsapiClientFactory rsapiClientFactory)
		{
			_helper = helper;
			_serviceContextHelper = serviceContextHelper;
			_serializer = serializer;
			_repositoryFactory = repositoryFactory;
			_rsapiClientFactory = rsapiClientFactory;
			_workspaceArtifactId = workspaceArtifactId;
		}

		public IIntegrationPointService Create()
		{
			ICaseServiceContext caseServiceContext = new CaseServiceContext(_serviceContextHelper);
			IWorkspaceRepository workspaceRepository = _repositoryFactory.GetWorkspaceRepository();
			IRSAPIClient rsapiClient = _rsapiClientFactory.CreateClientForWorkspace(_workspaceArtifactId, ExecutionIdentity.System);
			IChoiceQuery choiceQuery = new ChoiceQuery(rsapiClient);
			IEddsServiceContext eddsServiceContext = new EddsServiceContext(_serviceContextHelper);
			IAgentService agentService = new AgentService(_helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
			IJobService jobService = new JobService(agentService, _helper);
			IDBContext dbContext = _helper.GetDBContext(_workspaceArtifactId);
			IWorkspaceDBContext workspaceDbContext = new WorkspaceContext(dbContext);
			JobResourceTracker jobResourceTracker = new JobResourceTracker(_repositoryFactory, workspaceDbContext);
			JobTracker jobTracker = new JobTracker(jobResourceTracker);
			IJobManager jobManager = new AgentJobManager(eddsServiceContext, jobService, _helper, _serializer, jobTracker);
			IJobHistoryService jobHistoryService = new JobHistoryService(caseServiceContext, workspaceRepository, _helper, _serializer);
			IContextContainerFactory contextContainerFactory = new ContextContainerFactory();
			IManagerFactory managerFactory = new ManagerFactory(_helper);

			IIntegrationPointProviderValidator ipValidator = new IntegrationPointProviderValidator(Enumerable.Empty<IValidator>(), _serializer);
			IIntegrationPointPermissionValidator permissionValidator = new IntegrationPointPermissionValidator(Enumerable.Empty<IPermissionValidator>(), _serializer);

			return new IntegrationPointService(_helper, caseServiceContext, contextContainerFactory, _serializer,
				choiceQuery, jobManager, jobHistoryService, managerFactory, ipValidator, permissionValidator);
		}
	}
}