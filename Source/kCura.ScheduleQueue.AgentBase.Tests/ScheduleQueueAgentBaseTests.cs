using System;
using System.Collections.Generic;
using System.Threading;
using kCura.IntegrationPoints.Core.Logging;
using kCura.IntegrationPoints.Core.Tests;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.ScheduleQueue.AgentBase.Tests
{
	[TestFixture]
	public class ScheduleQueueAgentBaseTests
	{
		private IAgentService _agentService;
		private IAPILog _logger;
		private IJobService _jobService;
		private const int _MAXIMUM_TEST_EXECUTION_TIME_IN_MILISECONDS = 5000;
		private const string _RIP_PREFIX = "RIP.";

		private class ScheduleQueueAgentBaseMock : ScheduleQueueAgentBase
		{
			public ScheduleQueueAgentBaseMock(IAPILog logger, IAgentService agentService, IJobService jobService) :
				base(Guid.Empty, agentService, jobService)
			{
				Logger = logger;
			}

			public override string Name => string.Empty;
			public override ITask GetTask(Job job)
			{
				ITask task = Substitute.For<ITask>();
				return task;
			}

			protected override void Initialize()
			{ }

			protected override IEnumerable<int> GetListOfResourceGroupIDs()
			{
				yield break;
			}
		}

		[SetUp]
		public void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_agentService = Substitute.For<IAgentService>();
			_jobService = Substitute.For<IJobService>();
		}

		[Test]
		public void ItShouldPushJobIdToLogContext()
		{
			long jobId = 123;

			ExecuteJob(jobId, 0, 0, 0);

			string expectedJobId = jobId.ToString();
			_logger.Received().LogContextPushProperty($"{_RIP_PREFIX}{nameof(AgentCorrelationContext.JobId)}", expectedJobId);
		}

		[Test]
		public void ItShouldPushRootJobIdToLogContext()
		{
			long? rootJobId = 879;

			ExecuteJob(0, rootJobId, 0, 0);

			string expectedRootJobId = rootJobId.ToString();
			_logger.Received().LogContextPushProperty($"{_RIP_PREFIX}{nameof(AgentCorrelationContext.RootJobId)}", expectedRootJobId);
		}

		[Test]
		public void ItShouldPushEmptyRootJobIdToLogContextIfRootJobIdIsNull()
		{
			long? rootJobId = null;

			ExecuteJob(0, rootJobId, 0, 0);

			object expectedRootJobId = string.Empty;
			_logger.Received().LogContextPushProperty($"{_RIP_PREFIX}{nameof(AgentCorrelationContext.RootJobId)}", expectedRootJobId);
		}

		[Test]
		public void ItShouldPushUserIdToLogContext()
		{
			int userId = 9;

			ExecuteJob(0, 0, 0, userId);

			string expectedUserId = userId.ToString();
			_logger.Received().LogContextPushProperty($"{_RIP_PREFIX}{nameof(AgentCorrelationContext.UserId)}", expectedUserId);
		}

		[Test]
		public void ItShouldPushWorkspaceIdToLogContext()
		{
			int workspaceId = 9;

			ExecuteJob(0, 0, workspaceId, 0);

			string expectedWorkspaceId = workspaceId.ToString();
			_logger.Received().LogContextPushProperty($"{_RIP_PREFIX}{nameof(AgentCorrelationContext.WorkspaceId)}", expectedWorkspaceId);
		}

		private void ExecuteJob(long jobId, long? rootJobId, int workspaceId, int submittedBy)
		{
			AddJobToQueue(jobId, rootJobId, workspaceId, submittedBy);
			
			var sut = new ScheduleQueueAgentBaseMock(_logger, _agentService, _jobService);
			sut.SetInterval(2 * _MAXIMUM_TEST_EXECUTION_TIME_IN_MILISECONDS);

			var semaphore = new Semaphore(0, 1); // Execute is executed in separate thread, so we need synchronization here
			bool wasSemaphoreReleased = false;
			sut.OnAgentExecuteFinish += () =>
			{
				wasSemaphoreReleased = true;
				semaphore.Release(1);
			};

			sut.Enabled = true;
			semaphore.WaitOne(_MAXIMUM_TEST_EXECUTION_TIME_IN_MILISECONDS);
			if (!wasSemaphoreReleased)
			{
				Assert.Fail("Test execution timeout"); 
			}
		}

		private void AddJobToQueue(long jobId, long? rootJobId, int workspaceId, int submittedBy)
		{
			Job job = JobHelper.GetJob(jobId, rootJobId, null, 0, 0, workspaceId, 0, 0, DateTime.Now, null, null, 0, DateTime.Now, submittedBy, null, null);
			_jobService.GetNextQueueJob(Arg.Any<IEnumerable<int>>(), Arg.Any<int>()).Returns(x => job, x => null);
		}
	}
}
