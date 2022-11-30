using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Unit.Storage
{
    internal class PermissionCheckConfigurationTests : ConfigurationTestBase
    {
        private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUser;
        private Mock<IObjectManager> _objectManager;
        private PermissionsCheckConfiguration _instance;
        private SyncJobParameters _sycJobParameters;

        private const int _WORKSPACE_ARTIFACT_ID = 101679;

        private static readonly Guid RelativityProviderGuid = new Guid("423b4d43-eae9-4e14-b767-17d629de4bb2");

        [SetUp]
        public void SetUp()
        {
            _serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            _objectManager = new Mock<IObjectManager>();
            _sycJobParameters = FakeHelper.CreateSyncJobParameters();
            _instance = new PermissionsCheckConfiguration(_configuration, _sycJobParameters, _serviceFactoryForUser.Object);
        }

        [Test]
        public void ShouldReturnSourceProviderArtifactIdTests()
        {
            // Arrange
            _serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

            var relativityObjects = new List<RelativityObject>
            {
                new RelativityObject()
                {
                    ArtifactID = _WORKSPACE_ARTIFACT_ID
                }
            };

            var objectManagerValue = new QueryResult
            {
                TotalCount = 1,
                Objects = relativityObjects
            };

            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 0, 1))
                .ReturnsAsync(objectManagerValue);

            // Act
            int sourceArtifactId = _instance.SourceProviderArtifactId;

            //Assert
            sourceArtifactId.Should().Be(_WORKSPACE_ARTIFACT_ID);
        }

        [Test]
        [TestCase(2)]
        [TestCase(0)]
        public void ShouldThrowExceptionTests(int totalCountValue)
        {
            // Arrange
            _serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

            var relativityObjects = new List<RelativityObject>
            {
                new RelativityObject()
                {
                    ArtifactID = _WORKSPACE_ARTIFACT_ID
                }
            };

            var objectManagerValue = new QueryResult
            {
                TotalCount = totalCountValue,
                Objects = relativityObjects
            };

            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 0, 1))
                .ReturnsAsync(objectManagerValue);

            // Act-Assert
            int actualResult;
            SyncException actualException = Assert.Throws<SyncException>(() => actualResult = _instance.SourceProviderArtifactId);
            actualException.Message.Should().Be(
                $"Error while querying for 'Relativity' provider using ObjectManager. Query returned {totalCountValue} objects, but exactly 1 was expected.");
        }

        [Test]
        public void VerifyCorrectnessOfQueryRequestTests()
        {
            // Arrange
            _serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

            var relativityObjects = new List<RelativityObject>
            {
                new RelativityObject()
                {
                    ArtifactID = _WORKSPACE_ARTIFACT_ID
                }
            };

            var objectManagerValue = new QueryResult
            {
                TotalCount = 1,
                Objects = relativityObjects
            };

            _objectManager.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<QueryRequest>(), 0, 1))
                .ReturnsAsync(objectManagerValue);

            // Act
            int sourceArtifactId = _instance.SourceProviderArtifactId;

            //Assert
            sourceArtifactId.Should().Be(_WORKSPACE_ARTIFACT_ID);
            _objectManager.Verify(x => x.QueryAsync(It.IsAny<int>(), It.Is<QueryRequest>(qr => AssertQueryRequest(qr)), 0, 1));
        }

        private bool AssertQueryRequest(QueryRequest queryRequest)
        {
            queryRequest.ObjectType.Guid.Should().Be("5be4a1f7-87a8-4cbe-a53f-5027d4f70b80");
            queryRequest.Condition.Should().Be($"'Identifier' == '{RelativityProviderGuid}'");
            return true;
        }
    }
}
