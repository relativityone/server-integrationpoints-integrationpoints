﻿using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public class GetImagesFileSizeForProduction : IStatisticsTestCase
	{
		public void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings)
		{
			long total;
			using (IStatisticsManager statisticsManager = helper.CreateProxy<IStatisticsManager>())
			{
				total = statisticsManager.GetImagesFileSizeForProductionAsync(workspaceArtifactId, testCaseSettings.ProductionId).Result;
			}

			var sqlText =
				$"SELECT SUM([Size]) FROM [ProductionDocumentFile_{testCaseSettings.ProductionId}] AS PDF JOIN [File] ON PDF.[ProducedFileID] = [File].[FileID]";
			
			var expectedResult = helper.GetDBContext(workspaceArtifactId).ExecuteSqlStatementAsScalar<int>(sqlText);

			Assert.That(total, Is.EqualTo(expectedResult));
		}
	}
}