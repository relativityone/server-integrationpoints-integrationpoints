using System;
using System.Threading;
using Castle.Windsor;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;

namespace kCura.IntegrationPoint.Tests.Core
{
	public static class Status
	{
		public static void WaitForProcessToComplete(IRSAPIClient rsapiClient, Guid processId, int timeout = 300, int interval = 500)
		{
			double timeWaitedInSeconds = 0.0;
			ProcessInformation processInfo = rsapiClient.GetProcessState(rsapiClient.APIOptions, processId);
			while (processInfo.State != ProcessStateValue.Completed)
			{
				if (timeWaitedInSeconds >= timeout)
				{
					throw new Exception($"Timed out waiting for Process: {processId} to complete");
				}

				if (processInfo.State == ProcessStateValue.CompletedWithError)
				{
					throw new Exception($"An error occurred while waiting on the Process {processId} to complete. Error: {processInfo.Message}.");
				}

				Thread.Sleep(interval);
				timeWaitedInSeconds += (interval / 1000.0);
				processInfo = rsapiClient.GetProcessState(rsapiClient.APIOptions, processId);
			}
		}

		public static void WaitForIntegrationPointJobToComplete(IWindsorContainer container, int workspaceArtifactId, int integrationPointArtifactId, int timeoutInSeconds = 300, int intervalInMilliseconds = 500)
		{
			IQueueRepository queueRepository = container.Resolve<IQueueRepository>();

			double timeWaitedInSeconds = 0.0;
			int numberOfJobsQueuedOrInProgress = queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);

			while (numberOfJobsQueuedOrInProgress > 0)
			{
				if (timeWaitedInSeconds >= timeoutInSeconds)
				{
					throw new Exception($"Timed out waiting for IntegrationPoint: { integrationPointArtifactId } to finish. Waited { timeWaitedInSeconds } seconds.");
				}

				Thread.Sleep(intervalInMilliseconds);
				timeWaitedInSeconds += (intervalInMilliseconds / 1000.0);
				numberOfJobsQueuedOrInProgress = queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);
			}
		}

		public static void WaitForScheduledJobToComplete(IWindsorContainer container, int workspaceArtifactId, int integrationPointArtifactId, string taskType, int timeoutInSeconds = 300, int intervalInMilliseconds = 500)
		{
			IRepositoryFactory repositoryFactory = container.Resolve<IRepositoryFactory>();
			IIntegrationPointRepository integrationPointRepository = repositoryFactory.GetIntegrationPointRepository(workspaceArtifactId);

			double timeWaitedInSeconds = 0.0;
			IntegrationPointDTO integrationPoint = integrationPointRepository.Read(integrationPointArtifactId);

			while (integrationPoint.LastRuntimeUTC == null)
			{
				if (timeWaitedInSeconds >= timeoutInSeconds)
				{
					throw new Exception($"Timed out waiting for Scheduled IntegrationPoint: { integrationPointArtifactId } to finish. Waited { timeWaitedInSeconds } seconds.");
				}

				Thread.Sleep(intervalInMilliseconds);
				timeWaitedInSeconds += (intervalInMilliseconds / 1000.0);
				integrationPoint = integrationPointRepository.Read(integrationPointArtifactId);
			}
		}
	}
}