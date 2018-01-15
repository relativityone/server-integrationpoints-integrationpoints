﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.Attributes;
using kCura.IntegrationPoints.Agent.Exceptions;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
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
	[TestFixture]
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
			var contextProviderFactory = Substitute.For<IContextContainerFactory>();
			var managerFactory = Substitute.For<IManagerFactory>();
			_queueManager = Substitute.For<IQueueManager>();
			managerFactory.CreateQueueManager(Arg.Any<IContextContainer>()).Returns(_queueManager);

			var taskFactoryJobHistoryServiceFactory = Substitute.For<ITaskFactoryJobHistoryServiceFactory>();
			_jobHistoryService = Substitute.For<ITaskFactoryJobHistoryService>();
			taskFactoryJobHistoryServiceFactory.CreateJobHistoryService(Arg.Any<Data.IntegrationPoint>())
				.Returns(_jobHistoryService);

			_sut = new JobSynchronizationChecker(helper, contextProviderFactory, _jobService, managerFactory, taskFactoryJobHistoryServiceFactory);
		}

		[TestCaseSource(nameof(GetAllTasksWithSynchronizedAttribute))]
		public void ItShouldThrowExceptionWhenOtherTaskIsExecutingForSynchronizedTask(Type taskType)
		{
			// Arrange
			int jobId = 53243;
			var job = JobExtensions.CreateJob(jobId);

			int integrationPointArtifactId = 434641;
			var ip = new Data.IntegrationPoint
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
			var job = JobExtensions.CreateJob(jobId);

			int integrationPointArtifactId = 434641;
			var ip = new Data.IntegrationPoint
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
			var job = JobExtensions.CreateJob(jobId);

			int integrationPointArtifactId = 434641;
			var ip = new Data.IntegrationPoint
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
			var job = JobExtensions.CreateJob(jobId);

			int integrationPointArtifactId = 434641;
			var ip = new Data.IntegrationPoint
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
			var job = JobExtensions.CreateJob(jobId, "scheduledJob");

			int integrationPointArtifactId = 434641;
			var ip = new Data.IntegrationPoint
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
			var job = JobExtensions.CreateJob(jobId);

			int integrationPointArtifactId = 434641;
			var ip = new Data.IntegrationPoint
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
			public AgentMock(IScheduleRuleFactory scheduleRuleFactory = null) : base(Guid.Empty, null, null, scheduleRuleFactory)
			{ }

			public override string Name { get; }
			public override ITask GetTask(Job job)
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