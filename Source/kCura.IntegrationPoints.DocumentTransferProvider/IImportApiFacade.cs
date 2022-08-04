using System.Collections.Generic;

namespace kCura.IntegrationPoints.DocumentTransferProvider
{
    public interface IImportApiFacade
    {
        HashSet<int> GetMappableArtifactIdsExcludeFields(int workspaceArtifactID, int artifactTypeID,
            HashSet<string> ignoredFields);
    }
}
