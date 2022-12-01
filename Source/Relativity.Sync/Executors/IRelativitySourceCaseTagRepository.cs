using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
    internal interface IRelativitySourceCaseTagRepository
    {
        Task<RelativitySourceCaseTag> CreateAsync(int destinationWorkspaceArtifactId, RelativitySourceCaseTag sourceCaseTag);

        Task<RelativitySourceCaseTag> ReadAsync(int destinationWorkspaceArtifactId, int sourceWorkspaceArtifactId, string sourceInstanceName, CancellationToken token);

        Task UpdateAsync(int destinationWorkspaceArtifactId, RelativitySourceCaseTag sourceCaseTag);
    }
}
