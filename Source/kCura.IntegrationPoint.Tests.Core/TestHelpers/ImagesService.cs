using System.Collections.Generic;
using System.Linq;
using Relativity.Core.DTO;
using System.Data;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	public class ImagesService : FileServiceBase, IImagesService
	{
		public ImagesService(ITestHelper testHelper) : base(testHelper)
		{
		}

		public IList<File> GetImagesFileInfo(int workspaceId, int documentArtifactId)
		{
			DataTable imagesTable = SearchManager.RetrieveImagesForDocuments(workspaceId, new[] { documentArtifactId }).Tables[0];

			if (imagesTable == null || imagesTable.Rows.Count == 0)
			{
				return new List<File>();
			}

			return imagesTable.Rows.Cast<DataRow>().Select(row => new File(row)).ToList();
		}
	}
}