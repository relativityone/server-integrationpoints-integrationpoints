using System;
using System.Collections.Generic;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace kCura.IntegrationPoints.Core.Telemetry
{
	internal class TelemetryMetricProvider : TelemetryMetricProviderBase
	{
		public TelemetryMetricProvider(IHelper helper) : base(helper)
		{
		}

		public static readonly List<MetricIdentifier> MetricIdentifiers = new List<MetricIdentifier>()
		{
			new MetricIdentifier()
				{   Name = Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINT_REC_SAVE_DURATION_METRIC_COLLECTOR,
					Description = "Length of time (in milliseconds) that Integration Points takes to save Integration Point record"},

			new MetricIdentifier()
				{   Name = Constants.IntegrationPoints.Telemetry.BUCKET_SYNC_WORKER_EXEC_DURATION_METRIC_COLLECTOR,
					Description = "Length of time (in milliseconds) that Integration Points takes to run Sync Worker job"},

			new MetricIdentifier()
				{   Name = Constants.IntegrationPoints.Telemetry.BUCKET_SYNC_MANAGER_EXEC_DURATION_METRIC_COLLECTOR,
					Description = "Length of time (in milliseconds) that Integration Points takes to run Sync Manager job"},

			new MetricIdentifier()
				{	Name = Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_PUSH_KICK_OFF_IMPORT,
				Description = "Length of time (in milliseconds) that RIP Relativity Provider takes to kick off import"},

			new MetricIdentifier()
				{   Name = Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_PUSH_TARGET_DOCUMENTS_TAGGING_IMPORT,
				Description = "Length of time (in milliseconds) that RIP Relativity Provider takes to run documents tagging"},

			new MetricIdentifier()
			{
				Name = Constants.IntegrationPoints.Telemetry.BUCKET_INTEGRATION_POINTS,
				Description = "Integration Points usage and performance metrics"
			}
		};

		protected override List<MetricIdentifier> GetMetricIdentifiers()
		{
			return MetricIdentifiers;
		}

		protected override string ProviderName => "Integration Points Core";
	}
}
