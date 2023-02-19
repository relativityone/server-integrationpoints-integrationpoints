using System;
using System.Globalization;
using kCura.IntegrationPoints.Common.Metrics;
using kCura.IntegrationPoints.Common.Monitoring;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Core.Monitoring.MessageSink.Aggregated;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;

namespace kCura.IntegrationPoints.Core.Tests.Monitoring
{
    [TestFixture, Category("Unit")]
    public class AggregatedJobSinkTests
    {
        private IAPILog _logger;
        private IMetricsManagerFactory _metricsManagerFactory;
        private IMetricsManager _sum, _apm;
        private IDateTimeHelper _dateTimeHelper;
        private IRipMetrics _ripMetrics;
        private string _provider = "TestProvider";
        private string _correlationId;
        private AggregatedJobSink _sink;

        // Didn't want to use MinTime/MaxTime in case of under-/overflow, so this is the example time used by Microsoft in its DateTime docs! :P
        private static readonly DateTime _DEFAULT_START_TIME = DateTime.Parse("2009-06-15T13:45:30.0000000", CultureInfo.InvariantCulture);

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<IAPILog>();
            _logger.ForContext<AggregatedJobSink>().Returns(_logger);
            _metricsManagerFactory = Substitute.For<IMetricsManagerFactory>();
            _sum = Substitute.For<IMetricsManager>();
            _metricsManagerFactory.CreateSUMManager().Returns(_sum);
            _apm = Substitute.For<IMetricsManager>();
            _metricsManagerFactory.CreateAPMManager().Returns(_apm);
            _correlationId = Guid.NewGuid().ToString();
            _dateTimeHelper = Substitute.For<IDateTimeHelper>();
            _ripMetrics = Substitute.For<IRipMetrics>();

            _sink = new AggregatedJobSink(_logger, _metricsManagerFactory, _dateTimeHelper, _ripMetrics);
        }

        private IMetricMetadata CreateValidator()
        {
            return Arg.Is<IMetricMetadata>(x => x.CorrelationID.Equals(_correlationId));
        }

        private T CreateMessage<T>(Action<T> update = null) where T : JobMessageBase, new()
        {
            T message = new T();
            message.CorrelationID = _correlationId;
            message.Provider = _provider;
            update?.Invoke(message);
            return message;
        }

        [Test]
        public void ShouldSendJobStartedTest()
        {
            // Arrange
            JobStartedMessage message = CreateMessage<JobStartedMessage>();
            string expectedMetricBucket = $"IntegrationPoints.Performance.JobStartedCount.{_provider}";

            // Act
            _sink.OnMessage(message);

            // Assert
            _sum.Received(1).LogCount(expectedMetricBucket, 1, CreateValidator());
            _ripMetrics.Received(1).PointInTimeLong(expectedMetricBucket, 1, message.CustomData);
        }

        [Test]
        public void ShouldSendJobCompletedTest()
        {
            // Arrange
            JobCompletedMessage message = CreateMessage<JobCompletedMessage>();
            string expectedMetricBucket = $"IntegrationPoints.Performance.JobCompletedCount.{_provider}";

            // Act
            _sink.OnMessage(message);

            // Assert
            _sum.Received(1).LogCount(expectedMetricBucket, 1, CreateValidator());
            _ripMetrics.Received(1).PointInTimeLong(expectedMetricBucket, 1, message.CustomData);
        }

        [Test]
        public void ShouldSendJobFailedTest()
        {
            // Arrange
            JobFailedMessage message = CreateMessage<JobFailedMessage>();
            string expectedMetricBucket = $"IntegrationPoints.Performance.JobFailedCount.{_provider}";

            // Act
            _sink.OnMessage(message);

            // Assert
            _sum.Received(1).LogCount(expectedMetricBucket, 1, CreateValidator());
            _ripMetrics.Received(1).PointInTimeLong(expectedMetricBucket, 1, message.CustomData);
        }

