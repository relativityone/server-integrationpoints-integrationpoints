using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
    internal interface IImportJob : IDisposable
    {
        Task<ImportJobResult> RunAsync(CompositeCancellationToken token);
        Task<IEnumerable<int>> GetPushedDocumentArtifactIdsAsync();
        Task<IEnumerable<string>> GetPushedDocumentIdentifiersAsync();
        ISyncImportBulkArtifactJob SyncImportBulkArtifactJob { get; }
    }
}