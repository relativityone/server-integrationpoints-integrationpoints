using System;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.IntegrationPointRdoService
{
    public interface IIntegrationPointRdoService
    {
        Task TryUpdateLastRuntimeAsync(int workspaceId, int integrationPointId, DateTime lastRuntime);

        Task TryUpdateHasErrorsAsync(int workspaceId, int integrationPointId, bool hasErrors);
    }
}