﻿using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
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

		private const int _WORKSPACE_ID = 111;
		private readonly Guid _workspaceGuid = Guid.NewGuid();

		[SetUp]
		public void SetUp()
		{
			_syncJobParamters = new SyncJobParameters(It.IsAny<int>(), _WORKSPACE_ID, It.IsAny<int>());

			_metricsManagerMock = new Mock<IMetricsManager>();

			Mock<ISyncServiceManager> syncServiceManager = new Mock<ISyncServiceManager>();
			syncServiceManager.Setup(x => x.CreateProxy<IMetricsManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_metricsManagerMock.Object);

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
				WorkflowId = "id",
				Test = 1000
			};

			// Act
			_sut.Send(metric);

			// Assert
			_metricsManagerMock.Verify(x => x.LogPointInTimeLongAsync("TestName", _workspaceGuid, metric.WorkflowId, metric.Test.Value), Times.Once);
		}

		[Test]
		public void Send_ShouldLogErrorButNotThrow_WhenAttributeAndValueTypeDoesNotMatch()
		{
			// Arrange
			NotMatchingValueMetric metric = new NotMatchingValueMetric
			{
				WorkflowId = "id",
				InvalidValue = "1000"
			};

			// Act
			_sut.Send(metric);

			// Assert
			_syncLogMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), "InvalidType", metric.WorkflowId, metric.InvalidValue), Times.Once);
			_metricsManagerMock.Verify(x => x.LogPointInTimeDoubleAsync("InvalidType", _workspaceGuid, metric.WorkflowId, It.IsAny<double>()), Times.Never);
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
			_metricsManagerMock.VerifyNoOtherCalls();
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
			_metricsManagerMock.VerifyNoOtherCalls();
		}

		internal class TestMetric : MetricBase
		{
			[Metric(MetricType.PointInTimeLong, "TestName")]
			public long? Test { get; set; }
		}

		internal class NotMatchingValueMetric : MetricBase
		{
			[Metric(MetricType.PointInTimeDouble, "InvalidType")]
			public string InvalidValue { get; set; }
		}

		internal class NonAttributeMetric : MetricBase
		{
			public string SomeValue { get; set; }
		}
	}
}
