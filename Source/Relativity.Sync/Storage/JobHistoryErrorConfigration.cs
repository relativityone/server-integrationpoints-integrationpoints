using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class JobHistoryErrorRepositoryConfiguration : IJobHistoryErrorRepositoryConfigration
    {
        private readonly IConfiguration _cache;

        public JobHistoryErrorRepositoryConfiguration(IConfiguration cache)
        {
            _cache = cache;
        }

        public bool LogItemLevelErrors => _cache.GetFieldValue(x => x.LogItemLevelErrors);
    }
}