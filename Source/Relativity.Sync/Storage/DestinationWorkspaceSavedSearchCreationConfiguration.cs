using System;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class DestinationWorkspaceSavedSearchCreationConfiguration : IDestinationWorkspaceSavedSearchCreationConfiguration
	{
		private readonly IConfiguration _cache;

		public DestinationWorkspaceSavedSearchCreationConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid);

		public int SourceJobTagArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.SourceJobTagArtifactIdGuid);

		public int SourceWorkspaceTagArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.SourceWorkspaceTagArtifactIdGuid);

		public bool CreateSavedSearchForTags => _cache.GetFieldValue<bool>(SyncConfigurationRdo.CreateSavedSearchInDestinationGuid);

		public bool IsSavedSearchArtifactIdSet => _cache.GetFieldValue<int>(SyncConfigurationRdo.SavedSearchInDestinationArtifactIdGuid) != 0;

		public string GetSourceJobTagName() => _cache.GetFieldValue<string>(SyncConfigurationRdo.SourceJobTagNameGuid);

		public async Task SetSavedSearchInDestinationArtifactIdAsync(int artifactId)
		{
			await _cache.UpdateFieldValueAsync(SyncConfigurationRdo.SavedSearchInDestinationArtifactIdGuid, artifactId).ConfigureAwait(false);
		}
	}
}