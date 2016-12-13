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
		public void ItShouldCreateRelativityIntegrationPoint()
		{
			var overwriteFieldsModel = _client.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactId).Result.First(x => x.Name == "Append Only");

			var expectedDestinationConfiguration = new RelativityProviderDestinationConfiguration
			{
				ArtifactTypeID = (int) ArtifactType.Document,
				CaseArtifactId = TargetWorkspaceArtifactId,
				ImportNativeFile = false,
				UseFolderPathInformation = false,
				FolderPathSourceField = 0,
				FieldOverlayBehavior = "Use Field Settings",
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
				EmailNotificationRecipients = "a421248620@kcura.com",
				LogErrors = true,
				Name = "integrationpoint_565",
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
			Assert.That(actualIntegrationPoint.EmailNotificationRecipients, Is.EqualTo(expectedIntegrationPointModel.EmailNotificationRecipients));
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