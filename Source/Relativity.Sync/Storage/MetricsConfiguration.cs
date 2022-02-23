using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class MetricsConfiguration : IMetricsConfiguration
	{
		private readonly ConfigurationBase _cache;
		private readonly SyncJobParameters _syncJobParameters;

        public MetricsConfiguration(ConfigurationBase cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public string CorrelationId => _cache.ConfigurationValue.GetFieldValue(x => x.CorrelationId);
		
		public string ExecutingApplication => _cache.ConfigurationValue.GetFieldValue(x => x.ExecutingApplication);
		
		public string ExecutingApplicationVersion => _cache.ConfigurationValue.GetFieldValue(x => x.ExecutingApplicationVersion);

		public DataSourceType DataSourceType => _cache.ConfigurationValue.GetFieldValue(x => x.DataSourceType);

		public DestinationLocationType DataDestinationType => _cache.ConfigurationValue.GetFieldValue(x => x.DataDestinationType);

		public bool ImageImport => _cache.ConfigurationValue.GetFieldValue(x => x.ImageImport);

		public int? JobHistoryToRetryId => _cache.ConfigurationValue.GetFieldValue(x => x.JobHistoryToRetryId);

		public string SyncVersion => _syncJobParameters.SyncBuildVersion;
	}
}