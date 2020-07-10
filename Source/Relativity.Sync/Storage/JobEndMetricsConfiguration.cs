using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using System;

namespace Relativity.Sync.Storage
{
	internal sealed class JobEndMetricsConfiguration : IJobEndMetricsConfiguration
	{
		private readonly SyncJobParameters _syncJobParameters;
		private readonly IConfiguration _cache;

		private static readonly Guid JobHistoryToRetryGuid = new Guid("D7D0DDB9-D383-4578-8D7B-6CBDD9E71549");

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
		public int? JobHistoryToRetryId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryToRetryGuid)?.ArtifactID;

		public JobEndMetricsConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_syncJobParameters = syncJobParameters;
			_cache = cache;
		}
	}
}