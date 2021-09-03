using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Core.Monitoring.JobLifetime;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using NUnit.Framework;
using Relativity.DataTransfer.MessageService;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture, Category("Unit")]
	public class JobLifetimeMetricBatchStatusTests : TestBase
	{
		private Mock<IMessageService> _messageServiceMock;
		private Mock<IJobHistoryService> _jobHistoryServiceFake;
		private Mock<IDateTimeHelper> _dateTimeHelperFake;
		private Mock<IJobStatusUpdater> _jobStatusUpdaterFake;

		private JobLifetimeMetricBatchStatus _sut;

		private const string _EXPECTED_PROVIDER_NAME = "SomeProvider";

		private readonly DateTime _JOB_HISTORY_START_TIME_UTC = new DateTime(2021, 9, 3, 7, 0, 0);

		[SetUp]
		public override void SetUp()
		{
			_messageServiceMock = new Mock<IMessageService>();

			Mock<IIntegrationPointService> integrationPointService = new Mock<IIntegrationPointService>();
			integrationPointService.Setup(x => x.ReadIntegrationPoint(It.IsAny<int>()))
				.Returns(new Data.IntegrationPoint
				{
					SourceProvider = It.IsAny<int>(),
					DestinationProvider = It.IsAny<int>()
				});

			_jobHistoryServiceFake = new Mock<IJobHistoryService>();
			SetupJobHistory();

			_dateTimeHelperFake = new Mock<IDateTimeHelper>();

			Mock<IProviderTypeService> providerTypeService = new Mock<IProviderTypeService>();
			providerTypeService.Setup(x => x.GetProviderName(It.IsAny<int>(), It.IsAny<int>()))
				.Returns(_EXPECTED_PROVIDER_NAME);

			Mock<ISerializer> serializer = new Mock<ISerializer>();
			serializer.Setup(x => x.Deserialize<TaskParameters>(It.IsAny<string>()))
				.Returns(new TaskParameters { BatchInstance = Guid.NewGuid() });

			_jobStatusUpdaterFake = new Mock<IJobStatusUpdater>();

			_sut = new JobLifetimeMetricBatchStatus(_messageServiceMock.Object, integrationPointService.Object,
				providerTypeService.Object, _jobStatusUpdaterFake.Object, _jobHistoryServiceFake.Object, serializer.Object, _dateTimeHelperFake.Object);
		}

		[Test]
		public void OnJobStart_SendJobStartedMessage()
		{
			// Arrange
			Job job = JobExtensions.CreateJob();

			// Act
			_sut.OnJobStart(job);

			// Assert
			_messageServiceMock.Verify(x => x.Send(It.IsAny<JobStartedMessage>()), Times.Never);
		}

		[Test]
		public void OnJobComplete_SendJobFailedMessage()
		{
			// Arrange
			Job job = JobExtensions.CreateJob();

			SetupJobHistoryExpectedStatus(JobStatusChoices.JobHistoryErrorJobFailed);

			// Act
			_sut.OnJobComplete(job);

			// Assert
			_messageServiceMock.Verify(x => x.Send(It.IsAny<JobFailedMessage>()), Times.Once);
		}

		[Test]
		public void OnJobComplete_SendJobValidationFailedMessage()
		{
			// Arrange
			Job job = JobExtensions.CreateJob();

			SetupJobHistoryExpectedStatus(JobStatusChoices.JobHistoryValidationFailed);

			// Act
			_sut.OnJobComplete(job);

			// Assert
			_messageServiceMock.Verify(x => x.Send(It.IsAny<JobValidationFailedMessage>()), Times.Once);
		}

		[Test]
		[TestCaseSource(nameof(JobCompletedStatusChoices))]
		public void OnJobComplete_SendJobCompletedMessage(ChoiceRef status)
		{
			// Arrange
			Job job = JobExtensions.CreateJob();

			SetupJobHistoryExpectedStatus(status);

			// Act
			_sut.OnJobComplete(job);

			// Assert
			_messageServiceMock.Verify(x => x.Send(It.IsAny<JobCompletedMessage>()), Times.Once);
		}

		[TestCase(50, 10)]
		[TestCase(10, 1000)]
		[TestCase(1, 9999)]
		[TestCase(1000, 1)]
		public void OnJobComplete_SendThroughput(int records, double durationInSeconds)
		{
			// Arrange
			Job job = JobExtensions.CreateJob();

			SetupJobHistory(
				endTimeUtc: _JOB_HISTORY_START_TIME_UTC.AddSeconds(durationInSeconds),
				totalItems: records,
				itemsTransferred: records);

			SetupJobHistoryExpectedStatus(JobStatusChoices.JobHistoryCompleted);

			// Act
			_sut.OnJobComplete(job);

			// Assert
			double expectedThroughput = records / durationInSeconds;
			_messageServiceMock.Verify(x => x.Send(It.Is<JobThroughputMessage>(m => m.RecordsPerSecond == expectedThroughput)), Times.Once);
		}

		[Test]
		public void OnJobComplete_NotSendingThroughputWhenZeroRecords()
		{
			// Arrange
			Job job = JobExtensions.CreateJob();

			SetupJobHistory(endTimeUtc: _JOB_HISTORY_START_TIME_UTC.AddSeconds(1));

			SetupJobHistoryExpectedStatus(JobStatusChoices.JobHistoryCompleted);

			// Act
			_sut.OnJobComplete(job);

			// Assert
			_messageServiceMock.Verify(x => x.Send(It.IsAny<JobThroughputMessage>()), Times.Never);
		}

		[Test]
		public void OnJobComplete_NullEndTimeDoesntBreakMetric()
		{
			// Arrange
			const int itemsTransferred = 10;
			const int seconds = 10;

			Job job = JobExtensions.CreateJob();

			SetupJobHistoryExpectedStatus(JobStatusChoices.JobHistoryErrorJobFailed);

			SetupJobHistory(itemsTransferred: itemsTransferred);

			_dateTimeHelperFake.Setup(x => x.Now()).Returns(_JOB_HISTORY_START_TIME_UTC.AddSeconds(seconds));

			// Act
			_sut.OnJobComplete(job);

			// Assert
			double expectedThroughput = (double)itemsTransferred / seconds;
			_messageServiceMock.Verify(x => x.Send(It.Is<JobThroughputMessage>(m => m.RecordsPerSecond == expectedThroughput)), Times.Once);
		}

		[Test]
		public void OnJobComplete_ShouldSendCorrectProviderName()
		{
			// Arrange
			const int itemsTransferred = 10;

			Job job = JobExtensions.CreateJob();

			SetupJobHistoryExpectedStatus(JobStatusChoices.JobHistoryCompleted);

			SetupJobHistory(itemsTransferred: itemsTransferred);

			// Act
			_sut.OnJobComplete(job);

			// Assert
			_messageServiceMock.Verify(x => x.Send(It.Is<JobCompletedMessage>(m => m.Provider == _EXPECTED_PROVIDER_NAME)), Times.Once);
			_messageServiceMock.Verify(x => x.Send(It.Is<JobThroughputMessage>(m => m.Provider == _EXPECTED_PROVIDER_NAME)), Times.Once);
			_messageServiceMock.Verify(x => x.Send(It.Is<JobTotalRecordsCountMessage>(m => m.Provider == _EXPECTED_PROVIDER_NAME)), Times.Once);
			_messageServiceMock.Verify(x => x.Send(It.Is<JobCompletedRecordsCountMessage>(m => m.Provider == _EXPECTED_PROVIDER_NAME)), Times.Once);
		}

		[Test]
		public void OnJobComplete_ShouldNotSendAnyMetrics_WhenJobHasBeenSuspended()
		{
			// Arrange
			Job job = JobExtensions.CreateJob();

			SetupJobHistoryExpectedStatus(JobStatusChoices.JobHistorySuspended);

			// Act
			_sut.OnJobComplete(job);

			// Assert
			_messageServiceMock.Verify(x => x.Send(It.IsAny<IMessage>()), Times.Never);
		}

		private void SetupJobHistoryExpectedStatus(ChoiceRef status)
		{
			_jobStatusUpdaterFake.Setup(x => x.GenerateStatus(It.IsAny<JobHistory>(), It.IsAny<long?>()))
				.Returns(status);
		}

		private void SetupJobHistory(DateTime? endTimeUtc = null, int totalItems = 0, 
			int itemsTransferred = 0, int itemsWithErrors = 0)
		{
			_jobHistoryServiceFake.Setup(x => x.GetRdo(It.IsAny<Guid>()))
				.Returns(new JobHistory
				{
					StartTimeUTC = _JOB_HISTORY_START_TIME_UTC,
					EndTimeUTC = endTimeUtc,
					TotalItems = totalItems,
					ItemsTransferred = itemsTransferred,
					ItemsWithErrors = itemsWithErrors
				});
		}

		private static IEnumerable<ChoiceRef> JobCompletedStatusChoices()
		{
			yield return JobStatusChoices.JobHistoryCompletedWithErrors;
			yield return JobStatusChoices.JobHistoryCompleted;
			yield return JobStatusChoices.JobHistoryStopped;
		}
	}
}