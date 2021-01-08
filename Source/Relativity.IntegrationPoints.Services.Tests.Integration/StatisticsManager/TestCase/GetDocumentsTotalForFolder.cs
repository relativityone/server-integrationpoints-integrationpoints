using System.Data.SqlClient;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public class GetDocumentsTotalForFolder : IStatisticsTestCase
	{
		public void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings)
		{
			long total;
			using (IStatisticsManager statisticsManager = helper.CreateProxy<IStatisticsManager>())
			{
				total = statisticsManager.GetDocumentsTotalForFolderAsync(workspaceArtifactId, testCaseSettings.FolderId, testCaseSettings.ViewId, false).GetAwaiter().GetResult();
			}

			var sqlText =
				"SELECT COUNT(*) FROM [Document] WHERE [Document].[ParentArtifactID_D] = @FolderId";

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