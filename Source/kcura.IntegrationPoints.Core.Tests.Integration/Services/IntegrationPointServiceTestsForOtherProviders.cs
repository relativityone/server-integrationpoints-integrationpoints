using System;
using System.Data;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class IntegrationPointServiceTestsForOtherProviders : OtherProvidersTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private IQueueDBContext _queueContext;
		private IJobService _jobService;

		public IntegrationPointServiceTestsForOtherProviders() : base("IntegrationPointServiceTestsForOtherProviders")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			IntegrationPoint.Tests.Core.Agent.EnableAllIntegrationPointsAgentsAsync().GetAwaiter().GetResult();

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_jobService = Container.Resolve<IJobService>();
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
		}

		[IdentifiedTest("a623e9b8-dd04-4f0e-9359-ce7afdad1d5c")]
		public void Ldap_MultipleJobsInQueue_ThrowsJobsAlreadyRunning()
		{
			Job fakeJob = null;

			try
			{
				// ARRANGE
				IntegrationPointModel model = CreateDefaultLdapIntegrationModel("Ldap_MultipleJobsInQueue_ThrowsJobsAlreadyRunning");
				model = CreateOrUpdateIntegrationPoint(model);
				int integrationPointId = model.ArtifactID;

				// creates a Run job and inserts into the queue
				Guid batchInstance = Guid.NewGuid();
				string jobDetails = $@"{{""BatchInstance"":""{batchInstance}"",""BatchParameters"":null}}";
				CreateJobHistoryOnIntegrationPoint(integrationPointId, batchInstance, JobTypeChoices.JobHistoryRun);

				DataRow row;
				using (DataTable dataTable = new CreateScheduledJob(_queueContext).Execute(
					workspaceID: WorkspaceArtifactId,
					relatedObjectArtifactID: integrationPointId,
					taskType: "SyncManager",
					nextRunTime: DateTime.UtcNow,
					AgentTypeID: 1,
					scheduleRuleType: null,
					serializedScheduleRule: null,
					jobDetails: jobDetails,
					jobFlags: 0,
					SubmittedBy: 777,
					rootJobID: 1,
					parentJobID: 1))
				{
					row = dataTable.Rows[0];
				}

				fakeJob = new Job(row);

				// ACT & ASSERT
				Exception ex = Assert.Throws<Exception>(
						() => _integrationPointService.RunIntegrationPoint(WorkspaceArtifactId, integrationPointId, 9));
				Assert.That(Constants.IntegrationPoints.JOBS_ALREADY_RUNNING, Is.EqualTo(ex.Message));
			}
			finally
			{
				if (fakeJob != null)
				{
					_jobService.DeleteJob(fakeJob.JobId);
				}
			}
		}
	}
}
