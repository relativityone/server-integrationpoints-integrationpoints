﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using kCura.ScheduleQueue.Core.Validation;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.ServiceBus.Contracts.Interfaces;

namespace kCura.ScheduleQueue.AgentBase.Tests
{
	[TestFixture, Category("Unit")]
	public class ScheduleQueueAgentBaseTests
	{
		private Mock<IJobService> _jobServiceMock;
		private Mock<IQueueJobValidator> _queueJobValidatorFake;

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

			IList<Job> expectedProcessedJobs = new List<Job> {validJob1, validJob2};

			TestAgent sut = GetSut();

			SetupJobQueue(validJob1, invalidJob, validJob2);

			SetupJobAsInvalid(invalidJob);

			// Act
			sut.Execute();

			// Assert
			sut.ProcessedJobs.ShouldBeEquivalentTo(expectedProcessedJobs);

			_jobServiceMock.Verify(x => x.DeleteJob(invalidJob.JobId));
		}

		private TestAgent GetSut()
		{
			var agentService = new Mock<IAgentService>();
			var scheduleRuleFactory = new Mock<IScheduleRuleFactory>();
			var emptyLog = new Mock<IAPILog>();

			_jobServiceMock = new Mock<IJobService>();
			_jobServiceMock.Setup(x => x.FinalizeJob(It.IsAny<Job>(), It.IsAny<IScheduleRuleFactory>(),
					It.IsAny<TaskResult>()))
				.Returns(new FinalizeJobResult {JobState = JobLogState.Finished, Details = string.Empty});

			_queueJobValidatorFake = new Mock<IQueueJobValidator>();
			_queueJobValidatorFake.Setup(x => x.ValidateAsync(It.IsAny<Job>()))
				.ReturnsAsync(ValidationResult.Success);

			return new TestAgent(agentService.Object, _jobServiceMock.Object,
				scheduleRuleFactory.Object, _queueJobValidatorFake.Object, emptyLog.Object);
		}

		private void SetupJobQueue(params Job[] jobs)
		{
			var sequenceSetup = _jobServiceMock.SetupSequence(x => x.GetNextQueueJob(
				It.IsAny<IEnumerable<int>>(), It.IsAny<int>()));

			foreach (var job in jobs)
			{
				sequenceSetup = sequenceSetup.Returns(job);
			}
		}

		private void SetupJobAsInvalid(Job job)
		{
			_queueJobValidatorFake.Setup(x => x.ValidateAsync(job))
				.ReturnsAsync(ValidationResult.Failed(It.IsAny<string>()));
		}

		private class TestAgent : ScheduleQueueAgentBase
		{
			public TestAgent(IAgentService agentService = null, IJobService jobService = null, 
				IScheduleRuleFactory scheduleRuleFactory = null, IQueueJobValidator queueJobValidator = null, IAPILog log = null) 
				: base(Guid.NewGuid(), agentService, jobService, scheduleRuleFactory, queueJobValidator, log)
			{
				//'Enabled = true' triggered Execute() immediately. I needed to set the field only to enable getting job from the queue
				typeof(Agent.AgentBase).GetField("_enabled", BindingFlags.NonPublic | BindingFlags.Instance)
					.SetValue(this, true);
			}

			public override string Name { get; } = "Test";
			
			protected override TaskResult ProcessJob(Job job)
			{
				ProcessedJobs.Add(job);

				return new TaskResult {Status = TaskStatusEnum.Success, };
			}

			protected override void LogJobState(Job job, JobLogState state, Exception exception = null, string details = null)
			{
			}

			protected override IEnumerable<int> GetListOfResourceGroupIDs()
			{
				return Enumerable.Empty<int>();
			}

			//Testing Evidence
			public IList<Job> ProcessedJobs { get; } = new List<Job>();
		}

	}
}
