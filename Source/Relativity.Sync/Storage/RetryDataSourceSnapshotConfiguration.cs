using System;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class RetryDataSourceSnapshotConfiguration : DataSourceSnapshotConfigurationBase, IRetryDataSourceSnapshotConfiguration
	{
		private static readonly Guid JobHistoryToRetryGuid = new Guid("d7d0ddb9-d383-4578-8d7b-6cbdd9e71549");
		private static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		
		public int? JobHistoryToRetryId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryToRetryGuid)?.ArtifactID;

		public ImportOverwriteMode ImportOverwriteMode
		{
			get => (ImportOverwriteMode) (Enum.Parse(typeof(ImportOverwriteMode), _cache.GetFieldValue<string>(ImportOverwriteModeGuid)));
			set => _cache.UpdateFieldValueAsync(ImportOverwriteModeGuid, value.ToString());
		}
		
		public RetryDataSourceSnapshotConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters)
			: base(cache, fieldMappings, syncJobParameters)
		{
		}
	}
}
