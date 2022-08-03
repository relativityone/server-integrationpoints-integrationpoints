using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Interfaces
{
    internal interface IJobExecutor
    {
        event ExceptionEventHandler JobExecutionError;

        TaskResult ProcessJob(Job job);
    }
}
