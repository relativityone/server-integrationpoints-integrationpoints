using System.Data.SqlClient;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public class GetDocumentsTotalForProduction : IStatisticsTestCase
	{
		public void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings)
		{
			int total;
			using (IStatisticsManager statisticsManager = helper.CreateAdminProxy<IStatisticsManager>())
			{
				total = statisticsManager.GetDocumentsTotalForProductionAsync(workspaceArtifactId, testCaseSettings.ProductionId).Result;
			}

			var sqlText =
				"SELECT COUNT(DISTINCT([Document])) FROM [ProductionInformation] WHERE [Identifier] = @ProductionId";

			var folderParameter = new SqlParameter
			{
				ParameterName = "@ProductionId",
				Value = testCaseSettings.ProductionId
			};

			var expectedResult = helper.GetDBContext(workspaceArtifactId).ExecuteSqlStatementAsScalar<int>(sqlText, folderParameter);

			Assert.That(total, Is.EqualTo(expectedResult));
		}
	}
}