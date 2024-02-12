using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.Sync.Storage
{
    internal interface IProgressRepository
    {
        Task<IProgress> CreateAsync(int workspaceArtifactId, int syncConfigurationArtifactId, string name, int order, SyncJobStatus status);

        Task<IProgress> GetAsync(int workspaceArtifactId, int artifactId);

        Task<IReadOnlyCollection<IProgress>> QueryAllAsync(int workspaceArtifactId, int syncConfigurationArtifactId);

        Task<IProgress> QueryAsync(int workspaceArtifactId, int syncConfigurationArtifactId, string name);
    }
}
