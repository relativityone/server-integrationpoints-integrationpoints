using System.Collections.Generic;
using kCura.Relativity.ImportAPI.Data;

namespace Relativity.IntegrationPoints.FieldsMapping.ImportApi
{
    public interface IFieldsMappingImportApiFacade
    {
        IEnumerable<Field> GetWorkspaceFields(int workspaceId, int documentArtifactTypeId);
    }
}
