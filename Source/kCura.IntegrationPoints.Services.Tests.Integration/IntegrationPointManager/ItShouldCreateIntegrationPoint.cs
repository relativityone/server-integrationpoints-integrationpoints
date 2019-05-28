using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Testing.Identification;
using Constants = kCura.IntegrationPoints.Core.Constants;
using APIHelper_SecretStoreFactory = Relativity.APIHelper.SecretStore.SecretStoreFactory;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	public class ItShouldCreateIntegrationPoint : RelativityProviderTemplate
	{
		public ItShouldCreateIntegrationPoint() : base($"create_s_{Utils.FormattedDateTimeNow}", $"create_d_{Utils.FormattedDateTimeNow}")
		{
		}

		private IIntegrationPointManager _client;

		public override void SuiteSetup()
		{
			base.SuiteSetup();

#pragma warning disable 414, CS0618
			// Platform made currently BuildSecretStore method internal. The only option is for now use obsolute method. 
			// When Platofrm team deliver final solution we should replace the code
			ExtensionPointServiceFinder.SecretStoreHelper = APIHelper_SecretStoreFactory.SecretCatalog;
#pragma warning restore

			_client = Helper.CreateAdminProxy<IIntegrationPointManager>();
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
			var overwriteFieldsModel = _client.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactID).Result.First(x => x.Name == "Append/Overlay");

			var sourceConfiguration = new RelativityProviderSourceConfiguration
			{
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactID,
				SavedSearchArtifactId = SavedSearchArtifactID
			};

			var destinationConfiguration = new
			{
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
					IntegrationPointBaseHelper.GetDestinationProviderArtifactId(Constants.IntegrationPoints.DestinationProviders.LOADFILE, SourceWorkspaceArtifactID, Helper),
				SourceProvider = IntegrationPointBaseHelper.GetSourceProviderArtifactId(Constants.IntegrationPoints.SourceProviders.RELATIVITY, SourceWorkspaceArtifactID, Helper),
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
				},
				PromoteEligible = false
			};
			var createRequest = new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactID,
				IntegrationPoint = integrationPointModel
			};

			IntegrationPointModel createdIntegrationPoint = _client.CreateIntegrationPointAsync(createRequest).Result;

			Data.IntegrationPoint actualIntegrationPoint =
				IntegrationPointRepository.ReadAsync(createdIntegrationPoint.ArtifactId).GetAwaiter().GetResult();
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
				CreateDefaultIntegrationPointProfileModel(ImportOverwriteModeEnum.AppendOnly, "profile_name", "Append Only", true));

			IntegrationPointModel integrationPointModel = _client
				.CreateIntegrationPointFromProfileAsync(SourceWorkspaceArtifactID, profile.ArtifactID, integrationPointName).Result;

			Data.IntegrationPoint actualIntegrationPoint =
				IntegrationPointRepository.ReadAsync(integrationPointModel.ArtifactId).GetAwaiter().GetResult();

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
			Assert.That(actualIntegrationPoint.PromoteEligible, Is.EqualTo(profile.PromoteEligible));
		}

		[IdentifiedTest("09d2a68e-879e-431a-889e-5059b2f76b82")]
		public void ItShouldCreateIntegrationPointWithEncryptedCredentials()
		{
			var username = "username_933";
			var password = "password_729";

			OverwriteFieldsModel overwriteFieldsModel = _client.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactID).Result.First(x => x.Name == "Append/Overlay");

			CreateIntegrationPointRequest createRequest = IntegrationPointBaseHelper.CreateCreateIntegrationPointRequest(Helper, RepositoryFactory, SourceWorkspaceArtifactID, SavedSearchArtifactID, TypeOfExport,
				TargetWorkspaceArtifactID, false, true, false, string.Empty, "Use Field Settings", overwriteFieldsModel,
				GetDefaultFieldMap().ToList(), false);

			createRequest.IntegrationPoint.SecuredConfiguration = new Credentials
			{
				Username = username,
				Password = password
			};

			IntegrationPointModel integrationPointModel = _client.CreateIntegrationPointAsync(createRequest).Result;

			IntegrationPointModel integrationPoint = _client.GetIntegrationPointAsync(createRequest.WorkspaceArtifactId, integrationPointModel.ArtifactId).Result;

			string actualSecret = integrationPoint.SecuredConfiguration as string;
			string expectedSecret = JsonConvert.SerializeObject(createRequest.IntegrationPoint.SecuredConfiguration);
			Assert.That(actualSecret, Is.EqualTo(expectedSecret));
		}

		[IdentifiedTestCase("68b4d5e2-5b3c-4d3d-ab88-5a0e8ab62f92", false, false, false, "a421248620@relativity.com", "Use Field Settings", "Overlay Only", true)]
		[IdentifiedTestCase("ca67dbfb-8272-4010-b084-d9ba689b28dc", true, true, true, "", "Use Field Settings", "Append Only", false)]
		[IdentifiedTestCase("0e1847bd-e140-42f8-adb4-ee6d7ed46f8c", false, false, false, null, "Replace Values", "Append/Overlay", false)]
		[IdentifiedTestCase("596cf427-9fb2-499b-b5fa-d00469c26df6", false, false, false, "a937467@relativity.com", "Merge Values", "Append/Overlay", false)]
		public void ItShouldCreateRelativityIntegrationPoint(bool importNativeFile, bool logErrors, bool useFolderPathInformation, string emailNotificationRecipients,
			string fieldOverlayBehavior, string overwriteFieldsChoices, bool promoteEligible)
		{
			OverwriteFieldsModel overwriteFieldsModel = _client.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactID).Result.First(x => x.Name == overwriteFieldsChoices);

			CreateIntegrationPointRequest createRequest = IntegrationPointBaseHelper.CreateCreateIntegrationPointRequest(Helper, RepositoryFactory, SourceWorkspaceArtifactID, SavedSearchArtifactID, TypeOfExport,
				TargetWorkspaceArtifactID, importNativeFile, logErrors, useFolderPathInformation, emailNotificationRecipients, fieldOverlayBehavior, overwriteFieldsModel, 
				GetDefaultFieldMap().ToList(), promoteEligible);

			IntegrationPointModel createdIntegrationPoint = _client.CreateIntegrationPointAsync(createRequest).Result;

			Data.IntegrationPoint actualIntegrationPoint =
				IntegrationPointRepository.ReadAsync(createdIntegrationPoint.ArtifactId).GetAwaiter().GetResult();
			IntegrationPointModel expectedIntegrationPointModel = createRequest.IntegrationPoint;

			IntegrationPointBaseHelper.AssertIntegrationPointModelBase(actualIntegrationPoint, 
				expectedIntegrationPointModel,
				new IntegrationPointFieldGuidsConstants());
		}
	}
}