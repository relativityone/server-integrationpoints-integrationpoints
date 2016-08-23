using System;
using System.Data;
using Castle.MicroKernel.Registration;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Constants = kCura.IntegrationPoint.Tests.Core.Constants;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	[Category(Constants.INTEGRATION_CATEGORY)]
	public class SyncManagerTests : OtherProvidersTemplate
	{
		private SyncManager _syncManager;
		private IIntegrationPointService _integrationPointService;
		private IJobService _jobService;
		private IJobHistoryService _jobHistoryService;
		private ICaseServiceContext _caseServiceContext;
		private ISerializer _serializer;
		private IQueueDBContext _queueContext;
		private Relativity.Client.DTOs.Workspace _workspaceDto;
		private int _agentTypeId;
		private Query _integrationPointAgentsQuery;

		public SyncManagerTests() : base("SyncManagerTests")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_agentTypeId = IntegrationPoint.Tests.Core.Agent.GetAgentTypeByName("Integration Points Agent").ArtifactID;
			_integrationPointAgentsQuery = new Query()
			{
				Condition = $"'AgentTypeArtifactID' == {_agentTypeId}"
			};
			IntegrationPoint.Tests.Core.Agent.DisableAgents(_integrationPointAgentsQuery);
		}

		public override void SuiteTeardown()
		{
			IntegrationPoint.Tests.Core.Agent.EnableAgents(_integrationPointAgentsQuery);
			base.SuiteTeardown();
		}

		protected override void Install()
		{
			base.Install();
			Container.Register(Component.For<JobStatisticsService>().ImplementedBy<JobStatisticsService>().LifeStyle.Transient);
			Container.Register(Component.For<IOnBehalfOfUserClaimsPrincipalFactory>()
				.ImplementedBy<OnBehalfOfUserClaimsPrincipalFactory>()
				.LifestyleTransient());
			Container.Register(
				Component.For<IRSAPIService>()
					.Instance(new RSAPIService(Container.Resolve<IHelper>(), WorkspaceArtifactId))
					.LifestyleTransient());
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
		}

		public override void TestSetup()
		{
			_caseServiceContext = Container.Resolve<ICaseServiceContext>();
			IDataProviderFactory providerFactory = Container.Resolve<IDataProviderFactory>();
			IJobManager jobManager = Container.Resolve<IJobManager>();
			_jobService = Container.Resolve<IJobService>();
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_serializer = Container.Resolve<ISerializer>();
			IGuidService guidService = Container.Resolve<IGuidService>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			JobHistoryErrorService jobHistoryErrorService = Container.Resolve<JobHistoryErrorService>();
			IScheduleRuleFactory scheduleRuleFactory = new DefaultScheduleRuleFactory();
			IManagerFactory managerFactory = Container.Resolve<IManagerFactory>();
			IContextContainerFactory contextContainerFactory = Container.Resolve<IContextContainerFactory>();

			IJobStatusUpdater jobStatusUpdater = Container.Resolve<IJobStatusUpdater>();
			JobHistoryBatchUpdateStatus jobHistoryUpdater = new JobHistoryBatchUpdateStatus(jobStatusUpdater, _jobHistoryService,
				_jobService, _serializer);

			_syncManager = new SyncManager(_caseServiceContext,
				providerFactory,
				jobManager,
				_jobService,
				Helper,
				_integrationPointService,
				_serializer,
				guidService,
				_jobHistoryService,
				jobHistoryErrorService,
				scheduleRuleFactory,
				managerFactory,
				contextContainerFactory,
				new[] {jobHistoryUpdater}
				);

			_workspaceDto = IntegrationPoint.Tests.Core.Workspace.GetWorkspaceDto(WorkspaceArtifactId);
		}

		[Test]
		public void Ldap_RunJob_SingleJob()
		{
			// arrange
			IntegrationModel model = CreateDefaultLdapIntegrationModel("Ldap_RunJob_SingleJob");
			model = CreateOrUpdateIntegrationPoint(model); // create integration point

			_integrationPointService.RunIntegrationPoint(WorkspaceArtifactId, model.ArtifactID, 9); // run now
			//Job job = null;
			//try
			//{
			//	job = GetNextInQueue(new[] { _sourceWorkspaceDto.ResourcePoolID.Value }, model.ArtifactID); // pick up job

			//	TaskParameters parameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);

			//	// act
			//	Assert.IsNotNull(job, "There is no job to execute");
			//	_exportManager.Execute(job); // run the job

			//	// assert
			//	model = RefreshIntegrationModel(model);
			//	IJobHistoryService jobHistoryService = Container.Resolve<IJobHistoryService>();
			//	JobHistory history = jobHistoryService.GetRdo(parameters.BatchInstance);

			//	Assert.IsNotNull(model);
			//	Assert.IsFalse(model.HasErrors ?? false);
			//	Assert.AreEqual(0, history.ItemsWithErrors);
			//	Assert.AreEqual(JobStatusChoices.JobHistoryCompleted.Name, history.JobStatus.Name);
			//	Assert.IsFalse(model.HasErrors ?? false);
			//}
			//finally
			//{
			//	if (job != null)
			//	{
			//		_jobService.DeleteJob(job.JobId);
			//	}
			//}
		}

		[Test]
		public void Ldap_MultipleJobs_AgentDropsJob()
		{
			Job scheduledJob = null;
			Job runJob = null;

			try
			{
				// ARRANGE
				const int fakeAgentId = 78945;
				Scheduler scheduler = new Scheduler
				{
					EnableScheduler = true,
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
					StartDate = DateTime.MaxValue.ToString(),
					ScheduledTime = DateTime.Now.ToString()
				};
				IntegrationModel model = CreateDefaultLdapIntegrationModel("Ldap_RunJob_MultipleJobs_AgentDropsJob", scheduler);

				// creates integration point, it also creates a Scheduled job since the scheduler is not null
				model = CreateOrUpdateIntegrationPoint(model);
				int integrationPointId = model.ArtifactID;

				Guid batchInstance = Guid.NewGuid();
				CreateJobHistoryOnIntegrationPoint(integrationPointId, batchInstance);

				DataRow row = new CreateScheduledJob(_queueContext).Execute(
					WorkspaceArtifactId,
					integrationPointId,
					"SyncManager",
					DateTime.UtcNow,
					1,
					null,
					null,
					null,
					0,
					777,
					1,
					1);
				runJob = new Job(row);
				Assert.IsNotNull(runJob, "There is no Run job to execute");

				int lastScheduledJobId = GetLastScheduledJobId(WorkspaceArtifactId, integrationPointId);
				scheduledJob = _jobService.GetJob(lastScheduledJobId);
				
				// creates the first Run job, but agents are disabled at this point so it's not picked up yet
				//_integrationPointService.RunIntegrationPoint(WorkspaceArtifactId, integrationPointId, 9);

				// Run job should be next up in the queue
				//runJob = GetNextJobInScheduleQueue(new[] {_workspaceDto.ResourcePoolID.Value}, integrationPointId, 1);
				
				// ACT
				// assign the fake agent to the Scheduled job
				AssignJobToAgent(fakeAgentId, scheduledJob.JobId);

				// ASSERT
				string exceptionMessage =
					$"Unable to execute Integration Point job: There is already a job currently running. Job is re-scheduled for {scheduledJob.NextRunTime}.";
				// run the Scheduled job, expect exception
				AgentDropJobException ex = Assert.Throws<AgentDropJobException>(() => _syncManager.Execute(scheduledJob));
				Assert.That(exceptionMessage, Is.EqualTo(ex.Message));
				
				TaskParameters runJobParameters = _serializer.Deserialize<TaskParameters>(runJob.JobDetails);
				TaskParameters scheduledJobParameters = _serializer.Deserialize<TaskParameters>(scheduledJob.JobDetails);

				model = RefreshIntegrationModel(model);
				JobHistory scheduledJobhistory = _jobHistoryService.GetRdo(scheduledJobParameters.BatchInstance);
				JobHistory history = _jobHistoryService.GetRdo(runJobParameters.BatchInstance);

				//Assert.IsNotNull(history); // job history is deleted
				//Assert.IsNotNull(model); // ip object does not get deleted
				//Assert.IsFalse(model.HasErrors ?? false); // expect the error being logged
			}
			finally
			{
				if (scheduledJob != null)
				{
					_jobService.DeleteJob(scheduledJob.JobId);
				}
				if (runJob != null)
				{
					_jobService.DeleteJob(runJob.JobId);
				}
			}
		}
	}
}
