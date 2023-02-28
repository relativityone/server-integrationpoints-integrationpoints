using System;
using System.Linq;
using System.Collections.Generic;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Relativity.API;
using kCura.IntegrationPoints.Domain.Exceptions;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
    public class ImportApiFacade : IImportApiFacade
    {
        private const string _IAPI_GET_WORKSPACE_FIELDS_EXC = "EC: 4.2 There was an error in Import API when fetching workspace fields.";
        private const string _IAPI_GET_WORKSPACE_FIELDS_ERR =
            "EC: 4.2 There was an error in Import API when fetching workspace fields. workspaceArtifactId: {WorkspaceArtifactId}, artifactTypeID: {artifactTypeId}";
        private readonly Lazy<IImportAPI> _importApi;
        private readonly IAPILog _logger;

        public ImportApiFacade(IImportApiFactory importApiFactory, IAPILog logger)
        {
            _importApi = new Lazy<IImportAPI>(importApiFactory.Create);
            _logger = logger.ForContext<ImportApiFacade>();
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
