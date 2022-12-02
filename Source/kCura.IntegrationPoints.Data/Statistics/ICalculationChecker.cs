using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Data.Statistics
{
    public interface ICalculationChecker
    {
        Task<CalculationState> MarkAsCalculating(int integrationPointId);

        Task<CalculationState> MarkCalculationAsFinished(int integrationPointId, DocumentsStatistics statistics);

        Task<CalculationState> GetCalculationState(int integrationPointId);

        Task<CalculationState> MarkCalculationAsCancelled(int integrationPointId);
    }
}
