using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.RelativitySync.RipOverride;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using Relativity.API;
using Relativity.Testing.Identification;
using ChoiceRef = Relativity.Services.Choice.ChoiceRef;
using FieldMap = Relativity.IntegrationPoints.Services.FieldMap;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	[TestFixture]
	public class SyncConfigurationCreationTests : RelativityProviderTemplate
	{
		private IJobHistoryService _jobHistoryService;
		private IIntegrationPointService _integrationPointService;

		private IJobHistorySyncService _jobHistorySyncService;
		private SyncServiceManagerForRip _syncServicesMgr;

		public SyncConfigurationCreationTests() 
			: base("Test Workspace", null)
		{
		}

		[SetUp]
		public void SetUp()
		{
			_jobHistoryService = Container.Resolve<IJobHistoryService>();
			_integrationPointService = Container.Resolve<IIntegrationPointService>();
			_jobHistorySyncService = new JobHistorySyncService(Helper);
			_syncServicesMgr = new SyncServiceManagerForRip(Helper.GetServicesManager());
		}

		[IdentifiedTest("251679FD-D739-4479-AE2A-351BEB1A6D58")]
		public async Task SyncConfiguration_ShouldBeCreated_WhenDefaultIntegrationPoint()
		{
			// Arrange
			ImportSettings destinationConfiguration = CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.AppendOnly, TargetWorkspaceArtifactID);
			SourceConfiguration sourceConfiguration = CreateSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactID);

			IntegrationPointModel integrationPoint =
				CreateDefaultIntegrationPointModel(sourceConfiguration, destinationConfiguration, GetDefaultFieldMap(false));

			SyncConfigurationRDO expectedSyncConfigurationRdo = SyncConfigurationRDO.CreateDefaultDocumentSyncConfiguration(
				Serializer, Logger, integrationPoint, sourceConfiguration, destinationConfiguration);

			IExtendedJob extendedJob = CreateExtendedJob(integrationPoint);

			var sut = GetSut();

			// Act
			int configurationId = await sut.CreateSyncConfigurationAsync(extendedJob, _syncServicesMgr).ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(configurationId, expectedSyncConfigurationRdo).ConfigureAwait(false);
		}

		[IdentifiedTest("4D43F811-B649-4CD0-86B2-DFA9013D51DA")]
		public async Task SyncConfiguration_ShouldBeCreated_WhenIntegrationPointWithEmailNotifications()
		{
			// Arrange
			const string emailNotifications = "    test@relativity.com;;  test2@relativity.com";

			ImportSettings destinationConfiguration = CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.AppendOnly, TargetWorkspaceArtifactID);
			SourceConfiguration sourceConfiguration = CreateSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactID);

			IntegrationPointModel integrationPoint =
				CreateDefaultIntegrationPointModel(sourceConfiguration, destinationConfiguration, GetDefaultFieldMap(false));
			integrationPoint.NotificationEmails = emailNotifications;

			SyncConfigurationRDO expectedSyncConfigurationRdo = SyncConfigurationRDO.CreateDefaultDocumentSyncConfiguration(
				Serializer, Logger, integrationPoint, sourceConfiguration, destinationConfiguration);
			expectedSyncConfigurationRdo.EmailNotificationRecipients = "test@relativity.com;test2@relativity.com";

			IExtendedJob extendedJob = CreateExtendedJob(integrationPoint);

			var sut = GetSut();

			// Act
			int configurationId = await sut.CreateSyncConfigurationAsync(extendedJob, _syncServicesMgr).ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(configurationId, expectedSyncConfigurationRdo).ConfigureAwait(false);
		}

		[IdentifiedTestCase("1811F5ED-A05D-4000-A877-7DAFC07FC7E5", ImportOverwriteModeEnum.AppendOnly, "Use Field Settings", "AppendOnly")]
		[IdentifiedTestCase("72D08758-A4BB-4D15-9AE2-CC1898B7A1EE", ImportOverwriteModeEnum.AppendOverlay, "Merge Values", "AppendOverlay")]
		[IdentifiedTestCase("BC7B75F2-9BAB-461B-8434-6226C479EDC5", ImportOverwriteModeEnum.OverlayOnly, "Replace Values", "OverlayOnly")]
		public async Task SyncConfiguration_ShouldBeCreated_WhenIntegrationPointWithOverlayBehavior(
			ImportOverwriteModeEnum importOverwriteMode, string fieldOverlayBehavior, string syncOverwriteMode)
		{
			// Arrange
			ImportSettings destinationConfiguration = CreateDestinationConfigWithTargetWorkspace(importOverwriteMode, TargetWorkspaceArtifactID);
			destinationConfiguration.FieldOverlayBehavior = fieldOverlayBehavior;

			SourceConfiguration sourceConfiguration = CreateSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactID);

			IntegrationPointModel integrationPoint =
				CreateDefaultIntegrationPointModel(sourceConfiguration, destinationConfiguration, GetDefaultFieldMap(false));

			SyncConfigurationRDO expectedSyncConfigurationRdo = SyncConfigurationRDO.CreateDefaultDocumentSyncConfiguration(
				Serializer, Logger, integrationPoint, sourceConfiguration, destinationConfiguration);
			expectedSyncConfigurationRdo.ImportOverwriteMode = syncOverwriteMode;
			expectedSyncConfigurationRdo.FieldOverlayBehavior = fieldOverlayBehavior;

			IExtendedJob extendedJob = CreateExtendedJob(integrationPoint);

			var sut = GetSut();

			// Act
			int configurationId = await sut.CreateSyncConfigurationAsync(extendedJob, _syncServicesMgr).ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(configurationId, expectedSyncConfigurationRdo).ConfigureAwait(false);
		}

		[IdentifiedTestCase("BE874772-F860-42FD-ACA7-2B584642FDB2", false, false, 0, false, "None", null)]
		[IdentifiedTestCase("8679144F-8EBF-4F54-A5AC-005FA81DA775", true, false, 0, true, "RetainSourceWorkspaceStructure", null)]
		[IdentifiedTestCase("FDB75908-3712-4ABB-9757-94A88D78CD5B", false, true, 1038081, true, "ReadFromField", "Document Folder Path")]
		public async Task SyncConfiguration_ShouldBeCreated_WhenIntegrationPointWithFolderStructureBehavior(
			bool useDynamicFolderPath, bool useFolderPathInformation, int folderPathSourceFieldId,
			bool moveExistingDocuments, string syncFolderStructureBehavior, string syncFolderFieldName)
		{
			// Arrange
			ImportSettings destinationConfiguration = CreateDestinationConfigWithTargetWorkspace(
				ImportOverwriteModeEnum.AppendOnly, TargetWorkspaceArtifactID);
			ExtendedImportSettings destinationConfigurationExt =
				new ExtendedImportSettings(destinationConfiguration)
				{
					UseDynamicFolderPath = useDynamicFolderPath,
					UseFolderPathInformation = useFolderPathInformation,
					FolderPathSourceField = folderPathSourceFieldId
				};
			destinationConfigurationExt.MoveExistingDocuments = moveExistingDocuments;

			SourceConfiguration sourceConfiguration = CreateSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactID);

			IntegrationPointModel integrationPoint =
				CreateDefaultIntegrationPointModel(sourceConfiguration, destinationConfigurationExt, GetDefaultFieldMap(false));

			SyncConfigurationRDO expectedSyncConfigurationRdo = SyncConfigurationRDO.CreateDefaultDocumentSyncConfiguration(
				Serializer, Logger, integrationPoint, sourceConfiguration, destinationConfigurationExt);
			expectedSyncConfigurationRdo.DestinationFolderStructureBehavior = syncFolderStructureBehavior;
			expectedSyncConfigurationRdo.FolderPathSourceFieldName = syncFolderFieldName;
			expectedSyncConfigurationRdo.MoveExistingDocuments = moveExistingDocuments;

			IExtendedJob extendedJob = CreateExtendedJob(integrationPoint);

			var sut = GetSut();

			// Act
			int configurationId = await sut.CreateSyncConfigurationAsync(extendedJob, _syncServicesMgr).ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(configurationId, expectedSyncConfigurationRdo).ConfigureAwait(false);
		}

		[IdentifiedTestCase("4C68E4F4-BD37-44A8-A9F8-9E78B9E2715D", ImportNativeFileCopyModeEnum.CopyFiles, "Copy")]
		[IdentifiedTestCase("E21F3341-4195-4EC1-A6E5-3F30F8C3E82C", ImportNativeFileCopyModeEnum.SetFileLinks, "Link")]
		[IdentifiedTestCase("4FE0309E-2B31-4C47-8656-CA540B110795", ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, "None")]
		public async Task SyncConfiguration_ShouldBeCreated_WhenDefaultIntegrationPointWithCopyNativesBehavior(
			ImportNativeFileCopyModeEnum nativeCopyMode, string expectedNativeCopyMode)
		{
			// Arrange
			ImportSettings destinationConfiguration = CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.AppendOnly, TargetWorkspaceArtifactID);
			destinationConfiguration.ImportNativeFileCopyMode = nativeCopyMode;

			SourceConfiguration sourceConfiguration = CreateSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactID);

			IntegrationPointModel integrationPoint =
				CreateDefaultIntegrationPointModel(sourceConfiguration, destinationConfiguration, GetDefaultFieldMap(false));

			SyncConfigurationRDO expectedSyncConfigurationRdo = SyncConfigurationRDO.CreateDefaultDocumentSyncConfiguration(
				Serializer, Logger, integrationPoint, sourceConfiguration, destinationConfiguration);
			expectedSyncConfigurationRdo.NativesBehavior = expectedNativeCopyMode;

			IExtendedJob extendedJob = CreateExtendedJob(integrationPoint);

			var sut = GetSut();

			// Act
			int configurationId = await sut.CreateSyncConfigurationAsync(extendedJob, _syncServicesMgr).ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(configurationId, expectedSyncConfigurationRdo).ConfigureAwait(false);
		}

		[IdentifiedTestCase("A747F687-0F17-4AAC-8B2C-3EE7A7A6F691", ImportNativeFileCopyModeEnum.CopyFiles, "Copy")]
		[IdentifiedTestCase("A5E73F2E-AFC6-4213-BB99-EB2BF89318EA", ImportNativeFileCopyModeEnum.DoNotImportNativeFiles, "Link")]
		[IdentifiedTestCase("C73EDC5E-EB5F-42C9-9282-026EE8E1DA66", ImportNativeFileCopyModeEnum.SetFileLinks, "Link")]
		public async Task SyncConfiguration_ShouldBeCreated_WhenIntegrationPointWithCopyImageModeSync( 
			ImportNativeFileCopyModeEnum imageCopyMode, string syncImageCopyMode)
		{
			// Arrange
			ImportSettings destinationConfiguration = CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.AppendOnly, TargetWorkspaceArtifactID);
			destinationConfiguration.ImageImport = true;
			destinationConfiguration.IncludeOriginalImages = true;
			destinationConfiguration.ImportNativeFileCopyMode = imageCopyMode;
			destinationConfiguration.ImagePrecedence = Array.Empty<ProductionDTO>();
			
			SourceConfiguration sourceConfiguration = CreateSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactID);

			IntegrationPointModel integrationPoint =
				CreateDefaultIntegrationPointModel(sourceConfiguration, destinationConfiguration, GetDefaultFieldMap(false));

			SyncConfigurationRDO expectedSyncConfigurationRdo = SyncConfigurationRDO.CreateDefaultImageSyncConfiguration(
				Serializer, Logger, integrationPoint, sourceConfiguration, destinationConfiguration);
			expectedSyncConfigurationRdo.ImageImport = true;
			expectedSyncConfigurationRdo.IncludeOriginalImages = true;
			expectedSyncConfigurationRdo.ImageFileCopyMode = syncImageCopyMode;
			expectedSyncConfigurationRdo.ProductionImagePrecedence = "[]";

			IExtendedJob extendedJob = CreateExtendedJob(integrationPoint);

			var sut = GetSut();

			// Act
			int configurationId = await sut.CreateSyncConfigurationAsync(extendedJob, _syncServicesMgr).ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(configurationId, expectedSyncConfigurationRdo).ConfigureAwait(false);
		}

		[IdentifiedTestCase("53F333C2-DDD4-46CE-9A01-40C8BB1E0514", true, "[1,2]")]
		[IdentifiedTestCase("54DA4D85-6CE6-45E3-B2AE-942877DB3E9B", false, "[1,2]")]
		public async Task SyncConfiguration_ShouldBeCreated_WhenIntegrationPointWithImageProductionPrecedenceModeSync(
			bool includeOriginalImages, string syncProductionImagePrecedence)
		{
			// Arrange
			ImportSettings destinationConfiguration = CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.AppendOnly, TargetWorkspaceArtifactID);
			destinationConfiguration.ImageImport = true;
			destinationConfiguration.IncludeOriginalImages = includeOriginalImages;
			destinationConfiguration.ImagePrecedence =
				new[] { new ProductionDTO { ArtifactID = "1" }, new ProductionDTO { ArtifactID = "2" } };
			destinationConfiguration.ProductionPrecedence = "1";

			SourceConfiguration sourceConfiguration = CreateSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactID);

			IntegrationPointModel integrationPoint =
				CreateDefaultIntegrationPointModel(sourceConfiguration, destinationConfiguration, GetDefaultFieldMap(false));

			SyncConfigurationRDO expectedSyncConfigurationRdo = SyncConfigurationRDO.CreateDefaultImageSyncConfiguration(
				Serializer, Logger, integrationPoint, sourceConfiguration, destinationConfiguration);
			expectedSyncConfigurationRdo.ImageImport = true;
			expectedSyncConfigurationRdo.ImageFileCopyMode = "Link";
			expectedSyncConfigurationRdo.IncludeOriginalImages = includeOriginalImages;
			expectedSyncConfigurationRdo.ProductionImagePrecedence = syncProductionImagePrecedence;

			IExtendedJob extendedJob = CreateExtendedJob(integrationPoint);

			var sut = GetSut();

			// Act
			int configurationId = await sut.CreateSyncConfigurationAsync(extendedJob, _syncServicesMgr).ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(configurationId, expectedSyncConfigurationRdo).ConfigureAwait(false);
		}
		
		[IdentifiedTestCase("03FC30D8-854A-41B9-A144-DF622AD0A3F8")]
		public async Task SyncConfiguration_ShouldBeCreated_WhenIntegrationPointWithOriginalImagesAndImagesPrecedenceSelected()
		{
			// Arrange
			ImportSettings destinationConfiguration = CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.AppendOnly, TargetWorkspaceArtifactID);
			destinationConfiguration.ImageImport = true;
			destinationConfiguration.IncludeOriginalImages = true;
			destinationConfiguration.ImagePrecedence =
				new[] { new ProductionDTO { ArtifactID = "1" }, new ProductionDTO { ArtifactID = "2" } };
			destinationConfiguration.ProductionPrecedence = "0";

			SourceConfiguration sourceConfiguration = CreateSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactID);

			IntegrationPointModel integrationPoint =
				CreateDefaultIntegrationPointModel(sourceConfiguration, destinationConfiguration, GetDefaultFieldMap(false));

			SyncConfigurationRDO expectedSyncConfigurationRdo = SyncConfigurationRDO.CreateDefaultImageSyncConfiguration(
				Serializer, Logger, integrationPoint, sourceConfiguration, destinationConfiguration);
			expectedSyncConfigurationRdo.ImageImport = true;
			expectedSyncConfigurationRdo.IncludeOriginalImages = true;
			expectedSyncConfigurationRdo.ImageFileCopyMode = "Link";
			expectedSyncConfigurationRdo.ProductionImagePrecedence = "[]";

			IExtendedJob extendedJob = CreateExtendedJob(integrationPoint);

			var sut = GetSut();

			// Act
			int configurationId = await sut.CreateSyncConfigurationAsync(extendedJob, _syncServicesMgr).ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(configurationId, expectedSyncConfigurationRdo).ConfigureAwait(false);
		}

		[IdentifiedTest("5B14D16A-3E9A-47DD-878B-7BDFF24CB457")]
		public async Task SyncConfiguration_ShouldBeCreated_WhenRetryDefaultIntegrationPoint()
		{
			// Arrange
			ImportSettings destinationConfiguration = CreateDestinationConfigWithTargetWorkspace(ImportOverwriteModeEnum.AppendOnly, TargetWorkspaceArtifactID);
			SourceConfiguration sourceConfiguration = CreateSourceConfigWithTargetWorkspace(TargetWorkspaceArtifactID);

			IntegrationPointModel integrationPoint =
				CreateDefaultIntegrationPointModel(sourceConfiguration, destinationConfiguration, GetDefaultFieldMap(false));
			
			IExtendedJob extendedJob = CreateRetryExtendedJob(integrationPoint, out JobHistory jobHistory);

			SyncConfigurationRDO expectedSyncConfigurationRdo = SyncConfigurationRDO.CreateDefaultDocumentSyncConfiguration(
				Serializer, Logger, integrationPoint, sourceConfiguration, destinationConfiguration, jobHistory);

			var sut = GetSut();

			// Act
			int configurationId = await sut.CreateSyncConfigurationAsync(extendedJob, _syncServicesMgr).ConfigureAwait(false);

			// Assert
			await AssertCreatedConfigurationAsync(configurationId, expectedSyncConfigurationRdo).ConfigureAwait(false);
		}

		private IntegrationPointToSyncConverter GetSut()
		{
			return new IntegrationPointToSyncConverter(
				Serializer, _jobHistoryService, _jobHistorySyncService, Logger);
		}

		private async Task AssertCreatedConfigurationAsync(int createdConfigurationId, SyncConfigurationRDO expectedConfiguration)
		{
			var createdSyncConfiguration = await ReadSyncConfigurationAsync(createdConfigurationId).ConfigureAwait(false);

			createdSyncConfiguration.ShouldBeEquivalentTo(expectedConfiguration);
		}

		private IntegrationPointModel CreateDefaultIntegrationPointModel(
			SourceConfiguration sourceConfig, ImportSettings destinationConfig, FieldMap[] fieldMap)
		{
			return new IntegrationPointModel
			{
				Destination = Serializer.Serialize(destinationConfig),
				DestinationProvider = RelativityDestinationProviderArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = Serializer.Serialize(sourceConfig),
				LogErrors = true,
				NotificationEmails = null,
				Name = Guid.NewGuid().ToString(),
				SelectedOverwrite = "Append Only",
				Scheduler = new Scheduler() { EnableScheduler = false },
				Map = Serializer.Serialize(fieldMap),
				Type = Container.Resolve<IIntegrationPointTypeService>()
					.GetIntegrationPointType(Core.Constants.IntegrationPoints.IntegrationPointTypes
						.ExportGuid)
					.ArtifactId
			};

		}
		
		private IExtendedJob CreateExtendedJob(IntegrationPointModel integrationPoint)
		{
			Guid jobHistoryGuid = Guid.NewGuid();

			integrationPoint = CreateIntegrationPointWithLinkedJobHistory(integrationPoint, JobTypeChoices.JobHistoryRun, jobHistoryGuid, out JobHistory jobHistory);

			Job job = new JobBuilder()
				.WithRelatedObjectArtifactId(integrationPoint.ArtifactID)
				.WithJobDetails(new TaskParameters { BatchInstance = jobHistoryGuid })
				.WithWorkspaceId(SourceWorkspaceArtifactID)
				.Build();

			return new ExtendedJob(job, _jobHistoryService, _integrationPointService, Serializer, Logger);
		}

		private IExtendedJob CreateRetryExtendedJob(IntegrationPointModel integrationPoint, out JobHistory jobHistory)
		{
			Guid jobHistoryGuid = Guid.NewGuid();

			integrationPoint = CreateIntegrationPointWithLinkedJobHistory(integrationPoint,
				JobTypeChoices.JobHistoryRetryErrors, jobHistoryGuid, out JobHistory history);

			Job job = new JobBuilder()
				.WithRelatedObjectArtifactId(integrationPoint.ArtifactID)
				.WithJobDetails(new TaskParameters { BatchInstance = jobHistoryGuid })
				.WithWorkspaceId(SourceWorkspaceArtifactID)
				.Build();

			SetupRetryJobHistory(history);

			jobHistory = history;

			return new ExtendedJob(job, _jobHistoryService, _integrationPointService, Serializer, Logger);
		}

		private IntegrationPointModel CreateIntegrationPointWithLinkedJobHistory(IntegrationPointModel integrationPoint, ChoiceRef jobType, Guid jobHistoryBatchInstanceGuid, out JobHistory jobHistory)
		{
			var integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPoint);

			jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointModel.ArtifactID, jobHistoryBatchInstanceGuid, jobType);

			return integrationPointModel;
		}

		private void SetupRetryJobHistory(JobHistory jobHistory)
		{
			using (IObjectManager objectManager = Helper.CreateProxy<IObjectManager>())
			{
				var request = new ReadRequest
				{
					Object = new RelativityObjectRef() { ArtifactID = jobHistory.ArtifactId }
				};

				var result = objectManager.ReadAsync(SourceWorkspaceArtifactID, request).GetAwaiter().GetResult();

				Mock<IJobHistorySyncService> jobHistorySyncService = new Mock<IJobHistorySyncService>();
				jobHistorySyncService.Setup(x =>
						x.GetLastJobHistoryWithErrorsAsync(It.IsAny<int>(), It.IsAny<int>()))
					.ReturnsAsync(result.Object);

				_jobHistorySyncService = jobHistorySyncService.Object;
			}
		}

		private async Task<SyncConfigurationRDO> ReadSyncConfigurationAsync(int configurationId)
		{
			T ReadSyncConfigurationValue<T>(RelativityObject configurationRdo, Guid fieldGuid)
			{
				object val = configurationRdo.FieldValues.Single(x => x.Field.Guids.Contains(fieldGuid)).Value;

				return val == null ? default(T) : (T)val;
			}

			RelativityObject configuration;
			using (IObjectManager objectManager = Helper.CreateProxy<IObjectManager>())
			{
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = new Guid("3BE3DE56-839F-4F0E-8446-E1691ED5FD57")
					},
					Fields = new List<FieldRef>
					{
						new FieldRef {Name = "*"}
					},
					Condition = $"'ArtifactID' == {configurationId}",
				};

				var result = await objectManager.QueryAsync(SourceWorkspaceArtifactID, request, 0, 1).ConfigureAwait(false);

				configuration = result.Objects.Single();
			}

			RelativityObjectValue jobHistoryToRetryValue =
				ReadSyncConfigurationValue<RelativityObjectValue>(configuration,
					SyncConfigurationRDO.JobHistoryToRetryGuid);

			return new SyncConfigurationRDO
			{
				CreateSavedSearchInDestination = ReadSyncConfigurationValue<bool>(configuration, SyncConfigurationRDO.CreateSavedSearchInDestinationGuid),
				DataDestinationArtifactId = ReadSyncConfigurationValue<int>(configuration, SyncConfigurationRDO.DataDestinationArtifactIdGuid),
				DataDestinationType = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRDO.DataDestinationTypeGuid),
				DataSourceArtifactId = ReadSyncConfigurationValue<int>(configuration, SyncConfigurationRDO.DataSourceArtifactIdGuid),
				DataSourceType = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRDO.DataSourceTypeGuid),
				DestinationFolderStructureBehavior = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRDO.DestinationFolderStructureBehaviorGuid),
				DestinationWorkspaceArtifactId = ReadSyncConfigurationValue<int>(configuration, SyncConfigurationRDO.DestinationWorkspaceArtifactIdGuid),
				EmailNotificationRecipients = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRDO.EmailNotificationRecipientsGuid),
				FieldMappings = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRDO.FieldMappingsGuid),
				FieldOverlayBehavior = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRDO.FieldOverlayBehaviorGuid),
				FolderPathSourceFieldName = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRDO.FolderPathSourceFieldNameGuid),
				ImportOverwriteMode = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRDO.ImportOverwriteModeGuid),
				MoveExistingDocuments = ReadSyncConfigurationValue<bool>(configuration, SyncConfigurationRDO.MoveExistingDocumentsGuid),
				NativesBehavior = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRDO.NativesBehaviorGuid),
				RDOArtifactTypeId = ReadSyncConfigurationValue<int>(configuration, SyncConfigurationRDO.RdoArtifactTypeIdGuid),
				JobHistoryToRetry = jobHistoryToRetryValue == null ? null : GetBasicRelativityObject(jobHistoryToRetryValue.ArtifactID),
				ImageImport = ReadSyncConfigurationValue<bool>(configuration, SyncConfigurationRDO.ImageImportGuid),
				IncludeOriginalImages = ReadSyncConfigurationValue<bool>(configuration, SyncConfigurationRDO.IncludeOriginalImagesGuid),
				ProductionImagePrecedence = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRDO.ProductionImagePrecedenceGuid),
				ImageFileCopyMode = ReadSyncConfigurationValue<string>(configuration, SyncConfigurationRDO.ImageFileCopyModeGuid)
			};
		}

		static RelativityObject GetBasicRelativityObject(int artifactId)
		{
			return new RelativityObject() {ArtifactID = artifactId };
		}

		private class SyncConfigurationRDO
		{
			public bool CreateSavedSearchInDestination { get; set; }
			public int DataDestinationArtifactId { get; set; }
			public string DataDestinationType { get; set; }
			public int DataSourceArtifactId { get; set; }
			public string DataSourceType { get; set; }
			public string DestinationFolderStructureBehavior { get; set; }
			public int DestinationWorkspaceArtifactId { get; set; }
			public string EmailNotificationRecipients { get; set; }
			public string FieldMappings { get; set; }
			public string FieldOverlayBehavior { get; set; }
			public string FolderPathSourceFieldName { get; set; }
			public string ImportOverwriteMode { get; set; }
			public bool MoveExistingDocuments { get; set; }
			public string NativesBehavior { get; set; }
			public int RDOArtifactTypeId { get; set; }
			public RelativityObject JobHistoryToRetry { get; set; }
			public bool ImageImport { get; set; }
			public bool IncludeOriginalImages { get; set; }
			public string ProductionImagePrecedence { get; set; }
			public string ImageFileCopyMode { get; set; }

			public static SyncConfigurationRDO CreateDefaultDocumentSyncConfiguration(ISerializer serializer,
				IAPILog logger, IntegrationPointModel integrationPoint, SourceConfiguration sourceConfiguration, 
				ImportSettings destinationConfiguration, JobHistory jobHistory = null)
			{
				return new SyncConfigurationRDO()
				{
					RDOArtifactTypeId = 10,
					CreateSavedSearchInDestination = destinationConfiguration.CreateSavedSearchForTagging,
					DataDestinationArtifactId = destinationConfiguration.DestinationFolderArtifactId,
					DataDestinationType = "Folder",
					DataSourceArtifactId = sourceConfiguration.SavedSearchArtifactId,
					DataSourceType = "SavedSearch",
					DestinationWorkspaceArtifactId = sourceConfiguration.TargetWorkspaceArtifactId,
					EmailNotificationRecipients = integrationPoint.NotificationEmails ?? "",
					FieldMappings = serializer.Serialize(
						FieldMapHelper.FixedSyncMapping(integrationPoint.Map, serializer, logger)),

					DestinationFolderStructureBehavior = "None",
					FolderPathSourceFieldName = null,
					MoveExistingDocuments = false,

					NativesBehavior = "None",

					ImportOverwriteMode = "AppendOnly",
					FieldOverlayBehavior = "Use Field Settings",

					ImageImport = false,
					ImageFileCopyMode = null,
					IncludeOriginalImages = false,
					ProductionImagePrecedence = null,

					JobHistoryToRetry = jobHistory == null ? null : GetBasicRelativityObject(jobHistory.ArtifactId)
				};
			}

			public static SyncConfigurationRDO CreateDefaultImageSyncConfiguration(ISerializer serializer, 
				IAPILog logger, IntegrationPointModel integrationPoint, SourceConfiguration sourceConfiguration,
				ImportSettings destinationConfiguration, JobHistory jobHistory = null)
			{
				return new SyncConfigurationRDO()
				{
					RDOArtifactTypeId = 10,
					CreateSavedSearchInDestination = destinationConfiguration.CreateSavedSearchForTagging,
					DataDestinationArtifactId = destinationConfiguration.DestinationFolderArtifactId,
					DataDestinationType = "Folder",
					DataSourceArtifactId = sourceConfiguration.SavedSearchArtifactId,
					DataSourceType = "SavedSearch",
					DestinationWorkspaceArtifactId = sourceConfiguration.TargetWorkspaceArtifactId,
					EmailNotificationRecipients = integrationPoint.NotificationEmails ?? "",
					FieldMappings = serializer.Serialize(
						FieldMapHelper.FixedSyncMapping(integrationPoint.Map, serializer, logger)),

					DestinationFolderStructureBehavior = null,
					FolderPathSourceFieldName = null,
					MoveExistingDocuments = false,

					NativesBehavior = null,

					ImportOverwriteMode = "AppendOnly",
					FieldOverlayBehavior = "Use Field Settings",

					ImageImport = true,
					ImageFileCopyMode = "Link",
					IncludeOriginalImages = true,
					ProductionImagePrecedence = "[]",

					JobHistoryToRetry = jobHistory == null ? null : GetBasicRelativityObject(jobHistory.ArtifactId)
				};
			}

			public static readonly Guid CreateSavedSearchInDestinationGuid = new Guid("BFAB4AF6-4704-4A12-A8CA-C96A1FBCB77D");
			public static readonly Guid DataDestinationArtifactIdGuid = new Guid("0E9D7B8E-4643-41CC-9B07-3A66C98248A1");
			public static readonly Guid DataDestinationTypeGuid = new Guid("86D9A34A-B394-41CF-BFF4-BD4FF49A932D");
			public static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
			public static readonly Guid DataSourceTypeGuid = new Guid("A00E6BC1-CA1C-48D9-9712-629A63061F0D");
			public static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");
			public static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
			public static readonly Guid EmailNotificationRecipientsGuid = new Guid("4F03914D-9E86-4B72-B75C-EE48FEEBB583");
			public static readonly Guid FieldMappingsGuid = new Guid("E3CB5C64-C726-47F8-9CB0-1391C5911628");
			public static readonly Guid FieldOverlayBehaviorGuid = new Guid("34ECB263-1370-4D6C-AC11-558447504EC4");
			public static readonly Guid FolderPathSourceFieldNameGuid = new Guid("66A37443-EF92-47ED-BEEA-392464C853D3");
			public static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
			public static readonly Guid MoveExistingDocumentsGuid = new Guid("26F9BF88-420D-4EFF-914B-C47BA36E10BF");
			public static readonly Guid NativesBehaviorGuid = new Guid("D18F0199-7096-4B0C-AB37-4C9A3EA1D3D2");
			public static readonly Guid RdoArtifactTypeIdGuid = new Guid("4DF15F2B-E566-43CE-830D-671BD0786737");

			public static readonly Guid JobHistoryToRetryGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");

			public static readonly Guid ImageImportGuid = new Guid("b282bbe4-7b32-41d1-bb50-960a0e483bb5");
			public static readonly Guid IncludeOriginalImagesGuid = new Guid("f2cad5c5-63d5-49fc-bd47-885661ef1d8b");
			public static readonly Guid ProductionImagePrecedenceGuid = new Guid("421cf05e-bab4-4455-a9ca-fa83d686b5ed");
			public static readonly Guid ImageFileCopyModeGuid = new Guid("bd5dc6d2-faa2-4312-8dc0-4d1b6945dfe1");
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
				ExtractedTextFieldContainsFilePath = baseSettings.ExtractedTextFieldContainsFilePath;
				FieldOverlayBehavior = baseSettings.FieldOverlayBehavior;
				RelativityUsername = baseSettings.RelativityUsername;
				RelativityPassword = baseSettings.RelativityPassword;
				DestinationProviderType = baseSettings.DestinationProviderType;
				DestinationFolderArtifactId = baseSettings.DestinationFolderArtifactId;
				FederatedInstanceArtifactId = baseSettings.FederatedInstanceArtifactId;
				ExtractedTextFileEncoding = baseSettings.ExtractedTextFileEncoding;
			}
		}
	}
}
