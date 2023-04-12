using System.Threading.Tasks;
using Relativity.Services.Choice;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistory
{
    public interface IJobHistoryService
    {
        Task UpdateStatusAsync(int workspaceId, int jobHistoryId, ChoiceRef status);

        Task SetTotalItemsAsync(int workspaceId, int jobHistoryId, int totalItemsCount);

        Task UpdateProgressAsync(int workspaceId, int jobHistoryId, int readItemsCount, int transferredItemsCount);
    }
}
