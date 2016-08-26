using System;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.Windows.Process;
using kCura.WinEDDS;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
	public class StatisticsLoggingMediator : ILoggingMediator, IBatchReporter
	{
		#region Fields

		private int _currentExportedItemChunkCount;
		private const int _EXPORTED_ITEMS_UPDATE_THRESHOLD = 1000;

		#endregion //Fields

		#region Events

		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit { add { } remove { } }
		public event BatchCreated OnBatchCreate { add { } remove { } }
		public event StatusUpdate OnStatusUpdate;
		public event JobError OnJobError { add { } remove { } }
		public event RowError OnDocumentError;

		#endregion //Events

		#region Methods

		public void RegisterEventHandlers(IUserMessageNotification userMessageNotification,
			ICoreExporterStatusNotification exporterStatusNotification)
		{
			exporterStatusNotification.OnBatchCompleted += OnOnBatchCompleteChanged;
			exporterStatusNotification.StatusMessage += OnStatusMessageChanged;
			exporterStatusNotification.StatusMessage += OnDocumentErrorChnaged;
		}

		private void OnDocumentErrorChnaged(ExportEventArgs exportArgs)
		{
			// Here we track document error level
			if (exportArgs.EventType == EventType.Error)
			{
				OnDocumentError?.Invoke(exportArgs.Message, exportArgs.Message);
			}
		}

		private void OnStatusMessageChanged(ExportEventArgs exportArgs)
		{
			// RDC firse many events even exported items count has not been chnaged. We need to filter it out
			if (CanUpdateJobStatus(exportArgs))
			{
				_currentExportedItemChunkCount += exportArgs.DocumentsExported;
				OnStatusUpdate?.Invoke(_EXPORTED_ITEMS_UPDATE_THRESHOLD, 0);
			}
		}

		private bool CanUpdateJobStatus(ExportEventArgs exportArgs)
		{
			return exportArgs.EventType == EventType.Progress 
				&& (exportArgs.DocumentsExported % _EXPORTED_ITEMS_UPDATE_THRESHOLD) == 0
				&& exportArgs.DocumentsExported != _currentExportedItemChunkCount;
		}

		private void OnOnBatchCompleteChanged(DateTime startTime, DateTime endTime, int totalRows, int errorRowCount)
		{
			OnBatchComplete?.Invoke(startTime, endTime, totalRows, errorRowCount);
		}

		#endregion //Methods
	}
}
