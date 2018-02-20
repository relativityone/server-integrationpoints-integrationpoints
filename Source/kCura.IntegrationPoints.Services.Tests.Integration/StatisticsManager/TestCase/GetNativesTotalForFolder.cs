using System.Data.SqlClient;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public class GetNativesTotalForFolder : IStatisticsTestCase
	{
		public void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings)
		{
			long total;
			using (IStatisticsManager statisticsManager = helper.CreateAdminProxy<IStatisticsManager>())
			{
				total = statisticsManager.GetNativesTotalForFolderAsync(workspaceArtifactId, testCaseSettings.FolderId, testCaseSettings.ViewId, false).Result;
			}

			var sqlText =
				"SELECT COUNT(*) FROM [File] JOIN [Document] ON [Document].[ArtifactID] = [File].[DocumentArtifactID] WHERE [Document].[ParentArtifactID_D] = @FolderId AND [File].[Type] = 0";

			var folderParameter = new SqlParameter
			{
				ParameterName = "@FolderId",
				Value = testCaseSettings.FolderId
			};

			var expectedResult = helper.GetDBContext(workspaceArtifactId).ExecuteSqlStatementAsScalar<int>(sqlText, folderParameter);

			Assert.That(total, Is.EqualTo(expectedResult));
		}
	}
}