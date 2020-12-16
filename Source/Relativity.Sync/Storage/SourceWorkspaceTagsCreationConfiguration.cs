using System;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class SourceWorkspaceTagsCreationConfiguration : ISourceWorkspaceTagsCreationConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid DestinationWorkspaceTagArtifactIdGuid = new Guid("E2100C10-B53B-43FA-BB1B-51E43DCE8208");

		public SourceWorkspaceTagsCreationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid);
		public int JobHistoryArtifactId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryGuid).ArtifactID;
		public bool IsDestinationWorkspaceTagArtifactIdSet { get; private set; }

		public async Task SetDestinationWorkspaceTagArtifactIdAsync(int artifactId)
		{
			await _cache.UpdateFieldValueAsync(DestinationWorkspaceTagArtifactIdGuid, artifactId).ConfigureAwait(false);
			IsDestinationWorkspaceTagArtifactIdSet = true;
		}
	}
}