using System;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class JobStatusConsolidationConfiguration : IJobStatusConsolidationConfiguration
	{
		private readonly Storage.IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;

		private static readonly Guid _JOB_HISTORY_GUID = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid _SOURCE_WORKSPACE_TAG_ARTIFACT_ID_GUID = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");

		public JobStatusConsolidationConfiguration(Storage.IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public int SourceWorkspaceArtifactId => _cache.GetFieldValue<int>(_SOURCE_WORKSPACE_TAG_ARTIFACT_ID_GUID);
		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
		public int JobHistoryArtifactId => _cache.GetFieldValue<RelativityObjectValue>(_JOB_HISTORY_GUID).ArtifactID;
	}
}