using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Metrics;
using kCura.IntegrationPoints.Common.Metrics.Sink;
using Moq;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Common.Tests.Metrics
{
	[TestFixture]
	public class RipMetricsTests
	{
		private RipMetrics _sut;
		private Mock<IRipMetricsSink> _sinkMock;

		[SetUp]
		public void SetUp()
		{
			_sinkMock = new Mock<IRipMetricsSink>();
			_sut = new RipMetrics(new List<IRipMetricsSink>()
			{
				_sinkMock.Object
			});
		}


		[Test]
		public void TimedOperation_ShouldCreateAndLogMetric()
		{
			// Arrange
			const string name = "Timed Operation";
			TimeSpan duration = TimeSpan.FromSeconds(1);
			const string propertyKey = "prop 1";
			const string propertyValue = "value 1";
			Dictionary<string, object> props = new Dictionary<string, object>()
			{
				{ propertyKey, propertyValue }
			};

			// Act
			_sut.TimedOperation(name, duration, props);

			// Assert
			_sinkMock.Verify(x => x.Log(It.Is<RipMetric>(metric =>
				metric.Name == name && 
				metric.Type == RipMetricType.TimedOperation &&
				metric.CustomData.ContainsKey(propertyKey) &&
				metric.CustomData[propertyKey].ToString() == propertyValue
			)), Times.Once);
		}

		[Test]
		public void PointInTimeLong_ShouldCreateAndLogMetric()
		{
			// Arrange
			const string name = "Timed Operation";
			long value = 2L;
			const string propertyKey = "prop 1";
			const string propertyValue = "value 1";
			Dictionary<string, object> props = new Dictionary<string, object>()
			{
				{ propertyKey, propertyValue }
			};

			// Act
			_sut.PointInTimeLong(name, value, props);

			// Assert
			_sinkMock.Verify(x => x.Log(It.Is<RipMetric>(metric =>
				metric.Name == name &&
				metric.Type == RipMetricType.PointInTimeLong &&
				metric.CustomData.ContainsKey(propertyKey) &&
				metric.CustomData[propertyKey].ToString() == propertyValue
			)), Times.Once);
		}

		[Test]
		public void PointInTimeDouble_ShouldCreateAndLogMetric()
		{
			// Arrange
			const string name = "Timed Operation";
			double value = 3.14;
			const string propertyKey = "prop 1";
			const string propertyValue = "value 1";
			Dictionary<string, object> props = new Dictionary<string, object>()
			{
				{ propertyKey, propertyValue }
			};

			// Act
			_sut.PointInTimeDouble(name, value, props);

			// Assert
			_sinkMock.Verify(x => x.Log(It.Is<RipMetric>(metric =>
				metric.Name == name &&
				metric.Type == RipMetricType.PointInTimeDouble &&
				metric.CustomData.ContainsKey(propertyKey) &&
				metric.CustomData[propertyKey].ToString() == propertyValue
			)), Times.Once);
		}
	}
}