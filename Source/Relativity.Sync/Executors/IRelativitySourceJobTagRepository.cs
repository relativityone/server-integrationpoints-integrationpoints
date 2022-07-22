using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Executors
{
    internal interface IRelativitySourceJobTagRepository
    {
        Task<RelativitySourceJobTag> ReadAsync(int destinationWorkspaceArtifactId, int jobHistoryArtifactId, CancellationToken token);

        Task<RelativitySourceJobTag> CreateAsync(int destinationWorkspaceArtifactId, RelativitySourceJobTag sourceJobTag, CancellationToken token);
    }
}