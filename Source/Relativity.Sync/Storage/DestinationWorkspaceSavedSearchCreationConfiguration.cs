using System;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class DestinationWorkspaceSavedSearchCreationConfiguration : IDestinationWorkspaceSavedSearchCreationConfiguration
	{
		private readonly IConfigurationCache _cache;

		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid SourceJobTagNameGuid = new Guid("DA0E1931-9460-4A61-9033-A8035697C1A4");
		private static readonly Guid SourceJobTagArtifactIdGuid = new Guid("C0A63A29-ABAE-4BF4-A3F4-59E5BD87A33E");
		private static readonly Guid SourceWorkspaceTagNameGuid = new Guid("D828B69E-AAAE-4639-91E2-416E35C163B1");
		private static readonly Guid SourceWorkspaceTagArtifactIdGuid = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");
		private static readonly Guid CreateSavedSearchInDestinationGuid = new Guid("BFAB4AF6-4704-4A12-A8CA-C96A1FBCB77D");
		private static readonly Guid SavedSearchInDestinationArtifactIdGuid = new Guid("83F4DD7A-2231-4C54-BAAA-D1D5B0FE6E31");

		public DestinationWorkspaceSavedSearchCreationConfiguration(IConfigurationCache cache)
		{
			_cache = cache;
		}

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid);
		public string SourceJobTagName => _cache.GetFieldValue<string>(SourceJobTagNameGuid);
		public int SourceJobTagArtifactId => _cache.GetFieldValue<int>(SourceJobTagArtifactIdGuid);
		public string SourceWorkspaceTagName => _cache.GetFieldValue<string>(SourceWorkspaceTagNameGuid);
		public int SourceWorkspaceTagArtifactId => _cache.GetFieldValue<int>(SourceWorkspaceTagArtifactIdGuid);
		public bool CreateSavedSearchForTags => _cache.GetFieldValue<bool>(CreateSavedSearchInDestinationGuid);
		public bool IsSavedSearchArtifactIdSet => _cache.GetFieldValue<int>(SavedSearchInDestinationArtifactIdGuid) != 0;

		public async Task SetSavedSearchInDestinationArtifactIdAsync(int artifactId)
		{
			await _cache.UpdateFieldValueAsync(SavedSearchInDestinationArtifactIdGuid, artifactId).ConfigureAwait(false);
		}
	}
}