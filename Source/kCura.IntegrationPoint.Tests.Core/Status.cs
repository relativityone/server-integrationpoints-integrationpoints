using System;
using System.Threading;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;
using Relativity.Logging;
using Serilog;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Status
	{
		public static void WaitForProcessToComplete(IRSAPIClient rsapiClient, Guid processId, int timeoutInSeconds = 300, int sleepIntervalInMilliseconds = 500, ILogger log = null)
		{
			double timeWaitedInSeconds = 0.0;
			ProcessInformation processInfo = rsapiClient.GetProcessState(rsapiClient.APIOptions, processId);
			while (processInfo.State != ProcessStateValue.CompletedWithError && processInfo.State != ProcessStateValue.Completed)
			{
				if (processInfo.State == ProcessStateValue.UnhandledException)
				{
					throw new Exception($"An error occurred while waiting on the Process {processId} to complete. Error status: {processInfo.Status}. Error message: {processInfo.Message}.");
				}

				VerifyTimeout(timeWaitedInSeconds, timeoutInSeconds, nameof(WaitForProcessToComplete));
				timeWaitedInSeconds = SleepAndUpdateTimeout(sleepIntervalInMilliseconds, timeWaitedInSeconds);
				processInfo = rsapiClient.GetProcessState(rsapiClient.APIOptions, processId);
			}

			if (processInfo.State == ProcessStateValue.CompletedWithError)
			{
				log?.Warning($"Process '{processInfo.Name}' completed with errors but still indicates success. Status: {processInfo.Status}. Message: {processInfo.Message}");
			}
		}

		public static void WaitForIntegrationPointJobToComplete(IWindsorContainer container, int workspaceArtifactId, int integrationPointArtifactId, int timeoutInSeconds = 300, int sleepIntervalInMilliseconds = 500)
		{
			IQueueRepository queueRepository = container.Resolve<IQueueRepository>();

			double timeWaitedInSeconds = 0.0;
			int numberOfJobsQueuedOrInProgress = queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);

			while (numberOfJobsQueuedOrInProgress > 0)
			{
				VerifyTimeout(timeWaitedInSeconds, timeoutInSeconds, nameof(WaitForIntegrationPointJobToComplete));
				timeWaitedInSeconds = SleepAndUpdateTimeout(sleepIntervalInMilliseconds, timeWaitedInSeconds);
				numberOfJobsQueuedOrInProgress = queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);
			}
		}

		public static void WaitForIntegrationPointToLeavePendingState(IWindsorContainer container, int workspaceArtifactId, int integrationPointArtifactId, int timeoutInSeconds = 300, int sleepIntervalInMilliseconds = 500)
		{
			IQueueRepository queueRepository = container.Resolve<IQueueRepository>();

			var timeWaitedInSeconds = 0.0;
			int numberOfPendingJobs = queueRepository.GetNumberOfPendingJobs(workspaceArtifactId, integrationPointArtifactId);

			while (numberOfPendingJobs > 0)
			{
				VerifyTimeout(timeWaitedInSeconds, timeoutInSeconds, nameof(WaitForIntegrationPointToLeavePendingState));
				timeWaitedInSeconds = SleepAndUpdateTimeout(sleepIntervalInMilliseconds, timeWaitedInSeconds);
				numberOfPendingJobs = queueRepository.GetNumberOfPendingJobs(workspaceArtifactId, integrationPointArtifactId);
			}
		}

		public static void WaitForScheduledJobToComplete(IWindsorContainer container, int workspaceArtifactId, int integrationPointArtifactId, int timeoutInSeconds = 300, int sleepIntervalInMilliseconds = 500)
		{
			var rsapiService = container.Resolve<IRSAPIService>();

			double timeWaitedInSeconds = 0.0;
			IntegrationPoints.Data.IntegrationPoint integrationPoint = rsapiService.RelativityObjectManager.Read<IntegrationPoints.Data.IntegrationPoint>(integrationPointArtifactId);

			while (integrationPoint.LastRuntimeUTC == null)
			{
				VerifyTimeout(timeWaitedInSeconds, timeoutInSeconds, nameof(WaitForScheduledJobToComplete));
				timeWaitedInSeconds = SleepAndUpdateTimeout(sleepIntervalInMilliseconds, timeWaitedInSeconds);
				integrationPoint = rsapiService.RelativityObjectManager.Read<IntegrationPoints.Data.IntegrationPoint>(integrationPointArtifactId);
			}
		}

		private static void VerifyTimeout(double timeWaitedInSeconds, int timeoutInSeconds, string operationName)
		{
			if (timeWaitedInSeconds >= timeoutInSeconds)
			{
				throw new Exception($"Timed out waiting for {operationName} to complete. Waited { timeWaitedInSeconds } seconds when timeout was { timeoutInSeconds }.");
			}
		}

		private static double SleepAndUpdateTimeout(int sleepInMilliseconds, double timeWaitedInSeconds)
		{
			Thread.Sleep(sleepInMilliseconds);
			timeWaitedInSeconds += (sleepInMilliseconds / 1000.0);
			return timeWaitedInSeconds;
		}
	}
}