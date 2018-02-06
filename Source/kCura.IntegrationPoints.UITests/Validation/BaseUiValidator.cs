﻿using System;
using System.Diagnostics;
using kCura.IntegrationPoint.Tests.Core.Models;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.UITests.Pages;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;

namespace kCura.IntegrationPoints.UITests.Validation
{
	public class BaseUiValidator
	{
		public void ValidateJobStatus(IntegrationPointDetailsPage integrationPointDetailsPage, Choice expectedJobStatus)
		{
			const int jobExecutionTimeoutInMinutes = 5; //TODO Make it configurable per validator

			string actualJobStatusAfterExecuted = WaitUntilJobFinishedAndThenGetStatus(integrationPointDetailsPage, jobExecutionTimeoutInMinutes);

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