using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Sync.Configuration;
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
	internal class ImageSyncConfigurationSearchToFolderCreationTests : SyncConfigurationCreationTestsBase
	{
		private int _savedSearchId;
		private int _destinationFolderId;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup();

			_savedSearchId = await Rdos.GetSavedSearchInstanceAsync(ServiceFactory, SourceWorkspaceId).ConfigureAwait(false);

			_destinationFolderId = await Rdos.GetRootFolderInstanceAsync(ServiceFactory, DestinationWorkspaceId).ConfigureAwait(false);
		}

		[IdentifiedTest("2889569E-95A8-4BDC-9177-5220B2C2740F")]
		public async Task Create_DefaultImageSyncConfigurationSavedSearchToFolder()
		{
			// Arrange
			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);

			ISyncContext syncContext =
				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

			ImageSyncOptions options = new ImageSyncOptions(DataSourceType.SavedSearch,
				_savedSearchId, DestinationLocationType.Folder, _destinationFolderId);

			// Act
			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
				.ConfigureRdos(DefaultGuids.DefaultRdoOptions)
				.ConfigureImageSync(options)
				.SaveAsync().ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
		}

		[IdentifiedTest("89DF5EE0-3C3A-429B-B449-900F71446EFE")]
		public async Task Create_ImageSyncConfigurationWithCopyImage()
		{
			// Arrange
			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
			expectedSyncConfiguration.ImageFileCopyMode =ImportImageFileCopyMode.CopyFiles;

			ISyncContext syncContext =
				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

			ImageSyncOptions options = new ImageSyncOptions(DataSourceType.SavedSearch,
				_savedSearchId, DestinationLocationType.Folder, _destinationFolderId);
			options.CopyImagesMode = ImportImageFileCopyMode.CopyFiles;

			// Act
			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
				.ConfigureRdos(DefaultGuids.DefaultRdoOptions)
				.ConfigureImageSync(options)
				.SaveAsync().ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
		}

		[IdentifiedTest("9A0DBAF9-94F7-4D10-AFA3-939BDECEB8A3")]
		public async Task Create_ImageSyncConfigurationWithRetry()
		{
			// Arrange
			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
			expectedSyncConfiguration.JobHistoryToRetryId = JobHistory.ArtifactID;

			ISyncContext syncContext =
				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

			ImageSyncOptions options = new ImageSyncOptions(DataSourceType.SavedSearch,
				_savedSearchId, DestinationLocationType.Folder, _destinationFolderId);
			RetryOptions retryOptions = new RetryOptions(JobHistory.ArtifactID);

			// Act
			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
				.ConfigureRdos(DefaultGuids.DefaultRdoOptions)
				.ConfigureImageSync(options)
				.IsRetry(retryOptions)
				.SaveAsync().ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
		}

		private static IEnumerable<object[]> OverwriteModeDataSource => new List<object[]>()
		{
			new object[] {new OverwriteOptions(ImportOverwriteMode.AppendOnly),ImportOverwriteMode.AppendOnly, FieldOverlayBehavior.UseFieldSettings},
			new object[] {new OverwriteOptions(ImportOverwriteMode.AppendOverlay) {FieldsOverlayBehavior = FieldOverlayBehavior.ReplaceValues},
				ImportOverwriteMode.AppendOverlay, FieldOverlayBehavior.ReplaceValues},
			new object[] {new OverwriteOptions(ImportOverwriteMode.AppendOverlay) { FieldsOverlayBehavior = FieldOverlayBehavior.MergeValues },
				ImportOverwriteMode.AppendOverlay, FieldOverlayBehavior.MergeValues},
			new object[] { new OverwriteOptions(ImportOverwriteMode.OverlayOnly), ImportOverwriteMode.OverlayOnly, FieldOverlayBehavior.UseFieldSettings}
		};

		[TestCaseSource(nameof(OverwriteModeDataSource))]
		public async Task Create_ImageSyncConfigurationWithOverwriteAppendOnly(
			OverwriteOptions overwriteOptions, ImportOverwriteMode expectedOverwriteMode, FieldOverlayBehavior expectedFieldsOverlay)
		{
			// Arrange
			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
			expectedSyncConfiguration.ImportOverwriteMode = expectedOverwriteMode;
			expectedSyncConfiguration.FieldOverlayBehavior = expectedFieldsOverlay;

			ISyncContext syncContext =
				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

			ImageSyncOptions options = new ImageSyncOptions(DataSourceType.SavedSearch,
				_savedSearchId, DestinationLocationType.Folder, _destinationFolderId);

			// Act
			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
				.ConfigureRdos(DefaultGuids.DefaultRdoOptions)
				.ConfigureImageSync(options)
				.OverwriteMode(overwriteOptions)
				.SaveAsync().ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task Create_ImageSyncConfigurationSavedSearchToFolderWithProductionsAndIncludeOriginal(bool includeOriginalImages)
		{
			// Arrange
			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
			expectedSyncConfiguration.ProductionImagePrecedence = "[1,2]";
			expectedSyncConfiguration.IncludeOriginalImages = includeOriginalImages;

			ISyncContext syncContext =
				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

			ImageSyncOptions options = new ImageSyncOptions(DataSourceType.SavedSearch,
				_savedSearchId, DestinationLocationType.Folder, _destinationFolderId);
			ProductionImagePrecedenceOptions productionOptions = new ProductionImagePrecedenceOptions(
				new List<int> {1, 2}, includeOriginalImages);

			// Act
			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
				.ConfigureRdos(DefaultGuids.DefaultRdoOptions)
				.ConfigureImageSync(options)
				.ProductionImagePrecedence(productionOptions)
				.SaveAsync().ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
		}

		[Test]
		public async Task Create_ImageSyncConfigurationWithEmailNotifications()
		{
			// Arrange
			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
			expectedSyncConfiguration.EmailNotificationRecipients = "test1@relativity.com;test2@relativity.com";

			ISyncContext syncContext =
				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

			ImageSyncOptions options = new ImageSyncOptions(DataSourceType.SavedSearch,
				_savedSearchId, DestinationLocationType.Folder, _destinationFolderId);
			EmailNotificationsOptions emailOptions = new EmailNotificationsOptions(new List<string>()
			{
				"test1@relativity.com",
				"test2@relativity.com"
			});

			// Act
			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
				.ConfigureRdos(DefaultGuids.DefaultRdoOptions)
				.ConfigureImageSync(options)
				.EmailNotifications(emailOptions)
				.SaveAsync().ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
		}

		[Test]
		public async Task Create_DocumentSyncConfigurationWithCreateSavedSearchInDestination()
		{
			// Arrange
			SyncConfigurationRdo expectedSyncConfiguration = await CreateDefaultExpectedConfigurationAsync().ConfigureAwait(false);
			expectedSyncConfiguration.CreateSavedSearchInDestination = true;

			ISyncContext syncContext =
				new SyncContext(SourceWorkspaceId, DestinationWorkspaceId, JobHistory.ArtifactID);

			ImageSyncOptions options = new ImageSyncOptions(DataSourceType.SavedSearch,
				_savedSearchId, DestinationLocationType.Folder, _destinationFolderId);

			// Act
			int createdConfigurationId = await new SyncConfigurationBuilder(syncContext, SyncServicesMgr)
				.ConfigureRdos(DefaultGuids.DefaultRdoOptions)
				.ConfigureImageSync(options)
				.CreateSavedSearch(
					new CreateSavedSearchOptions(true))
				.SaveAsync().ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(createdConfigurationId, expectedSyncConfiguration).ConfigureAwait(false);
		}

		async Task<SyncConfigurationRdo> CreateDefaultExpectedConfigurationAsync(List<FieldMap> expectedFieldsMapping = null)
		{
			var fieldsMappingToSerialize = expectedFieldsMapping ?? await GetDocumentIdentifierMappingAsync(SourceWorkspaceId, DestinationWorkspaceId).ConfigureAwait(false);

			return new SyncConfigurationRdo
			{
				RdoArtifactTypeId = 10,
				DataSourceType =  DataSourceType.SavedSearch,
				DataSourceArtifactId = _savedSearchId,
				DestinationWorkspaceArtifactId = DestinationWorkspaceId,
				DataDestinationArtifactId = _destinationFolderId,
				DataDestinationType = DestinationLocationType.Folder,
				ImportOverwriteMode = ImportOverwriteMode.AppendOnly,
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings,
				JobHistoryId = JobHistory.ArtifactID,
				ImageImport = true,
				ImageFileCopyMode = ImportImageFileCopyMode.SetFileLinks,
				IncludeOriginalImages = true,

				FieldsMapping = new JSONSerializer().Serialize(fieldsMappingToSerialize)
			};
		}
	}
}
