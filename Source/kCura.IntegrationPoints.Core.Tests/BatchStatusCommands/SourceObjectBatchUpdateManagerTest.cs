using System;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.BatchStatusCommands.Implementations;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Tagging;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.ScheduleQueue.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.BatchStatusCommands
{
    [TestFixture, Category("Unit")]
    public class SourceObjectBatchUpdateManagerTest : TestBase
    {
        private Mock<IScratchTableRepository> _scratchTableRepositoryMock;
        private Mock<IRepositoryFactory> _repositoryFactoryMock;
        private Mock<ISourceWorkspaceTagCreator> _sourceWorkspaceTagsCreatorMock;
        private Mock<ISourceDocumentsTagger> _sourceWorkspaceDocumentsTaggerMock;
        private Mock<IAPILog> _loggerMock;
        private SourceConfiguration _sourceConfig;
        private SourceObjectBatchUpdateManager _sut;
        private const int _JOB_HISTORY_RDO_ID = 12345;
        private const int _DESTINATION_WORKSPACE_INSTANCE_ID = 54321;
        private const int _FEDERATED_INSTANCE_ID = 134648;
        private const int _DESTINATION_WORKSPACE_ID = 99999;
        private const string _UNIQUE_JOB_ID = "1_SomeGuid";
        private readonly Job _job = null;

        [SetUp]
        public override void SetUp()
        {
            _scratchTableRepositoryMock = new Mock<IScratchTableRepository>();

            _sourceWorkspaceTagsCreatorMock = new Mock<ISourceWorkspaceTagCreator>();
            _sourceWorkspaceTagsCreatorMock
                .Setup(x => x.CreateDestinationWorkspaceTag(_DESTINATION_WORKSPACE_ID, _JOB_HISTORY_RDO_ID, _FEDERATED_INSTANCE_ID))
                .Returns(_DESTINATION_WORKSPACE_INSTANCE_ID);

            _sourceWorkspaceDocumentsTaggerMock = new Mock<ISourceDocumentsTagger>();

            _sourceConfig = new SourceConfiguration
            {
                SourceWorkspaceArtifactId = 56879,
                TargetWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID,
                FederatedInstanceArtifactId = _FEDERATED_INSTANCE_ID
            };

            _repositoryFactoryMock = new Mock<IRepositoryFactory>();
            _repositoryFactoryMock
                .Setup(
                    x => x.GetScratchTableRepository(
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(_scratchTableRepositoryMock.Object);

            _loggerMock = new Mock<IAPILog>
            {
                DefaultValue = DefaultValue.Mock
            };

            _sut = CreateSut();
        }

        [Test]
        public void Constructor_ShouldRetrieveScratchTableRepositoryUsingRepositoryFactory()
        {
            // act
            CreateSut();

            // assert
            _repositoryFactoryMock.Verify(
                x => x.GetScratchTableRepository(
                    _sourceConfig.SourceWorkspaceArtifactId,
                    Data.Constants.TEMPORARY_DOC_TABLE_SOURCE_OBJECTS,
                    _UNIQUE_JOB_ID)
                );
        }

        [Test]
        public void OnJobStart_ShouldCreateDestinationWorkspaceTag()
        {
            // act
            _sut.OnJobStart(_job);

            // assert
            _sourceWorkspaceTagsCreatorMock.Verify(
                x => x.CreateDestinationWorkspaceTag(_DESTINATION_WORKSPACE_ID, _JOB_HISTORY_RDO_ID, _FEDERATED_INSTANCE_ID));
        }

        [Test]
        public void OnJobStart_ShouldRethrowException()
        {
            // arrange
            _sourceWorkspaceTagsCreatorMock
                .Setup(x => x.CreateDestinationWorkspaceTag(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int?>()))
                .Throws<Exception>();

            // act
            Action onJobStartAction = () => _sut.OnJobStart(_job);

            // assert
            onJobStartAction.ShouldThrow<IntegrationPointsException>();
        }

        [Test]
        public void OnJobComplete_ShouldTagDocuments()
        {
            // Arrange
            _sut.OnJobStart(_job);

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            _sourceWorkspaceDocumentsTaggerMock.Verify(
                x => x.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(_scratchTableRepositoryMock.Object, _DESTINATION_WORKSPACE_INSTANCE_ID, _JOB_HISTORY_RDO_ID),
                Times.Once);
        }

        [Test]
        public void OnJobComplete_ShouldDisposeScratchTable()
        {
            // Arrange
            _sut.OnJobStart(_job);

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            _scratchTableRepositoryMock.Verify(
                x => x.Dispose(),
                Times.Once);
        }

        [Test]
        public void OnJobComplete_ShouldRethrowTaggingException()
        {
            // Arrange
            _sourceWorkspaceDocumentsTaggerMock
                .Setup(x => x.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
                    _scratchTableRepositoryMock.Object,
                    _DESTINATION_WORKSPACE_INSTANCE_ID,
                    _JOB_HISTORY_RDO_ID))
                .Throws<Exception>();

            _sut.OnJobStart(_job);

            // Act
            Action onJobCompleteAction = () => _sut.OnJobComplete(_job);

            // Assert
            onJobCompleteAction.ShouldThrow<IntegrationPointsException>();
        }

        [Test]
        public void OnJobComplete_ShouldDisposeScratchTableInCaseOfTaggingException()
        {
            // Arrange
            _sourceWorkspaceDocumentsTaggerMock
                .Setup(x => x.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
                    _scratchTableRepositoryMock.Object,
                    _DESTINATION_WORKSPACE_INSTANCE_ID,
                    _JOB_HISTORY_RDO_ID))
                .Throws<Exception>();

            _sut.OnJobStart(_job);

            // Act
            try
            {
                _sut.OnJobComplete(_job);
            }
            catch
            {
                // ignored
            }

            // Assert
            _scratchTableRepositoryMock.Verify(
                x => x.Dispose(),
                Times.Once);
        }

        [Test]
        public void OnJobComplete_ShouldNotTagDocumentsWhenOnJobStartFailed()
        {
            // Arrange
            _sourceWorkspaceTagsCreatorMock
                .Setup(x => x.CreateDestinationWorkspaceTag(_DESTINATION_WORKSPACE_ID, _JOB_HISTORY_RDO_ID, _FEDERATED_INSTANCE_ID))
                .Throws<Exception>();

            try
            {
                _sut.OnJobStart(_job);
            }
            catch
            {
                // ignored
            }

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            _sourceWorkspaceDocumentsTaggerMock.Verify(
                x => x.TagDocumentsWithDestinationWorkspaceAndJobHistoryAsync(
                    It.IsAny<IScratchTableRepository>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public void OnJobComplete_ShouldDisposeScratchTableWhenOnJobStartFailed()
        {
            // Arrange
            _sourceWorkspaceTagsCreatorMock
                .Setup(x => x.CreateDestinationWorkspaceTag(_DESTINATION_WORKSPACE_ID, _JOB_HISTORY_RDO_ID, _FEDERATED_INSTANCE_ID))
                .Throws<Exception>();

            try
            {
                _sut.OnJobStart(_job);
            }
            catch
            {
                // ignored
            }

            // Act
            _sut.OnJobComplete(_job);

            // Assert
            _scratchTableRepositoryMock.Verify(
                x => x.Dispose(),
                Times.Once);
        }

        [Test]
        public void GetScratchTableRepository_AlwaysGivesTheSameObject()
        {
            // Act
            IScratchTableRepository repository = _sut.ScratchTableRepository;
            IScratchTableRepository repository2 = _sut.ScratchTableRepository;

            // Assert
            Assert.AreSame(repository, repository2);
        }

        private SourceObjectBatchUpdateManager CreateSut()
        {
            var jobHistory = new JobHistory
            {
                ArtifactId = _JOB_HISTORY_RDO_ID,
                BatchInstance = Guid.NewGuid().ToString()
            };

            return new SourceObjectBatchUpdateManager(
                _repositoryFactoryMock.Object,
                _loggerMock.Object,
                _sourceWorkspaceTagsCreatorMock.Object,
                _sourceWorkspaceDocumentsTaggerMock.Object,
                _sourceConfig,
                jobHistory,
                _UNIQUE_JOB_ID);
        }
    }
}
