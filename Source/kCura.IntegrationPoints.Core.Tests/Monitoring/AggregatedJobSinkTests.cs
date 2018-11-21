﻿using System;
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
	[TestFixture]
	public class AggregatedJobSinkTests
	{
		private IAPILog _logger;
		private IMetricsManagerFactory _metricsManagerFactory;
		private IMetricsManager _sum, _apm;

		private string _provider = "TestProvider";
		private string _correlationId;
		private AggregatedJobSink _sink;

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

			_sink = new AggregatedJobSink(_logger, _metricsManagerFactory);
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
			_sink.OnMessage(CreateMessage<JobStartedMessage>());
			_sum.Received(1).LogCount($"IntegrationPoints.Performance.JobStartedCount.{_provider}", 1, CreateValidator());
		}

		[Test]
		public void ShouldSendJobCompletedTest()
		{
			_sink.OnMessage(CreateMessage<JobCompletedMessage>());
			_sum.Received(1).LogCount($"IntegrationPoints.Performance.JobCompletedCount.{_provider}", 1, CreateValidator());
		}

		[Test]
		public void ShouldSendJobFailedTest()
		{
			_sink.OnMessage(CreateMessage<JobFailedMessage>());
			_sum.Received(1).LogCount($"IntegrationPoints.Performance.JobFailedCount.{_provider}", 1, CreateValidator());
		}

		[Test]
		public void ShouldSendJobValidationFailedTest()
		{
			_sink.OnMessage(CreateMessage<JobValidationFailedMessage>());
			_sum.Received(1).LogCount($"IntegrationPoints.Performance.JobValidationFailedCount.{_provider}", 1, CreateValidator());
		}

		[Test]
		public void ShouldSendTotalRecordsTest()
		{
			const long totalRecords = 5;
			_sink.OnMessage(CreateMessage<JobTotalRecordsCountMessage>(msg => msg.TotalRecordsCount = totalRecords));
			_sum.Received(1).LogLong($"IntegrationPoints.Usage.TotalRecords.{_provider}", totalRecords, CreateValidator());
		}

		[Test]
		public void ShouldSendCompletedRecordsTest()
		{
			const long completedRecords = 5;
			_sink.OnMessage(CreateMessage<JobCompletedRecordsCountMessage>(msg => msg.CompletedRecordsCount = completedRecords));
			_sum.Received(1).LogLong($"IntegrationPoints.Usage.CompletedRecords.{_provider}", completedRecords, CreateValidator());
		}

		[Test]
		public void ShouldSendThroughputTest()
		{
			const double throughput = 10.8;
			_sink.OnMessage(CreateMessage<JobThroughputMessage>(msg => msg.RecordsPerSecond = throughput));
			_sum.Received(1).LogDouble($"IntegrationPoints.Performance.Throughput.{_provider}", throughput, CreateValidator());
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
