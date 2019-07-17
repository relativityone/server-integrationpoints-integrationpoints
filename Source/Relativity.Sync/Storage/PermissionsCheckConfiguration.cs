using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class PermissionsCheckConfiguration : IPermissionsCheckConfiguration
	{
		private readonly IConfiguration _cache;

		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid DataDestinationArtifactIdGuid = new Guid("0E9D7B8E-4643-41CC-9B07-3A66C98248A1");

		public PermissionsCheckConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;
			IntegrationPointArtifactId = syncJobParameters.IntegrationPointArtifactId;
		}

		public int SourceWorkspaceArtifactId { get; }
		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid);
		public int DestinationFolderArtifactId => _cache.GetFieldValue<int>(DataDestinationArtifactIdGuid);
		public int IntegrationPointArtifactId { get; }
		public int SourceProviderArtifactId => 0;

	}
}