using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class MetricsConfiguration : IMetricsConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;

        public MetricsConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public string CorrelationId => _cache.GetFieldValue(x => x.CorrelationId);
		
		public string ExecutingApplication => _cache.GetFieldValue(x => x.ExecutingApplication);
		
		public string ExecutingApplicationVersion => _cache.GetFieldValue(x => x.ExecutingApplicationVersion);

		public DataSourceType DataSourceType => _cache.GetFieldValue(x => x.DataSourceType);

		public DestinationLocationType DataDestinationType => _cache.GetFieldValue(x => x.DataDestinationType);

		public bool ImageImport => _cache.GetFieldValue(x => x.ImageImport);

		public int? JobHistoryToRetryId => _cache.GetFieldValue(x => x.JobHistoryToRetryId);

		public string SyncVersion => _syncJobParameters.SyncBuildVersion;
	}
}