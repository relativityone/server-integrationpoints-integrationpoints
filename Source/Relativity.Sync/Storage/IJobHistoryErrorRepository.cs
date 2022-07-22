using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
    internal interface IJobHistoryErrorRepository
    {
        Task<IEnumerable<int>> MassCreateAsync(int workspaceArtifactId, int jobHistoryArtifactId, IList<CreateJobHistoryErrorDto> createJobHistoryErrorDtos);
        Task<int> CreateAsync(int workspaceArtifactId, int jobHistoryArtifactId, CreateJobHistoryErrorDto createJobHistoryErrorDto);
        Task<IJobHistoryError> GetLastJobErrorAsync(int workspaceArtifactId, int jobHistoryArtifactId);
    }
}