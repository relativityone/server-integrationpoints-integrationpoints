using System.Collections.Generic;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace Relativity.Sync
{
	internal class DefaultTelemetryMetricsProvider : TelemetryMetricsProviderBase
	{
		public static readonly List<MetricIdentifier> MetricIdentifiers = new List<MetricIdentifier>()
		{
			new MetricIdentifier()
			{
				Name = BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR,
				Description = "Length of time (in milliseconds) that Integration Points takes to save Integration Point record"
			},

			new MetricIdentifier()
			{
				Name = BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR,
				Description = "Length of time (in milliseconds) that Integration Points takes to run Sync Worker job"
			},

			new MetricIdentifier()
			{
				Name = BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR,
				Description = "Length of time (in milliseconds) that Integration Points takes to run Sync Manager job"
			},

			new MetricIdentifier()
			{
				Name = BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR,
				Description = "Length of time (in milliseconds) that RIP Relativity Provider takes to kick off import"

			},

			new MetricIdentifier()
			{
				Name = BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR,
				Description = "Length of time (in milliseconds) that RIP Relativity Provider takes to run documents tagging"

			},

			new MetricIdentifier()
			{
				Name = BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR,
				Description = "Integration Points usage and performance metrics"
			}
		};

		public static string BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR { get; } = "ABC";

		public DefaultTelemetryMetricsProvider(IServicesMgr servicesManager, ISyncLog logger) : base(servicesManager, logger)
		{
		}

		protected override List<MetricIdentifier> GetMetricIdentifiers()
		{
			return MetricIdentifiers;
		}
	}
}