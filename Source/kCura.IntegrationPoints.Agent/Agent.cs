using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using kCura.Agent.CustomAttributes;
using kCura.Apps.Common.Data;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Logging;
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
		private const string _AGENT_NAME = "Integration Points Agent";

		private IRSAPIClient _eddsRsapiClient;
		private ITaskFactory _taskFactory;
		private CreateErrorRdo _errorService;
	    private IAPILog _logger;
		private IRSAPIClient EddsRsapiClient => _eddsRsapiClient ??
												(_eddsRsapiClient = new RsapiClientFactory(Helper).CreateAdminClient(-1));

		private ITaskFactory TaskFactory => _taskFactory ?? (_taskFactory = new TaskFactory(Helper));

		public Agent() : base(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID))
		{
			RaiseException += RaiseJobException;
			RaiseJobLogEntry += RaiseJobLog;
			Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);

#if TIME_MACHINE
			AgentTimeMachineProvider.Current =
				new DefaultAgentTimeMachineProvider(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
#endif
		}

		public override string Name => _AGENT_NAME;

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

		private void RaiseJobException(Job job, Exception exception)
		{
			if (_errorService == null)
			{
				_errorService = new CreateErrorRdo(EddsRsapiClient, Helper);
			}
			_errorService.Execute(job, exception, _AGENT_NAME);
		}

		private void RaiseJobLog(Job job, JobLogState state, string details = null)
		{
			_logger.LogInformation("Integration Points job status update: {@JobLogInformation}", new JobLogInformation() { Job = job, State = state, Details = details });
		}

		public void Dispose()
		{
		}
	}
}