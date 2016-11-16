using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler;
using kCura.EventHandler.CustomAttributes;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Implementation;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Installers
{
	[Guid("5E882EE9-9E9D-4AFA-9B2C-EAC6C749A8D4")]
	[Description("Updates the Has Errors field on existing Integration Points.")]
	[RunOnce(true)]
	public class SetHasErrorsField : PostInstallEventHandler
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

		public override Response Execute()
		{
			CreateServices();
			return ExecuteInstanced();
		}

		internal Response ExecuteInstanced()
		{
			var response = new Response
			{
				Message = "Updated successfully.",
				Success = true
			};

			try
			{
				IList<Data.IntegrationPoint> integrationPoints = GetIntegrationPoints();

				foreach (Data.IntegrationPoint integrationPoint in integrationPoints)
				{
					UpdateIntegrationPointHasErrorsField(integrationPoint);
				}
			}
			catch (Exception e)
			{
				LogSettingHasErrorsFieldError(e);
				response.Message = $"Updating the Has Errors field on the Integration Point object failed. Exception message: {e.Message}.";
				response.Exception = e;
				response.Success = false;
			}

			return response;
		}

		/// <summary>
		///     It is best to use the Castle Windsor container here instead of manually creating the dependencies.
		///     TODO: replace the below with the container and resolve the dependencies.
		/// </summary>
		private void CreateServices()
		{
			RsapiClientFactory rsapiClientFactory = new RsapiClientFactory(Helper);
			IServiceContextHelper serviceContextHelper = new ServiceContextHelperForEventHandlers(Helper, Helper.GetActiveCaseID(), rsapiClientFactory);
			ICaseServiceContext caseServiceContext = new CaseServiceContext(serviceContextHelper);
			IRepositoryFactory repositoryFactory = new RepositoryFactory(Helper);
			IWorkspaceRepository workspaceRepository = repositoryFactory.GetWorkspaceRepository();
			IRSAPIClient rsapiClient = rsapiClientFactory.CreateClientForWorkspace(Helper.GetActiveCaseID(), ExecutionIdentity.System);
			IChoiceQuery choiceQuery = new ChoiceQuery(rsapiClient);
			IEddsServiceContext eddsServiceContext = new EddsServiceContext(serviceContextHelper);
			IAgentService agentService = new AgentService(Helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
			IJobService jobService = new JobService(agentService, Helper);
			IDBContext dbContext = Helper.GetDBContext(Helper.GetActiveCaseID());
			IWorkspaceDBContext workspaceDbContext = new WorkspaceContext(dbContext);
			JobResourceTracker jobResourceTracker = new JobResourceTracker(repositoryFactory, workspaceDbContext);
			JobTracker jobTracker = new JobTracker(jobResourceTracker);
			ISerializer serializer = new JSONSerializer();
			IJobManager jobManager = new AgentJobManager(eddsServiceContext, jobService, Helper, serializer, jobTracker);
			_jobHistoryService = new JobHistoryService(caseServiceContext, workspaceRepository, Helper, serializer);
			IContextContainerFactory contextContainerFactory = new ContextContainerFactory();
			IManagerFactory managerFactory = new ManagerFactory(Helper);

			_caseServiceContext = caseServiceContext;
			IIntegrationModelValidator ipValidator = new IntegrationModelValidator(Enumerable.Empty<IValidator>());
			_integrationPointService = new IntegrationPointService(Helper, caseServiceContext, contextContainerFactory, serializer, choiceQuery, jobManager, _jobHistoryService, managerFactory, ipValidator);
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

			_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(integrationPoint);
		}

		internal IList<Data.IntegrationPoint> GetIntegrationPoints()
		{
			IList<Data.IntegrationPoint> integrationPoints = _integrationPointService.GetAllIntegrationPoints();
			return integrationPoints;
		}

		#region Logging

		private void LogSettingHasErrorsFieldError(Exception e)
		{
			var logger = Helper.GetLoggerFactory().GetLogger().ForContext<SetHasErrorsField>();
			logger.LogError(e, "Updating the Has Errors field on the Integration Point object failed.");
		}

		#endregion
	}
}