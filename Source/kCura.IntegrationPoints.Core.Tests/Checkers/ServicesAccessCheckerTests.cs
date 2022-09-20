using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Checkers;
using kCura.IntegrationPoints.Core.Monitoring.SystemReporter;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Checkers
{
    public class ServicesAccessCheckerTests
    {
        private Mock<IAPILog> _loggerMock;
        private Mock<IServiceHealthChecker> _databasePingReporterMock;
        private Mock<IServiceHealthChecker> _keplerPingReporterMock;
        private Mock<IServiceHealthChecker> _fileShareDiskUsageReporterMock;

        private ServicesAccessChecker _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _databasePingReporterMock = new Mock<IServiceHealthChecker>();
            _keplerPingReporterMock = new Mock<IServiceHealthChecker>();
            _fileShareDiskUsageReporterMock = new Mock<IServiceHealthChecker>();

            _databasePingReporterMock.Setup(x => x.IsServiceHealthyAsync()).ReturnsAsync(true);
            _keplerPingReporterMock.Setup(x => x.IsServiceHealthyAsync()).ReturnsAsync(true);
            _fileShareDiskUsageReporterMock.Setup(x => x.IsServiceHealthyAsync()).ReturnsAsync(true);

            _sut = new ServicesAccessChecker(
                _databasePingReporterMock.Object,
                _keplerPingReporterMock.Object,
                _fileShareDiskUsageReporterMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public async Task AreServicesHealthy_ShouldReturnHealthyState_WhenAllServicesAreHealthy()
        {
            // Act
            bool areServicesHealthy = await _sut.AreServicesHealthyAsync().ConfigureAwait(false);

            // Assert
            areServicesHealthy.ShouldBeEquivalentTo(true);
        }

        [Test]
        public async Task AreServicesHealthy_ShouldReturnNotHealthyState_WhenDatabasePingReporterIsNotHealthy()
        {
            // Arrange
            _databasePingReporterMock.Setup(x => x.IsServiceHealthyAsync()).ReturnsAsync(false);

            // Act
            bool areServicesHealthy = await _sut.AreServicesHealthyAsync().ConfigureAwait(false);

            // Assert
            areServicesHealthy.ShouldBeEquivalentTo(false);
            _loggerMock.Verify(x => x.LogError("Database is not accessible."), Times.Once);
        }

        [Test]
        public async Task AreServicesHealthy_ShouldReturnNotHealthyState_WhenKeplerPingReporterIsNotHealthy()
        {
            // Arrange
            _keplerPingReporterMock.Setup(x => x.IsServiceHealthyAsync()).ReturnsAsync(false);

            // Act
            bool areServicesHealthy = await _sut.AreServicesHealthyAsync().ConfigureAwait(false);

            // Assert
            areServicesHealthy.ShouldBeEquivalentTo(false);
            _loggerMock.Verify(x => x.LogError("Kepler service is not accessible."), Times.Once);
        }

        [Test]
        public async Task AreServicesHealthy_ShouldReturnNotHealthyState_WhenFileShareDiskUsageReporterIsNotHealthy()
        {
            // Arrange
            _fileShareDiskUsageReporterMock.Setup(x => x.IsServiceHealthyAsync()).ReturnsAsync(false);

            // Act
            bool areServicesHealthy = await _sut.AreServicesHealthyAsync().ConfigureAwait(false);

            // Assert
            areServicesHealthy.ShouldBeEquivalentTo(false);
            _loggerMock.Verify(x => x.LogError("Not all fileShares are healthy."), Times.Once);
        }
    }
}
