using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Common.Helpers;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Validation;
using Moq;
using Moq.Language;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.Environmental;
using Relativity.Services.ResourceServer;

namespace kCura.ScheduleQueue.AgentBase.Tests
{
    [TestFixture]
    [Category("Unit")]
    public class ScheduleQueueAgentBaseTests
    {
        private Mock<IJobService> _jobServiceMock;
        private Mock<IQueueJobValidator> _queueJobValidatorFake;
        private Mock<IQueueQueryManager> _queryManager;
        private Mock<IKubernetesMode> _kubernetesModeFake;
        private Mock<IDateTime> _dateTime;
        private Mock<IConfig> _config;

        [Test]
        public void Execute_ShouldProcessJobInQueue()
        {
            // Arrange
            Job expectedJob = new JobBuilder().WithJobId(1).Build();

            TestAgent sut = GetSut();

            SetupJobQueue(expectedJob);

            // Act
            sut.Execute();

            // Assert
            sut.ProcessedJobs.Single().ShouldBeEquivalentTo(expectedJob);
        }

        [Test]
        public void Execute_ShouldDeleteJobFromQueue_WhenJobIsInvalid()
        {
            // Arrange
            Job validJob1 = new JobBuilder().WithJobId(1).Build();
            Job invalidJob = new JobBuilder().WithJobId(-1).Build();
            Job validJob2 = new JobBuilder().WithJobId(2).Build();

            IList<Job> expectedProcessedJobs = new List<Job> { validJob1, validJob2 };

            TestAgent sut = GetSut();

            SetupJobQueue(validJob1, invalidJob, validJob2);

            SetupJobAsInvalid(invalidJob);

            // Act
            sut.Execute();

            // Assert
            sut.ProcessedJobs.ShouldBeEquivalentTo(expectedProcessedJobs);

            _jobServiceMock.Verify(x => x.FinalizeJob(
                It.Is<Job>(y => y.JobFailed != null),
                It.IsAny<IScheduleRuleFactory>(),
                It.Is<TaskResult>(y => y.Status == TaskStatusEnum.Fail)));
        }

        [Test]
        public void Execute_ShouldRunAndFailJob_WhenJobIsInvalidButWithExecution()
        {
            // Arrange
            Job job = new JobBuilder().WithJobId(1).Build();

            TestAgent sut = GetSut();

            SetupJobQueue(job);

            SetupJobAsInvalid(job, true);

            // Act
            sut.Execute();

            // Assert
            sut.ProcessedJobs.Should().Contain(job);

            job.JobFailed.Should().NotBeNull();
        }

        [Test]
        public void Execute_ShouldProcessJobInQueue_WhenJobValidationThrowsException()
        {
            // Arrange
            Job expectedJob = new JobBuilder().WithJobId(1).Build();

            TestAgent sut = GetSut();

            SetupJobQueue(expectedJob);

            _queueJobValidatorFake.Setup(x => x.ValidateAsync(expectedJob))
                .Throws<Exception>();

            // Act
            sut.Execute();

            // Assert
            sut.ProcessedJobs.Single().ShouldBeEquivalentTo(expectedJob);
        }

        [Test]
        public void Execute_ShouldNotExecuteJob_WhenToBeRemovedIsTrue()
        {
            // Arrange
            Job job = new JobBuilder().WithJobId(1).Build();
            TestAgent sut = GetSut();
            SetupJobQueue(job);

            sut.ToBeRemoved = true;

            // Act
            sut.Execute();

            // Assert
            sut.ProcessedJobs.Count.Should().Be(0);
        }

        [Test]
        public void Execute_ShouldNotFinalizeJob_WhenDrainStopped()
        {
            // Arrange
            Job job = new JobBuilder().WithJobId(1).Build();
            TestAgent sut = GetSut(TaskStatusEnum.DrainStopped);
            SetupJobQueue(job);

            // Act
            sut.Execute();

            // Assert
            _jobServiceMock.Verify(
                x => x.FinalizeJob(It.IsAny<Job>(), It.IsAny<IScheduleRuleFactory>(), It.IsAny<TaskResult>()),
                Times.Never);
        }

