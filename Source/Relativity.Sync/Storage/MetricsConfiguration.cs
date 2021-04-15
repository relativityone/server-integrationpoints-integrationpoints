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
		
		public string ExecutingApplication => _cache.GetFieldValue(x => x.ExecutingApplication);
		
		public string ExecutingApplicationVersion => _cache.GetFieldValue(x => x.ExecutingApplicationVersion);

		public string DataSourceType => _cache.GetFieldValue(x => x.DataSourceType);

		public string DataDestinationType => _cache.GetFieldValue(x => x.DataDestinationType);

		public bool ImageImport => _cache.GetFieldValue(x => x.ImageImport);

		public int? JobHistoryToRetryId => _cache.GetFieldValue(x => x.JobHistoryToRetryId);
	}
}