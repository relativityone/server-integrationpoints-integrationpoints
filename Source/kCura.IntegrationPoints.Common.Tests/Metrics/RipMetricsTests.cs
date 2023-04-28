using System;
using System.Collections.Generic;
using System.Linq;
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
            MetricValues<TimeSpan> metricValues = MetricValues<TimeSpan>.PrepareValues("TimedOperation", TimeSpan.FromSeconds(1), Guid.NewGuid().ToString());

            // Act
            _sut.TimedOperation(metricValues.Name, metricValues.Value, metricValues.CustomData, metricValues.CorrelationId);

            // Assert
            _sinkMock.Verify(
                x => x.Log(It.Is<RipMetric>(metric =>
                metric.Name == metricValues.Name &&
                metric.WorkflowId == metricValues.CorrelationId &&
                metric.Type == RipMetricType.TimedOperation &&
                DictionariesAreEqual(metric.CustomData, metricValues.CustomData))), Times.Once);
        }

        [Test]
        public void PointInTimeLong_ShouldCreateAndLogMetric()
        {
            // Arrange
            MetricValues<long> metricValues = MetricValues<long>.PrepareValues("PointInTimeLong", 2L, Guid.NewGuid().ToString());

            // Act
            _sut.PointInTimeLong(metricValues.Name, metricValues.Value, metricValues.CustomData, metricValues.CorrelationId);

            // Assert
            _sinkMock.Verify(
                x => x.Log(It.Is<RipMetric>(metric =>
                metric.Name == metricValues.Name &&
                metric.WorkflowId == metricValues.CorrelationId &&
                metric.Type == RipMetricType.PointInTimeLong &&
                DictionariesAreEqual(metric.CustomData, metricValues.CustomData))), Times.Once);
        }

        [Test]
        public void PointInTimeDouble_ShouldCreateAndLogMetric()
        {
            // Arrange
            MetricValues<double> metricValues = MetricValues<double>.PrepareValues("PointInTimeDouble", 3.14, Guid.NewGuid().ToString());

            // Act
            _sut.PointInTimeDouble(metricValues.Name, metricValues.Value, metricValues.CustomData, metricValues.CorrelationId);

            // Assert
            _sinkMock.Verify(
                x => x.Log(It.Is<RipMetric>(metric =>
                metric.Name == metricValues.Name &&
                metric.WorkflowId == metricValues.CorrelationId &&
                metric.Type == RipMetricType.PointInTimeDouble &&
                DictionariesAreEqual(metric.CustomData, metricValues.CustomData))), Times.Once);
        }

        private bool DictionariesAreEqual(Dictionary<string, object> dict1, Dictionary<string, object> dict2) =>
            dict1.Count() == dict2.Count() && !dict1.Except(dict2).Any();

        private class MetricValues<T>
        {
            public string Name;
            public T Value;
            public string CorrelationId;
            public Dictionary<string, object> CustomData = new Dictionary<string, object>
            {
                { "prop 1", "val 1" },
                { "prop 2", "val 2" }
            };

            public static MetricValues<T> PrepareValues(string name, T value, string correlationId) =>
                new MetricValues<T>() { Name = name, Value = value, CorrelationId = correlationId };
        }
    }
}
