using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Keywords;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Core.Tests
{
    [TestFixture, Category("Unit")]
    public class BatchEmailTests : TestBase
    {
        private BatchEmail _sut;

        private Mock<ICaseServiceContext> _caseServiceContextMock;
        private Mock<IEmailFormatter> _emailFormatterMock;
        private Mock<IHelper> _helperMock;
        private Mock<IIntegrationPointRepository> _integrationPointRepositoryMock;
        private Mock<IJobManager> _jobManagerMock;
        private Mock<IJobService> _jobServiceMock;
        private Mock<IJobStatusUpdater> _jobStatusUpdaterMock;
        private Mock<IManagerFactory> _managerFactoryMock;
        private Mock<IRelativityObjectManager> _objectManagerMock;
        private Mock<IRelativityObjectManagerService> _objectManagerServiceMock;

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
        private readonly ISynchronizerFactory _appDomainRdoSynchronizerFactoryFactory = null;
        private readonly JobHistoryErrorService _jobHistoryErrorService = null;
        private readonly JSONSerializer _jsonSerializer = new JSONSerializer();

        [SetUp]
        public override void SetUp()
        {
            _helperMock = new Mock<IHelper>(MockBehavior.Loose) { DefaultValue = DefaultValue.Mock };
            _emailFormatterMock = new Mock<IEmailFormatter>();
            _caseServiceContextMock = new Mock<ICaseServiceContext>();
            _jobStatusUpdaterMock = new Mock<IJobStatusUpdater>();
            _objectManagerServiceMock = new Mock<IRelativityObjectManagerService>();
            _objectManagerMock = new Mock<IRelativityObjectManager>();
            _managerFactoryMock = new Mock<IManagerFactory>();
            _jobServiceMock = new Mock<IJobService>();
            _integrationPointRepositoryMock = new Mock<IIntegrationPointRepository>();
            _jobManagerMock = new Mock<IJobManager>();
            _objectManagerServiceMock.Setup(x => x.RelativityObjectManager).Returns(_objectManagerMock.Object);
            _caseServiceContextMock.Setup(x => x.RelativityObjectManagerService).Returns(_objectManagerServiceMock.Object);

            _sut = new BatchEmail(
                _caseServiceContextMock.Object,
                _helperMock.Object,
                _dataProviderFactory,
                _jsonSerializer,
                _appDomainRdoSynchronizerFactoryFactory,
                _jobHistoryService,
                _jobHistoryErrorService,
                _jobManagerMock.Object,
                _jobStatusUpdaterMock.Object,
                _emailFormatterMock.Object,
                _managerFactoryMock.Object,
                _jobServiceMock.Object,
                _integrationPointRepositoryMock.Object,
                new EmptyDiagnosticLog());
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

            _integrationPointRepositoryMock.Setup(x => x.ReadWithFieldMappingAsync(_INTEGRATION_POINT_ID)).ReturnsAsync(integrationPoint);

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
            _jobStatusUpdaterMock
                .Setup(x => x.GenerateStatus(It.IsAny<Guid>()))
                .Returns(JobStatusChoices.JobHistoryCompleted);

            _emailFormatterMock.Setup(x => x.Format(It.IsAny<string>())).Returns<string>(y => y);
            string taskParametersSerialized = _jsonSerializer.Serialize(taskParameters);
            job.JobDetails = taskParametersSerialized;

            _integrationPointRepositoryMock.Setup(x => x.ReadWithFieldMappingAsync(_INTEGRATION_POINT_ID)).ReturnsAsync(integrationPoint);
            Job emailJob = new JobBuilder().WithJobId(1338).Build();
            _jobManagerMock.Setup(x => x.CreateJob(job, It.IsAny<TaskParameters>(), TaskType.SendEmailWorker)).Returns(emailJob);
            //ACT
            _sut.OnJobComplete(job);
            //ASSERT
            _jobManagerMock.Verify(x => x.CreateJob(job, It.IsAny<TaskParameters>(), TaskType.SendEmailWorker));
        }

        [Test]
        public void EmailJobParametersShouldHaveTheSameBatchInstanceAsParentJob()
        {
            //ARRANGE
            Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint();
            integrationPoint.EmailNotificationRecipients = "email@email.com";
            _integrationPointRepositoryMock
                .Setup(x => x.ReadWithFieldMappingAsync(It.IsAny<int>()))
                .ReturnsAsync(integrationPoint);

            Guid batchInstanceGuid = Guid.NewGuid();
            string jobDetails = $"{{\"BatchInstance\":\"{batchInstanceGuid.ToString()}\"}}";
            _jobStatusUpdaterMock.Setup(x => x.GenerateStatus(It.IsAny<Guid>()))
                .Returns(Data.JobStatusChoices.JobHistoryCompletedWithErrors);
            Job parentJob = GetTestJob();
            parentJob.JobDetails = jobDetails;
            Job emailJob = new JobBuilder().WithJobId(1338).Build();
            _jobManagerMock.Setup(x => x.CreateJob(parentJob, It.IsAny<TaskParameters>(), TaskType.SendEmailWorker)).Returns(emailJob);
            //ACT
            _sut.OnJobComplete(parentJob);

            //ASSERT
            _jobManagerMock.Verify(x => x.CreateJob(
                parentJob,
                It.Is<TaskParameters>(y => y.BatchInstance == batchInstanceGuid), TaskType.SendEmailWorker)
            );
        }

        [Test]
        public void EmailJobShouldHaveStopStateResetToNoneAfterCreation()
        {
            //arrange
            Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint();
            integrationPoint.EmailNotificationRecipients = "xyz@email.com";
            _integrationPointRepositoryMock
                .Setup(x => x.ReadWithFieldMappingAsync(It.IsAny<int>()))
                .ReturnsAsync(integrationPoint);

            Guid batchInstanceGuid = Guid.NewGuid();
            string jobDetails = $"{{\"BatchInstance\":\"{batchInstanceGuid.ToString()}\"}}";
            _jobStatusUpdaterMock.Setup(x => x.GenerateStatus(It.IsAny<Guid>()))
                .Returns(JobStatusChoices.JobHistoryCompletedWithErrors);
            Job job = GetTestJob();
            job.JobDetails = jobDetails;

            Job emailJob = new JobBuilder().WithJobId(1338).Build();
            _jobManagerMock.Setup(x => x.CreateJob(job, It.IsAny<TaskParameters>(), TaskType.SendEmailWorker)).Returns(emailJob);
            
            //act
            _sut.OnJobComplete(job);

            //assert                    
            _jobServiceMock.Verify(x => x.UpdateStopState(It.Is<IList<long>>(j => j.Contains(emailJob.JobId)),
                It.Is<StopState>(s => s == StopState.None)), Times.Once);            
        }

        [TestCaseSource(nameof(_generateEmailSource))]
        public void GenerateEmail(ChoiceRef jobStatus, string expectedSubject, string expectedBody)
        {
            // ACT
            _emailFormatterMock.Setup(x => x.Format(It.IsAny<string>())).Returns<string>(y => y);
            (string Subject, string MessageBody) jobParameters = _sut.GetEmailSubjectAndBodyForJobStatus(jobStatus);

            // ASSERT
            jobParameters.Subject.Should().Be(expectedSubject);
            jobParameters.MessageBody.Should().Be(expectedBody);
        }

        private Job GetTestJob()
        {
            return JobHelper.GetJob(1337, null, null, 1, 1, 111, _INTEGRATION_POINT_ID, TaskType.ExportManager, new DateTime(), null, "", 0, new DateTime(), 1, null, null);
        }
    }
}
