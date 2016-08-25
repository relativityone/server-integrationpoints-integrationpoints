using System;
using kCura.IntegrationPoints.Core;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.MetricsCollection;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class SimpleExporter : ExporterEventsWrapper
	{
		private readonly Exporter _exporter;

		public SimpleExporter(Exporter exporter) : base(exporter)
		{
			_exporter = exporter;
		}

		public override IUserNotification InteractionManager
		{
			get { return _exporter.InteractionManager; }
			set { _exporter.InteractionManager = value; }
		}

		public override void Run()
		{
			using (Client.MetricsClient.LogDuration(
				Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_LIB_EXEC_DURATION_METRIC_COLLECTOR,
				Guid.Empty, MetricTargets.SUM))
			{
				_exporter.ExportSearch();
			}
		}
	}
}