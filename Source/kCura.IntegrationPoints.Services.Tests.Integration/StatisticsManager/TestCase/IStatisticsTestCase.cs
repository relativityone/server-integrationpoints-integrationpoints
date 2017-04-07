using kCura.IntegrationPoint.Tests.Core.TestHelpers;

namespace kCura.IntegrationPoints.Services.Tests.Integration.StatisticsManager.TestCase
{
	public interface IStatisticsTestCase
	{
		void Execute(ITestHelper helper, int workspaceArtifactId, TestCaseSettings testCaseSettings);
	}
}