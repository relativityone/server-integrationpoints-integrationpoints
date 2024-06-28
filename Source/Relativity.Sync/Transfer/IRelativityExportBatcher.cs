using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
    internal interface IRelativityExportBatcher
    {
        Task<RelativityObjectSlim[]> GetNextItemsFromBatchAsync(CancellationToken cancellationToken);
    }
}
