using System.Threading.Tasks;

namespace Relativity.IntegrationPoints.Services.Repositories
{
    public interface IDocumentRepository
    {
        Task<CurrentPromotionStatusModel> GetCurrentPromotionStatusAsync(CurrentPromotionStatusRequest request);
        Task<HistoricalPromotionStatusSummaryModel> GetHistoricalPromotionStatusAsync(HistoricalPromotionStatusRequest request);
        Task<PercentagePushedToReviewModel> GetPercentagePushedToReviewAsync(PercentagePushedToReviewRequest request);
    }
}