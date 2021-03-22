using System;
using FluentAssertions;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
	[IdentifiedTestFixture("DADEA8A3-8043-44B6-A8BF-A0B9BBC793D0")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class ScheduleQueueTestsForScheduledJobs : TestsBase
	{
		[IdentifiedTest("C6E9E6A2-BDD6-4767-97EE-55BE95323AE3")]
		public void Job_ShouldNotBePushedToTheQueueAfterRun_WhenScheduledNextRunExceedsEndDate()
		{
			// Arrange
			AgentTest agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			DateTime startDateTime = Context.CurrentDateTime;
			DateTime endDateTime = startDateTime;

			ScheduleRuleTest rule = ScheduleRuleTest.CreateDailyRule(startDateTime, endDateTime, TimeZoneInfo.Utc);
			PrepareJob(rule);

			FakeAgent sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			Database.JobsInQueue.Should().BeEmpty();
		}

		[IdentifiedTest("A74C60F2-6FC7-4884-9AB3-FFCB794E26BF")]
		public void Job_ShouldBePushedToTheQueueAfterRun_WhenIsScheduledWithDailyInterval()
		{
			// Arrange
			AgentTest agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			DateTime startDateTime = Context.CurrentDateTime;
			DateTime endDateTime = startDateTime.AddDays(2);

			DateTime expectedNextRunTime = startDateTime.AddDays(1);

			ScheduleRuleTest rule = ScheduleRuleTest.CreateDailyRule(startDateTime, endDateTime, TimeZoneInfo.Utc);
			JobTest job = PrepareJob(rule);

			FakeAgent sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			HelperManager.JobHelper.VerifyScheduledJobWasReScheduled(job, expectedNextRunTime);
		}

		[IdentifiedTest("8A840DD4-C9F6-4D83-8762-5F6A62D22074")]
		public void Job_ShouldBePushedToTheQueueAfterRun_WhenIsScheduledWithWeeklyInterval()
		{
			// Arrange
			AgentTest agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			DateTime startDateTime = Context.CurrentDateTime;
			DateTime endDateTime = startDateTime.AddMonths(1);

			DateTime expectedNextRunTime = GetNextWeekDay(startDateTime, DayOfWeek.Monday);

			ScheduleRuleTest rule = ScheduleRuleTest.CreateWeeklyRule(startDateTime, endDateTime, TimeZoneInfo.Utc, DaysOfWeek.Monday);
			JobTest job = PrepareJob(rule);

			FakeAgent sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			HelperManager.JobHelper.VerifyScheduledJobWasReScheduled(job, expectedNextRunTime);
		}

		[IdentifiedTest("639EF3D4-D655-4C2C-AD74-37E9A1300B0A")]
		public void Job_ShouldBePushedToTheQueueAfterRun_WhenIsScheduledWithMonthlyInterval()
		{
			// Arrange
			AgentTest agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			DateTime startDateTime = Context.CurrentDateTime;
			DateTime endDateTime = startDateTime.AddMonths(3);

			DateTime expectedNextRunTime = new DateTime(startDateTime.Year, startDateTime.Month + 1, 1);

			ScheduleRuleTest rule = ScheduleRuleTest.CreateMonthlyRule(startDateTime, endDateTime, 
				TimeZoneInfo.Utc, dayOfMonth: 1);
			JobTest job = PrepareJob(rule);

			FakeAgent sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			HelperManager.JobHelper.VerifyScheduledJobWasReScheduled(job, expectedNextRunTime);
		}

		private JobTest PrepareJob(ScheduleRuleTest rule)
		{
			return HelperManager.JobHelper.ScheduleJobWithScheduleRule(SourceWorkspace, rule);
		}

		private DateTime GetNextWeekDay(DateTime dateTime, DayOfWeek dayOfWeek)
		{
			int daysToAdd = ((int)dayOfWeek - (int)dateTime.DayOfWeek + 7) % 7;
			return dateTime.AddDays(daysToAdd);
		}

		private FakeAgent PrepareSutWithMockedQueryManager(AgentTest agent)
		{
			return new FakeAgent(agent,
				Container.Resolve<IAgentHelper>(),
				queryManager: Container.Resolve<IQueryManager>());
		}
	}
}
