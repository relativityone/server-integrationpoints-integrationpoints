using System;

namespace kCura.IntegrationPoints.Core.Contracts.BatchReporter
{
    public delegate void JobError(Exception ex);
    public delegate void RowError(string documentIdentifier, string errorMessage);
    public delegate void BatchCompleted(DateTime startTime, DateTime endTime, int totalRows, int errorRowCount);
    public delegate void BatchSubmitted(int size, int batchSize);
    public delegate void BatchCreated(int batchSize);
    public delegate void StatusUpdate(int importedCount, int errorCount);
    public delegate void StatisticsUpdate(double metadataThroughput, double fileThroughput);

    public interface IBatchReporter
    {
        event BatchCompleted OnBatchComplete;
        event BatchSubmitted OnBatchSubmit;
        event BatchCreated OnBatchCreate;
        event StatusUpdate OnStatusUpdate;
        event StatisticsUpdate OnStatisticsUpdate;
        event JobError OnJobError;
        event RowError OnDocumentError;
    }
}
