using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ItShouldRetrieveAllIntegrationPoints : RelativityProviderTemplate
	{
		public ItShouldRetrieveAllIntegrationPoints() : base($"KeplerService_{Utils.FormattedDateTimeNow}", $"KeplerService_Target_{Utils.FormattedDateTimeNow}")
		{
		}

		private IIntegrationPointManager _client;
		private IList<kCura.IntegrationPoints.Core.Models.IntegrationPointModel> _expectedIntegrationPoints;

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			var rel1 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "Relativity_Provider_1", "Append Only");
			var rel2 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "Relativity_Provider_2", "Append Only");
			var ldap1 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "LDAP_Provider_1", "Append Only");
			ldap1.SourceProvider = LdapProvider.ArtifactId;
			ldap1.Type = Container.Resolve<IIntegrationPointTypeService>()
				.GetIntegrationPointType(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid)
				.ArtifactId;

			_expectedIntegrationPoints = new List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel> { rel1, rel2, ldap1 };

			foreach (kCura.IntegrationPoints.Core.Models.IntegrationPointModel integrationPoint in _expectedIntegrationPoints)
			{
				integrationPoint.ArtifactID = CreateOrUpdateIntegrationPoint(integrationPoint).ArtifactID;
			}

			_client = Helper.CreateProxy<IIntegrationPointManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		[IdentifiedTest("ba528b02-8658-4085-8aef-4cb47d0f81be")]
		public void ItShouldRetrieveAllIntegrationPointsAtOnce()
		{
			var integrationPointModels = _client.GetAllIntegrationPointsAsync(WorkspaceArtifactId).Result;
			Assert.That(integrationPointModels.Count, Is.EqualTo(_expectedIntegrationPoints.Count));

			foreach (var integrationPointModel in integrationPointModels)
			{
				Assert.That(_expectedIntegrationPoints.Any(x => x.Name == integrationPointModel.Name && x.SourceProvider == integrationPointModel.SourceProvider));
			}
		}

		[IdentifiedTest("50657dd7-a487-499d-8a47-033ab9dc3171")]
		public void ItShouldRetrieveIntegrationPointsOneByOne()
		{
			foreach (var expectedIntegrationPoint in _expectedIntegrationPoints)
			{
				var integrationPoint = _client.GetIntegrationPointAsync(WorkspaceArtifactId, expectedIntegrationPoint.ArtifactID).Result;
				Assert.That(integrationPoint.Name, Is.EqualTo(expectedIntegrationPoint.Name));
				Assert.That(integrationPoint.SourceProvider, Is.EqualTo(expectedIntegrationPoint.SourceProvider));
			}
		}
	}
}