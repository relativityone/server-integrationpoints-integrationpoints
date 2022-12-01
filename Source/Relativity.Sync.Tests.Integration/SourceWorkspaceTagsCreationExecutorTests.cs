using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    internal sealed class SourceWorkspaceTagsCreationExecutorTests
    {
        private CompositeCancellationToken _token;
        private IAPILog _logger;

        private IExecutor<ISourceWorkspaceTagsCreationConfiguration> _executor;
        private Mock<IObjectManager> _destinationObjectManagerMock;
        private Mock<IObjectManager> _sourceObjectManagerMock;
        private Mock<IWorkspaceManager> _workspaceManagerMock;

        private const int _TEST_DEST_CASE_ARTIFACT_ID = 1014854;
        private const string _TEST_DEST_CASE_NAME = "Cool Workspace";
        private const int _TEST_DEST_CASE_TAG_ARTIFACT_ID = 1031000;
        private const string _TEST_DEST_CASE_TAG_NAME = "Tag test name";
        private const int _TEST_INSTANCE_ARTIFACT_ID = 1013283;
        private const string _TEST_INSTANCE_NAME = "This Instance";
        private const int _TEST_JOB_ARTIFACT_ID = 101000;
        private const int _TEST_SOURCE_CASE_ARTIFACT_ID = 1014853;

        private readonly Guid _destinationInstanceArtifactIdFieldGuid = Guid.Parse("323458DB-8A06-464B-9402-AF2516CF47E0");
        private readonly Guid _destinationInstanceNameFieldGuid = Guid.Parse("909ADC7C-2BB9-46CA-9F85-DA32901D6554");
        private readonly Guid _destinationWorkspaceArtifactIdFieldGuid = Guid.Parse("207E6836-2961-466B-A0D2-29974A4FAD36");
        private readonly Guid _destinationWorkspaceNameFieldGuid = Guid.Parse("348D7394-2658-4DA4-87D0-8183824ADF98");
        private readonly Guid _nameFieldGuid = Guid.Parse("155649C0-DB15-4EE7-B449-BFDF2A54B7B5");

        private readonly ConfigurationStub _configurationStub = new ConfigurationStub
        {
            SourceWorkspaceArtifactId = _TEST_SOURCE_CASE_ARTIFACT_ID,
            DestinationWorkspaceArtifactId = _TEST_DEST_CASE_ARTIFACT_ID,
            JobHistoryArtifactId = _TEST_JOB_ARTIFACT_ID
        };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _token = CompositeCancellationToken.None;
            _logger = new EmptyLogger();
        }

        [SetUp]
        public void SetUp()
        {
            ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
            IntegrationTestsContainerBuilder.MockStepsExcept<ISourceWorkspaceTagsCreationConfiguration>(containerBuilder);

            _workspaceManagerMock = new Mock<IWorkspaceManager>();

            _workspaceManagerMock.Setup(x => x.ReadAsync(_TEST_DEST_CASE_ARTIFACT_ID))
                .ReturnsAsync(new WorkspaceResponse { Name = _TEST_DEST_CASE_NAME });

            _sourceObjectManagerMock = new Mock<IObjectManager>();
            var sourceServiceFactoryMock = new Mock<ISourceServiceFactoryForUser>();
            sourceServiceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_sourceObjectManagerMock.Object);
            sourceServiceFactoryMock.Setup(x => x.CreateProxyAsync<IWorkspaceManager>()).ReturnsAsync(_workspaceManagerMock.Object);

            _destinationObjectManagerMock = new Mock<IObjectManager>();
            var destinationServiceFactoryMock = new Mock<IDestinationServiceFactoryForUser>();
            destinationServiceFactoryMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_destinationObjectManagerMock.Object);
            destinationServiceFactoryMock.Setup(x => x.CreateProxyAsync<IWorkspaceManager>()).ReturnsAsync(_workspaceManagerMock.Object);

            containerBuilder.RegisterInstance(sourceServiceFactoryMock.Object).As<ISourceServiceFactoryForUser>();
            containerBuilder.RegisterInstance(destinationServiceFactoryMock.Object).As<IDestinationServiceFactoryForUser>();
            containerBuilder.RegisterType<SourceWorkspaceTagsCreationExecutor>().As<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();

            containerBuilder.RegisterInstance(_logger).As<IAPILog>();

            var syncMetrics = new Mock<ISyncMetrics>();
            containerBuilder.RegisterInstance(syncMetrics.Object).As<ISyncMetrics>();

            IContainer container = containerBuilder.Build();
            _executor = container.Resolve<IExecutor<ISourceWorkspaceTagsCreationConfiguration>>();
        }

        [Test]
        public async Task ItShouldBuildProperQueryForLocalInstance()
        {
            // Arrange
            string expectedInstanceCondition = $"'{_destinationWorkspaceArtifactIdFieldGuid}' == {_TEST_DEST_CASE_ARTIFACT_ID} AND ('{_destinationInstanceArtifactIdFieldGuid}' == -1)";

            _sourceObjectManagerMock.Setup(x => x.CreateAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.IsAny<CreateRequest>()))
                .ReturnsAsync(new CreateResult { Object = new RelativityObject { ArtifactID = _TEST_DEST_CASE_TAG_ARTIFACT_ID } })
                .Verifiable();

            _sourceObjectManagerMock.Setup(x => x.QueryAsync(
                    _TEST_SOURCE_CASE_ARTIFACT_ID,
                    It.Is<QueryRequest>(request => request.Condition == expectedInstanceCondition),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new QueryResult())
                .Verifiable();

            // Act
            ExecutionResult executionResult = await _executor.ExecuteAsync(_configurationStub, _token).ConfigureAwait(false);

            // Assert
            executionResult.Status.Should().Be(ExecutionStatus.Completed);
            Mock.Verify(_sourceObjectManagerMock);
        }

        [Test]
        public async Task ItCreatesTagIfItDoesNotExist()
        {
            // Arrange
            _sourceObjectManagerMock.Setup(x => x.QueryAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(new QueryResult());

            _sourceObjectManagerMock.Setup(x => x.CreateAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.IsAny<CreateRequest>()))
                .ReturnsAsync(new CreateResult { Object = new RelativityObject { ArtifactID = _TEST_DEST_CASE_TAG_ARTIFACT_ID } })
                .Verifiable();

            _sourceObjectManagerMock.Setup(x => x.UpdateAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.Is<UpdateRequest>(r => r.Object.ArtifactID == _TEST_JOB_ARTIFACT_ID)))
                .ReturnsAsync(new UpdateResult())
                .Verifiable();

            // Act
            await _executor.ExecuteAsync(_configurationStub, _token).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(_TEST_DEST_CASE_TAG_ARTIFACT_ID, _configurationStub.DestinationWorkspaceTagArtifactId);

            Mock.Verify(_sourceObjectManagerMock, _destinationObjectManagerMock);
        }

        [Test]
        public async Task ItUpdatesIncorrectDestinationWorkspaceNameOnExistingTag()
        {
            // Arrange
            const string expectedDestinationWorkspaceName = "Foo Bar Baz";

            _workspaceManagerMock.Setup(x => x.ReadAsync(_TEST_DEST_CASE_ARTIFACT_ID))
                .ReturnsAsync(new WorkspaceResponse { Name = expectedDestinationWorkspaceName });

            _sourceObjectManagerMock.Setup(x => x.QueryAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>
                {
                    new RelativityObject
                    {
                        ArtifactID = _TEST_DEST_CASE_TAG_ARTIFACT_ID,
                        FieldValues = BuildFieldValuePairs(_TEST_DEST_CASE_NAME, _TEST_INSTANCE_NAME)
                    }
                }
                });

            _sourceObjectManagerMock.Setup(x => x.UpdateAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.Is<UpdateRequest>(y =>
                    y.FieldValues.Any(fv => fv.Field.Guid == _destinationInstanceNameFieldGuid) &&
                    y.FieldValues.First(fv => fv.Field.Guid == _destinationWorkspaceNameFieldGuid).Value.Equals(expectedDestinationWorkspaceName))))
                .ReturnsAsync(new UpdateResult())
                .Verifiable();

            _sourceObjectManagerMock.Setup(x => x.UpdateAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.Is<UpdateRequest>(r => r.Object.ArtifactID == _TEST_JOB_ARTIFACT_ID)))
                .ReturnsAsync(new UpdateResult())
                .Verifiable();

            // Act
            await _executor.ExecuteAsync(_configurationStub, _token).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(_TEST_DEST_CASE_TAG_ARTIFACT_ID, _configurationStub.DestinationWorkspaceTagArtifactId);

            Mock.Verify(_sourceObjectManagerMock, _destinationObjectManagerMock);
        }

        [Test]
        public async Task ItUpdatesIncorrectDestinationInstanceNameOnExistingTag()
        {
            // Arrange
            _sourceObjectManagerMock.Setup(x => x.QueryAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>
                {
                    new RelativityObject
                    {
                        ArtifactID = _TEST_DEST_CASE_TAG_ARTIFACT_ID,
                        FieldValues = BuildFieldValuePairs(_TEST_DEST_CASE_NAME, "Some Other Weird Instance")
                    }
                }
                });

            _sourceObjectManagerMock
                .Setup(x => x.UpdateAsync(_TEST_SOURCE_CASE_ARTIFACT_ID, It.Is<UpdateRequest>(y =>
                    y.FieldValues.Any(fv => fv.Field.Guid == _destinationInstanceNameFieldGuid) &&
                    y.FieldValues.FirstOrDefault(fv => fv.Field.Guid == _destinationInstanceNameFieldGuid).Value.Equals(_TEST_INSTANCE_NAME))))
                .ReturnsAsync(new UpdateResult())
                .Verifiable();

            _sourceObjectManagerMock.Setup(x => x.UpdateAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.Is<UpdateRequest>(r => r.Object.ArtifactID == _TEST_JOB_ARTIFACT_ID)))
                .ReturnsAsync(new UpdateResult())
                .Verifiable();

            // Act
            await _executor.ExecuteAsync(_configurationStub, _token).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(_TEST_DEST_CASE_TAG_ARTIFACT_ID, _configurationStub.DestinationWorkspaceTagArtifactId);

            Mock.Verify(_sourceObjectManagerMock, _destinationObjectManagerMock);
        }

        [Test]
        public async Task ItDoesNotUpdateCorrectExistingTag()
        {
            // Arrange
            _sourceObjectManagerMock.Setup(x => x.QueryAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>
                {
                    new RelativityObject()
                    {
                        ArtifactID = _TEST_DEST_CASE_TAG_ARTIFACT_ID,
                        FieldValues = BuildFieldValuePairs(_TEST_DEST_CASE_NAME, _TEST_INSTANCE_NAME)
                    }
                }
                });

            _sourceObjectManagerMock.Setup(x => x.UpdateAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.Is<UpdateRequest>(r => r.Object.ArtifactID == _TEST_JOB_ARTIFACT_ID)))
                .ReturnsAsync(new UpdateResult())
                .Verifiable();

            // Act
            await _executor.ExecuteAsync(_configurationStub, _token).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(_TEST_DEST_CASE_TAG_ARTIFACT_ID, _configurationStub.DestinationWorkspaceTagArtifactId);

            Mock.Verify(_sourceObjectManagerMock, _destinationObjectManagerMock);
            _sourceObjectManagerMock.Verify(x => x.UpdateAsync(_TEST_SOURCE_CASE_ARTIFACT_ID, It.Is<UpdateRequest>(y => y.Object.ArtifactID == _TEST_DEST_CASE_TAG_ARTIFACT_ID)), Times.Never);
        }

        [Test]
        public async Task ItReturnsFailedResultIfDestinationWorkspaceDoesNotExist()
        {
            // Arrange
            _workspaceManagerMock.Setup(x => x.ReadAsync(_TEST_DEST_CASE_ARTIFACT_ID))
                .ReturnsAsync((WorkspaceResponse)null);

            // Act
            ExecutionResult result = await _executor.ExecuteAsync(_configurationStub, _token).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<SyncException>(result.Exception);
            Assert.IsTrue(result.Exception.Message.Contains(_TEST_DEST_CASE_ARTIFACT_ID.ToString(CultureInfo.InvariantCulture)));
        }

        [Test]
        public async Task ItReturnsFailedResultIfTagCreationThrows()
        {
            // Arrange
            _sourceObjectManagerMock.Setup(x => x.QueryAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(new QueryResult());

            _sourceObjectManagerMock.Setup(x => x.CreateAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.IsAny<CreateRequest>()))
                .Throws<Exception>();

            // Act
            ExecutionResult result = await _executor.ExecuteAsync(_configurationStub, _token).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<SyncKeplerException>(result.Exception);
            Assert.AreEqual(
                $"Failed to create {nameof(DestinationWorkspaceTag)} in workspace {_TEST_SOURCE_CASE_ARTIFACT_ID}",
                result.Exception.Message);
        }

        [Test]
        public async Task ItReturnsFailedResultIfTagUpdateThrows()
        {
            // Arrange
            _sourceObjectManagerMock.Setup(x => x.QueryAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(new QueryResult
                {
                    Objects = new List<RelativityObject>
                {
                    new RelativityObject()
                    {
                        ArtifactID = _TEST_DEST_CASE_TAG_ARTIFACT_ID,
                        FieldValues = BuildFieldValuePairs("Some Other Workspace", _TEST_INSTANCE_NAME)
                    }
                }
                });

            _sourceObjectManagerMock.Setup(x => x.UpdateAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.Is<UpdateRequest>(y => y.Object.ArtifactID == _TEST_DEST_CASE_TAG_ARTIFACT_ID)))
                .Throws<Exception>();

            // Act
            ExecutionResult result = await _executor.ExecuteAsync(_configurationStub, _token).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<SyncKeplerException>(result.Exception);
            Assert.AreEqual($"Failed to update {nameof(DestinationWorkspaceTag)} with id {_TEST_DEST_CASE_TAG_ARTIFACT_ID} in workspace {_TEST_SOURCE_CASE_ARTIFACT_ID}", result.Exception.Message);
        }

        [Test]
        public async Task ItReturnsFailedResultIfTagLinkingThrows()
        {
            // Arrange
            _sourceObjectManagerMock.Setup(x => x.QueryAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .ReturnsAsync(new QueryResult());

            _sourceObjectManagerMock.Setup(x => x.CreateAsync(
                    It.IsAny<int>(),
                    It.IsAny<CreateRequest>()))
                .ReturnsAsync(new CreateResult { Object = new RelativityObject { ArtifactID = _TEST_DEST_CASE_TAG_ARTIFACT_ID } });

            _sourceObjectManagerMock.Setup(x => x.UpdateAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.Is<UpdateRequest>(y => y.Object.ArtifactID == _TEST_JOB_ARTIFACT_ID)))
                .Throws<Exception>();

            // Act
            ExecutionResult result = await _executor.ExecuteAsync(_configurationStub, _token).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<DestinationWorkspaceTagsLinkerException>(result.Exception);
            Assert.AreEqual($"Failed to link {nameof(DestinationWorkspaceTag)} to Job History", result.Exception.Message);
        }

        [Test]
        public async Task ItReturnsFailedResultIfTagQueryThrows()
        {
            // Arrange
            _sourceObjectManagerMock.Setup(x => x.QueryAsync(
                _TEST_SOURCE_CASE_ARTIFACT_ID,
                It.IsAny<QueryRequest>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
                .Throws<Exception>();

            // Act
            ExecutionResult result = await _executor.ExecuteAsync(_configurationStub, _token).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<SyncKeplerException>(result.Exception);
            Assert.AreEqual($"Failed to query {nameof(DestinationWorkspaceTag)} in workspace {_TEST_SOURCE_CASE_ARTIFACT_ID}.", result.Exception.Message);
        }

        private List<FieldValuePair> BuildFieldValuePairs(string testDestinationCaseName, string testDestinationInstanceName)
        {
            var fieldValues = new Dictionary<Guid, object>
            {
                { _nameFieldGuid, _TEST_DEST_CASE_TAG_NAME },
                { _destinationWorkspaceNameFieldGuid, testDestinationCaseName },
                { _destinationInstanceNameFieldGuid, testDestinationInstanceName },
                { _destinationInstanceArtifactIdFieldGuid, _TEST_INSTANCE_ARTIFACT_ID },
                { _destinationWorkspaceArtifactIdFieldGuid, _TEST_DEST_CASE_ARTIFACT_ID }
            };

            return fieldValues
                .Select(kv => new FieldValuePair { Field = new Field { Guids = new List<Guid> { kv.Key } }, Value = kv.Value })
                .ToList();
        }
    }
}
