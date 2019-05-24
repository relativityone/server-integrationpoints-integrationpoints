﻿using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	public class ItShouldRunIntegrationPoint : RelativityProviderTemplate
	{
		public ItShouldRunIntegrationPoint() : base($"KeplerService_{Utils.FormattedDateTimeNow}", $"KeplerService_Target_{Utils.FormattedDateTimeNow}")
		{
		}

        [Test]
		[SmokeTest]
		public void Execute()
		{
			var ipModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, $"ip_{Utils.FormattedDateTimeNow}", "Append Only");
			var ip = CreateOrUpdateIntegrationPoint(ipModel);

			var client = Helper.CreateAdminProxy<IIntegrationPointManager>();
			client.RunIntegrationPointAsync(SourceWorkspaceArtifactID, ip.ArtifactID).Wait();

			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactID, ip.ArtifactID);

			var jobHistoryDataTable = Helper.GetDBContext(SourceWorkspaceArtifactID).ExecuteSqlStatementAsDataTable("SELECT * FROM [JobHistory]");

			Assert.That(jobHistoryDataTable.Rows.Count, Is.EqualTo(1));

			Assert.That(jobHistoryDataTable.Rows[0]["Name"], Is.EqualTo(ip.Name));
		}
	}
}