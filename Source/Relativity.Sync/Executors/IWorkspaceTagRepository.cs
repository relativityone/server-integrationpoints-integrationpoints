using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal interface IWorkspaceTagRepository<TIdentifier>
    {
        Task<IList<TagDocumentsResult<TIdentifier>>> TagDocumentsAsync(ISynchronizationConfiguration synchronizationConfiguration, IList<TIdentifier> documentIdentifiers, CancellationToken token);
    }
}
