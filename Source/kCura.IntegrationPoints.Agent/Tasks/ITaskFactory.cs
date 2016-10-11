using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Attributes;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.Installer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Services;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public interface ITaskFactory
	{
		ITask CreateTask(Job job, ScheduleQueueAgentBase agentBase);

		void Release(ITask task);
	}

	public class TaskFactory : ITaskFactory
	{
		private readonly IAgentHelper _helper;
		private readonly IAPILog _logger;
		private IAgentService _agentService;
		private ICaseServiceContext _caseServiceContext;

		private IWindsorContainer _container;
		private IContextContainerFactory _contextContainerFactory;
		private IEddsServiceContext _eddsServiceContext;
		private IIntegrationPointService _integrationPointService;
		private IJobHistoryService _jobHistoryService;
		private IJobService _jobService;
		private IManagerFactory _managerFactory;
		private IRepositoryFactory _repositoryFactory;
		private IRSAPIClient _rsapiClient;
		private ISerializer _serializer;
		private IWorkspaceDBContext _workspaceDbContext;

		public TaskFactory(IAgentHelper helper)
		{
			_helper = helper;
			_logger = _helper.GetLoggerFactory().GetLogger().ForContext<TaskFactory>();
		}

		public TaskFactory(IAgentHelper helper, IWindsorContainer container) : this(helper)
		{
			Container = container;
		}

		/// <summary>
		///     For unit tests only
		/// </summary>
		internal TaskFactory(IAgentHelper helper, ISerializer serializer, IContextContainerFactory contextContainerFactory, ICaseServiceContext caseServiceContext,
			IRSAPIClient rsapiClient, IWorkspaceDBContext workspaceDbContext, IEddsServiceContext eddsServiceContext,
			IRepositoryFactory repositoryFactory, IJobHistoryService jobHistoryService, IAgentService agentService, IJobService jobService, IManagerFactory managerFactory,
			IAPILog apiLog)
		{
			_helper = helper;
			_serializer = serializer;
			_contextContainerFactory = contextContainerFactory;
			_caseServiceContext = caseServiceContext;
			_rsapiClient = rsapiClient;
			_workspaceDbContext = workspaceDbContext;
			_eddsServiceContext = eddsServiceContext;
			_repositoryFactory = repositoryFactory;
			_jobHistoryService = jobHistoryService;
			_agentService = agentService;
			_jobService = jobService;
			_managerFactory = managerFactory;
			_logger = apiLog;
		}

		private IWindsorContainer Container
		{
			get { return _container ?? (_container = new WindsorContainer()); }
			set { _container = value; }
		}

		public ITask CreateTask(Job job, ScheduleQueueAgentBase agentBase)
		{
			LogCreatingTaskInformation(job);
			Install(job, agentBase);
			ResolveDependencies();
			IntegrationPoint integrationPointDto = GetIntegrationPoint(job);
			try
			{
				TaskType taskType;
				Enum.TryParse(job.TaskType, true, out taskType);

				//InjectionManager.Instance.Evaluate("E702D4CF-0468-4FEA-BA8D-6C8C20ED91F4");
				switch (taskType)
				{
					case TaskType.SyncManager:
						CheckForSynchronization(typeof(SyncManager), job, integrationPointDto, agentBase);
						return Container.Resolve<SyncManager>();

					case TaskType.SyncWorker:
						CheckForSynchronization(typeof(SyncWorker), job, integrationPointDto, agentBase);
						return Container.Resolve<SyncWorker>();

					case TaskType.SyncCustodianManagerWorker:
						CheckForSynchronization(typeof(SyncCustodianManagerWorker), job, integrationPointDto, agentBase);
						return Container.Resolve<SyncCustodianManagerWorker>();

					case TaskType.SendEmailManager:
						CheckForSynchronization(typeof(SendEmailManager), job, integrationPointDto, agentBase);
						return Container.Resolve<SendEmailManager>();

					case TaskType.SendEmailWorker:
						CheckForSynchronization(typeof(SendEmailWorker), job, integrationPointDto, agentBase);
						return Container.Resolve<SendEmailWorker>();

					case TaskType.ExportService:
						CheckForSynchronization(typeof(ExportServiceManager), job, integrationPointDto, agentBase);
						return Container.Resolve<ExportServiceManager>();

					case TaskType.ExportManager:
						CheckForSynchronization(typeof(ExportManager), job, integrationPointDto, agentBase);
						return Container.Resolve<ExportManager>();

					case TaskType.ExportWorker:
						CheckForSynchronization(typeof(ExportWorker), job, integrationPointDto, agentBase);
						return Container.Resolve<ExportWorker>();

					default:
						LogUnknownTaskTypeError(taskType);
						return null;
				}
			}
			catch (AgentDropJobException e)
			{
				//we catch this type of exception when an agent explicitly drops a Job, and we bubble up the exception message to the Errors tab.
				LogAgentDropJobException(job, e);
				throw;
			}
			catch (Exception e)
			{
				LogErrorDuringTaskCreation(e, job.TaskType, job.JobId);
				UpdateJobHistoryOnFailure(job, integrationPointDto, e);
				throw;
			}
		}

		public void Release(ITask task)
		{
			try
			{
				if (task != null)
				{
					Container.Release(task);
				}
			}
			finally
			{
				Container = null;
			}
		}

		private void Install(Job job, ScheduleQueueAgentBase agentBase)
		{
			var agentInstaller = new AgentInstaller(_helper, job, agentBase.ScheduleRuleFactory);
			Container.Install(agentInstaller);
		}

		private void ResolveDependencies()
		{
			_serializer = Container.Resolve<ISerializer>();
			_caseServiceContext = Container.Resolve<ICaseServiceContext>();
			_rsapiClient = Container.Resolve<IRSAPIClient>();
			_workspaceDbContext = Container.Resolve<IWorkspaceDBContext>();
			_eddsServiceContext = Container.Resolve<IEddsServiceContext>();
			_repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_contextContainerFactory = Container.Resolve<IContextContainerFactory>();
			_integrationPointService = Container.Resolve<IIntegrationPointService>();

			_agentService = new AgentService(_helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
			_jobService = new JobService(_agentService, _helper);
			_managerFactory = new ManagerFactory();
		}

		private void CheckForSynchronization(Type type, Job job, IntegrationPoint integrationPointDto, ScheduleQueueAgentBase agentBase)
		{
			object[] attributes = type.GetCustomAttributes(false);
			foreach (object attribute in attributes)
			{
				if (attribute is SynchronizedTaskAttribute)
				{
					if (HasOtherJobsExecuting(job))
					{
						DropJobAndThrowException(job, integrationPointDto, agentBase);
					}
					break;
				}
			}
		}

		private IntegrationPoint GetIntegrationPoint(Job job)
		{
			IIntegrationPointService integrationPointService;
			if (_integrationPointService == null)
			{
				IChoiceQuery choiceQuery = new ChoiceQuery(_rsapiClient);
				JobResourceTracker jobResourceTracker = new JobResourceTracker(_repositoryFactory, _workspaceDbContext);
				JobTracker jobTracker = new JobTracker(jobResourceTracker);
				IJobManager jobManager = new AgentJobManager(_eddsServiceContext, _jobService, _serializer, jobTracker);

				integrationPointService = new IntegrationPointService(_helper, _caseServiceContext,
					_contextContainerFactory, _serializer, choiceQuery, jobManager, _jobHistoryService, _managerFactory);
			}
			else
			{
				integrationPointService = _integrationPointService;
			}

			IntegrationPoint integrationPoint = integrationPointService.GetRdo(job.RelatedObjectArtifactID);

			if (integrationPoint == null)
			{
				LogIntegrationPointNotFound(job);
				throw new NullReferenceException(
					$"Unable to retrieve the integration point for the following job: {job.JobId}");
			}

			return integrationPoint;
		}

		private JobHistory GetJobHistory(Job job, IntegrationPoint integrationPointDto)
		{
			TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
			JobHistory jobHistory = _jobHistoryService.CreateRdo(
				integrationPointDto,
				taskParameters.BatchInstance,
				string.IsNullOrEmpty(job.ScheduleRuleType)
					? JobTypeChoices.JobHistoryRun
					: JobTypeChoices.JobHistoryScheduledRun, DateTime.Now);

			return jobHistory;
		}

		private void UpdateJobHistoryOnFailure(Job job, IntegrationPoint integrationPointDto, Exception e)
		{
			JobHistory jobHistory = GetJobHistory(job, integrationPointDto);

			IJobHistoryErrorService jobHistoryErrorService = Container.Resolve<IJobHistoryErrorService>();
			jobHistoryErrorService.IntegrationPoint = integrationPointDto;
			jobHistoryErrorService.JobHistory = jobHistory;
			jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
			jobHistoryErrorService.CommitErrors();

			jobHistory.JobStatus = JobStatusChoices.JobHistoryErrorJobFailed;
			_jobHistoryService.UpdateRdo(jobHistory);

			// No updates to IP since the job history error service handles IP updates
		}

		internal bool HasOtherJobsExecuting(Job job)
		{
			IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper);
			IQueueManager queueManager = _managerFactory.CreateQueueManager(contextContainer);

			bool hasOtherJobsExecuting = queueManager.HasJobsExecuting(job.WorkspaceID, job.RelatedObjectArtifactID, job.JobId, job.NextRunTime);

			return hasOtherJobsExecuting;
		}

		internal void DropJobAndThrowException(Job job, IntegrationPoint integrationPointDto, ScheduleQueueAgentBase agentBase)
		{
			string exceptionMessage = "Unable to execute Integration Point job: There is already a job currently running.";

			//check if it's a scheduled job
			if (!string.IsNullOrEmpty(job.ScheduleRuleType))
			{
				integrationPointDto.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, agentBase.ScheduleRuleFactory, new TaskResult {Status = TaskStatusEnum.None});
				exceptionMessage = $@"{exceptionMessage} Job is re-scheduled for {integrationPointDto.NextScheduledRuntimeUTC}.";
			}
			else
			{
				JobHistory jobHistory = GetJobHistory(job, integrationPointDto);
				RemoveJobHistoryFromIntegrationPoint(integrationPointDto, jobHistory);
			}

			LogDroppingJob(job, integrationPointDto, exceptionMessage);

			throw new AgentDropJobException(exceptionMessage);
		}

		internal void RemoveJobHistoryFromIntegrationPoint(IntegrationPoint integrationPointDto, JobHistory jobHistory)
		{
			List<int> jobHistoryIds = integrationPointDto.JobHistory.ToList();
			jobHistoryIds.Remove(jobHistory.ArtifactId);
			integrationPointDto.JobHistory = jobHistoryIds.ToArray();
			_caseServiceContext.RsapiService.IntegrationPointLibrary.Update(integrationPointDto);

			jobHistory.JobStatus = JobStatusChoices.JobHistoryStopped;
			_jobHistoryService.UpdateRdo(jobHistory);
			_jobHistoryService.DeleteRdo(jobHistory.ArtifactId);
		}

		#region Logging

		private void LogCreatingTaskInformation(Job job)
		{
			_logger.LogInformation("Attempting to create task {TaskType} for job {JobId}.", job.TaskType, job.JobId);
		}

		private void LogUnknownTaskTypeError(TaskType taskType)
		{
			_logger.LogError("Unable to create task. Unknown task type ({TaskType})", taskType);
		}

		private void LogErrorDuringTaskCreation(Exception exception, string taskType, long jobId)
		{
			_logger.LogError(exception, "Error during task creation ({TaskType}) for job {JobId}", taskType, jobId);
		}

		private void LogAgentDropJobException(Job job, AgentDropJobException agentDropJobException)
		{
			_logger.LogError(agentDropJobException, "Agent explicitly dropped job {JobId}.", job.JobId);
		}

		private void LogIntegrationPointNotFound(Job job)
		{
			_logger.LogError("Unable to retrieve the integration point for the following job: {JobId}", job.JobId);
		}

		private void LogDroppingJob(Job job, IntegrationPoint integrationPointDto, string exceptionMessage)
		{
			_logger.LogError("{ExceptionMessage}. Job Id: {JobId}. Task type: {TaskType}. Integration Point Id: {IntegrationPointId}.", exceptionMessage, job.JobId, job.TaskType,
				integrationPointDto.ArtifactId);
		}

		#endregion
	}
}