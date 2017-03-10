using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointProfileManager
{
	[TestFixture]
	public class ItShouldRetrieveAllIntegrationPointProfiles : RelativityProviderTemplate
	{
		public ItShouldRetrieveAllIntegrationPointProfiles() : base($"KeplerService_{Utils.FormatedDateTimeNow}", $"KeplerService_Target_{Utils.FormatedDateTimeNow}")
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

			_expectedIntegrationPointProfiles = new List<IntegrationPointProfileModel> {rel1, rel2, ldap1};

			_expectedIntegrationPointProfiles.ForEach(x => { x.ArtifactID = CreateOrUpdateIntegrationPointProfile(x).ArtifactID; });

			_client = Helper.CreateAdminProxy<IIntegrationPointProfileManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		[Test]
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

		[Test]
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