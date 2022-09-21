using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
    internal class BatchDataSourcePreparationConfiguration : IBatchDataSourcePreparationConfiguration
    {
        private readonly IConfiguration _cache;

        public BatchDataSourcePreparationConfiguration(IConfiguration cache)
        {
            _cache = cache;
        }
    }
}
