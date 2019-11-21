using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IImportJob : IDisposable
	{
		Task<ImportJobResult> RunAsync(CancellationToken token);
		Task<IEnumerable<int>> GetPushedDocumentArtifactIdsAsync();
		Task<IEnumerable<string>> GetPushedDocumentIdentifiersAsync();
		ISyncImportBulkArtifactJob SyncImportBulkArtifactJob { get; }
	}
}