using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Common.Metrics;
using kCura.IntegrationPoints.Common.Metrics.Sink;
using Moq;
using NUnit.Framework;
using Relativity.Telemetry.APM;

namespace kCura.IntegrationPoints.Common.Tests.Metrics.Sink
{
    [TestFixture]
    public class ApmMetricSinkTests
    {
        private Mock<IAPM> _apmMock;
        private ApmRipMetricSink _sut;

        [SetUp]
        public void SetUp()
        {
            _apmMock = new Mock<IAPM>();
            _apmMock.Setup(x => x.CountOperation(It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<int?>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<IEnumerable<ISink>>()))
                .Returns(Mock.Of<ICounterMeasure>());
            _sut = new ApmRipMetricSink(_apmMock.Object);
        }

        [Test]
        public void Log_ShouldReportCountOperation()
        {
            // Arrange
            RipMetric metric = RipMetric.TimedOperation("Metric Name", TimeSpan.FromSeconds(1), Guid.NewGuid().ToString());
            const string propertyKey = "key";
            const string propertyValue = "value";
            metric.CustomData.Add(propertyKey, propertyValue);

            // Act
            _sut.Log(metric);

            // Assert
            _apmMock.Verify(x => x.CountOperation(
                It.Is<string>(name => name == metric.Name),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.Is<Dictionary<string, object>>(dictionary =>
                    dictionary.ContainsKey(propertyKey) &&
                    dictionary[propertyKey].ToString() == propertyValue),
                It.IsAny<IEnumerable<ISink>>()));
        }
    }
}
