namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	using System.Data;
	using global::Relativity.Core.DTO;

	public class NativesService : FileServiceBase, INativesService
	{
		public NativesService(ITestHelper testHelper) : base(testHelper)
		{
			
		}

		public File GetNativeFileInfo(int workspaceId, int documentArtifactId)
		{
			DataTable nativesTable = SearchManager.RetrieveNativesForSearch(workspaceId, documentArtifactId.ToString()).Tables[0];

			if (nativesTable.Rows.Count == 0)
			{
				return null;
			}

			return new File(nativesTable.Rows[0]);
		}
	}
}