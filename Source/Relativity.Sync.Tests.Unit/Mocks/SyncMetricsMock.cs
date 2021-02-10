using System;
using System.Collections.Generic;
using Moq;
using Relativity.API;
using Relativity.Sync.Telemetry;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit.Mocks
{
	internal class SyncMetricsMock : SyncMetrics
	{
		public Mock<ISyncLog> SyncLogMock { get; private set; }

		public Mock<IMetricsManager> MetricsManagerMock { get; private set; }

		public Mock<IAPMClient> ApmClientMock { get; private set; }

		private SyncMetricsMock(IEnumerable<ISyncMetricsSink> sinks, SyncJobParameters syncJobParameters) 
			: base(sinks, syncJobParameters)
		{
		}

		public static SyncMetricsMock Initialize(SyncJobParameters syncJobParameters, Guid workpsaceGuid,
			Mock<ISyncLog> syncLogMock = null, Mock<IMetricsManager> metricsManagerMock = null, Mock<IAPMClient> apmClientMock = null)
		{
			syncLogMock = syncLogMock ?? new Mock<ISyncLog>();
			metricsManagerMock = metricsManagerMock ?? new Mock<IMetricsManager>();
			apmClientMock = apmClientMock ?? new Mock<IAPMClient>();

			Mock<ISyncServiceManager> syncServiceManager = new Mock<ISyncServiceManager>();
			syncServiceManager.Setup(x => x.CreateProxy<IMetricsManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(metricsManagerMock.Object);

			Mock<IWorkspaceGuidService> workspaceGuidService = new Mock<IWorkspaceGuidService>();
			workspaceGuidService.Setup(x => x.GetWorkspaceGuidAsync(syncJobParameters.WorkspaceId))
				.ReturnsAsync(workpsaceGuid);

			IEnumerable<ISyncMetricsSink> sinks = new List<ISyncMetricsSink>()
			{
				new SumSyncMetricsSink(syncServiceManager.Object, syncLogMock.Object,
					workspaceGuidService.Object, syncJobParameters),
				new NewRelicSyncMetricsSink(apmClientMock.Object),
				new SplunkSyncMetricsSink(syncLogMock.Object)
			};

			return new SyncMetricsMock(sinks, syncJobParameters)
			{
				SyncLogMock = syncLogMock,
				MetricsManagerMock = metricsManagerMock,
				ApmClientMock = apmClientMock
			};
		}
	}
}
