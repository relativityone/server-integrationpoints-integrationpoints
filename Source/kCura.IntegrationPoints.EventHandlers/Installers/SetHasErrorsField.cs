using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.EventHandler.CustomAttributes;
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
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
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

		public SetHasErrorsField(IIntegrationPointService integrationPointService, IJobHistoryService jobHistoryService, ICaseServiceContext caseServiceContext)
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

		public void ExecuteInstanced()
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
			IRsapiClientWithWorkspaceFactory rsapiClientFactory = new RsapiClientWithWorkspaceFactory(Helper);
			IServiceContextHelper serviceContextHelper = new ServiceContextHelperForEventHandlers(Helper, Helper.GetActiveCaseID());
			ICaseServiceContext caseServiceContext = new CaseServiceContext(serviceContextHelper);
			IRepositoryFactory repositoryFactory = new RepositoryFactory(Helper, Helper.GetServicesManager());
			IRSAPIClient rsapiClient = rsapiClientFactory.CreateAdminClient(Helper.GetActiveCaseID());
			IChoiceQuery choiceQuery = new ChoiceQuery(rsapiClient);
			IEddsServiceContext eddsServiceContext = new EddsServiceContext(serviceContextHelper);
			IAgentService agentService = new AgentService(Helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
			IJobServiceDataProvider jobServiceDataProvider = new JobServiceDataProvider(agentService, Helper);
			IJobService jobService = new JobService(agentService, jobServiceDataProvider, Helper);
			IDBContext dbContext = Helper.GetDBContext(Helper.GetActiveCaseID());
			IWorkspaceDBContext workspaceDbContext = new WorkspaceDBContext(dbContext);
			IJobResourceTracker jobResourceTracker = new JobResourceTracker(repositoryFactory, workspaceDbContext);
			IJobTracker jobTracker = new JobTracker(jobResourceTracker);
			IIntegrationPointSerializer integrationPointSerializer = new IntegrationPointSerializer(Logger);
			IJobManager jobManager = new AgentJobManager(eddsServiceContext, jobService, Helper, integrationPointSerializer, jobTracker);
			IWorkspaceManager workspaceManager = new WorkspaceManager(repositoryFactory);
			IFederatedInstanceManager federatedInstanceManager = new FederatedInstanceManager();
			IProviderTypeService providerTypeService = new ProviderTypeService(CreateObjectManager(Helper, caseServiceContext.WorkspaceID));
			IMessageService messageService = new MessageService();

			_jobHistoryService = new JobHistoryService(
				caseServiceContext.RsapiService.RelativityObjectManager,
				federatedInstanceManager,
				workspaceManager,
				Logger,
				integrationPointSerializer,
				providerTypeService,
				messageService);

			IManagerFactory managerFactory = new ManagerFactory(Helper);

			_caseServiceContext = caseServiceContext;
			IIntegrationPointProviderValidator ipValidator = new IntegrationPointProviderValidator(Enumerable.Empty<IValidator>(), integrationPointSerializer);
			IIntegrationPointPermissionValidator permissionValidator = new IntegrationPointPermissionValidator(Enumerable.Empty<IPermissionValidator>(), integrationPointSerializer);
			IValidationExecutor validationExecutor = new ValidationExecutor(ipValidator, permissionValidator, Helper);

			ISecretsRepository secretsRepository = new SecretsRepository(
				SecretStoreFacadeFactory_Deprecated.Create(Helper.GetSecretStore, Logger),
				Logger
			);
			IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(
				caseServiceContext.RsapiService.RelativityObjectManager,
				integrationPointSerializer,
				secretsRepository,
				Logger);

			IJobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(caseServiceContext, Helper, integrationPointRepository);

			_integrationPointService = new IntegrationPointService(
				Helper, 
				caseServiceContext,
				integrationPointSerializer, 
				choiceQuery, 
				jobManager, 
				_jobHistoryService, 
				jobHistoryErrorService, 
				managerFactory,
				validationExecutor, 
				providerTypeService, 
				messageService, 
				integrationPointRepository,
				caseServiceContext.RsapiService.RelativityObjectManager);
		}

		private IRelativityObjectManager CreateObjectManager(IEHHelper helper, int workspaceID)
		{
			return new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(workspaceID);
		}

		public void UpdateIntegrationPointHasErrorsField(IntegrationPoint integrationPoint)
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

			_integrationPointService.UpdateIntegrationPoint(integrationPoint);
		}

		public IList<Data.IntegrationPoint> GetIntegrationPoints()
		{
			IList<Data.IntegrationPoint> integrationPoints = _integrationPointService.GetAllRDOsWithAllFields();
			return integrationPoints;
		}
	}
}