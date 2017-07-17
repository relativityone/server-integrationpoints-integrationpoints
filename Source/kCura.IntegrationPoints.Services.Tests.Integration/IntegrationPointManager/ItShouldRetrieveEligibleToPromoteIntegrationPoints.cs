using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	public class ItShouldRetrieveEligibleToPromoteIntegrationPoints : RelativityProviderTemplate
	{
		public ItShouldRetrieveEligibleToPromoteIntegrationPoints()
			: base($"KeplerService_{Utils.FormatedDateTimeNow}", $"KeplerService_Target_{Utils.FormatedDateTimeNow}")
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
		public void ItShouldRetrieveEligibleToPromoteIntegrationPointsAtOnce()
		{
			// Arrange
			IList<Core.Models.IntegrationPointModel> expectedIntegrationPoints = CreateEligibleToPromoteIntegrationPoints();
			List<Core.Models.IntegrationPointModel> allIntegrationPoints = new List<Core.Models.IntegrationPointModel>();
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

		[Test]
		public void ItShouldNotRetrieveEligibleToPromoteIntegrationPoints()
		{
			// Arrange
			List<Core.Models.IntegrationPointModel> allIntegrationPoints = new List<Core.Models.IntegrationPointModel>();
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

		private IList<Core.Models.IntegrationPointModel> CreateEligibleToPromoteIntegrationPoints()
		{
			var eligibleToPromote1 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly,
				"Relativity_Provider_1", "Append Only", true);
			var eligibleToPromote2 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly,
				"Relativity_Provider_2", "Append Only", true);

			return new List<Core.Models.IntegrationPointModel>() { eligibleToPromote1 , eligibleToPromote2};


		}

		private IList<Core.Models.IntegrationPointModel> CreateNotEligibleToPromoteIntegrationPoints()
		{
			var notEligibleToPromote1 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly,
				"Relativity_Provider_3", "Append Only", false);
			var notEligibleToPromoteLdap = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly,
				"Relativity_Provider_4", "Append Only", false);
			notEligibleToPromoteLdap.SourceProvider = LdapProvider.ArtifactId;
			notEligibleToPromoteLdap.Type = Container.Resolve<IIntegrationPointTypeService>()
				.GetIntegrationPointType(kCura.IntegrationPoints.Core.Constants.IntegrationPoints.IntegrationPointTypes.ImportGuid)
				.ArtifactId;

			return new List<Core.Models.IntegrationPointModel>() {notEligibleToPromote1, notEligibleToPromoteLdap};
		}
	}
}