        [Test]
        public void Execute_ShouldNotTakeNextJob_WhenDrainStopped()
        {
            // Arrange
            Job job = new JobBuilder().WithJobId(1).Build();
            TestAgent sut = GetSut(TaskStatusEnum.DrainStopped);
            SetupJobQueue(job);

            // Act
            sut.Execute();

            // Assert
            _jobServiceMock.Verify(
                x => x.GetNextQueueJob(It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<long?>()),
                Times.Once);
        }

        [Test]
        public void Execute_ShouldSetDidWorkToFalse_WhenQueueIsEmpty()
        {
            // Arrange
            TestAgent sut = GetSut();

            // Act
            sut.Execute();

            // Assert
            sut.DidWork.Should().BeFalse();
        }

        [Test]
        public void Execute_ShouldSetDidWorkToFalse_WhenServicesUnavailable()
        {
            // Arrange
            TestAgent sut = GetSut();

            sut.HelperMock.Setup(x => x.GetServicesManager()).Throws(new Exception());

            // Act
            sut.Execute();

            // Assert
            sut.DidWork.Should().BeFalse();
        }

        [Test]
        public void Execute_ShouldSetDidWorkToTrue_WhenFinishedExecution()
        {
            // Arrange
            TestAgent sut = GetSut();

            Job job = new JobBuilder().WithJobId(1).Build();
            SetupJobQueue(job);

            // Act
            sut.Execute();

            // Assert
            sut.DidWork.Should().BeTrue();
        }

        [Test]
        public void GetAgentID_ShouldReturnAgentID_WhenNotInKubernetes()
        {
            // Arrange
            TestAgent sut = GetSut();
            _dateTime.SetupGet(x => x.UtcNow).Returns(new DateTime(2021, 10, 13));
            _kubernetesModeFake.Setup(x => x.IsEnabled()).Returns(false);

            // Act
            int agentId = sut.GetAgentIDTest();

            // Assert
            agentId.Should().Be(0);
        }

        [Test]
        public void GetAgentID_ShouldReturnAgentIDBasedOnTimestamp_WhenInKubernetes()
        {
            // Arrange
            TestAgent sut = GetSut();
            _dateTime.SetupGet(x => x.UtcNow).Returns(new DateTime(2021, 10, 13));
            _kubernetesModeFake.Setup(x => x.IsEnabled()).Returns(true);

            // Act
            int agentId = sut.GetAgentIDTest();

            // Assert
            agentId.Should().Be(1290028852);
        }

        [Test]
        public void Execute_ShouldCleanUpJob_WhenTimeOutWasExceeded()
        {
            // Arrange
            const int jobId = 100;

            DateTime utcNow = new DateTime(2022, 8, 1);
            DateTime lastHeartbeat = utcNow.Subtract(TimeSpan.FromHours(10));

            TestAgent sut = GetSut();

            _config.Setup(x => x.TransientStateJobTimeout).Returns(TimeSpan.FromHours(5));

            _dateTime.Setup(x => x.UtcNow).Returns(utcNow);

            Job job = new JobBuilder()
                .WithJobId(jobId)
                .WithHeartbeat(lastHeartbeat)
                .Build();
            SetupJobQueue(job);

            // Act
            sut.Execute();

            // Assert
            _jobServiceMock.Verify(
                x => x.FinalizeJob(
                    It.Is<Job>(y => y.JobFailed != null),
                    It.IsAny<IScheduleRuleFactory>(),
                    It.IsAny<TaskResult>()));
        }

        [Test]
        public void Execute_ShouldCleanUp_WhenJobIsStucked()
        {
            // Arrange
            const int jobId = 100;

            TestAgent sut = GetSut();

            Job job = new JobBuilder()
                .WithJobId(jobId)
                .WithLockedByAgentId(null)
                .WithStopState(StopState.DrainStopping)
                .Build();
            SetupJobQueue(job);

            // Act
            sut.Execute();

            // Assert
            _jobServiceMock.Verify(
                x => x.FinalizeJob(
                    It.Is<Job>(y => y.JobFailed != null),
                    It.IsAny<IScheduleRuleFactory>(),
                    It.IsAny<TaskResult>()));
        }

