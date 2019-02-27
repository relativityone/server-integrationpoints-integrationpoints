using System;
using System.Collections.Concurrent;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.Tools;

namespace kCura.IntegrationPoints.Core.Monitoring.MessageSink.Aggregated
{
	public class AggregatedJobSink : IMessageSink<JobStartedMessage>, IMessageSink<JobCompletedMessage>,
		IMessageSink<JobFailedMessage>, IMessageSink<JobValidationFailedMessage>, IMessageSink<JobTotalRecordsCountMessage>,
		IMessageSink<JobCompletedRecordsCountMessage>, IMessageSink<JobThroughputMessage>,
		IMessageSink<JobThroughputBytesMessage>,
		IMessageSink<JobStatisticsMessage>, IMessageSink<JobProgressMessage>
	{
		private const double _TOLERANCE = 0.0000001;

		private readonly IMetricsManagerFactory _metricsManagerFactory;
		private readonly IAPILog _logger;
		private readonly IDateTimeHelper _dateTimeHelper;
		
		private readonly ConcurrentDictionary<string, JobStatistics>
			_jobs = new ConcurrentDictionary<string, JobStatistics>();

		public AggregatedJobSink(IAPILog logger, IMetricsManagerFactory metricsManagerFactory, IDateTimeHelper dateTimeHelper)
		{
			_metricsManagerFactory = metricsManagerFactory;
			_logger = logger.ForContext<AggregatedJobSink>();
			_dateTimeHelper = dateTimeHelper;
		}

		public void OnMessage(JobStartedMessage message)
		{
			if (!_jobs.ContainsKey(message.CorrelationID))
			{
				_metricsManagerFactory.CreateSUMManager().LogCount($"IntegrationPoints.Performance.JobStartedCount.{message.Provider}", 1, message);
				DateTime now = _dateTimeHelper.Now();
				UpdateJobStatistics(
					message,
					statistics =>
					{
						statistics.JobStatus = JobStatus.Started;
						statistics.StartTime = now;
					});
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

			UpdateJobStatistics(message, jobStatistics => jobStatistics.TotalRecordsCount = message.TotalRecordsCount);
		}

		public void OnMessage(JobCompletedRecordsCountMessage message)
		{
			_metricsManagerFactory.CreateSUMManager().LogLong($"IntegrationPoints.Usage.CompletedRecords.{message.Provider}", message.CompletedRecordsCount, message);

			UpdateJobStatistics(message, jobStatistics => jobStatistics.CompletedRecordsCount = message.CompletedRecordsCount);
		}

		public void OnMessage(JobThroughputMessage message)
		{
			_metricsManagerFactory.CreateSUMManager().LogDouble($"IntegrationPoints.Performance.Throughput.{message.Provider}", message.RecordsPerSecond, message);

			UpdateJobStatistics(message, jobStatistics => jobStatistics.RecordsPerSecond = message.RecordsPerSecond);
		}

		public void OnMessage(JobThroughputBytesMessage message)
		{
			UpdateJobStatistics(message, jobStatistics => jobStatistics.BytesPerSecond = message.BytesPerSecond);
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
			JobProgressStatisticsMessage reportedStatistics = new JobProgressStatisticsMessage(message);
			UpdateJobStatistics(message, s => UpdateAverageThroughputs(message, reportedStatistics, s));

			_metricsManagerFactory.CreateAPMManager().LogDouble("IntegrationPoints.Performance.Progress", 1, reportedStatistics);
		}

		// Updates the AverageFileThroughput and AverageMetadataThroughput properties on the given
		// JobStatisitics object with new throughput data from the given JobProgressMessage, then
		// sets those same properties on the given JobProgressStatisticsMessage to report in APM.
		private void UpdateAverageThroughputs(JobProgressMessage message, JobProgressStatisticsMessage reportedStatistics, JobStatistics statistics)
		{
			DateTime now = _dateTimeHelper.Now();
			if (Math.Abs(statistics.AverageFileThroughput - default(double)) < _TOLERANCE)
			{
				statistics.AverageFileThroughput = message.FileThroughput;
			}
			else
			{
				double newThroughput = CalculateAverageThroughput(
					statistics.AverageFileThroughput,
					statistics.LastThroughputCheck - statistics.StartTime,
					message.FileThroughput,
					now - statistics.LastThroughputCheck);

				statistics.AverageFileThroughput = newThroughput;
			}

			if (Math.Abs(statistics.AverageMetadataThroughput - default(double)) < _TOLERANCE)
			{
				statistics.AverageMetadataThroughput = message.MetadataThroughput;
			}
			else
			{
				double newThroughput = CalculateAverageThroughput(
					statistics.AverageMetadataThroughput,
					statistics.LastThroughputCheck - statistics.StartTime,
					message.MetadataThroughput,
					now - statistics.LastThroughputCheck);

				statistics.AverageMetadataThroughput = newThroughput;
			}

			statistics.LastThroughputCheck = now;

			reportedStatistics.AverageFileThroughput = statistics.AverageFileThroughput;
			reportedStatistics.AverageMetadataThroughput = statistics.AverageMetadataThroughput;
		}

		private void OnJobEnd(JobMessageBase message, JobStatus jobStatus)
		{
			if (!IsJobStarted(message.CorrelationID))
			{
				LogMissingJobStartedMetric(message.CorrelationID);
			}

			DateTime now = _dateTimeHelper.Now();
			UpdateJobStatistics(
				message,
				s =>
				{
					s.JobStatus = jobStatus;
					s.EndTime = now;
				});

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

				// Set the floor for job duration at TimeSpan.Zero. This might happen if we don't receive a JobStartedMessage before a job end message.
				TimeSpan calculatedJobDuration = jobStatistics.EndTime - jobStatistics.StartTime;
				TimeSpan reportedJobDuration = calculatedJobDuration <= TimeSpan.Zero ? TimeSpan.Zero : calculatedJobDuration;
				double averageThroughput = Math.Abs(reportedJobDuration.TotalSeconds) > _TOLERANCE ? jobSize / reportedJobDuration.TotalSeconds : 0;

				// OverallThroughputBytes may differ from ThroughputBytes, since the latter value is reported by external services, which may have
				// their own way of calculating throughput and which may "finish" earlier than the job as a whole finishes.
				jobStatistics.OverallThroughputBytes = averageThroughput;
				jobStatistics.DurationSeconds = reportedJobDuration.TotalSeconds;

				// Ugly hack - remove data related to tracking throughput progress metrics. We don't want to report these in our end-of-job metrics.
				jobStatistics.CustomData.Remove(JobStatistics.AVERAGE_FILE_THROUGHPUT_NAME);
				jobStatistics.CustomData.Remove(JobStatistics.AVERAGE_METADATA_THROUGHPUT_NAME);
				jobStatistics.CustomData.Remove(JobStatistics.LAST_THROUGHPUT_CHECK_NAME);

				_metricsManagerFactory.CreateAPMManager().LogDouble("IntegrationPoints.Performance.JobStatistics", jobSize, jobStatistics);
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

		// Given a throughput r1 over a duration t1 and a throughput r2 over a duration t2, returns the average throughput over the
		// duration (t1 + t2). E.g. if you had an average throughput of 8 b/s for 10 seconds and then a throughput of 2 b/s for
		// 2 seconds, this would calculate an overall throughput of 7 b/s.
		private double CalculateAverageThroughput(double throughput1, TimeSpan duration1, double throughput2, TimeSpan duration2)
		{
			double duration1Sec = duration1.TotalSeconds;
			double duration2Sec = duration2.TotalSeconds;
			double updatedRate = (throughput1 * duration1Sec + throughput2 * duration2Sec) / (duration1Sec + duration2Sec);
			return updatedRate;
		}

		#region Logging

		void LogMissingJobStartedMetric(string correlationId)
		{
			_logger.LogWarning($"Job finished, but didn't received job started metric. CorrelationID: {correlationId}");
		}

		#endregion
	}
}
