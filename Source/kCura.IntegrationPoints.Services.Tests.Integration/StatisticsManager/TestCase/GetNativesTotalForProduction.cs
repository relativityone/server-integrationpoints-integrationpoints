using System.Data.SqlClient;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public class GetNativesTotalForProduction : IStatisticsTestCase
	{
		public void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings)
		{
			long total;
			using (IStatisticsManager statisticsManager = helper.CreateAdminProxy<IStatisticsManager>())
			{
				total = statisticsManager.GetNativesTotalForProductionAsync(workspaceArtifactId, testCaseSettings.ProductionId).Result;
			}

			var sqlText =
				"SELECT COUNT(DISTINCT([Document].[ArtifactId])) FROM [ProductionInformation] JOIN [Document] ON [Document].[ArtifactID] = [ProductionInformation].Document WHERE [Document].[HasNative] = 1 AND [ProductionInformation].[Identifier] = @ProductionId";

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