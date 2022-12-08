using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests
{
    [TestFixture, Category("Unit")]
    public class TaskExceptionServiceTests
    {
        private TaskExceptionService _subjectUnderTest;
        private IJobHistoryErrorService _jobHistoryErrorServiceMock;
        private IJobHistoryService _jobHistoryServiceMock;
        private IJobService _jobServiceMock;
        private JobHistory _jobHistoryDto;

        [SetUp]
        public void SetUp(){

            _jobHistoryErrorServiceMock = Substitute.For<IJobHistoryErrorService>();
            _jobHistoryServiceMock = Substitute.For<IJobHistoryService>();
            _jobServiceMock = Substitute.For<IJobService>();
            ISerializer serializer = Substitute.For<ISerializer>();
            IAPILog logger = Substitute.For<IAPILog>();
            _jobHistoryDto = new JobHistory();
            _subjectUnderTest = new TaskExceptionService(logger,
                _jobHistoryErrorServiceMock,
                _jobHistoryServiceMock,
                _jobServiceMock, serializer);
        }

        [Test]
        public void ItShould_EndTaskWithError_ForITaskWithHistory()
        {
            //Arange
            var task = Substitute.For<ITaskWithJobHistory>();
            task.JobHistory.Returns(_jobHistoryDto);
            var exception = new IntegrationPointsException("Job failed miserably.");

            //Act
            _subjectUnderTest.EndTaskWithError(task, exception);

            //Assert
            Assert.AreEqual(JobStatusChoices.JobHistoryErrorJobFailed.Name, _jobHistoryDto.JobStatus.Name);
            _jobHistoryErrorServiceMock.Received(1).AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, exception.Message, exception.StackTrace );
            _jobHistoryServiceMock.Received(1).UpdateRdo(_jobHistoryDto);
            _jobServiceMock.Received(1).CleanupJobQueueTable();
        }
    }
}
