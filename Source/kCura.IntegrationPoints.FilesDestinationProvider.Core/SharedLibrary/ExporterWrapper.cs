using System;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.MetricsCollection;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class ExporterWrapper : IExporter
	{
		private readonly Exporter _exporter;

		public ExporterWrapper(Exporter exporter)
		{
			_exporter = exporter;
		}

		public event IExporterStatusNotification.FatalErrorEventEventHandler FatalErrorEvent
		{
			add { _exporter.FatalErrorEvent += value; }
			remove { _exporter.FatalErrorEvent -= value; }
		}

		public event IExporterStatusNotification.StatusMessageEventHandler StatusMessage

		{
			add { _exporter.StatusMessage += value; }
			remove { _exporter.StatusMessage -= value; }
		}

		public event IExporterStatusNotification.FileTransferModeChangeEventEventHandler FileTransferModeChangeEvent

		{
			add { _exporter.FileTransferModeChangeEvent += value; }
			remove { _exporter.FileTransferModeChangeEvent -= value; }
		}

		public IUserNotification InteractionManager
		{
			get { return _exporter.InteractionManager; }
			set { _exporter.InteractionManager = value; }
		}

		public bool Run()
		{
			using (Client.MetricsClient.LogDuration(
					Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_LIB_EXEC_DURATION_METRIC_COLLECTOR,
					Guid.Empty, MetricTargets.SUM))
			{
				DateTime start = DateTime.UtcNow;
				var successResult = _exporter.ExportSearch();
				DateTime end = DateTime.UtcNow;

				OnBatchCompleted?.Invoke(start, end, _exporter.TotalExportArtifactCount, 10);

				return successResult;
			}
		}

		public event BatchCompleted OnBatchCompleted;
	}
}