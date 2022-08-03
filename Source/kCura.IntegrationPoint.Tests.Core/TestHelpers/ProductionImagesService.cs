using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.TestHelpers.Converters;
using kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
    public class ProductionImagesService : FileServiceBase, IProductionImagesService
    {
        public ProductionImagesService(ITestHelper testHelper) : base(testHelper)
        {
        }

        public IList<FileTestDto> GetProductionImagesFileInfo(int workspaceId, int documentArtifactId)
        {
            using (ISearchService searchService = TestHelper.CreateProxy<ISearchService>())
            {
                DataSet dataSet = searchService
                    .RetrieveProducedImagesForDocumentAsync(workspaceId, documentArtifactId, string.Empty)
                    .GetAwaiter()
                    .GetResult()
                    .Unwrap();

                DataTable imagesTable = dataSet.Tables[0];

                if (imagesTable == null || imagesTable.Rows.Count == 0)
                {
                    return new List<FileTestDto>();
                }

                return imagesTable
                    .Rows
                    .Cast<DataRow>()
                    .Select(row => row.ToFileTestDto())
                    .ToList();
            }
        }
    }
}