using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class MetricsConfiguration : IMetricsConfiguration
	{
		private readonly IConfiguration _cache;

		public MetricsConfiguration(IConfiguration cache)
		{
			_cache = cache;
		}

		public string CorrelationId => _cache.GetFieldValue(x => x.CorrelationId);
	}
}