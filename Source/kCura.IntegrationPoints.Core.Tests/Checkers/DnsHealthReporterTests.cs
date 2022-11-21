using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter.DNS;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Checkers
{
    [TestFixture]
    public class DnsHealthReporterTests
    {
        private Mock<IDns> _dnsMock;

        private DnsHealthReporter _sut;

        [SetUp]
        public void SetUp()
        {
            _dnsMock = new Mock<IDns>();
            _dnsMock.Setup(x => x.GetHostEntry(It.IsAny<string>())).Returns(new IPHostEntry());

            Mock<IAPILog> logger = new Mock<IAPILog>();
            logger.Setup(x => x.ForContext<DnsHealthReporter>()).Returns(logger.Object);

            _sut = new DnsHealthReporter(_dnsMock.Object, logger.Object);
        }

        [Test]
        public async Task IsServiceHealthyAsync_ShouldCheckHardcodedDomains()
        {
            // Arrange
            string[] hostNames = new[]
            {
                "google.com",
                "microsoft.com",
                "amazon.com"
            };

            // Act
            bool actualResult = await _sut.IsServiceHealthyAsync();

            // Assert
            actualResult.Should().BeTrue();
            foreach (string hostName in hostNames)
            {
                _dnsMock.Verify(x => x.GetHostEntry(hostName), Times.Once);
            }
        }

        [Test]
        public async Task IsServiceHealthyAsync_ShouldReturnFalse_WhenAnyDomainFailToResolve()
        {
            // Arrange
            _dnsMock.Setup(x => x.GetHostEntry("google.com")).Returns(new IPHostEntry());
            _dnsMock.Setup(x => x.GetHostEntry("microsoft.com")).Throws<Exception>();

            // Act
            bool actualResult = await _sut.IsServiceHealthyAsync();

            // Assert
            actualResult.Should().BeFalse();
        }
    }
}
