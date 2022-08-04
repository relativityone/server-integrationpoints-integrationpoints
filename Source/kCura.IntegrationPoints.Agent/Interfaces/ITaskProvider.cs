using kCura.IntegrationPoints.Data;
using kCura.ScheduleQueue.Core;

namespace kCura.IntegrationPoints.Agent.Interfaces
{
    internal interface ITaskProvider
    {
        ITask GetTask(Job job);
        void ReleaseTask(ITask task);
    }
}
