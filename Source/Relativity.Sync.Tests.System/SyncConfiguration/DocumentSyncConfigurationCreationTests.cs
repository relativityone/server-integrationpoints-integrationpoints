// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using NUnit.Framework;
// using Relativity.API;
// using Relativity.Services.Interfaces.Field;
// using Relativity.Services.Objects;
// using Relativity.Services.Objects.DataContracts;
// using Relativity.Sync.Configuration;
// using Relativity.Sync.RDOs;
// using Relativity.Sync.Storage;
// using Relativity.Sync.SyncConfiguration;
// using Relativity.Sync.SyncConfiguration.Options;
// using Relativity.Sync.Tests.System.Core.Helpers;
// using Relativity.Sync.Utils;
// using Relativity.Testing.Identification;
//
// namespace Relativity.Sync.Tests.System.SyncConfiguration
// {
// 	[TestFixture]
// 	[Feature.DataTransfer.IntegrationPoints.Sync]
// 	internal class DocumentSyncConfigurationCreationTests : SyncConfigurationCreationTestsBase
// 	{
// 		private int _savedSearchId;
// 		private int _destinationFolderId;
//
// 		protected override async Task ChildSuiteSetup()
// 		{
// 			await base.ChildSuiteSetup();
//
// 			_savedSearchId = await Rdos.GetSavedSearchInstance(ServiceFactory, SourceWorkspaceId).ConfigureAwait(false);
//
// 			_destinationFolderId = await Rdos.GetRootFolderInstance(ServiceFactory, DestinationWorkspaceId).ConfigureAwait(false);
// 		}
//
// 		[Test]
// 		public async Task Create_DefaultDocumentSyncConfiguration()
// 		{
// 			// Arrange
// 			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
//
// 			ISyncContext syncContext =
// 				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);
//
// 			DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);
//
// 			// Act
// 			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
// 				.ConfigureDocumentSync(options)
// 				.SaveAsync().ConfigureAwait(false);
//
// 			// Assert
// 			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
// 		}
//
// 		[Test]
// 		public async Task Create_DocumentSyncConfigurationWithMapping()
// 		{
// 			// Arrange
// 			const string extractedTextField = "Extracted Text";
//
// 			var identifierFieldsMapping = await GetIdentifierMappingAsync(SourceWorkspaceId, DestinationWorkspaceId)
// 				.ConfigureAwait(false);
// 			var extractedTextFieldsMapping = await GetExtractedTextMappingAsync(SourceWorkspaceId, DestinationWorkspaceId)
// 				.ConfigureAwait(false);
//
// 			var expectedFieldsMapping = identifierFieldsMapping.Concat(extractedTextFieldsMapping).ToList();
//
// 			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync(expectedFieldsMapping).ConfigureAwait(false);
//
// 			ISyncContext syncContext =
// 				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);
//
// 			// Act
// 			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
// 				.ConfigureDocumentSync(
// 					new DocumentSyncOptions(_savedSearchId, _destinationFolderId))
// 				.WithFieldsMapping(builder => 
// 					builder
// 						.WithIdentifier()
// 						.WithField(extractedTextField, extractedTextField))
// 				.SaveAsync().ConfigureAwait(false);
//
// 			// Assert
// 			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
// 		}
//
// 		[Test]
// 		public async Task Create_DocumentSyncConfigurationWithRetry()
// 		{
// 			// Arrange
// 			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
// 			expectedSyncConfiguration.JobHistoryToRetry = GetBasicRelativityObject(JobHistory.ArtifactID);
//
// 			ISyncContext syncContext =
// 				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);
//
// 			DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);
// 			RetryOptions retryOptions = new RetryOptions(JobHistory);
//
// 			// Act
// 			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
// 				.ConfigureDocumentSync(options)
// 				.IsRetry(retryOptions)
// 				.SaveAsync().ConfigureAwait(false);
//
// 			// Assert
// 			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
// 		}
//
// 		private static IEnumerable<object[]> OverwriteModeDataSource => new List<object[]>()
// 		{
// 			new object[] {new OverwriteOptions(ImportOverwriteMode.AppendOnly), "AppendOnly", "Use Field Settings"},
// 			new object[] {new OverwriteOptions(ImportOverwriteMode.AppendOverlay) {FieldsOverlayBehavior = FieldOverlayBehavior.ReplaceValues},
// 				"AppendOverlay", "Replace Values"},
// 			new object[] {new OverwriteOptions(ImportOverwriteMode.AppendOverlay) { FieldsOverlayBehavior = FieldOverlayBehavior.MergeValues },
// 				"AppendOverlay", "Merge Values"},
// 			new object[] { new OverwriteOptions(ImportOverwriteMode.OverlayOnly), "OverlayOnly", "Use Field Settings"}
// 		};
//
// 		[TestCaseSource(nameof(OverwriteModeDataSource))]
// 		public async Task Create_DocumentSyncConfigurationWithOverwriteAppendOnly(
// 			OverwriteOptions overwriteOptions, string expectedOverwriteMode, string expectedFieldsOverlay)
// 		{
// 			// Arrange
// 			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
// 			expectedSyncConfiguration.ImportOverwriteMode = expectedOverwriteMode;
// 			expectedSyncConfiguration.FieldOverlayBehavior = expectedFieldsOverlay;
//
// 			ISyncContext syncContext =
// 				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);
//
// 			DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);
//
// 			// Act
// 			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
// 				.ConfigureDocumentSync(options)
// 				.OverwriteMode(overwriteOptions)
// 				.SaveAsync().ConfigureAwait(false);
//
// 			// Assert
// 			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
// 		}
//
// 		[Test]
// 		public async Task Create_DocumentSyncConfigurationWithRetainFolderStructureFromWorkspaceAndMoveExistingDocuments()
// 		{
// 			// Arrange
// 			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
// 			expectedSyncConfiguration.DestinationFolderStructureBehavior = "RetainSourceWorkspaceStructure";
// 			expectedSyncConfiguration.MoveExistingDocuments = true;
//
// 			ISyncContext syncContext =
// 				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);
//
// 			DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);
// 			DestinationFolderStructureOptions folderOptions = 
// 				DestinationFolderStructureOptions.RetainFolderStructureFromSourceWorkspace();
// 			folderOptions.MoveExistingDocuments = true;
//
// 			// Act
// 			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
// 				.ConfigureDocumentSync(options)
// 				.DestinationFolderStructure(folderOptions)
// 				.SaveAsync().ConfigureAwait(false);
//
// 			// Assert
// 			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
// 		}
//
// 		[Test]
// 		public async Task Create_DocumentSyncConfigurationWithFolderStructureFromFieldAndMoveExistingDocuments()
// 		{
// 			// Arrange
// 			const string documentFolderPathName = "Document Folder Path";
//
// 			int documentFolderPathFieldId = await ReadFieldByName(SourceWorkspaceId, documentFolderPathName).ConfigureAwait(false);
//
// 			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
// 			expectedSyncConfiguration.DestinationFolderStructureBehavior = "ReadFromField";
// 			expectedSyncConfiguration.FolderPathSourceFieldName = documentFolderPathName;
// 			expectedSyncConfiguration.MoveExistingDocuments = true;
//
// 			ISyncContext syncContext =
// 				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);
//
// 			DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);
// 			DestinationFolderStructureOptions folderOptions = 
// 				DestinationFolderStructureOptions.ReadFromField(documentFolderPathFieldId);
// 			folderOptions.MoveExistingDocuments = true;
//
// 			// Act
// 			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
// 				.ConfigureDocumentSync(options)
// 				.DestinationFolderStructure(folderOptions)
// 				.SaveAsync().ConfigureAwait(false);
//
// 			// Assert
// 			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
// 		}
//
// 		[Test]
// 		public async Task Create_DocumentSyncConfigurationWithEmailNotifications()
// 		{
// 			// Arrange
// 			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
// 			expectedSyncConfiguration.EmailNotificationRecipients = "test1@relativity.com;test2@relativity.com";
//
// 			ISyncContext syncContext =
// 				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);
//
// 			DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);
// 			EmailNotificationsOptions emailOptions = new EmailNotificationsOptions(new List<string>()
// 			{
// 				"test1@relativity.com",
// 				"test2@relativity.com"
// 			});
//
// 			// Act
// 			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
// 				.ConfigureDocumentSync(options)
// 				.EmailNotifications(emailOptions)
// 				.SaveAsync().ConfigureAwait(false);
//
// 			// Assert
// 			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
// 		}
//
// 		[Test]
// 		public async Task Create_DocumentSyncConfigurationWithCreateSavedSearchInDestination()
// 		{
// 			// Arrange
// 			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
// 			expectedSyncConfiguration.CreateSavedSearchInDestination = true;
//
// 			ISyncContext syncContext =
// 				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);
//
// 			DocumentSyncOptions options = new DocumentSyncOptions(_savedSearchId, _destinationFolderId);
//
// 			// Act
// 			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
// 				.ConfigureDocumentSync(options)
// 				.CreateSavedSearch(
// 					new CreateSavedSearchOptions(true))
// 				.SaveAsync().ConfigureAwait(false);
//
// 			// Assert
// 			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
// 		}
//
// 		async Task<SyncConfigurationRdo> CreateDefaultExpectedConfigurationAsync(List<FieldMap> expectedFieldsMapping = null)
// 		{
// 			var fieldsMappingToSerialize = expectedFieldsMapping ?? await GetIdentifierMappingAsync(SourceWorkspaceId, DestinationWorkspaceId).ConfigureAwait(false);
//
// 			return new SyncConfigurationRdo
// 			{
// 				RdoArtifactTypeId = 10,
// 				DataSourceType = "SavedSearch",
// 				DataSourceArtifactId = _savedSearchId,
// 				DestinationWorkspaceArtifactId = DestinationWorkspaceId,
// 				DataDestinationArtifactId = _destinationFolderId,
// 				DataDestinationType = "Folder",
// 				DestinationFolderStructureBehavior = "None",
// 				ImportOverwriteMode = "AppendOnly",
// 				FieldOverlayBehavior = "Use Field Settings",
// 				NativesBehavior = "None",
//
// 				FieldsMapping = new JSONSerializer().Serialize(fieldsMappingToSerialize)
// 			};
// 		}
//
// 		private async Task<int> ReadFieldByName(int workspaceId, string fieldName)
// 		{
// 			using (IObjectManager objectManager =
// 				SyncServicesMgr.CreateProxy<IObjectManager>(ExecutionIdentity.System))
// 			{
// 				QueryRequest request = new QueryRequest()
// 				{
// 					ObjectType = new ObjectTypeRef()
// 					{
// 						ArtifactTypeID = (int)ArtifactType.Field
// 					},
// 					Condition = $"'Name' == '{fieldName}'"
// 				};
//
// 				var result = await objectManager.QuerySlimAsync(workspaceId, request, 0, 1).ConfigureAwait(false);
//
// 				return result.Objects.Single().ArtifactID;
// 			}
// 		}
// 	}
// }
