using kCura.IntegrationPoints.Core.Monitoring.JobLifetimeMessages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;
using Relativity.DataTransfer.MessageService.Tools;
using Relativity.Telemetry.MetricsCollection;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public class SumMetricSink : IMessageSink<JobStartedMessage>, IMessageSink<JobCompletedMessage>,
		IMessageSink<JobFailedMessage>, IMessageSink<JobValidationFailedMessage>, IMessageSink<JobTotalRecordsCountMessage>,
		IMessageSink<JobCompletedRecordsCountMessage>, IMessageSink<JobThroughputMessage>
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
			LogDouble($"IntegrationPoints.Performance.Throughput.{message.Provider}", message.Throughput, message);
		}

		private void LogCount(string bucketName, IMetricMetadata metadata)
		{
			IMetricsManager metricsManager = _metricsManagerFactory.CreateSUMManager();
			metricsManager.LogCount(bucketName, 1, metadata);
		}

		private void LogLong(string bucketName, long number, IMetricMetadata metadata)
		{
			IMetricsManager metricsManager = _metricsManagerFactory.CreateSUMManager();
			metricsManager.LogLong(bucketName, number, metadata);
		}

		private void LogDouble(string bucketName, double number, IMetricMetadata metadata)
		{
			IMetricsManager metricsManager = _metricsManagerFactory.CreateSUMManager();
			metricsManager.LogDouble(bucketName, number, metadata);
		}
	}
}