using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer
{
    internal interface IFileLocationManager
    {
        void TranslateAndStoreFilePaths(IDictionary<int, INativeFile> artifactIdToNativeFile);
        IList<FmsBatchInfo> GetStoredLocations();
        void ClearStoredLocations();
    }
}
