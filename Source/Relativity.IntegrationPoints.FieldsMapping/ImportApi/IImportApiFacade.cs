using System.Collections.Generic;

namespace Relativity.IntegrationPoints.FieldsMapping.ImportApi
{
    public interface IImportApiFacade
    {
        HashSet<int> GetMappableArtifactIdsWithNotIdentifierFieldCategory(int workspaceArtifactID, int artifactTypeID);

        Dictionary<int, string> GetWorkspaceFieldsNames(int workspaceArtifactId, int artifactTypeId);

        Dictionary<int, string> GetWorkspaceNames();
    }
}
