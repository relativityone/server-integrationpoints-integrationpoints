using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IImportJob : IDisposable
	{
		Task<ImportJobResult> RunAsync(CancellationToken token);
		Task<IEnumerable<int>> GetPushedDocumentArtifactIds();
		Task<IEnumerable<string>> GetPushedDocumentIdentifiers();
		ISyncImportBulkArtifactJob SyncImportBulkArtifactJob { get; }
	}
}