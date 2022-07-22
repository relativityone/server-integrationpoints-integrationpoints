namespace Relativity.Sync.Configuration
{
    internal interface IObjectLinkingSnapshotPartitionConfiguration : ISnapshotPartitionConfiguration
    {
        bool LinkingExportExists { get; }
    }
}