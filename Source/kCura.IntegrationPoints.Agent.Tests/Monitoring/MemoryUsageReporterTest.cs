using kCura.IntegrationPoints.Agent.Monitoring.MemoryUsageReporter;
using Moq;
using NUnit.Framework;
using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;
using System.Threading;

namespace kCura.IntegrationPoints.Agent.Tests.Monitoring
{
    [TestFixture, Category("Unit")]
    public class MemoryUsageReporterTest
    {
        private Mock<IAPM> _apmMock;
        private MemoryUsageReporter _sut;

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
            _sut = new MemoryUsageReporter(_apmMock.Object);
        }

        [Test]
        public void ItShouldReportMemoryUsage()
        {

        }
	}
}
