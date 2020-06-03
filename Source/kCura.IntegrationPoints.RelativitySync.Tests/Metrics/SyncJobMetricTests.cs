using FluentAssertions;
using kCura.IntegrationPoints.RelativitySync.Metrics;
using kCura.ScheduleQueue.Core;
using Moq;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Threading.Tasks;

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

			IAPILog log = Substitute.For<IAPILog>();

			_sut = new SyncJobMetric(_metricsFactoryFake.Object, log);
		}

		[Test]
		public void SendJobCompletedAsync_ShouldTrowSyncMetricException_WhenExceptionOccurs()
		{
			// Arrange
			_metricFake.Setup(x => x.SendAsync()).Throws<Exception>();

			// Act
			Func<Task> action = () => _sut.SendJobCompletedAsync(null);

			// Assert
			action.ShouldThrow<SyncMetricException>();
		}

		[Test]
		public void SendJobFailedAsync_ShouldTrowSyncMetricException_WhenExceptionOccurs()
		{
			// Arrange
			_metricFake.Setup(x => x.SendAsync()).Throws<Exception>();

			// Act
			Func<Task> action = () => _sut.SendJobFailedAsync(null);

			// Assert
			action.ShouldThrow<SyncMetricException>();
		}

		[Test]
		public void SendJobStartedAsync_ShouldTrowSyncMetricException_WhenExceptionOccurs()
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
