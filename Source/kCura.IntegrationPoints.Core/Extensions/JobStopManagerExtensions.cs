using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Managers;

namespace kCura.IntegrationPoints.Core.Extensions
{
    public static class JobStopManagerExtensions
    {
        public static void StopCheckingDrainStopAndUpdateStopState(this IJobStopManager stopManager, Job job, bool shouldDrainStop)
        {
            stopManager.StopCheckingDrainStop();
            if (!shouldDrainStop)
            {
                job.StopState = StopState.None;
                stopManager.CleanUpJobDrainStop();
            }
        }
    }
}
