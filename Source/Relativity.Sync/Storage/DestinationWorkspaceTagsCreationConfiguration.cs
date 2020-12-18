using System;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class DestinationWorkspaceTagsCreationConfiguration : IDestinationWorkspaceTagsCreationConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		public DestinationWorkspaceTagsCreationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid);
		public int JobHistoryArtifactId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryGuid).ArtifactID;

		public async Task SetSourceJobTagAsync(int artifactId, string name)
		{
			await _cache.UpdateFieldValueAsync(SyncConfigurationRdo.SourceJobTagArtifactIdGuid, artifactId).ConfigureAwait(false);
			await _cache.UpdateFieldValueAsync(SyncConfigurationRdo.SourceJobTagNameGuid, name).ConfigureAwait(false);
		}

		public async Task SetSourceWorkspaceTagAsync(int artifactId, string name)
		{
			await _cache.UpdateFieldValueAsync(SyncConfigurationRdo.SourceWorkspaceTagArtifactIdGuid, artifactId).ConfigureAwait(false);
			await _cache.UpdateFieldValueAsync(SyncConfigurationRdo.SourceWorkspaceTagNameGuid, name).ConfigureAwait(false);
		}
	}
}