﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;

namespace Rip.E2ETests.CustomProviders.Helpers
{
	internal static class JobHistoryTestHelper
	{
		private static readonly TimeSpan _delayBetweenChecks = TimeSpan.FromMilliseconds(500);

		public static Task<JobHistory> GetCompletedJobHistoryAsync(
			IRelativityObjectManager objectManager,
			int integrationPointID,
			TimeSpan timeout)
		{
			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.CancelAfter(timeout);

			return GetCompletedJobHistoryAsync(objectManager, integrationPointID, cancellationTokenSource.Token);
		}

		public static async Task<JobHistory> GetCompletedJobHistoryAsync(
			IRelativityObjectManager objectManager,
			int integrationPointID,
			CancellationToken cancellationToken)
		{
			string[] jobCompletedStatuses =
			{
				JobStatusChoices.JobHistoryCompleted.Name,
				JobStatusChoices.JobHistoryCompletedWithErrors.Name,
				JobStatusChoices.JobHistoryErrorJobFailed.Name,
				JobStatusChoices.JobHistoryStopped.Name,
				JobStatusChoices.JobHistoryValidationFailed.Name
			};
			while (true)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					throw new TaskCanceledException();
				}

				JobHistory jobHistory = await GetSingleJobHistoryForIntegrationPointAsync(objectManager, integrationPointID).ConfigureAwait(false);

				string jobStatusName = jobHistory?.JobStatus?.Name; // jobHistory or jobHistoryStatus might not exists yet
				if (jobCompletedStatuses.Contains(jobStatusName))
				{
					return jobHistory;
				}

				await Task
					.Delay(_delayBetweenChecks, cancellationToken)
					.ConfigureAwait(false);
			}
		}

		private static async Task<JobHistory> GetSingleJobHistoryForIntegrationPointAsync(
			IRelativityObjectManager objectManager,
			int integrationPointID)
		{
			var qr = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = ObjectTypeGuids.JobHistoryGuid
				},
				Condition = $"('{JobHistoryFields.IntegrationPoint}' INTERSECTS MULTIOBJECT [{integrationPointID}])"
			};
			List<JobHistory> jobHistoriesForIntegrationPoint = await objectManager
				.QueryAsync<JobHistory>(qr)
				.ConfigureAwait(false);

			return jobHistoriesForIntegrationPoint.Single();
		}
	}
}