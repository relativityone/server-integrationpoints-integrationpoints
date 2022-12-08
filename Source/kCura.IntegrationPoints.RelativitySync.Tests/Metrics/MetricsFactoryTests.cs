using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Telemetry;
using kCura.IntegrationPoints.RelativitySync.Metrics;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Telemetry.Services.Metrics;
using System;
using System.IO.Packaging;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Metrics
{
    [TestFixture, Category("Unit")]
    public class MetricsFactoryTests
    {
        private IMetricsFactory _sut;

        private Mock<ISerializer> _serializerFake;
        private Mock<IIntegrationPointService> _integrationPointServiceFake;
        private Mock<IScheduleRuleFactory> _scheduleRuleFactoryFake;
        private Mock<IServicesMgr> _servicesMgrFake;

        private Mock<IMetricsManager> _metricsManagerMock;

        private Mock<IScheduleRule> _scheduleRuleFake;

        private readonly IntegrationPointDto _integrationPoint = new IntegrationPointDto
        {
            ArtifactId = 100,
            SourceConfiguration = string.Empty
        };

        [SetUp]
        public void SetUp()
        {
            _serializerFake = new Mock<ISerializer>();
            _serializerFake.Setup(x => x.Deserialize<SourceConfiguration>(It.IsAny<string>()))
                .Returns(new SourceConfiguration
                {
                    TypeOfExport = SourceConfiguration.ExportType.SavedSearch
                });

            _integrationPointServiceFake = new Mock<IIntegrationPointService>();
            _integrationPointServiceFake.Setup(x => x.Read(It.IsAny<int>()))
                .Returns(_integrationPoint);

            _scheduleRuleFactoryFake = new Mock<IScheduleRuleFactory>();

            _metricsManagerMock = new Mock<IMetricsManager>();
            _metricsManagerMock
                .Setup(x => x.LogCountAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Task.CompletedTask);

            _servicesMgrFake = new Mock<IServicesMgr>();
            _servicesMgrFake.Setup(x => x.CreateProxy<IMetricsManager>(ExecutionIdentity.System))
                .Returns(_metricsManagerMock.Object);

            _scheduleRuleFake = new Mock<IScheduleRule>();

            _sut = new MetricsFactory(_serializerFake.Object, _scheduleRuleFactoryFake.Object,
                _integrationPointServiceFake.Object, _servicesMgrFake.Object);
        }

        [Test]
        public void CreateScheduleJobStartedMetrics_ShouldReturnEmptyMetric_WhenScheduleRuleIsNotSet()
        {
            // Arrange
            Job job = PrepareNonScheduledJob();

            // Act
            var metric = _sut.CreateScheduleJobStartedMetric(job);

            // Assert
            VerifyEmptyMetric(metric);
        }

        [Test]
        public void CreateScheduleJobCompletedMetric_ShouldReturnEmptyMetric_WhenScheduleRuleIsNotSet()
        {
            // Arrange
            Job job = PrepareNonScheduledJob();

            // Act
            var metric = _sut.CreateScheduleJobCompletedMetric(job);

            // Assert
            VerifyEmptyMetric(metric);
        }

        [Test]
        public void CreateScheduleJobFailedMetric_ShouldReturnEmptyMetric_WhenScheduleRuleIsNotSet()
        {
            // Arrange
            Job job = PrepareNonScheduledJob();

            // Act
            var metric = _sut.CreateScheduleJobFailedMetric(job);

            // Assert
            VerifyEmptyMetric(metric);
        }

        [Test]
        public void CreateScheduleJobStartedMetric_ShouldReturnDailyMetric_WhenScheduleRuleIsSet()
        {
            // Arrange
            const int jobId = 1;
            DateTime dailyScheduleTime = new DateTime(2000, 1, 1, 13, 0, 0);

            Job job = PrepareScheduleJob(jobId, dailyScheduleTime);

            // Act
            var metric = _sut.CreateScheduleJobStartedMetric(job);

            // Assert
            VerifyScheduleMetric(jobId, metric, MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_STARTED_DAILY);
        }

        [Test]
        public void CreateScheduleJobStartedMetric_ShouldReturnNightlyMetric_WhenScheduleRuleIsSet()
        {
            // Arrange
            const int jobId = 1;
            DateTime dailyScheduleTime = new DateTime(2000, 1, 1, 21, 0, 0);

            Job job = PrepareScheduleJob(jobId, dailyScheduleTime);

            // Act
            var metric = _sut.CreateScheduleJobStartedMetric(job);

            // Assert
            VerifyScheduleMetric(jobId, metric, MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_STARTED_NIGHTLY);
        }

        [Test]
        public void CreateScheduleJobCompletedMetric_ShouldReturnMetric_WhenScheduleRuleIsSet()
        {
            // Arrange
            const int jobId = 1;

            Job job = PrepareScheduleJob(jobId);

            // Act
            var metric = _sut.CreateScheduleJobCompletedMetric(job);

            // Assert
            VerifyScheduleMetric(jobId, metric, MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_COMPLETED);
        }

        [Test]
        public void CreateScheduleJobFailedMetric_ShouldReturnMetric_WhenScheduleRuleIsSet()
        {
            // Arrange
            const int jobId = 1;

            Job job = PrepareScheduleJob(jobId);

            // Act
            var metric = _sut.CreateScheduleJobFailedMetric(job);

            // Assert
            VerifyScheduleMetric(jobId, metric, MetricsBucket.SyncSchedule.SCHEDULE_SYNC_JOB_FAILED);
        }

        #region Helpers

        private Job PrepareNonScheduledJob()
        {
            Job job = JobHelper.GetJob(0, 0, null, 0, 0, 0, 0, TaskType.None, DateTime.MinValue,
                null, null, 0, DateTime.MinValue, 0, null, null);

            _scheduleRuleFactoryFake.Setup(x => x.Deserialize(It.IsAny<Job>()))
                .Returns<IScheduleRule>(null);

            return job;
        }

        private Job PrepareScheduleJob(int jobId, DateTime scheduleDateTime = default(DateTime))
        {
            Job job = JobHelper.GetJob(jobId, 0, null, 0, 0, 0, 0, TaskType.None, DateTime.MinValue,
                null, null, 0, DateTime.MinValue, 0, null, null);

            _scheduleRuleFactoryFake.Setup(x => x.Deserialize(It.IsAny<Job>()))
                .Returns(_scheduleRuleFake.Object);

            _scheduleRuleFake.Setup(x => x.GetNextUTCRunDateTime()).Returns(scheduleDateTime);

            return job;
        }

        private void VerifyScheduleMetric(int jobId, IMetric metric, string expectedBucket)
        {
            metric.SendAsync().GetAwaiter().GetResult();

            metric.Should().BeOfType(typeof(ScheduleMetric));

            _metricsManagerMock.Verify(x => x.LogCountAsync(
                expectedBucket,
                Guid.Empty,
                $"Sync_SavedSearch_{_integrationPoint.ArtifactId}_{jobId}",
                1), Times.Once);
        }

        private void VerifyEmptyMetric(IMetric metric)
        {
            metric.Should().BeOfType(typeof(EmptyMetric));

            _metricsManagerMock.Verify(x => x.LogCountAsync(
                It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>()), Times.Never);
        }

        #endregion
    }
}
