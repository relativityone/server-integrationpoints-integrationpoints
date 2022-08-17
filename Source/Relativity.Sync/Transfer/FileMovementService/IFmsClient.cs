using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Transfer.FileMovementService.Models;

namespace Relativity.Sync.Transfer.FileMovementService
{
    /// <summary>
    /// An interface for communicating with FileMovementService
    /// </summary>
    internal interface IFmsClient
    {
        Task<RunStatusResponse> GetRunStatusAsync(RunStatusRequest request, CancellationToken cancellationToken);

        Task<CopyListOfFilesResponse> CopyListOfFilesAsync(CopyListOfFilesRequest request, CancellationToken cancellationToken);

        Task<string> CancelRunAsync(RunCancelRequest request, CancellationToken cancellationToken);
    }
}
