using System;
using kCura.IntegrationPoint.Tests.Core.TestHelpers.Converters;
using kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;

namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	using System.Data;

	public class NativesService : FileServiceBase, INativesService
	{
		public NativesService(ITestHelper testHelper) : base(testHelper)
		{
		}

		public FileTestDto GetNativeFileInfo(int workspaceId, int documentArtifactId)
		{
            using (ISearchService searchService = TestHelper.CreateProxy<ISearchService>())
            {
                DataTable nativesTable = searchService
                    .RetrieveNativesForSearchAsync(workspaceId, documentArtifactId.ToString(), string.Empty)
                    .GetAwaiter()
                    .GetResult()
                    .Unwrap()
                    .Tables[0];

                if (nativesTable.Rows.Count == 0)
                {
                    return null;
                }

                DataRow firstDataRow = nativesTable.Rows[0];
                return firstDataRow.ToFileTestDto();
			}
        }
	}
}