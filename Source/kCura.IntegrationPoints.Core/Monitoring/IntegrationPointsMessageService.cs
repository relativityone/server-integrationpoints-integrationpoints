using System;
using kCura.IntegrationPoints.Core.Monitoring.JobLifetimeMessages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecordsMessages;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public class IntegrationPointsMessageService : MessageService
	{
		protected readonly IMetricsManagerFactory MetricsManagerFactory;

		public IntegrationPointsMessageService(IMetricsManagerFactory metricsManagerFactory)
		{
			MetricsManagerFactory = metricsManagerFactory;

			Subscribe<JobStartedMessage>(OnJobStartedMessage);
			Subscribe<JobCompletedMessage>(OnJobCompletedMessage);
			Subscribe<JobFailedMessage>(OnJobFailedMessage);
			Subscribe<JobValidationFailedMessage>(OnJobValidationFailedMessage);
			Subscribe<JobTotalRecordsCountMessage>(OnTotalRecordsMessage);
			Subscribe<JobCompletedRecordsCountMessage>(OnCompletedRecordsMessage);
			Subscribe<JobThroughputMessage>(OnJobThroughputMessage);
			Subscribe<JobApmThroughputMessage>(OnJobApmThroughputMessage);
		}

		private void OnJobStartedMessage(JobStartedMessage message)
		{
			IMetricsManager metricsManager = MetricsManagerFactory.CreateSUMManager();
			metricsManager.LogCount($"IntegrationPoints.Performance.JobStartedCount.{message.Provider}", 1);
		}

		private void OnJobCompletedMessage(JobCompletedMessage message)
		{
			IMetricsManager metricsManager = MetricsManagerFactory.CreateSUMManager();
			metricsManager.LogCount($"IntegrationPoints.Performance.JobCompletedCount.{message.Provider}", 1);
		}

		private void OnJobFailedMessage(JobFailedMessage message)
		{
			IMetricsManager metricsManager = MetricsManagerFactory.CreateSUMManager();
			metricsManager.LogCount($"IntegrationPoints.Performance.JobFailedCount.{message.Provider}", 1);
		}

		private void OnJobValidationFailedMessage(JobValidationFailedMessage message)
		{
			IMetricsManager metricsManager = MetricsManagerFactory.CreateSUMManager();
			metricsManager.LogCount($"IntegrationPoints.Performance.JobValidationFailedCount.{message.Provider}", 1);
		}

		private void OnTotalRecordsMessage(JobTotalRecordsCountMessage message)
		{
			IMetricsManager metricsManager = MetricsManagerFactory.CreateSUMManager();
			metricsManager.LogLong($"IntegrationPoints.Usage.TotalRecords.{message.Provider}", message.TotalRecordsCount);
		}

		private void OnCompletedRecordsMessage(JobCompletedRecordsCountMessage message)
		{
			IMetricsManager metricsManager = MetricsManagerFactory.CreateSUMManager();
			metricsManager.LogLong($"IntegrationPoints.Usage.CompletedRecords.{message.Provider}", message.CompletedRecordsCount);
		}

		private void OnJobThroughputMessage(JobThroughputMessage message)
		{
			IMetricsManager metricsManager = MetricsManagerFactory.CreateSUMManager();
			metricsManager.LogDouble($"IntegrationPoints.Performance.Throughput.{message.Provider}", message.Throughput);
		}

		private void OnJobApmThroughputMessage(JobApmThroughputMessage message)
		{
			IMetricsManager metricsManager = MetricsManagerFactory.CreateAPMManager();
			metricsManager.LogDouble($"IntegrationPoints.Performance.Progress", 1, message);
		}
	}
}