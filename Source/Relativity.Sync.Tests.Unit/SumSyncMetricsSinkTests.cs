using System;
using kCura.Vendor.Castle.Core.Internal;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
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

		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<ISyncLog>();
			_metricsManager = new Mock<IMetricsManager>();
		}

		private SumSyncMetricsSink CreateInstance()
		{
			var servicesManager = new Mock<IServicesMgr>();
			servicesManager.Setup(x => x.CreateProxy<IMetricsManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_metricsManager.Object);

			return new SumSyncMetricsSink(servicesManager.Object, _logger.Object);
		}

		[Test]
		public void ItSendsMetricsOnDispose()
		{
			// ARRANGE
			SumSyncMetricsSink instance = CreateInstance();

			const int gaugeValue = 123;
			const string correlationId = "foobar";
			const string unitOfMeasure = "docs";
			TimeSpan timeSpan = TimeSpan.FromDays(1);
			Metric[] expectedMetrics =
			{
				Metric.TimedOperation("Test1", timeSpan, ExecutionStatus.Canceled, correlationId),
				Metric.CountOperation("Test2", ExecutionStatus.Completed, correlationId),
				Metric.GaugeOperation("Test3", ExecutionStatus.Failed, correlationId, gaugeValue, unitOfMeasure)
			};

			// ACT
			expectedMetrics.ForEach(x => instance.Log(x));
			instance.Dispose();

			// ASSERT
			_metricsManager.Verify(x => x.LogTimerAsDoubleAsync(
				It.Is<string>(y => y.Equals("Test1", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(Guid.Empty)),
				It.Is<string>(y => y.Equals(correlationId, StringComparison.Ordinal)),
				It.Is<double>(y => y.Equals(timeSpan.TotalMilliseconds))
				));

			_metricsManager.Verify(x => x.LogCountAsync(
					It.Is<string>(y => y.Equals("Test2", StringComparison.Ordinal)),
					It.Is<Guid>(y => y.Equals(Guid.Empty)),
					It.Is<string>(y => y.Equals(correlationId, StringComparison.Ordinal)),
					It.Is<long>(y => y.Equals(1))
				));

			_metricsManager.Verify(x => x.LogGaugeAsync(
					It.Is<string>(y => y.Equals("Test3", StringComparison.Ordinal)),
					It.Is<Guid>(y => y.Equals(Guid.Empty)),
					It.Is<string>(y => y.Equals(correlationId, StringComparison.Ordinal)),
					It.Is<long>(y => y.Equals(gaugeValue))
				));

			_logger.Verify(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);

			_metricsManager.Verify(x => x.Dispose(), Times.Once);
		}

		[Test]
		public void ItDoesNotSendMetricsOnLog()
		{
			// ARRANGE
			SumSyncMetricsSink instance = CreateInstance();

			const int gaugeValue = 123;
			const string correlationId = "foobar";
			const string unitOfMeasure = "docs";
			TimeSpan timeSpan = TimeSpan.FromDays(1);
			Metric[] expectedMetrics =
			{
				Metric.TimedOperation("Test1", timeSpan, ExecutionStatus.Canceled, correlationId),
				Metric.CountOperation("Test2", ExecutionStatus.Completed, correlationId),
				Metric.GaugeOperation("Test3", ExecutionStatus.Failed, correlationId, gaugeValue, unitOfMeasure)
			};

			// ACT
			expectedMetrics.ForEach(x => instance.Log(x));

			// ASSERT
			_metricsManager.Verify(x => x.LogTimerAsDoubleAsync(
				It.Is<string>(y => y.Equals("Test1", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(Guid.Empty)),
				It.Is<string>(y => y.Equals(correlationId, StringComparison.Ordinal)),
				It.Is<double>(y => y.Equals(timeSpan.TotalMilliseconds))
			), Times.Never);

			_metricsManager.Verify(x => x.LogCountAsync(
				It.Is<string>(y => y.Equals("Test2", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(Guid.Empty)),
				It.Is<string>(y => y.Equals(correlationId, StringComparison.Ordinal)),
				It.Is<long>(y => y.Equals(1))
			), Times.Never);

			_metricsManager.Verify(x => x.LogGaugeAsync(
				It.Is<string>(y => y.Equals("Test3", StringComparison.Ordinal)),
				It.Is<Guid>(y => y.Equals(Guid.Empty)),
				It.Is<string>(y => y.Equals(correlationId, StringComparison.Ordinal)),
				It.Is<long>(y => y.Equals(gaugeValue))
			), Times.Never);

			_logger.Verify(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public void ItDisposesMetricsManager()
		{
			// ARRANGE
			SumSyncMetricsSink instance = CreateInstance();

			// ACT
			instance.Dispose();
			
			// ASSERT
			_metricsManager.Verify(x => x.Dispose(), Times.Once);
		}
	}
}