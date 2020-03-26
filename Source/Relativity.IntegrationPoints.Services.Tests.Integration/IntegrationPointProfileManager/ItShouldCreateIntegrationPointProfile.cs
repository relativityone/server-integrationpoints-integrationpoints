using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.Tests.Integration.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.IntegrationPointProfileManager
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ItShouldCreateIntegrationPointProfile : RelativityProviderTemplate
	{
		public ItShouldCreateIntegrationPointProfile() : base($"create_s_{Utils.FormattedDateTimeNow}", $"create_d_{Utils.FormattedDateTimeNow}")
		{
		}

		private IIntegrationPointProfileManager _client;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_client = Helper.CreateProxy<IIntegrationPointProfileManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		[IdentifiedTestCase("610db4ea-7c8f-4eda-9abf-d6c1242eef48", false, false, false, "a421248620@relativity.com", "Use Field Settings", "Overlay Only")]
		[IdentifiedTestCase("7b63f04a-d8dd-413b-ba6e-ef45bf696977", true, true, true, "", "Use Field Settings", "Append Only")]
		[IdentifiedTestCase("c844e131-e72f-45cb-b4aa-fa0f5688cd13", false, false, false, null, "Replace Values", "Append/Overlay")]
		[IdentifiedTestCase("4fac46a5-2f24-48e2-b2e7-a33356077cf9", false, false, false, "a937467@relativity.com", "Merge Values", "Append/Overlay")]
		public void ItShouldCreateRelativityIntegrationPoint(bool importNativeFile, bool logErrors, bool useFolderPathInformation, string emailNotificationRecipients,
			string fieldOverlayBehavior, string overwriteFieldsChoices)
		{
			OverwriteFieldsModel overwriteFieldsModel = _client.GetOverwriteFieldsChoicesAsync(SourceWorkspaceArtifactID).Result.First(x => x.Name == overwriteFieldsChoices);

			CreateIntegrationPointRequest createRequest = IntegrationPointBaseHelper.CreateCreateIntegrationPointRequest(Helper, RepositoryFactory, SourceWorkspaceArtifactID, SavedSearchArtifactID, TypeOfExport,
				TargetWorkspaceArtifactID, importNativeFile, logErrors, useFolderPathInformation, emailNotificationRecipients, fieldOverlayBehavior, overwriteFieldsModel, GetDefaultFieldMap().ToList());

			IntegrationPointModel createdIntegrationPointProfile = _client.CreateIntegrationPointProfileAsync(createRequest).Result;

			IntegrationPointProfile actualIntegrationPointProfile = CaseContext.RsapiService.RelativityObjectManager.Read<IntegrationPointProfile>(createdIntegrationPointProfile.ArtifactId);
			IntegrationPointModel expectedIntegrationPointModel = createRequest.IntegrationPoint;

			IntegrationPointBaseHelper.AssertIntegrationPointModelBase(actualIntegrationPointProfile, expectedIntegrationPointModel,
				new IntegrationPointProfileFieldGuidsConstants());
		}

		[IdentifiedTest("9f7f07d9-9a22-4aed-b3db-52222915926f")]
		public void ItShouldCreateProfileBasedOnIntegrationPoint()
		{
			string profileName = "profile_name_507";

			var integrationPoint = CreateOrUpdateIntegrationPoint(CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "ip_name", "Append Only"));

			var integrationPointProfileModel =
				_client.CreateIntegrationPointProfileFromIntegrationPointAsync(SourceWorkspaceArtifactID, integrationPoint.ArtifactID, profileName).Result;

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