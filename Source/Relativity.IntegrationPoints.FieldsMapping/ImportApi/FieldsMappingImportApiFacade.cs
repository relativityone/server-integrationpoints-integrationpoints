using System.Collections.Generic;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;

namespace Relativity.IntegrationPoints.FieldsMapping.ImportApi
{
    class FieldsMappingImportApiFacade : IFieldsMappingImportApiFacade
    {
        private readonly IImportAPI _importApi;

        public FieldsMappingImportApiFacade(IImportAPI importApi)
        {
            _importApi = importApi;
        }
        public IEnumerable<Field> GetWorkspaceFields(int workspaceId, int documentArtifactTypeId)
        {
            return _importApi.GetWorkspaceFields(workspaceId, documentArtifactTypeId);
        }
    }
}
