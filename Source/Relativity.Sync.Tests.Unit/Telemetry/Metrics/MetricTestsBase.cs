using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.RelEye;
using Relativity.Sync.Tests.Common;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
    [TestFixture]
    internal abstract class MetricTestsBase<T> where T : IMetric
    {
        private const string _DOCUMENT_FLOW_NAME = "NativesOrMetadata";
        private const string _IMAGES_FLOW_NAME = "Images";
        private const string _NON_DOCUMENT_FLOW_NAME = "NonDocumentObjects";

        private ISyncMetrics _syncMetrics;

        private Mock<IAPILog> _syncLogMock;
        private Mock<IMetricsManager> _metricsManagerMock;
        private Mock<IAPMClient> _apmMock;

        protected IFixture _fxt;

        protected const int _WORKSPACE_ID = 100;
        private const int _USER_ID = 323454;

        protected readonly Guid _EXPECTED_WORKSPACE_GUID = Guid.NewGuid();
        protected readonly SyncJobParameters _jobParameters = new SyncJobParameters(It.IsAny<int>(), _WORKSPACE_ID, _USER_ID, It.IsAny<Guid>(), Guid.Empty);
        private Mock<IMetricsConfiguration> _metricsConfigurationFake;

        protected const string _APPLICATION_NAME = "Relativity.Sync";

        [SetUp]
        public void SetUp()
        {
            _fxt = FixtureFactory.Create();

            _syncLogMock = new Mock<IAPILog>();
            _metricsManagerMock = new Mock<IMetricsManager>(MockBehavior.Strict);
            _metricsManagerMock.Setup(x => x.Dispose());
            _apmMock = new Mock<IAPMClient>();

            ISyncMetricsSink splunkSink = new SplunkSyncMetricsSink(_syncLogMock.Object);

            Mock<ISourceServiceFactoryForAdmin> serviceFactoryForAdminMock = new Mock<ISourceServiceFactoryForAdmin>();
            serviceFactoryForAdminMock.Setup(x => x.CreateProxyAsync<IMetricsManager>())
                .Returns(Task.FromResult(_metricsManagerMock.Object));

            Mock<IWorkspaceGuidService> workspaceGuidService = new Mock<IWorkspaceGuidService>();
            workspaceGuidService.Setup(x => x.GetWorkspaceGuidAsync(_WORKSPACE_ID))
                .ReturnsAsync(_EXPECTED_WORKSPACE_GUID);

            ISyncMetricsSink sumSink = new SumSyncMetricsSink(serviceFactoryForAdminMock.Object, _syncLogMock.Object,
                workspaceGuidService.Object, _jobParameters);

            ISyncMetricsSink apmSink = new NewRelicSyncMetricsSink(_apmMock.Object);

            var sinks = new ISyncMetricsSink[]
            {
                splunkSink,
                sumSink,
                apmSink
            };

            Mock<IAPILog> log = new Mock<IAPILog>();

            Mock<IEventPublisher> eventPublisher = new Mock<IEventPublisher>();

            _metricsConfigurationFake = new Mock<IMetricsConfiguration>();
            _metricsConfigurationFake.SetupGet(x => x.RdoArtifactTypeId).Returns((int)ArtifactType.Document);
            _metricsConfigurationFake.SetupGet(x => x.DestinationRdoArtifactTypeId).Returns((int)ArtifactType.Document);
            _syncMetrics = new SyncMetrics(eventPublisher.Object, sinks, _metricsConfigurationFake.Object, log.Object);
        }

        [Test]
        public void Send_ShouldSendCorrectMetricsToAllSinks()
        {
            // Arrange
            IMetric metric = ArrangeTestMetric();

            // Act
            _syncMetrics.Send(metric);

            // Assert
            VerifySplunkSink(metric);

            VerifySumSink(_metricsManagerMock);

            VerifyApmSink(_apmMock);
        }

        [Test]
        public void Send_ShouldNotSendSumMetrics_ForNullValues()
        {
            // Arrange
            IMetric metric = EmptyTestMetric();

            // Act
            _syncMetrics.Send(metric);

            // Assert
            _metricsManagerMock.Verify(x => x.Dispose());
        }

        [Test]
        public void Send_ShouldSetAllDecoratorsOnMetric()
        {
            // Arrange
            IMetric metric = EmptyTestMetric();
            string correlationId = Guid.NewGuid().ToString();
            const string executingAppName = "SomeApp";
            const string executingAppVersion = "1.2.3.4";
            const string syncVersion = "1.2.3.5";
            const DestinationLocationType dataDestinationType = DestinationLocationType.Folder;
            const DataSourceType dataSourceType = DataSourceType.SavedSearch;
            const bool imagePush = true;
            const int rdoArtifactTypeId = (int)ArtifactType.Document;
            const int destinationRdoArtifactTypeId = (int)ArtifactType.Document;
            int? jobHistoryToRetry = 123;

            _metricsConfigurationFake.SetupGet(x => x.CorrelationId).Returns(correlationId);
            _metricsConfigurationFake.SetupGet(x => x.ExecutingApplication).Returns(executingAppName);
            _metricsConfigurationFake.SetupGet(x => x.ExecutingApplicationVersion).Returns(executingAppVersion);
            _metricsConfigurationFake.SetupGet(x => x.SyncVersion).Returns(syncVersion);
            _metricsConfigurationFake.SetupGet(x => x.DataSourceType).Returns(dataSourceType);
            _metricsConfigurationFake.SetupGet(x => x.DataDestinationType).Returns(dataDestinationType);
            _metricsConfigurationFake.SetupGet(x => x.JobHistoryToRetryId).Returns(jobHistoryToRetry);
            _metricsConfigurationFake.SetupGet(x => x.ImageImport).Returns(imagePush);
            _metricsConfigurationFake.SetupGet(x => x.RdoArtifactTypeId).Returns(rdoArtifactTypeId);
            _metricsConfigurationFake.SetupGet(x => x.DestinationRdoArtifactTypeId).Returns(destinationRdoArtifactTypeId);

            // Act
            _syncMetrics.Send(metric);

            // Assert
            metric.CorrelationId.Should().Be(correlationId);
            metric.ExecutingApplication.Should().Be(executingAppName);
            metric.ExecutingApplicationVersion.Should().Be(executingAppVersion);
            metric.SyncVersion.Should().Be(syncVersion);
            metric.DataSourceType.Should().Be(dataSourceType.GetDescription());
            metric.DataDestinationType.Should().Be(dataDestinationType.GetDescription());
            metric.IsRetry.Should().Be(true);
            metric.FlowName.Should().Be("Images");
        }

        [TestCase(null, false)]
        [TestCase(123, true)]
        public void Send_ShouldSetIsRetryProperty(int? jobHistoryToRetry, bool expectedResult)
        {
            // Arrange
            IMetric metric = EmptyTestMetric();
            _metricsConfigurationFake.SetupGet(x => x.JobHistoryToRetryId).Returns(jobHistoryToRetry);

            // Act
            _syncMetrics.Send(metric);

            // Assert
            metric.IsRetry.Should().Be(expectedResult);
        }

        [TestCase(true, _IMAGES_FLOW_NAME)]
        [TestCase(false, _DOCUMENT_FLOW_NAME)]
        [TestCase(false, _NON_DOCUMENT_FLOW_NAME)]
        public void Send_ShouldSetFlowType(bool imageImport, string expectedFlowType)
        {
            // Arrange
            IMetric metric = EmptyTestMetric();
            _metricsConfigurationFake.SetupGet(x => x.ImageImport).Returns(imageImport);

            if (expectedFlowType == _NON_DOCUMENT_FLOW_NAME)
            {
                _metricsConfigurationFake.SetupGet(x => x.RdoArtifactTypeId).Returns((int)ArtifactType.Agent);
                _metricsConfigurationFake.SetupGet(x => x.DestinationRdoArtifactTypeId).Returns((int)ArtifactType.Sync);
            }

            // Act
            _syncMetrics.Send(metric);

            // Assert
            metric.FlowName.Should().Be(expectedFlowType);
        }

        protected void VerifySplunkSink(IMetric metric)
        {
            _syncLogMock.Verify(x => x.LogInformation(It.IsAny<string>(), metric.GetType(), metric));
        }

        protected abstract IMetric ArrangeTestMetric();

        protected abstract IMetric EmptyTestMetric();

        protected abstract void VerifySumSink(Mock<IMetricsManager> metricsManagerMock);

        protected abstract void VerifyApmSink(Mock<IAPMClient> apmMock);
    }
}
