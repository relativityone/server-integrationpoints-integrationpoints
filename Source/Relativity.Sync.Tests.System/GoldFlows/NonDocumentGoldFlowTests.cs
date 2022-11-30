using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.GoldFlows
{
    [TestFixture]
    internal class NonDocumentGoldFlowTests : SystemTest
    {
        private const string EntityArtifactTypeName = "Entity";
        private const string ViewName = "Entities - Legal Hold View";
        private const string ManagerName = "My Manager";

        private readonly int _entitiesCount = 5;

        private WorkspaceRef _sourceWorkspace;
        private WorkspaceRef _destinationWorkspace;
        private int _sourceEntityArtifactTypeId;
        private int _destinationEntityArtifactTypeId;
        private int _viewArtifactId;

        protected override async Task ChildSuiteSetup()
        {
            _sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
            _destinationWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);

            await Environment.InstallCustomHelperAppAsync(_sourceWorkspace.ArtifactID).ConfigureAwait(false);
            Task installLegalHoldToSourceWorkspaceTask = Environment.InstallLegalHoldToWorkspaceAsync(_sourceWorkspace.ArtifactID);
            Task installLegalHoldToDestinationWorkspaceTask = Environment.InstallLegalHoldToWorkspaceAsync(_destinationWorkspace.ArtifactID);
            await Task.WhenAll(installLegalHoldToSourceWorkspaceTask, installLegalHoldToDestinationWorkspaceTask).ConfigureAwait(false);

            _sourceEntityArtifactTypeId = await GetArtifactTypeIdAsync(_sourceWorkspace.ArtifactID, EntityArtifactTypeName).ConfigureAwait(false);
            _destinationEntityArtifactTypeId = await GetArtifactTypeIdAsync(_destinationWorkspace.ArtifactID, EntityArtifactTypeName).ConfigureAwait(false);
            _viewArtifactId = await GetViewArtifactIdAsync(ViewName).ConfigureAwait(false);

            await PrepareSourceDataEntitiesAsync(_entitiesCount, _sourceEntityArtifactTypeId).ConfigureAwait(false);
        }

        [IdentifiedTest("C721DA78-1D27-4463-B49C-9A9E9E65F700")]
        public async Task SyncJob_Should_SyncEntities()
        {
            // Arrange
            int jobHistoryId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspace.ArtifactID, $"Sync Job {DateTime.Now:yyyy MMMM dd HH.mm.ss.fff}", CustomAppGuids.JobHistory.TypeGuid).ConfigureAwait(false);

            SyncConfigurationBuilder builder = new SyncConfigurationBuilder(
                new SyncContext(_sourceWorkspace.ArtifactID, _destinationWorkspace.ArtifactID, jobHistoryId),
                new ServicesManagerStub(), new EmptyLogger());

            int syncConfigurationId = await builder
                .ConfigureRdos(CustomAppGuids.Guids)
                .ConfigureNonDocumentSync(new NonDocumentSyncOptions(_viewArtifactId, _sourceEntityArtifactTypeId, _destinationEntityArtifactTypeId))
                .OverwriteMode(new OverwriteOptions(ImportOverwriteMode.AppendOverlay))
                .WithFieldsMapping(mappingBuilder => mappingBuilder
                    .WithIdentifier()
                    .WithField("Email", "Email")
                    .WithField("Manager", "Manager")
                )
                .SaveAsync()
                .ConfigureAwait(false);


            ContainerBuilder containerBuilder = new ContainerBuilder();
            SyncDataAndUserConfiguration syncDataAndUserConfiguration = new SyncDataAndUserConfiguration(User.ArtifactID);
            containerBuilder.RegisterInstance(syncDataAndUserConfiguration).As<IUserContextConfiguration>();

            SyncJobParameters syncJobParameters = new SyncJobParameters(syncConfigurationId, _sourceWorkspace.ArtifactID, syncDataAndUserConfiguration.ExecutingUserId, Guid.NewGuid(), Guid.Empty);

            SyncRunner syncRunner = new SyncRunner(AppSettings.RelativityUrl, new NullAPM(), Logger);

            // Act
            SyncJobState result = await syncRunner.RunAsync(syncJobParameters, User.ArtifactID).ConfigureAwait(false);

            // Assert
            result.Status.Should().Be(SyncJobStatus.Completed);

            await AssertEntityCountInDestinationAsync(_destinationWorkspace.ArtifactID, _destinationEntityArtifactTypeId,
                expectedEntityCount: _entitiesCount + 1 // +1 for manager
                       ).ConfigureAwait(false);
            List<RelativityObjectSlim> entitiesFromDesintion = await GetEntitiesFromDestinationAsync(_destinationWorkspace.ArtifactID, _destinationEntityArtifactTypeId, _entitiesCount).ConfigureAwait(false);

            AssertManagerIsLinked(_entitiesCount, entitiesFromDesintion);
            AssertEmailWasTranfered(entitiesFromDesintion);
        }

        private void AssertEmailWasTranfered(List<RelativityObjectSlim> entities)
        {
            entities
                .Select(x => x.Values[1])
                .All(x => x.ToString().Contains("@email.com"))
                .Should()
                .BeTrue("Emails were not transferred");
        }

        private async Task<List<RelativityObjectSlim>> GetEntitiesFromDestinationAsync(int workspaceId, int entityArtifactTypeId, int entitiesCount)
        {
            ObjectTypeRef entityObjectType = new ObjectTypeRef()
            {
                ArtifactTypeID = entityArtifactTypeId
            };

            using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceId,
                    new QueryRequest()
                    {
                        ObjectType = entityObjectType,
                        Fields = new[]
                        {
                            new FieldRef
                            {
                                Name = "Manager"
                            },
                            new FieldRef
                            {
                                Name = "Email"
                            }
                        },
                    }, 0, entitiesCount + 1).ConfigureAwait(false);

                return result.Objects;
            }
        }

        private static void AssertManagerIsLinked(int entitiesCount, List<RelativityObjectSlim> entities)
        {
            entities.Select(x => x.Values.FirstOrDefault()).Count(x => x != null)
                .Should().Be(entitiesCount, "Entities should be linked to manager");
        }

        private async Task AssertEntityCountInDestinationAsync(int workspaceId, int entityArtifactTypeId, int expectedEntityCount)
        {
            ObjectTypeRef entityObjectType = new ObjectTypeRef()
            {
                ArtifactTypeID = entityArtifactTypeId
            };


            using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                QueryResultSlim result = await objectManager.QuerySlimAsync(workspaceId,
                    new QueryRequest()
                    {
                        ObjectType = entityObjectType,
                    }, 0, 1).ConfigureAwait(false);

                result.TotalCount.Should().Be(expectedEntityCount);
            }
        }

        private async Task PrepareSourceDataEntitiesAsync(int entitiesCount, int artifactTypeId)
        {
            using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                ObjectTypeRef entityObjectType = new ObjectTypeRef()
                {
                    ArtifactTypeID = artifactTypeId
                };

                // Create Manager Entity
                int managerArtifactId;

                QueryResult managerQueryResult = await objectManager.QueryAsync(_sourceWorkspace.ArtifactID, new QueryRequest()
                {
                    ObjectType = entityObjectType,
                    Condition = $"'Full Name' == '{ManagerName}'"
                }, 0, 1).ConfigureAwait(false);

                if (managerQueryResult.Objects.Any())
                {
                    managerArtifactId = managerQueryResult.Objects.First().ArtifactID;
                }
                else
                {
                    CreateResult managerCreateResult = await objectManager.CreateAsync(_sourceWorkspace.ArtifactID, new CreateRequest()
                    {
                        ObjectType = entityObjectType,
                        FieldValues = new[]
                    {
                        new FieldRefValuePair()
                        {
                            Field = new FieldRef()
                            {
                                Name = "Full Name"
                            },
                            Value = ManagerName
                        },
                        new FieldRefValuePair()
                        {
                            Field = new FieldRef()
                            {
                                Name = "Email"
                            },
                            Value = "manager@email.com"
                        },
                    }
                    }).ConfigureAwait(false);

                    managerArtifactId = managerCreateResult.Object.ArtifactID;
                }

                // Create Entities linked to Manager
                FieldRef[] fields = new[]
                {
                    new FieldRef()
                    {
                        Name = "Full Name"
                    },
                    new FieldRef()
                    {
                        Name    = "Email"
                    },
                    new FieldRef()
                    {
                        Name = "Manager"
                    }
                };

                IReadOnlyList<IReadOnlyList<object>> values = Enumerable
                    .Range(0, entitiesCount)
                    .Select(i => new List<object>()
                    {
                        $"Employee {i}",
                        $"{i}@email.com",
                        new RelativityObjectRef()
                        {
                            ArtifactID = managerArtifactId
                        }
                    })
                    .ToList();

                MassCreateResult massCreateResult = await objectManager.CreateAsync(_sourceWorkspace.ArtifactID, new MassCreateRequest()
                {
                    ObjectType = entityObjectType,
                    Fields = fields,
                    ValueLists = values
                }, CancellationToken.None).ConfigureAwait(false);
            }
        }

        private async Task<int> GetArtifactTypeIdAsync(int workspaceId, string artifactTypeName)
        {
            using (var service = ServiceFactory.CreateProxy<IObjectTypeManager>())
            {
                List<ObjectTypeIdentifier> artifactTypes = await service.GetAvailableParentObjectTypesAsync(workspaceId).ConfigureAwait(false);
                ObjectTypeIdentifier artifactType = artifactTypes.FirstOrDefault(x => x.Name == artifactTypeName);

                if (artifactType == null)
                {
                    throw new Exception($"Can't find Artifact Type: {artifactTypeName}");
                }

                return artifactType.ArtifactTypeID;
            }
        }

        private async Task<int> GetViewArtifactIdAsync(string viewName)
        {
            using (IObjectManager objectManager = ServiceFactory.CreateProxy<IObjectManager>())
            {
                QueryResult queryResult = await objectManager.QueryAsync(_sourceWorkspace.ArtifactID, new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef()
                    {
                        ArtifactTypeID = (int)ArtifactType.View
                    },
                    Condition = $"'Name' == '{viewName}'"
                }, 0, 1).ConfigureAwait(false);

                if (queryResult.Objects.Count == 0)
                {
                    throw new Exception($"Can't find view: {viewName}");
                }

                return queryResult.Objects[0].ArtifactID;
            }
        }
    }
}
