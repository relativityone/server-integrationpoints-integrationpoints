using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.RelativitySync.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.SyncConfiguration;
using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.SyncConfiguration.Options;
using System;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using Relativity.Sync.Configuration;
using kCura.IntegrationPoints.Domain.Models;
using System.Linq;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.RelativitySync.Tests
{
	[TestFixture, Category("Unit")]
	public class IntegrationPointToSyncConverterTests
	{
		private IntegrationPointToSyncConverter _sut;

		private Mock<ISyncConfigurationBuilder> _configurationBuilderMock;
		private Mock<ISyncJobConfigurationBuilder> _syncConfigurationBuilderMock;
		private Mock<IDocumentSyncConfigurationBuilder> _documentSyncConfigurationBuilderMock;
		private Mock<IImageSyncConfigurationBuilder> _imageSyncConfigurationBuilderMock;

		private Mock<ISerializer> _serializerFake;
		private Mock<IJobHistoryService> _jobHistoryServiceFake;

		private SourceConfiguration _sourceConfiguration;
		private ExtendedImportSettings _destinationConfiguration;

		private Data.IntegrationPoint _integrationPointModel;

		private const int _JOB_HISTORY_ID = 30;
		private const int _JOB_HISTORY_TO_RETRY = 31;
		private const int _INTEGRATION_POINT_ID = 40;

		private const int _SOURCE_WORKSPACE_ID = 100;
		private const int _SAVED_SEARCH_ARTIFACT_ID = 101;

		private const int _DESTINATION_WORKSPACE_ID = 200;
		private const int _DESTINATION_FOLDER_ARTIFACT_ID = 201;

		[SetUp]
		public void SetUp()
		{
			_serializerFake = new Mock<ISerializer>();

			_jobHistoryServiceFake = new Mock<IJobHistoryService>();

			Mock<IJobHistorySyncService> jobHistorySyncService = new Mock<IJobHistorySyncService>();
			jobHistorySyncService.Setup(x => x.GetLastJobHistoryWithErrorsAsync(_SOURCE_WORKSPACE_ID, _INTEGRATION_POINT_ID))
				.ReturnsAsync(new RelativityObject { ArtifactID = _JOB_HISTORY_TO_RETRY });

			Mock<IAPILog> log = new Mock<IAPILog>();

			ISyncOperationsWrapper syncOperations = SetupSyncOperations();

			_destinationConfiguration = CreateNativeDestinationConfiguration();
			_sourceConfiguration = CreateSourceConfiguration();

			_sut = new IntegrationPointToSyncConverter(
				_serializerFake.Object,
				_jobHistoryServiceFake.Object,
				jobHistorySyncService.Object,
				log.Object,
				syncOperations);
		}

		[Test]
		public async Task CreateSyncConfigurationAsync_ShouldCreateSyncConfiguration_WhenDefaultNativeIntegrationPoint()
		{
			// Arrange
			IExtendedJob job = SetupExtendedJob();

			// Act
			await _sut.CreateSyncConfigurationAsync(job).ConfigureAwait(false);

			// Assert
			_configurationBuilderMock.Verify(x => x.ConfigureRdos(It.IsAny<RdoOptions>()));

			_syncConfigurationBuilderMock.Verify(x => x.ConfigureDocumentSync(It.Is<DocumentSyncOptions>(
				o => o.SavedSearchId == _SAVED_SEARCH_ARTIFACT_ID &&
					 o.DestinationFolderId == _DESTINATION_FOLDER_ARTIFACT_ID)));
		}

		[Test]
		public async Task CreateSyncConfigurationAsync_ShouldCreateSyncConfiguration_WhenRetryDefaultIntegrationPoint()
		{
			// Arrange
			IExtendedJob job = SetupExtendedJob(
				isRetry: true);

			// Act
			await _sut.CreateSyncConfigurationAsync(job).ConfigureAwait(false);

			// Assert
			_configurationBuilderMock.Verify(x => x.ConfigureRdos(It.IsAny<RdoOptions>()));

			_documentSyncConfigurationBuilderMock.Verify(x => x.IsRetry(It.Is<RetryOptions>(
				o => o.JobToRetry == _JOB_HISTORY_TO_RETRY)));
		}

		[TestCase(ImportNativeFileCopyModeEnum.CopyFiles, ImportNativeFileCopyMode.CopyFiles)]
		[TestCase(ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, ImportNativeFileCopyMode.DoNotImportNativeFiles)]
		[TestCase(ImportNativeFileCopyModeEnum.SetFileLinks, ImportNativeFileCopyMode.SetFileLinks)]
		public async Task CreateSyncConfigurationAsync_ShouldCreateSyncConfiguration_WhenDefaultIntegrationPointWithCopyNativesBehavior(
			ImportNativeFileCopyModeEnum copyMode, ImportNativeFileCopyMode expectedCopyMode)
		{
			// Arrange
			_destinationConfiguration.ImportNativeFileCopyMode = copyMode;

			IExtendedJob job = SetupExtendedJob();

			// Act
			await _sut.CreateSyncConfigurationAsync(job).ConfigureAwait(false);

			// Assert
			_configurationBuilderMock.Verify(x => x.ConfigureRdos(It.IsAny<RdoOptions>()));

			_syncConfigurationBuilderMock.Verify(x => x.ConfigureDocumentSync(It.Is<DocumentSyncOptions>(
				o => o.SavedSearchId == _SAVED_SEARCH_ARTIFACT_ID &&
					 o.DestinationFolderId == _DESTINATION_FOLDER_ARTIFACT_ID &&
					 o.CopyNativesMode == expectedCopyMode)));
		}

		[Test]
		public async Task CreateSyncConfigurationAsync_ShouldCreateSyncConfiguration_WhenIntegrationPointEmailNotifications()
		{
			// Arrange
			const string email1 = "test@relativity.com";
			const string email2 = "test2@relativity.com";

			string emailNotifications = $"    {email1};;  {email2}";

			IExtendedJob job = SetupExtendedJob(
				emailNotifications: emailNotifications);

			// Act
			await _sut.CreateSyncConfigurationAsync(job).ConfigureAwait(false);

			// Assert
			_documentSyncConfigurationBuilderMock.Verify(x => x.EmailNotifications(It.Is<EmailNotificationsOptions>(
				o => o.Emails.Contains(email1) &&
					 o.Emails.Contains(email2))));
		}

		[TestCase(ImportOverwriteModeEnum.AppendOnly, "Use Field Settings", ImportOverwriteMode.AppendOnly, FieldOverlayBehavior.UseFieldSettings)]
		[TestCase(ImportOverwriteModeEnum.AppendOverlay, "Merge Values", ImportOverwriteMode.AppendOverlay, FieldOverlayBehavior.MergeValues)]
		[TestCase(ImportOverwriteModeEnum.OverlayOnly, "Replace Values", ImportOverwriteMode.OverlayOnly, FieldOverlayBehavior.ReplaceValues)]
		public async Task CreateSyncConfigurationAsync_ShouldCreateSyncConfiguration_WhenIntegrationPointWithOverlayBehavior(
			ImportOverwriteModeEnum importOverwriteMode, string fieldOverlayBehavior,
			ImportOverwriteMode expectedOverwriteMode, FieldOverlayBehavior expectedFieldOverlayMode)
		{
			// Arrange
			_destinationConfiguration.ImportOverwriteMode = importOverwriteMode;
			_destinationConfiguration.FieldOverlayBehavior = fieldOverlayBehavior;

			IExtendedJob job = SetupExtendedJob();

			// Act
			await _sut.CreateSyncConfigurationAsync(job).ConfigureAwait(false);

			// Assert
			_documentSyncConfigurationBuilderMock.Verify(x => x.OverwriteMode(It.Is<OverwriteOptions>(
				o => o.OverwriteMode == expectedOverwriteMode &&
					 o.FieldsOverlayBehavior == expectedFieldOverlayMode)));
		}

		[TestCase(false, false, 0, false, DestinationFolderStructureBehavior.None)]
		[TestCase(true, false, 0, true, DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)]
		[TestCase(false, true, 1038081, true, DestinationFolderStructureBehavior.ReadFromField)]
		public async Task CreateSyncConfigurationAsync_ShouldCreateSyncConfiguration_WhenIntegrationPointWithFolderStructureBehavior(
			bool useDynamicFolderPath, bool useFolderPathInformation, int folderPathSourceFieldId,
			bool moveExistingDocuments, DestinationFolderStructureBehavior expectedFolderStructureBehavior)
		{
			// Arrange
			_destinationConfiguration.UseDynamicFolderPath = useDynamicFolderPath;
			_destinationConfiguration.UseFolderPathInformation = useFolderPathInformation;
			_destinationConfiguration.FolderPathSourceField = folderPathSourceFieldId;
			_destinationConfiguration.MoveExistingDocuments = moveExistingDocuments;

			IExtendedJob job = SetupExtendedJob();

			// Act
			await _sut.CreateSyncConfigurationAsync(job).ConfigureAwait(false);

			// Assert
			_configurationBuilderMock.Verify(x => x.ConfigureRdos(It.IsAny<RdoOptions>()));

			_documentSyncConfigurationBuilderMock.Verify(x => x.DestinationFolderStructure(It.Is<DestinationFolderStructureOptions>(
				o => o.DestinationFolderStructure == expectedFolderStructureBehavior &&
					 o.FolderPathSourceFieldId == folderPathSourceFieldId &&
					 o.MoveExistingDocuments == moveExistingDocuments)));
		}

		[TestCase(ImportNativeFileCopyModeEnum.CopyFiles, ImportImageFileCopyMode.CopyFiles)]
		[TestCase(ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, ImportImageFileCopyMode.SetFileLinks)]
		[TestCase(ImportNativeFileCopyModeEnum.SetFileLinks, ImportImageFileCopyMode.SetFileLinks)]
		public async Task CreateSyncConfigurationAsync_ShouldCreateSyncConfiguration_WhenDefaultIntegrationPointWithCopyImagesBehavior(
			ImportNativeFileCopyModeEnum copyMode, ImportImageFileCopyMode expectedCopyMode)
		{
			// Arrange
			_destinationConfiguration = CreateImageDestinationConfiguration(
				importFileCopyMode: copyMode);

			IExtendedJob job = SetupExtendedJob();

			// Act
			await _sut.CreateSyncConfigurationAsync(job).ConfigureAwait(false);

			// Assert
			_configurationBuilderMock.Verify(x => x.ConfigureRdos(It.IsAny<RdoOptions>()));

			_syncConfigurationBuilderMock.Verify(x => x.ConfigureImageSync(It.Is<ImageSyncOptions>(
				o => o.DataSourceId == _SAVED_SEARCH_ARTIFACT_ID &&
					 o.DestinationLocationId == _DESTINATION_FOLDER_ARTIFACT_ID &&
					 o.CopyImagesMode == expectedCopyMode)));
		}

		[TestCase(new int[] { 1, 2 }, true)]
		[TestCase(new int[] { 1, 2 }, false)]
		public async Task CreateSyncConfigurationAsync_ShouldCreateSyncConfiguration_WhenIntegrationPointWithImageProductionPrecedenceModeSync(
			int[] imagePrecedence, bool includeOriginalImages)
		{
			// Arrange
			_destinationConfiguration = CreateImageDestinationConfiguration(
				includeOriginalImages: includeOriginalImages,
				imagePrecedence: imagePrecedence.Select(x => new ProductionDTO { ArtifactID = x.ToString() }));
			_sourceConfiguration = CreateSourceConfiguration();

			IExtendedJob job = SetupExtendedJob();

			// Act
			await _sut.CreateSyncConfigurationAsync(job).ConfigureAwait(false);

			// Assert
			_imageSyncConfigurationBuilderMock.Verify(x => x.ProductionImagePrecedence(It.Is<ProductionImagePrecedenceOptions>(
				o => o.IncludeOriginalImagesIfNotFoundInProductions == includeOriginalImages &&
				     o.ProductionImagePrecedenceIds.SequenceEqual(imagePrecedence))));
		}
		
		[Test]
		public async Task SyncConfiguration_ShouldBeCreated_WhenIntegrationPointWithOriginalImagesAndImagesPrecedenceSelected()
		{
			// Arrange
			_destinationConfiguration = CreateImageDestinationConfiguration(
				includeOriginalImages: true,
				imagePrecedence: new[] {new ProductionDTO {ArtifactID = "1"}, new ProductionDTO {ArtifactID = "2"}});
			_sourceConfiguration = CreateSourceConfiguration();

			IExtendedJob job = SetupExtendedJob();

			// Act
			await _sut.CreateSyncConfigurationAsync(job).ConfigureAwait(false);

			// Assert
			_imageSyncConfigurationBuilderMock.Verify(x => x.ProductionImagePrecedence(It.Is<ProductionImagePrecedenceOptions>(
				o => o.IncludeOriginalImagesIfNotFoundInProductions &&
				     o.ProductionImagePrecedenceIds.ToList().Count == 0)));
		}
		private static ExtendedImportSettings CreateNativeDestinationConfiguration()
		{
			ImportSettings settings = new ImportSettings
			{
				DestinationFolderArtifactId = _DESTINATION_FOLDER_ARTIFACT_ID,
				ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles,
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				FieldOverlayBehavior = "Use Field Settings"
			};

			return new ExtendedImportSettings(settings);
		}

		private static ExtendedImportSettings CreateImageDestinationConfiguration(
			ImportNativeFileCopyModeEnum importFileCopyMode = ImportNativeFileCopyModeEnum.SetFileLinks,
			bool includeOriginalImages = true,
			IEnumerable<ProductionDTO> imagePrecedence = null)
		{
			ImportSettings settings = new ImportSettings
			{
				DestinationFolderArtifactId = _DESTINATION_FOLDER_ARTIFACT_ID,
				ImportNativeFileCopyMode = importFileCopyMode,
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				FieldOverlayBehavior = "Use Field Settings",
				ImageImport = true,
				IncludeOriginalImages = includeOriginalImages,
				ImagePrecedence = imagePrecedence ?? Array.Empty<ProductionDTO>(),
				ProductionPrecedence = imagePrecedence?.Any() == true ? "1" : "0"
			};

			return new ExtendedImportSettings(settings);
		}

		private static SourceConfiguration CreateSourceConfiguration()
		{
			return new SourceConfiguration
			{
				SavedSearchArtifactId = _SAVED_SEARCH_ARTIFACT_ID,
				SourceWorkspaceArtifactId = _SOURCE_WORKSPACE_ID,
				TargetWorkspaceArtifactId = _DESTINATION_WORKSPACE_ID,
				TypeOfExport = SourceConfiguration.ExportType.SavedSearch
			};
		}

		private IExtendedJob SetupExtendedJob(
			bool isRetry = false,
			string emailNotifications = "")
		{
			_integrationPointModel = new Data.IntegrationPoint
			{
				EmailNotificationRecipients = emailNotifications,
				SourceConfiguration = null,
				DestinationConfiguration = null
			};

			_serializerFake.Setup(x => x.Deserialize<SourceConfiguration>(It.IsAny<string>()))
				.Returns(_sourceConfiguration);
			_serializerFake.Setup(x => x.Deserialize<ImportSettings>(It.IsAny<string>()))
				.Returns(_destinationConfiguration);
			_serializerFake.Setup(x => x.Deserialize<FolderConf>(It.IsAny<string>()))
				.Returns(new FolderConf
				{
					UseDynamicFolderPath = _destinationConfiguration.UseDynamicFolderPath,
					UseFolderPathInformation = _destinationConfiguration.UseFolderPathInformation,
					FolderPathSourceField = _destinationConfiguration.FolderPathSourceField
				});
			_serializerFake.Setup(x => x.Deserialize<TaskParameters>(It.IsAny<string>()))
				.Returns(new TaskParameters { BatchInstance = It.IsAny<Guid>() });

			_jobHistoryServiceFake.Setup(x => x.GetRdo(It.IsAny<Guid>()))
				.Returns(new Data.JobHistory
				{
					JobType = isRetry ? JobTypeChoices.JobHistoryRetryErrors : JobTypeChoices.JobHistoryRun
				});

			Job job = new JobBuilder()
				.WithJobDetails(new TaskParameters())
				.Build();

			Mock<IExtendedJob> extendedJob = new Mock<IExtendedJob>();
			extendedJob.SetupGet(x => x.WorkspaceId).Returns(_SOURCE_WORKSPACE_ID);
			extendedJob.SetupGet(x => x.JobHistoryId).Returns(_JOB_HISTORY_ID);
			extendedJob.SetupGet(x => x.IntegrationPointId).Returns(_INTEGRATION_POINT_ID);
			extendedJob.SetupGet(x => x.IntegrationPointModel).Returns(_integrationPointModel);
			extendedJob.SetupGet(x => x.Job).Returns(job);

			return extendedJob.Object;
		}

		private ISyncOperationsWrapper SetupSyncOperations()
		{
			_syncConfigurationBuilderMock = new Mock<ISyncJobConfigurationBuilder>();
			_documentSyncConfigurationBuilderMock = new Mock<IDocumentSyncConfigurationBuilder>();
			_imageSyncConfigurationBuilderMock = new Mock<IImageSyncConfigurationBuilder>();

			_documentSyncConfigurationBuilderMock.Setup(x => x.CreateSavedSearch(It.IsAny<CreateSavedSearchOptions>()))
				.Returns(_documentSyncConfigurationBuilderMock.Object);
			_documentSyncConfigurationBuilderMock.Setup(x => x.DestinationFolderStructure(It.IsAny<DestinationFolderStructureOptions>()))
				.Returns(_documentSyncConfigurationBuilderMock.Object);
			_documentSyncConfigurationBuilderMock.Setup(x => x.EmailNotifications(It.IsAny<EmailNotificationsOptions>()))
				.Returns(_documentSyncConfigurationBuilderMock.Object);
			_documentSyncConfigurationBuilderMock.Setup(x => x.IsRetry(It.IsAny<RetryOptions>()))
				.Returns(_documentSyncConfigurationBuilderMock.Object);
			_documentSyncConfigurationBuilderMock.Setup(x => x.OverwriteMode(It.IsAny<OverwriteOptions>()))
				.Returns(_documentSyncConfigurationBuilderMock.Object);
			_documentSyncConfigurationBuilderMock.Setup(x => x.WithFieldsMapping(It.IsAny<Action<IFieldsMappingBuilder>>()))
				.Returns(_documentSyncConfigurationBuilderMock.Object);

			_imageSyncConfigurationBuilderMock.Setup(x => x.CreateSavedSearch(It.IsAny<CreateSavedSearchOptions>()))
				.Returns(_imageSyncConfigurationBuilderMock.Object);
			_imageSyncConfigurationBuilderMock.Setup(x => x.EmailNotifications(It.IsAny<EmailNotificationsOptions>()))
				.Returns(_imageSyncConfigurationBuilderMock.Object);
			_imageSyncConfigurationBuilderMock.Setup(x => x.IsRetry(It.IsAny<RetryOptions>()))
				.Returns(_imageSyncConfigurationBuilderMock.Object);
			_imageSyncConfigurationBuilderMock.Setup(x => x.OverwriteMode(It.IsAny<OverwriteOptions>()))
				.Returns(_imageSyncConfigurationBuilderMock.Object);
			_imageSyncConfigurationBuilderMock.Setup(x => x.ProductionImagePrecedence(It.IsAny<ProductionImagePrecedenceOptions>()))
				.Returns(_imageSyncConfigurationBuilderMock.Object);

			_syncConfigurationBuilderMock.Setup(x => x.ConfigureDocumentSync(It.IsAny<DocumentSyncOptions>()))
				.Returns(_documentSyncConfigurationBuilderMock.Object);
			_syncConfigurationBuilderMock.Setup(x => x.ConfigureImageSync(It.IsAny<ImageSyncOptions>()))
				.Returns(_imageSyncConfigurationBuilderMock.Object);

			_configurationBuilderMock = new Mock<ISyncConfigurationBuilder>();
			_configurationBuilderMock.Setup(x => x.ConfigureRdos(It.IsAny<RdoOptions>()))
				.Returns(_syncConfigurationBuilderMock.Object);

			Mock<ISyncOperationsWrapper> syncOperations = new Mock<ISyncOperationsWrapper>();
			syncOperations.Setup(x => x.GetSyncConfigurationBuilder(It.IsAny<ISyncContext>()))
				.Returns(_configurationBuilderMock.Object);

			return syncOperations.Object;
		}

		private class ExtendedImportSettings : ImportSettings
		{
			public bool UseFolderPathInformation { get; set; }

			public int FolderPathSourceField { get; set; }

			public ExtendedImportSettings(ImportSettings baseSettings)
			{
				ArtifactTypeId = baseSettings.ArtifactTypeId;
				CaseArtifactId = baseSettings.CaseArtifactId;
				Provider = baseSettings.Provider;
				ImportOverwriteMode = baseSettings.ImportOverwriteMode;
				ImportNativeFile = baseSettings.ImportNativeFile;
				ImportNativeFileCopyMode = baseSettings.ImportNativeFileCopyMode;
				ExtractedTextFieldContainsFilePath = baseSettings.ExtractedTextFieldContainsFilePath;
				FieldOverlayBehavior = baseSettings.FieldOverlayBehavior;
				RelativityUsername = baseSettings.RelativityUsername;
				RelativityPassword = baseSettings.RelativityPassword;
				DestinationProviderType = baseSettings.DestinationProviderType;
				DestinationFolderArtifactId = baseSettings.DestinationFolderArtifactId;
				FederatedInstanceArtifactId = baseSettings.FederatedInstanceArtifactId;
				ExtractedTextFileEncoding = baseSettings.ExtractedTextFileEncoding;
				ImageImport = baseSettings.ImageImport;
				IncludeOriginalImages = baseSettings.IncludeOriginalImages;
				ImagePrecedence = baseSettings.ImagePrecedence;
				ProductionPrecedence = baseSettings.ProductionPrecedence;
			}
		}
	}
}
