using System.Collections.Generic;

namespace Relativity.Sync.Transfer
{
    internal interface IFileLocationManager
    {
        void TranslateAndStoreFilePaths(IDictionary<int, INativeFile> artifactIdToNativeFile);
        IList<FmsBatchInfo> GetStoredLocations();
        void ClearStoredLocations();
    }
}
