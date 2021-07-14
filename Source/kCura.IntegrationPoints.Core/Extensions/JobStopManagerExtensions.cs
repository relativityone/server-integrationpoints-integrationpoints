using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;

namespace kCura.IntegrationPoints.Core.Extensions
{
    public static class JobStopManagerExtensions
    {
        public static void StopCheckingDrainStopAndUpdateStopState(this IJobStopManager stopManager, Job job, bool shouldDrainStop)
        {
            stopManager.StopCheckingDrainStop(shouldDrainStop);
            if (!shouldDrainStop)
            {
                job.StopState = StopState.None;
            }
        }
    }
}
