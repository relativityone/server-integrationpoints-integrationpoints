using System;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public class IntegrationPointsMessageService : MessageService
	{
		private readonly IMetricsManagerFactory _metricsManagerFactory;

		public IntegrationPointsMessageService(IMetricsManagerFactory metricsManagerFactory)
		{
			_metricsManagerFactory = metricsManagerFactory;
			Subscribe<JobStartedMessage>(OnJobStartedMessage);
			Subscribe<JobCompletedMessage>(OnJobCompletedMessage);
			Subscribe<JobFailedMessage>(OnJobFailedMessage);
			Subscribe<JobValidationFailedMessage>(OnJobValidationFailedMessage);
		}

		private void OnJobStartedMessage(JobStartedMessage message)
		{
			IMetricsManager metricsManager = _metricsManagerFactory.CreateSUMManager();
			metricsManager.LogCount($"IntegrationPoints.Performance.JobStartedCount.{message.Provider}", 1);
		}

		private void OnJobCompletedMessage(JobCompletedMessage message)
		{
			IMetricsManager metricsManager = _metricsManagerFactory.CreateSUMManager();
			metricsManager.LogCount($"IntegrationPoints.Performance.JobCompletedCount.{message.Provider}", 1);
		}

		private void OnJobFailedMessage(JobFailedMessage message)
		{
			IMetricsManager metricsManager = _metricsManagerFactory.CreateSUMManager();
			metricsManager.LogCount($"IntegrationPoints.Performance.JobFailedCount.{message.Provider}", 1);
		}

		private void OnJobValidationFailedMessage(JobValidationFailedMessage message)
		{
			IMetricsManager metricsManager = _metricsManagerFactory.CreateSUMManager();
			metricsManager.LogCount($"IntegrationPoints.Performance.JobValidationFailedCount.{message.Provider}", 1);
		}
	}
}