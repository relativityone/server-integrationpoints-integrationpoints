using kCura.IntegrationPoints.Core.Monitoring.JobLifetimeMessages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Relativity.DataTransfer.MessageService;
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
			LogCount($"IntegrationPoints.Performance.JobStartedCount.{message.Provider}");
		}

		public void OnMessage(JobCompletedMessage message)
		{
			LogCount($"IntegrationPoints.Performance.JobCompletedCount.{message.Provider}");
		}

		public void OnMessage(JobFailedMessage message)
		{
			LogCount($"IntegrationPoints.Performance.JobFailedCount.{message.Provider}");
		}

		public void OnMessage(JobValidationFailedMessage message)
		{
			LogCount($"IntegrationPoints.Performance.JobValidationFailedCount.{message.Provider}");
		}

		public void OnMessage(JobTotalRecordsCountMessage message)
		{
			LogLong($"IntegrationPoints.Usage.TotalRecords.{message.Provider}", message.TotalRecordsCount);
		}

		public void OnMessage(JobCompletedRecordsCountMessage message)
		{
			LogLong($"IntegrationPoints.Usage.CompletedRecords.{message.Provider}", message.CompletedRecordsCount);
		}

		public void OnMessage(JobThroughputMessage message)
		{
			LogDouble($"IntegrationPoints.Performance.Throughput.{message.Provider}", message.RecordsPerSecond);
		}

		public void OnMessage(ExportJobThroughputBytesMessage message)
		{
			LogThroughputBytes(message.Provider, message.BytesPerSecond);
		}

		public void OnMessage(ImportJobThroughputBytesMessage message)
		{
			LogThroughputBytes(message.Provider, message.BytesPerSecond);
		}

		private void LogThroughputBytes(string provider, double throughputBytes)
		{
			LogDouble($"IntegrationPoints.Performance.ThroughputBytes.{provider}", throughputBytes);
		}

		public void OnMessage(ImportJobStatisticsMessage message)
		{
			LogJobSize(message.Provider, message.JobSizeInBytes);
		}

		public void OnMessage(ExportJobStatisticsMessage message)
		{
			LogJobSize(message.Provider, message.JobSizeInBytes);
		}

		private void LogJobSize(string provider, long jobSize)
		{
			LogLong($"IntegrationPoints.Performance.JobSize.{provider}", jobSize);
		}

		private void LogCount(string bucketName)
		{
			_metricsManagerFactory.CreateSUMManager().LogCount(bucketName, 1);
		}

		private void LogLong(string bucketName, long number)
		{
			_metricsManagerFactory.CreateSUMManager().LogLong(bucketName, number);
		}

		private void LogDouble(string bucketName, double number)
		{
			_metricsManagerFactory.CreateSUMManager().LogDouble(bucketName, number);
		}


	}
}