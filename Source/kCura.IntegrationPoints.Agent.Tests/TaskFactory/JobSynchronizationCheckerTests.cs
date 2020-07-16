using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.ScheduleQueue.AgentBase;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
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
			taskFactoryJobHistoryServiceFactory.CreateJobHistoryService(Arg.Any<Data.IntegrationPoint>())
				.Returns(_jobHistoryService);

			_sut = new JobSynchronizationChecker(helper, _jobService, managerFactory, taskFactoryJobHistoryServiceFactory);
		}

		[Test]
		public void ItShouldThrowExceptionWhenOtherTaskIsExecutingForSynchronizedTask()
		{
			// Arrange
			int jobId = 53243;
			Job job = new JobBuilder().WithJobId(jobId).Build();

			int integrationPointArtifactId = 434641;
			var ip = new Data.IntegrationPoint
			{
				ArtifactId = integrationPointArtifactId
			};

			_queueManager.HasJobsExecuting(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<DateTime>()).Returns(true);

			// Act & Assert
			Assert.Throws<AgentDropJobException>(() => _sut.CheckForSynchronization(job, ip, _agentBase));
		}

		[Test]
		public void ItShouldNotThrowExceptionWhenOtherTaskIsNotExecutingForSynchronizedTask()
		{
			// Arrange
			int jobId = 53243;
			Job job = new JobBuilder().WithJobId(jobId).Build();

			int integrationPointArtifactId = 434641;
			var ip = new Data.IntegrationPoint
			{
				ArtifactId = integrationPointArtifactId
			};

			_queueManager.HasJobsExecuting(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<DateTime>()).Returns(false);

			// Act & Assert
			_sut.CheckForSynchronization(job, ip, _agentBase);
		}

		[Test]
		public void ItShouldNotThrowExceptionWhenOtherTaskIsNotExecutinForSynchronizedTask()
		{
			// Arrange
			int jobId = 53243;
			Job job = new JobBuilder().WithJobId(jobId).Build();

			int integrationPointArtifactId = 434641;
			var ip = new Data.IntegrationPoint
			{
				ArtifactId = integrationPointArtifactId
			};

			_queueManager.HasJobsExecuting(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<DateTime>()).Returns(false);

			// Act & Assert
			_sut.CheckForSynchronization(job, ip, _agentBase);
		}

		[Test]
		public void ItShouldRescheduleScheduledJobInCaseOfDrop()
		{
			// Arrange
			int jobId = 53243;
			Job job = new JobBuilder().WithJobId(jobId).WithScheduleRuleType("scheduledJob").Build();

			int integrationPointArtifactId = 434641;
			var ip = new Data.IntegrationPoint
			{
				ArtifactId = integrationPointArtifactId
			};

			_queueManager.HasJobsExecuting(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<DateTime>()).Returns(true);

			// Act
			try
			{
				_sut.CheckForSynchronization(job, ip, _agentBase);
			}
			catch (AgentDropJobException) { }


			// Assert
			_jobService.Received().GetJobNextUtcRunDateTime(Arg.Is<Job>(x => x.JobId == jobId), Arg.Any<IScheduleRuleFactory>(),
				Arg.Any<TaskResult>());
		}

		[Test]
		public void ItShouldRemovedJobHistoryFromUnscheduledJobInCaseOfDrop()
		{
			// Arrange
			int jobId = 53243;
			Job job = new JobBuilder().WithJobId(jobId).Build();

			int integrationPointArtifactId = 434641;
			var ip = new Data.IntegrationPoint
			{
				ArtifactId = integrationPointArtifactId
			};

			_queueManager.HasJobsExecuting(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<long>(), Arg.Any<DateTime>()).Returns(true);

			// Act
			try
			{
				_sut.CheckForSynchronization(job, ip, _agentBase);
			}
			catch (AgentDropJobException) { }


			// Assert
			_jobHistoryService.Received().RemoveJobHistoryFromIntegrationPoint(Arg.Is<Job>(x => x.JobId == jobId));
		}

		private class AgentMock : ScheduleQueueAgentBase
		{
			public AgentMock(IScheduleRuleFactory scheduleRuleFactory = null) : base(Guid.Empty, null, null, scheduleRuleFactory)
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
