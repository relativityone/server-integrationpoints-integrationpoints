using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.IntegrationPointProfileManager
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ItShouldRetrieveAllIntegrationPointProfiles : RelativityProviderTemplate
	{
		public ItShouldRetrieveAllIntegrationPointProfiles() : base($"KeplerService_{Utils.FormattedDateTimeNow}", $"KeplerService_Target_{Utils.FormattedDateTimeNow}")
		{
		}

		private IIntegrationPointProfileManager _client;
		private IList<IntegrationPointProfileModel> _expectedIntegrationPointProfiles;

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			var rel1 = CreateDefaultIntegrationPointProfileModel(ImportOverwriteModeEnum.AppendOnly, "Relativity_Provider_1", "Append Only", true);
			var rel2 = CreateDefaultIntegrationPointProfileModel(ImportOverwriteModeEnum.AppendOnly, "Relativity_Provider_2", "Append Only", false);
			var ldap1 = CreateDefaultIntegrationPointProfileModel(ImportOverwriteModeEnum.AppendOnly, "LDAP_Provider_1", "Append Only", true);
			ldap1.SourceProvider = LdapProvider.ArtifactId;
			ldap1.Type = Container.Resolve<IIntegrationPointTypeService>()
				.GetIntegrationPointType(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid)
				.ArtifactId;

			_expectedIntegrationPointProfiles = new List<IntegrationPointProfileModel> {rel1, rel2, ldap1};

			foreach (IntegrationPointProfileModel integrationPointProfile in _expectedIntegrationPointProfiles)
			{
				IntegrationPointProfileModel createdIntegrationPointProfile = CreateOrUpdateIntegrationPointProfile(integrationPointProfile);
				integrationPointProfile.ArtifactID = createdIntegrationPointProfile.ArtifactID;
			}
			
			_client = Helper.CreateProxy<IIntegrationPointProfileManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		[IdentifiedTest("1167ee79-3661-4ea0-9b64-b0d4454240dc")]
		public void ItShouldRetrieveAllIntegrationPointProfilesAtOnce()
		{
			var integrationPointProfileModels = _client.GetAllIntegrationPointProfilesAsync(WorkspaceArtifactId).Result;
			Assert.That(integrationPointProfileModels.Count, Is.EqualTo(_expectedIntegrationPointProfiles.Count));

			foreach (var integrationPointProfileModel in integrationPointProfileModels)
			{
				Assert.That(
					_expectedIntegrationPointProfiles.Any(x => (x.Name == integrationPointProfileModel.Name) && (x.SourceProvider == integrationPointProfileModel.SourceProvider)));
			}
		}

		[IdentifiedTest("9dcec291-1b77-4b36-930b-bf419e1d47c5")]
		public void ItShouldRetrieveIntegrationPointProfilesOneByOne()
		{
			foreach (var expectedIntegrationPointProfile in _expectedIntegrationPointProfiles)
			{
				var integrationPointProfile = _client.GetIntegrationPointProfileAsync(WorkspaceArtifactId, expectedIntegrationPointProfile.ArtifactID).Result;
				Assert.That(integrationPointProfile.Name, Is.EqualTo(expectedIntegrationPointProfile.Name));
				Assert.That(integrationPointProfile.SourceProvider, Is.EqualTo(expectedIntegrationPointProfile.SourceProvider));
			}
		}
	}
}