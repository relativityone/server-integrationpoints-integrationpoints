using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal sealed class SourceWorkspaceTagsCreationConfiguration : ISourceWorkspaceTagsCreationConfiguration
    {
        private readonly IConfiguration _cache;
        private readonly SyncJobParameters _syncJobParameters;

        public SourceWorkspaceTagsCreationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
        {
            _cache = cache;
            _syncJobParameters = syncJobParameters;
        }

        public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

        public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);

        public int JobHistoryArtifactId => _cache.GetFieldValue(x => x.JobHistoryId);

        public bool IsDestinationWorkspaceTagArtifactIdSet { get; private set; }

        public bool EnableTagging => _cache.GetFieldValue(x => x.EnableTagging);

        public async Task SetDestinationWorkspaceTagArtifactIdAsync(int artifactId)
        {
            await _cache.UpdateFieldValueAsync(x => x.DestinationWorkspaceTagArtifactId, artifactId).ConfigureAwait(false);
            IsDestinationWorkspaceTagArtifactIdSet = true;
        }
    }
}
