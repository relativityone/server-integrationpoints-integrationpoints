using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Import;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.Core.Helpers.Implementations;
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
using kCura.IntegrationPoints.ImportProvider.Parser;
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
		private readonly Guid _agentGuid = new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID);

		private IIntegrationPointService _integrationPointService;
		private IJobHistoryService _jobHistoryService;

		public SetHasErrorsField()
		{
		}

		internal SetHasErrorsField(IIntegrationPointService integrationPointService, IJobHistoryService jobHistoryService)
		{
			_integrationPointService = integrationPointService;
			_jobHistoryService = jobHistoryService;
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
			IServiceContextHelper serviceContextHelper = new ServiceContextHelperForEventHandlers(Helper, Helper.GetActiveCaseID());
			ICaseServiceContext caseServiceContext = new CaseServiceContext(serviceContextHelper);
			IRepositoryFactory repositoryFactory = new RepositoryFactory(Helper, Helper.GetServicesManager());
			IChoiceQuery choiceQuery = new ChoiceQuery(Helper.GetServicesManager());
			IEddsServiceContext eddsServiceContext = new EddsServiceContext(serviceContextHelper);
			IQueryManager queryManager = new QueryManager(Helper, _agentGuid);
			IAgentService agentService = new AgentService(Helper, queryManager, _agentGuid);
			IJobServiceDataProvider jobServiceDataProvider = new JobServiceDataProvider(queryManager);
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
				caseServiceContext.RelativityObjectManagerService.RelativityObjectManager,
				federatedInstanceManager,
				workspaceManager,
				Logger,
				integrationPointSerializer
				);

			IManagerFactory managerFactory = new ManagerFactory(Helper, new FakeNonRemovableAgent(), jobServiceDataProvider);

			IIntegrationPointProviderValidator ipValidator = new IntegrationPointProviderValidator(Enumerable.Empty<IValidator>(), integrationPointSerializer);
			IIntegrationPointPermissionValidator permissionValidator = new IntegrationPointPermissionValidator(Enumerable.Empty<IPermissionValidator>(), integrationPointSerializer);
			IValidationExecutor validationExecutor = new ValidationExecutor(ipValidator, permissionValidator, Helper);

			ISecretsRepository secretsRepository = new SecretsRepository(
				SecretStoreFacadeFactory_Deprecated.Create(Helper.GetSecretStore, Logger),
				Logger
			);
			IIntegrationPointRepository integrationPointRepository = new IntegrationPointRepository(
				caseServiceContext.RelativityObjectManagerService.RelativityObjectManager,
				integrationPointSerializer,
				secretsRepository,
				Logger);

			IJobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(caseServiceContext, Helper, integrationPointRepository);

			IIntegrationPointTypeService integrationPointTypeService = new IntegrationPointTypeService(Helper, caseServiceContext);

			IDataTransferLocationService dataTransferLocationService = new DataTransferLocationService(Helper,
				integrationPointTypeService, new LongPathDirectory(), new CryptographyHelper());

			IImportFileLocationService importFileLocationService = new ImportFileLocationService(dataTransferLocationService,
				new JSONSerializer(), new LongPathDirectory());
			ITaskParametersBuilder taskParametersBuilder = new TaskParametersBuilder(importFileLocationService);

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
				caseServiceContext.RelativityObjectManagerService.RelativityObjectManager,
				taskParametersBuilder);
		}

		private IRelativityObjectManager CreateObjectManager(IEHHelper helper, int workspaceID)
		{
			return new RelativityObjectManagerFactory(helper).CreateRelativityObjectManager(workspaceID);
		}

		internal void UpdateIntegrationPointHasErrorsField(IntegrationPoint integrationPoint)
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

		internal IList<Data.IntegrationPoint> GetIntegrationPoints()
		{
			IList<Data.IntegrationPoint> integrationPoints = _integrationPointService.GetAllRDOsWithAllFields();
			return integrationPoints;
		}
	}
}
