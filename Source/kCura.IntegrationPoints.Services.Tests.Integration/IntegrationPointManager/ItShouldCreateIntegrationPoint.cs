﻿using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity;
using Relativity.Core;
using Relativity.SecretCatalog;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	public class ItShouldCreateIntegrationPoint : RelativityProviderTemplate
	{
		public ItShouldCreateIntegrationPoint() : base($"create_s_{Utils.FormatedDateTimeNow}", $"create_d_{Utils.FormatedDateTimeNow}")
		{
		}

		private IIntegrationPointManager _client;
		private ISecretCatalog _secretCatalog;
		private ISecretManager _secretManager;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_secretCatalog = SecretStoreFactory.GetSecretStore(BaseServiceContextHelper.Create().GetMasterRdgContext());
			_secretManager = new SecretManager(WorkspaceArtifactId);
			_client = Helper.CreateAdminProxy<IIntegrationPointManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		private string ReadSecret(string secretId)
		{
			var secret = _secretManager.RetrieveIdentifier(secretId);
			return _secretManager.RetrieveValue(_secretCatalog.GetSecret(secret));
		}

		private class Credentials
		{
			public string Username { get; set; }
			public string Password { get; set; }
		}

		[Test]
		public void ItShouldCreateExportToLoadFileIntegrationPoint()
		{
			var overwriteFieldsModel = _client.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactId).Result.First(x => x.Name == "Append/Overlay");

			var sourceConfiguration = new LoadFileExportSourceConfiguration
			{
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactId,
				SavedSearchArtifactId = SavedSearchArtifactId
			};

			var destinationConfiguration = new LoadFileExportDestinationConfiguration
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
				IncludeNativeFilesPath = false
			};
			var integrationPointModel = new IntegrationPointModel
			{
				Name = "export_lf_ip",
				DestinationProvider =
					IntegrationPointBaseHelper.GetDestinationProviderArtifactId(Constants.IntegrationPoints.DestinationProviders.LOADFILE, SourceWorkspaceArtifactId, Helper),
				SourceProvider = IntegrationPointBaseHelper.GetSourceProviderArtifactId(Constants.IntegrationPoints.SourceProviders.RELATIVITY, SourceWorkspaceArtifactId, Helper),
				Type = IntegrationPointBaseHelper.GetTypeArtifactId(Helper, SourceWorkspaceArtifactId, "Export"),
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
				WorkspaceArtifactId = SourceWorkspaceArtifactId,
				IntegrationPoint = integrationPointModel
			};

			var createdIntegrationPoint = _client.CreateIntegrationPointAsync(createRequest).Result;

			var actualIntegrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(createdIntegrationPoint.ArtifactId);
			var expectedIntegrationPointModel = createRequest.IntegrationPoint;

			Assert.That(expectedIntegrationPointModel.Name, Is.EqualTo(actualIntegrationPoint.Name));
			Assert.That(expectedIntegrationPointModel.DestinationProvider, Is.EqualTo(actualIntegrationPoint.DestinationProvider));
			Assert.That(expectedIntegrationPointModel.SourceProvider, Is.EqualTo(actualIntegrationPoint.SourceProvider));
			Assert.That(expectedIntegrationPointModel.Type, Is.EqualTo(actualIntegrationPoint.Type));
			Assert.That(expectedIntegrationPointModel.LogErrors, Is.EqualTo(actualIntegrationPoint.LogErrors));
		}

		[Test]
		public void ItShouldCreateIntegrationPointBasedOnProfile()
		{
			string integrationPointName = "ip_name_234";

			var profile = CreateOrUpdateIntegrationPointProfile(CreateDefaultIntegrationPointProfileModel(ImportOverwriteModeEnum.AppendOnly, "profile_name", "Append Only"));

			var integrationPointModel = _client.CreateIntegrationPointFromProfileAsync(SourceWorkspaceArtifactId, profile.ArtifactID, integrationPointName).Result;

			var actualIntegrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel.ArtifactId);

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

		[Test]
		public void ItShouldCreateIntegrationPointWithEncryptedCredentials()
		{
			string username = "username_933";
			string password = "password_729";

			var overwriteFieldsModel = _client.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactId).Result.First(x => x.Name == "Append/Overlay");

			var createRequest = IntegrationPointBaseHelper.CreateCreateIntegrationPointRequest(Helper, RepositoryFactory, SourceWorkspaceArtifactId, SavedSearchArtifactId,
				TargetWorkspaceArtifactId, false, true, false, string.Empty, "Use Field Settings", overwriteFieldsModel,
				GetDefaultFieldMap().ToList());

			createRequest.IntegrationPoint.SecuredConfiguration = new Credentials
			{
				Username = username,
				Password = password
			};

			var integrationPointModel = _client.CreateIntegrationPointAsync(createRequest).Result;

			var actualSecretId =
				Helper.GetDBContext(SourceWorkspaceArtifactId)
					.ExecuteSqlStatementAsScalar<string>($"SELECT SecuredConfiguration FROM [IntegrationPoint] WHERE ArtifactID = {integrationPointModel.ArtifactId}");

			var integrationPoint = new RsapiClientLibrary<Data.IntegrationPoint>(Helper, SourceWorkspaceArtifactId).Read(integrationPointModel.ArtifactId);

			var actualSecret = ReadSecret(integrationPoint.SecuredConfiguration);
			var expectedSecret = JsonConvert.SerializeObject(createRequest.IntegrationPoint.SecuredConfiguration);
			Assert.That(actualSecretId, Is.EqualTo(integrationPoint.SecuredConfiguration));
			Assert.That(actualSecret, Is.EqualTo(expectedSecret));
		}

		[Test]
		[TestCase(false, false, false, "a421248620@kcura.com", "Use Field Settings", "Overlay Only")]
		[TestCase(true, true, true, "", "Use Field Settings", "Append Only")]
		[TestCase(false, false, false, null, "Replace Values", "Append/Overlay")]
		[TestCase(false, false, false, "a937467@kcura.com", "Merge Values", "Append/Overlay")]
		public void ItShouldCreateRelativityIntegrationPoint(bool importNativeFile, bool logErrors, bool useFolderPathInformation, string emailNotificationRecipients,
			string fieldOverlayBehavior, string overwriteFieldsChoices)
		{
			var overwriteFieldsModel = _client.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactId).Result.First(x => x.Name == overwriteFieldsChoices);

			var createRequest = IntegrationPointBaseHelper.CreateCreateIntegrationPointRequest(Helper, RepositoryFactory, SourceWorkspaceArtifactId, SavedSearchArtifactId,
				TargetWorkspaceArtifactId,
				importNativeFile, logErrors, useFolderPathInformation, emailNotificationRecipients, fieldOverlayBehavior, overwriteFieldsModel, GetDefaultFieldMap().ToList());

			var createdIntegrationPoint = _client.CreateIntegrationPointAsync(createRequest).Result;

			var actualIntegrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(createdIntegrationPoint.ArtifactId);
			var expectedIntegrationPointModel = createRequest.IntegrationPoint;

			IntegrationPointBaseHelper.AssertIntegrationPointModelBase(actualIntegrationPoint, expectedIntegrationPointModel, new IntegrationPointFieldGuidsConstants());
		}
	}
}