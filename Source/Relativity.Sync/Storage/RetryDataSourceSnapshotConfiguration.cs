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
            get => Cache.GetFieldValue(x => x.ImportOverwriteMode);
            set => Cache.UpdateFieldValueAsync(x => x.ImportOverwriteMode, value);
        }
    }
}
