using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    internal sealed class DestinationWorkspaceTagsCreationExecutorTests
    {
        private IExecutor<IDestinationWorkspaceTagsCreationConfiguration> _executor;
        private Mock<IWorkspaceManager> _workspaceManagerMock;
        private Mock<IObjectManager> _objectManagerMock;

        private static readonly Guid SourceCaseTagObjectTypeGuid = new Guid("7E03308C-0B58-48CB-AFA4-BB718C3F5CAC");
        private static readonly Guid SourceJobTagObjectType = new Guid("6f4dd346-d398-4e76-8174-f0cd8236cbe7");
        private static readonly Guid CaseIdFieldGuid = new Guid("90c3472c-3592-4c5a-af01-51e23e7f89a5");
        private static readonly Guid InstanceNameFieldGuid = new Guid("C5212F20-BEC4-426C-AD5C-8EBE2697CB19");
        private static readonly Guid SourceWorkspaceNameFieldGuid = new Guid("a16f7beb-b3b0-4658-bb52-1c801ba920f0");

        [SetUp]
        public void SetUp()
        {
            ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
            IntegrationTestsContainerBuilder.MockStepsExcept<IDestinationWorkspaceTagsCreationConfiguration>(containerBuilder);

            _workspaceManagerMock = new Mock<IWorkspaceManager>();
            _objectManagerMock = new Mock<IObjectManager>();
            var destinationServiceFactoryForUser = new Mock<IDestinationServiceFactoryForUser>();
            var sourceServiceFactoryForUserMock = new Mock<ISourceServiceFactoryForUser>();
            destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IWorkspaceManager>()).Returns(Task.FromResult(_workspaceManagerMock.Object));
            sourceServiceFactoryForUserMock.Setup(x => x.CreateProxyAsync<IWorkspaceManager>()).Returns(Task.FromResult(_workspaceManagerMock.Object));
            
            destinationServiceFactoryForUser.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));
            sourceServiceFactoryForUserMock.Setup(x => x.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(_objectManagerMock.Object));

            containerBuilder.RegisterInstance(destinationServiceFactoryForUser.Object).As<IDestinationServiceFactoryForUser>();
            containerBuilder.RegisterInstance(sourceServiceFactoryForUserMock.Object).As<ISourceServiceFactoryForUser>();
            containerBuilder.RegisterType<DestinationWorkspaceTagsCreationExecutor>().As<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();

            containerBuilder.RegisterInstance(new EmptyLogger()).As<IAPILog>();

            IContainer container = containerBuilder.Build();
            _executor = container.Resolve<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();
        }

        [Test]
        public void ItShouldCreateSourceCaseTagIfItDoesNotExist()
        {
            // Arrange
            const int sourceWorkspaceArtifactId = 1014853;
            const int destinationWorkspaceArtifactId = 1014854;
            const int jobArtifactId = 101000;
            const int newSourceWorkspaceTagArtifactId = 103000;
            const int newSourceJobTagArtifactId = 103000;
            string sourceWorkspaceName = "Cool Workspace";
            string destinationWorkspaceName = "Even Cooler Workspace";
            string jobHistoryName = "Cool Job";

            var configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
                JobHistoryArtifactId = jobArtifactId,
            };

            _workspaceManagerMock.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId)).ReturnsAsync(
                new WorkspaceResponse
                {
                    Name = sourceWorkspaceName
                });

            _workspaceManagerMock.Setup(x => x.ReadAsync(destinationWorkspaceArtifactId)).ReturnsAsync(
                new WorkspaceResponse
                {
                    Name = destinationWorkspaceName
                });

            _objectManagerMock.Setup(x => x.QueryAsync(
                destinationWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
                0,
                1)
            ).Returns(Task.FromResult(new QueryResult()));
            
            _objectManagerMock.Setup(x => x.CreateAsync(
                destinationWorkspaceArtifactId,
                It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)))
            ).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceWorkspaceTagArtifactId } })
            ).Verifiable();

            _objectManagerMock.Setup(x => x.QueryAsync(
                sourceWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
                0,
                1
            )).Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = jobHistoryName } } }));

            _objectManagerMock.Setup(x => x.CreateAsync(
                destinationWorkspaceArtifactId,
                It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceJobTagObjectType))
            )).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceJobTagArtifactId } }));

            SetupObjectManagerWithNonExistingSourceJobTag(destinationWorkspaceArtifactId);

            // Act
            _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

            // Assert
            _workspaceManagerMock.Verify();
            Assert.AreEqual(newSourceWorkspaceTagArtifactId, configuration.SourceWorkspaceTagArtifactId);
            Assert.AreEqual(newSourceJobTagArtifactId, configuration.SourceJobTagArtifactId);
        }

        [Test]
        public void ItShouldUpdateSourceCaseTagIfItDoesExist()
        {
            // Arrange
            const int sourceWorkspaceArtifactId = 1014853;
            const int destinationWorkspaceArtifactId = 1014854;
            const int jobArtifactId = 101000;
            const int sourceWorkspaceTagArtifactId = 103000;
            const int newSourceJobTagArtifactId = 103001;
            string oldSourceWorkspaceName = "Not Cool Workspace";
            string newSourceWorkspaceName = "Cool Workspace";
            string destinationWorkspaceName = "Even Cooler Workspace";
            string jobHistoryName = "Cool Job";
            string oldSourceWorkspaceTagName = "Super tag";

            var configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
                JobHistoryArtifactId = jobArtifactId,
            };

            _workspaceManagerMock.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId))
                .ReturnsAsync(new WorkspaceResponse {Name = newSourceWorkspaceName});

            
            _workspaceManagerMock.Setup(x => x.ReadAsync(destinationWorkspaceArtifactId))
                .ReturnsAsync(new WorkspaceResponse {Name = destinationWorkspaceName});
        

            _objectManagerMock.Setup(x => x.QueryAsync(
                destinationWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
                0,
                1)
            ).Returns(Task.FromResult(new QueryResult
            {
                ObjectType = new ObjectType
                {
                    Guids = new List<Guid>() { SourceCaseTagObjectTypeGuid }
                },
                Objects = new List<RelativityObject>()
                {
                    new RelativityObject()
                    {
                        Name = oldSourceWorkspaceTagName,
                        ArtifactID = sourceWorkspaceTagArtifactId,
                        FieldValues = new List<FieldValuePair>()
                        {
                            new FieldValuePair()
                            {
                                Field = new Field() {Guids = new List<Guid> { CaseIdFieldGuid }},
                                Value = sourceWorkspaceArtifactId
                            },
                            new FieldValuePair()
                            {
                                Field = new Field() {Guids = new List<Guid> { SourceWorkspaceNameFieldGuid }},
                                Value = oldSourceWorkspaceName
                            },
                            new FieldValuePair()
                            {
                                Field = new Field() {Guids = new List<Guid> { InstanceNameFieldGuid }},
                                Value = "This Instance"
                            }
                        }
                    }
                }
            }));

            _objectManagerMock.Setup(x => x.UpdateAsync(
                destinationWorkspaceArtifactId,
                It.Is<UpdateRequest>(y => VerifyUpdateRequest(y, sourceWorkspaceTagArtifactId,
                    pair => pair.Field.Guid.Equals(CaseIdFieldGuid) && pair.Value.Equals(sourceWorkspaceArtifactId),
                    pair => pair.Field.Guid.Equals(SourceWorkspaceNameFieldGuid) && pair.Value.Equals(newSourceWorkspaceName)))
                )).Returns(Task.FromResult(new UpdateResult())).Verifiable();

            _objectManagerMock.Setup(x => x.QueryAsync(
                sourceWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
                0,
                1
            )).Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = jobHistoryName } } }));

            _objectManagerMock.Setup(x => x.CreateAsync(
                destinationWorkspaceArtifactId,
                It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceJobTagObjectType))
            )).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceJobTagArtifactId } }));

            SetupObjectManagerWithNonExistingSourceJobTag(destinationWorkspaceArtifactId);

            // Act
            _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

            // Assert
            _objectManagerMock.Verify(
                x => x.CreateAsync(destinationWorkspaceArtifactId,
                    It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid))), Times.Never);

            _objectManagerMock.Verify();
            Assert.AreEqual(sourceWorkspaceTagArtifactId, configuration.SourceWorkspaceTagArtifactId);
            Assert.AreEqual(newSourceJobTagArtifactId, configuration.SourceJobTagArtifactId);
        }

        [Test]
        public void ItShouldNotUpdateSourceCaseTagIfDoesNotNeedTo()
        {
            // Arrange
            const int sourceWorkspaceArtifactId = 1014853;
            const int destinationWorkspaceArtifactId = 1014854;
            const int jobArtifactId = 101000;
            const int sourceWorkspaceTagArtifactId = 103000;
            const int sourceJobTagArtifactId = 103001;
            string sourceWorkspaceName = "Not Cool Workspace";
            string destinationWorkspaceName = "Even Cooler Workspace";
            string jobHistoryName = "Cool Job";
            string sourceWorkspaceTagName = "Super tag";

            var configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
                JobHistoryArtifactId = jobArtifactId,
            };

            _workspaceManagerMock.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId))
                .ReturnsAsync(new WorkspaceResponse {Name = sourceWorkspaceName});

            _workspaceManagerMock.Setup(x => x.ReadAsync(destinationWorkspaceArtifactId))
                .ReturnsAsync(new WorkspaceResponse {Name = destinationWorkspaceName});

            _objectManagerMock.Setup(x => x.QueryAsync(
                destinationWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
                0,
                1)
            ).Returns(Task.FromResult(new QueryResult
            {
                ObjectType = new ObjectType
                {
                    Guids = new List<Guid>() { SourceCaseTagObjectTypeGuid }
                },
                Objects = new List<RelativityObject>()
                {
                    new RelativityObject()
                    {
                        Name = sourceWorkspaceTagName,
                        ArtifactID = sourceWorkspaceTagArtifactId,
                        FieldValues = new List<FieldValuePair>()
                        {
                            new FieldValuePair()
                            {
                                Field = new Field() {Guids = new List<Guid> { CaseIdFieldGuid }},
                                Value = sourceWorkspaceArtifactId
                            },
                            new FieldValuePair()
                            {
                                Field = new Field() {Guids = new List<Guid> { SourceWorkspaceNameFieldGuid }},
                                Value = sourceWorkspaceName
                            },
                            new FieldValuePair()
                            {
                                Field = new Field() {Guids = new List<Guid> { InstanceNameFieldGuid }},
                                Value = "This Instance"
                            }
                        }
                    }
                }
            }));

            _objectManagerMock.Verify(x => x.UpdateAsync(
                destinationWorkspaceArtifactId,
                It.Is<UpdateRequest>(y => VerifyUpdateRequest(y, sourceWorkspaceTagArtifactId))
                ), Times.Never);

            _objectManagerMock.Setup(x => x.QueryAsync(
                sourceWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
                0,
                1
            )).Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = jobHistoryName } } }));

            _objectManagerMock.Setup(x => x.CreateAsync(
                destinationWorkspaceArtifactId,
                It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceJobTagObjectType))
            )).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = sourceJobTagArtifactId } }));

            SetupObjectManagerWithNonExistingSourceJobTag(destinationWorkspaceArtifactId);

            // Act
            _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

            // Assert
            _objectManagerMock.Verify(
                x => x.CreateAsync(destinationWorkspaceArtifactId,
                    It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid))), Times.Never);

            _objectManagerMock.Verify();
            Assert.AreEqual(sourceWorkspaceTagArtifactId, configuration.SourceWorkspaceTagArtifactId);
            Assert.AreEqual(sourceJobTagArtifactId, configuration.SourceJobTagArtifactId);
        }

        private static bool VerifyUpdateRequest(UpdateRequest request, int tagArtifactId, params Predicate<FieldRefValuePair>[] predicates)
        {
            List<FieldRefValuePair> fields = request.FieldValues.ToList();

            return request.Object.ArtifactID == tagArtifactId && predicates.All(fields.Exists);
        }

        [Test]
        public async Task ItReturnsFailedResultIfSourceWorkspaceNameQueryThrows()
        {
            // Arrange
            const int sourceWorkspaceArtifactId = 1014853;
            const int destinationWorkspaceArtifactId = 1014854;
            const int jobArtifactId = 101000;

            var configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
                JobHistoryArtifactId = jobArtifactId,
            };

            _workspaceManagerMock.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId)).Throws<ServiceException>();

            // Act
            ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<ServiceException>(result.Exception);
        }

        [Test]
        public async Task ItReturnsFailedResultIfSourceWorkspaceTagQueryThrows()
        {
            // Arrange
            const int sourceWorkspaceArtifactId = 1014853;
            const int destinationWorkspaceArtifactId = 1014854;
            const int jobArtifactId = 101000;
            string sourceWorkspaceName = "Cool Workspace";

            var configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
                JobHistoryArtifactId = jobArtifactId,
            };

            _workspaceManagerMock.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId))
                .ReturnsAsync(new WorkspaceResponse {Name = sourceWorkspaceName});

            _objectManagerMock.Setup(x => x.QueryAsync(
                destinationWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
                0,
                1)
            ).Throws<ServiceException>();

            // Act
            ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<RelativitySourceCaseTagRepositoryException>(result.Exception);
            Assert.IsNotNull(result.Exception.InnerException);
            Assert.IsInstanceOf<ServiceException>(result.Exception.InnerException);
        }

        [Test]
        public async Task ItReturnsFailedResultIfSourceWorkspaceTagCreationThrows()
        {
            // Arrange
            const int sourceWorkspaceArtifactId = 1014853;
            const int destinationWorkspaceArtifactId = 1014854;
            const int jobArtifactId = 101000;
            string sourceWorkspaceName = "Cool Workspace";

            var configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
                JobHistoryArtifactId = jobArtifactId,
            };

            _workspaceManagerMock.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId))
                .ReturnsAsync(new WorkspaceResponse {Name = sourceWorkspaceName});

            _objectManagerMock.Setup(x => x.QueryAsync(
                destinationWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
                0,
                1)
            ).Returns(Task.FromResult(new QueryResult()));

            _objectManagerMock.Setup(x => x.CreateAsync(
                destinationWorkspaceArtifactId,
                It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)))
            ).Throws<ServiceException>();

            // Act
            ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<RelativitySourceCaseTagRepositoryException>(result.Exception);
            Assert.IsNotNull(result.Exception.InnerException);
            Assert.IsInstanceOf<ServiceException>(result.Exception.InnerException);
        }

        [Test]
        public async Task ItReturnsFailedResultIfJobHistoryQueryThrows()
        {
            // Arrange
            const int sourceWorkspaceArtifactId = 1014853;
            const int destinationWorkspaceArtifactId = 1014854;
            const int jobArtifactId = 101000;
            const int newSourceWorkspaceTagArtifactId = 103000;
            string sourceWorkspaceName = "Cool Workspace";

            var configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
                JobHistoryArtifactId = jobArtifactId,
            };
            
            _workspaceManagerMock.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId))
                .ReturnsAsync(new WorkspaceResponse {Name = sourceWorkspaceName});

        
            _objectManagerMock.Setup(x => x.QueryAsync(
                destinationWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
                0,
                1)
            ).Returns(Task.FromResult(new QueryResult()));

            _objectManagerMock.Setup(x => x.CreateAsync(
                destinationWorkspaceArtifactId,
                It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)))
            ).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceWorkspaceTagArtifactId } })
            ).Verifiable();
            
            _objectManagerMock.Setup(x => x.QueryAsync(
                sourceWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
                0,
                1
            )).Throws<ServiceException>();

            SetupObjectManagerWithNonExistingSourceJobTag(destinationWorkspaceArtifactId);

            // Act
            ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<ServiceException>(result.Exception);
        }

        [Test]
        public async Task ItReturnsFailedResultIfSourceJobTagCreationThrows()
        {
            // Arrange
            const int sourceWorkspaceArtifactId = 1014853;
            const int destinationWorkspaceArtifactId = 1014854;
            const int jobArtifactId = 101000;
            const int newSourceWorkspaceTagArtifactId = 103000;
            string sourceWorkspaceName = "Cool Workspace";
            string jobHistoryName = "Cool Job";

            var configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
                JobHistoryArtifactId = jobArtifactId,
            };

            _workspaceManagerMock.Setup(x => x.ReadAsync(sourceWorkspaceArtifactId))
                .ReturnsAsync(new WorkspaceResponse {Name = sourceWorkspaceName});

            _objectManagerMock.Setup(x => x.QueryAsync(
                destinationWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)),
                0,
                1)
            ).Returns(Task.FromResult(new QueryResult()));

            _objectManagerMock.Setup(x => x.CreateAsync(
                destinationWorkspaceArtifactId,
                It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceCaseTagObjectTypeGuid)))
            ).Returns(Task.FromResult(new CreateResult { Object = new RelativityObject { ArtifactID = newSourceWorkspaceTagArtifactId } })
            ).Verifiable();

            _objectManagerMock.Setup(x => x.QueryAsync(
                sourceWorkspaceArtifactId,
                It.Is<QueryRequest>(y => y.Condition.Contains(jobArtifactId.ToString(CultureInfo.InvariantCulture))),
                0,
                1
            )).Returns(Task.FromResult(new QueryResult() { Objects = new List<RelativityObject> { new RelativityObject() { Name = jobHistoryName } } }));

            _objectManagerMock.Setup(x => x.CreateAsync(
                destinationWorkspaceArtifactId,
                It.Is<CreateRequest>(y => y.ObjectType.Guid.Equals(SourceJobTagObjectType))
            )).Throws<ServiceException>();

            SetupObjectManagerWithNonExistingSourceJobTag(destinationWorkspaceArtifactId);

            // Act
            ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<RelativitySourceJobTagRepositoryException>(result.Exception);
            Assert.IsNotNull(result.Exception.InnerException);
            Assert.IsInstanceOf<ServiceException>(result.Exception.InnerException);
        }

        [Test]
        public async Task ItReturnsFailedResultIfDestinationWorkspaceDoesNotExist()
        {
            // Arrange
            const int sourceWorkspaceArtifactId = 1014853;
            const int destinationWorkspaceArtifactId = 1014853;
            const int jobArtifactId = 101000;
            var configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
                JobHistoryArtifactId = jobArtifactId
            };

            _objectManagerMock.Setup(x => x.QueryAsync(
                    -1,
                    It.Is<QueryRequest>(y => y.Condition.Contains(destinationWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                    ))
                    .Returns(Task.FromResult(new QueryResult()));

            // Act
            ExecutionResult result =
                await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<SyncException>(result.Exception);
            Assert.IsTrue(result.Exception.Message.Contains(destinationWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture)));
        }

        [Test]
        public async Task ItReturnsFailedResultIfSourceWorkspaceDoesNotExist()
        {
            // Arrange
            const int sourceWorkspaceArtifactId = 1014853;
            const int destinationWorkspaceArtifactId = 1014853;
            const int jobArtifactId = 101000;
            var configuration = new ConfigurationStub
            {
                SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
                DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
                JobHistoryArtifactId = jobArtifactId
            };

            _objectManagerMock.Setup(x => x.QueryAsync(
                    -1,
                    It.Is<QueryRequest>(y => y.Condition.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture))),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                    ))
                    .Returns(Task.FromResult(new QueryResult()));

            // Act
            ExecutionResult result = await _executor.ExecuteAsync(configuration, CompositeCancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(ExecutionStatus.Failed, result.Status);
            Assert.IsNotNull(result.Exception);
            Assert.IsInstanceOf<SyncException>(result.Exception);
            Assert.IsTrue(result.Exception.Message.Contains(sourceWorkspaceArtifactId.ToString(CultureInfo.InvariantCulture)));
        }

        private void SetupObjectManagerWithNonExistingSourceJobTag(int workspaceArtifactId)
        {
            _objectManagerMock.Setup(x => x.QueryAsync(
                    workspaceArtifactId,
                    It.Is<QueryRequest>(q =>
                        q.ObjectType.Guid == SourceJobTagObjectType),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .ReturnsAsync(new QueryResult() { TotalCount = 0 });
        }
    }
}