        [Test]
        public void Execute_ShouldNotThrow_WhenInvalidJobsCleanUpErrorsOut()
        {
            // Arrange
            const int jobId = 100;

            TestAgent sut = GetSut();

            Job job = new JobBuilder()
                .WithJobId(jobId)
                .WithLockedByAgentId(null)
                .WithStopState(StopState.DrainStopping)
                .Build();
            SetupJobQueue(job);

            _jobServiceMock.Setup(x => x.FinalizeJob(job, It.IsAny<IScheduleRuleFactory>(), It.IsAny<TaskResult>())).Throws<Exception>();
            _jobServiceMock.Setup(x => x.GetNextQueueJob(It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<long?>()))
                .Returns((Job)null);

            // Act
            Action action = () => sut.Execute();

            // Assert
            action.ShouldNotThrow();

            _jobServiceMock.Verify(x => x.GetNextQueueJob(It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<long?>()));
        }

        [Test]
        public void Execute_ShouldFailJobInTransientState_WhenValidationFails()
        {
            // Arrange
            const int jobId = 100;

            DateTime utcNow = new DateTime(2022, 8, 1);
            DateTime lastHeartbeat = utcNow.Subtract(TimeSpan.FromHours(10));

            TestAgent sut = GetSut();

            _config.Setup(x => x.TransientStateJobTimeout).Returns(TimeSpan.FromHours(5));

            _dateTime.Setup(x => x.UtcNow).Returns(utcNow);

            Job job = new JobBuilder()
                .WithJobId(jobId)
                .WithHeartbeat(lastHeartbeat)
                .Build();
            SetupJobQueue(job);

            const string exceptionMessage = "Invalid job";
            _queueJobValidatorFake.Setup(x => x.ValidateAsync(It.Is<Job>(y => y.JobId == jobId)))
                .ReturnsAsync(PreValidationResult.InvalidJob(exceptionMessage, false));

            // Act
            sut.Execute();

            // Assert
            _jobServiceMock.Verify(
                x => x.FinalizeJob(
                    It.Is<Job>(
                        y => y.JobFailed.ShouldBreakSchedule == true),
                    It.IsAny<IScheduleRuleFactory>(),
                    It.Is<TaskResult>(
                        z => z.Status == TaskStatusEnum.Fail &&
                             z.Exceptions.First().GetType() == typeof(InvalidOperationException) &&
                        z.Exceptions.First().Message == exceptionMessage)));
        }

        private TestAgent GetSut(TaskStatusEnum jobStatus = TaskStatusEnum.Success)
        {
            var agentService = new Mock<IAgentService>();
            var scheduleRuleFactory = new Mock<IScheduleRuleFactory>();
            var emptyLog = new Mock<IAPILog>();

            _jobServiceMock = new Mock<IJobService>();
            _jobServiceMock.Setup(
                    x => x.FinalizeJob(
                        It.IsAny<Job>(),
                        It.IsAny<IScheduleRuleFactory>(),
                        It.IsAny<TaskResult>()))
                .Returns(new FinalizeJobResult { JobState = JobLogState.Finished, Details = string.Empty });

            _queueJobValidatorFake = new Mock<IQueueJobValidator>();
            _queueJobValidatorFake.Setup(x => x.ValidateAsync(It.IsAny<Job>()))
                .ReturnsAsync(PreValidationResult.Success);

            _queryManager = new Mock<IQueueQueryManager>();
            _kubernetesModeFake = new Mock<IKubernetesMode>();
            _dateTime = new Mock<IDateTime>();

            _config = new Mock<IConfig>();
            _config.Setup(x => x.TransientStateJobTimeout).Returns(TimeSpan.MaxValue);

            return new TestAgent(
                agentService.Object,
                _jobServiceMock.Object,
                scheduleRuleFactory.Object,
                _queueJobValidatorFake.Object,
                _queryManager.Object,
                _kubernetesModeFake.Object,
                _dateTime.Object,
                emptyLog.Object,
                _config.Object)
            {
                JobResult = jobStatus
            };
        }

