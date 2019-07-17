using kCura.IntegrationPoint.Tests.Core.TestHelpers.Converters;
using kCura.IntegrationPoint.Tests.Core.TestHelpers.Dto;

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
			DataTable nativesTable = SearchManager.RetrieveNativesForSearch(workspaceId, documentArtifactId.ToString()).Tables[0];

			if (nativesTable.Rows.Count == 0)
			{
				return null;
			}

			DataRow firstDataRow = nativesTable.Rows[0];
			return firstDataRow.ToFileTestDto();
		}
	}
}