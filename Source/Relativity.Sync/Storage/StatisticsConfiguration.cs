using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal class StatisticsConfiguration : IStatisticsConfiguration
	{
		private readonly IConfiguration _cache;

		public int SyncStatisticsId => _cache.GetFieldValue(x => x.SyncStatisticsIdGuid);

		public StatisticsConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}
	}
}
