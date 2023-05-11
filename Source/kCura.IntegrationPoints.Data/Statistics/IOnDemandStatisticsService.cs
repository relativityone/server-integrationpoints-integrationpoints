using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Statistics
{
    public interface IOnDemandStatisticsService
    {
        Task<CalculationState> MarkAsCalculating(int workspaceId, int integrationPointId);

        Task<CalculationState> MarkCalculationAsFinished(int workspaceId, int integrationPointId, DocumentsStatistics statistics);

        CalculationState GetCalculationState(int workspaceId, int integrationPointId);
    }
}
