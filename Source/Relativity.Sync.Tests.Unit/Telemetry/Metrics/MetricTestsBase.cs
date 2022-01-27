using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry.Metrics
{
	[TestFixture]
	internal abstract class MetricTestsBase<T> where T: IMetric
	{
		private ISyncMetrics _syncMetrics;

		private Mock<ISyncLog> _syncLogMock;
		private Mock<IMetricsManager> _metricsManagerMock;
		private Mock<IAPMClient> _apmMock;

		protected const int _WORKSPACE_ID = 100;

		protected readonly Guid _EXPECTED_WORKSPACE_GUID = Guid.NewGuid();
		protected readonly SyncJobParameters _jobParameters = new SyncJobParameters(It.IsAny<int>(), _WORKSPACE_ID, It.IsAny<Guid>());
		private Mock<IMetricsConfiguration> _metricsConfigurationFake;

		protected const string _APPLICATION_NAME = "Relativity.Sync";
		[SetUp]
		public void SetUp()
		{
			_syncLogMock = new Mock<ISyncLog>();
			_metricsManagerMock = new Mock<IMetricsManager>(MockBehavior.Strict);
			_metricsManagerMock.Setup(x => x.Dispose());
			_apmMock = new Mock<IAPMClient>();

			ISyncMetricsSink splunkSink = new SplunkSyncMetricsSink(_syncLogMock.Object);

			Mock<ISyncServiceManager> serviceManager = new Mock<ISyncServiceManager>();
			serviceManager.Setup(x => x.CreateProxy<IMetricsManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_metricsManagerMock.Object);

			Mock<IWorkspaceGuidService> workspaceGuidService = new Mock<IWorkspaceGuidService>();
			workspaceGuidService.Setup(x => x.GetWorkspaceGuidAsync(_WORKSPACE_ID))
				.ReturnsAsync(_EXPECTED_WORKSPACE_GUID);

			ISyncMetricsSink sumSink = new SumSyncMetricsSink(serviceManager.Object, _syncLogMock.Object,
				workspaceGuidService.Object, _jobParameters);

			ISyncMetricsSink apmSink = new NewRelicSyncMetricsSink(_apmMock.Object);

			var sinks = new ISyncMetricsSink[]
			{
				splunkSink,
				sumSink,
				apmSink
			};

			_metricsConfigurationFake = new Mock<IMetricsConfiguration>();
			_syncMetrics = new SyncMetrics(sinks, _metricsConfigurationFake.Object);
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
			//_metricsManagerMock.VerifyNoOtherCalls();
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
			const DataSourceType dataSourceType = DataSourceType.SavedSearch;
			const DestinationLocationType dataDestinationType = DestinationLocationType.Folder;
			int? jobHistoryToRetry = 123;
			const bool imagePush = true;

			_metricsConfigurationFake.SetupGet(x => x.CorrelationId).Returns(correlationId);
			_metricsConfigurationFake.SetupGet(x => x.ExecutingApplication).Returns(executingAppName);
			_metricsConfigurationFake.SetupGet(x => x.ExecutingApplicationVersion).Returns(executingAppVersion);
			_metricsConfigurationFake.SetupGet(x => x.SyncVersion).Returns(syncVersion);
			_metricsConfigurationFake.SetupGet(x => x.DataSourceType).Returns(dataSourceType);
			_metricsConfigurationFake.SetupGet(x => x.DataDestinationType).Returns(dataDestinationType);
			_metricsConfigurationFake.SetupGet(x => x.JobHistoryToRetryId).Returns(jobHistoryToRetry);
			_metricsConfigurationFake.SetupGet(x => x.ImageImport).Returns(imagePush);

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

		[TestCase(true, "Images")]
		[TestCase(false, "NativesOrMetadata")]
		public void Send_ShouldSetFlowType(bool imageImport, string expectedFlowType)
		{
			// Arrange
			IMetric metric = EmptyTestMetric();
			_metricsConfigurationFake.SetupGet(x => x.ImageImport).Returns(imageImport);

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
