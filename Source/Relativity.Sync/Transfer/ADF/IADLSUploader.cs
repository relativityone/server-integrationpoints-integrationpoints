using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.ADF
{
    internal interface IADLSUploader
    {
        Task<string> UploadFileAsync(string sourceFilePath, CancellationToken cancellationToken);

        string CreateBatchFile(IEnumerable<FmsBatchInfo> storedLocations, CancellationToken cancellationToken);
    }
}
