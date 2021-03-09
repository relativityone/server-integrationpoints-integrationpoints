using System;
using FluentAssertions;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Tests.Integration.Mocks;
using Relativity.IntegrationPoints.Tests.Integration.Models;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.ScheduleQueue
{
	[TestFixture]
	public class ScheduleQueueTestsForScheduledJobs : TestsBase
	{
		[Test]
		public void Job_ShouldNotBePushedToTheQueueAfterRun_WhenScheduledNextRunExceedsEndDate()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			DateTime startDateTime = Context.CurrentDateTime;
			DateTime endDateTime = startDateTime;

			ScheduleRuleTest rule = ScheduleRuleTest.CreateDailyRule(startDateTime, endDateTime, TimeZoneInfo.Utc);
			var job = HelperManager.JobHelper.ScheduleJobWithScheduleRule(rule);

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			Database.JobsInQueue.Should().BeEmpty();
		}

		[Test]
		public void Job_ShouldBePushedToTheQueueAfterRun_WhenIsScheduledWithDailyInterval()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			DateTime startDateTime = Context.CurrentDateTime;
			DateTime endDateTime = startDateTime.AddDays(2);

			DateTime expectedNextRunTime = startDateTime.AddDays(1);

			ScheduleRuleTest rule = ScheduleRuleTest.CreateDailyRule(startDateTime, endDateTime, TimeZoneInfo.Utc);
			var job = HelperManager.JobHelper.ScheduleJobWithScheduleRule(rule);

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			HelperManager.JobHelper.VerifyScheduledJobWasReScheduled(job, expectedNextRunTime);
		}

		[Test]
		public void Job_ShouldBePushedToTheQueueAfterRun_WhenIsScheduledWithWeeklyInterval()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			DateTime startDateTime = Context.CurrentDateTime;
			DateTime endDateTime = startDateTime.AddMonths(1);

			DateTime expectedNextRunTime = GetNextWeekDay(startDateTime, DayOfWeek.Monday);

			ScheduleRuleTest rule = ScheduleRuleTest.CreateWeeklyRule(startDateTime, endDateTime, TimeZoneInfo.Utc, DaysOfWeek.Monday);
			var job = HelperManager.JobHelper.ScheduleJobWithScheduleRule(rule);

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			HelperManager.JobHelper.VerifyScheduledJobWasReScheduled(job, expectedNextRunTime);
		}

		[Test]
		public void Job_ShouldBePushedToTheQueueAfterRun_WhenIsScheduledWithMonthlyInterval()
		{
			// Arrange
			var agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			DateTime startDateTime = Context.CurrentDateTime;
			DateTime endDateTime = startDateTime.AddMonths(3);

			DateTime expectedNextRunTime = new DateTime(startDateTime.Year, startDateTime.Month + 1, 1);

			ScheduleRuleTest rule = ScheduleRuleTest.CreateMonthlyRule(startDateTime, endDateTime, 
				TimeZoneInfo.Utc, dayOfMonth: 1);
			var job = HelperManager.JobHelper.ScheduleJobWithScheduleRule(rule);

			var sut = PrepareSutWithMockedQueryManager(agent);

			// Act
			sut.Execute();

			// Assert
			HelperManager.JobHelper.VerifyScheduledJobWasReScheduled(job, expectedNextRunTime);
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
