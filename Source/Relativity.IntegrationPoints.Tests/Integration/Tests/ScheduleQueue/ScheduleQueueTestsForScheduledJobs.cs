﻿using System;
using FluentAssertions;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Data;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NUnit.Framework;
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
		private readonly DateTime _FIXED_DATE_TIME = new DateTime(2021, 10, 20);

		[IdentifiedTest("C6E9E6A2-BDD6-4767-97EE-55BE95323AE3")]
		public void Job_ShouldNotBePushedToTheQueueAfterRun_WhenScheduledNextRunExceedsEndDate()
		{
			// Arrange
			Context.SetDateTime(_FIXED_DATE_TIME);

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
			Context.SetDateTime(_FIXED_DATE_TIME);

			AgentTest agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			DateTime startDateTime = Context.CurrentDateTime;
			DateTime endDateTime = startDateTime.AddYears(1);

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
			Context.SetDateTime(_FIXED_DATE_TIME);

			AgentTest agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			DaysOfWeek dayOfWeek = ConvertToInternalDaysOfWeek(Context.CurrentDateTime.DayOfWeek);

			DateTime startDateTime = Context.CurrentDateTime;
			DateTime endDateTime = startDateTime.AddYears(1);

			DateTime expectedNextRunTime = startDateTime.AddDays(7);

			ScheduleRuleTest rule = ScheduleRuleTest.CreateWeeklyRule(startDateTime, endDateTime, TimeZoneInfo.Utc, dayOfWeek);
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
			Context.SetDateTime(_FIXED_DATE_TIME);

			AgentTest agent = HelperManager.AgentHelper.CreateIntegrationPointAgent();

			DateTime startDateTime = Context.CurrentDateTime;
			DateTime endDateTime = startDateTime.AddYears(1);

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

		private FakeAgent PrepareSutWithMockedQueryManager(AgentTest agent)
		{
			FakeAgent fakeAgent = new FakeAgent(agent,
				Container.Resolve<IAgentHelper>(),
				scheduleRuleFactory: Container.Resolve<IScheduleRuleFactory>(),
				queryManager: Container.Resolve<IQueryManager>());

			fakeAgent.ProcessJobMockFunc = (job) =>
			{
				DateTime timeAfterJobFinished = Context.CurrentDateTime.AddMinutes(10);
				Context.SetDateTime(timeAfterJobFinished);

				return new TaskResult { Status = TaskStatusEnum.Success };
			};

			return fakeAgent;
		}

		private JobTest PrepareJob(ScheduleRuleTest rule)
		{
			return HelperManager.JobHelper.ScheduleJobWithScheduleRule(SourceWorkspace, rule);
		}

		private DaysOfWeek ConvertToInternalDaysOfWeek(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
				case DayOfWeek.Monday:
					return DaysOfWeek.Monday;
				case DayOfWeek.Tuesday:
					return DaysOfWeek.Tuesday;
				case DayOfWeek.Wednesday:
					return DaysOfWeek.Wednesday;
				case DayOfWeek.Thursday:
					return DaysOfWeek.Thursday;
				case DayOfWeek.Friday:
					return DaysOfWeek.Friday;
				case DayOfWeek.Saturday:
					return DaysOfWeek.Saturday;
				case DayOfWeek.Sunday:
					return DaysOfWeek.Sunday;
				default:
					return DaysOfWeek.None;
			}
		}
	}
}
