using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class ExportJobErrorService
	{
		private readonly IScratchTableRepository[] _scratchTable;
		private const int _FLUSH_ERROR_BATCH_SIZE = 1500;
		private List<string> _erroredDocumentIds;

		public ExportJobErrorService(IScratchTableRepository[] scratchTable)
		{
			_scratchTable = scratchTable;
			_erroredDocumentIds = new List<string>();
		}

		public void SubscribeToBatchReporterEvents(object batchReporter)
		{
			if (batchReporter is IBatchReporter)
			{
				((IBatchReporter)batchReporter).OnDocumentError += new RowError(OnRowError);
				((IBatchReporter)batchReporter).OnBatchComplete += new BatchCompleted(OnBatchComplete);
			}
		}

		private void OnRowError(string documentIdentifier, string errorMessage)
		{
			_erroredDocumentIds.Add(documentIdentifier);
			if (_erroredDocumentIds.Count == _FLUSH_ERROR_BATCH_SIZE)
			{
				FlushDocumentLevelErrors();
			}
		}

		private void OnBatchComplete(DateTime start, DateTime end, int total, int errorCount)
		{
			if (_erroredDocumentIds.Count != 0)
			{
				FlushDocumentLevelErrors();
			}	
		}

		private void FlushDocumentLevelErrors()
		{
			foreach (IScratchTableRepository table in _scratchTable)
			{
				table.RemoveErrorDocuments(_erroredDocumentIds);
			}
			_erroredDocumentIds.Clear();
		}
	}
}