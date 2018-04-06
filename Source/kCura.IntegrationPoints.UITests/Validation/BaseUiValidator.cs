using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
		private readonly int _jobExecutionTimeoutInMinutes;

		public BaseUiValidator()
		{
			_jobExecutionTimeoutInMinutes = 5;
		}

		public BaseUiValidator(int jobExecutionTimeoutInMinutes)
		{
			_jobExecutionTimeoutInMinutes = jobExecutionTimeoutInMinutes;
		}

		public void ValidateJobStatus(IntegrationPointDetailsPage integrationPointDetailsPage, Choice expectedJobStatus)
		{
			string actualJobStatusAfterExecuted = WaitUntilJobFinishedAndThenGetStatus(integrationPointDetailsPage, _jobExecutionTimeoutInMinutes);

			Assert.That(actualJobStatusAfterExecuted, Is.EqualTo(expectedJobStatus.Name));
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
			bool jobInProcessingOrPendingStatus = jobHistoryModel == null ||
										  jobHistoryModel.JobStatus == JobStatusChoices.JobHistoryProcessing.Name ||
										  jobHistoryModel.JobStatus == JobStatusChoices.JobHistoryPending.Name;

			bool timeoutExceeded = sw.Elapsed > TimeSpan.FromMinutes(timeoutInMinutes);

			return jobInProcessingOrPendingStatus && !timeoutExceeded;
		}
	}
}
