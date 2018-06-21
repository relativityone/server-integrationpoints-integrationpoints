using System;
using System.Linq;
using System.Collections.Generic;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Relativity.API;
using kCura.IntegrationPoints.Domain.Exceptions;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
	public class ExtendedImportApiFacade : IExtendedImportApiFacade
	{
		private const string _IAPI_GET_WORKSPACE_FIELDS_EXC = "EC: 4.2 There was an error in Extended Import API when fetching workspace fields.";
		private const string _IAPI_GET_WORKSPACE_FIELDS_ERR =
			"EC: 4.2 There was an error in Extended Import API when fetching workspace fields. workspaceArtifactId: {WorkspaceArtifactId}, artifactTypeID: {artifactTypeId}";

		private readonly Lazy<IExtendedImportAPI> _extendedImportApi;
		private readonly IAPILog _logger;

		public ExtendedImportApiFacade(IExtendedImportApiFactory extendedImportApiFactory, IAPILog logger)
		{
			_extendedImportApi = new Lazy<IExtendedImportAPI>(extendedImportApiFactory.Create);
			_logger = logger.ForContext<ExtendedImportApiFacade>();
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
			try
			{
				return _extendedImportApi.Value.GetWorkspaceFields(workspaceArtifactID, artifactTypeID);
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
