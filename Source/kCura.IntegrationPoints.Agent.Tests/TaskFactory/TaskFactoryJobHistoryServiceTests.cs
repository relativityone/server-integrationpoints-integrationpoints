using System;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Agent.Tests.TaskFactory
{
    [TestFixture, Category("Unit")]
    public class TaskFactoryJobHistoryServiceTests
    {
        private ITaskFactoryJobHistoryService _sut;
        private IJobHistoryService _jobHistoryService;
        private IJobHistoryErrorService _jobHistoryErrorService;
        private IIntegrationPointRepository _integrationPointRepository;

        public void SetUp(Data.IntegrationPoint ip = null)
        {
            IAPILog logger = Substitute.For<IAPILog>();
            IIntegrationPointSerializer serializer = Substitute.For<IIntegrationPointSerializer>();
            serializer.Deserialize<DestinationConfiguration>(Arg.Any<string>()).Returns(new DestinationConfiguration());
            serializer.Deserialize<TaskParameters>(Arg.Any<string>()).Returns(new TaskParameters());

            _jobHistoryService = Substitute.For<IJobHistoryService>();

            var serviceFactory = Substitute.For<IServiceFactory>();
            serviceFactory.CreateJobHistoryService(Arg.Any<IAPILog>()).Returns(_jobHistoryService);
            _jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
            _integrationPointRepository = Substitute.For<IIntegrationPointRepository>();

            ip = ip ?? GetDefaultIntegrationPoint();
            _sut = new TaskFactoryJobHistoryService(
                logger,
                serializer,
                serviceFactory,
                _jobHistoryErrorService,
                _integrationPointRepository,
                ip
            );
        }

        [Test]
        public void ItShouldUpdateJobHistoryWhenItDoesnNotHaveValue()
        {
            SetUp();
            // Arrange
            int jobId = 123;
            Job job = new JobBuilder().WithJobId(jobId).Build();

            var jobHistory = new JobHistory
            {
                JobID = null
            };
            _jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns(jobHistory);

            // Act
            _sut.SetJobIdOnJobHistory(job);

            // Assert
            _jobHistoryService.Received().UpdateRdo(Arg.Is<JobHistory>(h => h.JobID == jobId.ToString()));
        }

        [Test]
        public void ItShouldNotUpdateJobHistoryWhenItHasValue()
        {
            SetUp();
            // Arrange
            int jobId = 123;
            Job job = new JobBuilder().WithJobId(jobId).Build();

            var jobHistory = new JobHistory
            {
                JobID = jobId.ToString()
            };
            _jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns(jobHistory);

            // Act
            _sut.SetJobIdOnJobHistory(job);

            // Assert
            _jobHistoryService.DidNotReceiveWithAnyArgs().UpdateRdo(Arg.Any<JobHistory>());
        }

        [Test]
        public void ItShouldNotUpdateJobHistoryWhenHobHistoryServiceReturnNull()
        {
            SetUp();
            // Arrange
            Job job = JobExtensions.CreateJob();
            _jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns((JobHistory)null);

            // Act
            _sut.SetJobIdOnJobHistory(job);

            // Assert
            _jobHistoryService.DidNotReceiveWithAnyArgs().UpdateRdo(Arg.Any<JobHistory>());
        }

        [Test]
        public void ItShouldNotUpdateJobHistoryWhenJobWasNull()
        {
            // Arrange
            SetUp();
            Job job = null;
            var jobHistory = new JobHistory();
            _jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns(jobHistory);

            // Act
            _sut.SetJobIdOnJobHistory(job);

            // Assert
            _jobHistoryService.DidNotReceiveWithAnyArgs().UpdateRdo(Arg.Any<JobHistory>());
        }

        [TestCase(new[] { 123 }, 123, new int[] { })]
        [TestCase(new[] { 1, 2 }, 2, new[] { 1 })]
        [TestCase(new[] { 1, 2 }, 3, new[] { 1, 2 })]
        [TestCase(new int[] { }, 3, new int[] { })]
        public void ItShouldUpdateIntegrationPointWhenRemovingJobHistoryFromIntegrationPoint(int[] ipJobHistoryIds, int jobHistoryId, int[] expectedIpJobHistoryIdsAfterUpdate)
        {
            // Arrange 
            Data.IntegrationPoint ip = GetDefaultIntegrationPoint();
            ip.JobHistory = ipJobHistoryIds;
            SetUp(ip);

            Job job = JobExtensions.CreateJob();
            var jobHistory = new JobHistory
            {
                ArtifactId = jobHistoryId
            };
            _jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns(jobHistory);

            // Act
            _sut.RemoveJobHistoryFromIntegrationPoint(job);

            // Assert
            _integrationPointRepository.Received()
                .Update(Arg.Is<Data.IntegrationPoint>(x => x.JobHistory.SequenceEqual(expectedIpJobHistoryIdsAfterUpdate)));
        }

        [Test]
        public void ItShouldUpdateJobHistoryStatusWhenRemovingJobHistoryFromIntegrationPoint()
        {
            // Arrange 
            Data.IntegrationPoint ip = GetDefaultIntegrationPoint();
            ip.JobHistory = new int[] { };
            SetUp(ip);

            Job job = JobExtensions.CreateJob();
            var jobHistory = new JobHistory();
            _jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns(jobHistory);

            // Act
            _sut.RemoveJobHistoryFromIntegrationPoint(job);

            // Assert
            _jobHistoryService.Received().UpdateRdo(Arg.Is<JobHistory>(x => x.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryStopped)));
        }

        [Test]
        public void ItShouldDeleteJobHistoryWhenRemovingJobHistoryFromIntegrationPoint()
        {
            // Arrange 
            Data.IntegrationPoint ip = GetDefaultIntegrationPoint();
            ip.JobHistory = new int[] { };
            SetUp(ip);

            Job job = JobExtensions.CreateJob();
            int jobHistoryArtifactId = 654;
            var jobHistory = new JobHistory
            {
                ArtifactId = jobHistoryArtifactId
            };
            _jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns(jobHistory);

            // Act
            _sut.RemoveJobHistoryFromIntegrationPoint(job);

            // Assert
            _jobHistoryService.Received().DeleteRdo(Arg.Is<int>(x => x == jobHistoryArtifactId));
        }

        [Test]
        public void ItShouldUpdateJobHistoryStatusOnFailure()
        {
            // Arrange
            SetUp();

            Job job = JobExtensions.CreateJob();
            int jobHistoryArtifactId = 654;
            var jobHistory = new JobHistory
            {
                ArtifactId = jobHistoryArtifactId
            };
            _jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns(jobHistory);

            // Act
            _sut.UpdateJobHistoryOnFailure(job, null);

            // Assert
            _jobHistoryService.Received().UpdateRdoWithoutDocuments(Arg.Is<JobHistory>(x => x.JobStatus.Name == JobStatusChoices.JobHistoryErrorJobFailed.Name));
        }

        [Test]
        public void ItShouldAddJobHistoryErrorOnFailure()
        {
            // Arrange
            int integrationPointArtifactId = 943438;
            var ip = GetDefaultIntegrationPoint();
            ip.ArtifactId = integrationPointArtifactId;
            SetUp(ip);

            Job job = JobExtensions.CreateJob();
            int jobHistoryArtifactId = 654;
            var jobHistory = new JobHistory
            {
                ArtifactId = jobHistoryArtifactId
            };
            _jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<ChoiceRef>(), Arg.Any<DateTime?>()).Returns(jobHistory);

            string expectedExceptionMessage = "Expected exception message";
            var exception = new Exception(expectedExceptionMessage);

            // Act
            _sut.UpdateJobHistoryOnFailure(job, exception);

            // Assert
            Assert.AreEqual(integrationPointArtifactId, _jobHistoryErrorService.IntegrationPoint.ArtifactId);
            Assert.AreEqual(jobHistoryArtifactId, _jobHistoryErrorService.JobHistory.ArtifactId);
            _jobHistoryErrorService.Received().AddError(Arg.Is<ChoiceRef>(x => x.Name == ErrorTypeChoices.JobHistoryErrorJob.Name), exception);
        }

        private Data.IntegrationPoint GetDefaultIntegrationPoint()
        {
            return new Data.IntegrationPoint
            {
                DestinationConfiguration = string.Empty,
                SecuredConfiguration = string.Empty
            };
        }
    }
}
