using FluentAssertions;
using kCura.IntegrationPoints.RelativitySync.Metrics;
using kCura.ScheduleQueue.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Metrics
{
	[TestFixture, Category("Unit")]
	public class SyncJobMetricTests
	{
		private Mock<IMetricsFactory> _metricsFactoryFake;
		private Mock<IMetric> _metricFake;
	
		private SyncJobMetric _sut;

		[SetUp]
		public void SetUp()
		{
			_metricFake = new Mock<IMetric>();

			_metricsFactoryFake = new Mock<IMetricsFactory>();
			_metricsFactoryFake.Setup(x => x.CreateScheduleJobCompletedMetric(It.IsAny<Job>())).Returns(_metricFake.Object);
			_metricsFactoryFake.Setup(x => x.CreateScheduleJobFailedMetric(It.IsAny<Job>())).Returns(_metricFake.Object);
			_metricsFactoryFake.Setup(x => x.CreateScheduleJobStartedMetric(It.IsAny<Job>())).Returns(_metricFake.Object);

			Mock<IAPILog> logFake = new Mock<IAPILog>();

			_sut = new SyncJobMetric(_metricsFactoryFake.Object, logFake.Object);
		}

		[Test]
		public void SendJobCompletedAsync_ShouldThrowSyncMetricException_WhenExceptionOccurs()
		{
			// Arrange
			_metricFake.Setup(x => x.SendAsync()).Throws<Exception>();

			// Act
			Func<Task> action = () => _sut.SendJobCompletedAsync(null);

			// Assert
			action.ShouldThrow<SyncMetricException>();
		}

		[Test]
		public void SendJobFailedAsync_ShouldThrowSyncMetricException_WhenExceptionOccurs()
		{
			// Arrange
			_metricFake.Setup(x => x.SendAsync()).Throws<Exception>();
			Exception ex = new Exception();

			// Act
			Func<Task> action = () => _sut.SendJobFailedAsync(null, ex);

			// Assert
			action.ShouldThrow<SyncMetricException>();
		}

		[Test]
		public void SendJobStartedAsync_ShouldThrowSyncMetricException_WhenExceptionOccurs()
		{
			// Arrange
			_metricFake.Setup(x => x.SendAsync()).Throws<Exception>();

			// Act
			Func<Task> action = () => _sut.SendJobStartedAsync(null);

			// Assert
			action.ShouldThrow<SyncMetricException>();
		}
	}
}
