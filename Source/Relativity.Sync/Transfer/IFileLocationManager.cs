using System.Collections.Generic;

namespace Relativity.Sync.Transfer
{
    internal interface IFileLocationManager
    {
        void TranslateAndStoreFilePaths(IDictionary<int, INativeFile> artifactIdToNativeFile);

        List<FmsBatchInfo> GetStoredLocations();

        void ClearStoredLocations();
    }
}
