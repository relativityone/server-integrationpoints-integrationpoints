using kCura.IntegrationPoints.Core.Monitoring.JobLifetimeMessages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;
using Relativity.DataTransfer.MessageService.Tools;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public class SumMetricSink : IMessageSink<JobStartedMessage>, IMessageSink<JobCompletedMessage>,
		IMessageSink<JobFailedMessage>, IMessageSink<JobValidationFailedMessage>, IMessageSink<JobTotalRecordsCountMessage>,
		IMessageSink<JobCompletedRecordsCountMessage>, IMessageSink<JobThroughputMessage>, IMessageSink<ExportJobThroughputBytesMessage>, IMessageSink<ImportJobThroughputBytesMessage>,
		IMessageSink<ImportJobStatisticsMessage>, IMessageSink<ExportJobStatisticsMessage>
	{
		private readonly IMetricsManagerFactory _metricsManagerFactory;

		public SumMetricSink(IMetricsManagerFactory metricsManagerFactory)
		{
			_metricsManagerFactory = metricsManagerFactory;
		}

		public void OnMessage(JobStartedMessage message)
		{
			LogCount($"IntegrationPoints.Performance.JobStartedCount.{message.Provider}", message);
		}

		public void OnMessage(JobCompletedMessage message)
		{
			LogCount($"IntegrationPoints.Performance.JobCompletedCount.{message.Provider}", message);
		}

		public void OnMessage(JobFailedMessage message)
		{
			LogCount($"IntegrationPoints.Performance.JobFailedCount.{message.Provider}", message);
		}

		public void OnMessage(JobValidationFailedMessage message)
		{
			LogCount($"IntegrationPoints.Performance.JobValidationFailedCount.{message.Provider}", message);
		}

		public void OnMessage(JobTotalRecordsCountMessage message)
		{
			LogLong($"IntegrationPoints.Usage.TotalRecords.{message.Provider}", message.TotalRecordsCount, message);
		}

		public void OnMessage(JobCompletedRecordsCountMessage message)
		{
			LogLong($"IntegrationPoints.Usage.CompletedRecords.{message.Provider}", message.CompletedRecordsCount, message);
		}

		public void OnMessage(JobThroughputMessage message)
		{
			LogDouble($"IntegrationPoints.Performance.Throughput.{message.Provider}", message.RecordsPerSecond, message);
		}

		public void OnMessage(ExportJobThroughputBytesMessage message)
		{
			LogThroughputBytes(message.Provider, message.BytesPerSecond, message);
		}

		public void OnMessage(ImportJobThroughputBytesMessage message)
		{
			LogThroughputBytes(message.Provider, message.BytesPerSecond, message);
		}

		private void LogThroughputBytes(string provider, double throughputBytes, IMetricMetadata metricMetadata)
		{
			LogDouble($"IntegrationPoints.Performance.ThroughputBytes.{provider}", throughputBytes, metricMetadata);
		}

		public void OnMessage(ImportJobStatisticsMessage message)
		{
			LogJobSize(message.Provider, message.JobSizeInBytes, message);
		}

		public void OnMessage(ExportJobStatisticsMessage message)
		{
			LogJobSize(message.Provider, message.JobSizeInBytes, message);
		}

		private void LogJobSize(string provider, long jobSize, IMetricMetadata message)
		{
			LogLong($"IntegrationPoints.Performance.JobSize.{provider}", jobSize, message);
		}

		private void LogCount(string bucketName, IMetricMetadata message)
		{
			_metricsManagerFactory.CreateSUMManager().LogCount(bucketName, 1, message);
		}

		private void LogLong(string bucketName, long number, IMetricMetadata message)
		{
			_metricsManagerFactory.CreateSUMManager().LogLong(bucketName, number, message);
		}

		private void LogDouble(string bucketName, double number, IMetricMetadata message)
		{
			_metricsManagerFactory.CreateSUMManager().LogDouble(bucketName, number, message);
		}
	}
}
