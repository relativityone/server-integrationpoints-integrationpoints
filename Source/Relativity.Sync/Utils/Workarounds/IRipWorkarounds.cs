using System;
using System.Threading.Tasks;

namespace Relativity.Sync.Utils.Workarounds
{
    internal interface IRipWorkarounds
    {
        /// <summary>
        /// This is a workaround to update Has Errors and Last Runtime on Integration Point RDO.
        /// </summary>
        Task TryUpdateIntegrationPointAsync(int workspaceId, int jobHistoryId, bool? hasErrors, DateTime lastRuntime);
    }
}