using System;
using System.Data;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using ITaskFactory = kCura.IntegrationPoints.Agent.Tasks.ITaskFactory;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	public class TaskFactoryTestsForOtherProviders : OtherProvidersTemplate
	{
		private IJobService _jobService;
		private IQueueDBContext _queueContext;
		private ScheduleQueueAgentBase _agent;
		private IJobHistoryService _jobHistoryService;
		private ITaskFactory _taskFactory;

		public TaskFactoryTestsForOtherProviders() : base("TaskFactoryTestsForOtherProviders")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			ControlIntegrationPointAgents(false);
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
		}

		public override void SuiteTeardown()
		{
			ControlIntegrationPointAgents(true);
			base.SuiteTeardown();
		}

		public override void TestSetup()
		{
			_jobService = Container.Resolve<IJobService>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_agent = new Agent();
			_taskFactory = new TaskFactory(new ExtendedIAgentHelper(Helper));
		}

		[Test]
		public void Ldap_MultipleJobs_AgentDropsJob_RunJob()
		{
			Job job1 = null;
			Job job2 = null;
			try
			{
				// ARRANGE
				IntegrationModel model = CreateDefaultLdapIntegrationModel("Ldap_MultipleJobs_AgentDropsJob");
				model = CreateOrUpdateIntegrationPoint(model); // create integration point

				Guid batchInstance = Guid.NewGuid();
				string jobDetails = $@"{{""BatchInstance"":""{batchInstance}"",""BatchParameters"":null}}";
				CreateJobHistoryOnIntegrationPoint(model.ArtifactID, batchInstance, JobTypeChoices.JobHistoryRun);

				DataRow row1 = new CreateScheduledJob(_queueContext).Execute(
					workspaceID: WorkspaceArtifactId,
					relatedObjectArtifactID: model.ArtifactID,
					taskType: "SyncManager",
					nextRunTime: DateTime.MaxValue,
					AgentTypeID: 1,
					scheduleRuleType: null,
					serializedScheduleRule: null,
					jobDetails: jobDetails,
					jobFlags: 0,
					SubmittedBy: 777,
					rootJobID: 1,
					parentJobID: 1);
				job1 = new Job(row1);

				// inserts a job entry to the ScheduleQueue table that is locked by the disabled agent
				job2 = JobExtensions.Execute(
					qDBContext: _queueContext,
					workspaceID: WorkspaceArtifactId,
					relatedObjectArtifactID: model.ArtifactID,
					taskType: "SyncManager",
					nextRunTime: DateTime.MaxValue,
					AgentTypeID: 1,
					scheduleRuleType: null,
					serializedScheduleRule: null,
					jobDetails: jobDetails,
					jobFlags: 0,
					SubmittedBy: 777,
					locked: AgentArtifactId,
					rootJobID: 1,
					parentJobID: 1);

				// ACT & ASSERT
				string exceptionMessage =
					"Unable to execute Integration Point job: There is already a job currently running.";
				AgentDropJobException ex = Assert.Throws<AgentDropJobException>(() => _taskFactory.CreateTask(job1, _agent));
				Assert.That(exceptionMessage, Is.EqualTo(ex.Message));

				JobHistory jobHistory = _jobHistoryService.GetRdo(batchInstance);
				Assert.IsNull(jobHistory);
			}
			finally
			{
				if (job1 != null)
				{
					_jobService.DeleteJob(job1.JobId);
				}
				if (job2 != null)
				{
					_jobService.DeleteJob(job2.JobId);
				}
			}
		}

		[Test]
		public void Ldap_MultipleJobs_AgentDropsJob_ScheduledJob()
		{
			Job job1 = null;
			Job job2 = null;
			try
			{
				// ARRANGE
				Scheduler scheduler = new Scheduler
				{
					EnableScheduler = true,
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
					StartDate = DateTime.MaxValue.ToString(),
					ScheduledTime = DateTime.Now.ToString()
				};
				IntegrationModel model = CreateDefaultLdapIntegrationModel("Ldap_MultipleJobs_AgentDropsJob_ScheduledJob", scheduler);
				model = CreateOrUpdateIntegrationPoint(model); // create integration point

				int lastScheduledJobId = GetLastScheduledJobId(WorkspaceArtifactId, model.ArtifactID);
				job1 = _jobService.GetJob(lastScheduledJobId);

				Guid batchInstance = Guid.NewGuid();
				string jobDetails = $@"{{""BatchInstance"":""{batchInstance}"",""BatchParameters"":null}}";

				// inserts a job entry to the ScheduleQueue table that is locked by the disabled agent
				job2 = JobExtensions.Execute(
					qDBContext: _queueContext,
					workspaceID: WorkspaceArtifactId,
					relatedObjectArtifactID: model.ArtifactID,
					taskType: "SyncManager",
					nextRunTime: DateTime.MaxValue.AddDays(-1),
					AgentTypeID: 1,
					scheduleRuleType: null,
					serializedScheduleRule: null,
					jobDetails: jobDetails,
					jobFlags: 0,
					SubmittedBy: 777,
					locked: AgentArtifactId,
					rootJobID: 1,
					parentJobID: 1);

				// ACT & ASSERT
				AgentDropJobException ex = Assert.Throws<AgentDropJobException>(() => _taskFactory.CreateTask(job1, _agent));
				string exceptionMessage =
					"Unable to execute Integration Point job: There is already a job currently running. Job is re-scheduled for";
				StringAssert.Contains(exceptionMessage, ex.Message);

				Job scheduledJobStillExists = _jobService.GetJob(job1.JobId);
				Assert.IsNotNull(scheduledJobStillExists.NextRunTime);
			}
			finally
			{
				if (job1 != null)
				{
					_jobService.DeleteJob(job1.JobId);
				}
				if (job2 != null)
				{
					_jobService.DeleteJob(job2.JobId);
				}
			}
		}
	}
}
