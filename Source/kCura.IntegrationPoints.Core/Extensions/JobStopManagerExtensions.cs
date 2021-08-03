using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;

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
