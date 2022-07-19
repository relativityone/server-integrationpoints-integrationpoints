using System;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Transfer
{
    internal interface IBatchDataReaderBuilder
    {
        Action<string, string> ItemLevelErrorHandler { get; set; }

        Task<IBatchDataReader> BuildAsync(int sourceWorkspaceArtifactId, RelativityObjectSlim[] batch, CancellationToken token);
    }
}