using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.Interfaces.Private.Models.IntegrationPoint;
using Relativity.IntegrationPoints.Services.Tests.Integration.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ItShouldCreateIntegrationPoint : RelativityProviderTemplate
	{
		private IIntegrationPointManager _client;

		public ItShouldCreateIntegrationPoint() : base($"create_s_{Utils.FormattedDateTimeNow}", $"create_d_{Utils.FormattedDateTimeNow}")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_client = Helper.CreateProxy<IIntegrationPointManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		private class Credentials
		{
			public string Username { get; set; }
			public string Password { get; set; }
		}

		[IdentifiedTest("3a9a3545-a6d3-4cff-82b6-7cc9694f5884")]
		public void ItShouldCreateExportToLoadFileIntegrationPoint()
		{
			OverwriteFieldsModel overwriteFieldsModel = _client.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactID).GetAwaiter().GetResult().First(x => x.Name == "Append/Overlay");

			var sourceConfiguration = new RelativityProviderSourceConfiguration
			{
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactID,
				SavedSearchArtifactId = SavedSearchArtifactID
			};

			var destinationConfiguration = new
			{
				CaseArtifactId = SourceWorkspaceArtifactID,
				ArtifactTypeId = (int) ArtifactType.Document,
				AppendOriginalFileName = false,
				DataFileEncodingType = "UTF-16",
				ExportFullTextAsFile = false,
				ExportImages = false,
				ExportMultipleChoiceFieldsAsNested = false,
				ExportNatives = false,
				ExportType = "SavedSearch",
				StartExportAtRecord = 1,
				Fileshare = "\\localhost\\Export",
				IncludeOriginalImages = false,
				OverwriteFiles = true,
				FilePath = "Relative",
				SelectedDataFileFormat = "Concordance",
				SelectedImageDataFileFormat = "Opticon",
				SelectedImageFileType = "SinglePage",
				SubdirectoryDigitPadding = 3,
				SubdirectoryImagePrefix = "IMG",
				SubdirectoryMaxFiles = 500,
				SubdirectoryNativePrefix = "NATIVE",
				SubdirectoryStartNumber = 1,
				SubdirectoryTextPrefix = "TEXT",
				TextFileEncodingType = "",
				UserPrefix = "",
				VolumeDigitPadding = 2,
				VolumeMaxSize = 4400,
				VolumePrefix = "VOL",
				VolumeStartNumber = 1,
				IncludeNativeFilesPath = false,
				IsAutomaticFolderCreationEnabled = true
			};
			var integrationPointModel = new IntegrationPointModel
			{
				Name = "export_lf_ip",
				DestinationProvider =
					IntegrationPointBaseHelper.GetDestinationProviderArtifactId(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.LOADFILE, SourceWorkspaceArtifactID, Helper),
				SourceProvider = IntegrationPointBaseHelper.GetSourceProviderArtifactId(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY, SourceWorkspaceArtifactID, Helper),
				Type = IntegrationPointBaseHelper.GetTypeArtifactId(Helper, SourceWorkspaceArtifactID, "Export"),
				DestinationConfiguration = destinationConfiguration,
				SourceConfiguration = sourceConfiguration,
				EmailNotificationRecipients = string.Empty,
				FieldMappings = GetDefaultFieldMap().ToList(),
				LogErrors = true,
				OverwriteFieldsChoiceId = overwriteFieldsModel.ArtifactId,
				ScheduleRule = new ScheduleModel
				{
					EnableScheduler = false
				}
			};
			var createRequest = new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactID,
				IntegrationPoint = integrationPointModel
			};

			IntegrationPointModel createdIntegrationPoint = _client.CreateIntegrationPointAsync(createRequest).GetAwaiter().GetResult();

			IntegrationPoint actualIntegrationPoint =
				IntegrationPointRepository.ReadWithFieldMappingAsync(createdIntegrationPoint.ArtifactId).GetAwaiter().GetResult();
			IntegrationPointModel expectedIntegrationPointModel = createRequest.IntegrationPoint;

			Assert.That(expectedIntegrationPointModel.Name, Is.EqualTo(actualIntegrationPoint.Name));
			Assert.That(expectedIntegrationPointModel.DestinationProvider, Is.EqualTo(actualIntegrationPoint.DestinationProvider));
			Assert.That(expectedIntegrationPointModel.SourceProvider, Is.EqualTo(actualIntegrationPoint.SourceProvider));
			Assert.That(expectedIntegrationPointModel.Type, Is.EqualTo(actualIntegrationPoint.Type));
			Assert.That(expectedIntegrationPointModel.LogErrors, Is.EqualTo(actualIntegrationPoint.LogErrors));
		}

		[IdentifiedTest("238baa40-e640-4db3-88ba-f496ec624e60")]
		public void ItShouldCreateIntegrationPointBasedOnProfile()
		{
			const string integrationPointName = "ip_name_234";

			IntegrationPointProfileModel profile = CreateOrUpdateIntegrationPointProfile(
				CreateDefaultIntegrationPointProfileModel(ImportOverwriteModeEnum.AppendOnly, "profile_name", "Append Only"));

			IntegrationPointModel integrationPointModel = _client
				.CreateIntegrationPointFromProfileAsync(SourceWorkspaceArtifactID, profile.ArtifactID, integrationPointName).GetAwaiter().GetResult();

			IntegrationPoint actualIntegrationPoint =
				IntegrationPointRepository.ReadWithFieldMappingAsync(integrationPointModel.ArtifactId).GetAwaiter().GetResult();

			Assert.That(actualIntegrationPoint.Name, Is.EqualTo(integrationPointName));
			Assert.That(actualIntegrationPoint.SourceProvider, Is.EqualTo(profile.SourceProvider));
			Assert.That(actualIntegrationPoint.DestinationProvider, Is.EqualTo(profile.DestinationProvider));
			Assert.That(actualIntegrationPoint.DestinationConfiguration, Is.EqualTo(profile.Destination));
			Assert.That(actualIntegrationPoint.SourceConfiguration, Is.EqualTo(profile.SourceConfiguration));
			Assert.That(actualIntegrationPoint.EmailNotificationRecipients, Is.EqualTo(profile.NotificationEmails));
			Assert.That(actualIntegrationPoint.EnableScheduler, Is.EqualTo(profile.Scheduler.EnableScheduler));
			Assert.That(actualIntegrationPoint.FieldMappings, Is.EqualTo(profile.Map));
			Assert.That(actualIntegrationPoint.Type, Is.EqualTo(profile.Type));
			Assert.That(actualIntegrationPoint.LogErrors, Is.EqualTo(profile.LogErrors));
		}

		[IdentifiedTest("09d2a68e-879e-431a-889e-5059b2f76b82")]
		public async Task ItShouldCreateIntegrationPointWithEncryptedCredentials()
		{
			string username = "username_933";
			string password = "password_729";

			IList<OverwriteFieldsModel> choicesModels = await _client
				.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactID)
				.ConfigureAwait(false);

			OverwriteFieldsModel overwriteFieldsModel = choicesModels
				.First(x => x.Name == "Append/Overlay");

			CreateIntegrationPointRequest createRequest = IntegrationPointBaseHelper.CreateCreateIntegrationPointRequest(
				Helper, 
				RepositoryFactory, 
				SourceWorkspaceArtifactID, 
				SavedSearchArtifactID, 
				TypeOfExport,
				TargetWorkspaceArtifactID, 
				false, 
				true, 
				false, 
				string.Empty, 
				"Use Field Settings", 
				overwriteFieldsModel,
				GetDefaultFieldMap().ToList()
			);

			createRequest.IntegrationPoint.SecuredConfiguration = new Credentials
			{
				Username = username,
				Password = password
			};

			IntegrationPointModel integrationPointModel = await _client
				.CreateIntegrationPointAsync(createRequest)
				.ConfigureAwait(false);

			IntegrationPointModel integrationPoint = await _client
				.GetIntegrationPointAsync(createRequest.WorkspaceArtifactId, integrationPointModel.ArtifactId)
				.ConfigureAwait(false);

			string actualSecret = integrationPoint.SecuredConfiguration as string;
			string expectedSecret = JsonConvert.SerializeObject(createRequest.IntegrationPoint.SecuredConfiguration);
			Assert.That(actualSecret, Is.EqualTo(expectedSecret));
		}

		[IdentifiedTestCase("68b4d5e2-5b3c-4d3d-ab88-5a0e8ab62f92", false, false, false, "a421248620@relativity.com", "Use Field Settings", "Overlay Only")]
		[IdentifiedTestCase("ca67dbfb-8272-4010-b084-d9ba689b28dc", true, true, true, "", "Use Field Settings", "Append Only")]
		[IdentifiedTestCase("0e1847bd-e140-42f8-adb4-ee6d7ed46f8c", false, false, false, null, "Replace Values", "Append/Overlay")]
		[IdentifiedTestCase("596cf427-9fb2-499b-b5fa-d00469c26df6", false, false, false, "a937467@relativity.com", "Merge Values", "Append/Overlay")]
		public async Task ItShouldCreateRelativityIntegrationPoint(
			bool importNativeFile, 
			bool logErrors, 
			bool useFolderPathInformation, 
			string emailNotificationRecipients,
			string fieldOverlayBehavior, 
			string overwriteFieldsChoices)
		{
			IList<OverwriteFieldsModel> overwriteFieldsModels = await _client
				.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactID)
				.ConfigureAwait(false);

			OverwriteFieldsModel overwriteFieldsModel = overwriteFieldsModels
				.First(x => x.Name == overwriteFieldsChoices);

			CreateIntegrationPointRequest createRequest = IntegrationPointBaseHelper.CreateCreateIntegrationPointRequest(
				Helper, 
				RepositoryFactory, 
				SourceWorkspaceArtifactID, 
				SavedSearchArtifactID, 
				TypeOfExport,
				TargetWorkspaceArtifactID, 
				importNativeFile, 
				logErrors, 
				useFolderPathInformation, 
				emailNotificationRecipients, 
				fieldOverlayBehavior, 
				overwriteFieldsModel, 
				GetDefaultFieldMap().ToList()
			);

			IntegrationPointModel createdIntegrationPoint = await _client.CreateIntegrationPointAsync(createRequest)
				.ConfigureAwait(false);

			IntegrationPoint actualIntegrationPoint = await IntegrationPointRepository
				.ReadWithFieldMappingAsync(createdIntegrationPoint.ArtifactId)
				.ConfigureAwait(false);
			IntegrationPointModel expectedIntegrationPointModel = createRequest.IntegrationPoint;

			IntegrationPointBaseHelper.AssertIntegrationPointModelBase(actualIntegrationPoint, 
				expectedIntegrationPointModel,
				new IntegrationPointFieldGuidsConstants());
		}

		[IdentifiedTestCase("1b2ac7a7-9642-41fc-a6c2-b2ea35a50ad5", ImportFileCopyModeEnum.CopyFiles, true, ImportNativeFileCopyModeEnum.CopyFiles)]
		[IdentifiedTestCase("20c17c25-4fa2-45ce-9e4e-91a4830070c4", ImportFileCopyModeEnum.SetFileLinks, true, ImportNativeFileCopyModeEnum.SetFileLinks)]
		[IdentifiedTestCase("1476723a-a87d-4535-a694-5a5de144660c", ImportFileCopyModeEnum.DoNotImportNativeFiles, false, ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)]
		public async Task CreateIntergationPointAsync_ShouldCreateRelativityIntegrationPointWithImportMode(
			ImportFileCopyModeEnum fileCopyMode, bool expectedImporNativeFile, ImportNativeFileCopyModeEnum expectedImportNativeFileCopyMode)
		{
			// Arrange
			IList<OverwriteFieldsModel> overwriteFieldsModels = await _client
				.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactID)
				.ConfigureAwait(false);

			OverwriteFieldsModel overwriteFieldsModel = overwriteFieldsModels
				.First(x => x.Name == "Append Only");

			CreateIntegrationPointRequest createRequest = IntegrationPointBaseHelper.CreateCreateIntegrationPointRequest(
				Helper, RepositoryFactory, SourceWorkspaceArtifactID, SavedSearchArtifactID, TypeOfExport, TargetWorkspaceArtifactID,
				false, true, false, null, "Use Field Settings", overwriteFieldsModel, GetDefaultFieldMap().ToList()
			);

			createRequest.IntegrationPoint.ImportFileCopyMode = fileCopyMode;

			// Act
			IntegrationPointModel createdIntegrationPoint = await _client.CreateIntegrationPointAsync(createRequest)
				.ConfigureAwait(false);

			// Assert
			IntegrationPoint integrationPoint = await IntegrationPointRepository
				.ReadWithFieldMappingAsync(createdIntegrationPoint.ArtifactId)
				.ConfigureAwait(false);

			ImportSettings destinationConfiguration = JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.DestinationConfiguration);

			destinationConfiguration.ImportNativeFile.Should().Be(expectedImporNativeFile);
			destinationConfiguration.ImportNativeFileCopyMode.Should().Be(expectedImportNativeFileCopyMode);
		}

		[IdentifiedTestCase("8a627052-d7e6-466f-9fee-7bc85a738f67")]
		public async Task UpdateIntergationPointAsync_ShouldUpdateImportFileConfigurationValues()
		{
			// Arrange
			IList<OverwriteFieldsModel> overwriteFieldsModels = await _client
				.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactID)
				.ConfigureAwait(false);

			OverwriteFieldsModel overwriteFieldsModel = overwriteFieldsModels
				.First(x => x.Name == "Append Only");

			CreateIntegrationPointRequest createRequest = IntegrationPointBaseHelper.CreateCreateIntegrationPointRequest(
				Helper, RepositoryFactory, SourceWorkspaceArtifactID, SavedSearchArtifactID, TypeOfExport, TargetWorkspaceArtifactID,
				false, true, false, null, "Use Field Settings", overwriteFieldsModel, GetDefaultFieldMap().ToList()
			);

			IntegrationPointModel createdIntegrationPoint = await _client.CreateIntegrationPointAsync(createRequest)
				.ConfigureAwait(false);

			UpdateIntegrationPointRequest updateRequest = UpdateRequestFrom(createRequest);
			updateRequest.IntegrationPoint.ArtifactId = createdIntegrationPoint.ArtifactId;
			updateRequest.IntegrationPoint.ImportFileCopyMode = ImportFileCopyModeEnum.CopyFiles;

			// Act
			IntegrationPointModel updatedIntegrationPoint = await _client.UpdateIntegrationPointAsync(updateRequest).ConfigureAwait(false);

			// Assert
			IntegrationPoint integrationPoint = await IntegrationPointRepository
				.ReadWithFieldMappingAsync(updatedIntegrationPoint.ArtifactId)
				.ConfigureAwait(false);

			ImportSettings destinationConfiguration = JsonConvert.DeserializeObject<ImportSettings>(integrationPoint.DestinationConfiguration);

			destinationConfiguration.ImportNativeFile.Should().Be(true);
			destinationConfiguration.ImportNativeFileCopyMode.Should().Be(ImportNativeFileCopyModeEnum.CopyFiles);
		}

		private UpdateIntegrationPointRequest UpdateRequestFrom(CreateIntegrationPointRequest request)
		{
			return new UpdateIntegrationPointRequest
			{
				IntegrationPoint = request.IntegrationPoint,
				WorkspaceArtifactId = request.WorkspaceArtifactId
			};
		}
	}
}