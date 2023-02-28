using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.Core.Services.JobHistory
{
    public class ExportJobErrorService
    {
        private int? _flushErrorBatchSize = null;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IScratchTableRepository[] _scratchTable;
        private readonly List<string> _erroredDocumentIds;
        private static readonly object _lock = new Object();

        public int FlushErrorBatchSize
        {
            get
            {
                if (!_flushErrorBatchSize.HasValue)
                {
                    _flushErrorBatchSize = RetrieveBatchSizeInstanceSetting();
                }
                return _flushErrorBatchSize.Value;
            }
        }

        public ExportJobErrorService(IScratchTableRepository[] scratchTable, IRepositoryFactory repositoryFactory)
        {
            _scratchTable = scratchTable;
            _repositoryFactory = repositoryFactory;
            _erroredDocumentIds = new List<string>();
        }

        /// <summary>
        /// For unit tests only
        /// </summary>
        internal ExportJobErrorService(IScratchTableRepository[] scratchTable, IRepositoryFactory repositoryFactory, List<string> documentIds)
        {
            _scratchTable = scratchTable;
            _repositoryFactory = repositoryFactory;
            _erroredDocumentIds = documentIds;
        }

        public void SubscribeToBatchReporterEvents(object batchReporter)
        {
            if (batchReporter is IBatchReporter)
            {
                ((IBatchReporter)batchReporter).OnDocumentError += new RowError(OnRowError);
                ((IBatchReporter)batchReporter).OnBatchComplete += new BatchCompleted(OnBatchComplete);
            }

        }

        internal void OnRowError(string documentIdentifier, string errorMessage)
        {
            lock (_lock)
            {
                _erroredDocumentIds.Add(documentIdentifier);
                if (_erroredDocumentIds.Count == FlushErrorBatchSize)
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

        internal int RetrieveBatchSizeInstanceSetting()
        {
            IInstanceSettingRepository instanceSettingRepository = _repositoryFactory.GetInstanceSettingRepository();

            string configuredBatchSize = instanceSettingRepository.GetConfigurationValue(IntegrationPoints.Domain.Constants.INTEGRATION_POINT_INSTANCE_SETTING_SECTION,
                IntegrationPoints.Domain.Constants.REMOVE_ERROR_BATCH_SIZE_INSTANCE_SETTING_NAME);

            int batchSize;
            if (String.IsNullOrEmpty(configuredBatchSize))
            {
                batchSize = 1000;
            }
            else
            {
                batchSize = Convert.ToInt32(configuredBatchSize);
            }

            return batchSize;
        }
    }
}
