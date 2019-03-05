using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using System;
using System.Data;
using System.Globalization;

namespace kCura.IntegrationPoints.Agent.Tests.Integration
{
	[TestFixture]
	public class TaskFactoryTestsForOtherProviders : OtherProvidersTemplate
	{
		private IJobService _jobService;
		private IQueueDBContext _queueContext;
		private TestingAgent _agent;
		private IJobHistoryService _jobHistoryService;


		public TaskFactoryTestsForOtherProviders() : base("TaskFactoryTestsForOtherProviders")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			IntegrationPoint.Tests.Core.Agent.DisableAllAgents();
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
		}

		public override void SuiteTeardown()
		{
			IntegrationPoint.Tests.Core.Agent.EnableAllAgents();
			base.SuiteTeardown();
		}

		public override void TestSetup()
		{
			_jobService = Container.Resolve<IJobService>();
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_agent = new TestingAgent
			{
				Helper = new ExtendedIAgentHelper(Helper)
			};
		}

		[Test]
		[SmokeTest]
		public void Ldap_MultipleJobs_AgentDropsJob_RunJob()
		{
			Job job1 = null;
			Job job2 = null;
			try
			{
				// ARRANGE
				var FutureDate = DateTime.Now.AddYears(10);
				IntegrationPointModel model = CreateDefaultLdapIntegrationModel("Ldap_MultipleJobs_AgentDropsJob");
				model = CreateOrUpdateIntegrationPoint(model); // create integration point

				Guid batchInstance = Guid.NewGuid();
				string jobDetails = $@"{{""BatchInstance"":""{batchInstance}"",""BatchParameters"":null}}";
				CreateJobHistoryOnIntegrationPoint(model.ArtifactID, batchInstance, JobTypeChoices.JobHistoryRun);
				using (DataTable dataTable = new CreateScheduledJob(_queueContext).Execute(
					workspaceID: WorkspaceArtifactId,
					relatedObjectArtifactID: model.ArtifactID,
					taskType: "SyncManager",
					nextRunTime: FutureDate,
					AgentTypeID: 1,
					scheduleRuleType: null,
					serializedScheduleRule: null,
					jobDetails: jobDetails,
					jobFlags: 0,
					SubmittedBy: 777,
					rootJobID: 1,
					parentJobID: 1))
				{
					job1 = new Job(dataTable.Rows[0]);
				}

				// inserts a job entry to the ScheduleQueue table that is locked by the disabled agent
				job2 = JobExtensions.Execute(
					qDBContext: _queueContext,
					workspaceID: WorkspaceArtifactId,
					relatedObjectArtifactID: model.ArtifactID,
					taskType: "SyncManager",
					nextRunTime: FutureDate,
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
				AgentDropJobException ex = Assert.Throws<AgentDropJobException>(() => _agent.GetTask(job1));
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
				var FutureDate = DateTime.Now.AddYears(10);
				Scheduler scheduler = new Scheduler
				{
					EnableScheduler = true,
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
					StartDate = FutureDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					ScheduledTime = DateTime.Now.ToString("hh:mm")
				};
				IntegrationPointModel model = CreateDefaultLdapIntegrationModel("Ldap_MultipleJobs_AgentDropsJob_ScheduledJob", scheduler);
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
					nextRunTime: FutureDate.AddDays(-1),
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
				AgentDropJobException ex = Assert.Throws<AgentDropJobException>(() => _agent.GetTask(job1));
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

		[Test]
		[SmokeTest]
		public void ItShouldSetJobIdOnJobHistory()
		{
			Job job1 = null;
			try
			{
				// ARRANGE
				var FutureDate = DateTime.Now.AddYears(10);
				IntegrationPointModel model = CreateDefaultLdapIntegrationModel("Ldap_MultipleJobs_AgentDropsJob");
				model = CreateOrUpdateIntegrationPoint(model); // create integration point

				Guid batchInstance = Guid.NewGuid();
				string jobDetails = $@"{{""BatchInstance"":""{batchInstance}"",""BatchParameters"":null}}";
				CreateJobHistoryOnIntegrationPoint(model.ArtifactID, batchInstance, JobTypeChoices.JobHistoryRun);
				using (DataTable dataTable = new CreateScheduledJob(_queueContext).Execute(
					workspaceID: WorkspaceArtifactId,
					relatedObjectArtifactID: model.ArtifactID,
					taskType: "SyncManager",
					nextRunTime: FutureDate,
					AgentTypeID: 1,
					scheduleRuleType: null,
					serializedScheduleRule: null,
					jobDetails: jobDetails,
					jobFlags: 0,
					SubmittedBy: 777,
					rootJobID: 1,
					parentJobID: 1))
				{
					job1 = new Job(dataTable.Rows[0]);
				}


				// ACT & ASSERT
				_agent.GetTask(job1);

				JobHistory jobHistory = _jobHistoryService.GetRdo(batchInstance);
				Assert.AreEqual(job1.JobId.ToString(), jobHistory.JobID);
			}
			finally
			{
				if (job1 != null)
				{
					_jobService.DeleteJob(job1.JobId);
				}
			}
		}
	}
}
