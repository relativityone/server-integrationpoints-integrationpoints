using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.RDOs;
using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Utils;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.SyncConfiguration
{
    internal class DocumentSyncConfigurationCreationTests : SyncConfigurationCreationTestsBase
    {
        private int _savedSearchId;
        private int _destinationFolderId;

        protected override async Task ChildSuiteSetup()
        {
            await base.ChildSuiteSetup();

            _savedSearchId = await Rdos.GetSavedSearchInstanceAsync(ServiceFactory, SourceWorkspaceId).ConfigureAwait(false);

            _destinationFolderId = await Rdos.GetRootFolderInstanceAsync(ServiceFactory, DestinationWorkspaceId).ConfigureAwait(false);
        }

        [IdentifiedTest("8252CA9A-DC74-450C-890B-F096CD5068FC")]
        public async Task Create_DefaultDocumentSyncConfiguration()
        {
            // Arrange
            SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);

            ISyncContext syncContext =
                new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

            DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);

            // Act
            int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, ServicesMgr, new EmptyLogger())
                .ConfigureRdos(DefaultGuids.DefaultRdoOptions)
                .ConfigureDocumentSync(options)
                .SaveAsync()
                .ConfigureAwait(false);

            // Assert
            await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
        }

        [IdentifiedTest("FDE3DCBD-0999-4159-A08F-184EFE383263")]
        public async Task Create_DocumentSyncConfigurationWithMapping()
        {
            // Arrange
            const string extractedTextField = "Extracted Text";

            var identifierFieldsMapping = await GetDocumentIdentifierMappingAsync(SourceWorkspaceId, DestinationWorkspaceId)
                .ConfigureAwait(false);
            var extractedTextFieldsMapping = await GetExtractedTextMappingAsync(SourceWorkspaceId, DestinationWorkspaceId)
                .ConfigureAwait(false);

            var expectedFieldsMapping = identifierFieldsMapping.Concat(extractedTextFieldsMapping).ToList();

            SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync(expectedFieldsMapping).ConfigureAwait(false);

            ISyncContext syncContext =
                new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

            // Act
            int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, ServicesMgr, new EmptyLogger())
                .ConfigureRdos(DefaultGuids.DefaultRdoOptions)
                .ConfigureDocumentSync(
                    new DocumentSyncOptions(_savedSearchId, _destinationFolderId))
                .WithFieldsMapping(builder =>
                    builder
                        .WithIdentifier()
                        .WithField(extractedTextField, extractedTextField))
                .SaveAsync().ConfigureAwait(false);

            // Assert
            await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
        }

        [IdentifiedTest("0CC1F916-B359-4C12-8A2D-532935C0FB5C")]
        public async Task Create_DocumentSyncConfigurationWithRetry()
        {
            // Arrange
            SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
            expectedSyncConfiguration.JobHistoryToRetryId = JobHistory.ArtifactID;

            ISyncContext syncContext =
                new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

            DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);
            RetryOptions retryOptions = new RetryOptions(JobHistory.ArtifactID);

            // Act
            int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, ServicesMgr, new EmptyLogger())
                .ConfigureRdos(DefaultGuids.DefaultRdoOptions)
                .ConfigureDocumentSync(options)
                .IsRetry(retryOptions)
                .SaveAsync().ConfigureAwait(false);

            // Assert
            await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
        }

        private static IEnumerable<object[]> OverwriteModeDataSource => new List<object[]>()
        {
            new object[] { new OverwriteOptions(ImportOverwriteMode.AppendOnly), ImportOverwriteMode.AppendOnly, FieldOverlayBehavior.UseFieldSettings },
            new object[]
            {
                new OverwriteOptions(ImportOverwriteMode.AppendOverlay) { FieldsOverlayBehavior = FieldOverlayBehavior.ReplaceValues },
                ImportOverwriteMode.AppendOverlay, FieldOverlayBehavior.ReplaceValues
            },
            new object[]
            {
                new OverwriteOptions(ImportOverwriteMode.AppendOverlay) { FieldsOverlayBehavior = FieldOverlayBehavior.MergeValues },
                ImportOverwriteMode.AppendOverlay, FieldOverlayBehavior.MergeValues
            },
            new object[] { new OverwriteOptions(ImportOverwriteMode.OverlayOnly), ImportOverwriteMode.OverlayOnly, FieldOverlayBehavior.UseFieldSettings }
        };

        [TestCaseSource(nameof(OverwriteModeDataSource))]
        [IdentifiedTest("26F1771E-3BCC-41C3-8BC2-BB3C2947A82A")]
        public async Task Create_DocumentSyncConfigurationWithOverwriteAppendOnly(
            OverwriteOptions overwriteOptions, ImportOverwriteMode expectedOverwriteMode, FieldOverlayBehavior expectedFieldsOverlay)
        {
            // Arrange
            SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);

            expectedSyncConfiguration.ImportOverwriteMode = expectedOverwriteMode;
            expectedSyncConfiguration.FieldOverlayBehavior = expectedFieldsOverlay;

            ISyncContext syncContext =
                new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

            DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);

            // Act
            int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, ServicesMgr, new EmptyLogger())
                .ConfigureRdos(DefaultGuids.DefaultRdoOptions)
                .ConfigureDocumentSync(options)
                .OverwriteMode(overwriteOptions)
                .SaveAsync().ConfigureAwait(false);

            // Assert
            await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
        }

        [IdentifiedTest("9C2B5ACA-10EC-4FEB-99FC-987C91F546DB")]
        public async Task Create_DocumentSyncConfigurationWithRetainFolderStructureFromWorkspaceAndMoveExistingDocuments()
        {
            // Arrange
            SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
            expectedSyncConfiguration.DestinationFolderStructureBehavior =
                DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure;
            expectedSyncConfiguration.MoveExistingDocuments = true;

            ISyncContext syncContext =
                new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

            DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);
            DestinationFolderStructureOptions folderOptions =
                DestinationFolderStructureOptions.RetainFolderStructureFromSourceWorkspace();
            folderOptions.MoveExistingDocuments = true;

            // Act
            int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, ServicesMgr, new EmptyLogger())
                .ConfigureRdos(DefaultGuids.DefaultRdoOptions)
                .ConfigureDocumentSync(options)
                .DestinationFolderStructure(folderOptions)
                .SaveAsync().ConfigureAwait(false);

            // Assert
            await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
        }

        [IdentifiedTest("A289BCE8-D6FF-4C8A-91B2-128E216E1340")]
        public async Task Create_DocumentSyncConfigurationWithFolderStructureFromFieldAndMoveExistingDocuments()
        {
            // Arrange
            const string documentFolderPathName = "Document Folder Path";

            int documentFolderPathFieldId = await ReadFieldByName(SourceWorkspaceId, documentFolderPathName).ConfigureAwait(false);

            SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
            expectedSyncConfiguration.DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.ReadFromField;
            expectedSyncConfiguration.FolderPathSourceFieldName = documentFolderPathName;
            expectedSyncConfiguration.MoveExistingDocuments = true;

            ISyncContext syncContext =
                new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

            DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);
            DestinationFolderStructureOptions folderOptions =
                DestinationFolderStructureOptions.ReadFromField(documentFolderPathFieldId);
            folderOptions.MoveExistingDocuments = true;

            // Act
            int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, ServicesMgr, new EmptyLogger())
                .ConfigureRdos(DefaultGuids.DefaultRdoOptions)
                .ConfigureDocumentSync(options)
                .DestinationFolderStructure(folderOptions)
                .SaveAsync().ConfigureAwait(false);

            // Assert
            await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
        }

        [IdentifiedTest("77DE9D01-7F32-4250-A425-4C6BFD9FD6B4")]
        public async Task Create_DocumentSyncConfigurationWithEmailNotifications()
        {
            // Arrange
            SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
            expectedSyncConfiguration.EmailNotificationRecipients = "test1@relativity.com;test2@relativity.com";

            ISyncContext syncContext =
                new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

            DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);
            EmailNotificationsOptions emailOptions = new EmailNotificationsOptions(new List<string>()
            {
                "test1@relativity.com",
                "test2@relativity.com"
            });

            // Act
            int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, ServicesMgr, new EmptyLogger())
                .ConfigureRdos(DefaultGuids.DefaultRdoOptions)
                .ConfigureDocumentSync(options)
                .EmailNotifications(emailOptions)
                .SaveAsync().ConfigureAwait(false);

            // Assert
            await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
        }

        [IdentifiedTest("C37D734D-33A2-413F-9EE7-842A408483E0")]
        public async Task Create_DocumentSyncConfigurationWithCreateSavedSearchInDestination()
        {
            // Arrange
            SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
            expectedSyncConfiguration.CreateSavedSearchInDestination = true;

            ISyncContext syncContext =
                new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

            DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);

            // Act
            int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, ServicesMgr, new EmptyLogger())
                .ConfigureRdos(DefaultGuids.DefaultRdoOptions)
                .ConfigureDocumentSync(options)
                .CreateSavedSearch(
                    new CreateSavedSearchOptions(true))
                .SaveAsync().ConfigureAwait(false);

            // Assert
            await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
        }

        private async Task<SyncConfigurationRdo> CreateDefaultExpectedConfigurationAsync(List<FieldMap> expectedFieldsMapping = null)
        {
            var fieldsMappingToSerialize = expectedFieldsMapping ?? await GetDocumentIdentifierMappingAsync(SourceWorkspaceId, DestinationWorkspaceId).ConfigureAwait(false);

            return new SyncConfigurationRdo
            {
                RdoArtifactTypeId = 10,
                DestinationRdoArtifactTypeId = 10,
                DataSourceType = DataSourceType.SavedSearch,
                DataSourceArtifactId = _savedSearchId,
                DestinationWorkspaceArtifactId = DestinationWorkspaceId,
                DataDestinationArtifactId = _destinationFolderId,
                DataDestinationType = DestinationLocationType.Folder,
                DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
                ImportOverwriteMode = ImportOverwriteMode.AppendOnly,
                FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings,
                NativesBehavior = ImportNativeFileCopyMode.DoNotImportNativeFiles,
                JobHistoryId = JobHistory.ArtifactID,

                FieldsMapping = new JSONSerializer().Serialize(fieldsMappingToSerialize)
            };
        }

        private async Task<int> ReadFieldByName(int workspaceId, string fieldName)
        {
            using (IObjectManager objectManager = await
                ServiceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
            {
                QueryRequest request = new QueryRequest()
                {
                    ObjectType = new ObjectTypeRef()
                    {
                        ArtifactTypeID = (int)ArtifactType.Field
                    },
                    Condition = $"'Name' == '{fieldName}'"
                };

                var result = await objectManager.QuerySlimAsync(workspaceId, request, 0, 1).ConfigureAwait(false);

                return result.Objects.Single().ArtifactID;
            }
        }
    }
}
