using System;
using System.Linq;
using System.Collections.Generic;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using kCura.Relativity.ImportAPI.Enumeration;
using Relativity.API;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	public class ImportApiFacade : IImportApiFacade
	{
		private const string _IAPI_GET_WORKSPACE_FIELDS_EXC = "EC: 4.1 There was an error in Import API when fetching workspace fields.";
		private const string _IAPI_GET_WORKSPACE_FIELDS_ERR =
			"EC: 4.1 There was an error in Import API when fetching workspace fields. workspaceArtifactId: {WorkspaceArtifactId}, artifactTypeID: {artifactTypeId}";

		private readonly Lazy<IImportAPI> _importApi;
		private readonly IAPILog _logger;

		public ImportApiFacade(IImportApiFactory importApiFactory, IConfig config, IAPILog logger)
		{
			_importApi = new Lazy<IImportAPI>(() => importApiFactory.GetImportAPI(new ImportSettings { WebServiceURL = config.WebApiPath }));
			_logger = logger.ForContext<ImportApiFacade>();
		}
		
		public ImportApiFacade(IImportApiFactory importApiFactory, ImportSettings importSettings, IAPILog logger)
		{
			_importApi = new Lazy<IImportAPI>(() => importApiFactory.GetImportAPI(importSettings));
			_logger = logger.ForContext<ImportApiFacade>();
		}

		public HashSet<int> GetMappableArtifactIdsWithNotIdentifierFieldCategory(int workspaceArtifactID, int artifactTypeID)
		{
			IEnumerable<int> fields = GetWorkspaceFields(workspaceArtifactID, artifactTypeID)
				.Where(y => y.FieldCategory != FieldCategoryEnum.Identifier)
				.Select(x => x.ArtifactID);
			return new HashSet<int>(fields);
		}

		public Dictionary<int, string> GetWorkspaceFieldsNames(int workspaceArtifactId, int artifactTypeId)
		{
			return GetWorkspaceFields(workspaceArtifactId, artifactTypeId)
				.ToDictionary(x => x.ArtifactID, x => x.Name);
		}

		public Dictionary<int, string> GetWorkspaceNames()
		{
			return _importApi.Value.Workspaces().ToDictionary(x => x.ArtifactID, x => x.Name);
		}

		private IEnumerable<Field> GetWorkspaceFields(int workspaceArtifactID, int artifactTypeID)
		{
			try
			{
				return _importApi.Value.GetWorkspaceFields(workspaceArtifactID, artifactTypeID);
			}
			catch (Exception e)
			{
				var exc = new IntegrationPointsException(_IAPI_GET_WORKSPACE_FIELDS_EXC, e)
				{
					ShouldAddToErrorsTab = true
				};
				_logger.LogError(exc, _IAPI_GET_WORKSPACE_FIELDS_ERR, workspaceArtifactID, artifactTypeID);
				throw exc;
			}
		}
	}
}
