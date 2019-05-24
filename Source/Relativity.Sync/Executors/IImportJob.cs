using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
	internal interface IImportJob : IDisposable
	{
		Task<ExecutionResult> RunAsync(CancellationToken token);

		Task<IEnumerable<int>> GetPushedDocumentArtifactIds();
	}
}