using System;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Managers;
using kCura.Windows.Process;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.MetricsCollection;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class StoppableExporter : IExporter
	{
		private readonly Controller _controller;
		private readonly WinEDDS.IExporter _exporter;
		private readonly IJobStopManager _jobStopManager;

		public StoppableExporter(WinEDDS.IExporter exporter, Controller controller, IJobStopManager jobStopManager)
		{
			_exporter = exporter;
			_controller = controller;
			_jobStopManager = jobStopManager;
		}

		public IUserNotification InteractionManager
		{
			get { return _exporter.InteractionManager; }
			set { _exporter.InteractionManager = value; }
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

		public bool ExportSearch()
		{
			using (Client.MetricsClient.LogDuration(
				Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_LIB_EXEC_DURATION_METRIC_COLLECTOR,
				Guid.Empty))
			{
				try
				{
					_jobStopManager.StopRequestedEvent += OnStopRequested;

					var exportJobStats = new ExportJobStats
					{
						StartTime = DateTime.UtcNow
					};

					_exporter.ExportSearch();

					exportJobStats.EndTime = DateTime.UtcNow;
					exportJobStats.ExportedItemsCount = _exporter.DocumentsExported;
					DocumentsExported = _exporter.DocumentsExported;

					CompleteExportJob(exportJobStats);
					_jobStopManager.ThrowIfStopRequested();
				}
				finally
				{
					_jobStopManager.StopRequestedEvent -= OnStopRequested;
				}
			}
			return true;
		}

		public int DocumentsExported { get; set; }

		public event BatchCompleted OnBatchCompleted;

		private void OnStopRequested(object sender, EventArgs eventArgs)
		{
			_controller.HaltProcess(Guid.Empty);
		}

		private void CompleteExportJob(ExportJobStats exportJobStats)
		{
			// Error count number is stored in JobStatisticsService class
			OnBatchCompleted?.Invoke(exportJobStats.StartTime, exportJobStats.EndTime,
				exportJobStats.ExportedItemsCount, 0);
		}

		#region Types

		private struct ExportJobStats
		{
			public DateTime StartTime { get; set; }
			public DateTime EndTime { get; set; }
			public int ExportedItemsCount { get; set; }
		}

		#endregion Types
	}
}