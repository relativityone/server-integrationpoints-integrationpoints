using System;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Kepler.Snapshot
{
    internal interface ISnapshotRepository
    {
        Task<RelativityObjectSlim[]> ReadSnapshotResultsAsync(
            int workspaceId,
            Guid snapshotId,
            int resultsBlockSize,
            int exportIndex,
            Identity identity);
    }
}
