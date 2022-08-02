using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.ADF
{
    internal interface IAdlsUploader
    {
        Task<string> UploadFileAsync(string sourceFilePath, CancellationToken cancellationToken);

        string CreateBatchFile(FmsBatchInfo storedLocation, CancellationToken cancellationToken);

        Task DeleteFileOnAdlsAsync(string filePath, CancellationToken cancellationToken);
    }
}
