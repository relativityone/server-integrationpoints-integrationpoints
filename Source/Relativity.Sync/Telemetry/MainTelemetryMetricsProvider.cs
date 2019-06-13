using System.Collections.Generic;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace Relativity.Sync.Telemetry
{
	internal sealed class MainTelemetryMetricsProvider : TelemetryMetricsProviderBase
	{
		private readonly MetricIdentifier[] metricIdentifiers = new[]
		{
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.JOB_START_TYPE,
				Description = ""
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.JOB_END_STATUS,
				Description = ""
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_FIELDS_MAPPED,
				Description = ""
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_FILES_SIZE,
				Description = ""
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_RECORDS_FAILED,
				Description = ""
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TOTAL_REQUESTED,
				Description = ""
			},
			new MetricIdentifier
			{
				Name = TelemetryConstants.MetricIdentifiers.DATA_RECORDS_TRANSFERRED,
				Description = ""
			}
		};

		public MainTelemetryMetricsProvider(IServicesMgr servicesManager, ISyncLog logger) : base(servicesManager, logger)
		{
		}

		protected override string ProviderName { get; } = nameof(MainTelemetryMetricsProvider);

		protected override IEnumerable<MetricIdentifier> GetMetricIdentifiers()
		{
			return metricIdentifiers;
		}
	}
}