using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public class GetDocumentsTotalForSavedSearch : IStatisticsTestCase
	{
		public void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings)
		{
			long total;
			using (IStatisticsManager statisticsManager = helper.CreateAdminProxy<IStatisticsManager>())
			{
				total = statisticsManager.GetDocumentsTotalForSavedSearchAsync(workspaceArtifactId, testCaseSettings.SavedSearchId).Result;
			}

			Assert.That(total, Is.EqualTo(testCaseSettings.DocumentsTestData.AllDocumentsDataTable.Rows.Count));
		}
	}
}