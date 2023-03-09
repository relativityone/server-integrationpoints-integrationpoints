using System;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core
{
    public interface ITaskParametersBuilder
    {
        TaskParameters Build(TaskType taskType, Guid batchInstanceId, string sourceConfiguration, ImportSettings destinationConfiguration);
    }
}
