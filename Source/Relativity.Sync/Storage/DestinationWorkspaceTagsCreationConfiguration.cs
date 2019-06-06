using System;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class DestinationWorkspaceTagsCreationConfiguration : IDestinationWorkspaceTagsCreationConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid SourceJobTagArtifactIdGuid = new Guid("C0A63A29-ABAE-4BF4-A3F4-59E5BD87A33E");
		private static readonly Guid SourceJobTagNameGuid = new Guid("DA0E1931-9460-4A61-9033-A8035697C1A4");
		private static readonly Guid SourceWorkspaceTagArtifactIdGuid = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");
		private static readonly Guid SourceWorkspaceTagNameGuid = new Guid("D828B69E-AAAE-4639-91E2-416E35C163B1");

		public DestinationWorkspaceTagsCreationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid);
		public int JobHistoryArtifactId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryGuid).ArtifactID;
		public bool IsSourceJobTagSet { get; private set; }
		public async Task SetSourceJobTagAsync(int artifactId, string name)
		{
			await _cache.UpdateFieldValueAsync(SourceJobTagArtifactIdGuid, artifactId).ConfigureAwait(false);
			await _cache.UpdateFieldValueAsync(SourceJobTagNameGuid, name).ConfigureAwait(false);
			IsSourceJobTagSet = true;
		}

		public bool IsSourceWorkspaceTagSet { get; private set; }
		public async Task SetSourceWorkspaceTagAsync(int artifactId, string name)
		{
			await _cache.UpdateFieldValueAsync(SourceWorkspaceTagArtifactIdGuid, artifactId).ConfigureAwait(false);
			await _cache.UpdateFieldValueAsync(SourceWorkspaceTagNameGuid, name).ConfigureAwait(false);
			IsSourceWorkspaceTagSet = true;
		}
	}
}