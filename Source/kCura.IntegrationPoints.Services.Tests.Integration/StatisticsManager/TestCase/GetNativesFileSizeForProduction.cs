using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public class GetNativesFileSizeForProduction : IStatisticsTestCase
	{
		public void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings)
		{
			int total;
			using (IStatisticsManager statisticsManager = helper.CreateAdminProxy<IStatisticsManager>())
			{
				total = statisticsManager.GetNativesFileSizeForProductionAsync(workspaceArtifactId, testCaseSettings.ProductionId).Result;
			}

			var sqlText =
				$"SELECT SUM([Size]) FROM [File] WHERE [File].[Type] = 0 AND [File].[DocumentArtifactID] IN (SELECT PDF.[DocumentArtifactId] FROM [ProductionDocumentFile_{testCaseSettings.ProductionId}] AS PDF)";

			var expectedResult = helper.GetDBContext(workspaceArtifactId).ExecuteSqlStatementAsScalar<int>(sqlText);

			Assert.That(total, Is.EqualTo(expectedResult));
		}
	}
}