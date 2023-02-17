using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [Test]
        public async Task AreServicesHealthyAsync_ShouldReturnTrue_WhenAllServicesAreHealthy()
        {
            // Arrange
            const int numberOfHealthCheckers = 3;

            List<Mock<IServiceHealthChecker>> healthChecks = Enumerable
                .Range(0, numberOfHealthCheckers)
                .Select(x =>
                {
                    Mock<IServiceHealthChecker> mock = new Mock<IServiceHealthChecker>();
                    mock.Setup(m => m.IsServiceHealthyAsync()).ReturnsAsync(true);
                    return mock;
                })
                .ToList();

            ServicesAccessChecker sut = PrepareSut(healthChecks.Select(x => x.Object));

            // Act
            bool areServicesHealthy = await sut.AreServicesHealthyAsync().ConfigureAwait(false);

            // Assert
            areServicesHealthy.ShouldBeEquivalentTo(true);
            foreach (Mock<IServiceHealthChecker> healthCheck in healthChecks)
            {
                healthCheck.Verify(x => x.IsServiceHealthyAsync(), Times.Once);
            }
        }

        [Test]
        public async Task AreServicesHealthyAsync_ShouldReturnFalse_WhenOneHealthCheckFail()
        {
            // Arrange
            Mock<IServiceHealthChecker> healthyService = new Mock<IServiceHealthChecker>();
            healthyService.Setup(x => x.IsServiceHealthyAsync()).ReturnsAsync(true);

            Mock<IServiceHealthChecker> badService = new Mock<IServiceHealthChecker>();
            badService.Setup(x => x.IsServiceHealthyAsync()).ReturnsAsync(false);

            ServicesAccessChecker sut = PrepareSut(new[] { healthyService.Object, badService.Object });

            // Act
            bool areServicesHealthy = await sut.AreServicesHealthyAsync().ConfigureAwait(false);

            // Assert
            areServicesHealthy.ShouldBeEquivalentTo(false);
            healthyService.Verify(x => x.IsServiceHealthyAsync(), Times.Once);
            badService.Verify(x => x.IsServiceHealthyAsync(), Times.Once);
        }

        [Test]
        public void AreServicesHealthyAsync_ShouldNotThrow_WhenHealthCheckThrows()
        {
            // Arrange
            Mock<IServiceHealthChecker> badService = new Mock<IServiceHealthChecker>();
            badService.Setup(x => x.IsServiceHealthyAsync()).Throws<InvalidOperationException>();

            ServicesAccessChecker sut = PrepareSut(new[] { badService.Object });

            // Act
            Func<Task<bool>> action = () => sut.AreServicesHealthyAsync();

            // Assert
            action.ShouldNotThrow();
        }

        private ServicesAccessChecker PrepareSut(IEnumerable<IServiceHealthChecker> healthChecks)
        {
            return new ServicesAccessChecker(healthChecks, Mock.Of<IAPILog>());
        }
    }
}
