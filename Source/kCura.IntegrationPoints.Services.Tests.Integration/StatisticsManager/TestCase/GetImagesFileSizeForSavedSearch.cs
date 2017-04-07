using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public class GetImagesFileSizeForSavedSearch : IStatisticsTestCase
	{
		public void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings)
		{
			int total;
			using (IStatisticsManager statisticsManager = helper.CreateAdminProxy<IStatisticsManager>())
			{
				total = statisticsManager.GetImagesFileSizeForSavedSearchAsync(workspaceArtifactId, testCaseSettings.SavedSearchId).Result;
			}

			var expectedResult = helper.GetDBContext(workspaceArtifactId).ExecuteSqlStatementAsScalar<int>("SELECT SUM([Size]) FROM [File] WHERE Type=1");

			Assert.That(total, Is.EqualTo(expectedResult));
		}
	}
}