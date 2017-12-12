using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using kCura.Agent.CustomAttributes;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Agent.Logging;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Logging;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.BatchProcess;
using kCura.ScheduleQueue.Core.TimeMachine;
using Relativity.API;
using ITaskFactory = kCura.IntegrationPoints.Agent.Tasks.ITaskFactory;

namespace kCura.IntegrationPoints.Agent
{
	[Name(_AGENT_NAME)]
	[Guid(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID)]
	[Description("An agent that manages Integration Point jobs.")]
	public class Agent : ScheduleQueueAgentBase, IDisposable
	{
		private CreateErrorRdo _errorService;
		private IAPILog _logger;
		private ITaskFactory _taskFactory;
		private const string _AGENT_NAME = "Integration Points Agent";
		//private ITaskExceptionService _taskExceptionService;

		private ITaskFactory TaskFactory => _taskFactory ?? (_taskFactory = new TaskFactory(Helper));

		public Agent() : base(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID))
		{
			JobExecutionError += OnJobExecutionError;
			Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);

#if TIME_MACHINE
			AgentTimeMachineProvider.Current =
				new DefaultAgentTimeMachineProvider(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
#endif
		}

		public override string Name => _AGENT_NAME;

		private CreateErrorRdo ErrorService => _errorService ?? (_errorService = new CreateErrorRdo(new RsapiClientFactory(Helper), Helper, new SystemEventLoggingService()));

		protected override void Initialize()
		{
			base.Initialize();
			_logger = Helper.GetLoggerFactory().GetLogger().ForContext<Agent>();
		}

		public override ITask GetTask(Job job)
		{
			ITask task = TaskFactory.CreateTask(job, this);
			return task;
		}

		protected override void ReleaseTask(ITask task)
		{
			TaskFactory.Release(task);
		}

		protected override void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null)
		{
			if (exception != null)
			{
				details = details ?? string.Empty;
				details += Environment.NewLine;
				details += exception.Message + Environment.NewLine + exception.StackTrace;
			}

			_logger.LogInformation("Integration Points job status update: {@JobLogInformation}",
				new JobLogInformation() {Job = job, State = state, Details = details});
		}

		protected void OnJobExecutionError(Job job, ITask task, Exception exception)
		{
	        LogJobExecutionError(job, exception);
			LogJobState(job, JobLogState.Error, exception);
			var integrationPointsException = exception as IntegrationPointsException;
			if (integrationPointsException != null)
			{
				ErrorService.Execute(job, integrationPointsException);
			}
			else
			{
				ErrorService.Execute(job, exception, _AGENT_NAME);
			}
		}

		public void Dispose()
		{
		}


		private void LogJobExecutionError(Job job, Exception exception)
		{
			Logger.LogError(exception, "An error occured during execution of Job with ID: {JobID} in {TypeName}", job.JobId,
				nameof(ScheduleQueueAgentBase));
		}
	}
}