using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobProgress
{
    public interface IJobProgressHandler
    {
        Task BeginAsync(int workspaceId, CustomProviderJobDetails jobDetails);
    }
}