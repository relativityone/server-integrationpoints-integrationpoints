using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Executors
{
    internal interface ISourceJobTagService
    {
        Task<RelativitySourceJobTag> CreateOrReadSourceJobTagAsync(IDestinationWorkspaceTagsCreationConfiguration configuration, int sourceCaseTagArtifactId, CancellationToken token);
    }
}
