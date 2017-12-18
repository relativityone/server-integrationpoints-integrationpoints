using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public class GetImagesTotalForProduction : IStatisticsTestCase
	{
		public void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings)
		{
			long total;
			using (IStatisticsManager statisticsManager = helper.CreateAdminProxy<IStatisticsManager>())
			{
				total = statisticsManager.GetImagesTotalForProductionAsync(workspaceArtifactId, testCaseSettings.ProductionId).Result;
			}

			var sqlText =
				$"SELECT COUNT(*) FROM [ProductionDocumentFile_{testCaseSettings.ProductionId}] AS PDF JOIN [File] ON PDF.[ProducedFileID] = [File].[FileID] WHERE [File].[Type] = 3";

			var expectedResult = helper.GetDBContext(workspaceArtifactId).ExecuteSqlStatementAsScalar<int>(sqlText);

			Assert.That(total, Is.EqualTo(expectedResult));
		}
	}
}