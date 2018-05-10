using System;
using System.Collections.Generic;
using System.Data;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Monitoring;
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
			_serializer = Substitute.For<ISerializer>();

			_integrationPoint = Substitute.For<Data.IntegrationPoint>();
			_integrationPoint.SourceProvider.Returns(0);
			_integrationPoint.DestinationProvider.Returns(0);
			
			_integrationPointService.GetRdo(Arg.Any<int>()).Returns(_integrationPoint);

			_taskParameters = Substitute.For<TaskParameters>();
			_taskParameters.BatchInstance = Guid.Empty;

			_serializer.Deserialize<TaskParameters>(Arg.Any<string>()).Returns(_taskParameters);

			_instance = new JobLifetimeMetricBatchStatus(_messageService, _integrationPointService, _providerTypeService, _updater, _jobHistoryService, _serializer);
		}

		[Test]
		public void OnJobStart_SendJobStartedMessage()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob(null);

			// ACT
			_instance.OnJobStart(job);

			// ASSERT
			_messageService.Received().Send(Arg.Any<JobStartedMessage>());
		}

		[Test]
		public void OnJobComplete_SendJobFailedMessage()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob();
			_updater.GenerateStatus(Arg.Any<JobHistory>(), Arg.Any<long>()).Returns(JobStatusChoices.JobHistoryErrorJobFailed);

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
			_updater.GenerateStatus(Arg.Any<JobHistory>(), Arg.Any<long>()).Returns(JobStatusChoices.JobHistoryValidationFailed);

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
			_updater.GenerateStatus(Arg.Any<JobHistory>(), Arg.Any<long>()).Returns(status);

			// ACT
			_instance.OnJobComplete(job);

			// ASSERT
			_messageService.Received().Send(Arg.Any<JobCompletedMessage>());
		}

		private static IEnumerable<Choice> JobCompletedStatusChoices()
		{
			yield return JobStatusChoices.JobHistoryCompletedWithErrors;
			yield return JobStatusChoices.JobHistoryCompleted;
			yield return JobStatusChoices.JobHistoryStopped;
		}
	}
}