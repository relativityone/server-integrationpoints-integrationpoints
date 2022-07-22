using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Telemetry;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Unit
{
    [TestFixture]
    public sealed class APMClientTests
    {
        [Test]
        public void Count_LogsCorrectData()
        {
            string metricName = "FooBar";
            var metricData = new Dictionary<string, object> { { "Blech", 1 }, { "Blorz", "Blech" } };

            Mock<IAPM> apmMock = new Mock<IAPM>();
            Mock<ICounterMeasure> counterMock = new Mock<ICounterMeasure>();
            apmMock.Setup(a => a.CountOperation(
                It.Is<string>(s => s.Equals(metricName, StringComparison.InvariantCulture)),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.Is<Dictionary<string, object>>(d => Enumerable.SequenceEqual(d, metricData)),
                It.IsAny<IEnumerable<ISink>>())
            ).Returns(counterMock.Object).Verifiable();

            // Act
            var client = new APMClient(apmMock.Object);
            client.Count(metricName, metricData);

            // Assert
            apmMock.Verify();
            counterMock.Verify(c => c.Write(), Times.Once);
        }

        [Test]
        public void Gauge_LogsCorrectData()
        {
            string metricName = "FooBar";
            string correlationId = "correlation";
            var metricData = new Dictionary<string, object> { { "Blech", 1 }, { "Blorz", "Blech" } };

            Mock<IGaugeMeasure> gaugeMeasure = new Mock<IGaugeMeasure>();
            Mock<IAPM> apmMock = new Mock<IAPM>();

            apmMock.Setup(a => a.GaugeOperation(
                It.Is<string>(s => s.Equals(metricName, StringComparison.InvariantCulture)),
                It.IsAny<Func<int>>(),
                It.IsAny<Guid>(),
                correlationId,
                It.IsAny<string>(),
                It.Is<Dictionary<string, object>>(d => Enumerable.SequenceEqual(d, metricData)),
                It.IsAny<IEnumerable<ISink>>())
            ).Returns(gaugeMeasure.Object).Verifiable();

            // Act
            var client = new APMClient(apmMock.Object);
            client.Gauge(metricName, correlationId, metricData);

            // Assert
            apmMock.Verify();
            gaugeMeasure.Verify(c => c.Write(), Times.Once);
        }
    }
}
