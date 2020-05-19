using kCura.IntegrationPoint.Tests.Core;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	internal class ProductionHelper
	{
		private readonly WorkspaceService _workspaceService;

		private readonly TestContext _testContext;

		public ProductionHelper(TestContext testContext)
		{
			_testContext = testContext;
			_workspaceService = new WorkspaceService(new ImportHelper());
		}

		public void CreateProductionSet(string productionName)
		{
			_workspaceService.CreateProductionAsync(_testContext.GetWorkspaceId(), productionName).GetAwaiter().GetResult();
		}

		public void CreateAndRunProduction(string productionName)
		{
			CreateProductionSet(productionName);
		}

		public void CreateAndRunProduction(string savedSearchName, string productionName)
		{
			CreateProductionSet(productionName);
		}
	}
}
