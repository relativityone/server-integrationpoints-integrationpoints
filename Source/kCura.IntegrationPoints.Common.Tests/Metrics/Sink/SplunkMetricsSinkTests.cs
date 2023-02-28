using System;
using kCura.IntegrationPoints.Common.Metrics;
using kCura.IntegrationPoints.Common.Metrics.Sink;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Common.Tests.Metrics.Sink
{
    [TestFixture]
    public class SplunkMetricsSinkTests
    {
        private Mock<IAPILog> _loggerMock;
        private SplunkRipMetricSink _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _sut = new SplunkRipMetricSink(_loggerMock.Object);
        }

        [Test]
        public void Log_ShouldWriteMetricToSplunk()
        {
            // Arrange
            RipMetric metric = RipMetric.TimedOperation("Metric Name", TimeSpan.FromSeconds(1), Guid.NewGuid().ToString());
            const string propertyKey = "key";
            const string propertyValue = "value";
            metric.CustomData.Add(propertyKey, propertyValue);

            // Act
            _sut.Log(metric);

            // Assert
            _loggerMock.Verify(x => x.LogInformation(
                It.Is<string>(template => template == "Logging metric '{MetricName}' with properties: {@MetricProperties}"),
                It.Is<string>(name => name == metric.Name),
                It.Is<RipMetric>(actualMetric => actualMetric == metric)));
        }
    }
}
