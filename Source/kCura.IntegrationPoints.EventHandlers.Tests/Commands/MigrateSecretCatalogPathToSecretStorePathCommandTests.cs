using System;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.Commands;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Context;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class MigrateSecretCatalogPathToSecretStorePathCommandTests
    {
        private MigrateSecretCatalogPathToSecretStorePathCommand _sut;

        private Mock<IWorkspaceContext> _workspaceContextMock;
        private Mock<IAPILog> _apiLogMock;
        private Mock<IRelativityObjectManager> _relativityObjectManagerMock;
        private Mock<ISecretStoreMigrationService> _migrationServiceMock;
        private List<Data.IntegrationPoint> _integrationPoints;
        private const int _WORKSPACE_ID = 123321;
        private const int _INTEGRATION_POINT_ARTIFACT_ID = 555555;
        private const string _SECURED_CONFIGURATION_DEFAULT = "SecuredConfiguration";

        [SetUp]
        public void SetUp()
        {
            _relativityObjectManagerMock = new Mock<IRelativityObjectManager>();
            _relativityObjectManagerMock.Setup(x => x.Query<Data.IntegrationPoint>(
                It.IsAny<QueryRequest>(),
                ExecutionIdentity.CurrentUser)
            ).Returns(_integrationPoints);

            _migrationServiceMock = new Mock<ISecretStoreMigrationService>();
            _workspaceContextMock = new Mock<IWorkspaceContext>();
            _apiLogMock = new Mock<IAPILog>();
            _apiLogMock
                .Setup(x => x.ForContext<MigrateSecretCatalogPathToSecretStorePathCommand>())
                .Returns(_apiLogMock.Object);
            _workspaceContextMock
                .Setup(x => x.GetWorkspaceID())
                .Returns(_WORKSPACE_ID);
            _sut = new MigrateSecretCatalogPathToSecretStorePathCommand(
                _relativityObjectManagerMock.Object, 
                _migrationServiceMock.Object,
                _workspaceContextMock.Object,
                _apiLogMock.Object
            );
        }

        [Test]
        public void ShouldNotCallMigrateSecretAndLogInformationIfThereIsNoIntegrationPoints()
        {
            //ARRANGE
            _integrationPoints = new List<Data.IntegrationPoint>();
            _relativityObjectManagerMock.Setup(x => x.Query<Data.IntegrationPoint>(
                It.IsAny<QueryRequest>(),
                ExecutionIdentity.CurrentUser)
            ).Returns(_integrationPoints);

            //ACT
            _sut.Execute();

            //ASSERT
            VerifyMigrateSecretHasNotBeenCalled();
            VerifyIfLoggerHasBeenCalled();
        }

        [Test]
        public void ShouldCallMigrateSecretOnce()
        {
            //ARRANGE
            Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint()
            {
                SecuredConfiguration = _SECURED_CONFIGURATION_DEFAULT,
                ArtifactId = _INTEGRATION_POINT_ARTIFACT_ID
            };
            _integrationPoints = new List<Data.IntegrationPoint> { integrationPoint };
            _relativityObjectManagerMock.Setup(x => x.Query<Data.IntegrationPoint>(
                It.IsAny<QueryRequest>(), 
                ExecutionIdentity.CurrentUser)
            ).Returns(_integrationPoints);

            //ACT
            _sut.Execute();

            //ASSERT
            _migrationServiceMock.Verify(x => x.TryMigrateSecret(
                    _WORKSPACE_ID,
                    _INTEGRATION_POINT_ARTIFACT_ID, 
                    _SECURED_CONFIGURATION_DEFAULT), Times.Once
            );
        }

        [Test]
        public void ShouldCallMigrateSecretOnceForEveryIntegrationPoint()
        {
            //ARRANGE
            int[] integrationPointIDs = { 12345, 44123, 121212, 32132 };
            _integrationPoints = CreateListOfIntegrationPointsWithArtifactIDs(integrationPointIDs).ToList();
            _relativityObjectManagerMock
                .Setup(x => x.Query<Data.IntegrationPoint>(
                    It.IsAny<QueryRequest>(), 
                    ExecutionIdentity.CurrentUser))
                .Returns(_integrationPoints);

            //ACT
            _sut.Execute();

            //ASSERT
            foreach (int integrationPointID in integrationPointIDs)
            {
                VerifyMigrateSecretHasBeenCalledWithProperIntegrationPointID(integrationPointID);
            }
        }

        [Test]
        public void ShouldThrowWhenObjectManagerThrows()
        {
            //ARRANGE
            _relativityObjectManagerMock
                .Setup(x => x.Query<Data.IntegrationPoint>(
                    It.IsAny<QueryRequest>(),
                    ExecutionIdentity.CurrentUser))
                .Throws<Exception>();

            //ACT
            Action action = () => _sut.Execute();

            //ASSERT
            action.ShouldThrow<Exception>();
            VerifyMigrateSecretHasNotBeenCalled();
        }

        [Test]
        public void ShouldThrowWhenMigrationSecretServiceThrows()
        {
            //ARRANGE
            Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint
            {
                SecuredConfiguration = _SECURED_CONFIGURATION_DEFAULT,
                ArtifactId = _INTEGRATION_POINT_ARTIFACT_ID
            };
            _integrationPoints = new List<Data.IntegrationPoint> { integrationPoint };
            _relativityObjectManagerMock.Setup(x => x.Query<Data.IntegrationPoint>(
                It.IsAny<QueryRequest>(),
                ExecutionIdentity.CurrentUser)
            ).Returns(_integrationPoints);
            _migrationServiceMock
                .Setup(x => x.TryMigrateSecret(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .Throws<Exception>();

            //ACT
            Action action = () => _sut.Execute();

            //ASSERT
            action.ShouldThrow<Exception>();
        }

        private void VerifyMigrateSecretHasBeenCalledWithProperIntegrationPointID(int integrationPointID)
        {
            _migrationServiceMock.Verify(x => x.TryMigrateSecret(
                    _WORKSPACE_ID,
                    integrationPointID, 
                    _SECURED_CONFIGURATION_DEFAULT), 
                Times.Once
            );
        }

        private void VerifyMigrateSecretHasNotBeenCalled()
        {
            _migrationServiceMock.Verify(x => x.TryMigrateSecret(
                    It.IsAny<int>(),
                    It.IsAny<int>(), 
                    It.IsAny<string>()), 
                Times.Never
            );
        }

        private void VerifyIfLoggerHasBeenCalled()
        {
            _apiLogMock.Verify(
                x => x.LogInformation("There was no integration point in a given workspace ({workspaceID}) that needs to be migrated to Secret Store.", _WORKSPACE_ID),
                Times.Once
            );
        }

        private IEnumerable<Data.IntegrationPoint> CreateListOfIntegrationPointsWithArtifactIDs(int[] integrationPointIDs)
        {
            return integrationPointIDs.Select(
                integrationPointID => new Data.IntegrationPoint
                {
                    ArtifactId = integrationPointID,
                    SecuredConfiguration = _SECURED_CONFIGURATION_DEFAULT
                }
            );
        }
    }
}