        [Test]
        public void ShouldSendJobValidationFailedTest()
        {
            // Arrange
            JobValidationFailedMessage message = CreateMessage<JobValidationFailedMessage>();
            string expectedMetricBucket = $"IntegrationPoints.Performance.JobValidationFailedCount.{_provider}";

            // Act
            _sink.OnMessage(message);

            // Assert
            _sum.Received(1).LogCount(expectedMetricBucket, 1, CreateValidator());
            _ripMetrics.Received(1).PointInTimeLong(expectedMetricBucket, 1, message.CustomData);
        }

        [Test]
        public void ShouldSendJobSuspendedTest()
        {
            // Arrange
            JobSuspendedMessage message = CreateMessage<JobSuspendedMessage>();
            string expectedMetricBucket = $"IntegrationPoints.Performance.JobSuspendedCount.{_provider}";

            // Act
            _sink.OnMessage(message);

            // Assert
            _sum.Received(1).LogCount(expectedMetricBucket, 1, CreateValidator());
            _ripMetrics.Received(1).PointInTimeLong(expectedMetricBucket, 1, message.CustomData);
        }

        [Test]
        public void ShouldSendTotalRecordsTest()
        {
            // Arrange
            const long totalRecords = 5;
            JobTotalRecordsCountMessage message = CreateMessage<JobTotalRecordsCountMessage>(msg => msg.TotalRecordsCount = totalRecords);
            string expectedMetricBucket = $"IntegrationPoints.Usage.TotalRecords.{_provider}";

            // Act
            _sink.OnMessage(message);

            // Assert
            _sum.Received(1).LogLong(expectedMetricBucket, totalRecords, CreateValidator());
            _ripMetrics.Received(1).PointInTimeLong(expectedMetricBucket, totalRecords, message.CustomData);
        }

        [Test]
        public void ShouldSendCompletedRecordsTest()
        {
            // Arrange
            const long completedRecords = 5;
            JobCompletedRecordsCountMessage message = CreateMessage<JobCompletedRecordsCountMessage>(msg => msg.CompletedRecordsCount = completedRecords);
            string expectedMetricBucket = $"IntegrationPoints.Usage.CompletedRecords.{_provider}";

            // Act
            _sink.OnMessage(message);

            // Assert
            _sum.Received(1).LogLong(expectedMetricBucket, completedRecords, CreateValidator());
            _ripMetrics.Received(1).PointInTimeLong(expectedMetricBucket, completedRecords, message.CustomData);
        }

        [Test]
        public void ShouldSendThroughputTest()
        {
            // Arrange
            const double throughput = 10.8;
            JobThroughputMessage message = CreateMessage<JobThroughputMessage>(msg => msg.RecordsPerSecond = throughput);
            string expectedMetricBucket = $"IntegrationPoints.Performance.Throughput.{_provider}";

            // Act
            _sink.OnMessage(message);

            // Assert
            _sum.Received(1).LogDouble(expectedMetricBucket, throughput, CreateValidator());
            _ripMetrics.Received(1).PointInTimeDouble(expectedMetricBucket, throughput, message.CustomData);
        }

        [Test]
        public void ShouldNotSendJobSizeWhenJobStatisticsReceived()
        {
            _sink.OnMessage(CreateMessage<JobStatisticsMessage>());
            _sink.OnMessage(CreateMessage<JobThroughputBytesMessage>());

            _sum.DidNotReceive().LogLong("IntegrationPoints.Performance.JobSize", Arg.Any<long>(), CreateValidator());
            _sum.DidNotReceive().LogDouble($"IntegrationPoints.Performance.ThroughputBytes.{_provider}", Arg.Any<double>(), CreateValidator());
        }

        [Test]
        public void ShouldSendDifferentSumMessages()
        {
            const long fileBytes = 100;
            const long metaBytes = 50;
            const long completedRecords = 5;
            const double recordsPerSecond = 50.2;

            _sink.OnMessage(CreateMessage<JobStatisticsMessage>(msg =>
            {
                msg.FileBytes = fileBytes;
                msg.MetaBytes = metaBytes;
            }));
            _sink.OnMessage(CreateMessage<JobCompletedRecordsCountMessage>(msg => msg.CompletedRecordsCount = completedRecords));
            _sink.OnMessage(CreateMessage<JobThroughputMessage>(msg => msg.RecordsPerSecond = recordsPerSecond));
            _sink.OnMessage(CreateMessage<JobCompletedMessage>());

            _sum.Received().LogLong($"IntegrationPoints.Performance.JobSize.{_provider}", fileBytes + metaBytes, CreateValidator());
            _sum.Received().LogLong($"IntegrationPoints.Usage.CompletedRecords.{_provider}", completedRecords, CreateValidator());
            _sum.Received().LogDouble($"IntegrationPoints.Performance.Throughput.{_provider}", recordsPerSecond, CreateValidator());
        }

