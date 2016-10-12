using System.Collections.Generic;
using Relativity.API;
using Relativity.Services.InternalMetricsCollection;

namespace kCura.IntegrationPoints.Core.Telemetry
{
	/// <summary>
	/// This class provides necessary data to register export metrics that will be used by TelemetryManager
	/// </summary>
	/// <remarks>
	/// This class should be part of seperate export provider installers when we be ready to merge application.xml file
	/// </remarks>
	internal class ExportTelemetryMetricProvider : TelemetryMetricProviderBase
	{
		public ExportTelemetryMetricProvider(IHelper helper) : base(helper)
		{
		}

		public static readonly List<MetricIdentifier> ExportMetricIdentifiers = new List<MetricIdentifier>()
		{
			new MetricIdentifier()
				{	Name = Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_LIB_EXEC_DURATION_METRIC_COLLECTOR,
					Description = "Length of time (in milliseconds) that Integration Points takes to run Export Shared Library"},
		};

		protected override List<MetricIdentifier> GetMetricIdentifiers()
		{
			return ExportMetricIdentifiers;
		}

		protected override string ProviderName => "Export";
	}
}
