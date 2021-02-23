using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.Interfaces.Field;
using Relativity.Services.Interfaces.Field.Models;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.RDOs;
using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.Tests.Unit.SyncConfiguration
{
    [TestFixture]
    public class SyncConfigurationRdoTests
    {
        private const int OBJECT_TYPE_ID = 5;
        private const int WORKSPACE_ID = 6;

        private const int CREATED_FIELD_ID = 7;

        private Mock<IObjectManager> _objectManagerMock;
        private Mock<ISyncServiceManager> _syncServicesMgrMock;
        private Mock<IFieldManager> _fieldManagerMock;
        private Mock<IObjectTypeManager> _objectTypeManagerMock;

        [SetUp]
        public void SetUp()
        {
            _objectManagerMock = new Mock<IObjectManager>();
            _syncServicesMgrMock = new Mock<ISyncServiceManager>();
            _fieldManagerMock = new Mock<IFieldManager>();
            _objectTypeManagerMock = new Mock<IObjectTypeManager>();

            _syncServicesMgrMock.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.System))
                .Returns(_objectManagerMock.Object);

            _syncServicesMgrMock.Setup(x => x.CreateProxy<IObjectTypeManager>(ExecutionIdentity.System))
                .Returns(_objectTypeManagerMock.Object);

            _syncServicesMgrMock.Setup(x => x.CreateProxy<IFieldManager>(ExecutionIdentity.System)).Returns(
                _fieldManagerMock.Object);

            SetupFieldManager(_fieldManagerMock);
        }

        private void SetupFieldManager(Mock<IFieldManager> fieldManagerMock)
        {
            fieldManagerMock.Setup(x => x.CreateYesNoFieldAsync(WORKSPACE_ID, It.IsAny<YesNoFieldRequest>()))
                .ReturnsAsync(CREATED_FIELD_ID);
            fieldManagerMock
                .Setup(x => x.CreateWholeNumberFieldAsync(WORKSPACE_ID, It.IsAny<WholeNumberFieldRequest>()))
                .ReturnsAsync(CREATED_FIELD_ID);
            fieldManagerMock
                .Setup(x => x.CreateFixedLengthFieldAsync(WORKSPACE_ID, It.IsAny<FixedLengthFieldRequest>()))
                .ReturnsAsync(CREATED_FIELD_ID);
            fieldManagerMock.Setup(x => x.CreateLongTextFieldAsync(WORKSPACE_ID, It.IsAny<LongTextFieldRequest>()))
                .ReturnsAsync(CREATED_FIELD_ID);
            fieldManagerMock
                .Setup(x => x.CreateSingleObjectFieldAsync(WORKSPACE_ID, It.IsAny<SingleObjectFieldRequest>()))
                .ReturnsAsync(CREATED_FIELD_ID);
        }

        [Test]
        public void FieldsDefinitions_ShouldContainAllGuidsKeys()
        {
            // Assert
            SyncConfigurationRdo.GetFieldsDefinition(OBJECT_TYPE_ID).Keys
                .All(x => SyncConfigurationRdo.GuidNames.ContainsKey(x)).Should().BeTrue();
        }

        [Test]
        public void FieldsDefinitions_ShouldContainUniqueNames()
        {
            // Act
            var values = SyncConfigurationRdo.GetFieldsDefinition(OBJECT_TYPE_ID).Values;

            // Assert
            values.Distinct().Count().Should().Be(values.Count);
        }

        // [Test]
        // public async Task CreateTypeAsync_ShouldCreateAllFields()
        // {
        //     // Arrange
        //     var guidManagerMock = new Mock<IArtifactGuidManager>();
        //
        //     guidManagerMock.Setup(x => x.CreateSingleAsync(WORKSPACE_ID, OBJECT_TYPE_ID, It.IsAny<List<Guid>>()))
        //         .Returns(Task.CompletedTask);
        //
        //     _syncServicesMgrMock.Setup(x => x.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
        //         .Returns(guidManagerMock.Object);
        //
        //
        //     // Act
        //     await SyncConfigurationRdo.CreateTypeAsync(WORKSPACE_ID, OBJECT_TYPE_ID, _syncServicesMgrMock.Object)
        //         .ConfigureAwait(false);
        //
        //     // Assert
        //     SyncConfigurationRdo.GetFieldsDefinition(OBJECT_TYPE_ID).ForEach(x =>
        //     {
        //         guidManagerMock.Verify(gm =>
        //             gm.CreateSingleAsync(WORKSPACE_ID, CREATED_FIELD_ID,
        //                 It.Is<List<Guid>>(l => l.Contains(x.Key))));
        //     });
        // }
        //
        // [Test]
        // public async Task SaveAsync_ShouldReturnCreatedObjectId()
        // {
        //     // Arrange
        //     const int parentObjectId = 10;
        //     const int expectedCreatedId = 11;
        //
        //     _objectManagerMock.Setup(x =>
        //             x.CreateAsync(WORKSPACE_ID, It.Is<CreateRequest>(
        //                 r => r.ParentObject.ArtifactID == parentObjectId
        //             )))
        //         .ReturnsAsync(new CreateResult {Object = new RelativityObject {ArtifactID = expectedCreatedId}});
        //
        //     // Act
        //     int createdId = await new SyncConfigurationRdo()
        //         .SaveAsync(WORKSPACE_ID, parentObjectId, _syncServicesMgrMock.Object)
        //         .ConfigureAwait(false);
        //
        //     // Assert
        //     createdId.Should().Be(expectedCreatedId);
        // }
        //
        // [Test]
        // public async Task SaveAsync_ShouldPassCorrectValues()
        // {
        //     // Arrange
        //     const int parentObjectId = 10;
        //     const int createdConfigurationId = 11;
        //
        //     CreateRequest createRequest = null;
        //     _objectManagerMock.Setup(x =>
        //             x.CreateAsync(WORKSPACE_ID, It.Is<CreateRequest>(
        //                 r => r.ParentObject.ArtifactID == parentObjectId
        //             )))
        //         .Callback<int, CreateRequest>((_, request) => { createRequest = request; })
        //         .ReturnsAsync(new CreateResult {Object = new RelativityObject {ArtifactID = createdConfigurationId}});
        //
        //     // Act 
        //     SyncConfigurationRdo syncConfigurationRdo = GetSampleConfiguration();
        //
        //
        //     _ = await syncConfigurationRdo
        //         .SaveAsync(WORKSPACE_ID, parentObjectId, _syncServicesMgrMock.Object)
        //         .ConfigureAwait(false);
        //
        //
        //     // Assert
        //     GetFieldValueSelectors().Select(x =>
        //             new
        //             {
        //                 Name = SyncConfigurationRdo.GuidNames[x.Key],
        //                 Value = createRequest.FieldValues.FirstOrDefault(f =>
        //                     f.Field.Guid == x.Key)?.Value?.ToString(), // because in the end it's all serialized to json
        //                 Expected = x.Value(syncConfigurationRdo)?.ToString()
        //             })
        //         .Where(x => x.Value == null || x.Expected != x.Value)
        //         .Should().BeEmpty();
        // }
       
        [Test]
        public void GetFieldValueSelectors_ShouldContainAllFields()
        {
            GetFieldValueSelectors().Keys.Should()
                .BeEquivalentTo(SyncConfigurationRdo.GetFieldsDefinition(OBJECT_TYPE_ID).Keys);
        }

        private static SyncConfigurationRdo GetSampleConfiguration()
        {
            return new SyncConfigurationRdo
            {
                JobHistoryToRetryId = 1,
                SnapshotRecordsCount = 2,
                DataDestinationArtifactId = 3,
                DataSourceArtifactId = 4,
                DestinationWorkspaceArtifactId = 5,
                RdoArtifactTypeId = 6,
                DestinationWorkspaceTagArtifactId = 7,
                SourceJobTagArtifactId = 8,
                SourceWorkspaceTagArtifactId = 9,
                SavedSearchInDestinationArtifactId = 10,

                SnapshotId = "snapshot id",
                FieldsMapping = "fields mapping",
                ImageImport = true,
                NativesBehavior = "natives behaviour",
                DataDestinationType = "data destination type",
                DataSourceType = "data source type",
                EmailNotificationRecipients = "email",
                FieldOverlayBehavior = "field overlay",
                ImportOverwriteMode = "import override",
                IncludeOriginalImages = true,
                MoveExistingDocuments = true,
                ProductionImagePrecedence = "production precedence",

                DestinationFolderStructureBehavior = "destination folder behaviour",
                ImageFileCopyMode = "image copy mode",
                SourceJobTagName = "source job tag name",
                SourceWorkspaceTagName = "source workspace tag name",
                CreateSavedSearchInDestination = true,
                FolderPathSourceFieldName = "folder path structure",

                // JobHistory
                JobHistoryType = Guid.NewGuid(),
                JobHistoryCompletedItemsField = Guid.NewGuid(),
                JobHistoryGuidFailedField = Guid.NewGuid(),
                JobHistoryGuidTotalField = Guid.NewGuid(),
                JobHistoryDestinationWorkspaceInformationField = Guid.NewGuid(),

                // JobHistoryError
                JobHistoryErrorType = Guid.NewGuid(),
                JobHistoryErrorErrorMessages = Guid.NewGuid(),
                JobHistoryErrorErrorStatus = Guid.NewGuid(),
                JobHistoryErrorErrorType = Guid.NewGuid(),
                JobHistoryErrorName = Guid.NewGuid(),
                JobHistoryErrorSourceUniqueId = Guid.NewGuid(),
                JobHistoryErrorStackTrace = Guid.NewGuid(),
                JobHistoryErrorTimeStamp = Guid.NewGuid(),
                JobHistoryErrorItemLevelError = Guid.NewGuid(),
                JobHistoryErrorJobLevelError = Guid.NewGuid(),
                JobHistoryErrorJobHistoryRelation = Guid.NewGuid(),
                JobHistoryErrorNewChoice = Guid.NewGuid(),
                JobHistoryErrorExpiredChoice = Guid.NewGuid(),
                JobHistoryErrorInProgressChoice = Guid.NewGuid(),
                JobHistoryErrorRetriedChoice = Guid.NewGuid(),
            };
        }


        private Dictionary<Guid, Func<SyncConfigurationRdo, object>> GetFieldValueSelectors()
        {
            return new Dictionary<Guid, Func<SyncConfigurationRdo, object>>
            {
                {SyncRdoGuids.RdoArtifactTypeIdGuid, x => x.RdoArtifactTypeId},
                {SyncRdoGuids.DataSourceTypeGuid, x => x.DataSourceType},
                {SyncRdoGuids.DataSourceArtifactIdGuid, x => x.DataSourceArtifactId},
                {SyncRdoGuids.DestinationWorkspaceArtifactIdGuid, x => x.DestinationWorkspaceArtifactId},
                {SyncRdoGuids.DataDestinationArtifactIdGuid, x => x.DataDestinationArtifactId},
                {SyncRdoGuids.DataDestinationTypeGuid, x => x.DataDestinationType},
                {
                    SyncRdoGuids.DestinationFolderStructureBehaviorGuid,
                    x => x.DestinationFolderStructureBehavior
                },
                {SyncRdoGuids.FolderPathSourceFieldNameGuid, x => x.FolderPathSourceFieldName},
                {SyncRdoGuids.CreateSavedSearchInDestinationGuid, x => x.CreateSavedSearchInDestination},
                {
                    SyncRdoGuids.SavedSearchInDestinationArtifactIdGuid,
                    x => x.SavedSearchInDestinationArtifactId
                },
                {SyncRdoGuids.ImportOverwriteModeGuid, x => x.ImportOverwriteMode},
                {SyncRdoGuids.FieldOverlayBehaviorGuid, x => x.FieldOverlayBehavior},
                {SyncRdoGuids.FieldMappingsGuid, x => x.FieldsMapping},
                {SyncRdoGuids.MoveExistingDocumentsGuid, x => x.MoveExistingDocuments},
                {SyncRdoGuids.NativesBehaviorGuid, x => x.NativesBehavior},
                {SyncRdoGuids.ImageImportGuid, x => x.ImageImport},
                {SyncRdoGuids.IncludeOriginalImagesGuid, x => x.IncludeOriginalImages},
                {SyncRdoGuids.ProductionImagePrecedenceGuid, x => x.ProductionImagePrecedence},
                {SyncRdoGuids.ImageFileCopyModeGuid, x => x.ImageFileCopyMode},
                {SyncRdoGuids.EmailNotificationRecipientsGuid, x => x.EmailNotificationRecipients},
                {SyncRdoGuids.JobHistoryToRetryIdGuid, x => x.JobHistoryToRetryId},
                {SyncRdoGuids.SnapshotIdGuid, x => x.SnapshotId},
                {SyncRdoGuids.SnapshotRecordsCountGuid, x => x.SnapshotRecordsCount},
                {SyncRdoGuids.SourceJobTagArtifactIdGuid, x => x.SourceJobTagArtifactId},
                {SyncRdoGuids.SourceJobTagNameGuid, x => x.SourceJobTagName},
                {SyncRdoGuids.SourceWorkspaceTagArtifactIdGuid, x => x.SourceWorkspaceTagArtifactId},
                {SyncRdoGuids.SourceWorkspaceTagNameGuid, x => x.SourceWorkspaceTagName},
                {SyncRdoGuids.DestinationWorkspaceTagArtifactIdGuid, x => x.DestinationWorkspaceTagArtifactId},

                // JobHistory
                {SyncRdoGuids.JobHistoryTypeGuid, x => x.JobHistoryType},
                {SyncRdoGuids.JobHistoryCompletedItemsFieldGuid, x => x.JobHistoryCompletedItemsField},
                {SyncRdoGuids.JobHistoryFailedItemsFieldGuid, x => x.JobHistoryGuidFailedField},
                {SyncRdoGuids.JobHistoryTotalItemsFieldGuid, x => x.JobHistoryGuidTotalField},
                {
                    SyncRdoGuids.JobHistoryDestinationWorkspaceInformationGuid,
                    x => x.JobHistoryDestinationWorkspaceInformationField
                },

                // JobHistoryError
                {SyncRdoGuids.JobHistoryErrorTypeGuid, x => x.JobHistoryErrorType},
                {SyncRdoGuids.JobHistoryErrorErrorMessagesGuid, x => x.JobHistoryErrorErrorMessages},
                {SyncRdoGuids.JobHistoryErrorErrorStatusGuid, x => x.JobHistoryErrorErrorStatus},
                {SyncRdoGuids.JobHistoryErrorErrorTypeGuid, x => x.JobHistoryErrorErrorType},
                {SyncRdoGuids.JobHistoryErrorNameGuid, x => x.JobHistoryErrorName},
                {SyncRdoGuids.JobHistoryErrorSourceUniqueIdGuid, x => x.JobHistoryErrorSourceUniqueId},
                {SyncRdoGuids.JobHistoryErrorStackTraceGuid, x => x.JobHistoryErrorStackTrace},
                {SyncRdoGuids.JobHistoryErrorTimeStampGuid, x => x.JobHistoryErrorTimeStamp},
                {SyncRdoGuids.JobHistoryErrorItemLevelErrorGuid, x => x.JobHistoryErrorItemLevelError},
                {SyncRdoGuids.JobHistoryErrorJobLevelErrorGuid, x => x.JobHistoryErrorJobLevelError},
                {SyncRdoGuids.JobHistoryErrorJobHistoryRelationGuid, x => x.JobHistoryErrorJobHistoryRelation},
                {SyncRdoGuids.JobHistoryErrorNewChoiceGuid, x => x.JobHistoryErrorNewChoice},
                {SyncRdoGuids.JobHistoryErrorExpiredChoiceGuid, x => x.JobHistoryErrorExpiredChoice},
                {SyncRdoGuids.JobHistoryErrorInProgressChoiceGuid, x => x.JobHistoryErrorInProgressChoice},
                {SyncRdoGuids.JobHistoryErrorRetriedChoiceGuid, x => x.JobHistoryErrorRetriedChoice}
            };
        }
    }
}