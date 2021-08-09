using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class RetryDataSourceSnapshotConfiguration : DataSourceSnapshotConfiguration, IRetryDataSourceSnapshotConfiguration
	{
		public RetryDataSourceSnapshotConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters) 
			: base(cache, fieldMappings, syncJobParameters)
		{
		}

		public int? JobHistoryToRetryId => Cache.GetFieldValue(x => x.JobHistoryToRetryId);

		public ImportOverwriteMode ImportOverwriteMode
		{
			get => (ImportOverwriteMode)(Enum.Parse(typeof(ImportOverwriteMode), Cache.GetFieldValue<string>(x => x.ImportOverwriteMode)));
			set => Cache.UpdateFieldValueAsync(x => x.ImportOverwriteMode, value.ToString());
		}
	}
}
