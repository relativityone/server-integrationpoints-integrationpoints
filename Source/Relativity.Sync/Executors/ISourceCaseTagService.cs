using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal interface ISourceCaseTagService
    {
        Task<RelativitySourceCaseTag> CreateOrUpdateSourceCaseTagAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, CancellationToken token);
    }
}
