﻿using System;
using kCura.IntegrationPoints.Agent.Interfaces;
using kCura.IntegrationPoints.Core.Tests;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.ScheduleQueue.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests
{
	[TestFixture]
	public class JobExecutorTests
	{
		private IAPILog _logger;
		private ITaskProvider _taskProvider;
		private IAgentNotifier _agentNotifier;
		private IJobExecutor _subjectUnderTest;
		private const string _RIP_PREFIX = "RIP.";


		[SetUp]
		public void SetUp()
		{
			_logger = Substitute.For<IAPILog>();
			_taskProvider = Substitute.For<ITaskProvider>();
			_agentNotifier = Substitute.For<IAgentNotifier>();

			_subjectUnderTest = new JobExecutor(_taskProvider, _agentNotifier, _logger);
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
			Job job = JobHelper.GetJob(jobId, rootJobId, null, 0, 0, workspaceId, 0, 0, DateTime.Now, null, null, 0, DateTime.Now, submittedBy, null, null);

			_subjectUnderTest.ProcessJob(job);
		}
	}
}