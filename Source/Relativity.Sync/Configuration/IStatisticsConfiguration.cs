namespace Relativity.Sync.Configuration
{
    internal interface IStatisticsConfiguration : IConfiguration
    {
        int SyncStatisticsId { get; }

        int BatchSizeForFileQueries { get; }
    }
}
