using System;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal sealed class DestinationWorkspaceTagsCreationConfiguration : IDestinationWorkspaceTagsCreationConfiguration
    {
        private readonly IConfiguration _cache;
        private readonly SyncJobParameters _syncJobParameters;
        
        public DestinationWorkspaceTagsCreationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
        {
            _cache = cache;
            _syncJobParameters = syncJobParameters;
        }

        public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
        public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);
        public Guid JobHistoryObjectTypeGuid => _cache.GetFieldValue(x => x.JobHistoryType);
        public int JobHistoryArtifactId => _cache.GetFieldValue(x => x.JobHistoryId);

        public async Task SetSourceJobTagAsync(int artifactId, string name)
        {
            await _cache.UpdateFieldValueAsync(x => x.SourceJobTagArtifactId, artifactId).ConfigureAwait(false);
            await _cache.UpdateFieldValueAsync(x => x.SourceJobTagName, name).ConfigureAwait(false);
        }

        public async Task SetSourceWorkspaceTagAsync(int artifactId, string name)
        {
            await _cache.UpdateFieldValueAsync(x => x.SourceWorkspaceTagArtifactId, artifactId).ConfigureAwait(false);
            await _cache.UpdateFieldValueAsync(x => x.SourceWorkspaceTagName, name).ConfigureAwait(false);
        }
    }
}