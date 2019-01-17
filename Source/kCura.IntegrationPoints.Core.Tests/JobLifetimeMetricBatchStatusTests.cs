using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Common.Monitoring.Messages;
using kCura.IntegrationPoints.Common.Monitoring.Messages.JobLifetime;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Monitoring.JobLifetime;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture]
	public class JobLifetimeMetricBatchStatusTests : TestBase
	{
		private IMessageService _messageService;
		private IIntegrationPointService _integrationPointService;
		private IProviderTypeService _providerTypeService;
		private IJobStatusUpdater _updater;
		private IJobHistoryService _jobHistoryService;
		private ISerializer _serializer;
		private IDateTimeHelper _dateTimeHelper;

		private Data.IntegrationPoint _integrationPoint;
		private TaskParameters _taskParameters;

		private JobLifetimeMetricBatchStatus _instance;

		[SetUp]
		public override void SetUp()
		{
			_messageService = Substitute.For<IMessageService>();
			_integrationPointService = Substitute.For<IIntegrationPointService>();
			_providerTypeService = Substitute.For<IProviderTypeService>();
			_providerTypeService.GetProviderType(Arg.Any<int>(), Arg.Any<int>()).Returns(ProviderType.FTP);
			_updater = Substitute.For<IJobStatusUpdater>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns(new JobHistory() { ItemsTransferred = 0, TotalItems = 0, StartTimeUTC = DateTime.UtcNow, EndTimeUTC = DateTime.UtcNow.AddSeconds(5) });
			_serializer = Substitute.For<ISerializer>();
			_dateTimeHelper = Substitute.For<IDateTimeHelper>();

			_integrationPoint = Substitute.For<Data.IntegrationPoint>();
			_integrationPoint.SourceProvider.Returns(0);
			_integrationPoint.DestinationProvider.Returns(0);

			_integrationPointService.GetRdo(Arg.Any<int>()).Returns(_integrationPoint);

			_taskParameters = Substitute.For<TaskParameters>();
			_taskParameters.BatchInstance = Guid.Empty;

			_serializer.Deserialize<TaskParameters>(Arg.Any<string>()).Returns(_taskParameters);

			_instance = new JobLifetimeMetricBatchStatus(_messageService, _integrationPointService, _providerTypeService, _updater, _jobHistoryService, _serializer, _dateTimeHelper);
		}

		[Test]
		public void OnJobStart_SendJobStartedMessage()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob();

			// ACT
			_instance.OnJobStart(job);

			// ASSERT
			_messageService.DidNotReceive().Send(Arg.Any<JobStartedMessage>());
		}

		[Test]
		public void OnJobComplete_SendJobFailedMessage()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob();
			_updater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryErrorJobFailed);

			// ACT
			_instance.OnJobComplete(job);

			// ASSERT
			_messageService.Received().Send(Arg.Any<JobFailedMessage>());
		}

		[Test]
		public void OnJobComplete_SendJobValidationFailedMessage()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob();
			_updater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryValidationFailed);

			// ACT
			_instance.OnJobComplete(job);

			// ASSERT
			_messageService.Received().Send(Arg.Any<JobValidationFailedMessage>());
		}

		[Test]
		[TestCaseSource(nameof(JobCompletedStatusChoices))]
		public void OnJobComplete_SendJobCompletedMessage(Choice status)
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob();
			_updater.GenerateStatus(Arg.Any<JobHistory>()).Returns(status);

			// ACT
			_instance.OnJobComplete(job);

			// ASSERT
			_messageService.Received().Send(Arg.Any<JobCompletedMessage>());
		}

		[TestCase(50, 10)]
		[TestCase(10, 1000)]
		[TestCase(1, 9999)]
		[TestCase(1000, 1)]
		public void OnJobComplete_SendThroughput(int records, double durationInSeconds)
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob();
			IJobHistoryService jobHistoryService = Substitute.For<IJobHistoryService>();
			JobHistory jobHistory = new JobHistory();
			jobHistory.StartTimeUTC = new DateTime(2018, 1, 1, 0, 0, 0);
			jobHistory.EndTimeUTC = jobHistory.StartTimeUTC.Value.AddSeconds(durationInSeconds);
			jobHistory.TotalItems = records;
			jobHistory.ItemsTransferred = records;
			jobHistory.ItemsWithErrors = 0;
			jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns(jobHistory);

			_updater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryCompleted);

			JobLifetimeMetricBatchStatus metric = new JobLifetimeMetricBatchStatus(_messageService, _integrationPointService, _providerTypeService, _updater, jobHistoryService, _serializer, _dateTimeHelper);

			// ACT
			metric.OnJobComplete(job);

			// ASSERT
			double expectedThroughput = records / durationInSeconds;
			_messageService.Received().Send(Arg.Is<JobThroughputMessage>(msg => msg.RecordsPerSecond == expectedThroughput));
		}

		[Test]
		public void OnJobComplete_NotSendingThroughputWhenZeroRecords()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob();
			IJobHistoryService jobHistoryService = Substitute.For<IJobHistoryService>();
			JobHistory jobHistory = new JobHistory();
			jobHistory.StartTimeUTC = new DateTime(2018, 1, 1, 0, 0, 0);
			jobHistory.EndTimeUTC = jobHistory.StartTimeUTC.Value.AddSeconds(1);
			jobHistory.TotalItems = 10;
			jobHistory.ItemsTransferred = 0;
			jobHistory.ItemsWithErrors = 0;
			jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns(jobHistory);

			_updater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryCompleted);

			JobLifetimeMetricBatchStatus metric = new JobLifetimeMetricBatchStatus(_messageService, _integrationPointService, _providerTypeService, _updater, jobHistoryService, _serializer, _dateTimeHelper);

			// ACT
			metric.OnJobComplete(job);

			// ASSERT
			_messageService.DidNotReceive().Send(Arg.Any<JobThroughputMessage>());
		}

		[Test]
		public void OnJobComplete_NullEndTimeDoesntBreakMetric()
		{
			// ARRANGE
			int itemsTransfered = 10;
			int seconds = 10;
			Job job = JobExtensions.CreateJob();
			_updater.GenerateStatus(Arg.Any<JobHistory>()).Returns(JobStatusChoices.JobHistoryErrorJobFailed);
			IJobHistoryService jobHistoryService = Substitute.For<IJobHistoryService>();
			JobHistory jobHistory = new JobHistory();
			jobHistory.StartTimeUTC = new DateTime(2018, 1, 1, 0, 0, 0);
			jobHistory.EndTimeUTC = null;
			jobHistory.TotalItems = 0;
			jobHistory.ItemsTransferred = itemsTransfered;
			jobHistory.ItemsWithErrors = 0;
			jobHistoryService.GetRdo(Arg.Any<Guid>()).Returns(jobHistory);

			IDateTimeHelper dateTimeHelper = Substitute.For<IDateTimeHelper>();
			dateTimeHelper.Now().Returns(jobHistory.StartTimeUTC.Value.AddSeconds(seconds));

			JobLifetimeMetricBatchStatus metric = new JobLifetimeMetricBatchStatus(_messageService, _integrationPointService, _providerTypeService, _updater, jobHistoryService, _serializer, dateTimeHelper);

			// ACT
			metric.OnJobComplete(job);

			// ASSERT
			double expectedThroughput = (double) itemsTransfered / seconds;
			_messageService.Received().Send(Arg.Is<JobThroughputMessage>(msg => msg.RecordsPerSecond == expectedThroughput));
		}

		private static IEnumerable<Choice> JobCompletedStatusChoices()
		{
			yield return JobStatusChoices.JobHistoryCompletedWithErrors;
			yield return JobStatusChoices.JobHistoryCompleted;
			yield return JobStatusChoices.JobHistoryStopped;
		}
	}
}