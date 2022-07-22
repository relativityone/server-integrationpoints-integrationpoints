using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal sealed class DestinationWorkspaceSavedSearchCreationConfiguration : IDestinationWorkspaceSavedSearchCreationConfiguration
    {
        private readonly IConfiguration _cache;

        public DestinationWorkspaceSavedSearchCreationConfiguration(IConfiguration cache)
        {
            _cache = cache;
        }

        public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);

        public int SourceJobTagArtifactId => _cache.GetFieldValue(x => x.SourceJobTagArtifactId);

        public int SourceWorkspaceTagArtifactId => _cache.GetFieldValue(x => x.SourceWorkspaceTagArtifactId);

        public bool CreateSavedSearchForTags => _cache.GetFieldValue(x => x.CreateSavedSearchInDestination);

        public bool IsSavedSearchArtifactIdSet => _cache.GetFieldValue(x => x.SavedSearchInDestinationArtifactId) != 0;

        public string GetSourceJobTagName() => _cache.GetFieldValue(x => x.SourceJobTagName);

        public async Task SetSavedSearchInDestinationArtifactIdAsync(int artifactId)
        {
            await _cache.UpdateFieldValueAsync(x => x.SavedSearchInDestinationArtifactId, artifactId).ConfigureAwait(false);
        }
    }
}