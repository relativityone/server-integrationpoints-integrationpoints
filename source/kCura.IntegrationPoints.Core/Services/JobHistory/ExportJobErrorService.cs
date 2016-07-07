using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
	public class ExportJobErrorService
	{
		private readonly IScratchTableRepository[] _scratchTable;
		private readonly IInstanceSettingRepository _instanceSettingRepository;
		private int FLUSH_ERROR_BATCH_SIZE = 1000;
		private List<string> _erroredDocumentIds;
		private static readonly object _lock = new Object();

		public ExportJobErrorService(IScratchTableRepository[] scratchTable, IRepositoryFactory repositoryFactory)
		{
			_scratchTable = scratchTable;
			_instanceSettingRepository = repositoryFactory.GetInstanceSettingRepository();
			_erroredDocumentIds = new List<string>();
		}

		public void SubscribeToBatchReporterEvents(object batchReporter)
		{
			if (batchReporter is IBatchReporter)
			{
				((IBatchReporter)batchReporter).OnDocumentError += new RowError(OnRowError);
				((IBatchReporter)batchReporter).OnBatchComplete += new BatchCompleted(OnBatchComplete);
			}

			SetBatchSize();
		}

		internal void OnRowError(string documentIdentifier, string errorMessage)
		{
			lock (_lock)
			{
				_erroredDocumentIds.Add(documentIdentifier);
				if (_erroredDocumentIds.Count == FLUSH_ERROR_BATCH_SIZE)
				{
					FlushDocumentLevelErrors();
				}
			}
		}

		internal void OnBatchComplete(DateTime start, DateTime end, int total, int errorCount)
		{
			lock (_lock)
			{
				if (_erroredDocumentIds.Count != 0)
				{
					FlushDocumentLevelErrors();
				}
			}
		}

		internal void FlushDocumentLevelErrors()
		{
			foreach (IScratchTableRepository table in _scratchTable)
			{
				table.RemoveErrorDocuments(_erroredDocumentIds);
			}
			_erroredDocumentIds.Clear();
		}

		internal void SetBatchSize()
		{
			string configuredBatchSize = _instanceSettingRepository.GetConfigurationValue(IntegrationPoints.Domain.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
				IntegrationPoints.Domain.Constants.REMOVE_ERROR_BATCH_SIZE_INSTANCE_SETTING_NAME);

			if (String.IsNullOrEmpty(configuredBatchSize))
			{
				return;
			}

			try
			{
				FLUSH_ERROR_BATCH_SIZE = Convert.ToInt32(configuredBatchSize);
			}
			catch
			{
				//suppress invalid casts, default to 1000
			}
		}
	}
}