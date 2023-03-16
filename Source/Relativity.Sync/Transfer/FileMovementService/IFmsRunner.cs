using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Transfer.FileMovementService.Models;

namespace Relativity.Sync.Transfer.FileMovementService
{
    /// <summary>
    /// An interface for running File Movement Service jobs
    /// </summary>
    internal interface IFmsRunner
    {
        Task<List<FmsBatchStatusInfo>> RunAsync(List<FmsBatchInfo> batches, CancellationToken cancellationToken);

        Task<List<FmsBatchStatusInfo>> MonitorAsync(List<FmsBatchStatusInfo> batches, CancellationToken cancellationToken);
    }
}
