using System;
using System.Diagnostics;
using System.Threading;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;

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

		private string WaitUntilJobFinishedAndThenGetStatus(IntegrationPointDetailsPage integrationPointDetailsPage, int jobExecutionTimeoutInMinutes)
		{
			JobHistoryModel jobHistoryModel = null;

			var sw = new Stopwatch();
			sw.Start();
			while (GetUntilJobFinishedConditionAndExecutionTimeout(jobHistoryModel, sw, jobExecutionTimeoutInMinutes))
			{
				jobHistoryModel = integrationPointDetailsPage.GetLatestJobHistoryFromJobStatusTable();
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
