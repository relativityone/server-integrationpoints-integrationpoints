using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Attributes;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Interfaces;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Validation;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Type = System.Type;

namespace kCura.IntegrationPoints.Agent.Tests.TaskFactory
{
    [TestFixture, Category("Unit")]
    public class JobSynchronizationCheckerTests
    {
        private IJobSynchronizationChecker _sut;
        private ScheduleQueueAgentBase _agentBase;
        private ITaskFactoryJobHistoryService _jobHistoryService;
        private IQueueManager _queueManager;
        private IJobService _jobService;

        [SetUp]
        public void SetUp()
        {
            _agentBase = new AgentMock();

            _jobService = Substitute.For<IJobService>();
            var helper = Substitute.For<IAgentHelper>();
            var managerFactory = Substitute.For<IManagerFactory>();
            _queueManager = Substitute.For<IQueueManager>();
            managerFactory.CreateQueueManager().Returns(_queueManager);

            var taskFactoryJobHistoryServiceFactory = Substitute.For<ITaskFactoryJobHistoryServiceFactory>();
            _jobHistoryService = Substitute.For<ITaskFactoryJobHistoryService>();
            taskFactoryJobHistoryServiceFactory.CreateJobHistoryService(Arg.Any<IntegrationPointDto>())
                .Returns(_jobHistoryService);

            _sut = new JobSynchronizationChecker(helper, _jobService, managerFactory, taskFactoryJobHistoryServiceFactory);
        }

        [TestCaseSource(nameof(GetAllTasksWithSynchronizedAttribute))]
        public void ItShouldThrowExceptionWhenOtherTaskIsExecutingForSynchronizedTask(Type taskType)
        {
            // Arrange
            int jobId = 53243;
            Job job = new JobBuilder().WithJobId(jobId).Build();

            int integrationPointArtifactId = 434641;
            var ip = new IntegrationPointDto
            {
                ArtifactId = integrationPointArtifactId
            };

            _queueManager.HasJobsExecuting(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<DateTime>()).Returns(true);

            // Act & Assert
            Assert.Throws<AgentDropJobException>(() => _sut.CheckForSynchronization(taskType, job, ip, _agentBase));
        }

        [TestCaseSource(nameof(GetAllTasksWithSynchronizedAttribute))]
        public void ItShouldNotThrowExceptionWhenOtherTaskIsNotExecutingForSynchronizedTask(Type taskType)
        {
            // Arrange
            int jobId = 53243;
            Job job = new JobBuilder().WithJobId(jobId).Build();

            int integrationPointArtifactId = 434641;
            var ip = new IntegrationPointDto
            {
                ArtifactId = integrationPointArtifactId
            };

            _queueManager.HasJobsExecuting(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<DateTime>()).Returns(false);

            // Act & Assert
            _sut.CheckForSynchronization(taskType, job, ip, _agentBase);
        }

        [TestCaseSource(nameof(GetAllTasksWithoutSynchronizedAttribute))]
        public void ItShouldNotThrowExceptionWhenOtherTaskIsExecutinForSynchronizedTask(Type taskType)
        {
            // Arrange
            int jobId = 53243;
            Job job = new JobBuilder().WithJobId(jobId).Build();

            int integrationPointArtifactId = 434641;
            var ip = new IntegrationPointDto
            {
                ArtifactId = integrationPointArtifactId
            };

            _queueManager.HasJobsExecuting(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<DateTime>()).Returns(true);

            // Act & Assert
            _sut.CheckForSynchronization(taskType, job, ip, _agentBase);
        }

        [TestCaseSource(nameof(GetAllTasksWithoutSynchronizedAttribute))]
        public void ItShouldNotThrowExceptionWhenOtherTaskIsNotExecutinForSynchronizedTask(Type taskType)
        {
            // Arrange
            int jobId = 53243;
            Job job = new JobBuilder().WithJobId(jobId).Build();

            int integrationPointArtifactId = 434641;
            var ip = new IntegrationPointDto
            {
                ArtifactId = integrationPointArtifactId
            };

            _queueManager.HasJobsExecuting(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<DateTime>()).Returns(false);

            // Act & Assert
            _sut.CheckForSynchronization(taskType, job, ip, _agentBase);
        }

        [TestCaseSource(nameof(GetAllTasksWithSynchronizedAttribute))]
        public void ItShouldRescheduleScheduledJobInCaseOfDrop(Type taskType)
        {
            // Arrange
            int jobId = 53243;
            Job job = new JobBuilder().WithJobId(jobId).WithScheduleRuleType("scheduledJob").Build();

            int integrationPointArtifactId = 434641;
            var ip = new IntegrationPointDto
            {
                ArtifactId = integrationPointArtifactId
            };

            _queueManager.HasJobsExecuting(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<DateTime>()).Returns(true);

            // Act
            try
            {
                _sut.CheckForSynchronization(taskType, job, ip, _agentBase);
            }
            catch (AgentDropJobException) { }

            // Assert
            _jobService.Received().GetJobNextUtcRunDateTime(Arg.Is<Job>(x => x.JobId == jobId), Arg.Any<IScheduleRuleFactory>(),
                Arg.Any<TaskResult>());
        }

        [TestCaseSource(nameof(GetAllTasksWithSynchronizedAttribute))]
        public void ItShouldRemovedJobHistoryFromUnscheduledJobInCaseOfDrop(Type taskType)
        {
            // Arrange
            int jobId = 53243;
            Job job = new JobBuilder().WithJobId(jobId).Build();

            int integrationPointArtifactId = 434641;
            var ip = new IntegrationPointDto
            {
                ArtifactId = integrationPointArtifactId
            };

            _queueManager.HasJobsExecuting(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<DateTime>()).Returns(true);

            // Act
            try
            {
                _sut.CheckForSynchronization(taskType, job, ip, _agentBase);
            }
            catch (AgentDropJobException) { }

            // Assert
            _jobHistoryService.Received().RemoveJobHistoryFromIntegrationPoint(Arg.Is<Job>(x => x.JobId == jobId));
        }

        private static IEnumerable<TestCaseData> GetAllTasksWithSynchronizedAttribute()
        {
            return GetAllTaskImplementations()
                .Where(type => type.GetCustomAttributes(false).Any(attribute => attribute is SynchronizedTaskAttribute))
                .Select(type => new TestCaseData(type) { TestName = type.ToString() });
        }

        private static IEnumerable<TestCaseData> GetAllTasksWithoutSynchronizedAttribute()
        {
            return GetAllTaskImplementations()
                .Where(type => !type.GetCustomAttributes(false).Any(attribute => attribute is SynchronizedTaskAttribute))
                .Select(type => new TestCaseData(type) { TestName = type.ToString() });
        }

        private static IEnumerable<Type> GetAllTaskImplementations()
        {
            var taskInterface = typeof(ITask);
            var assemblyWithTasks = typeof(SyncManager).Assembly;
            return assemblyWithTasks.GetTypes()
                .Where(type => type.IsClass && taskInterface.IsAssignableFrom(type));
        }

        private class AgentMock : ScheduleQueueAgentBase
        {
            public AgentMock(IScheduleRuleFactory scheduleRuleFactory = null) : base(Guid.Empty, Substitute.For<IKubernetesMode>(), scheduleRuleFactory: scheduleRuleFactory)
            { }

            public override string Name { get; }

            protected override TaskResult ProcessJob(Job job)
            {
                throw new NotImplementedException();
            }

            protected override void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null)
            {
                throw new NotImplementedException();
            }
        }
    }
}
