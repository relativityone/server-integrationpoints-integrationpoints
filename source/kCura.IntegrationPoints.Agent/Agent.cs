using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using kCura.Agent.CustomAttributes;
using kCura.Apps.Common.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Logging;
using kCura.ScheduleQueue.Core.Services;
using kCura.ScheduleQueue.Core.TimeMachine;
using Relativity.API;
using CreateErrorRdo = kCura.ScheduleQueue.Core.Logging.CreateErrorRdo;
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

		private IRSAPIClient EddsRsapiClient
		{
			get
			{
				if (_eddsRsapiClient == null)
				{
					_eddsRsapiClient = new RsapiClientFactory(Helper).CreateClientForWorkspace(-1, ExecutionIdentity.System);
				}
				return _eddsRsapiClient;
			}
		}

		private ITaskFactory TaskFactory => _taskFactory ?? (_taskFactory = new TaskFactory(Helper));

		public Agent() : base(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID))
		{
			base.RaiseException += RaiseException;
			RaiseJobLogEntry += RaiseJobLog;
			Apps.Common.Config.Manager.Settings.Factory = new HelperConfigSqlServiceFactory(Helper);

#if TIME_MACHINE
			AgentTimeMachineProvider.Current = new DefaultAgentTimeMachineProvider(Guid.Parse(GlobalConst.RELATIVITY_INTEGRATION_POINTS_AGENT_GUID));
#endif
		}

		public override string Name => _AGENT_NAME;

		public override ITask GetTask(Job job)
		{
			try
			{
				return TaskFactory.CreateTask(job, this);
			}
			catch (Exception e)
			{
				UpdateJobHistoryOnFailure(job, e);
				throw;
			}
		}

		protected override void ReleaseTask(ITask task)
		{
			TaskFactory.Release(task);
		}

		private new void RaiseException(Job job, Exception exception)
		{
			if (_errorService == null)
			{
				_errorService = new CreateErrorRdo(EddsRsapiClient);
			}
			_errorService.Execute(job, exception, _AGENT_NAME);
		}

		private void RaiseJobLog(Job job, JobLogState state, string details = null)
		{
			var jobLogService = new JobLogService(Helper);
			jobLogService.Log(AgentService.AgentTypeInformation, job, state, details);
		}

		public void Dispose() { }

		private void UpdateJobHistoryOnFailure(Job job, Exception e)
		{
			ISerializer serializer = new JSONSerializer();

			TaskParameters taskParameters = serializer.Deserialize<TaskParameters>(job.JobDetails);
			int workspaceArtifactId = job.WorkspaceID;
			
			RsapiClientFactory rsapiClientFactory = new RsapiClientFactory(Helper);
			IServiceContextHelper serviceContextHelper = new ServiceContextHelperForAgent(Helper, workspaceArtifactId, rsapiClientFactory);
			ICaseServiceContext caseServiceContext = new CaseServiceContext(serviceContextHelper);

			IRSAPIClient rsapiClient = rsapiClientFactory.CreateClientForWorkspace(workspaceArtifactId, ExecutionIdentity.System);
			IWorkspaceRepository workspaceRepository = new RsapiWorkspaceRepository(rsapiClient);
			ChoiceQuery choiceQuery = new ChoiceQuery(rsapiClient);

			IDBContext dbContext = Helper.GetDBContext(workspaceArtifactId);
			IWorkspaceDBContext workspaceDbContext = new WorkspaceContext(dbContext);
			JobResoureTracker jobResourceTracker = new JobResoureTracker(workspaceDbContext);
			JobTracker jobTracker = new JobTracker(jobResourceTracker);
			
			IEddsServiceContext eddsServiceContext = new EddsServiceContext(serviceContextHelper);
			
			IJobService jobService = new JobService(AgentService, Helper);
			IJobManager jobmanager = new AgentJobManager(eddsServiceContext, jobService, serializer, jobTracker);

			IntegrationPointService integrationPointService = new IntegrationPointService(caseServiceContext, serializer, choiceQuery, jobmanager);
			IntegrationPoint integrationPoint = integrationPointService.GetRdo(job.RelatedObjectArtifactID);

			JobHistoryService jobHistoryService = new JobHistoryService(caseServiceContext, workspaceRepository);
			JobHistory jobHistory = jobHistoryService.GetRdo(taskParameters.BatchInstance);

			JobHistoryErrorService jobHistoryErrorService = new JobHistoryErrorService(caseServiceContext)
			{
				IntegrationPoint = integrationPoint,
				JobHistory = jobHistory
			};

			jobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, e);
			jobHistoryErrorService.CommitErrors();

			jobHistory.Status = JobStatusChoices.JobHistoryErrorJobFailed;
			jobHistoryService.UpdateRdo(jobHistory);

			// No updates to IP since the job history error service handles IP updates
		}
	}
}
