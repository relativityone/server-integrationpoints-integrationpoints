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
