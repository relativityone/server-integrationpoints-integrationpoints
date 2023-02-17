using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Relativity.IntegrationPoints.FieldsMapping.Metrics;

namespace Relativity.IntegrationPoints.FieldsMapping.Tests
{
    [TestFixture]
    public class MetricsSenderTests
    {
        private MetricsSender _sut;
        private Mock<IMetricsSink> _sinkMock;

        [SetUp]
        public void SetUp()
        {
            _sinkMock = new Mock<IMetricsSink>();
            _sut = new MetricsSender(new List<IMetricsSink>()
            {
                _sinkMock.Object
            });
        }

        [Test]
        public void CountOperation_ShouldCreateAndLogMetric()
        {
            // Arrange
            const string name = "Counter Metric";

            // Act
            _sut.CountOperation(name);

            // Assert
            _sinkMock.Verify(x => x.Log(It.Is<Metric>(metric =>
                metric.Name == name && metric.Type == MetricType.Counter
            )), Times.Once);
        }

        [Test]
        public void GaugeOperation_ShouldCreateAndLogMetric()
        {
            // Arrange
            const string name = "Gauge Metric";
            const long value = 1111;
            const string unitOfMeasure = "Unit";

            // Act
            _sut.GaugeOperation(name, value, unitOfMeasure);

            // Assert
            _sinkMock.Verify(x => x.Log(It.Is<Metric>(metric =>
                metric.Name == name &&
                metric.Type == MetricType.GaugeOperation &&
                (long)metric.Value == value &&
                metric.CustomData.FirstOrDefault().Key == "unitOfMeasure" &&
                metric.CustomData.FirstOrDefault().Value.ToString() == unitOfMeasure
            )), Times.Once);
        }
    }
}
