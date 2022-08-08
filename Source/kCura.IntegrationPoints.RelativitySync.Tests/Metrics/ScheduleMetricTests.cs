using kCura.IntegrationPoints.RelativitySync.Metrics;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Telemetry;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Metrics
{
    [TestFixture, Category("Unit")]
    public class ScheduleMetricTests
    {
        private Mock<IServicesMgr> _servicesMgrFake;
        private Mock<IMetricsManager> _metricsManagerMock;

        private Mock<IScheduleRule> _scheduleRuleFake;

        [SetUp]
        public void SetUp()
        {
            _metricsManagerMock = new Mock<IMetricsManager>();
            _metricsManagerMock
                .Setup(x => x.LogCountAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Task.CompletedTask);

            _servicesMgrFake = new Mock<IServicesMgr>();
            _servicesMgrFake.Setup(x => x.CreateProxy<IMetricsManager>(ExecutionIdentity.System))
                .Returns(_metricsManagerMock.Object);

            _scheduleRuleFake = new Mock<IScheduleRule>();
        }

        [Test]
        public async Task CreateScheduleJobStartedMetric_ShouldSendDailyMetric()
        {
            // Arrange
            DateTime dailyScheduleTime = new DateTime(2000, 1, 1, 13, 0, 0);
            _scheduleRuleFake.Setup(x => x.GetNextUTCRunDateTime()).Returns(dailyScheduleTime);

            const int jobId = 1;
            const int integrationPointId = 2;

            // Act
            var sut = ScheduleMetric.CreateScheduleJobStarted(_servicesMgrFake.Object, integrationPointId, jobId,
                SourceConfiguration.ExportType.SavedSearch, _scheduleRuleFake.Object);

            await sut.SendAsync().ConfigureAwait(false);

            // Assert
            _metricsManagerMock.Verify(x => x.LogCountAsync(
                MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_STARTED_DAILY,
                Guid.Empty,
                $"Sync_SavedSearch_{integrationPointId}_{jobId}",
                1), Times.Once);
        }

        [Test]
        public async Task CreateScheduleJobStartedMetric_ShouldSendNightlyMetric()
        {
            // Arrange
            DateTime dailyScheduleTime = new DateTime(2000, 1, 1, 21, 0, 0);
            _scheduleRuleFake.Setup(x => x.GetNextUTCRunDateTime()).Returns(dailyScheduleTime);

            const int jobId = 1;
            const int integrationPointId = 2;

            // Act
            var sut = ScheduleMetric.CreateScheduleJobStarted(_servicesMgrFake.Object, integrationPointId, jobId,
                SourceConfiguration.ExportType.SavedSearch, _scheduleRuleFake.Object);

            await sut.SendAsync().ConfigureAwait(false);

            // Assert
            _metricsManagerMock.Verify(x => x.LogCountAsync(
                MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_STARTED_NIGHTLY,
                Guid.Empty,
                $"Sync_SavedSearch_{integrationPointId}_{jobId}",
                1), Times.Once);
        }

        [Test]
        public async Task CreateScheduleJobCompletedMetric_ShouldSendMetric()
        {
            // Arrange
            const int jobId = 1;
            const int integrationPointId = 2;

            // Act
            var sut = ScheduleMetric.CreateScheduleJobCompleted(_servicesMgrFake.Object, integrationPointId, jobId,
                SourceConfiguration.ExportType.SavedSearch);

            await sut.SendAsync().ConfigureAwait(false);

            // Assert
            _metricsManagerMock.Verify(x => x.LogCountAsync(
                MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_COMPLETED,
                Guid.Empty,
                $"Sync_SavedSearch_{integrationPointId}_{jobId}",
                1), Times.Once);
        }

        [Test]
        public async Task CreateScheduleJobFailedMetric_ShouldSendMetric()
        {
            // Arrange
            const int jobId = 1;
            const int integrationPointId = 2;

            // Act
            var sut = ScheduleMetric.CreateScheduleJobFailed(_servicesMgrFake.Object, integrationPointId, jobId,
                SourceConfiguration.ExportType.SavedSearch);

            await sut.SendAsync().ConfigureAwait(false);

            // Assert
            _metricsManagerMock.Verify(x => x.LogCountAsync(
                MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_FAILED,
                Guid.Empty,
                $"Sync_SavedSearch_{integrationPointId}_{jobId}",
                1), Times.Once);
        }
    }
}
