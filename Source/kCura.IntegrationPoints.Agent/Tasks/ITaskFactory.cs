using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using kCura.IntegrationPoints.Agent.Attributes;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.Installer;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Factories.Implementations;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
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
		private IJobHistoryService _jobHistoryService;
		private IJobService _jobService;
		private IManagerFactory _managerFactory;
		private IHelperFactory _helperFactory;
		private IServiceFactory _serviceFactory;
		private IIntegrationPointSerializer _serializer;

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
		internal TaskFactory(IAgentHelper helper, IIntegrationPointSerializer serializer, IContextContainerFactory contextContainerFactory, ICaseServiceContext caseServiceContext,
			IJobHistoryService jobHistoryService, IAgentService agentService, IJobService jobService, IManagerFactory managerFactory, IAPILog apiLog)
		{
			_helper = helper;
			_serializer = serializer;
			_contextContainerFactory = contextContainerFactory;
			_caseServiceContext = caseServiceContext;
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
			IntegrationPoint integrationPointDto = GetIntegrationPoint(job);
			ResolveDependencies(integrationPointDto);
			try
			{
				TaskType taskType;
				Enum.TryParse(job.TaskType, true, out taskType);

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

					case TaskType.ImportService:
						CheckForSynchronization(typeof(ImportServiceManager), job, integrationPointDto, agentBase);
						return Container.Resolve<ImportServiceManager>();

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
			Container.Install(new Data.Installers.QueryInstallers());
			Container.Install(new Core.Installers.KeywordInstaller());
			Container.Install(new Core.Installers.ServicesInstaller());
			Container.Install(new FilesDestinationProvider.Core.Installer.FileNamingInstaller());
			Container.Install(new FilesDestinationProvider.Core.Installer.ExportInstaller());
			Container.Install(new ImportProvider.Parser.ServicesInstaller());
			Container.Install(new AgentInstaller(_helper, job, agentBase.ScheduleRuleFactory));
		}

		private void ResolveDependencies(IntegrationPoint integrationPointDto)
		{
			_serializer = Container.Resolve<IIntegrationPointSerializer>();
			_contextContainerFactory = Container.Resolve<IContextContainerFactory>();
			_helperFactory = Container.Resolve<IHelperFactory>();
			_serviceFactory = Container.Resolve<IServiceFactory>();

			_agentService = new AgentService(_helper, new Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
			_jobService = new JobService(_agentService, _helper);
			_managerFactory = new ManagerFactory(_helper);
			_jobHistoryService = CreateJobHistoryService(integrationPointDto);
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
			_caseServiceContext = Container.Resolve<ICaseServiceContext>();

			var integrationPoint = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(job.RelatedObjectArtifactID);

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
				integrationPointDto.NextScheduledRuntimeUTC = _jobService.GetJobNextUtcRunDateTime(job, agentBase.ScheduleRuleFactory, new TaskResult { Status = TaskStatusEnum.None });
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

		private IJobHistoryService CreateJobHistoryService(IntegrationPoint integrationPoint)
		{
			DestinationConfiguration destinationConfiguration = _serializer.Deserialize<DestinationConfiguration>(integrationPoint.DestinationConfiguration);
			var targetHelper = _helperFactory.CreateTargetHelper(_helper, destinationConfiguration.FederatedInstanceArtifactId, integrationPoint.SecuredConfiguration);
			return _serviceFactory.CreateJobHistoryService(_helper, targetHelper);
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