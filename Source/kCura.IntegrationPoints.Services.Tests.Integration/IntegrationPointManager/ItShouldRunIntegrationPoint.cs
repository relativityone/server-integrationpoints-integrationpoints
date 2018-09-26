using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	public class ItShouldRunIntegrationPoint : RelativityProviderTemplate
	{
		public ItShouldRunIntegrationPoint() : base($"KeplerService_{Utils.FormatedDateTimeNow}", $"KeplerService_Target_{Utils.FormatedDateTimeNow}")
		{
		}

		//[Test]
		//[Category(Constants.SMOKE_TEST)]
		public void Execute()
		{
			var ipModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, $"ip_{Utils.FormatedDateTimeNow}", "Append Only");
			var ip = CreateOrUpdateIntegrationPoint(ipModel);

			var client = Helper.CreateAdminProxy<IIntegrationPointManager>();
			client.RunIntegrationPointAsync(SourceWorkspaceArtifactId, ip.ArtifactID).Wait();

			Status.WaitForIntegrationPointJobToComplete(Container, SourceWorkspaceArtifactId, ip.ArtifactID);

			var jobHistoryDataTable = Helper.GetDBContext(SourceWorkspaceArtifactId).ExecuteSqlStatementAsDataTable("SELECT * FROM [JobHistory]");

			Assert.That(jobHistoryDataTable.Rows.Count, Is.EqualTo(1));

			Assert.That(jobHistoryDataTable.Rows[0]["Name"], Is.EqualTo(ip.Name));
		}
	}
}