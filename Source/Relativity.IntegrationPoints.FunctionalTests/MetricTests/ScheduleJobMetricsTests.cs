using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Telemetry;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Moq;
using NUnit.Framework;
using Relativity.Services.ChoiceQueryManager.Models;
using Relativity.Telemetry.Services.Metrics;
using Relativity.Testing.Identification;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.FunctionalTests.MetricTests
{
	[TestFixture]
	public class ScheduleJobMetricsTests : RelativityProviderTemplate
	{
		private IIntegrationPointService _integrationPointService;
		private IJobService _jobService;

		private Mock<IMetricsManager> _metricsManagerMock;

		public ScheduleJobMetricsTests() : base("IntegrationPointScheduler Metrics", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			Agent.EnableAllIntegrationPointsAgentsAsync().GetAwaiter().GetResult();

			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_jobService = Container.Resolve<IJobService>();

			_metricsManagerMock = new Mock<IMetricsManager>();
			_metricsManagerMock.Setup(x => x.LogCountAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>()))
				.Returns(Task.CompletedTask);

			Helper.InjectProxy<IMetricsManager>(_metricsManagerMock.Object);
		}

		[IdentifiedTest("193589A4-B5A4-4ED3-9F8D-3AC6E8DF43B0")]
		[Category("Test")]
		public void CreateAndRunIntegrationPoint_ShouldSendScheduleMetrics_WhenScheduleIsValid()
		{
			//Arrange

			DateTime utcNow = DateTime.UtcNow;
			const int schedulerRunTimeDelayMinutes = 2;

			IntegrationPointModel integrationModel = new IntegrationPointModel
			{
				Destination = CreateDestinationConfig(ImportOverwriteModeEnum.AppendOnly),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = $"IntegrationPointServiceTest{DateTime.Now:yy-MM-dd HH-mm-ss}",
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = true,
					StartDate = utcNow.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					EndDate = utcNow.AddDays(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
					ScheduledTime = utcNow.AddMinutes(schedulerRunTimeDelayMinutes).ToString("HH:mm"),
					SelectedFrequency = ScheduleInterval.Daily.ToString(),
					TimeZoneId = TimeZoneInfo.Utc.Id
				},
				Map = CreateDefaultFieldMap(),
				Type = Container.Resolve<IIntegrationPointTypeService>().GetIntegrationPointType(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ExportGuid).ArtifactId
			};

			IntegrationPointModel integrationPointPreJobExecution = CreateOrUpdateIntegrationPoint(integrationModel);

			//Act
			Status.WaitForScheduledJobToComplete(Container, SourceWorkspaceArtifactID, integrationPointPreJobExecution.ArtifactID, timeoutInSeconds: 600);
			IntegrationPointModel integrationPointPostRun = _integrationPointService.ReadIntegrationPointModel(integrationPointPreJobExecution.ArtifactID);

			//Assert
			Assert.AreEqual(null, integrationPointPreJobExecution.LastRun);
			Assert.AreEqual(false, integrationPointPreJobExecution.HasErrors);
			Assert.AreEqual(false, integrationPointPostRun.HasErrors);
			Assert.IsNotNull(integrationPointPostRun.LastRun);
			Assert.IsNotNull(integrationPointPostRun.NextRun);

			// Assert schedule metrics
			AssertMetrics();

			//We need to clean ScheduleAgentQueue table because this Integration Point is scheduled and RIP Application and workspace 
			//will be deleted after test is finished
			CleanScheduleAgentQueueFromAllRipJobs(integrationPointPreJobExecution.ArtifactID);
		}

		private void AssertMetrics()
		{
			try
			{
				_metricsManagerMock.Verify(x => x.LogCountAsync(
					It.IsAny<string>(),
					It.IsAny<Guid>(),
					It.IsAny<string>(),
					It.IsAny<long>()));
			}
			catch(Exception)
			{
				throw;
			}


			//_metricsManagerMock.Verify(x => x.LogCountAsync(
			//	It.Is<string>(m => m.StartsWith("Relativity.Sync.Schedule.JobStarted")),
			//	It.IsAny<Guid>(),
			//	It.Is<string>(m => m.StartsWith("Sync_SavedSearch_")),
			//	It.IsAny<long>()));

			//_metricsManagerMock.Verify(x => x.LogCountAsync(
			//	It.Is<string>(m => m.StartsWith("Relativity.Sync.Schedule.JobCompleted")),
			//	It.IsAny<Guid>(),
			//	It.Is<string>(m => m.StartsWith("Sync_SavedSearch_")),
			//	It.IsAny<long>()));
		}

		private void CleanScheduleAgentQueueFromAllRipJobs(int integrationPointArtifactId)
		{
			IList<Job> jobs = _jobService.GetJobs(integrationPointArtifactId);
			foreach (Job job in jobs)
			{
				_jobService.DeleteJob(job.JobId);
			}
		}
	}
}
