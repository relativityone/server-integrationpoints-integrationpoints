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
	public class ItShouldRetrieveEligibleToPromoteIntegrationPoints : RelativityProviderTemplate
	{
		public ItShouldRetrieveEligibleToPromoteIntegrationPoints()
			: base($"KeplerService_{Utils.FormattedDateTimeNow}", $"KeplerService_Target_{Utils.FormattedDateTimeNow}")
		{
		}

		private IIntegrationPointManager _client;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			
			_client = Helper.CreateProxy<IIntegrationPointManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		[IdentifiedTest("d4bcc751-b7ab-439f-b2c9-c9247cc0ed16")]
		public void ItShouldRetrieveEligibleToPromoteIntegrationPointsAtOnce()
		{
			// Arrange
			IList<kCura.IntegrationPoints.Core.Models.IntegrationPointModel> expectedIntegrationPoints = CreateEligibleToPromoteIntegrationPoints();
			List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel> allIntegrationPoints = new List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel>();
			allIntegrationPoints.AddRange(expectedIntegrationPoints);
			allIntegrationPoints.AddRange(CreateNotEligibleToPromoteIntegrationPoints());

			allIntegrationPoints.ForEach(x =>
			{
				x.ArtifactID = CreateOrUpdateIntegrationPoint(x).ArtifactID;
			});


			// Act
			var integrationPointModels = _client.GetEligibleToPromoteIntegrationPointsAsync(WorkspaceArtifactId).Result;

			// Assert
			Assert.That(integrationPointModels.Count, Is.EqualTo(expectedIntegrationPoints.Count));

			foreach (var integrationPointModel in integrationPointModels)
			{
				Assert.That(
					expectedIntegrationPoints.Any(
						x => x.Name == integrationPointModel.Name && x.SourceProvider == integrationPointModel.SourceProvider));
			}
		}

		[IdentifiedTest("c03e4c71-3544-47ca-8df7-9d0e9c3a57aa")]
		public void ItShouldNotRetrieveEligibleToPromoteIntegrationPoints()
		{
			// Arrange
			List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel> allIntegrationPoints = new List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel>();
			allIntegrationPoints.AddRange(CreateNotEligibleToPromoteIntegrationPoints());

			allIntegrationPoints.ForEach(x =>
			{
				x.ArtifactID = CreateOrUpdateIntegrationPoint(x).ArtifactID;
			});


			// Act
			var integrationPointModels = _client.GetEligibleToPromoteIntegrationPointsAsync(WorkspaceArtifactId).Result;

			// Assert
			Assert.That(integrationPointModels.Count, Is.EqualTo(0));
		}

		private IList<kCura.IntegrationPoints.Core.Models.IntegrationPointModel> CreateEligibleToPromoteIntegrationPoints()
		{
			var eligibleToPromote1 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly,
				"Relativity_Provider_1", "Append Only");
			var eligibleToPromote2 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly,
				"Relativity_Provider_2", "Append Only");

			return new List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel>() { eligibleToPromote1 , eligibleToPromote2};


		}

		private IList<kCura.IntegrationPoints.Core.Models.IntegrationPointModel> CreateNotEligibleToPromoteIntegrationPoints()
		{
			var notEligibleToPromote1 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly,
				"Relativity_Provider_3", "Append Only");
			var notEligibleToPromoteLdap = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly,
				"Relativity_Provider_4", "Append Only");
			notEligibleToPromoteLdap.SourceProvider = LdapProvider.ArtifactId;
			notEligibleToPromoteLdap.Type = Container.Resolve<IIntegrationPointTypeService>()
				.GetIntegrationPointType(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid)
				.ArtifactId;

			return new List<kCura.IntegrationPoints.Core.Models.IntegrationPointModel>() {notEligibleToPromote1, notEligibleToPromoteLdap};
		}
	}
}
