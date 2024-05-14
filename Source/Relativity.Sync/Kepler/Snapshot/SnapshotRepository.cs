using System;
using System.Threading.Tasks;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Kepler.Snapshot
{
    internal class SnapshotRepository : ISnapshotRepository
    {
        private readonly IProxyFactoryDocument _serviceFactorydoc;

        public SnapshotRepository(IProxyFactoryDocument serviceFactorydoc)
        {
            _serviceFactorydoc = serviceFactorydoc;
        }

        public async Task<RelativityObjectSlim[]> ReadSnapshotResultsAsync(int workspaceId, Guid snapshotId, int resultsBlockSize, int exportIndex, Identity identity)
        {
            IObjectManager objectManager = await _serviceFactorydoc.CreateProxyDocumentAsync<IObjectManager>(identity).ConfigureAwait(false);

            return await objectManager
                    .RetrieveResultsBlockFromExportAsync(workspaceId, snapshotId, resultsBlockSize, exportIndex)
                    .ConfigureAwait(false);
        }
    }
}
