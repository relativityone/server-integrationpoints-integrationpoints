using System;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.WinEDDS;
using kCura.WinEDDS.Exporters;
using Relativity.DataExchange.Process;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary
{
    public class StoppableExporter : IExporter
    {
        private readonly ProcessContext _context;
        private readonly WinEDDS.IExporter _exporter;
        private readonly IJobStopManager _jobStopManager;

        public StoppableExporter(WinEDDS.IExporter exporter, ProcessContext context, IJobStopManager jobStopManager)
        {
            _exporter = exporter;
            _context = context;
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
        
        public event IExporterStatusNotification.FileTransferMultiClientModeChangeEventEventHandler FileTransferMultiClientModeChangeEvent

        {
            add { _exporter.FileTransferMultiClientModeChangeEvent += value; }
            remove { _exporter.FileTransferMultiClientModeChangeEvent -= value; }
        }

        public bool ExportSearch()
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

            return true;
        }

        public int DocumentsExported { get; set; }

        public event BatchCompleted OnBatchCompleted;

        private void OnStopRequested(object sender, EventArgs eventArgs)
        {
            _context.PublishCancellationRequest(Guid.Empty);
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