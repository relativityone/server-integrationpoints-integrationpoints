using System;
using System.Threading;
using Castle.Windsor;
using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoint.Tests.Core
{
    public static class Status
    {
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
