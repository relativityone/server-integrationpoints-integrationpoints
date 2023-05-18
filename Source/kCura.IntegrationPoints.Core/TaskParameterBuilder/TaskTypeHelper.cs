using System.Collections.Generic;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core
{
    public static class TaskTypeHelper
    {
        public static IEnumerable<TaskType> GetManagerTypes()
        {
            yield return TaskType.SyncManager;
            yield return TaskType.ExportManager;
            yield return TaskType.ExportService;
            yield return TaskType.ImportService;
        }
    }
}
