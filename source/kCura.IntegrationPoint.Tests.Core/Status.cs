using System;
using System.Threading;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using kCura.Data.RowDataGateway;
using Relativity.API;

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

		public static void WaitForIntegrationPointJobToComplete(IQueueRepository queueRepository,int workspaceArtifactId, int integrationPointArtifactId, int timeoutInSeconds = 300, int intervalInMilliseconds = 500)
		{
			string connectionString = String.Format(SharedVariables.WorkspaceConnectionStringFormat, workspaceArtifactId);
			Context baseContext = new Context(connectionString);
			DBContext context = new DBContext(baseContext);

			double timeWaitedInSeconds = 0.0;
			int numberOfJobsQueuedOrProgress = queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);

			while (numberOfJobsQueuedOrProgress > 0)
			{
				if (timeWaitedInSeconds >= timeoutInSeconds)
				{
					throw new Exception($"Timed out waiting for IntegrationPoint: { integrationPointArtifactId } to finish. Waited { timeoutInSeconds } seconds.");
				}

				Thread.Sleep(intervalInMilliseconds);
				timeWaitedInSeconds = (intervalInMilliseconds / 1000.0);
				numberOfJobsQueuedOrProgress = queueRepository.GetNumberOfJobsExecutingOrInQueue(workspaceArtifactId, integrationPointArtifactId);
			}
		}
	}
}