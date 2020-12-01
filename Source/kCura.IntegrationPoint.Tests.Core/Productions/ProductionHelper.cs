using kCura.IntegrationPoint.Tests.Core.Models;

namespace kCura.IntegrationPoint.Tests.Core.Productions
{
	public class ProductionHelper
	{
		private readonly int _workspaceId;
		private readonly WorkspaceService _workspaceService;

		public ProductionHelper(int workspaceId)
		{
			_workspaceId = workspaceId;
			_workspaceService = new WorkspaceService(new ImportHelper());
		}

		public int CreateProductionSet(string productionName)
		{
			return _workspaceService.CreateProductionAsync(_workspaceId, productionName).GetAwaiter().GetResult();
		}

		public int CreateProductionSetAndImportData(string productionName, DocumentTestDataBuilder.TestDataType testDataType)
		{
			int productionID = CreateProductionSet(productionName);
			ImportDocuments(testDataType);
			DocumentsTestData testDataForProductionImport = DocumentTestDataBuilder.BuildTestData(testDataType: testDataType);
			_workspaceService.ImportDataToProduction(_workspaceId, productionID, testDataForProductionImport.Images);

			return productionID;
		}

		private void ImportDocuments(DocumentTestDataBuilder.TestDataType testDataType)
		{
			DocumentsTestData testData = DocumentTestDataBuilder.BuildTestData(testDataType: testDataType);
			_workspaceService.ImportData(_workspaceId, testData);
		}
	}
}
