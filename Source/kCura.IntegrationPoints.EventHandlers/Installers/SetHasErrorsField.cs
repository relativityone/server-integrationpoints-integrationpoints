using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler.CustomAttributes;
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
using kCura.IntegrationPoints.SourceProviderInstaller;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;
using IFederatedInstanceManager = kCura.IntegrationPoints.Domain.Managers.IFederatedInstanceManager;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Guid("5E882EE9-9E9D-4AFA-9B2C-EAC6C749A8D4")]
	[Description("Updates the Has Errors field on existing Integration Points.")]
	[RunOnce(true)]
	public class SetHasErrorsField : PostInstallEventHandlerBase
	{
		private ICaseServiceContext _caseServiceContext;
		private IIntegrationPointService _integrationPointService;
		private IJobHistoryService _jobHistoryService;

		public SetHasErrorsField()
		{
		}

		internal SetHasErrorsField(IIntegrationPointService integrationPointService, IJobHistoryService jobHistoryService, ICaseServiceContext caseServiceContext)
		{
			_integrationPointService = integrationPointService;
			_jobHistoryService = jobHistoryService;
			_caseServiceContext = caseServiceContext;
		}

		protected override IAPILog CreateLogger()
		{
			return Helper.GetLoggerFactory().GetLogger().ForContext<SetHasErrorsField>();
		}

		protected override string SuccessMessage
			=> "Updating the Has Errors field on the Integration Point object completed successfully";

		protected override string GetFailureMessage(Exception ex)
		{
			return "Updating the Has Errors field on the Integration Point object failed.";
		}

		protected override void Run()
		{
			CreateServices();
			ExecuteInstanced();
		}

		internal void ExecuteInstanced()
		{
			
			IList<Data.IntegrationPoint> integrationPoints = GetIntegrationPoints();

			foreach (Data.IntegrationPoint integrationPoint in integrationPoints)
			{
				UpdateIntegrationPointHasErrorsField(integrationPoint);
			}
		}

		/// <summary>
		///     It is best to use the Castle Windsor container here instead of manually creating the dependencies.
		///     TODO: replace the below with the container and resolve the dependencies.
		/// </summary>
		private void CreateServices()
		{
			IRsapiClientFactory rsapiClientFactory = new RsapiClientFactory(Helper);
			IServiceContextHelper serviceContextHelper = new ServiceContextHelperForEventHandlers(Helper, Helper.GetActiveCaseID(), rsapiClientFactory);
			ICaseServiceContext caseServiceContext = new CaseServiceContext(serviceContextHelper);
			IRepositoryFactory repositoryFactory = new RepositoryFactory(Helper, Helper.GetServicesManager());
			IRSAPIClient rsapiClient = rsapiClientFactory.CreateAdminClient(Helper.GetActiveCaseID());
			IChoiceQuery choiceQuery = new ChoiceQuery(rsapiClient);
			IEddsServiceContext eddsServiceContext = new EddsServiceContext(serviceContextHelper);
			IAgentService agentService = new AgentService(Helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
			IJobServiceDataProvider jobServiceDataProvider = new JobServiceDataProvider(agentService, Helper);
			IJobService jobService = new JobService(agentService, jobServiceDataProvider, Helper);
			IDBContext dbContext = Helper.GetDBContext(Helper.GetActiveCaseID());
			IWorkspaceDBContext workspaceDbContext = new WorkspaceContext(dbContext);
			IJobResourceTracker jobResourceTracker = new JobResourceTracker(repositoryFactory, workspaceDbContext);
			IJobTracker jobTracker = new JobTracker(jobResourceTracker);
			IIntegrationPointSerializer integrationPointSerializer = new IntegrationPointSerializer();
			IJobManager jobManager = new AgentJobManager(eddsServiceContext, jobService, Helper, integrationPointSerializer, jobTracker);
			IWorkspaceManager workspaceManager = new WorkspaceManager(repositoryFactory);
			IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager(repositoryFactory);

			_jobHistoryService = new JobHistoryService(caseServiceContext, federatedInstanceManager, workspaceManager, Helper, integrationPointSerializer);
			IContextContainerFactory contextContainerFactory = new ContextContainerFactory();

			IConfigFactory configFactory = new ConfigFactory();
			IAuthProvider authProvider = new AuthProvider();
			IAuthTokenGenerator authTokenGenerator = new ClaimsTokenGenerator();
			ICredentialProvider credentialProvider = new TokenCredentialProvider(authProvider, authTokenGenerator, Helper);
			ITokenProvider tokenProvider = new RelativityCoreTokenProvider();
			ISerializer serializer = new JSONSerializer();
			ISqlServiceFactory sqlServiceFactory = new HelperConfigSqlServiceFactory(Helper);
			IServiceManagerProvider serviceManagerProvider = new ServiceManagerProvider(configFactory, credentialProvider, serializer, tokenProvider, sqlServiceFactory);
			IManagerFactory managerFactory = new ManagerFactory(Helper, serviceManagerProvider);

			_caseServiceContext = caseServiceContext;
			IIntegrationPointProviderValidator ipValidator = new IntegrationPointProviderValidator(Enumerable.Empty<IValidator>(), integrationPointSerializer);
			IIntegrationPointPermissionValidator permissionValidator = new IntegrationPointPermissionValidator(Enumerable.Empty<IPermissionValidator>(), integrationPointSerializer);

			_integrationPointService = new IntegrationPointService(Helper, caseServiceContext, contextContainerFactory, integrationPointSerializer,
				choiceQuery, jobManager, _jobHistoryService, managerFactory, ipValidator, permissionValidator);
		}

		internal void UpdateIntegrationPointHasErrorsField(Data.IntegrationPoint integrationPoint)
		{
			integrationPoint.HasErrors = false;

			if (integrationPoint.JobHistory.Length > 0)
			{
				IList<JobHistory> jobHistories = _jobHistoryService.GetJobHistory(integrationPoint.JobHistory);

				JobHistory lastCompletedJob = jobHistories?
					.Where(jobHistory => jobHistory.EndTimeUTC != null)
					.OrderByDescending(jobHistory => jobHistory.EndTimeUTC)
					.FirstOrDefault();

				if ((lastCompletedJob != null) && (lastCompletedJob.JobStatus.Name != JobStatusChoices.JobHistoryCompleted.Name))
				{
					integrationPoint.HasErrors = true;
				}
			}

			_caseServiceContext.RsapiService.RelativityObjectManager.Update(integrationPoint);
		}

		internal IList<Data.IntegrationPoint> GetIntegrationPoints()
		{
			IList<Data.IntegrationPoint> integrationPoints = _integrationPointService.GetAllRDOs();
			return integrationPoints;
		}
	}
}