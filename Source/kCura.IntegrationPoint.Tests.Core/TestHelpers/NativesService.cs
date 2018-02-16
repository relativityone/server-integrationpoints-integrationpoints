namespace kCura.IntegrationPoint.Tests.Core.TestHelpers
{
	using System;
	using System.Data;
	using global::Relativity.Core.DTO;
	using WinEDDS.Service.Export;

	public class NativesService : INativesService
	{
		private readonly Lazy<ISearchManager> _searchManagerLazy;
		private ISearchManager SearchManager => _searchManagerLazy.Value;

		public NativesService(ITestHelper testHelper)
		{
			_searchManagerLazy = new Lazy<ISearchManager>(testHelper.CreateSearchManager);
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