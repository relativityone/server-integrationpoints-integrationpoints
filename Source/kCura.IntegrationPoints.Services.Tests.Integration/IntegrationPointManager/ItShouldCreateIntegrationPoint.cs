using System;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity;
using Relativity.Services.Folder;
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

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_client = Helper.CreateAdminProxy<IIntegrationPointManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
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

			var folderPathSourceField = 0;
			if (useFolderPathInformation)
			{
				var artifactFieldDtos = RepositoryFactory.GetFieldRepository(SourceWorkspaceArtifactId).RetrieveLongTextFieldsAsync((int) ArtifactType.Document).Result;
				folderPathSourceField = artifactFieldDtos[0].ArtifactId;
			}

			var expectedDestinationConfiguration = new RelativityProviderDestinationConfiguration
			{
				ArtifactTypeID = (int) ArtifactType.Document,
				CaseArtifactId = TargetWorkspaceArtifactId,
				ImportNativeFile = importNativeFile,
				UseFolderPathInformation = useFolderPathInformation,
				FolderPathSourceField = folderPathSourceField,
				FieldOverlayBehavior = fieldOverlayBehavior,
				DestinationFolderArtifactId = GetRootFolder()
			};
			var expectedSourceConfiguration = new RelativityProviderSourceConfiguration
			{
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactId,
				SavedSearchArtifactId = SavedSearchArtifactId
			};
			var expectedIntegrationPointModel = new IntegrationPointModel
			{
				ArtifactId = 0,
				EmailNotificationRecipients = emailNotificationRecipients,
				LogErrors = logErrors,
				Name = $"relativity_{Utils.FormatedDateTimeNow}",
				SourceProvider = GetSourceProviderArtifactId(Constants.IntegrationPoints.SourceProviders.RELATIVITY),
				DestinationProvider = GetDestinationProviderArtifactId(Constants.IntegrationPoints.DestinationProviders.RELATIVITY),
				DestinationConfiguration = expectedDestinationConfiguration,
				SourceConfiguration = expectedSourceConfiguration,
				FieldMappings = GetDefaultFieldMap().ToList(),
				Type = GetTypeArtifactId(Constants.IntegrationPoints.IntegrationPointTypes.ExportName),
				OverwriteFieldsChoiceId = overwriteFieldsModel.ArtifactId,
				ScheduleRule = new ScheduleModel
				{
					EnableScheduler = false
				}
			};

			var createRequest = new CreateIntegrationPointRequest
			{
				WorkspaceArtifactId = SourceWorkspaceArtifactId,
				IntegrationPoint = expectedIntegrationPointModel
			};

			var createdIntegrationPoint = _client.CreateIntegrationPointAsync(createRequest).Result;

			var actualIntegrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(createdIntegrationPoint.ArtifactId);

			Assert.That(actualIntegrationPoint.SourceProvider, Is.EqualTo(expectedIntegrationPointModel.SourceProvider));
			Assert.That(actualIntegrationPoint.DestinationProvider, Is.EqualTo(expectedIntegrationPointModel.DestinationProvider));
			Assert.That(actualIntegrationPoint.EmailNotificationRecipients, Is.EqualTo(expectedIntegrationPointModel.EmailNotificationRecipients ?? string.Empty));
			Assert.That(actualIntegrationPoint.EnableScheduler, Is.EqualTo(expectedIntegrationPointModel.ScheduleRule.EnableScheduler));
			Assert.That(actualIntegrationPoint.LogErrors, Is.EqualTo(expectedIntegrationPointModel.LogErrors));
			Assert.That(actualIntegrationPoint.Name, Is.EqualTo(expectedIntegrationPointModel.Name));
			Assert.That(actualIntegrationPoint.Type, Is.EqualTo(expectedIntegrationPointModel.Type));
			Assert.That(actualIntegrationPoint.OverwriteFields.ArtifactID, Is.EqualTo(expectedIntegrationPointModel.OverwriteFieldsChoiceId));

			var actualSourceConfiguration = JsonConvert.DeserializeObject<RelativityProviderSourceConfiguration>(actualIntegrationPoint.SourceConfiguration);
			Assert.That(actualSourceConfiguration.SourceWorkspaceArtifactId, Is.EqualTo(expectedSourceConfiguration.SourceWorkspaceArtifactId));
			Assert.That(actualSourceConfiguration.SavedSearchArtifactId, Is.EqualTo(expectedSourceConfiguration.SavedSearchArtifactId));

			var actualDestinationConfiguration = JsonConvert.DeserializeObject<RelativityProviderDestinationConfiguration>(actualIntegrationPoint.DestinationConfiguration);
			Assert.That(actualDestinationConfiguration.ArtifactTypeID, Is.EqualTo(expectedDestinationConfiguration.ArtifactTypeID));
			Assert.That(actualDestinationConfiguration.CaseArtifactId, Is.EqualTo(expectedDestinationConfiguration.CaseArtifactId));
			Assert.That(actualDestinationConfiguration.DestinationFolderArtifactId, Is.EqualTo(expectedDestinationConfiguration.DestinationFolderArtifactId));
			Assert.That(actualDestinationConfiguration.FieldOverlayBehavior, Is.EqualTo(expectedDestinationConfiguration.FieldOverlayBehavior));
			Assert.That(actualDestinationConfiguration.FolderPathSourceField, Is.EqualTo(expectedDestinationConfiguration.FolderPathSourceField));
			Assert.That(actualDestinationConfiguration.ImportNativeFile, Is.EqualTo(expectedDestinationConfiguration.ImportNativeFile));
			Assert.That(actualDestinationConfiguration.UseFolderPathInformation, Is.EqualTo(expectedDestinationConfiguration.UseFolderPathInformation));

			Assert.That(actualIntegrationPoint.FieldMappings, Is.EqualTo(JsonConvert.SerializeObject(expectedIntegrationPointModel.FieldMappings)));
		}

		private int GetRootFolder()
		{
			using (var folderManager = Helper.CreateAdminProxy<IFolderManager>())
			{
				return folderManager.GetWorkspaceRootAsync(SourceWorkspaceArtifactId).Result.ArtifactID;
			}
		}

		private int GetTypeArtifactId(string typeName)
		{
			using (var typeClient = Helper.CreateAdminProxy<IIntegrationPointTypeManager>())
			{
				return typeClient.GetIntegrationPointTypes(SourceWorkspaceArtifactId).Result.First(x => x.Name == typeName).ArtifactId;
			}
		}

		private int GetSourceProviderArtifactId(string guid)
		{
			using (var providerClient = Helper.CreateAdminProxy<IProviderManager>())
			{
				return providerClient.GetSourceProviderArtifactIdAsync(SourceWorkspaceArtifactId, guid).Result;
			}
		}

		private int GetDestinationProviderArtifactId(string guid)
		{
			using (var providerClient = Helper.CreateAdminProxy<IProviderManager>())
			{
				return providerClient.GetDestinationProviderArtifactIdAsync(SourceWorkspaceArtifactId, guid).Result;
			}
		}
	}
}