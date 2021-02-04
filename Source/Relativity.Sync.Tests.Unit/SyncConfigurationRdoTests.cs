using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Relativity.Sync.Tests.Unit
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

        [Test]
        public async Task CreateTypeAsync_ShouldCreateAllFields()
        {
            // Arrange
            var guidManagerMock = new Mock<IArtifactGuidManager>();

            guidManagerMock.Setup(x => x.CreateSingleAsync(WORKSPACE_ID, OBJECT_TYPE_ID, It.IsAny<List<Guid>>()))
                .Returns(Task.CompletedTask);

            _syncServicesMgrMock.Setup(x => x.CreateProxy<IArtifactGuidManager>(ExecutionIdentity.System))
                .Returns(guidManagerMock.Object);


            // Act
            await SyncConfigurationRdo.CreateTypeAsync(WORKSPACE_ID, OBJECT_TYPE_ID, _syncServicesMgrMock.Object)
                .ConfigureAwait(false);

            // Assert
            SyncConfigurationRdo.GetFieldsDefinition(OBJECT_TYPE_ID).ForEach(x =>
            {
                guidManagerMock.Verify(gm =>
                    gm.CreateSingleAsync(WORKSPACE_ID, CREATED_FIELD_ID,
                        It.Is<List<Guid>>(l => l.Contains(x.Key))));
            });
        }

        [Test]
        public async Task SaveAsync_ShouldReturnCreatedObjectId()
        {
            // Arrange
            const int parentObjectId = 10;
            const int expectedCreatedId = 11;

            _objectManagerMock.Setup(x =>
                    x.CreateAsync(WORKSPACE_ID, It.Is<CreateRequest>(
                        r => r.ParentObject.ArtifactID == parentObjectId
                    )))
                .ReturnsAsync(new CreateResult {Object = new RelativityObject {ArtifactID = expectedCreatedId}});

            // Act
            int createdId = await new SyncConfigurationRdo()
                .SaveAsync(WORKSPACE_ID, parentObjectId, _syncServicesMgrMock.Object)
                .ConfigureAwait(false);

            // Assert
            createdId.Should().Be(expectedCreatedId);
        }

        [Test]
        public async Task SaveAsync_ShouldPassCorrectValues()
        {
            // Arrange
            const int parentObjectId = 10;
            const int createdConfigurationId = 11;

            CreateRequest createRequest = null;
            _objectManagerMock.Setup(x =>
                    x.CreateAsync(WORKSPACE_ID, It.Is<CreateRequest>(
                        r => r.ParentObject.ArtifactID == parentObjectId
                    )))
                .Callback<int, CreateRequest>((_, request) => { createRequest = request; })
                .ReturnsAsync(new CreateResult {Object = new RelativityObject {ArtifactID = createdConfigurationId}});

            // Act 
            SyncConfigurationRdo syncConfigurationRdo = new SyncConfigurationRdo
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


            _ = await syncConfigurationRdo
                .SaveAsync(WORKSPACE_ID, parentObjectId, _syncServicesMgrMock.Object)
                .ConfigureAwait(false);


            // Assert
            GetFieldValueSelectors().Select(x =>
                    new
                    {
                        Name = SyncConfigurationRdo.GuidNames[x.Key],
                        Value = createRequest.FieldValues.FirstOrDefault(f =>
                            f.Field.Guid == x.Key)?.Value?.ToString(), // because in the end it's all serialized to json
                        Expected = x.Value(syncConfigurationRdo)?.ToString()
                    })
                .Where(x => x.Value == null || x.Expected != x.Value)
                .Should().BeEmpty();
        }

        [Test]
        public void GetFieldValueSelectors_ShouldContainAllFields()
        {
            GetFieldValueSelectors().Keys.Should()
                .BeEquivalentTo(SyncConfigurationRdo.GetFieldsDefinition(OBJECT_TYPE_ID).Keys);
        }


        private Dictionary<Guid, Func<SyncConfigurationRdo, object>> GetFieldValueSelectors()
        {
            return new Dictionary<Guid, Func<SyncConfigurationRdo, object>>
            {
                {SyncConfigurationRdo.RdoArtifactTypeIdGuid, x => x.RdoArtifactTypeId},
                {SyncConfigurationRdo.DataSourceTypeGuid, x => x.DataSourceType},
                {SyncConfigurationRdo.DataSourceArtifactIdGuid, x => x.DataSourceArtifactId},
                {SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid, x => x.DestinationWorkspaceArtifactId},
                {SyncConfigurationRdo.DataDestinationArtifactIdGuid, x => x.DataDestinationArtifactId},
                {SyncConfigurationRdo.DataDestinationTypeGuid, x => x.DataDestinationType},
                {
                    SyncConfigurationRdo.DestinationFolderStructureBehaviorGuid,
                    x => x.DestinationFolderStructureBehavior
                },
                {SyncConfigurationRdo.FolderPathSourceFieldNameGuid, x => x.FolderPathSourceFieldName},
                {SyncConfigurationRdo.CreateSavedSearchInDestinationGuid, x => x.CreateSavedSearchInDestination},
                {
                    SyncConfigurationRdo.SavedSearchInDestinationArtifactIdGuid,
                    x => x.SavedSearchInDestinationArtifactId
                },
                {SyncConfigurationRdo.ImportOverwriteModeGuid, x => x.ImportOverwriteMode},
                {SyncConfigurationRdo.FieldOverlayBehaviorGuid, x => x.FieldOverlayBehavior},
                {SyncConfigurationRdo.FieldMappingsGuid, x => x.FieldsMapping},
                {SyncConfigurationRdo.MoveExistingDocumentsGuid, x => x.MoveExistingDocuments},
                {SyncConfigurationRdo.NativesBehaviorGuid, x => x.NativesBehavior},
                {SyncConfigurationRdo.ImageImportGuid, x => x.ImageImport},
                {SyncConfigurationRdo.IncludeOriginalImagesGuid, x => x.IncludeOriginalImages},
                {SyncConfigurationRdo.ProductionImagePrecedenceGuid, x => x.ProductionImagePrecedence},
                {SyncConfigurationRdo.ImageFileCopyModeGuid, x => x.ImageFileCopyMode},
                {SyncConfigurationRdo.EmailNotificationRecipientsGuid, x => x.EmailNotificationRecipients},
                {SyncConfigurationRdo.JobHistoryToRetryIdGuid, x => x.JobHistoryToRetryId},
                {SyncConfigurationRdo.SnapshotIdGuid, x => x.SnapshotId},
                {SyncConfigurationRdo.SnapshotRecordsCountGuid, x => x.SnapshotRecordsCount},
                {SyncConfigurationRdo.SourceJobTagArtifactIdGuid, x => x.SourceJobTagArtifactId},
                {SyncConfigurationRdo.SourceJobTagNameGuid, x => x.SourceJobTagName},
                {SyncConfigurationRdo.SourceWorkspaceTagArtifactIdGuid, x => x.SourceWorkspaceTagArtifactId},
                {SyncConfigurationRdo.SourceWorkspaceTagNameGuid, x => x.SourceWorkspaceTagName},
                {SyncConfigurationRdo.DestinationWorkspaceTagArtifactIdGuid, x => x.DestinationWorkspaceTagArtifactId},

                // JobHistory
                {SyncConfigurationRdo.JobHistoryTypeGuid, x => x.JobHistoryType},
                {SyncConfigurationRdo.JobHistoryCompletedItemsFieldGuid, x => x.JobHistoryCompletedItemsField},
                {SyncConfigurationRdo.JobHistoryFailedItemsFieldGuid, x => x.JobHistoryGuidFailedField},
                {SyncConfigurationRdo.JobHistoryTotalItemsFieldGuid, x => x.JobHistoryGuidTotalField},
                {
                    SyncConfigurationRdo.JobHistoryDestinationWorkspaceInformationGuid,
                    x => x.JobHistoryDestinationWorkspaceInformationField
                },

                // JobHistoryError
                {SyncConfigurationRdo.JobHistoryErrorTypeGuid, x => x.JobHistoryErrorType},
                {SyncConfigurationRdo.JobHistoryErrorErrorMessagesGuid, x => x.JobHistoryErrorErrorMessages},
                {SyncConfigurationRdo.JobHistoryErrorErrorStatusGuid, x => x.JobHistoryErrorErrorStatus},
                {SyncConfigurationRdo.JobHistoryErrorErrorTypeGuid, x => x.JobHistoryErrorErrorType},
                {SyncConfigurationRdo.JobHistoryErrorNameGuid, x => x.JobHistoryErrorName},
                {SyncConfigurationRdo.JobHistoryErrorSourceUniqueIdGuid, x => x.JobHistoryErrorSourceUniqueId},
                {SyncConfigurationRdo.JobHistoryErrorStackTraceGuid, x => x.JobHistoryErrorStackTrace},
                {SyncConfigurationRdo.JobHistoryErrorTimeStampGuid, x => x.JobHistoryErrorTimeStamp},
                {SyncConfigurationRdo.JobHistoryErrorItemLevelErrorGuid, x => x.JobHistoryErrorItemLevelError},
                {SyncConfigurationRdo.JobHistoryErrorJobLevelErrorGuid, x => x.JobHistoryErrorJobLevelError},
                {SyncConfigurationRdo.JobHistoryErrorJobHistoryRelationGuid, x => x.JobHistoryErrorJobHistoryRelation},
                {SyncConfigurationRdo.JobHistoryErrorNewChoiceGuid, x => x.JobHistoryErrorNewChoice},
                {SyncConfigurationRdo.JobHistoryErrorExpiredChoiceGuid, x => x.JobHistoryErrorExpiredChoice},
                {SyncConfigurationRdo.JobHistoryErrorInProgressChoiceGuid, x => x.JobHistoryErrorInProgressChoice},
                {SyncConfigurationRdo.JobHistoryErrorRetriedChoiceGuid, x => x.JobHistoryErrorRetriedChoice}
            };
        }
    }
}