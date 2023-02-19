using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class ResourcePoolManagerTests : TestBase
    {
        private ResourcePoolManager _sut;
        private Mock<IRepositoryFactory> _repositoryFactoryFake;
        private Mock<IResourcePoolRepository> _resourcePoolRepositoryMock;
        private Mock<IServicesMgr> _servicesMgrFake;
        private Mock<IWorkspaceManager> _workspaceManagerFake;
        private Mock<IHelper> _helperFake;
        private const int _RESOURCE_POOL_ID = 1234;
        private readonly WorkspaceResponse _workspaceResponse = new WorkspaceResponse()
        {
            ResourcePool = new Securable<DisplayableObjectIdentifier>(new DisplayableObjectIdentifier()
            {
                ArtifactID = _RESOURCE_POOL_ID
            })
        };

        private readonly ProcessingSourceLocationDTO _processingSourceLocation = new ProcessingSourceLocationDTO()
        {
            ArtifactId = 1,
            Location = @"\\localhost\Export"
        };

        [SetUp]
        public override void SetUp()
        {
            _resourcePoolRepositoryMock = new Mock<IResourcePoolRepository>();

            _repositoryFactoryFake = new Mock<IRepositoryFactory>();
            _repositoryFactoryFake.Setup(x => x.GetResourcePoolRepository())
                .Returns(_resourcePoolRepositoryMock.Object);

            _workspaceManagerFake = new Mock<IWorkspaceManager>();

            _servicesMgrFake = new Mock<IServicesMgr>();
            _servicesMgrFake.Setup(x => x.CreateProxy<IWorkspaceManager>(ExecutionIdentity.System))
                .Returns(_workspaceManagerFake.Object);

            Mock<IAPILog> logger = new Mock<IAPILog>();
            logger.Setup(x => x.ForContext<ResourcePoolManager>()).Returns(logger.Object);
            Mock<ILogFactory> logFactory = new Mock<ILogFactory>();
            logFactory.Setup(x => x.GetLogger()).Returns(logger.Object);
            _helperFake = new Mock<IHelper>();
            _helperFake.Setup(x => x.GetLoggerFactory()).Returns(logFactory.Object);

            _sut = new ResourcePoolManager(_repositoryFactoryFake.Object, _servicesMgrFake.Object, _helperFake.Object);
        }

        [Test]
        public void GetProcessingSourceLocation_ShouldReturnProcessingSourceLocations()
        {
            // Arrange

            _workspaceManagerFake.Setup(x => x.ReadAsync(It.IsAny<int>()))
                .ReturnsAsync(_workspaceResponse);

            var procSourceLocations = new List<ProcessingSourceLocationDTO>
            {
                _processingSourceLocation
            };
            _resourcePoolRepositoryMock.Setup(x => x.GetProcessingSourceLocationsByResourcePool(_RESOURCE_POOL_ID))
                .Returns(procSourceLocations);

            const int wkspId = 1;

            // Act
            List<ProcessingSourceLocationDTO> processingSourceLocations = _sut.GetProcessingSourceLocation(wkspId);

            // Assert
            Assert.That(processingSourceLocations, Is.Not.Null);
            Assert.That(processingSourceLocations.Count, Is.EqualTo(1));
            Assert.That(processingSourceLocations[0], Is.EqualTo(_processingSourceLocation));
        }

        [Test]
        public void GetProcessingSourceLocation_ShouldThrowException()
        {
            // Arrange

            _workspaceManagerFake.Setup(x => x.ReadAsync(It.IsAny<int>()))
                .Throws<NotFoundException>();

            const int wkspId = 0;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _sut.GetProcessingSourceLocation(wkspId));
            _resourcePoolRepositoryMock.Verify(x => x.GetProcessingSourceLocationsByResourcePool(It.IsAny<int>()), Times.Never);
        }
    }
}
