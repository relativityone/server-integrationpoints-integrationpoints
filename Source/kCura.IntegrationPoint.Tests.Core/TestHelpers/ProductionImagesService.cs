using System.Collections.Generic;
using System.Data;
using System.Linq;
using Relativity.Core.DTO;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ProductionImagesService : FileServiceBase, IProductionImagesService
	{
		public ProductionImagesService(ITestHelper testHelper) : base(testHelper)
		{
		}

		public IList<File> GetProductionImagesFileInfo(int workspaceId, int documentArtifactId)
		{
			DataSet dataSet = SearchManager.RetrieveProducedImagesForDocument(workspaceId, documentArtifactId);
			DataTable imagesTable = dataSet.Tables[0];

			if (imagesTable == null || imagesTable.Rows.Count == 0)
			{
				return new List<File>();
			}

			return imagesTable.Rows.Cast<DataRow>().Select(row => new File(row)).ToList();
		}
	}
}