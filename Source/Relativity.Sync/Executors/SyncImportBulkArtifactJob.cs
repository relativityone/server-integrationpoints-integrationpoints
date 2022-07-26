using System.Collections;
using System.Diagnostics.CodeAnalysis;
using kCura.Relativity.DataReaderClient;
using Relativity.Sync.Transfer;
using Relativity.Sync.Transfer.ImportAPI;

namespace Relativity.Sync.Executors
{
    [ExcludeFromCodeCoverage]
    internal sealed class SyncImportBulkArtifactJob : ISyncImportBulkArtifactJob
    {
        private int _sourceWorkspaceErrorItemsCount = 0;

        private const string _IAPI_DOCUMENT_IDENTIFIER_COLUMN = "Identifier";
        private const string _IAPI_IMAGE_IDENTIFIER_COLUMN = "DocumentID";
        private const string _IAPI_MESSAGE_COLUMN = "Message";

        private readonly IImportBulkArtifactJob _importBulkArtifactJob;

        private SyncImportBulkArtifactJob(ISourceWorkspaceDataReader sourceWorkspaceDataReader)
        {
            ItemStatusMonitor = sourceWorkspaceDataReader.ItemStatusMonitor;
            sourceWorkspaceDataReader.OnItemReadError += HandleSourceWorkspaceDataItemReadError;
        }

        public SyncImportBulkArtifactJob(ImportBulkArtifactJob importBulkArtifactJob, ISourceWorkspaceDataReader sourceWorkspaceDataReader)
            : this(sourceWorkspaceDataReader)
        {
            importBulkArtifactJob.OnProgress += RaiseOnProgress;
            importBulkArtifactJob.OnError += HandleIapiDocumentItemLevelError;
            importBulkArtifactJob.OnComplete += HandleIapiJobComplete;
            importBulkArtifactJob.OnFatalException += HandleIapiFatalException;

            _importBulkArtifactJob = importBulkArtifactJob;
        }

        public SyncImportBulkArtifactJob(ImageImportBulkArtifactJob imageImportBulkArtifactJob, ISourceWorkspaceDataReader sourceWorkspaceDataReader)
            : this(sourceWorkspaceDataReader)
        {
            imageImportBulkArtifactJob.OnProgress += RaiseOnProgress;
            imageImportBulkArtifactJob.OnError += HandleIapiImageItemLevelError;
            imageImportBulkArtifactJob.OnComplete += HandleIapiJobComplete;
            imageImportBulkArtifactJob.OnFatalException += HandleIapiFatalException;

            _importBulkArtifactJob = imageImportBulkArtifactJob;
        }

        public IItemStatusMonitor ItemStatusMonitor { get; }

        public event SyncJobEventHandler<ItemLevelError> OnItemLevelError;
        public event SyncJobEventHandler<ImportApiJobProgress> OnProgress;
        public event SyncJobEventHandler<ImportApiJobStatistics> OnComplete;
        public event SyncJobEventHandler<ImportApiJobStatistics> OnFatalException;

        public void Execute()
        {
            _importBulkArtifactJob.Execute();
        }

        private void RaiseOnProgress(long completedRow)
        {
            OnProgress?.Invoke(new ImportApiJobProgress(completedRow));
        }

        private void HandleIapiDocumentItemLevelError(IDictionary row)
        {
            RaiseOnItemLevelError(
                CreateItemLevelError(row, _IAPI_DOCUMENT_IDENTIFIER_COLUMN, _IAPI_MESSAGE_COLUMN)
            );
        }

        private void HandleIapiImageItemLevelError(IDictionary row)
        {
            RaiseOnItemLevelError(
                CreateItemLevelError(row, _IAPI_IMAGE_IDENTIFIER_COLUMN, _IAPI_MESSAGE_COLUMN)
            );
        }

        private void HandleIapiJobComplete(JobReport jobReport)
        {
            OnComplete?.Invoke(CreateJobStatistics(jobReport));
        }

        private void HandleIapiFatalException(JobReport jobReport)
        {
            OnFatalException?.Invoke(CreateJobStatistics(jobReport));
        }

        private ItemLevelError CreateItemLevelError(IDictionary rawItemLevelError, string identifierColumnName, string messageColumnName)
        {
            return new ItemLevelError(
                GetValueOrNull(rawItemLevelError, identifierColumnName),
                $"IAPI {GetValueOrNull(rawItemLevelError, messageColumnName)}"
            );
        }

        private void RaiseOnItemLevelError(ItemLevelError itemLevelError)
        {
            OnItemLevelError?.Invoke(itemLevelError);
        }

        private void HandleSourceWorkspaceDataItemReadError(long completedItem, ItemLevelError itemLevelError)
        {
            _sourceWorkspaceErrorItemsCount++;

            RaiseOnProgress(completedItem);
            RaiseOnItemLevelError(itemLevelError);
        }

        private ImportApiJobStatistics CreateJobStatistics(JobReport jobReport)
        {
            ImportApiJobStatistics statistics = new ImportApiJobStatistics(
                jobReport.TotalRows + _sourceWorkspaceErrorItemsCount,
                jobReport.ErrorRowCount + _sourceWorkspaceErrorItemsCount,
                jobReport.MetadataBytes,
                jobReport.FileBytes,
                jobReport.FatalException
            );

            return statistics;
        }

        private static string GetValueOrNull(IDictionary row, string key)
        {
            return row.Contains(key) ? row[key].ToString() : null;
        }
    }
}