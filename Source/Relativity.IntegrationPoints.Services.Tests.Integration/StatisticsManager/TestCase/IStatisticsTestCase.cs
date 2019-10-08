using kCura.IntegrationPoint.Tests.Core.TestHelpers;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public interface IStatisticsTestCase
	{
		void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings);
	}
}