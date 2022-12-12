using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Statistics
{
    public interface IOnDemandStatisticsService
    {
        Task<CalculationState> MarkAsCalculating(int integrationPointId);

        Task<CalculationState> MarkCalculationAsFinished(int integrationPointId, DocumentsStatistics statistics);

        CalculationState GetCalculationState(int integrationPointId);
    }
}