        private void SetupJobQueue(params Job[] jobs)
        {
            _jobServiceMock.Setup(x => x.GetAllScheduledJobs()).Returns(jobs);

            ISetupSequentialResult<Job> sequenceSetup = _jobServiceMock.SetupSequence(x => x.GetNextQueueJob(
                It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<long?>()));

            foreach (var job in jobs)
            {
                sequenceSetup = sequenceSetup.Returns(job);
            }
        }

        private void SetupJobAsInvalid(Job job, bool shouldExecute = false)
        {
            _queueJobValidatorFake.Setup(x => x.ValidateAsync(job))
                .ReturnsAsync(PreValidationResult.InvalidJob(It.IsAny<string>(), shouldExecute));
        }

        private class TestAgent : ScheduleQueueAgentBase
        {
            private Mock<IAgentHelper> _helperMockMock;

            public TestAgent(
                IAgentService agentService = null,
                IJobService jobService = null,
                IScheduleRuleFactory scheduleRuleFactory = null,
                IQueueJobValidator queueJobValidator = null,
                IQueueQueryManager queryManager = null,
                IKubernetesMode kubernetesMode = null,
                IDateTime dateTime = null,
                IAPILog log = null,
                IConfig config = null)
                : base(Guid.NewGuid(), kubernetesMode, agentService, jobService, scheduleRuleFactory, queueJobValidator, queryManager, dateTime, log, config)
            {
                // 'Enabled = true' triggered Execute() immediately. I needed to set the field only to enable getting job from the queue
                typeof(Agent.AgentBase).GetField("_enabled", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, true);

                MockHelper();
            }

            public TaskStatusEnum? JobResult { get; set; }

            public override string Name { get; } = "Test";

            public Mock<IAgentHelper> HelperMock => _helperMockMock;

            // Testing Evidence
            public IList<Job> ProcessedJobs { get; } = new List<Job>();

            public int GetAgentIDTest()
            {
                return GetAgentID();
            }

            protected override TaskResult ProcessJob(Job job)
            {
                ProcessedJobs.Add(job);

                return new TaskResult
                {
                    Status = JobResult ?? (job.JobFailed != null ? TaskStatusEnum.Fail : TaskStatusEnum.Success)
                };
            }

            protected override void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null)
            {
            }

            protected override IEnumerable<int> GetListOfResourceGroupIDs()
            {
                return Enumerable.Empty<int>();
            }

            private void MockHelper()
            {
                _helperMockMock = new Mock<IAgentHelper>();
                Mock<IDBContext> dbContextMock = new Mock<IDBContext>();
                DataTable result = new DataTable
                {
                    Columns = { new DataColumn() }
                };

                dbContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>())).Returns(result);
                _helperMockMock.Setup(x => x.GetDBContext(-1)).Returns(dbContextMock.Object);

                FileShareQueryResultSet fileShareQueryResultSet = new FileShareQueryResultSet
                {
                    Results = new List<Result<FileShareResourceServer>>
                    {
                        new Result<FileShareResourceServer>
                        {
                            Artifact = new FileShareResourceServer
                            {
                                UNCPath = Directory.GetCurrentDirectory()
                            }
                        }
                    }
                };

                Mock<IServicesMgr> servicesManagerMock = new Mock<IServicesMgr>();

                Mock<IFileShareServerManager> fileShareServerManagerMock = new Mock<IFileShareServerManager>();
                fileShareServerManagerMock.Setup(x => x.QueryAsync(It.IsAny<Query>())).ReturnsAsync(fileShareQueryResultSet);

                Mock<IPingService> pingServiceMock = new Mock<IPingService>();
                pingServiceMock.Setup(x => x.Ping()).ReturnsAsync("OK");

                servicesManagerMock.Setup(x => x.CreateProxy<IFileShareServerManager>(ExecutionIdentity.System))
                    .Returns(fileShareServerManagerMock.Object);

                servicesManagerMock.Setup(x => x.CreateProxy<IPingService>(ExecutionIdentity.System))
                    .Returns(pingServiceMock.Object);

                _helperMockMock.Setup(x => x.GetServicesManager()).Returns(servicesManagerMock.Object);

                typeof(Agent.AgentBase).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, _helperMockMock.Object);
            }
        }
    }
}
