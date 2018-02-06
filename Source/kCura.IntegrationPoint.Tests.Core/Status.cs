using System;
using System.Threading;
using Castle.Windsor;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Status
	{
		public static void WaitForProcessToComplete(IRSAPIClient rsapiClient, Guid processId, int timeoutInSeconds = 300, int sleepIntervalInMilliseconds = 500)
		{
			double timeWaitedInSeconds = 0.0;
			ProcessInformation processInfo = rsapiClient.GetProcessState(rsapiClient.APIOptions, processId);
			while (processInfo.State != ProcessStateValue.Completed)
			{
				if (processInfo.State == ProcessStateValue.CompletedWithError || processInfo.State == ProcessStateValue.UnhandledException)
				{
					throw new Exception($"An error occurred while waiting on the Process {processId} to complete. Error: {processInfo.Message}.");
				}

				VerifyTimeout(timeWaitedInSeconds, timeoutInSeconds);
				timeWaitedInSeconds = SleepAndUpdateTimeout(sleepIntervalInMilliseconds, timeWaitedInSeconds);
				processInfo = rsapiClient.GetProcessState(rsapiClient.APIOptions, processId);
			}
		}

		public static void WaitForIntegrationPointJobToComplete(IWindsorContainer container, int workspaceArtifactId, int integrationPointArtifactId, int timeoutInSeconds = 300, int sleepIntervalInMilliseconds = 500)
		{
			IQueueRepository queueRepository = container.Resolve<IQueueRepository>();

			double timeWaitedInSeconds = 0.0;
			int numberOfJobsQueuedOrInProgress = queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);

			while (numberOfJobsQueuedOrInProgress > 0)
			{
				VerifyTimeout(timeWaitedInSeconds, timeoutInSeconds);
				timeWaitedInSeconds = SleepAndUpdateTimeout(sleepIntervalInMilliseconds, timeWaitedInSeconds);
				numberOfJobsQueuedOrInProgress = queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);
			}
		}

		public static void WaitForScheduledJobToComplete(IWindsorContainer container, int workspaceArtifactId, int integrationPointArtifactId, int timeoutInSeconds = 300, int sleepIntervalInMilliseconds = 500)
		{
			var rsapiService = container.Resolve<IRSAPIService>();

			double timeWaitedInSeconds = 0.0;
			IntegrationPoints.Data.IntegrationPoint integrationPoint = rsapiService.RelativityObjectManager.Read<IntegrationPoints.Data.IntegrationPoint>(integrationPointArtifactId);

			while (integrationPoint.LastRuntimeUTC == null)
			{
				VerifyTimeout(timeWaitedInSeconds, timeoutInSeconds);
				timeWaitedInSeconds = SleepAndUpdateTimeout(sleepIntervalInMilliseconds, timeWaitedInSeconds);
				integrationPoint = rsapiService.RelativityObjectManager.Read<IntegrationPoints.Data.IntegrationPoint>(integrationPointArtifactId);
			}
		}

		private static void VerifyTimeout(double timeWaitedInSeconds, int timeoutInSeconds)
		{
			if (timeWaitedInSeconds >= timeoutInSeconds)
			{
				throw new Exception($"Timed out waiting for operation to complete. Waited { timeWaitedInSeconds } seconds when timeout was { timeoutInSeconds }.");
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