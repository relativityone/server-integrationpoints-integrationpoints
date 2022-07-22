using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal interface ITagSavedSearch
    {
        Task<int> CreateTagSavedSearchAsync(IDestinationWorkspaceSavedSearchCreationConfiguration configuration, int savedSearchFolderArtifactId, CancellationToken token);
    }
}