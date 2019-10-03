using System;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Keywords;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture]
	public class BatchEmailTests : TestBase
	{
		private BatchEmail _sut;

		private Mock<ICaseServiceContext> _caseServiceContext;
		private Mock<IHelper> _helper;
		private Mock<IIntegrationPointRepository> _integrationPointRepository;
		private Mock<IJobManager> _jobManager;
		private Mock<IJobService> _jobService;
		private Mock<IJobStatusUpdater> _jobStatusUpdater;
		private Mock<IEmailFormatter> _converter;
		private Mock<IManagerFactory> _managerFactory;
		private Mock<IRelativityObjectManager> _objectManager;
		private Mock<IRSAPIService> _rsapiService;

		private static object[] _generateEmailSource =
		{
			new object[] { JobStatusChoices.JobHistoryCompletedWithErrors, Properties.JobStatusMessages.JOB_COMPLETED_WITH_ERRORS_SUBJECT, Properties.JobStatusMessages.JOB_COMPLETED_WITH_ERRORS_BODY },
			new object[] { JobStatusChoices.JobHistoryErrorJobFailed, Properties.JobStatusMessages.JOB_FAILED_SUBJECT, Properties.JobStatusMessages.JOB_FAILED_BODY },
			new object[] { JobStatusChoices.JobHistoryStopped, Properties.JobStatusMessages.JOB_STOPPED_SUBJECT, Properties.JobStatusMessages.JOB_STOPPED_BODY },
			new object[] { JobStatusChoices.JobHistoryCompleted, Properties.JobStatusMessages.JOB_COMPLETED_SUCCESS_SUBJECT, Properties.JobStatusMessages.JOB_COMPLETED_SUCCESS_BODY },
		};

		private const int _INTEGRATION_POINT_ID = 1337;
		private readonly IDataProviderFactory _dataProviderFactory = null;
		private readonly IJobHistoryService _jobHistoryService = null;
		private Mock<ISerializer> _serializer;
		private readonly ISynchronizerFactory _appDomainRdoSynchronizerFactoryFactory = null;
		private readonly JobHistoryErrorService _jobHistoryErrorService = null;

		[SetUp]
		public override void SetUp()
		{
			_helper = new Mock<IHelper>(MockBehavior.Loose) { DefaultValue = DefaultValue.Mock };
			_converter = new Mock<IEmailFormatter>();
			_caseServiceContext = new Mock<ICaseServiceContext>();
			_jobStatusUpdater = new Mock<IJobStatusUpdater>();
			_rsapiService = new Mock<IRSAPIService>();
			_objectManager = new Mock<IRelativityObjectManager>();
			_managerFactory = new Mock<IManagerFactory>();
			_jobService = new Mock<IJobService>();
			_serializer = new Mock<ISerializer>();
			_integrationPointRepository = new Mock<IIntegrationPointRepository>();
			_jobManager = new Mock<IJobManager>();
			_rsapiService.Setup(x => x.RelativityObjectManager).Returns(_objectManager.Object);
			_caseServiceContext.Setup(x => x.RsapiService).Returns(_rsapiService.Object);

			_sut = new BatchEmail(_caseServiceContext.Object,
				_helper.Object,
				_dataProviderFactory,
				_serializer.Object,
				_appDomainRdoSynchronizerFactoryFactory,
				_jobHistoryService,
				_jobHistoryErrorService,
				_jobManager.Object,
				_jobStatusUpdater.Object,
				_converter.Object,
				_managerFactory.Object,
				_jobService.Object,
				_integrationPointRepository.Object);
		}

		[Test]
		public void OnJobComplete_NoEmails_Test()
		{
			// ARRANGE
			Job job = GetTestJob();

			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint
			{
				EmailNotificationRecipients = string.Empty,
			};

			_integrationPointRepository.Setup(x => x.ReadWithFieldMappingAsync(_INTEGRATION_POINT_ID)).ReturnsAsync(integrationPoint);

			// ACT + ASSERT
			Assert.DoesNotThrow(() =>
			{
				_sut.OnJobComplete(job);
			}
			, "Sending of email logic should have been skipped");
		}

		[Test]
		public void OnJobComplete_Emails_Test()
		{
			//ARRANGE
			string emailAddresses = "adr1@rip.com";
			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint
			{
				EmailNotificationRecipients = emailAddresses,
			};

			Job job = GetTestJob();
			TaskParameters taskParameters = new TaskParameters
			{
				BatchInstance = Guid.NewGuid()
			};
			_jobStatusUpdater
				.Setup(x => x.GenerateStatus(It.IsAny<Guid>()))
				.Returns(JobStatusChoices.JobHistoryCompleted);
			_converter.Setup(x => x.Format(It.IsAny<string>())).Returns<string>(y => y);
			JSONSerializer serializer = new JSONSerializer();
			string taskParametersSerialized = serializer.Serialize(taskParameters);
			job.JobDetails = taskParametersSerialized;

			_integrationPointRepository.Setup(x => x.ReadWithFieldMappingAsync(_INTEGRATION_POINT_ID)).ReturnsAsync(integrationPoint);
			//ACT
			_sut.OnJobComplete(job);
			//ASSERT
			_jobManager.Verify(x => x.CreateJob(job, It.IsAny<TaskParameters>(), TaskType.SendEmailWorker));
		}

		[Test]
		public void EmailJobParametersShouldHaveTheSameBatchInstanceAsParentJob()
		{
			//ARRANGE
			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint();
			integrationPoint.EmailNotificationRecipients = "email@email.com";
			_integrationPointRepository
				.Setup(x => x.ReadWithFieldMappingAsync(It.IsAny<int>()))
				.ReturnsAsync(integrationPoint);

			Guid batchInstanceGuid = Guid.NewGuid();
			string jobDetails = $"{{\"BatchInstance\":\"{batchInstanceGuid.ToString()}\",\"BatchParameters\":\"{{\"}}}}";
			TaskParameters taskParameters = new TaskParameters() { BatchInstance = batchInstanceGuid };
			_serializer.Setup(x => x.Deserialize<TaskParameters>(jobDetails)).Returns(taskParameters);
			_jobStatusUpdater.Setup(x => x.GenerateStatus(It.IsAny<Guid>()))
				.Returns(Data.JobStatusChoices.JobHistoryCompletedWithErrors);
			Job parentJob = JobHelper.GetJob(
				1,
				null,
				null,
				1,
				1,
				111,
				222,
				TaskType.ExportManager,
				new DateTime(),
				null,
				jobDetails,
				0,
				new DateTime(),
				1,
				null,
				null
			);

			//ACT
			_sut.OnJobComplete(parentJob);

			//ASSERT
			_jobManager.Verify(x =>
				x.CreateJob(
					parentJob,
					It.Is<TaskParameters>(y =>
						y.BatchInstance == batchInstanceGuid
					),
					TaskType.SendEmailWorker));
		}

		[TestCaseSource(nameof(_generateEmailSource))]
		public void GenerateEmail(Choice jobStatus, string expectedSubject, string expectedBody)
		{
			// ACT
			EmailJobParameters jobParameters = BatchEmail.GenerateEmailJobParameters(jobStatus);

			// ASSERT
			expectedSubject.Should().Be(jobParameters.Subject);
			expectedBody.Should().Be(jobParameters.MessageBody);
		}

		private Job GetTestJob()
		{
			return JobHelper.GetJob(1337, null, null, 1, 1, 111, _INTEGRATION_POINT_ID, TaskType.ExportManager, new DateTime(), null, "", 0, new DateTime(), 1, null, null);
		}
	}
}
