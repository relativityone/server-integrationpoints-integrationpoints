namespace Relativity.Sync.Configuration
{
    internal interface IRetryDataSourceSnapshotConfiguration : IDataSourceSnapshotConfiguration
    {
        int? JobHistoryToRetryId { get; }

        ImportOverwriteMode ImportOverwriteMode { get; set; }
    }
}
