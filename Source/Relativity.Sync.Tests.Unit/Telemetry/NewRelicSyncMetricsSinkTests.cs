using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;

namespace Relativity.Sync.Tests.Unit.Telemetry
{
    [TestFixture]
    public class NewRelicSyncMetricsSinkTests
    {
        private Mock<IAPMClient> _apmClientMock;
        private NewRelicSyncMetricsSink _sut;

        private const string _APPLICATION_NAME = "Relativity.Sync";

        [SetUp]
        public void SetUp()
        {
            _apmClientMock = new Mock<IAPMClient>();

            _sut = new NewRelicSyncMetricsSink(_apmClientMock.Object);
        }

        [Test]
        public void Send_ShouldSendMetric_WhenValuesHasBeenFound()
        {
            // Arrange
            TestMetric metric = new TestMetric { Value = 1 };

            // Act
            _sut.Send(metric);

            // Assert
            _apmClientMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(
                d => d["Value"].Equals(1))));
        }

        [Test]
        public void Send_ShouldSendMetric_WhenPropertyInMetricIsNull()
        {
            // Arrange
            TestMetric metric = new TestMetric();

            // Act
            _sut.Send(metric);

            // Assert
            _apmClientMock.Verify(x => x.Count(_APPLICATION_NAME, It.Is<Dictionary<string, object>>(
                d => d["Value"] == null)));
        }

        [Test]
        public void Send_ShouldGauge_BatchEndPerformanceMetric()
        {
            // Arrange
            const string correlationId = "Test";
            var metric = new BatchEndPerformanceMetric
            {
                CorrelationId = correlationId
            };

            // Act
            _sut.Send(metric);

            // Assert
            _apmClientMock.Verify(x => x.Gauge(_APPLICATION_NAME, correlationId, It.IsAny<Dictionary<string, object>>()));
        }

        internal class TestMetric : MetricBase<TestMetric>
        {
            public int? Value { get; set; }
        }
    }
}
