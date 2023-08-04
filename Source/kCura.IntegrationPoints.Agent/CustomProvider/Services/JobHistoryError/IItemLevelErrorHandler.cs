using System.Threading.Tasks;
using kCura.IntegrationPoints.Agent.CustomProvider.DTO;
using kCura.IntegrationPoints.Agent.CustomProvider.ImportStage;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.JobHistoryError
{
    internal interface IItemLevelErrorHandler
    {
        Task HandleItemErrorsAsync(ImportJobContext importJobContext, CustomProviderBatch batch);
    }
}