        [Test]
        public void ShouldSendSameSumMessageMultipleTimes()
        {
            const int count = 4;
            double recordsPerSecond = 2;

            for (int i = 0; i < count; i++)
            {
                _sink.OnMessage(CreateMessage<JobThroughputMessage>(msg => msg.RecordsPerSecond = recordsPerSecond));
            }

            _sum.Received(4).LogDouble($"IntegrationPoints.Performance.Throughput.{_provider}", 2, CreateValidator());
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void SameJobStartedMultipleTimes_ShouldSendOnlyOnce(int numberOfJobStartedMessages)
        {
            for (int i = 0; i < numberOfJobStartedMessages; i++)
            {
                _sink.OnMessage(CreateMessage<JobStartedMessage>());
            }
            _sum.Received(1).LogCount($"IntegrationPoints.Performance.JobStartedCount.{_provider}", 1, CreateValidator());
        }

        [Test]
        public void JobCompleted_WithoutStarted_ShouldSendMetricAndLogInfo()
        {
            _sink.OnMessage(CreateMessage<JobCompletedMessage>());

            _sum.Received(1).LogCount($"IntegrationPoints.Performance.JobCompletedCount.{_provider}", 1, CreateValidator());
            _sum.DidNotReceive().LogCount($"IntegrationPoints.Performance.JobStartedCount.{_provider}", 1, CreateValidator());
            _logger.Received(1).LogWarning(Arg.Any<string>());
        }

        [Test]
        public void JobFailed_WithoutStarted_ShouldSendMetricAndLogInfo()
        {
            _sink.OnMessage(CreateMessage<JobFailedMessage>());

            _sum.Received(1).LogCount($"IntegrationPoints.Performance.JobFailedCount.{_provider}", 1, CreateValidator());
            _sum.DidNotReceive().LogCount($"IntegrationPoints.Performance.JobStartedCount.{_provider}", 1, CreateValidator());
            _logger.Received(1).LogWarning(Arg.Any<string>());
        }

        [Test]
        public void JobValidationFailed_WithoutStarted_ShouldSendMetricAndLogInfo()
        {
            _sink.OnMessage(CreateMessage<JobValidationFailedMessage>());

            _sum.Received(1).LogCount($"IntegrationPoints.Performance.JobValidationFailedCount.{_provider}", 1, CreateValidator());
            _sum.DidNotReceive().LogCount($"IntegrationPoints.Performance.JobStartedCount.{_provider}", 1, CreateValidator());
            _logger.Received(1).LogWarning(Arg.Any<string>());
        }

        [Test]
        public void ShouldSendLiveMetric()
        {
            int numberOfMessages = 5;
            for (int i = 0; i < numberOfMessages; i++)
            {
                _sink.OnMessage(CreateMessage<JobProgressMessage>());
            }

            _apm.Received(numberOfMessages).LogDouble("IntegrationPoints.Performance.Progress", 1, CreateValidator());
        }

        [Test]
        public void ShouldSendJobStatisticsAndJobSizeOnJobCompleted()
        {
            _sink.OnMessage(CreateMessage<JobStatisticsMessage>());
            _sink.OnMessage(CreateMessage<JobCompletedMessage>());

            _apm.Received(1).LogDouble($"IntegrationPoints.Performance.JobStatistics", Arg.Any<double>(), CreateValidator());
            _sum.Received(1).LogLong($"IntegrationPoints.Performance.JobSize.{_provider}", Arg.Any<long>(), CreateValidator());
        }

        [Test]
        public void ShouldNotSendJobStatisticsOnJobCompletedWhenNoStatistics()
        {
            _sink.OnMessage(CreateMessage<JobCompletedMessage>());

            _apm.DidNotReceive().LogDouble("IntegrationPoints.Performance.JobStatistics", Arg.Any<double>(), CreateValidator());
            _sum.DidNotReceive().LogLong($"IntegrationPoints.Performance.JobSize.{_provider}", Arg.Any<long>(), CreateValidator());
        }

        [Test]
        public void ShouldSendJobStatisticsOnJobFailed()
        {
            _sink.OnMessage(CreateMessage<JobStatisticsMessage>());
            _sink.OnMessage(CreateMessage<JobFailedMessage>());

            _apm.Received(1).LogDouble("IntegrationPoints.Performance.JobStatistics", Arg.Any<double>(), CreateValidator());
            _sum.Received(1).LogLong($"IntegrationPoints.Performance.JobSize.{_provider}", Arg.Any<long>(), CreateValidator());
        }

        [Test]
        public void ShouldNotSendJobStatisticsOnJobFailedWhenNoStatistics()
        {
            _sink.OnMessage(CreateMessage<JobFailedMessage>());

            _apm.DidNotReceive().LogDouble("IntegrationPoints.Performance.JobStatistics", Arg.Any<double>(), CreateValidator());
            _sum.DidNotReceive().LogLong($"IntegrationPoints.Performance.JobSize.{_provider}", Arg.Any<long>(), CreateValidator());
        }

        [Test]
        public void ShouldNotSendJobStatisticsOnJobValidationFailed()
        {
            _sink.OnMessage(CreateMessage<JobValidationFailedMessage>());

            _apm.DidNotReceive().LogDouble("IntegrationPoints.Performance.JobStatistics", Arg.Any<double>(), CreateValidator());
            _sum.DidNotReceive().LogLong($"IntegrationPoints.Performance.JobSize.{_provider}", Arg.Any<long>(), CreateValidator());
        }

        [Test]
        [TestCase(2560, 2.5, 1024, 2.5)]
        [TestCase(12345, 10, 1234.5, 10)]
        [TestCase(0, 1, 0, 1)]
        [TestCase(2048, -1, 0, 0)]
        [TestCase(2048, 0, 0, 0)]
        [TestCase(0, 0, 0, 0)]
        public void ShouldSendDurationStatisticsWhenJobEnds(long jobSizeBytes, double durationSec, double expectedThroughputBsec, double expectedDurationSec)
        {
            _dateTimeHelper.Now().Returns(_DEFAULT_START_TIME, _DEFAULT_START_TIME + TimeSpan.FromSeconds(durationSec));

            _sink.OnMessage(CreateMessage<JobStartedMessage>());
            _sink.OnMessage(CreateMessage<JobStatisticsMessage>(message =>
            {
                message.FileBytes = jobSizeBytes;
            }
            ));
            _sink.OnMessage(CreateMessage<JobCompletedMessage>());

            _apm.Received(1).LogDouble("IntegrationPoints.Performance.JobStatistics", Arg.Any<double>(), Arg.Is<IMetricMetadata>(x =>
                x.CorrelationID.Equals(_correlationId) &&
                x.CustomData[JobStatistics.OVERALL_THROUGHPUT_BYTES_KEY_NAME].Equals(expectedThroughputBsec) &&
                x.CustomData[JobStatistics.DURATION_SECONDS_KEY_NAME].Equals(expectedDurationSec)
            ));
        }

        [Test]
        public void ShouldSendCorrectDurationStatisticsWhenJobEndsWithoutJobStart()
        {
            const long jobSize = 1024;
            TimeSpan expectedDuration = _DEFAULT_START_TIME - DateTime.MinValue;
            double expectedOverallThroughput = jobSize / expectedDuration.TotalSeconds;

            _dateTimeHelper.Now().Returns(_DEFAULT_START_TIME);

            _sink.OnMessage(CreateMessage<JobStatisticsMessage>(message =>
            {
                message.FileBytes = jobSize;
            }));
            _sink.OnMessage(CreateMessage<JobCompletedMessage>());

            _apm.Received(1).LogDouble("IntegrationPoints.Performance.JobStatistics", Arg.Any<double>(), Arg.Is<IMetricMetadata>(x =>
                x.CorrelationID.Equals(_correlationId) &&
                x.CustomData[JobStatistics.OVERALL_THROUGHPUT_BYTES_KEY_NAME].Equals(expectedOverallThroughput) &&
                x.CustomData[JobStatistics.DURATION_SECONDS_KEY_NAME].Equals(expectedDuration.TotalSeconds)
            ));
        }

        [Test]
        public void ShouldUseCombinedFileMetaJobSizeForOverallThroughput()
        {
            const long fileBytes = 1024;
            const long metadataBytes = 1024;
            const double expectedDurationSec = 1;
            const double expectedOverallThroughput = (fileBytes + metadataBytes) / expectedDurationSec;

            TimeSpan duration = TimeSpan.FromSeconds(expectedDurationSec);
            _dateTimeHelper.Now().Returns(_DEFAULT_START_TIME, _DEFAULT_START_TIME + duration);

            _sink.OnMessage(CreateMessage<JobStartedMessage>());
            _sink.OnMessage(CreateMessage<JobStatisticsMessage>(message =>
            {
                message.FileBytes = fileBytes;
                message.MetaBytes = metadataBytes;
            }));
            _sink.OnMessage(CreateMessage<JobCompletedMessage>());

            _apm.Received(1).LogDouble("IntegrationPoints.Performance.JobStatistics", Arg.Any<double>(), Arg.Is<IMetricMetadata>(x =>
                x.CorrelationID.Equals(_correlationId) &&
                x.CustomData[JobStatistics.OVERALL_THROUGHPUT_BYTES_KEY_NAME].Equals(expectedOverallThroughput) &&
                x.CustomData[JobStatistics.DURATION_SECONDS_KEY_NAME].Equals(expectedDurationSec)
            ));
        }

        [Test]
        public void ShouldNotStompOverExistingThroughput()
        {
            const long fileBytes = 1024;
            const long metadataBytes = 1024;
            const double expectedDurationSec = 1;
            const double expectedThroughput = 12345; // calculated throughput would be 2048 B / 1 s = 2048 B/s
            const double expectedOverallThoughput = (fileBytes + metadataBytes) / expectedDurationSec;

            TimeSpan duration = TimeSpan.FromSeconds(expectedDurationSec);
            _dateTimeHelper.Now().Returns(_DEFAULT_START_TIME, _DEFAULT_START_TIME + duration);

            _sink.OnMessage(CreateMessage<JobStartedMessage>());
            _sink.OnMessage(CreateMessage<JobStatisticsMessage>(message =>
            {
                message.FileBytes = fileBytes;
                message.MetaBytes = metadataBytes;
            }));
            _sink.OnMessage(CreateMessage<JobThroughputBytesMessage>(message =>
            {
                message.BytesPerSecond = expectedThroughput;
            }));
            _sink.OnMessage(CreateMessage<JobCompletedMessage>());

            _apm.Received(1).LogDouble("IntegrationPoints.Performance.JobStatistics", Arg.Any<double>(), Arg.Is<IMetricMetadata>(x =>
                x.CorrelationID.Equals(_correlationId) &&
                x.CustomData[JobStatistics.THROUGHPUT_BYTES_KEY_NAME].Equals(expectedThroughput) &&
                x.CustomData[JobStatistics.OVERALL_THROUGHPUT_BYTES_KEY_NAME].Equals(expectedOverallThoughput) &&
                x.CustomData[JobStatistics.DURATION_SECONDS_KEY_NAME].Equals(expectedDurationSec)
            ));
        }

        [Test]
        public void ShouldSendUpdatedProgressMessages()
        {
            const double progressDurationSec1 = 10;
            const double fileThroughput1 = 50;
            const double metadataThroughput1 = 20;
            const double expectedAverageFileThroughput1 = 50;
            const double expectedAverageMetadataThroughput1 = 20;

            const double progressDurationSec2 = 15;
            const double fileThroughput2 = 0;
            const double metadataThroughput2 = 20;
            const double expectedAverageFileThroughput2 = 20;
            const double expectedAverageMetadataThroughput2 = 20;

            const double progressDurationSec3 = 5;
            const double fileThroughput3 = 20;
            const double metadataThroughput3 = 80;
            const double expectedAverageFileThroughput3 = 20;
            const double expectedAverageMetadataThroughput3 = 30;

            TimeSpan progressDuration1 = TimeSpan.FromSeconds(progressDurationSec1);
            TimeSpan progressDuration2 = TimeSpan.FromSeconds(progressDurationSec2);
            TimeSpan progressDuration3 = TimeSpan.FromSeconds(progressDurationSec3);
            _dateTimeHelper.Now().Returns(
                _DEFAULT_START_TIME,
                _DEFAULT_START_TIME + progressDuration1,
                _DEFAULT_START_TIME + progressDuration1 + progressDuration2,
                _DEFAULT_START_TIME + progressDuration1 + progressDuration2 + progressDuration3);

            _sink.OnMessage(CreateMessage<JobStartedMessage>());
            _sink.OnMessage(CreateMessage<JobProgressMessage>(message =>
            {
                message.FileThroughput = fileThroughput1;
                message.MetadataThroughput = metadataThroughput1;
            }));
            _sink.OnMessage(CreateMessage<JobProgressMessage>(message =>
            {
                message.FileThroughput = fileThroughput2;
                message.MetadataThroughput = metadataThroughput2;
            }));
            _sink.OnMessage(CreateMessage<JobProgressMessage>(message =>
            {
                message.FileThroughput = fileThroughput3;
                message.MetadataThroughput = metadataThroughput3;
            }));

            _apm.Received(1).LogDouble("IntegrationPoints.Performance.Progress", Arg.Any<double>(), Arg.Is<IMetricMetadata>(x =>
                x.CorrelationID.Equals(_correlationId) &&
                x.CustomData["FileThroughput"].Equals(fileThroughput1) &&
                x.CustomData["MetadataThroughput"].Equals(metadataThroughput1) &&
                x.CustomData["AverageFileThroughput"].Equals(expectedAverageFileThroughput1) &&
                x.CustomData["AverageMetadataThroughput"].Equals(expectedAverageMetadataThroughput1)
            ));
            _apm.Received(1).LogDouble("IntegrationPoints.Performance.Progress", Arg.Any<double>(), Arg.Is<IMetricMetadata>(x =>
                x.CorrelationID.Equals(_correlationId) &&
                x.CustomData["FileThroughput"].Equals(fileThroughput2) &&
                x.CustomData["MetadataThroughput"].Equals(metadataThroughput2) &&
                x.CustomData["AverageFileThroughput"].Equals(expectedAverageFileThroughput2) &&
                x.CustomData["AverageMetadataThroughput"].Equals(expectedAverageMetadataThroughput2)
            ));
            _apm.Received(1).LogDouble("IntegrationPoints.Performance.Progress", Arg.Any<double>(), Arg.Is<IMetricMetadata>(x =>
                x.CorrelationID.Equals(_correlationId) &&
                x.CustomData["FileThroughput"].Equals(fileThroughput3) &&
                x.CustomData["MetadataThroughput"].Equals(metadataThroughput3) &&
                x.CustomData["AverageFileThroughput"].Equals(expectedAverageFileThroughput3) &&
                x.CustomData["AverageMetadataThroughput"].Equals(expectedAverageMetadataThroughput3)
            ));
        }

        [Test]
        public void ShouldNotSendProgressPropertiesOnJobEnd()
        {
            _sink.OnMessage(CreateMessage<JobStartedMessage>());
            _sink.OnMessage(CreateMessage<JobProgressMessage>());
            _sink.OnMessage(CreateMessage<JobStatisticsMessage>());
            _sink.OnMessage(CreateMessage<JobCompletedMessage>());

            _apm.Received(1).LogDouble("IntegrationPoints.Performance.JobStatistics", Arg.Any<double>(), Arg.Is<IMetricMetadata>(x =>
                x.CorrelationID.Equals(_correlationId) &&
                !x.CustomData.ContainsKey(JobStatistics.AVERAGE_FILE_THROUGHPUT_NAME) &&
                !x.CustomData.ContainsKey(JobStatistics.AVERAGE_METADATA_THROUGHPUT_NAME) &&
                !x.CustomData.ContainsKey(JobStatistics.LAST_THROUGHPUT_CHECK_NAME)
            ));
        }

        [Test]
        public void ShouldSendAggregatedJobStatistics()
        {
            const int workspaceId = 1000;

            JobStatisticsMessage jobStatisticsMessage1 = new JobStatisticsMessage()
            {
                CorrelationID = _correlationId,
                Provider = _provider,
                JobID = "job_id",
                FileBytes = 10000,
                MetaBytes = 2345,
                WorkspaceID = workspaceId,
                UnitOfMeasure = UnitsOfMeasureConstants.BYTES
            };
            JobStatisticsMessage jobStatisticsMessage2 = new JobStatisticsMessage()
            {
                CorrelationID = _correlationId,
                Provider = _provider,
                JobID = "job_id",
                FileBytes = 99999,
                MetaBytes = 3456,
                WorkspaceID = workspaceId,
                UnitOfMeasure = UnitsOfMeasureConstants.BYTES
            };

            _sink.OnMessage(jobStatisticsMessage1);
            _sink.OnMessage(jobStatisticsMessage2);

            _sink.OnMessage(CreateMessage<JobCompletedMessage>());

            long expectedFileBytes = jobStatisticsMessage1.FileBytes + jobStatisticsMessage2.FileBytes;
            long expectedMetaBytes = jobStatisticsMessage1.MetaBytes + jobStatisticsMessage2.MetaBytes;
            long expectedJobSizeBytes = expectedFileBytes + expectedMetaBytes;

            _apm.Received(1).LogDouble("IntegrationPoints.Performance.JobStatistics", expectedJobSizeBytes,
                Arg.Is<IMetricMetadata>(x =>
                    x.CorrelationID.Equals(_correlationId) &&
                    x.CustomData[JobStatistics.FILE_BYTES_KEY_NAME].Equals(expectedFileBytes) &&
                    x.CustomData[JobStatistics.METADATA_KEY_NAME].Equals(expectedMetaBytes)
                ));
        }

        [Test]
        public void ShouldSendAggregatedJobStatisticsWithAdditionalSumMetrics()
        {
            const long fileBytes = 2405;
            const long metaBytes = 134;
            const long totalRecords = 10;
            const long completedRecords = 10;
            const double throughput = 5.5;
            const double throughputBytes = 2345.3456;

            JobStatisticsMessage jobStatisticsMessage1 = new JobStatisticsMessage()
            {
                CorrelationID = _correlationId,
                Provider = _provider,
                JobID = "job_id",
                FileBytes = fileBytes,
                MetaBytes = metaBytes,
                WorkspaceID = 9999,
                UnitOfMeasure = UnitsOfMeasureConstants.BYTES
            };

            _sink.OnMessage(jobStatisticsMessage1);
            _sink.OnMessage(CreateMessage<JobTotalRecordsCountMessage>(msg => msg.TotalRecordsCount = totalRecords));
            _sink.OnMessage(CreateMessage<JobCompletedRecordsCountMessage>(msg => msg.CompletedRecordsCount = completedRecords));
            _sink.OnMessage(CreateMessage<JobThroughputMessage>(msg => msg.RecordsPerSecond = throughput));
            _sink.OnMessage(CreateMessage<JobThroughputBytesMessage>(msg => msg.BytesPerSecond = throughputBytes));
            _sink.OnMessage(CreateMessage<JobCompletedMessage>());

            _apm.Received(1).LogDouble("IntegrationPoints.Performance.JobStatistics", fileBytes + metaBytes,
                Arg.Is<IMetricMetadata>(x =>
                    x.CorrelationID.Equals(_correlationId) &&
                    x.CustomData[JobStatistics.FILE_BYTES_KEY_NAME].Equals(fileBytes) &&
                    x.CustomData[JobStatistics.METADATA_KEY_NAME].Equals(metaBytes) &&
                    x.CustomData[JobStatistics.TOTAL_RECORDS_KEY_NAME].Equals(totalRecords) &&
                    x.CustomData[JobStatistics.COMPLETED_RECORDS_KEY_NAME].Equals(completedRecords) &&
                    x.CustomData[JobStatistics.THROUGHPUT_KEY_NAME].Equals(throughput) &&
                    x.CustomData[JobStatistics.THROUGHPUT_BYTES_KEY_NAME].Equals(throughputBytes)
                ));
        }
    }
}
