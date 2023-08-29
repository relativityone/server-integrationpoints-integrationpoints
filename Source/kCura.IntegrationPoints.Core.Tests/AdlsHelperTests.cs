using System;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Core.AdlsHelpers;
using kCura.IntegrationPoints.Core.Storage;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Environment.V1.Workspace;
using Relativity.Services.Exceptions;

namespace kCura.IntegrationPoints.Core.Tests
{
    [TestFixture, Category("Unit")]
    public class AdlsHelperTests
    {
        private Mock<IKeplerServiceFactory> _keplerServiceFactory;
        private Mock<IWorkspaceManager> _workspaceManager;
        private Mock<IRelativityStorageService> _storageService;

        private AdlsHelper _sut;

        [SetUp]
        public void SetUp()
        {
            _workspaceManager = new Mock<IWorkspaceManager>();
            _keplerServiceFactory = new Mock<IKeplerServiceFactory>();
            _keplerServiceFactory
                .Setup(x => x.CreateProxyAsync<IWorkspaceManager>())
                .ReturnsAsync(_workspaceManager.Object);
            _storageService = new Mock<IRelativityStorageService>();

            _sut = new AdlsHelper(_keplerServiceFactory.Object, _storageService.Object, Mock.Of<IAPILog>());
        }

        [Test]
        public void IsWorkspaceMigratedToAdlsAsync_ShouldNotThrow()
        {
            // Arrange
            _workspaceManager
                .Setup(x => x.ReadAsync(It.IsAny<int>()))
                .Throws<ServiceException>();

            // Act
            Func<Task> action = () => _sut.IsWorkspaceMigratedToAdlsAsync(111);

            // Assert
            action.ShouldNotThrow();
        }

        [Test]
        public void LogFileSharesSummaryAsync_ShouldNotThrow()
        {
            // Arrange
            _storageService
                .Setup(x => x.GetStorageEndpointsAsync())
                .Throws<ServiceException>();

            // Act
            Func<Task> action = () => _sut.LogFileSharesSummaryAsync();

            // Assert
            action.ShouldNotThrow();
        }
    }
}