using System.Linq;
using System.Collections.Generic;
using kCura.IntegrationPoints.Config;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using kCura.Relativity.ImportAPI.Enumeration;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	public class ImportApiFacade : IImportApiFacade
	{
		private readonly IImportAPI _importApi;

		public ImportApiFacade(IImportApiFactory importApiFactory, IConfig config)
		{
			_importApi = importApiFactory.GetImportAPI(new ImportSettings {WebServiceURL = config.WebApiPath});
		}

		public HashSet<int> GetMappableArtifactIdsWithNotIdentifierFieldCategory(int workspaceArtifactID, int artifactTypeID)
		{
			IEnumerable<int> fields = GetWorkspaceFields(workspaceArtifactID, artifactTypeID)
				.Where(y => y.FieldCategory != FieldCategoryEnum.Identifier)
				.Select(x => x.ArtifactID);
			return new HashSet<int>(fields);
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
			return _importApi.GetWorkspaceFields(workspaceArtifactID, artifactTypeID);
		}

	}
}
