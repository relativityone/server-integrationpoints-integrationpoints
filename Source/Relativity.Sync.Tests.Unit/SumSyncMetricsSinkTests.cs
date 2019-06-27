using System;
using kCura.Vendor.Castle.Core.Internal;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;
using Relativity.Telemetry.Services.Metrics;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public class SumSyncMetricsSinkTests
	{
		private Mock<ISyncLog> _logger;
		private Mock<IMetricsManager> _metricsManager;
		private Mock<IServicesMgr> _servicesManager;
		private Mock<IWorkspaceGuidService> _workspaceGuidService;
		private SyncJobParameters _syncJobParameters;
		private Guid _workspaceGuid;
		private Metric[] _expectedMetrics;

		private const int _GAUGE_VALUE = 123;
		private const string _CORRELATION_ID = "foobar";
		private const string _UNIT_OF_MEASURE = "docs";
		private const string _WORKFLOW_ID = "Sync_101654_102985";

		private readonly TimeSpan _timeSpan = TimeSpan.FromDays(1);

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_expectedMetrics = new[]
			{
				Metric.TimedOperation("Test1", _timeSpan, ExecutionStatus.Canceled, _CORRELATION_ID),
				Metric.CountOperation("Test2", ExecutionStatus.Completed, _CORRELATION_ID),
				Metric.GaugeOperation("Test3", ExecutionStatus.Failed, _CORRELATION_ID, _GAUGE_VALUE, _UNIT_OF_MEASURE),
				Metric.PointInTimeStringOperation("Test.String", "Sync", _WORKFLOW_ID, _CORRELATION_ID),
				Metric.PointInTimeLongOperation("Test.Long", long.MaxValue, _WORKFLOW_ID, _CORRELATION_ID),
				Metric.PointInTimeDoubleOperation("Test.Double", double.MaxValue, _WORKFLOW_ID, _CORRELATION_ID)
			};
		}

		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<ISyncLog>();
			_metricsManager = new Mock<IMetricsManager>();
			_servicesManager = new Mock<IServicesMgr>();
			_workspaceGuidService = new Mock<IWorkspaceGuidService>();
			_workspaceGuid = Guid.NewGuid();
			_workspaceGuidService.Setup(x => x.GetWorkspaceGuidAsync(It.IsAny<int>())).ReturnsAsync(_workspaceGuid);
			_syncJobParameters = new SyncJobParameters(0, 0, new ImportSettingsDto());

			_servicesManager.Setup(x => x.CreateProxy<IMetricsManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_metricsManager.Object);
		}

		private SumSyncMetricsSink CreateInstance()
		{
			return new SumSyncMetricsSink(_servicesManager.Object, _logger.Object, _workspaceGuidService.Object, _syncJobParameters);
		}

		[Test]
		public void ItSendsMetricsOnLogAndMetricsManagerWasDisposed()
		{
			// ARRANGE
			SumSyncMetricsSink instance = CreateInstance();

			// ACT
			_expectedMetrics.ForEach(x => instance.Log(x));

			// ASSERT
			VerifyEachExpectedMetricLogIsCalled(Times.Once());

			VerifyMetricsManagerWasDisposed(Times.Exactly(_expectedMetrics.Length));
			VerifyLogErrorIsCalled(Times.Never());
		}

		[Test]
		public void ItShouldCatchAndLogExceptionsThrownByServicesManager()
		{
			// ARRANGE
			SumSyncMetricsSink instance = CreateInstance();

			_servicesManager.Setup(x => x.CreateProxy<IMetricsManager>(It.IsAny<ExecutionIdentity>()))
				.Throws<Exception>();

			// ACT
			Assert.DoesNotThrow(() => _expectedMetrics.ForEach(x => instance.Log(x)));

			// ASSERT
			VerifyEachExpectedMetricLogIsCalled(Times.Never());
			
			VerifyLogErrorIsCalled(Times.Exactly(_expectedMetrics.Length));
			VerifyMetricsManagerWasDisposed(Times.Never());
		}

		[Test]
		public void ItShouldCatchAndLogExceptionsThrownByLogSumMetric()
		{
			// ARRANGE
			SumSyncMetricsSink instance = CreateInstance();

			_metricsManager.Setup(x => x.LogGaugeAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>()))
				.Throws<Exception>();
			_metricsManager.Setup(x => x.LogCountAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>()))
				.Throws<Exception>();
			_metricsManager.Setup(x => x.LogTimerAsDoubleAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<double>()))
				.Throws<Exception>();
			_metricsManager.Setup(x => x.LogPointInTimeStringAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
				.Throws<Exception>();
			_metricsManager.Setup(x => x.LogPointInTimeLongAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long>()))
				.Throws<Exception>();
			_metricsManager.Setup(x => x.LogPointInTimeDoubleAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<double>()))
				.Throws<Exception>();

			// ACT
			Assert.DoesNotThrow(() => _expectedMetrics.ForEach(x => instance.Log(x)));

			// ASSERT
			VerifyEachExpectedMetricLogIsCalled(Times.Once());

			int callCount = _expectedMetrics.Length;
			VerifyMetricsManagerWasDisposed(Times.Exactly(callCount));
			VerifyLogErrorIsCalled(Times.Exactly(callCount));
			VerifyMetricsManagerWasDisposed(Times.Exactly(callCount));
		}

		private void VerifyEachExpectedMetricLogIsCalled(Times times)
		{
			_metricsManager.Verify(x => x.LogTimerAsDoubleAsync(
				It.Is<string>(y => y.Equals("Test1", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(_workspaceGuid)),
				It.Is<string>(y => y.Equals(_CORRELATION_ID, StringComparison.Ordinal)),
				It.Is<double>(y => y.Equals(_timeSpan.TotalMilliseconds))
			), times);

			_metricsManager.Verify(x => x.LogCountAsync(
				It.Is<string>(y => y.Equals("Test2", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(_workspaceGuid)),
				It.Is<string>(y => y.Equals(_CORRELATION_ID, StringComparison.Ordinal)),
				It.Is<long>(y => y.Equals(1))
			), times);

			_metricsManager.Verify(x => x.LogGaugeAsync(
				It.Is<string>(y => y.Equals("Test3", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(_workspaceGuid)),
				It.Is<string>(y => y.Equals(_CORRELATION_ID, StringComparison.Ordinal)),
				It.Is<long>(y => y.Equals(_GAUGE_VALUE))
			), times);

			_metricsManager.Verify(x => x.LogPointInTimeStringAsync(
				It.Is<string>(y => y.Equals("Test.String", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(_workspaceGuid)),
				It.Is<string>(y => y ==  _WORKFLOW_ID),
				It.Is<string>(y => y == "Sync")
			), times);

			_metricsManager.Verify(x => x.LogPointInTimeLongAsync(
				It.Is<string>(y => y.Equals("Test.Long", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(_workspaceGuid)),
				It.Is<string>(y => y == _WORKFLOW_ID),
				It.Is<long>(y => y == long.MaxValue)
			), times);

			_metricsManager.Verify(x => x.LogPointInTimeDoubleAsync(
				It.Is<string>(y => y.Equals("Test.Double", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(_workspaceGuid)),
				It.Is<string>(y => y == _WORKFLOW_ID),
				It.Is<double>(y => y == double.MaxValue)
			), times);
		}

		private void VerifyLogErrorIsCalled(Times times)
		{
			_logger.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), times);
		}

		private void VerifyMetricsManagerWasDisposed(Times times)
		{
			_metricsManager.Verify(x => x.Dispose(), times);
		}
	}
}