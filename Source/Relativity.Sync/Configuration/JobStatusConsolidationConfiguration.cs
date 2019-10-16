using System;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.Sync.Configuration
{
	internal class JobStatusConsolidationConfiguration : IJobStatusConsolidationConfiguration
	{
		private readonly Storage.IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid SourceWorkspaceTagArtifactIdGuid = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");

		public JobStatusConsolidationConfiguration(Storage.IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public int SourceWorkspaceArtifactId => _cache.GetFieldValue<int>(SourceWorkspaceTagArtifactIdGuid);
		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
		public int JobHistoryArtifactId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryGuid).ArtifactID;
	}
}