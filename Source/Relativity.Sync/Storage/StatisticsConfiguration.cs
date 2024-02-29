using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class StatisticsConfiguration : IStatisticsConfiguration
    {
        private readonly IConfiguration _cache;

        public int SyncStatisticsId => _cache.GetFieldValue(x => x.SyncStatisticsId);

        public int BatchSizeForFileQueries => 10000;

        public StatisticsConfiguration(IConfiguration cache)
        {
            _cache = cache;
        }
    }
}
