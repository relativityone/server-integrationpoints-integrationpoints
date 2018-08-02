using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.Tools;

namespace kCura.IntegrationPoints.Core.Monitoring.Sinks.Aggregated
{
	public class AggregatedJobSink : IMessageSink<JobStartedMessage>, IMessageSink<JobCompletedMessage>,
		IMessageSink<JobFailedMessage>, IMessageSink<JobValidationFailedMessage>, IMessageSink<JobTotalRecordsCountMessage>,
		IMessageSink<JobCompletedRecordsCountMessage>, IMessageSink<JobThroughputMessage>,
		IMessageSink<JobThroughputBytesMessage>,
		IMessageSink<JobStatisticsMessage>, IMessageSink<JobProgressMessage>
	{
		private readonly IMetricsManagerFactory _metricsManagerFactory;
		private readonly IAPILog _logger;

		private readonly ConcurrentDictionary<string, JobStatistics>
			_jobs = new ConcurrentDictionary<string, JobStatistics>();

		public AggregatedJobSink(IHelper helper, IMetricsManagerFactory metricsManagerFactory)
		{
			_metricsManagerFactory = metricsManagerFactory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<AggregatedJobSink>();
		}

		public void OnMessage(JobStartedMessage message)
		{
			if (!_jobs.ContainsKey(message.CorrelationID))
			{
				_metricsManagerFactory.CreateSUMManager().LogCount($"IntegrationPoints.Performance.JobStartedCount.{message.Provider}", 1, message);
				UpdateJobStatistics(message, statistics => { statistics.JobStatus = JobStatus.Started; });
			}
		}

		public void OnMessage(JobCompletedMessage message)
		{
			_metricsManagerFactory.CreateSUMManager().LogCount($"IntegrationPoints.Performance.JobCompletedCount.{message.Provider}", 1, message);
			OnJobEnd(message, JobStatus.Completed);
		}

		public void OnMessage(JobFailedMessage message)
		{
			_metricsManagerFactory.CreateSUMManager().LogCount($"IntegrationPoints.Performance.JobFailedCount.{message.Provider}", 1, message);
			OnJobEnd(message, JobStatus.Failed);
		}

		public void OnMessage(JobValidationFailedMessage message)
		{
			_metricsManagerFactory.CreateSUMManager().LogCount($"IntegrationPoints.Performance.JobValidationFailedCount.{message.Provider}", 1, message);
			OnJobEnd(message, JobStatus.ValidationFailed);
		}

		public void OnMessage(JobTotalRecordsCountMessage message)
		{
			_metricsManagerFactory.CreateSUMManager().LogLong($"IntegrationPoints.Usage.TotalRecords.{message.Provider}", message.TotalRecordsCount, message);

			UpdateJobStatistics(message, jobStatistics => { jobStatistics.TotalRecordsCount = message.TotalRecordsCount; });
		}

		public void OnMessage(JobCompletedRecordsCountMessage message)
		{
			_metricsManagerFactory.CreateSUMManager().LogLong($"IntegrationPoints.Usage.CompletedRecords.{message.Provider}", message.CompletedRecordsCount, message);

			UpdateJobStatistics(message, jobStatistics => { jobStatistics.CompletedRecordsCount = message.CompletedRecordsCount; });
		}

		public void OnMessage(JobThroughputMessage message)
		{
			_metricsManagerFactory.CreateSUMManager().LogDouble($"IntegrationPoints.Performance.Throughput.{message.Provider}", message.RecordsPerSecond, message);

			UpdateJobStatistics(message, jobStatistics => { jobStatistics.RecordsPerSecond = message.RecordsPerSecond; });
		}

		public void OnMessage(JobThroughputBytesMessage message)
		{
			UpdateJobStatistics(message, jobStatistics => { jobStatistics.BytesPerSecond = message.BytesPerSecond; });
		}

		public void OnMessage(JobStatisticsMessage message)
		{
			UpdateJobStatistics(message,
				jobStatistics =>
				{
					jobStatistics.FileBytes += message.FileBytes;
					jobStatistics.MetaBytes += message.MetaBytes;
					jobStatistics.JobSizeInBytes = message.JobSizeInBytes;
					jobStatistics.JobID = message.JobID;
					jobStatistics.WorkspaceID = message.WorkspaceID;
					jobStatistics.UnitOfMeasure = message.UnitOfMeasure;
					jobStatistics.ReceivedJobstatistics = true;
				});
		}

		public void OnMessage(JobProgressMessage message)
		{
			_metricsManagerFactory.CreateAPMManager().LogDouble("IntegrationPoints.Performance.Progress", 1, message);
		}

		private void OnJobEnd(JobMessageBase message, JobStatus jobStatus)
		{
			if (!IsJobStarted(message.CorrelationID))
			{
				LogMissingJobStartedMetric(message.CorrelationID);
			}

			UpdateJobStatistics(message, s => { s.JobStatus = jobStatus; });

			HandleJobEnd(message.CorrelationID);
		}

		private void HandleJobEnd(string correlationId)
		{
			JobStatistics jobStatistics;
			if (_jobs.TryRemove(correlationId, out jobStatistics) && CanSendJobStatistics(jobStatistics))
			{
				long jobSize = jobStatistics.FileBytes + jobStatistics.MetaBytes;
				IMetricsManager sum = _metricsManagerFactory.CreateSUMManager();
				sum.LogLong($"IntegrationPoints.Performance.JobSize.{jobStatistics.Provider}", jobSize, jobStatistics);
				sum.LogDouble($"IntegrationPoints.Performance.ThroughputBytes.{jobStatistics.Provider}", jobStatistics.BytesPerSecond, jobStatistics);
				_metricsManagerFactory.CreateAPMManager().LogDouble($"IntegrationPoints.Performance.JobStatistics", jobSize, jobStatistics);
			}
		}

		private bool CanSendJobStatistics(JobStatistics statistics)
		{
			return statistics.ReceivedJobstatistics && statistics.JobStatus != JobStatus.ValidationFailed;
		}

		private void UpdateJobStatistics(JobMessageBase baseMessage, Action<JobStatistics> updateAction)
		{
			_jobs.AddOrUpdate(
				baseMessage.CorrelationID,
				correlationId =>
				{
					JobStatistics jobStatistics = CreateBaseJobStatistics(baseMessage);
					updateAction(jobStatistics);
					return jobStatistics;
				},
				(correlationId, jobStatistics) =>
				{
					updateAction(jobStatistics);
					return jobStatistics;
				});
		}

		private JobStatistics CreateBaseJobStatistics(JobMessageBase message)
		{
			JobStatistics statistics = new JobStatistics()
			{
				CorrelationID = message.CorrelationID,
				Provider = message.Provider,
				UnitOfMeasure = message.UnitOfMeasure,
				WorkspaceID = message.WorkspaceID,
				JobStatus = JobStatus.New
			};
			return statistics;
		}

		private bool IsJobStarted(string correlationId)
		{
			JobStatistics jobStatistics;
			if (_jobs.TryGetValue(correlationId, out jobStatistics))
			{
				if (jobStatistics.JobStatus == JobStatus.New)
				{
					return false;
				}
			}
			else
			{
				return false;
			}

			return true;
		}

		#region Logging

		void LogMissingJobStartedMetric(string correlationId)
		{
			_logger.LogWarning($"Job finished, but didn't received job started metric. CorrelationID: {correlationId}");
		}

		#endregion
	}
}
