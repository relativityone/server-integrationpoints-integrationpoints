﻿using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;
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
		protected const int _JOB_HISTORY_ID = 200;

		protected readonly Guid _EXPECTED_WORKSPACE_GUID = Guid.NewGuid();
		protected readonly SyncJobParameters _jobParameters = new SyncJobParameters(It.IsAny<int>(), _WORKSPACE_ID, _JOB_HISTORY_ID);
		private ConfigurationStub _metricsConfiguration;

		protected const string _APPLICATION_NAME = "Relativity.Sync";

		[SetUp]
		public void SetUp()
		{
			_syncLogMock = new Mock<ISyncLog>();
			_metricsManagerMock = new Mock<IMetricsManager>();
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

			_metricsConfiguration = new ConfigurationStub();
			_syncMetrics = new SyncMetrics(sinks, _metricsConfiguration);
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
			_metricsManagerMock.VerifyNoOtherCalls();
		}
		
		[Test]
		public void Send_ShouldSetAllDecoratorsOnMetric()
		{
			// Arrange
			IMetric metric = EmptyTestMetric();


			// Act
			_syncMetrics.Send(metric);

			// Assert
			metric.CorrelationId.Should().Be(_metricsConfiguration.CorrelationId);
			metric.ExecutingApplication.Should().Be(_metricsConfiguration.ExecutingApplication);
			metric.ExecutingApplicationVersion.Should().Be(_metricsConfiguration.ExecutingApplicationVersion);
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
