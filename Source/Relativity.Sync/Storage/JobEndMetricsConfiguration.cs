using System;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class JobEndMetricsConfiguration : IJobEndMetricsConfiguration
	{
		private readonly SyncJobParameters _syncJobParameters;
		private readonly IConfiguration _cache;

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);
		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
		public int? JobHistoryToRetryId => _cache.GetFieldValue(x => x.JobHistoryToRetryId);

		public ImportOverwriteMode ImportOverwriteMode => (ImportOverwriteMode)(Enum.Parse(typeof(ImportOverwriteMode), _cache.GetFieldValue<string>(SyncConfigurationRdo.ImportOverwriteModeGuid)));
		public DataSourceType DataSourceType => (DataSourceType)(Enum.Parse(typeof(DataSourceType), _cache.GetFieldValue<string>(SyncConfigurationRdo.DataSourceTypeGuid)));
		public DestinationLocationType DestinationType => (DestinationLocationType)(Enum.Parse(typeof(DestinationLocationType), _cache.GetFieldValue<string>(SyncConfigurationRdo.DataDestinationTypeGuid)));
		public ImportNativeFileCopyMode ImportNativeFileCopyMode => _cache.GetFieldValue<string>(SyncConfigurationRdo.NativesBehaviorGuid).GetEnumFromDescription<ImportNativeFileCopyMode>();
		public ImportImageFileCopyMode ImportImageFileCopyMode => _cache.GetFieldValue<string>(SyncConfigurationRdo.ImageFileCopyModeGuid).GetEnumFromDescription<ImportImageFileCopyMode>();

		public JobEndMetricsConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_syncJobParameters = syncJobParameters;
			_cache = cache;
		}
	}
}