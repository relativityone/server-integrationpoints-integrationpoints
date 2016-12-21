using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
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

			var createdIntegrationPointProfile = _client.CreateIntegrationPointProfileAsync(createRequest).Result;

			var actualIntegrationPointProfile = CaseContext.RsapiService.IntegrationPointProfileLibrary.Read(createdIntegrationPointProfile.ArtifactId);
			var expectedIntegrationPointModel = createRequest.IntegrationPoint;

			IntegrationPointBaseHelper.AssertIntegrationPointModelBase(actualIntegrationPointProfile, expectedIntegrationPointModel, new IntegrationPointProfileFieldGuidsConstants());
		}
	}
}