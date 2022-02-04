using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class JobHistoryErrorRepositoryRepositoryConfiguration : IJobHistoryErrorRepositoryConfigration
    {
        private readonly IConfiguration _cache;

        public JobHistoryErrorRepositoryRepositoryConfiguration(IConfiguration cache)
        {
            _cache = cache;
        }

        public bool LogItemLevelErrors => _cache.GetFieldValue(x => x.LogItemLevelErrors);
    }
}