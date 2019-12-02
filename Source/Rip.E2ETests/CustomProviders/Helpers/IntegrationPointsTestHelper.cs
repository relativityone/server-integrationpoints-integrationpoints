using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.IntegrationPoints.Services;
using Rip.E2ETests.CustomProviders.TestCases;

namespace Rip.E2ETests.CustomProviders.Helpers
{
	internal static class IntegrationPointsTestHelper
	{
		public static async Task<int> SaveIntegrationPointAsync(
			ITestHelper testHelper,
			IRelativityObjectManager objectManager,
			CreateIntegrationPointRequest integrationPointCreateRequest,
			CustomProviderTestCase testCase)
		{
			using (var integrationPointManager = testHelper.CreateProxy<IIntegrationPointManager>())
			{
				IntegrationPointModel createdIntegrationPoint = await integrationPointManager
					.CreateIntegrationPointAsync(integrationPointCreateRequest)
					.ConfigureAwait(false);
				return createdIntegrationPoint.ArtifactId;
			}
		}

		public static async Task RunIntegrationPointAsync(
			ITestHelper testHelper,
			int workspaceID,
			int integrationPointId)
		{
			using (var integrationPointManager = testHelper.CreateProxy<IIntegrationPointManager>())
			{
				await integrationPointManager
					.RunIntegrationPointAsync(workspaceID, integrationPointId)
					.ConfigureAwait(false);
			}
		}
	}
}