using System;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using Relativity.Services.DataContracts.DTOs.MetricsCollection;
using Relativity.Telemetry.MetricsCollection;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
	public class SimpleExporter : ExporterEventsWrapper
	{
		#region Types

		private struct ExportJobStats
		{
			public DateTime StartTime { get; set; }
			public DateTime EndTime { get; set; }
			public int ExportedItemsCount { get; set; }
		}

		#endregion Types

		#region Fields

		private readonly Exporter _exporter;

		#endregion Fields

		#region Constructors

		public SimpleExporter(Exporter exporter) : base(exporter)
		{
			_exporter = exporter;
		}

		#endregion //Constructors

		#region Fields

		public override event BatchCompleted OnBatchCompleted;

		public override IUserNotification InteractionManager
		{
			get { return _exporter.InteractionManager; }
			set { _exporter.InteractionManager = value; }
		}

		#endregion Fields

		#region Methods

		public override void Run()
		{
			using (Client.MetricsClient.LogDuration(
					Constants.IntegrationPoints.Telemetry.BUCKET_EXPORT_LIB_EXEC_DURATION_METRIC_COLLECTOR,
					Guid.Empty, MetricTargets.SUM))
			{
				var exportJobStats = new ExportJobStats
				{
					StartTime = DateTime.UtcNow
				};

				_exporter.ExportSearch();

				exportJobStats.EndTime = DateTime.UtcNow;
				exportJobStats.ExportedItemsCount = _exporter.DocumentsExported;

				CompleteExportJob(exportJobStats);
			}
		}

		private void CompleteExportJob(ExportJobStats exportJobStats)
		{
			// Error count number is stored in JobStatisticsService class
			OnBatchCompleted?.Invoke(exportJobStats.StartTime, exportJobStats.EndTime,
				exportJobStats.ExportedItemsCount, 0);
		}

		#endregion //Methods
	}
}