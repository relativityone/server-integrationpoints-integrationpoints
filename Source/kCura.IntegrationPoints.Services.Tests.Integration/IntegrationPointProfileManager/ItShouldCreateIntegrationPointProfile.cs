using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointProfileManager
{
	[TestFixture]
	public class ItShouldCreateIntegrationPointProfile : RelativityProviderTemplate
	{
		public ItShouldCreateIntegrationPointProfile() : base($"create_s_{Utils.FormatedDateTimeNow}", $"create_d_{Utils.FormatedDateTimeNow}")
		{
		}

		private IIntegrationPointProfileManager _client;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_client = Helper.CreateAdminProxy<IIntegrationPointProfileManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		[Test]
		[TestCase(false, false, false, "a421248620@relativity.com", "Use Field Settings", "Overlay Only", true)]
		[TestCase(true, true, true, "", "Use Field Settings", "Append Only", false)]
		[TestCase(false, false, false, null, "Replace Values", "Append/Overlay", false)]
		[TestCase(false, false, false, "a937467@relativity.com", "Merge Values", "Append/Overlay", false)]
		public void ItShouldCreateRelativityIntegrationPoint(bool importNativeFile, bool logErrors, bool useFolderPathInformation, string emailNotificationRecipients,
			string fieldOverlayBehavior, string overwriteFieldsChoices, bool promoteEligible)
		{
			OverwriteFieldsModel overwriteFieldsModel = _client.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactId).Result.First(x => x.Name == overwriteFieldsChoices);

			CreateIntegrationPointRequest createRequest = IntegrationPointBaseHelper.CreateCreateIntegrationPointRequest(Helper, RepositoryFactory, SourceWorkspaceArtifactId, SavedSearchArtifactId, TypeOfExport,
				TargetWorkspaceArtifactId, importNativeFile, logErrors, useFolderPathInformation, emailNotificationRecipients, fieldOverlayBehavior, overwriteFieldsModel, GetDefaultFieldMap().ToList(),
				promoteEligible);

			IntegrationPointModel createdIntegrationPointProfile = _client.CreateIntegrationPointProfileAsync(createRequest).Result;

			IntegrationPointProfile actualIntegrationPointProfile = CaseContext.RsapiService.RelativityObjectManager.Read<IntegrationPointProfile>(createdIntegrationPointProfile.ArtifactId);
			IntegrationPointModel expectedIntegrationPointModel = createRequest.IntegrationPoint;

			IntegrationPointBaseHelper.AssertIntegrationPointModelBase(actualIntegrationPointProfile, expectedIntegrationPointModel,
				new IntegrationPointProfileFieldGuidsConstants());
		}

		[Test]
		public void ItShouldCreateProfileBasedOnIntegrationPoint()
		{
			string profileName = "profile_name_507";

			var integrationPoint = CreateOrUpdateIntegrationPoint(CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "ip_name", "Append Only"));

			var integrationPointProfileModel =
				_client.CreateIntegrationPointProfileFromIntegrationPointAsync(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, profileName).Result;

			var actualIntegrationPointProfile = CaseContext.RsapiService.RelativityObjectManager.Read<IntegrationPointProfile>(integrationPointProfileModel.ArtifactId);

			Assert.That(actualIntegrationPointProfile.Name, Is.EqualTo(profileName));
			Assert.That(actualIntegrationPointProfile.SourceProvider, Is.EqualTo(integrationPoint.SourceProvider));
			Assert.That(actualIntegrationPointProfile.DestinationProvider, Is.EqualTo(integrationPoint.DestinationProvider));
			Assert.That(actualIntegrationPointProfile.DestinationConfiguration, Is.EqualTo(integrationPoint.Destination));
			Assert.That(actualIntegrationPointProfile.SourceConfiguration, Is.EqualTo(integrationPoint.SourceConfiguration));
			Assert.That(actualIntegrationPointProfile.EmailNotificationRecipients, Is.EqualTo(integrationPoint.NotificationEmails));
			Assert.That(actualIntegrationPointProfile.EnableScheduler, Is.EqualTo(integrationPoint.Scheduler.EnableScheduler));
			Assert.That(actualIntegrationPointProfile.FieldMappings, Is.EqualTo(integrationPoint.Map));
			Assert.That(actualIntegrationPointProfile.Type, Is.EqualTo(integrationPoint.Type));
			Assert.That(actualIntegrationPointProfile.LogErrors, Is.EqualTo(integrationPoint.LogErrors));
		}
	}
}