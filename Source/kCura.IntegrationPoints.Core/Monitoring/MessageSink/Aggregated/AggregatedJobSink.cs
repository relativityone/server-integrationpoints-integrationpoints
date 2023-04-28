using System;
using System.Collections.Concurrent;
using kCura.IntegrationPoints.Common.Metrics;
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
        IMessageSink<JobStatisticsMessage>, IMessageSink<JobProgressMessage>, IMessageSink<JobSuspendedMessage>
    {
        private const string _INTEGRATION_POINTS_PERFORMANCE_PREFIX = "IntegrationPoints.Performance";
        private const string _INTEGRATION_POINTS_USAGE_PREFIX = "IntegrationPoints.Usage";
        private const double _TOLERANCE = 0.0000001;
        private readonly IMetricsManagerFactory _metricsManagerFactory;
        private readonly IAPILog _logger;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IRipMetrics _ripMetrics;
        private readonly ConcurrentDictionary<string, JobStatistics>
            _jobs = new ConcurrentDictionary<string, JobStatistics>();

        public AggregatedJobSink(IAPILog logger, IMetricsManagerFactory metricsManagerFactory, IDateTimeHelper dateTimeHelper, IRipMetrics ripMetrics)
        {
            _metricsManagerFactory = metricsManagerFactory;
            _logger = logger.ForContext<AggregatedJobSink>();
            _dateTimeHelper = dateTimeHelper;
            _ripMetrics = ripMetrics;
        }

        public void OnMessage(JobStartedMessage message)
        {
            if (!_jobs.ContainsKey(message.CorrelationID))
            {
                string bucket = JobStartedCountMetric(message);
                _metricsManagerFactory.CreateSUMManager().LogCount(bucket, 1, message);
                _ripMetrics.PointInTimeLong(bucket, 1, message.CustomData, message.CorrelationID);

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
            string bucket = JobCompletedCountMetric(message);
            _metricsManagerFactory.CreateSUMManager().LogCount(bucket, 1, message);
            _ripMetrics.PointInTimeLong(bucket, 1, message.CustomData, message.CorrelationID);

            OnJobEnd(message, JobStatus.Completed);
        }

        public void OnMessage(JobFailedMessage message)
        {
            string bucket = JobFailedCountMetric(message);
            _metricsManagerFactory.CreateSUMManager().LogCount(bucket, 1, message);
            _ripMetrics.PointInTimeLong(bucket, 1, message.CustomData, message.CorrelationID);

            OnJobEnd(message, JobStatus.Failed);
        }

        public void OnMessage(JobValidationFailedMessage message)
        {
            string bucket = JobValidationFailedCountMetric(message);
            _metricsManagerFactory.CreateSUMManager().LogCount(bucket, 1, message);
            _ripMetrics.PointInTimeLong(bucket, 1, message.CustomData, message.CorrelationID);

            OnJobEnd(message, JobStatus.ValidationFailed);
        }

        public void OnMessage(JobSuspendedMessage message)
        {
            string bucket = JobSuspendedCountMetric(message);
            _metricsManagerFactory.CreateSUMManager().LogCount(bucket, 1, message);
            _ripMetrics.PointInTimeLong(bucket, 1, message.CustomData, message.CorrelationID);

            OnJobEnd(message, JobStatus.Suspended);
        }

        public void OnMessage(JobTotalRecordsCountMessage message)
        {
            string bucket = TotalRecordsCountMetric(message);
            _metricsManagerFactory.CreateSUMManager().LogLong(bucket, message.TotalRecordsCount, message);
            _ripMetrics.PointInTimeLong(bucket, message.TotalRecordsCount, message.CustomData, message.CorrelationID);

            UpdateJobStatistics(message, jobStatistics => jobStatistics.TotalRecordsCount = message.TotalRecordsCount);
        }

        public void OnMessage(JobCompletedRecordsCountMessage message)
        {
            string bucket = CompletedRecordsCountMetric(message);
            _metricsManagerFactory.CreateSUMManager().LogLong(bucket, message.CompletedRecordsCount, message);
            _ripMetrics.PointInTimeLong(bucket, message.CompletedRecordsCount, message.CustomData, message.CorrelationID);

            UpdateJobStatistics(message, jobStatistics => jobStatistics.CompletedRecordsCount = message.CompletedRecordsCount);
        }

        public void OnMessage(JobThroughputMessage message)
        {
            string bucket = ThroughputMetric(message);
            _metricsManagerFactory.CreateSUMManager().LogDouble(bucket, message.RecordsPerSecond, message);
            _ripMetrics.PointInTimeDouble(bucket, message.RecordsPerSecond, message.CustomData, message.CorrelationID);

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
            if (!statistics.AverageFileThroughput.HasValue)
            {
                statistics.AverageFileThroughput = message.FileThroughput;
            }
            else
            {
                double newThroughput = CalculateAverageThroughput(
                    statistics.AverageFileThroughput.Value,
                    statistics.LastThroughputCheck - statistics.StartTime,
                    message.FileThroughput,
                    now - statistics.LastThroughputCheck);

                statistics.AverageFileThroughput = newThroughput;
            }

            if (!statistics.AverageMetadataThroughput.HasValue)
            {
                statistics.AverageMetadataThroughput = message.MetadataThroughput;
            }
            else
            {
                double newThroughput = CalculateAverageThroughput(
                    statistics.AverageMetadataThroughput.Value,
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

            HandleJobEnd(message);
        }

        private void HandleJobEnd(JobMessageBase message)
        {
            JobStatistics jobStatistics;
            if (_jobs.TryRemove(message.CorrelationID, out jobStatistics) && CanSendJobStatistics(jobStatistics))
            {
                FillInMissingFields(jobStatistics, message);

                long jobSize = jobStatistics.FileBytes + jobStatistics.MetaBytes;
                IMetricsManager sum = _metricsManagerFactory.CreateSUMManager();

                string jobSizeBucket = JobSizeMetric(jobStatistics);
                sum.LogLong(jobSizeBucket, jobSize, jobStatistics);
                _ripMetrics.PointInTimeLong(jobSizeBucket, jobSize, jobStatistics.CustomData, message.CorrelationID);

                string throughputBytesBucket = ThroughputBytesMetric(jobStatistics);
                sum.LogDouble(throughputBytesBucket, jobStatistics.BytesPerSecond, jobStatistics);
                _ripMetrics.PointInTimeDouble(throughputBytesBucket, jobStatistics.BytesPerSecond, jobStatistics.CustomData, message.CorrelationID);

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

        private void FillInMissingFields(JobStatistics jobStatistics, JobMessageBase message)
        {
            jobStatistics.Provider = string.IsNullOrEmpty(jobStatistics.Provider) ? message.Provider : jobStatistics.Provider;
            jobStatistics.CorrelationID = string.IsNullOrEmpty(jobStatistics.CorrelationID) ? message.CorrelationID : jobStatistics.CorrelationID;
            jobStatistics.UnitOfMeasure = string.IsNullOrEmpty(jobStatistics.UnitOfMeasure) ? message.UnitOfMeasure : jobStatistics.UnitOfMeasure;
            jobStatistics.WorkspaceID = jobStatistics.WorkspaceID == 0 ? message.WorkspaceID : jobStatistics.WorkspaceID;
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

        #region Metrics Definitions

        private string JobStartedCountMetric(JobMessageBase message) =>
            $"{_INTEGRATION_POINTS_PERFORMANCE_PREFIX}.JobStartedCount.{message.Provider}";
        private string JobCompletedCountMetric(JobMessageBase message) =>
            $"{_INTEGRATION_POINTS_PERFORMANCE_PREFIX}.JobCompletedCount.{message.Provider}";
        private string JobFailedCountMetric(JobMessageBase message) =>
            $"{_INTEGRATION_POINTS_PERFORMANCE_PREFIX}.JobFailedCount.{message.Provider}";
        private string JobValidationFailedCountMetric(JobMessageBase message) =>
            $"{_INTEGRATION_POINTS_PERFORMANCE_PREFIX}.JobValidationFailedCount.{message.Provider}";
        private string JobSuspendedCountMetric(JobMessageBase message) =>
            $"{_INTEGRATION_POINTS_PERFORMANCE_PREFIX}.JobSuspendedCount.{message.Provider}";
        private string TotalRecordsCountMetric(JobMessageBase message) =>
            $"{_INTEGRATION_POINTS_USAGE_PREFIX}.TotalRecords.{message.Provider}";
        private string CompletedRecordsCountMetric(JobMessageBase message) =>
            $"{_INTEGRATION_POINTS_USAGE_PREFIX}.CompletedRecords.{message.Provider}";
        private string ThroughputMetric(JobMessageBase message) =>
            $"{_INTEGRATION_POINTS_PERFORMANCE_PREFIX}.Throughput.{message.Provider}";
        private string JobSizeMetric(JobMessageBase message) =>
            $"{_INTEGRATION_POINTS_PERFORMANCE_PREFIX}.JobSize.{message.Provider}";
        private string ThroughputBytesMetric(JobMessageBase message) =>
            $"{_INTEGRATION_POINTS_PERFORMANCE_PREFIX}.ThroughputBytes.{message.Provider}";

        #endregion

        #region Logging

        private void LogMissingJobStartedMetric(string correlationId)
        {
            _logger.LogWarning($"Job finished, but didn't received job started metric. CorrelationID: {correlationId}");
        }

        #endregion
    }
}
