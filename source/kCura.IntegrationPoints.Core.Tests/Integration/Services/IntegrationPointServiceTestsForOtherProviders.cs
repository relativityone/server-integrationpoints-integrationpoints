using System;
using System.Data;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.Data.Queries;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Category("Integration Tests")]
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
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_jobService = Container.Resolve<IJobService>();
			_queueContext = new QueueDBContext(Helper, GlobalConst.SCHEDULE_AGENT_QUEUE_TABLE_NAME);
		}
		
		[Test]
		public void Ldap_MultipleJobsInQueue_ThrowsJobsAlreadyRunning()
		{
			Job fakeJob = null;

			try
			{
				// ARRANGE
				IntegrationModel model = CreateDefaultLdapIntegrationModel("Ldap_MultipleJobsInQueue_ThrowsJobsAlreadyRunning");
				model = CreateOrUpdateIntegrationPoint(model);
				int integrationPointId = model.ArtifactID;

				// creates a Run job and inserts into the queue
				Guid batchInstance = Guid.NewGuid();
				string jobDetails = $@"{{""BatchInstance"":""{batchInstance}"",""BatchParameters"":null}}";
				CreateJobHistoryOnIntegrationPoint(integrationPointId, batchInstance);

				DataRow row = new CreateScheduledJob(_queueContext).Execute(
					WorkspaceArtifactId,
					integrationPointId,
					"SyncManager",
					DateTime.UtcNow,
					1,
					null,
					null,
					jobDetails,
					0,
					777,
					1,
					1);
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
