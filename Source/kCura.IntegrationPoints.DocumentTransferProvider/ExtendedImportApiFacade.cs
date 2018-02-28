using System.Linq;
using System.Collections.Generic;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	public class ExtendedImportApiFacade : IExtendedImportApiFacade
	{
		private readonly IExtendedImportAPI _extendedImportApi;

		public ExtendedImportApiFacade(IExtendedImportApiFactory extendedImportApiFactory)
		{
			_extendedImportApi = extendedImportApiFactory.Create();
		}

		public HashSet<int> GetMappableArtifactIdsExcludeFields(int workspaceArtifactID, int artifactTypeID, HashSet<string> ignoredFields)
		{
			IEnumerable<int> fields = GetWorkspaceFields(workspaceArtifactID, artifactTypeID)
				.Where(f => !ignoredFields.Contains(f.Name))
				.Select(x => x.ArtifactID);
			return new HashSet<int>(fields);
		}

		private IEnumerable<Field> GetWorkspaceFields(int workspaceArtifactID, int artifactTypeID)
		{
			return _extendedImportApi.GetWorkspaceFields(workspaceArtifactID, artifactTypeID);
		}

	}
}
