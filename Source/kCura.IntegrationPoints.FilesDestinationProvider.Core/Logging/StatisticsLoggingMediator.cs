using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.Windows.Process;
using kCura.WinEDDS;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging
{
	public class StatisticsLoggingMediator : ILoggingMediator, IBatchReporter
	{
		#region Fields

		private int _currentExportedItemChunkCount;
		private const int _EXPORTED_ITEMS_UPDATE_THRESHOLD = 1000;
		private readonly IMessageService _messageService;
		private readonly IProviderTypeService _providerTypeService;
		private readonly IJobHistoryErrorService _historyErrorService;
		private readonly ICaseServiceContext _caseServiceContext;

		#endregion //Fields

		#region Events

		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit { add { } remove { } }
		public event BatchCreated OnBatchCreate { add { } remove { } }
		public event StatusUpdate OnStatusUpdate;
		public event JobError OnJobError { add { } remove { } }
		public event RowError OnDocumentError;
		public event StatisticsUpdate OnStatisticsUpdate { add { } remove { } }

		#endregion //Events

		#region Methods

		public StatisticsLoggingMediator(IMessageService messageService, IProviderTypeService providerTypeService, IJobHistoryErrorService historyErrorService, ICaseServiceContext caseServiceContext)
		{
			_messageService = messageService;
			_providerTypeService = providerTypeService;
			_historyErrorService = historyErrorService;
			_caseServiceContext = caseServiceContext;
		}

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

			if (CanSendStatistics(exportArgs))
			{
				SendStatistics(exportArgs);
			}
		}

		private void SendStatistics(ExportEventArgs exportArgs)
		{
			string provider = _historyErrorService.IntegrationPoint.GetProviderType(_providerTypeService).ToString();
			var message = new JobApmThroughputMessage()
			{
				Provider = provider,
				CorrelationID = _historyErrorService.JobHistory.BatchInstance,
				UnitOfMeasure = "Byte(s)",
				WorkspaceID = _caseServiceContext.WorkspaceID,
				JobID = _historyErrorService.JobHistory.JobID,
				MetadataThroughput = exportArgs.Statistics.MetadataThroughput,
				FileThroughput = exportArgs.Statistics.FileThroughput,
				CustomData = { ["Provider"] = provider }
			};

			_messageService.Send(message);
		}

		private bool CanUpdateJobStatus(ExportEventArgs exportArgs)
		{
			return exportArgs.EventType == EventType.Progress
				&& (exportArgs.DocumentsExported % _EXPORTED_ITEMS_UPDATE_THRESHOLD) == 0
				&& exportArgs.DocumentsExported != _currentExportedItemChunkCount;
		}

		private bool CanSendStatistics(ExportEventArgs e)
		{
			return e.EventType == EventType.Statistics;
		}

		private void OnOnBatchCompleteChanged(DateTime startTime, DateTime endTime, int totalRows, int errorRowCount)
		{
			OnBatchComplete?.Invoke(startTime, endTime, totalRows, errorRowCount);
		}

		#endregion //Methods
	}
}
