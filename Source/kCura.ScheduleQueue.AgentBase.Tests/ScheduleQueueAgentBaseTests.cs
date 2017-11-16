using System;
using System.Collections.Generic;
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
		private IAPILog logger;
		private IAgentService agentService;
		private IJobService jobService;

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
			logger = Substitute.For<IAPILog>();
			agentService = Substitute.For<IAgentService>();
			jobService = Substitute.For<IJobService>();
		}

		[Test]
		public void ItShouldPushJobIdToLogContext()
		{
			long jobId = 123;

			ExecuteJob(jobId, 0, 0, 0);

			string expectedJobId = jobId.ToString();
			logger.Received().LogContextPushProperty(nameof(AgentCorrelationContext.JobId), expectedJobId);
		}

		[Test]
		public void ItShouldPushRootJobIdToLogContext()
		{
			long? rootJobId = 879;

			ExecuteJob(0, rootJobId, 0, 0);

			string expectedRootJobId = rootJobId.ToString();
			logger.Received().LogContextPushProperty(nameof(AgentCorrelationContext.RootJobId), expectedRootJobId);
		}

		[Test]
		public void ItShouldPushEmptyRootJobIdToLogContextIfRootJobIdIsNull()
		{
			long? rootJobId = null;

			ExecuteJob(0, rootJobId, 0, 0);

			object expectedRootJobId = string.Empty;
			logger.Received().LogContextPushProperty(nameof(AgentCorrelationContext.RootJobId), expectedRootJobId);
		}

		[Test]
		public void ItShouldPushUserIdToLogContext()
		{
			int userId = 9;

			ExecuteJob(0, 0, 0, userId);

			string expectedUserId = userId.ToString();
			logger.Received().LogContextPushProperty(nameof(AgentCorrelationContext.UserId), expectedUserId);
		}

		[Test]
		public void ItShouldPushWorkspaceIdToLogContext()
		{
			int workspaceId = 9;

			ExecuteJob(0, 0, workspaceId, 0);

			string expectedWorkspaceId = workspaceId.ToString();
			logger.Received().LogContextPushProperty(nameof(AgentCorrelationContext.WorkspaceId), expectedWorkspaceId);
		}

		private void ExecuteJob(long jobId, long? rootJobId, int workspaceId, int submittedBy)
		{
			AddJobToQueue(jobId, rootJobId, workspaceId, submittedBy);

			var sut = new ScheduleQueueAgentBaseMock(logger, agentService, jobService);
			sut.Enabled = true;
			sut.Execute();
		}

		private void AddJobToQueue(long jobId, long? rootJobId, int workspaceId, int submittedBy)
		{
			Job job = JobHelper.GetJob(jobId, rootJobId, null, 0, 0, workspaceId, 0, 0, DateTime.Now, null, null, 0, DateTime.Now, submittedBy, null, null);
			jobService.GetNextQueueJob(Arg.Any<IEnumerable<int>>(), Arg.Any<int>()).Returns(x => job);
		}
	}
}
