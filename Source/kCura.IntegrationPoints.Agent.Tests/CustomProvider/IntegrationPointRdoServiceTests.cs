using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.CustomProvider.Services.IntegrationPointRdoService;
using kCura.IntegrationPoints.Common.Kepler;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Agent.Tests.CustomProvider
{
    [TestFixture]
    [Category("Unit")]
    public class IntegrationPointRdoServiceTests
    {
        private Mock<IKeplerServiceFactory> _keplerServiceFactory;
        private Mock<IObjectManager> _objectManager;

        private IntegrationPointRdoService _sut;

        private const int WorkspaceId = 111;
        private const int IntegrationPointId = 222;

        [SetUp]
        public void SetUp()
        {
            _objectManager = new Mock<IObjectManager>();
            _keplerServiceFactory = new Mock<IKeplerServiceFactory>();
            _keplerServiceFactory
                .Setup(x => x.CreateProxyAsync<IObjectManager>())
                .ReturnsAsync(_objectManager.Object);

            _sut = new IntegrationPointRdoService(_keplerServiceFactory.Object, Mock.Of<IAPILog>());
        }

        [Test]
        public async Task TryUpdateLastRuntimeAsync_ShouldUpdate()
        {
            // Arrange
            DateTime lastRuntime = DateTime.UtcNow;

            // Act
            await _sut.TryUpdateLastRuntimeAsync(WorkspaceId, IntegrationPointId, lastRuntime);

            // Assert
            _objectManager.Verify(x => x.UpdateAsync(WorkspaceId, It.Is<UpdateRequest>(request =>
                request.Object.ArtifactID == IntegrationPointId &&
                request.FieldValues.Single().Field.Guid == IntegrationPointFieldGuids.LastRuntimeUTCGuid &&
                (DateTime)request.FieldValues.Single().Value == lastRuntime)));
        }

        [Test]
        public void TryUpdateLastRuntimeAsync_ShouldNotThrow()
        {
            // Arrange
            _objectManager
                .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()))
                .Throws<ServiceException>();

            // Act
            Func<Task> action = () => _sut.TryUpdateLastRuntimeAsync(WorkspaceId, IntegrationPointId, DateTime.UtcNow);

            // Assert
            action.ShouldNotThrow();
        }

        [Test]
        public async Task TryUpdateHasErrorsAsync_ShouldUpdate()
        {
            // Arrange
            bool hasErrors = true;

            // Act
            await _sut.TryUpdateHasErrorsAsync(WorkspaceId, IntegrationPointId, hasErrors);

            // Assert
            _objectManager.Verify(x => x.UpdateAsync(WorkspaceId, It.Is<UpdateRequest>(request =>
                request.Object.ArtifactID == IntegrationPointId &&
                request.FieldValues.Single().Field.Guid == IntegrationPointFieldGuids.HasErrorsGuid &&
                (bool)request.FieldValues.Single().Value == hasErrors)));
        }

        [Test]
        public void TryUpdateHasErrorsAsync_ShouldNotThrow()
        {
            // Arrange
            _objectManager
                .Setup(x => x.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateRequest>()))
                .Throws<ServiceException>();

            // Act
            Func<Task> action = () => _sut.TryUpdateHasErrorsAsync(WorkspaceId, IntegrationPointId, true);

            // Assert
            action.ShouldNotThrow();
        }
    }
}
