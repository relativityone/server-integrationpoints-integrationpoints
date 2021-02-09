using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.RDOs;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.SyncConfiguration
{
    internal class SyncConfigurationRdoCreationTests : SystemTest
    {
        private readonly Guid _jobHistoryTypeGuid = Guid.Parse("1B405B4F-8C9E-4B07-8170-6A47D0C4E579");


        public ISyncServiceManager SyncServicesMgr = new ServicesManagerStub();


        [IdentifiedTest("B06A5312-4A3F-4F8F-8545-64401766AA6B")]
        public async Task Exists_ShouldReturnTrue_AfterRdoIsCreated()
        {
            // Arrange
            WorkspaceRef testWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);

            (_,int parentObjectTypeId) = await Rdos.CreateBasicRdoTypeAsync(ServiceFactory, testWorkspace.ArtifactID,
                $"{Guid.NewGuid()}",
                new ObjectTypeIdentifier {ArtifactTypeID = (int) ArtifactType.Case}).ConfigureAwait(false);

            await SyncConfigurationRdo.EnsureTypeExists(testWorkspace.ArtifactID, parentObjectTypeId, SyncServicesMgr)
                .ConfigureAwait(false);

            (SyncConfigurationRdo.RdoStatus rdoStatus, HashSet<Guid> existingFieldsGuids, int? existingArtifactId) =
                await SyncConfigurationRdo.ExistsAsync(testWorkspace.ArtifactID, SyncServicesMgr).ConfigureAwait(false);

            // Assert
            rdoStatus.Should().Be(SyncConfigurationRdo.RdoStatus.Exists);
            existingArtifactId.Should().NotBeNull();
            SyncConfigurationRdo.GuidNames.Keys.Should().Contain(existingFieldsGuids);
        }

        [IdentifiedTest("B818A2E7-850D-4224-9765-9492742E8DFD")]
        public async Task Exists_ShouldReturn_OutOfDate_WhenSyncConfigurationRdoIsOutOfDateInWorkspace()
        {
            // Arrange
            WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);

            Guid newFieldGuid = Guid.Parse("58F380EE-6DC0-4506-A658-04E8E090BF2A");
            try
            {
                SyncConfigurationRdo.GuidNames.Add(newFieldGuid, "Adler Sieben");


                // Act
                (SyncConfigurationRdo.RdoStatus rdoStatus, HashSet<Guid> existingFieldsGuids, int? existingArtifactId) =
                    await SyncConfigurationRdo.ExistsAsync(workspace.ArtifactID, SyncServicesMgr).ConfigureAwait(false);

                // Assert
                rdoStatus.Should().Be(SyncConfigurationRdo.RdoStatus.OutOfDate);
                existingArtifactId.Should().NotBeNull();
                existingFieldsGuids.Contains(newFieldGuid).Should().BeFalse();
            }
            finally
            {
                SyncConfigurationRdo.GuidNames.Remove(newFieldGuid);
            }
        }

        [IdentifiedTest("BBEFF510-0381-4CCA-B978-10BA71721A71")]
        public async Task Exists_ShouldReturn_Exists_WhenSyncConfigurationExistsInWorkspace()
        {
            // Arrange
            WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);

            // Act
            (SyncConfigurationRdo.RdoStatus rdoStatus, HashSet<Guid> existingFieldsGuids, int? existingArtifactId) =
                await SyncConfigurationRdo.ExistsAsync(workspace.ArtifactID, SyncServicesMgr).ConfigureAwait(false);


            // Assert
            existingArtifactId.Should().NotBeNull();
            SyncConfigurationRdo.GuidNames.Keys.All(g => existingFieldsGuids.Contains(g)).Should().BeTrue();
            rdoStatus.Should().Be(SyncConfigurationRdo.RdoStatus.Exists);
        }

        [IdentifiedTest("468C2533-1A42-4E8C-8F31-9B83F2CEE6AD")]
        public async Task CreateType_ShouldHandleSyncConfigurationCreation()
        {
            // Arrange
            WorkspaceRef refWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
            int refWorkspaceId = refWorkspace.ArtifactID;

            var refSyncConfigurationTypeId =
                await ReadRefSyncConfigurationTypeId(refWorkspace.ArtifactID,
                    SyncConfigurationRdo.SyncConfigurationGuid).ConfigureAwait(false);

            WorkspaceRef testWorkspace = await Environment.CreateWorkspaceAsync().ConfigureAwait(false);
            int testWorkspaceId = testWorkspace.ArtifactID;

            (_, int parentObjectTypeId) = await Rdos.CreateBasicRdoTypeAsync(ServiceFactory, testWorkspace.ArtifactID,
                $"{Guid.NewGuid()}",
                new ObjectTypeIdentifier {ArtifactTypeID = (int) ArtifactType.Case}).ConfigureAwait(false);

            // Act
            int createdConfigurationTypeId = await SyncConfigurationRdo.CreateTypeAsync(testWorkspace.ArtifactID,
                parentObjectTypeId, SyncServicesMgr).ConfigureAwait(false);

            // Assert
            await AssertConfigurationType(
                refWorkspaceId, refSyncConfigurationTypeId,
                testWorkspaceId, createdConfigurationTypeId, parentObjectTypeId).ConfigureAwait(false);
        }

        private async Task<int> GetJobHistoryTypeArtifactIdAsync(int workspaceId)
        {
            using (var guidManager = ServiceFactory.CreateProxy<IArtifactGuidManager>())
            {
                return await guidManager.ReadSingleArtifactIdAsync(workspaceId, _jobHistoryTypeGuid)
                    .ConfigureAwait(false);
            }
        }

        private async Task AssertConfigurationType(
            int refWorkspaceId, int refConfigurationTypeId,
            int testWorkspaceId, int createdConfigurationTypeId, int testParentObjectTypeId)
        {
            var expectedSyncConfigurationType =
                await ReadSyncConfigurationType(refWorkspaceId, refConfigurationTypeId)
                    .ConfigureAwait(false);

            var createdSyncConfigurationType =
                await ReadSyncConfigurationType(testWorkspaceId, createdConfigurationTypeId)
                    .ConfigureAwait(false);

            createdSyncConfigurationType.Should().BeEquivalentTo(expectedSyncConfigurationType,
                config =>
                {
                    config.Excluding(x => x.ArtifactID);
                    config.Excluding(x => x.ArtifactTypeID);
                    config.Excluding(x => x.CreatedBy);
                    config.Excluding(x => x.CreatedOn);
                    config.Excluding(x => x.FieldByteUsage);
                    config.Excluding(x => x.LastModifiedBy);
                    config.Excluding(x => x.LastModifiedOn);
                    config.Excluding(x => x.ParentObjectType);
                    config.Excluding(x => x.RelativityApplications);

                    return config;
                });

            createdSyncConfigurationType.ParentObjectType.Value.ArtifactTypeID.Should().Be(testParentObjectTypeId);

            var expectedSyncConfigurationFieldTypes =
                await ReadSyncConfigurationTypeFields(refWorkspaceId, expectedSyncConfigurationType.ArtifactTypeID)
                    .ConfigureAwait(false);

            var createdSyncConfigurationFieldTypes =
                await ReadSyncConfigurationTypeFields(testWorkspaceId, createdSyncConfigurationType.ArtifactTypeID)
                    .ConfigureAwait(false);

            createdSyncConfigurationFieldTypes.Should().BeEquivalentTo(expectedSyncConfigurationFieldTypes,
                config => config.Excluding(x => x.ArtifactID));
        }

        private async Task<int> ReadRefSyncConfigurationTypeId(int workspaceId, Guid guid)
        {
            using (IObjectManager objectManager = SyncServicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            using (IObjectTypeManager objectTypeManager =
                SyncServicesMgr.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
            {
                ReadRequest request = new ReadRequest()
                {
                    Object = new RelativityObjectRef()
                    {
                        Guid = guid
                    }
                };
                ReadResult result = await objectManager.ReadAsync(workspaceId, request).ConfigureAwait(false);

                var objectType = await objectTypeManager.ReadAsync(workspaceId, result.Object.ArtifactID)
                    .ConfigureAwait(false);

                return objectType.ArtifactID;
            }
        }

        private async Task<ObjectTypeResponse> ReadSyncConfigurationType(int workspaceId, int syncConfigurationTypeId)
        {
            using (IObjectTypeManager objectTypeManager =
                SyncServicesMgr.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
            {
                return await objectTypeManager.ReadAsync(workspaceId, syncConfigurationTypeId)
                    .ConfigureAwait(false);
            }
        }

        private async Task<List<RelativityObject>> ReadSyncConfigurationTypeFields(int workspaceId,
            int configurationTypeId)
        {
            using (IObjectManager objectManager = SyncServicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
            {
                var fieldsNames = SyncConfigurationRdo.GetFieldsDefinition(0)
                    .Values.Select(x => $"'{x.Name}'");

                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef
                    {
                        ArtifactTypeID = (int) ArtifactType.Field
                    },
                    IncludeNameInQueryResult = true,
                    Condition = $"'FieldArtifactTypeID' == {configurationTypeId} " +
                                $" AND 'DisplayName' IN [{string.Join(",", fieldsNames)}]"
                };

                var result = await objectManager.QueryAsync(workspaceId, request, 1, 100).ConfigureAwait(false);

                result.Objects.ForEach(x => x.Name = x.Name.ToLower());

                return result.Objects;
            }
        }
    }
}