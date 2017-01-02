using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using NUnit.Framework;

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

			var createRequest = IntegrationPointBaseHelper.CreateCreateIntegrationPointRequest(Helper, RepositoryFactory, SourceWorkspaceArtifactId, SavedSearchArtifactId,
				TargetWorkspaceArtifactId,
				importNativeFile, logErrors, useFolderPathInformation, emailNotificationRecipients, fieldOverlayBehavior, overwriteFieldsModel, GetDefaultFieldMap().ToList());

			var createdIntegrationPointProfile = _client.CreateIntegrationPointAsync(createRequest).Result;

			var actualIntegrationPointProfile = CaseContext.RsapiService.IntegrationPointLibrary.Read(createdIntegrationPointProfile.ArtifactId);
			var expectedIntegrationPointModel = createRequest.IntegrationPoint;

			IntegrationPointBaseHelper.AssertIntegrationPointModelBase(actualIntegrationPointProfile, expectedIntegrationPointModel, new IntegrationPointFieldGuidsConstants());
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

			var actualCredentials =
				Helper.GetDBContext(SourceWorkspaceArtifactId)
					.ExecuteSqlStatementAsScalar<string>($"SELECT SecuredConfiguration FROM [IntegrationPoint] WHERE ArtifactID = {integrationPointModel.ArtifactId}");

			var expectedCredentails = new DefaultEncryptionManager().Encrypt(JsonConvert.SerializeObject(createRequest.IntegrationPoint.SecuredConfiguration));

			Assert.That(actualCredentials, Is.EqualTo(expectedCredentails));
		}

		private class Credentials
		{
			public string Username { get; set; }
			public string Password { get; set; }
		}
	}
}