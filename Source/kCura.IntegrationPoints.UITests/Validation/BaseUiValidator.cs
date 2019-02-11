using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using OpenQA.Selenium;


namespace kCura.IntegrationPoints.UITests.Validation
{
	public class BaseUiValidator
	{
		private const int _DEFAULT_JOB_EXECUTION_TIMEOUT_IN_MINUTES = 10;
		private readonly int _jobExecutionTimeoutInMinutes;

		public BaseUiValidator()
		{
			_jobExecutionTimeoutInMinutes = _DEFAULT_JOB_EXECUTION_TIMEOUT_IN_MINUTES;
		}

		public void ValidateJobStatus(IntegrationPointDetailsPage integrationPointDetailsPage, params Choice[] expectedJobStatuses)
		{
			string actualJobStatusAfterExecuted = WaitUntilJobFinishedAndThenGetStatus(integrationPointDetailsPage, _jobExecutionTimeoutInMinutes);
			actualJobStatusAfterExecuted.Should().BeOneOf(expectedJobStatuses.Select(js => js.Name));
		}

		protected static void ValidateHasErrorsProperty(Dictionary<string, string> generalPropertiesTable, bool expectHasErrors)
		{
			Assert.AreEqual(expectHasErrors.AsHtmlString(), generalPropertiesTable["Has Errors:"]);
		}

		private string WaitUntilJobFinishedAndThenGetStatus(IntegrationPointDetailsPage integrationPointDetailsPage, int jobExecutionTimeoutInMinutes)
		{
			JobHistoryModel jobHistoryModel = null;

			var sw = new Stopwatch();
			sw.Start();
			while (GetUntilJobFinishedConditionAndExecutionTimeout(jobHistoryModel, sw, jobExecutionTimeoutInMinutes))
			{
				try
				{
					// this is a workaround for REL-148925 (RIP Job Status Not Updating)
					// the bug is not deterministically reproducible
					// exception logging was added to help with investigating that and assuring if this is still an issue
					//integrationPointDetailsPage.Refresh();
					//integrationPointDetailsPage.WaitForPage();
					jobHistoryModel = integrationPointDetailsPage.GetLatestJobHistoryFromJobStatusTable();
				}
				catch (StaleElementReferenceException)
				{
					jobHistoryModel = null;
				}
				Thread.Sleep(1000);
			}

			return jobHistoryModel?.JobStatus;
		}

		private static bool GetUntilJobFinishedConditionAndExecutionTimeout(JobHistoryModel jobHistoryModel, Stopwatch sw, int timeoutInMinutes)
		{
			bool isIntermediateStatus = jobHistoryModel == null ||
										jobHistoryModel.JobStatus == JobStatusChoices.JobHistoryValidating.Name ||
										jobHistoryModel.JobStatus == JobStatusChoices.JobHistoryProcessing.Name ||
										jobHistoryModel.JobStatus == JobStatusChoices.JobHistoryPending.Name;

			bool timeoutExceeded = sw.Elapsed > TimeSpan.FromMinutes(timeoutInMinutes);

			return isIntermediateStatus && !timeoutExceeded;
		}
	}
}
