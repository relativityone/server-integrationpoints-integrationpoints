using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Models;

namespace kCura.IntegrationPoints.UITests.Configuration.Helpers
{
	internal class ProductionHelper
	{
		private readonly TestContext _testContext;
		private readonly WorkspaceService _workspaceService;

		public ProductionHelper(TestContext testContext)
		{
			_testContext = testContext;
			_workspaceService = new WorkspaceService(new ImportHelper());
		}

		public int CreateProductionSet(string productionName)
		{
			return _workspaceService.CreateProductionAsync(_testContext.GetWorkspaceId(), productionName).GetAwaiter().GetResult();
		}

		public void CreateProductionSetAndImportData(string productionName, DocumentTestDataBuilder.TestDataType testDataType)
		{
			int productionID = CreateProductionSet(productionName);
			ImportDocuments(testDataType);
			DocumentsTestData testDataForProductionImport = DocumentTestDataBuilder.BuildTestData(testDataType: testDataType);
			_workspaceService.ImportDataToProduction(_testContext.GetWorkspaceId(), productionID, testDataForProductionImport.Images);
		}

		private void ImportDocuments(DocumentTestDataBuilder.TestDataType testDataType)
		{
			DocumentsTestData testData = DocumentTestDataBuilder.BuildTestData(testDataType: testDataType);
			_workspaceService.ImportData(_testContext.GetWorkspaceId(), testData);
		}
	}
}
