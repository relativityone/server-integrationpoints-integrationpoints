using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
	internal interface ISourceWorkspaceTagRepository
	{
		Task<IList<TagDocumentsResult>> TagDocumentsAsync(ISynchronizationConfiguration synchronizationConfiguration, IList<string> documentIdentifiers, CancellationToken token);
	}
}