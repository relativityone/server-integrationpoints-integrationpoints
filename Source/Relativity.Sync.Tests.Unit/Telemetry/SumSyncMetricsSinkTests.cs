using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Tests.Common;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry
{
	[TestFixture]
	public class SumSyncMetricsSinkTests
	{
		private Mock<IMetricsManager> _metricsManagerMock;
		private Mock<ISyncLog> _syncLogMock;
		private SyncJobParameters _syncJobParamters;

		private SumSyncMetricsSink _sut;

		private readonly Guid _workspaceGuid = Guid.NewGuid();

		[SetUp]
		public void SetUp()
		{
			_syncJobParamters = FakeHelper.CreateSyncJobParameters();

			_metricsManagerMock = new Mock<IMetricsManager>(MockBehavior.Strict);
			_metricsManagerMock.Setup(x => x.Dispose());

			Mock<ISourceServiceFactoryForAdmin> syncServiceManager = new Mock<ISourceServiceFactoryForAdmin>();
			syncServiceManager.Setup(x => x.CreateProxyAsync<IMetricsManager>())
				.Returns(Task.FromResult(_metricsManagerMock.Object));

			_syncLogMock = new Mock<ISyncLog>();

			Mock<IWorkspaceGuidService> workspaceGuidServiceFake = new Mock<IWorkspaceGuidService>();
			workspaceGuidServiceFake.Setup(x => x.GetWorkspaceGuidAsync(It.IsAny<int>())).ReturnsAsync(_workspaceGuid);

			_sut = new SumSyncMetricsSink(syncServiceManager.Object, _syncLogMock.Object, 
				workspaceGuidServiceFake.Object, _syncJobParamters);
		}

		[Test]
		public void Send_ShouldCallSumMetricSend_WhenAttributeInMetricExists()
		{
			// Arrange
			TestMetric metric = new TestMetric
			{
				CorrelationId = "id",
				Test = 1000
			};

			// Act
			_sut.Send(metric);

			// Assert
			_metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync("TestName", _workspaceGuid, metric.CorrelationId, metric.Test.Value), Times.Once);
		}

		[Test]
		public void Send_ShouldLogErrorButNotThrow_WhenAttributeAndValueTypeDoesNotMatch()
		{
			// Arrange
			NotMatchingValueMetric metric = new NotMatchingValueMetric
			{
				CorrelationId = "id",
				InvalidValue = "1000"
			};

			// Act
			_sut.Send(metric);

			// Assert
			_syncLogMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), "InvalidType", metric.CorrelationId, metric.InvalidValue), Times.Once);
			_metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync("InvalidType", _workspaceGuid, metric.CorrelationId, It.IsAny<double>()), Times.Never);
		}

		[Test]
		public void Send_ShouldNotSendAnyMetrics_WhenMetricIsEmpty()
		{
			// Arrange
			TestMetric metric = new TestMetric();

			// Act
			_sut.Send(metric);

			// Assert
			_metricsManagerMock.Verify(x => x.Dispose());
		}

		[Test]
		public void Send_ShouldNotSendMetric_WhenPropertyIsNull()
		{
			// Arrange
			TestMetric metric = new TestMetric()
			{
				Test = 1000
			};

			// Act
			_sut.Send(metric);

			// Assert
			_metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync("TimedTestName", It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<double>()), 
				Times.Never);
		}

		[Test]
		public void Send_ShouldNotThrowAndSendAnyMetrics_WhenNoAttributesWasFound()
		{
			// Arrange
			NonAttributeMetric metric = new NonAttributeMetric() { SomeValue = "value" };

			// Act
			Action action = () => _sut.Send(metric);

			// Assert
			action.Should().NotThrow();

			_metricsManagerMock.Verify(x => x.Dispose());
		}

		internal class TestMetric : MetricBase<TestMetric>
		{
			[Metric(MetricType.PointInTimeLong, "TestName")]
			public long? Test { get; set; }

			[Metric(MetricType.TimedOperation, "TimedTestName")]
			public long? TimedTest { get; set; }
		}

		internal class NotMatchingValueMetric : MetricBase<NotMatchingValueMetric>
		{
			[Metric(MetricType.PointInTimeDouble, "InvalidType")]
			public string InvalidValue { get; set; }
		}

		internal class NonAttributeMetric : MetricBase<NonAttributeMetric>
		{
			public string SomeValue { get; set; }
		}
	}
}
