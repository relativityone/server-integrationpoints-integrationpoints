using System.Collections.Generic;
using System.Linq;
using System.Data;
using kCura.IntegrationPoint.Tests.Core.TestHelpers.Converters;
using kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
    public class ImagesService : FileServiceBase, IImagesService
    {
        public ImagesService(ITestHelper testHelper) : base(testHelper)
        {
        }
               
        public IList<FileTestDto> GetImagesFileInfo(int workspaceId, int documentArtifactId)
        {
            using (ISearchService searchService = TestHelper.CreateProxy<ISearchService>())
            {
                DataTable imagesTable = searchService
                    .RetrieveImagesForSearchAsync(workspaceId, new[] { documentArtifactId }, string.Empty)
                    .GetAwaiter()
                    .GetResult()
                    .Unwrap()
                    .Tables[0];
                
                if (imagesTable == null || imagesTable.Rows.Count == 0)
                {
                    return new List<FileTestDto>();
                }
                return imagesTable
                    .Rows
                    .Cast<DataRow>()
                    .Select(x => x.ToFileTestDto())
                    .ToList();
            }
        }
    }
}