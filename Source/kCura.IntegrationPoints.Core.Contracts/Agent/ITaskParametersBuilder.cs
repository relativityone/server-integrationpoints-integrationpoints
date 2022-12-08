using kCura.ScheduleQueue.Core.Core;
using System;

namespace kCura.IntegrationPoints.Core.Contracts.Agent
{
    public interface ITaskParametersBuilder
    {
        TaskParameters Build(TaskType taskType, Guid batchInstanceId, string sourceConfiguration, string destinationConfiguration);
    }
}
