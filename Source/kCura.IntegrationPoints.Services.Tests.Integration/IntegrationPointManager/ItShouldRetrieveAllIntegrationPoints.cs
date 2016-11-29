﻿using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	public class ItShouldRetrieveAllIntegrationPoints : RelativityProviderTemplate
	{
		public ItShouldRetrieveAllIntegrationPoints() : base($"KeplerService_{Utils.FormatedDateTimeNow}", $"KeplerService_Target_{Utils.FormatedDateTimeNow}")
		{
		}

		private IIntegrationPointManager _client;
		private IList<Core.Models.IntegrationPointModel> _expectedIntegrationPoints;

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			var rel1 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "Relativity_Provider_1", "Append Only");
			var rel2 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "Relativity_Provider_2", "Append Only");
			var ldap1 = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "LDAP_Provider_1", "Append Only");
			ldap1.SourceProvider = LdapProvider.ArtifactId;

			_expectedIntegrationPoints = new List<Core.Models.IntegrationPointModel> {rel1, rel2, ldap1};

			_expectedIntegrationPoints.ForEach(x =>
			{
				x.ArtifactID = CreateOrUpdateIntegrationPoint(x).ArtifactID;
			});

			_client = Helper.CreateAdminProxy<IIntegrationPointManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client.Dispose();
		}

		[Test]
		public void ItShouldRetrieveAllIntegrationPointsAtOnce()
		{
			var integrationPointModels = _client.GetAllIntegrationPointsAsync(WorkspaceArtifactId).Result;
			Assert.That(integrationPointModels.Count, Is.EqualTo(_expectedIntegrationPoints.Count));

			foreach (var integrationPointModel in integrationPointModels)
			{
				Assert.That(_expectedIntegrationPoints.Any(x => x.Name == integrationPointModel.Name && x.SourceProvider == integrationPointModel.SourceProvider));
			}
		}

		[Test]
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