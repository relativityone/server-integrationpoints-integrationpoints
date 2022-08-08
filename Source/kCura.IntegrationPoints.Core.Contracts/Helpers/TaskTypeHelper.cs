using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Contracts.Agent;

namespace kCura.IntegrationPoints.Core.Contracts.Helpers
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
