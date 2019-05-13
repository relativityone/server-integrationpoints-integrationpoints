using System;
using kCura.Vendor.Castle.Core.Internal;
using Moq;
using NUnit.Framework;
using Relativity.API;
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
		private Metric[] _expectedMetrics;

		private const int _GAUGE_VALUE = 123;
		private const string _CORRELATION_ID = "foobar";
		private const string _UNIT_OF_MEASURE = "docs";

		private readonly TimeSpan _timeSpan = TimeSpan.FromDays(1);

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_expectedMetrics = new[]
			{
				Metric.TimedOperation("Test1", _timeSpan, ExecutionStatus.Canceled, _CORRELATION_ID),
				Metric.CountOperation("Test2", ExecutionStatus.Completed, _CORRELATION_ID),
				Metric.GaugeOperation("Test3", ExecutionStatus.Failed, _CORRELATION_ID, _GAUGE_VALUE, _UNIT_OF_MEASURE)
			};
		}

		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<ISyncLog>();
			_metricsManager = new Mock<IMetricsManager>();
			_servicesManager = new Mock<IServicesMgr>();

			_servicesManager.Setup(x => x.CreateProxy<IMetricsManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_metricsManager.Object);
		}

		private SumSyncMetricsSink CreateInstance()
		{
			return new SumSyncMetricsSink(_servicesManager.Object, _logger.Object);
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
				It.Is<Guid>(y => y.Equals(Guid.Empty)),
				It.Is<string>(y => y.Equals(_CORRELATION_ID, StringComparison.Ordinal)),
				It.Is<double>(y => y.Equals(_timeSpan.TotalMilliseconds))
			), times);

			_metricsManager.Verify(x => x.LogCountAsync(
				It.Is<string>(y => y.Equals("Test2", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(Guid.Empty)),
				It.Is<string>(y => y.Equals(_CORRELATION_ID, StringComparison.Ordinal)),
				It.Is<long>(y => y.Equals(1))
			), times);

			_metricsManager.Verify(x => x.LogGaugeAsync(
				It.Is<string>(y => y.Equals("Test3", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(Guid.Empty)),
				It.Is<string>(y => y.Equals(_CORRELATION_ID, StringComparison.Ordinal)),
				It.Is<long>(y => y.Equals(_GAUGE_VALUE))
			), times);
		}

		private void VerifyLogErrorIsCalled(Times times)
		{
			_logger.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>()), times);
		}

		private void VerifyMetricsManagerWasDisposed(Times times)
		{
			_metricsManager.Verify(x => x.Dispose(), times);
		}
	}
}