using System;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class JobStatusConsolidationConfiguration : IJobStatusConsolidationConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		public JobStatusConsolidationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
		public int JobHistoryArtifactId => _cache.GetFieldValue(x => x.JobHistoryId);
	}
}