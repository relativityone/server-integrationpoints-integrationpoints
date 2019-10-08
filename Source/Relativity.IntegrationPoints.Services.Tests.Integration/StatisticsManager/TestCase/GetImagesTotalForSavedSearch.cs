using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public class GetImagesTotalForSavedSearch : IStatisticsTestCase
	{
		public void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings)
		{
			long total;
			using (IStatisticsManager statisticsManager = helper.CreateProxy<IStatisticsManager>())
			{
				total = statisticsManager.GetImagesTotalForSavedSearchAsync(workspaceArtifactId, testCaseSettings.SavedSearchId).Result;
			}

			Assert.That(total, Is.EqualTo(testCaseSettings.DocumentsTestData.Images.Rows.Count));
		}
	}
}