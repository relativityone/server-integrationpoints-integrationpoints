using System.Collections.Generic;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;
using Relativity.Services.Objects;

namespace Relativity.Sync.Telemetry
{
	internal sealed class KeplerTelemetryMetricsProvider : TelemetryMetricsProviderBase
	{
		private readonly MetricIdentifier[] _metricIdentifiers =
		{
			new MetricIdentifier
			{
				Name = $"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{nameof(IObjectManager)}.{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_SUCCESS_SUFFIX}",
				Description = $"The count of retries needed for {nameof(IObjectManager)} Kepler Service to succeed."
			},
			new MetricIdentifier
			{
				Name = $"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{nameof(IObjectManager)}.{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_FAILED_SUFFIX}",
				Description = $"The count of retries despite which {nameof(IObjectManager)} Kepler Service failed."
			},
			new MetricIdentifier
			{
				Name = $"{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_PREFIX}.{nameof(IObjectManager)}.{TelemetryConstants.MetricIdentifiers.KEPLER_SERVICE_INTERCEPTOR_AUTH_REFRESH_SUFFIX}",
				Description = $"The count of auth token refreshes for {nameof(IObjectManager)} Kepler Service."
			}
		};
		public override string CategoryName { get; } = TelemetryConstants.SYNC_TELEMETRY_CATEGORY;

		protected override string ProviderName { get; } = nameof(KeplerTelemetryMetricsProvider);

		public KeplerTelemetryMetricsProvider(IAPILog logger)
			: base(logger)
		{
		}

		protected override IEnumerable<MetricIdentifier> GetMetricIdentifiers()
		{
			return _metricIdentifiers;
		}
	}
}
