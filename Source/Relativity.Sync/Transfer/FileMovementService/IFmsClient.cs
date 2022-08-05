using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.FileMovementService
{
    /// <summary>
    /// An interface for communicating with FileMovementService
    /// </summary>
    public interface IFmsClient
    {
        Task<RunStatusResponse> GetRunStatusAsync(
            RunStatusRequest runStatusRequest, CancellationToken cancellationToken);

        Task<CopyListOfFilesResponse> CopyListOfFilesAsync(
            CopyListOfFilesRequest copyListOfFilesRequest, CancellationToken cancellationToken);

        Task<string> CancelRunAsync(RunCancelRequest runCancelRequest, CancellationToken cancellationToken);
    }
}
