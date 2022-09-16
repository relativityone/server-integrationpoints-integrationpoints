using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.ADLS
{
    internal interface IAdlsUploader
    {
        Task<string> UploadFileAsync(string sourceFilePath, CancellationToken cancellationToken);

        string CreateBatchFile(FmsBatchInfo storedLocation, CancellationToken cancellationToken);

        Task DeleteFileAsync(string filePath, CancellationToken cancellationToken);
    }
}
